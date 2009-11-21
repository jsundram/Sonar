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

#xml = "http://10.20.10.140:1400/xml/zone_player.xml"
#my_device = Device()
#dev_assembler = DeviceAssembler(my_device, xml)
#dev_assembler.mount_device()
cp = ControlPointSonos()
cp.start()
cp.start_search(60)
