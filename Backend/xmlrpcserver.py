from SimpleXMLRPCServer import SimpleXMLRPCServer
from SimpleXMLRPCServer import SimpleXMLRPCRequestHandler

import sonos

# Restrict to a particular path.
class RequestHandler(SimpleXMLRPCRequestHandler):
    rpc_paths = ('/RPC2',)

# Create server
server = SimpleXMLRPCServer(("localhost", 8000),
                            requestHandler=RequestHandler)
server.register_introspection_functions()

# Defines
TRACK_TIME = 1234

from Queue import Queue, Empty, Full

PS_PLAYING, PS_PAUSED, PS_STOPPED = range(3)

# Initialize Global Variables
g_bSubscribed = False
g_szFakeHHID = "HHID_12345ABCDE"
g_lstFakeZGIDs = ["ZGID_1", "ZGID_2"]
g_nCurrentlyPlayingTrackNums = [0,0]
g_dictMyFakeTrackMD = {"Title" : "All My Sundays",
                       "Artist": "Your Mom",
                       "Album": "Greatest Hits",
                       "AlbumArtUri": "http://www.ibiblio.org/wm/paint/auth/hopper/street/hopper.early-sunday.jpg",
                       # Optional, and still mostly work-safe image:
                       # "AlbumArtUri": "http://promo.spunkyangels.com/amyredcowgirl/001.jpg",
                       "Uri": "http://www.yourmom.com/Sundays_I_Have_Loved.flac",
                       "PlayTime": TRACK_TIME}

g_lstCurrentQs = [[g_dictMyFakeTrackMD],[]]
g_qEvents = Queue(10)
g_qSignals = Queue(1)
g_aePlayState = [PS_STOPPED, PS_STOPPED]
g_anVolume = [30,30]
g_abMute = [False, False]
g_currentHHID = None
g_lstCurrentHHIDs = []

def PostEvent(szName, lstArgs):
    global g_qEvents
    try:
        g_qEvents.put_nowait({"Name": szName, "Args": lstArgs})
    except Full:
        pass

def OnZoneGroupsChanged():
    PostEvent("OnZoneGroupsChanged", [])

def zgtCallback(szHHID):
    global g_currentHHID
    if (szHHID == g_currentHHID):
        OnZoneGroupsChanged()

def OnTick(szZGID, nSeconds):
    PostEvent("OnTick", [szZGID, nSeconds])

def OnQueueChanged(szZGID):
    PostEvent("OnQueueChanged", [szZGID])

def OnTrackChanged(szZGID):
    PostEvent("OnTrackChanged", [szZGID])

def OnVolumeChanged(szZGID, nVol):
    PostEvent("OnVolumeChanged", [szZGID, nVol])

def OnMuteChanged(szZGID, bMute):
    PostEvent("OnMuteChanged", [szZGID, bMute])

def OnPlayStateChanged(szZGID, bPlaying):
    PostEvent("OnPlayStateChanged", [szZGID, bPlaying])    

def EnqueueTrack(zgId, dictMD):
    global g_dictMDCache, g_currentHHID
    try:
        if (sonos.enqueueTrack(g_currentHHID, zgId, dictMD)):
            print "Enqueued track at:", dictMD["Uri"], "with MD:", dictMD
            # Review: don't send this from here, wait for the event to bubble up from
            # the zp when that is plumbed
            OnQueueChanged(zgId)
            return True
    except Exception, e:
        print e

    return False

# Register EnqueueTrack() function; this will use the value of
# EnqueueTrack.__name__ as the name, which is just 'EnqueueTrack'.
server.register_function(EnqueueTrack)

def SearchForHHIDs():
    global g_lstCurrentHHIDs
    g_lstCurrentHHIDs = sonos.getHHIDs()
    return g_lstCurrentHHIDs

server.register_function(SearchForHHIDs)

def SubscribeToHH(szHHID):
    global g_lstCurrentHHIDs, g_currentHHID
    
    if (g_currentHHID == szHHID):
        return True
    if (szHHID in g_lstCurrentHHIDs):
        if (sonos.subToHHID(szHHID, zgtCallback)):
            g_currentHHID = szHHID
            return True
    return False

server.register_function(SubscribeToHH)

def GetAllZoneGroups():
    global g_currentHHID
    if(g_currentHHID):
        return sonos.getZgIdsForHHID(g_currentHHID)
    return []

server.register_function(GetAllZoneGroups)

def GetQueue(szZGID):
    global g_lstCurrentQs
    try:
        return g_lstCurrentQs[g_lstFakeZGIDs.index(szZGID)]
    except:
        return []

server.register_function(GetQueue)

def PollForEvents(nTimeout):
    ret = []
    try:
        ret.append(g_qEvents.get(True, nTimeout))
        while (1):
            # Once we have one, drain the queue in case there were
            ret.append(g_qEvents.get_nowait())
    except Empty:
        pass
    return ret

server.register_function(PollForEvents)

from time import sleep
g_anTimes = [0,0]

def Pause(szZGID):
    global g_lstFakeZGIDs, g_aePlayState
    try:
        ix = g_lstFakeZGIDs.index(szZGID)
        if (g_aePlayState[ix] == PS_PLAYING):
            g_aePlayState[ix] = PS_PAUSED
            OnPlayStateChanged(szZGID, False)
    except:
        return -1

server.register_function(Pause)

