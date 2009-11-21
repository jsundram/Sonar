# Licensed under the MIT license
# http://opensource.org/licenses/mit-license.php or see LICENSE file.
# Copyright 2007-2008 Brisa Team <brisa-develop@garage.maemo.org>

from brisa.utils.network_listeners import UDPListener
from brisa.threading import ThreadManager


def forward_data(data, addr=''):
    """ Forward data function which process the information how
        the developer desires.
    """
    print data, 'from ', addr
server = UDPListener('239.255.255.250',
                      15911,
                      '',
                      data_callback = forward_data)
# Be sure to register the UDPListener's stop function before using the
# run method, or the program doesn't end.
ThreadManager().register_stop_function(server.prepare_to_stop)
server.run()
