from brisa.core.reactors import install_default_reactor
reactor = install_default_reactor()

from brisa.upnp.control_point import ControlPoint

service = 'urn:schemas-upnp-org:service:SwitchPower:1'
binary_light_type = 'urn:schemas-upnp-org:device:BinaryLight:1'

class BinaryLightController(ControlPoint):

    def __init__(self):
        ControlPoint.__init__(self)
        self.subscribe('new_device_event', self.device_found)

    def device_found(self, dev):
        if dev.device_type == binary_light_type:
            if self.get_status(dev) == '1':
                print 'Found light (on). Switching off...' \
                    % dev.friendly_name
                self.switch_off(dev)
            else:
                print 'Found light (off). Switching on...' \
                    % dev.friendly_name
                self.switch_on(dev)

    def switch_on(self, lamp):
        switch = lamp.get_service_by_type(service)
        switch.SetTarget(NewTargetValue='1')

    def switch_off(self, lamp):
        switch = lamp.get_service_by_type(service)
        switch.SetTarget(NewTargetValue='0')

    def get_status(self, lamp):
        switch = lamp.get_service_by_type(service)
        return switch.GetStatus()['ResultStatus']

    def get_target(self, lamp):
        switch = lamp.get_service_by_type(service)
        return switch.GetTarget()['RetTargetValue']


ctl = BinaryLightController()

# Initializes the control point
ctl.start()

# Searches for binary lights every 20 seconds
ctl.start_search(20, binary_light_type)

# User commands
run_async_func(ctl.main)

# Main loop
reactor.add_after_stop_func(ctl.destroy)
reactor.main()
