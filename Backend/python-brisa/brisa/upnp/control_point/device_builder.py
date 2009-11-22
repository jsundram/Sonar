# Licensed under the MIT license
# http://opensource.org/licenses/mit-license.php
# Copyright 2007-2008 Brisa Team <brisa-develop@garage.maemo.org>

""" Builder module for devices.
"""

from xml.etree.ElementTree import ElementTree

from brisa.core import log
from brisa.core.network import url_fetch
from brisa.core.threaded_call import run_async_call

from brisa.upnp.control_point import Service
from brisa.upnp.upnp_defaults import UPnPDefaults

import brisa


class DeviceBuilder(object):

    def __init__(self, device, location, tree):
        self.device = device
        self.tree = tree
        self.location = location
        self.ns = UPnPDefaults.NAME_SPACE_XML_SCHEMA
        self._build()

    def _build(self):
        self._parse_device()
        self._parse_icons()
        self._parse_services()
        self._parse_embedded_devices()
        self.device.generate_soap_services()

    def _parse_device(self):
        self.device.device_type = self.tree.\
                                     findtext('.//{%s}deviceType' % self.ns)
        self.device.friendly_name = self.tree.\
                                  findtext('.//{%s}friendlyName' % self.ns)
        self.device.manufacturer = self.tree.\
                                     findtext('.//{%s}manufacturer' % self.ns)
        self.device.manufacturer_url = self.tree.\
                                 findtext('.//{%s}manufacturerURL' % self.ns)
        self.device.model_description = self.tree.\
                              findtext('.//{%s}modelDescription' % self.ns)
        self.device.model_name = self.tree.\
                                   findtext('.//{%s}modelName' % self.ns)
        self.device.model_number = self.tree.\
                                     findtext('.//{%s}modelNumber' % self.ns)
        self.device.model_url = self.tree.\
                                  findtext('.//{%s}modelURL' % self.ns)
        self.device.serial_number = self.tree.\
                                  findtext('.//{%s}serialNumber' % self.ns)
        self.device.udn = self.tree.findtext('.//{%s}UDN' % self.ns)
        self.device.upc = self.tree.findtext('.//{%s}UPC' % self.ns)
        self.device.presentation_url = self.tree.\
                                 findtext('.//{%s}presentationURL' % self.ns)

    def _parse_services(self):
        services = self.tree.find('.//{%s}serviceList' % self.ns)

        if not services:
            return

        for xml_service_element in services.\
                                    findall('.//{%s}service' % self.ns):
            service_type = xml_service_element.\
                                    findtext('{%s}serviceType' % self.ns)
            service_id = xml_service_element.\
                                    findtext('{%s}serviceId' % self.ns)
            control_url = xml_service_element.\
                                    findtext('{%s}controlURL' % self.ns)
            event_sub_url = xml_service_element.\
                                    findtext('{%s}eventSubURL' % self.ns)
            presentation_url = xml_service_element.\
                                    findtext('{%s}presentationURL' % self.ns)
            scpd_url = xml_service_element.\
                                    findtext('{%s}SCPDURL' % self.ns)
            service = Service(service_id, service_type, self.location,
                              scpd_url, control_url, event_sub_url,
                              presentation_url)
            self.device.add_service(service)

    def _parse_icons(self):
        if self.tree.findtext('.//{%s}iconList' % self.ns) != None:
            for xml_icon_element in self.tree.findall('.//{%s}icon' % self.ns):
                mimetype = xml_icon_element.findtext('{%s}mimetype' % self.ns)
                width = xml_icon_element.findtext('{%s}width' % self.ns)
                height = xml_icon_element.findtext('{%s}height' % self.ns)
                depth = xml_icon_element.findtext('{%s}depth' % self.ns)
                url = xml_icon_element.findtext('{%s}url' % self.ns)
                self.device.add_icon(mimetype, width, height, depth, url)

    def _parse_embedded_devices(self):
        device_list = self.tree.find('.//{%s}deviceList' % self.ns)
        if device_list != None:
            embedded_device_tag = device_list.findall('.//{%s}device' %
                                                      self.ns)

            for xml_device_element in embedded_device_tag:
                d = self.device.__class__()
                DeviceBuilder(d, self.location,
                              xml_device_element).cleanup()
                self.device.add_device(d)

    def cleanup(self):
        self.device = None
        self.tree = None
        self.location = None


class DeviceAssembler(object):

    def __init__(self, device, location, filename=None):
        self.device = device
        self.location = location
        self.filename = filename

    def mount_device(self):
        if self.filename is None:
            filecontent = url_fetch(self.location)
            if not filecontent:
                return
            data = filecontent.read()
            data = data[data.find("<"):data.rfind(">")+1]
            tree = ElementTree(data).getroot()
        else:
            from xml.etree.ElementTree import parse
            tree = parse(self.filename)

        DeviceBuilder(self.device, self.location, tree).cleanup()
        return self.device

    def mount_device_async(self, callback, cargo):
        self.callback = callback
        self.cargo = cargo
        log.debug('self.location is %s' % self.location)

        if self.filename is None:
            run_async_call(url_fetch,
                           success_callback=self.mount_device_async_gotdata,
                           error_callback=self.mount_device_async_error,
                           delay=0, url=self.location)
        else:
            self.mount_device_async_gotdata(self, open(self.filename))

    def mount_device_async_error(self, cargo, error):
        log.error("Error fetching %s - Error: %s" % (self.location,
                                                     str(error)))
        self.callback(self.cargo, None)
        return True

    def mount_device_async_gotdata(self, fd, cargo=None):
        try:
            log.debug('to object async got data getting tree')
            tree = ElementTree(file=fd).getroot()
        except Exception, e:
            log.error("Bad device XML %s" % e.message)
            self.callback(self.cargo, None)
            return

        DeviceBuilder(self.device, self.location, tree).cleanup()
        if brisa.__skip_service_xml__:
            self.callback(self.cargo, self.device)
        else:
            log.debug("Fetching device services")
            log.debug("embedded devices: %s" % str(self.device.devices))
            embedded_services = []
            for d in self.device.devices.values():
                embedded_services.extend(d.services.items())

            log.debug("Embedded devices services are %s" % str(embedded_services))
            self.pending_services = len(self.device.services) + \
                                    len(embedded_services)
            for service_name, service_object in \
                     self.device.services.items():
                service_object._build_async(self.service_has_been_built)
            for s_name, s_obj in embedded_services:
                s_obj._build_async(self.service_has_been_built)

    def service_has_been_built(self, built_ok):
        log.debug("Service fetched, %d left, result %s", \
                self.pending_services - 1, built_ok)
        if not built_ok and not brisa.__tolerate_service_parse_failure__:
            log.warning("Device killed")
            self.device = None
        self.pending_services -= 1

        if self.pending_services <= 0:
            log.debug("All services fetched, sending device forward")
            self.callback(self.cargo, self.device)
