from brisa.core.reactors import install_default_reactor
reactor = install_default_reactor()

from brisa.upnp.control_point import ControlPoint
from brisa.core.threaded_call import run_async_function

service = 'urn:schemas-upnp-org:service:SwitchPower:1'
binary_light_type = 'urn:schemas-upnp-org:device:BinaryLight:1'


class BinaryLightController(ControlPoint):

    def __init__(self):
        ControlPoint.__init__(self)
        self.lamps = []

    def get_lamps(self):
        self.lamps = [l for l in self.get_devices() if \
                      l.device_type == binary_light_type]
        return self.lamps

    def print_lamps(self);
        self.get_lamps()
        for i in range(len(self.lamps)):
            print 'Lamp %d: %s' % (i, self.lamps[i].friendly_name)

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

    def main(self):
        commands = {'switch_on': self.switch_on,
                    'switch_off': self.switch_off,
                    'get_status': self.get_status,
                    'get_target': self.get_target,
                    'get_lamps': self.print_lamps}
        while True:
            input, arg = raw_input('! ').strip().split(' ')
            if input[0] not in commands:
                print 'Invalid command'
            else:
                commands[input]()


ctl = BinaryLightController()
ctl.start()
ctl.start_search(2, binary_light_type)
run_async_func(ctl.main)

reactor.add_after_stop_func(ctl.destroy)
reactor.main()
