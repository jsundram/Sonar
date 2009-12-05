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
g_szFakeHHID = "HHID_DEBUG"
#g_lstFakeZGIDs = ["ZGID_1", "ZGID_2"]
g_dictCurrentlyPlayingTrackNums = {}
g_dictMyFakeTrackMD = {"Title" : "All My Sundays",
                       "Artist": "Your Mom",
                       "Album": "Greatest Hits",
                       "AlbumArtUri": "http://www.ibiblio.org/wm/paint/auth/hopper/street/hopper.early-sunday.jpg",
                       # Optional, and still mostly work-safe image:
                       # "AlbumArtUri": "http://promo.spunkyangels.com/amyredcowgirl/001.jpg",
                       "Uri": "http://www.yourmom.com/Sundays_I_Have_Loved.flac",
                       "PlayTime": TRACK_TIME}

#g_lstCurrentQs = [[g_dictMyFakeTrackMD],[]]
g_dictCurrentQs = {}
g_qEvents = Queue(10)
g_qSignals = Queue(1)
g_dictPlayState = {}
g_dictVolume = {}
g_dictMute = {}
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
    global g_dictCurrentQs
    try:
        return g_dictCurrentQs[szZGID]
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
g_dictTimes = {}

def Pause(szZGID):
    global g_dictPlayState, g_currentHHID
    try:
        if (g_dictPlayState[szZGID] == PS_PLAYING):
            g_aePlayState[szZGID] = PS_PAUSED
            if (sonos.pause(g_currentHHID, szZGID)):
                OnPlayStateChanged(szZGID, False)
    except:
        return -1

server.register_function(Pause)

def Play(szZGID, nIx):
    global g_dictCurrentlyPlayingTrackNums, g_dictTimes, g_dictCurrentQs, g_currentHHID
    global g_currentHHID
    try:
        if (nIx != -1):
            nTracks = len(g_dictCurrentQs[szZGID])
            if (0 <= nIx < nTracks):
                g_dictCurrentlyPlayingTrackNums[szZGID] = nIx
                g_dictTimes[szZGID] = 0
                sonos.seekTrack(g_currentHHID, szZGID, nIx)
                OnTrackChanged(szZGID)
            else:
                return False

        if (g_dictPlayState[szZGID] != PS_PLAYING):
            g_dictPlayState[szZGID] = PS_PLAYING
            if (sonos.play(g_currentHHID, szZGID)):
                OnPlayStateChanged(szZGID, True)
        return True

    except Exception, e:
        print e
        return False

server.register_function(Play)

def IsPlaying(szZGID):
    global g_dictPlayState, PS_PLAYING
    try:
        return (g_dictPlayState[szZGID] == PS_PLAYING)
    except:
        return False

server.register_function(IsPlaying)

def SetVolume(szZGID, nVol):
    global g_dictVolume
    try:
        if (0 <= nVol <= 100):
            if (g_dictVolume.get(szZGID, -1) != nVol):
                g_dictVolume[szZGID] = nVol
                OnVolumeChanged(szZGID, nVol)
            return True
    except:
        pass
    return False

server.register_function(SetVolume)

def SetMute(szZGID, bMute):
    global g_dictMute
    try:
        bCurrentMute = g_dictMute.get(szZGID)
        if (bCurrentMute == None or bCurrentMute != bMute):
            g_dictMute[szZGID] = bMute
            OnMuteChanged(szZGID, bMute)
        return True
    except:
        return False

server.register_function(SetMute)

def GetVolume(szZGID):
    global g_dictVolume
    try:
        return g_dictVolume.get(szZGID, 30)
    except:
        return -1
    
server.register_function(GetVolume)

def IsMuted(szZGID):
    global g_dictMute
    try:
        return g_dictMute.get(szZGID, False)
    except:
        return False

server.register_function(IsMuted)

def nextTrack(zgId, bWrap):
    global g_currentHHID, g_dictCurrentQs, g_dictTimes, g_dictCurrentlyPlayingTrackNums
    nTracks = len(g_dictCurrentQs[zgId])
    if (bWrap or g_dictCurrentlyPlayingTrackNums[zgId] < nTracks):
        g_dictCurrentlyPlayingTrackNums[zgId] += 1
        g_dictCurrentlyPlayingTrackNums[zgId] %= nTracks
        g_dictTimes[zgId] = 0
        sonos.skipNext(g_currentHHID, zgId)
        OnTrackChanged(zgId)
        return True
    return False

def prevTrack(zgId, bWrap):
    global g_currentHHID, g_dictCurrentQs, g_dictTimes, g_dictCurrentlyPlayingTrackNums
    nTracks = len(g_dictCurrentQs[zgId])
    if (bWrap or g_dictCurrentlyPlayingTrackNums[zgId] > 0):
        g_dictCurrentlyPlayingTrackNums[zgId] -= 1
        if (g_dictCurrentlyPlayingTrackNums[zgId] < 0):
            g_dictCurrentlyPlayingTrackNums[zgId] += nTracks
        g_dictTimes[zgId] = 0
        sonos.skipBack(g_currentHHID, zgId)
        OnTrackChanged(zgId)
        return True
    return False

def Next(szZGID):
    return nextTrack(szZGID, False)

server.register_function(Next)

def Back(szZGID):
    return prevTrack(szZGID,False)

server.register_function(Back)

def Eventer():
    global g_qSignals, g_dictTimes, g_dictCurrentQs, g_aePlayState
    while (1):
        sleep(1)
        for zgId in sonos.getZgIdsForHHID(g_currentHHID):
            nTracks = len(g_dictCurrentQs[zgId])
            if (nTracks == 0 or g_dictPlayState[zgId] != PS_PLAYING):
                continue
            q = g_dictCurrentQs[zgId]
            if (g_dictTimes[zgId] > q[g_dictCurrentlyPlayingTrackNums[zgId]]["PlayTime"]):
                g_dictTimes[zgId] += 1
                OnTick(zgId, g_dictTimes[zgId])
        try:
            if (g_qSignals.get_nowait()):
                return
        except Empty:
            pass
    

def GetCurrentTrackTime(szZGID):
    global g_dictTimes
    try:
        return g_dictTimes[szZGID]
    except:
        return -1

server.register_function(GetCurrentTrackTime)

def GetCurrentTrackMD(szZGID):
    global g_dictCurrentQs, g_dictCurrentlyPlayingTrackNums
    try:
        return g_dictCurrentQs[szZGID][g_dictCurrentlyPlayingTrackNums[szZGID]]
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
				"http://192.168.2.222:1400/xml/zone_player.xml",
				"Linux UPnP/1.0 Sonos/12.3-22270", 1800)


# To kill:
# g_qSignals.put(True)
# eventLoop.join()
