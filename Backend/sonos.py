import sys

# Set ourselves up to be able to import the pycpoint stuff without having to
# move it all to our local directory
sys.path.insert(1,"./pycpoint")

from brisa.core import log
from brisa.core.log import modcheck

import brisa
import brisa.core.reactors
reactor = brisa.core.reactors.install_default_reactor()

from control_point_sonos import ControlPointSonos
from brisa.upnp.control_point.device import Device
from brisa.upnp.control_point.device_builder import DeviceAssembler

from brisa.core.ireactor import EVENT_TYPE_READ

xml = "http://192.168.2.128:1400/xml/zone_player.xml"
my_device = Device()
dev_assembler = DeviceAssembler(my_device, xml)
dev_assembler.mount_device()
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
    for (udn, device) in cp.get_devices():
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
        hhInfo.["sid"] = sid
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
            zgs[zg.attrib['ID']] = {"coord" : zg.attrib['Coordinator'],
                                    "zones" : dictZones}
        except:
            pass

    return {"zgs": zgs}

def onChange(sid, seq, data):
    global g_Households
    try:
        hhInfo = g_Households[g_sids[sid]]
        hhInfo.update(_parsezgt(data))
        if (g_cbZGT):
            g_cbZGT(g_sids[sid])
    except:
        pass

def subToHHID(szHHID):
    global cp, g_Households
    try:
        hhInfo = g_Households[szHHID]
        # if we're not already subscribed to this household
        if not hhInfo.get("sid"):
            dev = hhInfo["device"]
            zgt_serv = dev.services[cp.ZT_namespace]
            cp.subscribe("device_event_seq", onChange)
            zgt_serv.event_subscribe(cp.event_host, cb, szHHID)
    except:
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

