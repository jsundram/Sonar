# Licensed under the MIT license
# http://opensource.org/licenses/mit-license.php or see LICENSE file.
# Copyright 2007-2008 Brisa Team <brisa-develop@garage.maemo.org>

from brisa.utils.network_senders import UDPTransport

client = UDPTransport()
try:
    client.send_data(raw_input('Send a message: '), ('localhost', 15911))
except EOFError:
    pass
