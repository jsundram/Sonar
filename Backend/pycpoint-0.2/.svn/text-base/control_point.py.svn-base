# Licensed under the MIT license
# http://opensource.org/licenses/mit-license.php or see LICENSE file.
# Copyright 2007-2008 Brisa Team <brisa-develop@garage.maemo.org>

import sys
from brisa.control_point.control_point import ControlPoint
from brisa.threading import ThreadManager


class CommandLineControlPoint(ControlPoint):

    def __init__(self):
        ControlPoint.__init__(self)
        self.running = False
        self._initial_subscribes()
        self.devices_found = []
        self.commands = {'search': self._search,
                         'stop': self._stop,
                         'exit': self._exit,
                         'list': self._cmd_list_devices,
                         'help': self._help}

    def _initial_subscribes(self):
        self.subscribe('new_device_event', self.on_new_device)
        self.subscribe('removed_device_event', self.on_remove_device)

    def on_new_device(self, dev):
        self.devices_found.append(dev)

    def on_remove_device(self, udn):
        for dev in self.devices:
            if dev.udn == udn:
                self.devices_found.remove(dev)
                break

    def _cmd_list_devices(self):
        n = 0
        for dev in self.devices_found:
            print 'device %d:' % n
            print '\tudn:', dev.udn
            print '\tfriendly_name:', dev.friendly_name
            print '\tservices:', dev.services
            print '\ttype:', dev.device_type
            if dev.devices:
                print '\tchild devices:'
                for child_dev in dev.devices:
                    print '\t\tudn:', child_dev.udn
                    print '\t\tfriendly_name:', child_dev.friendly_name
                    print '\t\tservices:', dev.services
                    print '\t\ttype:', child_dev.device_type
            print
            n += 1

    def _search(self):
        self.start_search(600, 'upnp:rootdevice')

    def _stop(self):
        self.stop_search()

    def _exit(self):
        self.running = False

    def _help(self):
        print 'commands: search, stop, list, ' \
              'exit'

    def run(self):
        self.running = True
        try:
            while self.running:
                command = str(raw_input('>>> '))
                try:
                    self.commands[command]()
                except KeyError:
                    print 'invalid command, try help'
                command = ''
        except KeyboardInterrupt, k:
            pass
        except EOFError, k:
            pass
        print 'quiting'
        ThreadManager().stop_all()
        return


def main():
    print "BRisa ControlPoint example\n"
    cmdline = CommandLineControlPoint()
    cmdline.run()

if __name__ == "__main__":
    main()
