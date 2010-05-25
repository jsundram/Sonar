from SimpleXMLRPCServer import SimpleXMLRPCServer
from SimpleXMLRPCServer import SimpleXMLRPCRequestHandler

import sonos

# Restrict to a particular path.
class RequestHandler(SimpleXMLRPCRequestHandler):
    rpc_paths = ('/RPC2',)

# Create server
server = SimpleXMLRPCServer(("localhost", 8000),requestHandler=RequestHandler)

server.register_introspection_functions()

# Defines
TRACK_TIME = 1234

from Queue import Queue, Empty, Full

PS_PLAYING, PS_PAUSED, PS_STOPPED, PS_INVALID = range(4)
ps_map = ["PLAYING", "PAUSED_PLAYBACK", "STOPPED"]

def psForString(ps_str):
    global ps_map
    try:
        return ps_map.index(ps_str)
    except:
        return PS_INVALID


class ZoneGroup:
    def __init__(self):
        self.currTrack = -1
        self.currQ = []
        self.playState = PS_INVALID
        self.vol = -1
        self.mute = None
        self.currPos = 0


# Initialize Global Variables
g_bSubscribed = False
g_szFakeHHID = "HHID_DEBUG"
g_dictMyFakeTrackMD = {"Title" : "All My Sundays",
                       "Artist": "Your Mom",
                       "Album": "Greatest Hits",
                       "AlbumArtUri": "http://www.ibiblio.org/wm/paint/auth/hopper/street/hopper.early-sunday.jpg",
                       # Optional, and still mostly work-safe image:
                       # "AlbumArtUri": "http://promo.spunkyangels.com/amyredcowgirl/001.jpg",
                       "Uri": "http://www.yourmom.com/Sundays_I_Have_Loved.flac",
                       "PlayTime": TRACK_TIME}

#g_lstCurrentQs = [[g_dictMyFakeTrackMD],[]]
g_dictZgs = {}
g_qEvents = Queue(10)
g_qSignals = Queue(1)
g_currentHHID = None
g_lstCurrentHHIDs = []

def PostEvent(szName, lstArgs):
    global g_qEvents
    try:
        print "Posting event: %s" % szName
        g_qEvents.put_nowait({"Name": szName, "Args": lstArgs})
    except Exception, Full:
        print "Something fucked up happened in PostEvent, %s: %s" % (szName, Full)

def OnZoneGroupsChanged():
    PostEvent("OnZoneGroupsChanged", [])

def popQueues():
    global g_currentHHID, g_dictZgs
    currZgIds = sonos.getZgIdsForHHID(g_currentHHID)
    zgsToRemove = frozenset(g_dictZgs.keys()).difference(currZgIds)
    for zgId in zgsToRemove:
        del g_dictZgs[zgId]

    for zgId in currZgIds:
        updateNPVars(zgId)

def updateNPVars(zgId):
    global g_currentHHID, g_dictZgs
    zg = g_dictZgs.setdefault(zgId, ZoneGroup())
    newQ = sonos.getQueue(g_currentHHID, zgId)
    if (zg.currQ != newQ):
        zg.currQ = newQ
        OnQueueChanged(zgId)
    (num, currTime, dur) = sonos.getTrackInfoForZg(g_currentHHID, zgId)
    if (zg.currTrack != num):
        zg.currTrack = num
        OnTrackChanged(zgId)
    # The duration is sometimes unknown until late in the process
    zg.currQ[num]['PlayTime'] = dur
    if (zg.currPos != currTime):
        zg.currPos = currTime
        OnTick(zgId, currTime)
        
    ps = psForString(sonos.getPlayStateForZg(g_currentHHID, zgId))
    if (zg.playState != ps):
        zg.playState = ps
        OnPlayStateChanged(zgId, ps == PS_PLAYING)

def zgtCallback(szHHID):
    global g_currentHHID
    if (szHHID == g_currentHHID):
        # Refresh the Qs
        popQueues()
        OnZoneGroupsChanged()

def avtCallback(hhId, zgId):
    global g_currentHHID

    if (hhId == g_currentHHID):
        updateNPVars(zgId)

# For now, the handling is the same as it is in the zgAvtCallback
def cdQCallback(hhId, zgId):
    zgAvtCallback(hhId, zgId)

def cdAICallback(hhId, zpId):
    pass

def updateLineIn(zpId):
    pass

def updateAllLineIn():
    for dev in sonos.getDevicesForHHID(g_currentHHID):
        pass

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
    global g_currentHHID
    try:
        print "Trying to enqueue track at:", dictMD["Uri"], "with MD:", dictMD
        if (sonos.enqueueTrack(g_currentHHID, zgId, dictMD)):
            print "Success"
            # Review: don't send this from here, wait for the event to bubble up from
            # the zp when that is plumbed
            OnQueueChanged(zgId)
            return True
    except Exception, e:
        print "Exception:", e

    print "Failure"
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
        if (sonos.subToHHID(szHHID, zgtCallback, avtCallback)):
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
    global g_dictZgs
    try:
        return g_dictZgs[szZGID].currQ
    except Exception:
        return []

