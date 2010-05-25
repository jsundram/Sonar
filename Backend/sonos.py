import sys

# Set ourselves up to be able to import the pycpoint stuff without having to
# move it all to our local directory
sys.path.insert(1,"./pycpoint")
# Also import the rest of brisa from local sources
sys.path.insert(2,"./python-brisa")
# And CherryPy
sys.path.insert(3,"./python-cherrypy")

from brisa.core import log
from brisa.core.log import modcheck

import brisa
import brisa.core.reactors
reactor = brisa.core.reactors.install_default_reactor()

from control_point_sonos import ControlPointSonos
from brisa.upnp.control_point.device import Device
from brisa.upnp.control_point.device_builder import DeviceAssembler

from brisa.core.ireactor import EVENT_TYPE_READ

##xml = "http://10.0.0.176:1400/xml/zone_player.xml"
##my_device = Device()
##dev_assembler = DeviceAssembler(my_device, xml)
##dev_assembler.mount_device()
cp = ControlPointSonos()
#cp.start()
#cp.start_search(60)

import socket
from struct import pack

##my_sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM,
##                                         socket.IPPROTO_UDP)
##
##my_sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
##my_sock.bind(('', 1900))
##mreq = pack('4sl', socket.inet_aton('239.255.255.250'),
##                             socket.INADDR_ANY)
##my_sock.setsockopt(socket.IPPROTO_IP, socket.IP_ADD_MEMBERSHIP, mreq)

from threading import Thread
def pump_cp_UDP():
    global bRunning
    bRunning = True
    while(bRunning):
            cp._ssdp_server.udp_listener._receive_datagram(None, None)

def print_UDP(arg1, arg2):
    print my_sock.recvfrom(1500)
    
thReactor = Thread(target=reactor.main, name="Reactor", args=())

cp.start()
cp.start_search(5)

#reactor.add_fd(my_sock, print_UDP, EVENT_TYPE_READ)
thReactor.start()

g_Households = {}
g_sids = {}

def getHHIDs():
    global cp, g_Households
    for (udn, device) in cp.get_devices().iteritems():
        if ("RINCON" in udn):
            dp_svc = device.services[cp.DP_namespace]
            hhid = dp_svc.GetHouseholdID().values()[0]
            if hhid not in g_Households:
                # Store this zp as my associated zp
                g_Households[hhid] = {"device": device}
    return g_Households.keys()

def _parsezgt(data):
    zgs_xml = data['ZoneGroupState']
    from xml.etree import ElementTree
    tree = ElementTree.fromstring(zgs_xml)
    zgs = {}
    for zg in tree.findall("ZoneGroup"):
        try:
            dictZones = {}
            for zone in zg.findall("ZoneGroupMember"):
                dictZones[zone.attrib["UUID"]] = zone.attrib
            zgs[zg.attrib['ID']] = {"coord" : zg.attrib['Coordinator'],
                                    "zones" : dictZones}
        except Exception, e:
            print "Exception in _parsezgt"
            print e

    return {"zgs": zgs}

def convertHMSToS(hms):
    # Catch the case where we are trying to convert strings from a stream
    # rather than an actual track.
    if (hms == 'NOT_IMPLEMENTED'):
        return -1
    
    total = 0
    for num in hms.split(":"):
        total *= 60
        total += int(num)

    return total

def convertSToHMS(s):
    return "%02d:%02d:%02d" % (s/3600, s / 60 % 60, s % 60)

def _fixupAAURI(s):
    if s.startswith('/getaa?'):
        return "http://" + "10.20.10.100:1400" + s

def _parseQueue(data):
    q_xml = data['Result']
    from xml.etree import ElementTree
    tree = ElementTree.fromstring(q_xml)
    q = []
    # heinous, heinous namespaces
    didl = "{urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/}"
    dc = "{http://purl.org/dc/elements/1.1/}"
    upnp = "{urn:schemas-upnp-org:metadata-1-0/upnp/}"
    r = "{urn:schemas-rinconnetworks-com:metadata-1-0/}"
    for track in tree.findall(didl+"item"):
        trackMD = {}
        res = track.find(didl+'res')
        trackMD['Uri'] = res.text
        trackMD['PlayTime'] = convertHMSToS(res.attrib.get('duration', '0'))
        trackMD['Title'] = track.findtext(dc + 'title')
        trackMD['Artist'] = track.findtext(dc + 'creator')
        trackMD['Album'] = track.findtext(upnp + 'album')
        trackMD['AlbumArtUri'] = _fixupAAURI(track.findtext(upnp + 'albumArtURI'))
        q.append(trackMD)

    return q

