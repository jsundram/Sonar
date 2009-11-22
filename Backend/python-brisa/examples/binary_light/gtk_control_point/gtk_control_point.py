from brisa.core.reactors import Gtk2Reactor
reactor = Gtk2Reactor()


import gtk
import gtk.glade
import os.path

gtk.gdk.threads_init()
from control_point_bl import ControlPointBL


class GTKControlPoint(object):

    def __init__(self):
        self.devices = {}
        self.cp = ControlPointBL()
        self.cp.subscribe('new_device_event', self.on_new_device)
        self.cp.subscribe('removed_device_event', self.on_removed_device)
        self.glade = gtk.glade.XML(os.path.join(os.path.dirname(__file__),
                                   'lightcp.glade'))
        self.main_window = self.glade.get_widget('cpwindow')
        self.image0 = self.glade.get_widget('image0')
        self.image1 = self.glade.get_widget('image1')
        self.main_window.connect('destroy', self.quit)
        self.set_target_window = self.glade.get_widget('set_target_window')
        self.set_target_window.connect('destroy', self.ok)
        self.vbox = self.glade.get_widget('vboxk')
        self.combobox = gtk.combo_box_new_text()
        self.combobox.show()
        self.vbox.pack_start(self.combobox)
        self.vbox.reorder_child(self.combobox, 0)
        self.value_checkbt = self.glade.get_widget('value')

        signals = {'on_get_status_clicked': self.get_status,
                   'on_get_target_clicked': self.get_target,
                   'on_set_target_clicked': self.set_target,
                   'on_cancel_clicked': self.cancel,
                   'on_ok_clicked': self.ok}

        self.glade.signal_autoconnect(signals)

    def run(self):
        self.main_window.show()
        self.cp.start_search(600, 'upnp:rootdevice')
        reactor.add_after_stop_func(self.cp.stop)
        reactor.main()

    def quit(self, widget=None):
        reactor.main_quit()

    def on_new_device(self, device):
        if device.device_type != 'urn:schemas-upnp-org:device:BinaryLight:1':
            return
        self.devices[device.udn] = device
        self.sync_combobox()

    def on_removed_device(self, udn):
        for d in self.devices:
            if d.udn == udn:
                self.devices.remove(d)
        self.sync_combobox()

    def sync_combobox(self):
        self.combobox.get_model().clear()

        for d in self.devices.values():
            self.combobox.append_text(str(d.friendly_name))

        self.combobox.set_active(0)

    def _get_selected_device(self):
        selected = self.combobox.get_active()
        model = self.combobox.get_model()
        if selected < 0:
            return None
        selected = model[selected][0]
        device = None
        for d in self.devices.values():
            if d.friendly_name == selected:
                device = d

        if not device:
            raise Exception('Device is None (critical)')

        return device

    def get_status(self, widget):
        self.cp.current_server = self._get_selected_device()
        value = self.cp.get_status()
        self.image0.hide()
        self.image0.set_from_stock('gtk-yes' if value else 'gtk-no',
                                   gtk.ICON_SIZE_BUTTON)
        self.image0.show()

    def get_target(self, widget):
        self.cp.current_server = self._get_selected_device()
        value = self.cp.get_target()
        self.image1.hide()
        self.image1.set_from_stock('gtk-yes' if value else 'gtk-no',
                                   gtk.ICON_SIZE_BUTTON)
        self.image1.show()

    def set_target(self, widget):
        self.set_target_window.show()

    def cancel(self, widget):
        self.set_target_window.hide()
        self.main_window.show()

    def ok(self, widget=None):
        self.set_target_window.hide()
        self.main_window.show()
        self.cp.current_server = self._get_selected_device()
        k = self.value_checkbt.get_active()
        self.cp.set_target(k)

if __name__ == '__main__':
    g = GTKControlPoint()
    g.run()