server.register_function(GetQueue)
g_dictLineIns = {}
def GetLineIns():
    global g_currentHHID, g_dictLineIns
    try:
        return sorted(g_dictLineIns)
    except:
        return []

server.register_function(GetLineIns)

def PollForEvents(nTimeout):
    ret = []
    try:
        ret.append(g_qEvents.get(True, nTimeout))
        while (1):
            # Once we have one, drain the queue in case there were
            ret.append(g_qEvents.get_nowait())
    except Exception:
        pass
    return ret

server.register_function(PollForEvents)

from time import sleep

def Pause(szZGID):
    global g_dictZgs, g_currentHHID
    try:
        if (g_dictZgs[szZGID].playState == PS_PLAYING):
            g_dictZgs[szZGID].playState = PS_PAUSED
            if (sonos.pause(g_currentHHID, szZGID)):
                OnPlayStateChanged(szZGID, False)
        return True
    except Exception, e:
        print e
        return False

server.register_function(Pause)

def Play(szZGID, nIx):
    global g_dictZgs, g_currentHHID
    try:
        zg = g_dictZgs[szZGID]
        if (nIx != -1):
            nTracks = len(zg.currQ)
            if (0 <= nIx < nTracks):
                zg.currTrack = nIx
                zg.currPos = 0
                sonos.seekTrack(g_currentHHID, szZGID, nIx)
                OnTrackChanged(szZGID)
            else:
                return False

        if (zg.playState != PS_PLAYING):
            zg.playState = PS_PLAYING
            if (sonos.play(g_currentHHID, szZGID)):
                OnPlayStateChanged(szZGID, True)
        return True

    except Exception, e:
        print e
        return False

server.register_function(Play)

def IsPlaying(szZGID):
    global g_dictZgs, PS_PLAYING
    try:
        return (g_dictZgs[szZGID].playState == PS_PLAYING)
    except:
        return False

server.register_function(IsPlaying)

def SetVolume(szZGID, nVol):
    global g_dictZgs
    try:
        if (0 <= nVol <= 100):
            if (g_dictZgs.vol != nVol):
                g_dictZgs.vol = nVol
                OnVolumeChanged(szZGID, nVol)
            return True
    except Exception:
        pass
    return False

server.register_function(SetVolume)

def SetMute(szZGID, bMute):
    global g_dictZgs
    try:
        bCurrentMute = g_dictZgs[szZGID].mute
        if (bCurrentMute == None or bCurrentMute != bMute):
            g_dictZgs[szZGID].mute = bMute
            OnMuteChanged(szZGID, bMute)
        return True
    except Exception:
        return False

server.register_function(SetMute)

def GetVolume(szZGID):
    global g_dictZgs
    try:
        vol = g_dictZgs[szZGID].vol
        return (vol if vol >= 0 else 30)
    except:
        return -1

server.register_function(GetVolume)

def IsMuted(szZGID):
    global g_dictZgs
    try:
        return (True if g_dictZgs[szZGID].mute else False)
    except:
        return False

server.register_function(IsMuted)

def nextTrack(zgId, bWrap):
    global g_currentHHID, g_dictZgs
    zg = g_dictZgs[zgId]
    nTracks = len(zg.currQ)
    if (bWrap or zg.currTrack < nTracks - 1):
        zg.currTrack += 1
        zg.currTrack %= nTracks
        zg.currPos = 0
        sonos.skipNext(g_currentHHID, zgId)
        OnTrackChanged(zgId)
        return True
    return False

def prevTrack(zgId, bWrap):
    global g_currentHHID, g_dictZgs
    zg = g_dictZgs[zgId]
    nTracks = len(zg.currQ)
    if (bWrap or zg.currTrack > 0):
        zg.currTrack -= 1
        if (zg.currTrack < 0):
            zg.currTrack += nTracks
        zg.currPos = 0
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
    global g_qSignals, g_dictZgs
    while (1):
        sleep(1)
        if (not g_currentHHID):
            continue
        for zg in g_dictZgs.itervalues():
            nTracks = len(zg.currQ)
            if (nTracks == 0 or zg.playState != PS_PLAYING):
                continue

            # We only need to increment the counter until we reach the end,
            # once we reach the end, we should be receiving an event from
            # the sonos module to tell us to update the track.
            if (zg.currPos < zg.currQ[zg.currTrack]["PlayTime"]):
                zg.currPos += 1
                OnTick(zgId, zg.currPos)
        try:
            if (g_qSignals.get_nowait()):
                return
        except Exception, Empty:
            pass
    

def GetCurrentTrackTime(szZGID):
    global g_dictZgs
    try:
        return g_dictZgs[szZGID].currPos
    except:
        return -1

server.register_function(GetCurrentTrackTime)

def GetCurrentTrackMD(szZGID):
    global g_dictZgs
    try:
        zg = g_dictZgs[szZGID]
        return zg.currQ[zg.currTrack]
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
##sonos.cp._ssdp_server._register('uuid:RINCON_000E5850027001400',
##				'upnp:rootdevice',
##				"http://192.168.0.101:1400/xml/zone_player.xml",
##				"Linux UPnP/1.0 Sonos/12.3-22270", 1800)


# To kill:
# g_qSignals.put(True)
# eventLoop.join()