def _parseAudioIn(data, devName):
    q_xml = data['Result']
    from xml.etree import ElementTree
    tree = ElementTree.fromstring(q_xml)
    aiList = []
    # heinous, heinous namespaces
    didl = "{urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/}"
    dc = "{http://purl.org/dc/elements/1.1/}"
    upnp = "{urn:schemas-upnp-org:metadata-1-0/upnp/}"
    r = "{urn:schemas-rinconnetworks-com:metadata-1-0/}"
    for ai in tree.findall(didl+"item"):
        aiMD = {}
        res = ai.find(didl+'res')
        aiMD['Uri'] = res.text
        aiMD['Title'] = "%s: %s" % (ai.findtext(dc + 'title'), devName)
        aiList.append(trackMD)

    return aiList

g_cbZGT = None

def onEvent(sid, seq, data):
    global g_sids
    if (sid in g_sids):
        fun, args = g_sids[sid]
        fun(seq, data, *args)

def onZgtChange(seq, data, szHHID):
    global g_Households, g_cbZGT
    try:
        hhInfo = g_Households[szHHID]
        oldZgs = hhInfo.get("zgs", {})
        hhInfo.update(_parsezgt(data))
        newZgs = hhInfo.get("zgs", {})
        # we need to now subscribe to all the zgs that we just discovered
        # And unsubscribe to the ones that went away
        # First figure out what Zone Groups have gone away
        oldZgIds = set(oldZgs.iterkeys())
        newZgIds = set(newZgs.iterkeys())
        zgsGone = oldZgIds - newZgIds
        zgsNew = newZgIds - oldZgIds
        zgCoordsSub = dict(((newZgs[zgid]["coord"], zgid) for zgid in zgsNew))
        zgCoordsUnsub = {}
        for zgid in zgsGone:
            # if we never managed to subscribe, then there's nothing else to do with it.
            if ("sid" not in oldZgs[zgid]):
                continue
            
            coord = oldZgs[zgid]["coord"]
            # If a coordinator for a now defunct zg is also the coordinator for a new shiny group
            if (coord in zgCoordsSub):
                # Copy the sid from the old zonegroup to the new one
                newZgs[zgCoordsSub[coord]]['sid'] = oldZgs[zgid]['sid']
            else:
                # Otherwise, add it to the reject list.
                zgCoordsUnsub[coord] = oldZgs[zgid]['sid']

        # Now unsubscribe the old ones
        for (coord, sid) in zgCoordsUnsub.iteritems():
            uuid = "uuid:" + coord
            if (uuid in cp.get_devices()):
                dev = cp.get_devices()[uuid]
                avt_serv = dev.services[cp.AVT_namespace]
                if (avt_serv.event_sid == sid):
                    avt_serv.event_unsubscribe(cp.event_host)
                g_sids.pop(sid, None)
            else:
                print "Unsubscribe: Unable to find coordinator", coord, "for subscription", sid

        # And subscribe to the new ones
        for (coord, zgid) in zgCoordsSub.iteritems():
            uuid = "uuid:" + coord
            if (uuid in cp.get_devices()):
                dev = cp.get_devices()[uuid]
                avt_serv = dev.services[cp.AVT_namespace]
                avt_serv.event_subscribe(cp.event_host, onSubAvt, (szHHID, zgid))
            else:
                print "Subscribe: Unable to find coordinator", coord, "for zone group", zgid

        if (g_cbZGT):
            g_cbZGT(szHHID)
    except Exception, e:
        print "Exception in onZgtChange"
        print e

