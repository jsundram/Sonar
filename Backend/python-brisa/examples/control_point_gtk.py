#!/usr/bin/env python
# Licensed under the MIT license
# http://opensource.org/licenses/mit-license.php or see LICENSE file.
# Copyright 2007-2008 Brisa Team <brisa-develop@garage.maemo.org>
# Implementation of the UPnP Control Point Gui

from brisa.core.reactors import Gtk2Reactor
reactor = Gtk2Reactor()


import gtk
import gtk.glade

from brisa.upnp.control_point import ControlPointAV
from brisa.upnp.didl.didl_lite import Container

try:
    import hildon
    is_hildon = True
except:
    is_hildon = False


class ControlPointGUI(object):

    combobox_server_devices = None
    combobox_renderer_devices = None
    container_treestore = None
    container_tree_view = None
    current_media_id = None
    current_media_url = None
    known_media_servers = {}
    known_media_renderers = {}

    def __init__(self):
        self.control_point = ControlPointAV()
        self.control_point.subscribe("new_device_event", \
                                     self.on_new_device)
        self.control_point.subscribe("removed_device_event", \
                                     self.on_del_device)
        self.glade_xml = gtk.glade.XML('control_point_gtk.glade')
        self.create_all_screen_objects()

        signals={"gtk_main_quit": self._main_quit,
                 "on_quit_activate": self._main_quit,
                 "on_refresh_clicked": self._on_refresh_clicked,
                 "on_play_clicked": self._on_play_clicked,
                 "on_pause_clicked": self._on_pause_clicked,
                 "on_stop_clicked": self._on_stop_clicked,
                 "on_next_clicked": self._on_next_clicked,
                 "on_previous_clicked": self._on_previous_clicked}
        self.glade_xml.signal_autoconnect(signals)

        self.adjust_to_hildon()
        self.control_point.start()
        self.control_point.start_search(600.0,
                                        "ssdp:all")

    def create_all_screen_objects(self):
        self.create_server_combo_box()
        self.create_renderer_combo_box()
        self.create_container_tree_view()
        self.create_item_media_list()

    def create_server_combo_box(self):
        hbox = self.glade_xml.get_widget("hbox_servers")
        liststore = gtk.ListStore(str, str)
        self.combobox_server_devices = gtk.ComboBox(liststore)
        self.combobox_server_devices.connect('changed',
                                             self._changed_server_devices)

        cell = gtk.CellRendererText()
        self.combobox_server_devices.pack_start(cell, True)
        self.combobox_server_devices.add_attribute(cell, 'text', 0)

        hbox.add(self.combobox_server_devices)
        self.combobox_server_devices.show()

    def create_renderer_combo_box(self):
        hbox = self.glade_xml.get_widget("hbox_renders")
        liststore = gtk.ListStore(str, str)
        self.combobox_renderer_devices = gtk.ComboBox(liststore)
        self.combobox_renderer_devices.connect('changed',
                                               self._changed_renderer_devices)

        cell = gtk.CellRendererText()
        self.combobox_renderer_devices.pack_start(cell, True)
        self.combobox_renderer_devices.add_attribute(cell, 'text', 0)

        hbox.add(self.combobox_renderer_devices)
        self.combobox_renderer_devices.show()

    def create_container_tree_view(self):
        self.container_treestore = gtk.TreeStore(str, str)

        self.container_treeview = gtk.TreeView(self.container_treestore)
        self.container_treeview.connect("row_activated",
                                        self._on_container_treeview_activated,
                                        '')

        tvcolumn = gtk.TreeViewColumn('Containers')
        self.container_treeview.append_column(tvcolumn)
        cell = gtk.CellRendererText()
        tvcolumn.pack_start(cell, True)

        tvcolumn.add_attribute(cell, 'text', 0)
        tree_hbox = self.glade_xml.get_widget('tree_hbox')
        tree_hbox.add(self.container_treeview)
        self.container_treeview.show()

    def create_item_media_list(self):
        self.item_media_list_liststore = gtk.ListStore(str, str)

        self.item_media_list_treeview = \
                                   gtk.TreeView(self.item_media_list_liststore)
        self.item_media_list_treeview.connect("cursor-changed",
                                              self.\
                                              _on_media_item_listview_changed)
        self.item_media_list_treeview.connect("row_activated",
                                              self._on_play_clicked)

        tvcolumn = gtk.TreeViewColumn('Title')
        self.item_media_list_treeview.append_column(tvcolumn)
        cell = gtk.CellRendererText()
        tvcolumn.pack_start(cell, True)
        tvcolumn.add_attribute(cell, 'text', 0)

        tree_hbox = self.glade_xml.get_widget('list_viewport')
        tree_hbox.add(self.item_media_list_treeview)
        self.item_media_list_treeview.show()

    def create_main_window(self):
        window = hildon.Window()
        window.set_title("Hildon Brisa")
        window.connect("destroy", self._main_quit)

        main_menu = self._create_main_menu()
        window.set_menu(main_menu)

        window.show_all()
        return window

    def _create_main_menu(self):
        about_item = gtk.MenuItem("About")
        about_item.connect("activate", self._on_about_activated)
        quit_item = gtk.MenuItem("Quit")
        quit_item.connect("activate", self._main_quit)

        help_menu = gtk.Menu()
        help_menu.append(about_item)
        help_item = gtk.MenuItem("Help")
        help_item.set_submenu(help_menu)

        menu = gtk.Menu()
        menu.append(quit_item)
        menu.show()
        return menu

    def adjust_to_hildon(self):
        """Adjust the GUI to be usable in maemo platform."""
        if is_hildon:
            self.app = hildon.Program()
            hildon_main_window = self.create_main_window()
            self.app.add_window(hildon_main_window)
            gtk_main_vbox=self.glade_xml.get_widget('main_vbox')
            gtk_main_vbox.reparent(hildon_main_window)
            main_menu=self.glade_xml.get_widget('main_menu')
            main_menu.destroy()
            gtk_main_window=self.glade_xml.get_widget('main_window')
            gtk_main_window.destroy()

    def refresh(self):
        self.generate_server_list()
        self.generate_render_list()

    def generate_server_list(self):
        self._generate_combo('server')

    def generate_render_list(self):
        self._generate_combo('render')

    def _generate_combo(self, type):
        if type=='server':
            combo_box = self.combobox_server_devices
            devices = self.known_media_servers
        else:
            combo_box = self.combobox_renderer_devices
            devices = self.known_media_renderers

        list_store = combo_box.get_model()
        list_store.clear()
        list_store.append(["", "None"])

        append = list_store.append
        for device_object in devices.values():
            append([device_object.friendly_name,
                    device_object.udn])

    def browse_media_server(self, id, iter=None):
        browse_result = self.control_point.browse(id, 'BrowseDirectChildren',
                                                  '*', 0, 20, 'dc:title')
        items = browse_result['Result']
        total = int(browse_result['TotalMatches'])
        returned = int(browse_result['NumberReturned'])

        if total > 20:
            while returned < total:
                b = self.control_point.browse(id, 'BrowseDirectChildren',
                                              '*', returned, 20, 'dc:title')
                items = items + b['Result']
                returned += int(b['NumberReturned'])
        container_append = self.container_treestore.append
        liststore_append = self.item_media_list_liststore.append
        for item in items:
            if isinstance(item, Container):
                if [item.title, item.id] not in self.container_treestore:
                    container_append(iter, [item.title, item.id])
            else:
                liststore_append([item.title, item.uri])

    def pause(self):
        try:
            self.control_point.av_pause()
        except Exception, e:
            print 'Choose a Render to play Music. Problem: %s' % str(e)

    def play(self):
        try:
            self.control_point.av_play(uri=self.current_media_url)
        except Exception, e:
            print 'Choose a Render to play Music. Problem: %s' % str(e)

    def stop(self):
        try:
            self.control_point.av_stop()
            self.playing = False
        except Exception, e:
            print 'Choose a Render to play Music. Problem: %s' % str(e)

    def next(self):
        self.control_point.av_next()

    def previous(self):
        self.control_point.av_previous()

    def _main_quit(self, window=None):
        if window:
            window.destroy()
        reactor.main_quit()

    def _changed_server_devices(self, combobox):
        self.playing = False
        model = combobox.get_model()
        index = combobox.get_active()
        if type(index) == int and index >= 0:
            self.container_treestore.clear()
            try:
                self.control_point.set_current_server(self.known_media_servers[
                                                              model[index][1]])
                self.browse_media_server(0)
                self.item_media_list_liststore.clear()
            except KeyError, k:
                pass

    def _changed_renderer_devices(self, combobox):
        self.playing = False
        model = combobox.get_model()
        index = combobox.get_active()
        if type(index) == int and index >= 0:
            try:
                renderer = self.known_media_renderers[model[index][1]]
                self.control_point.set_current_renderer(renderer)
                self.stop()
            except:
                pass

    def on_new_device(self, device_object):
        t = device_object.device_type

        if 'MediaServer' in t:
            self.on_new_media_server(device_object)
        elif 'MediaRenderer' in t:
            self.on_new_media_renderer(device_object)

    def on_new_media_server(self, device_object):
        self.known_media_servers[device_object.udn] = device_object
        self.refresh()

    def on_new_media_renderer(self, device_object):
        self.known_media_renderers[device_object.udn] = device_object
        self.refresh()

    def on_del_device(self, udn):
        if udn in self.known_media_servers:
            if self.known_media_servers[udn] == \
                self.control_point.get_current_server():
                self.item_media_list_liststore.clear()
                self.container_treestore.clear()
            del self.known_media_servers[udn]
        elif udn in self.known_media_renderers:
            del self.known_media_renderers[udn]
        self.refresh()

    def _on_refresh_clicked(self, button):
        pass

    def _on_container_treeview_activated(self, treeview, path, row, data):
        (model, iter) = treeview.get_selection().get_selected()
        if not treeview.row_expanded(path):
            self.item_media_list_liststore.clear()
            self.browse_media_server(model.get_value(iter, 1), iter)
            treeview.expand_to_path(path)

    def _on_media_item_listview_changed(self, listview):
        (model, iter) = listview.get_selection().get_selected()
        self.current_media_url = model.get_value(iter, 1)

    def _on_pause_clicked(self, pause_button, *args, **kwargs):
        self.pause()

    def _on_play_clicked(self, button, *args, **kwargs):
        self.play()

    def _on_stop_clicked(self, stop_button, *args, **kwargs):
        self.stop()

    def _on_next_clicked(self, next_button, *args, **kwargs):
        self.next()

    def _on_previous_clicked(self, previous_button, *args, **kwargs):
        self.previous()

    def _on_about_activated(self, widget):
        pass # TODO

    def destroy_server_combo_box(self):
        self.combobox_server_devices.destroy()

    def destroy_renderer_combo_box(self):
        self.combobox_renderer_devices.destroy()


def main():
    gui = ControlPointGUI()
    gtk.gdk.threads_init()
    reactor.main()
    gui.control_point.destroy()


if __name__ == "__main__":
    main()
