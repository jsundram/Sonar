import sys

# Set ourselves up to be able to import the pycpoint stuff without having to
# move it all to our local directory
sys.path.insert(1,"./pycpoint")
# Also import the rest of brisa from local sources
sys.path.insert(2,"./python-brisa")

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

def onSub(szHHID, sid, ttl):
    global g_Households, g_sids
    try:
        hhInfo = g_Households[szHHID]
        hhInfo["sid"] = sid
        # We also need to be able to get from a sid back to a HH, so we'll put
        # that in a dict too
        g_sids[sid] = szHHID
    except:
        pass

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
        except:
            pass

    return {"zgs": zgs}

g_cbZGT = None

def onChange(sid, seq, data):
    global g_Households, g_cbZGT
    try:
        hhInfo = g_Households[g_sids[sid]]
        hhInfo.update(_parsezgt(data))
        if (g_cbZGT):
            g_cbZGT(g_sids[sid])
    except:
        pass

def subToHHID(szHHID, zgtCallBack):
    global cp, g_Households
    try:
        hhInfo = g_Households[szHHID]
        # if we're not already subscribed to this household
        if not hhInfo.get("sid"):
            dev = hhInfo["device"]
            zgt_serv = dev.services[cp.ZT_namespace]
            cp.subscribe("device_event_seq", onChange)
            zgt_serv.event_subscribe(cp.event_host, onSub, szHHID)
        return True
    except Exception, e:
        print e
        return False

def getDevicesForHHID(szHHID):
    global cp
    setDevs = set()
    for (udn, device) in cp.get_devices():
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
    except:
        return []

g_dictMDCache = {}

def enqueueTrack(hhId, zgId, md):
    global g_dictMDCache, cp, g_Households
    try:
        zgCoord = g_Households[hhId]["zgs"][zgId]["coord"]
        dev = cp.get_devices()["uuid:" + zgCoord]
        avt = dev.get_service_by_type(cp.AVT_namespace)
        ret = avt.AddURIToQueue(InstanceID=0, EnqueuedURI=md["Uri"],
                          EnqueuedURIMetaData=",".join(["A" + md["Title"],
                                                        md["Artist"],
                                                        md["Album"],
                                                        md["PlayTime"]]),
                          DesiredFirstTrackNumberEnqueued=0,
                          EnqueueAsNext=False)
        num = ret['FirstTrackNumberEnqueued']
        g_dictMDCache[md["Uri"]] = md
        return True
    except Exception, e:
        print e
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
        print e
        return False

def pause(hhId, zgId):
    return False

def skipNext(hhId, zgId):
    return False

def skipBack(hhId, zgId):
    return False
