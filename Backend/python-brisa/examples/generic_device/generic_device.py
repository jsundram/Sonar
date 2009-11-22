# Licensed under the MIT license
# http://opensource.org/licenses/mit-license.php or see LICENSE file.
# Copyright 2007-2008 Brisa Team <brisa-develop@garage.maemo.org>

from brisa.core.reactors import install_default_reactor
reactor = install_default_reactor()

from myservice import MyService, MyServiceWithScpd
from brisa.core import config
from brisa.upnp.device import Device


class MyDevice(object):

    def __init__(self):
        """ Constructor for class MyDevice, which fill some basic information
        about the device.
        """
        self.server_name = 'My Generic Device'
        self.root_device = None
        self.upnp_urn = 'urn:schemas-upnp-org:device:MyDevice:1'

    def _add_root_device(self):
        """ Creates the root device object which will represent the device
        description.
        """
        project_page = 'http://brisa.garage.maemo.org'
        serial_no = config.manager.brisa_version.replace('.', '').rjust(7, '0')
        self.root_device = Device(self.upnp_urn,
                             self.server_name,
                             manufacturer='BRisa Team. Embedded '\
                                          'Laboratory and INdT Brazil',
                             manufacturer_url=project_page,
                             model_description='An Open Source UPnP generic '\
                                               'Device',
                             model_name='Generic Device Example',
                             model_number=config.manager.brisa_version,
                             model_url=project_page,
                             serial_number=serial_no)

    def _add_services(self):
        # Creating the example Service
        myservice = MyService()

        # Inserting a service into the root device
        self.root_device.add_service(myservice)

    def _load(self):
        self._add_root_device()
        self._add_services()

    def start(self):
        self._load()
        self.root_device.start()
        reactor.add_after_stop_func(self.root_device.stop)
        reactor.main()

if __name__ == '__main__':
    device = MyDevice()
    device.start()
