# Licensed under the MIT license
# http://opensource.org/licenses/mit-license.php or see LICENSE file.
# Copyright 2007-2008 Brisa Team <brisa-develop@garage.maemo.org>

from brisa.core.reactors import install_default_reactor
reactor = install_default_reactor()

import sys

from brisa.upnp.control_point.control_point_av import ControlPointAV
from brisa.core.threaded_call import run_async_function


class CommandLineControlPointAV(ControlPointAV):

    def __init__(self):
        ControlPointAV.__init__(self)
        self.running = True
        self._initial_subscribes()
        self.devices_found = []
        self.commands = {'start': self._search,
                         'stop': self._stop,
                         'list': self._cmd_list_devices,
                         'exit': self._exit,
                         'help': self._help}

    def _initial_subscribes(self):
        self.subscribe('new_device_event', self.on_new_device)
        self.subscribe('remove_device_event', self.on_remove_device)

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
                for child_dev in dev.devices.values():
                    print '\t\tudn:', child_dev.udn
                    print '\t\tfriendly_name:', child_dev.friendly_name
                    print '\t\tservices:', dev.services
                    print '\t\ttype:', child_dev.device_type
            print
            n += 1

    def _cmd_set_server(self, id):
        self._current_server = self.devices_found[id]

    def _cmd_set_render(self, id):
        self._current_renderer = self.devices_found[id]

    def _cmd_browse(self, id):
        result = self.browse(id, 'BrowseDirectChildren', '*', 0, 10)
        result = result['Result']
        for d in result:
            print "%s %s %s" % (d.id, d.title, d.upnp_class)

    def _search(self):
        self.start_search(600, 'upnp:rootdevice')
        print 'search started'

    def _stop(self):
        self.stop_search()
        print 'search stopped'

    def _help(self):
        print 'commands: start, stop, list, ' \
              'browse, set_server, set_render, play, exit, help'

    def _exit(self):
        self.running = False

    def run(self):
        self.start()
        run_async_function(self._handle_cmds)
        reactor.add_after_stop_func(self.stop)
        reactor.main()

    def _handle_cmds(self):
        try:
            while self.running:
                command = str(raw_input('>>> '))
                try:
                    self.commands[command]()
                except KeyError:
                    if command.startswith('browse'):
                        self._cmd_browse(command.split(' ')[1])
                    elif command.startswith('set_server'):
                        self._cmd_set_server(int(command.split(' ')[1]))
                    elif command.startswith('set_render'):
                        self._cmd_set_render(int(command.split(' ')[1]))
                    elif command.startswith('play'):
                        self.av_play(command.split(' ')[1])
                    else:
                        print 'Invalid command, try help'
                command = ''
        except KeyboardInterrupt, k:
            print 'quiting'

        reactor.main_quit()


def main():
    print "BRisa ControlPointAV example\n"
    cmdline = CommandLineControlPointAV()
    cmdline.run()

if __name__ == "__main__":
    main()