def onSubZgt(szHHID, sid, ttl):
    global g_Households, g_sids, onZgtChange
    try:
        hhInfo = g_Households[szHHID]
        hhInfo["sid"] = sid
        # We also need to be able to get from a sid back to a change function
        # and its arguments, so we'll put that in a dict too
        g_sids[sid] = (onZgtChange, (szHHID,))
    except Exception, e:
        print "Exception in onSubZgt"
        print e

g_cbAVT = None

def onAvtChange(seq, data, hhid, zgid):
    global g_Households, g_cbAVT
    if (g_cbAVT):
        g_cbAVT(hhid, zgid)

def onSubAvt((szHHID, zgId), sid, ttl):
    global g_Households, g_sids, onAvtChange
    try:
        zgInfo = g_Households[szHHID]['zgs'][zgId]
        zgInfo["sid"] = sid
        # We also need to be able to get from a sid back to a HH, so we'll put
        # that in a dict too
        g_sids[sid] = (onAvtChange, (szHHID, zgId))
    except Exception, e:
        print e


def subToHHID(szHHID, zgtCallBack = None, avtCallback = None):
    global cp, g_Households, g_cbZGT
    try:
        g_cbZGT = zgtCallBack
        g_cbAVT = avtCallback
        hhInfo = g_Households[szHHID]
        # if we're not already subscribed to this household
        if "sid" not in hhInfo:
            dev = hhInfo["device"]
            zgt_serv = dev.services[cp.ZT_namespace]
            cp.subscribe("device_event_seq", onEvent)
            zgt_serv.event_subscribe(cp.event_host, onSubZgt, szHHID)
        return True
    except Exception, e:
        print "Exception in subToHHID"
        print e
        return False

def getDevicesForHHID(szHHID):
    global cp
    setDevs = set()
    for (udn, device) in cp.get_devices().iteritems():
        if ("RINCON" in udn):
            dp_svc = device.services['urn:schemas-upnp-org:service:DeviceProperties:1']
            if (szHHID == dp_svc.GetHouseholdID().values()[0]):
                setDevs.add(device)
    return setDevs

def getZgIdsForHHID(szHHID):
    global g_Households
    try:
        hhInfo = g_Households[szHHID]
        return [zgId for zgId in hhInfo["zgs"].keys()]
    except Exception, e:
        print "Exception in getZgIdsForHHID"
        print e
        return []

def getTrackInfoForZg(hhId, zgId):
    global cp, g_Households
    res = {}
    try:
        zgCoord = g_Households[hhId]["zgs"][zgId]["coord"]
        dev = cp.get_devices()["uuid:" + zgCoord]
        avt = dev.get_service_by_type(cp.AVT_namespace)
        res = avt.GetPositionInfo(InstanceID=0)
        num = int(res['Track'])
        dur = convertHMSToS(res['TrackDuration'])
        currTime = convertHMSToS(res['RelTime'])
        return (num - 1, currTime, dur) # tracks numbers are '1' based in upnp
    except Exception, e:
        print "Exception in getTrackInfoForZg"
        print e
        print res
        return -1

def getPlayStateForZg(hhId, zgId):
    global cp, g_Households
    res = {}
    try:
        zgCoord = g_Households[hhId]["zgs"][zgId]["coord"]
        dev = cp.get_devices()["uuid:" + zgCoord]
        avt = dev.get_service_by_type(cp.AVT_namespace)
        res = avt.GetTransportInfo(InstanceID=0)
        ps = res['CurrentTransportState']
        return ps
    except Exception, e:
        print "Exception in getPlayStateForZg"
        print e
        print res
        return "UNKNOWN"

g_dictMDCache = {}

def getQueue(hhId, zgId):
    global g_dictMDCache, cp, g_Households
    ret = {}
    try:
        zgCoord = g_Households[hhId]["zgs"][zgId]["coord"]
        dev = cp.get_devices()["uuid:" + zgCoord]
        cd = dev.get_service_by_type(cp.CD_namespace)
        ret = cd.Browse(ObjectID="Q:0", BrowseFlag="BrowseDirectChildren",
                        Filter="", SortCriteria="", StartingIndex=0, RequestedCount=0)
        q = _parseQueue(ret)
        # replace the found MD with the better (enqueued) MD if we have it
        q = map(lambda x: g_dictMDCache.get(x["Uri"],x), q)
        return q
    except Exception, e:
        print "Exception in getQueue"
        print e
        print ret
        return []