def Play(szZGID, nIx):
    global g_lstFakeZGIDs, g_aePlayState, g_lstCurrentQs, g_nCurrentlyPlayingTrackNums
    try:
        if (nIx != -1):
            return False
            nTracks = len(g_lstCurrentQs[ix])
            if (0 <= nIx < nTracks):
                g_nCurrentlyPlayingTrackNums[ix] = nIx
                g_anTimes[ix] = 0
                OnTrackChanged(szZGID)
            else:
                return False

        if (g_aePlayState[0] != PS_PLAYING):
            g_aePlayState[0] = PS_PLAYING
            sonos.play(g_currentHHID, szZGID)
            OnPlayStateChanged(szZGID, True)
        return True

    except Exception, e:
        print e
        return False

server.register_function(Play)

def IsPlaying(szZGID):
    global g_lstFakeZGIDs, g_aePlayState
    try:
        ix = g_lstFakeZGIDs.index(szZGID)
        return (g_aePlayState[ix] == PS_PLAYING)
    except:
        return False

server.register_function(IsPlaying)

def SetVolume(szZGID, nVol):
    global g_lstFakeZGIDs, g_anVolume, g_abMute
    try:
        ix = g_lstFakeZGIDs.index(szZGID)
        if (0 <= nVol <= 100):
            if (g_anVolume[ix] != nVol):
                g_anVolume[ix] = nVol
                OnVolumeChanged(szZGID, nVol)
            return True
    except:
        pass
    return False

server.register_function(SetVolume)

def SetMute(szZGID, bMute):
    global g_lstFakeZGIDs, g_abMute
    try:
        ix = g_lstFakeZGIDs.index(szZGID)
        if (g_abMute[ix] != bMute):
            g_abMute[ix] = bMute
            OnMuteChanged(szZGID, bMute)
        return True
    except:
        return False

server.register_function(SetMute)

def GetVolume(szZGID):
    global g_lstFakeZGIDs, g_anVolume
    try:
        ix = g_lstFakeZGIDs.index(szZGID)
        return g_anVolume[ix]
    except:
        return -1
    
server.register_function(GetVolume)

def IsMuted(szZGID):
    global g_lstFakeZGIDs, g_abMute
    try:
        ix = g_lstFakeZGIDs.index(szZGID)
        return g_abMute[ix]
    except:
        return False

server.register_function(IsMuted)

def nextTrack(n, bWrap):
    global g_nCurrentlyPlayingTrackNums, g_lstFakeZGIDs, g_lstCurrentQs
    nTracks = len(g_lstCurrentQs[n])
    if (bWrap or g_nCurrentlyPlayingTrackNums[n] < nTracks):
        g_nCurrentlyPlayingTrackNums[n] += 1
        g_nCurrentlyPlayingTrackNums[n] %= nTracks
        g_anTimes[n] = 0
        OnTrackChanged(g_lstFakeZGIDs[n])
        return True
    return False

def prevTrack(n, bWrap):
    global g_nCurrentlyPlayingTrackNums, g_lstFakeZGIDs, g_lstCurrentQs
    nTracks = len(g_lstCurrentQs[n])
    if (bWrap or g_nCurrentlyPlayingTrackNums[n] > 0):
        g_nCurrentlyPlayingTrackNums[n] -= 1
        if (g_nCurrentlyPlayingTrackNums[n] < 0):
            g_nCurrentlyPlayingTrackNums[n] += nTracks
        g_anTimes[n] = 0
        OnTrackChanged(g_lstFakeZGIDs[n])
        return True
    return False

def Next(szZGID):
    global g_lstFakeZGIDs
    try:
        ix = g_lstFakeZGIDs.index(szZGID)
        return nextTrack(ix,False)
    except:
        return False

server.register_function(Next)

def Back(szZGID):
    global g_lstFakeZGIDs
    try:
        ix = g_lstFakeZGIDs.index(szZGID)
        return prevTrack(ix,False)
    except:
        return False

server.register_function(Back)

def Eventer():
    global g_qSignals, g_anTimes, g_lstFakeZGIDs, g_lstCurrentQs, g_aePlayState
    while (1):
        sleep(1)
        for i in range(0,2):
            nTracks = len(g_lstCurrentQs[i])
            if (nTracks == 0 or g_aePlayState[i] != PS_PLAYING):
                continue
            g_anTimes[i] += 1
            if(g_anTimes[i] > TRACK_TIME):
                nextTrack(i, True)
            OnTick(g_lstFakeZGIDs[i], g_anTimes[i])
        try:
            if (g_qSignals.get_nowait()):
                return
        except Empty:
            pass
    

def GetCurrentTrackTime(szZGID):
    global g_lstFakeZGIDs, g_anTimes
    try:
        ix = g_lstFakeZGIDs.index(szZGID)
        return g_anTimes[ix]
    except:
        return -1

server.register_function(GetCurrentTrackTime)

def GetCurrentTrackMD(szZGID):
    global g_lstFakeZGIDs, g_lstCurrentQs, g_nCurrentlyPlayingTrackNums
    try:
        ix = g_lstFakeZGIDs.index(szZGID)
        return g_lstCurrentQs[ix][g_nCurrentlyPlayingTrackNums[ix]]
    except:
        return {}
    
server.register_function(GetCurrentTrackMD)

from threading import Thread

eventLoop = Thread(target=Eventer, name="EventLoop")
eventLoop.start()
# Run the server's main loop
serverLoop = Thread(target=server.serve_forever, name="serverLoop")
serverLoop.start()

# Until we correctly discover devices, we can use this to kick-start the
# process.
sonos.cp._ssdp_server._register('uuid:RINCON_000E5850027001400',
				'upnp:rootdevice',
				"http://10.0.0.176:1400/xml/zone_player.xml",
				"Linux UPnP/1.0 Sonos/12.3-22270", 1800)


# To kill:
# g_qSignals.put(True)
# eventLoop.join()