def getLineIn(hhId, zpId):
    global g_dictMDCache, cp, g_Households
    ret = {}
    try:
        dev = cp.get_devices()["uuid:" + zpId]
        cd = dev.get_service_by_type(cp.CD_namespace)
        ret = cd.Browse(ObjectID="AI:", BrowseFlag="BrowseDirectChildren",
                        Filter="", SortCriteria="", StartingIndex=0, RequestedCount=0)
        ai = _parseAudioIn(ret)
        return ai
    except Exception, e:
        print "Exception in getLineIn"
        print e
        print ret
        return []

def enqueueTrack(hhId, zgId, md):
    global g_dictMDCache, cp, g_Households
    ret = {}
    try:
        zgCoord = g_Households[hhId]["zgs"][zgId]["coord"]
        dev = cp.get_devices()["uuid:" + zgCoord]
        avt = dev.get_service_by_type(cp.AVT_namespace)
        ret = avt.AddURIToQueue(InstanceID=0, EnqueuedURI=md["Uri"],
                          EnqueuedURIMetaData=",".join(["A" + md["Title"],
                                                        md["Artist"],
                                                        md["Album"],
                                                        convertSToHMS(md["PlayTime"])]),
                          DesiredFirstTrackNumberEnqueued=0,
                          EnqueueAsNext=False)
        num = ret['FirstTrackNumberEnqueued']
        g_dictMDCache[md["Uri"]] = md
        return True
    except Exception, e:
        print "Exception in enqueueTrack"
        print e
        print ret
        return False

def play(hhId, zgId):
    global g_dictMDCache, cp, g_Households
    try:
        zgCoord = g_Households[hhId]["zgs"][zgId]["coord"]
        dev = cp.get_devices()["uuid:" + zgCoord]
        avt = dev.get_service_by_type(cp.AVT_namespace)
        avt.Play(InstanceID=0, Speed="1")
        return True
    except Exception, e:
        print "Exception in play"
        print e
        return False

def seekTrack(hhId, zgId, nTrack):
    global g_dictMDCache, cp, g_Households
    try:
        zgCoord = g_Households[hhId]["zgs"][zgId]["coord"]
        dev = cp.get_devices()["uuid:" + zgCoord]
        avt = dev.get_service_by_type(cp.AVT_namespace)
        avt.Seek(InstanceID=0, Unit="TRACK_NR", Target=str(nTrack))
        return True
    except Exception, e:
        print "Exception in seekTrack"
        print e
        return False

def pause(hhId, zgId):
    global g_dictMDCache, cp, g_Households
    try:
        zgCoord = g_Households[hhId]["zgs"][zgId]["coord"]
        dev = cp.get_devices()["uuid:" + zgCoord]
        avt = dev.get_service_by_type(cp.AVT_namespace)
        avt.Pause(InstanceID=0)
        return True
    except Exception, e:
        print "Exception in pause"
        print e
        return False

def skipNext(hhId, zgId):
    global g_dictMDCache, cp, g_Households
    try:
        zgCoord = g_Households[hhId]["zgs"][zgId]["coord"]
        dev = cp.get_devices()["uuid:" + zgCoord]
        avt = dev.get_service_by_type(cp.AVT_namespace)
        avt.Next(InstanceID=0)
        return True
    except Exception, e:
        print "Exception in skipNext"
        print e
        return False

def skipBack(hhId, zgId):
    global g_dictMDCache, cp, g_Households
    try:
        zgCoord = g_Households[hhId]["zgs"][zgId]["coord"]
        dev = cp.get_devices()["uuid:" + zgCoord]
        avt = dev.get_service_by_type(cp.AVT_namespace)
        avt.Back(InstanceID=0)
        return True
    except Exception, e:
        print "Exception in skipBack"
        print e
        return False
