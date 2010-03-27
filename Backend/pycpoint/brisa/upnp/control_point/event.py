# Licensed under the MIT license
# http://opensource.org/licenses/mit-license.php or see LICENSE file.
# Copyright 2007-2008 Brisa Team <brisa-develop@garage.maemo.org>

""" Control point side UPnP event support.
"""

print "develop event.py"

from xml.etree import ElementTree

from brisa.core import log, webserver
from brisa.core.threaded_call import run_async_function
from brisa.core.network import get_active_ifaces, get_ip_address


class EventListener(webserver.CustomResource):
    """ EventListener resource available at the control point web server,
    listening for events.
    """

    def __init__(self, observer):
        """ Constructor for the EventListener class

        @param observer: observer class with a _on_event() method.
        """
        webserver.CustomResource.__init__(self, 'eventSub')
        self.observer = observer
        
    def cleanup(self):
        """ Removes reference to observer to make GC easier """
        self.observer = None

    def render(self, uri, request, response):
        """ Event renderer method. As events come only on NOTIFY messages, this
        method ignores any other type of message (GET, POST, ...).

        @param uri: URI of the request
        @param request: request object (Cherrypy)
        @param response: response object (Cherrypy)

        @type uri: string

        @note: see Cherrypy documentation for further info about request and
        response attributes and methods.
        """
        log.debug('Received render (%s)' % str((uri, request, response)))

        if request.method == 'NOTIFY':
            log.debug('Ok, got notify!')
            self.render_NOTIFY(request, response)
        else:
            log.debug('Did not get notify, got %s' % request.method)

        log.debug('Returning from render')
        
        return ['']

    def render_NOTIFY(self, request, response):
        """ Renders the notify message for an event.

        @param request: request object (Cherrypy)
        @param response: response object (Cherrypy)

        @note: see Cherrypy documentation for further info about request and
        response attributes and methods.
        """
        data = request.read()
        # extraneous characters after the end of XML will choke ElementTree
        data = data[data.find("<"):data.rfind(">")+1]

        run_async_function(self.forward_notification, (request.headers, data),
                           0.0001)
        return ""

    def forward_notification(self, received_headers, data):
        """ Forwards notifications to the observer registered.

        @param received_headers: headers received on the event notify
        @param data: XML data for the event

        @type received_headers: dictionary
        @type data: string
        """
        log.debug('forward notification')
        headers = {}
        changed_vars = {}
        for k, v in received_headers.items():
            headers[k.lower()] = v

        try:
            tree = ElementTree.XML(data)
        except:
            log.debug('Event XML invalid: %s', data)
            tree = None

        if tree:
            for prop1 in tree.findall('{%s}property' %
                                      'urn:schemas-upnp-org:event-1-0'):
                # prop1 = <e:property> <Ble> cont </Ble> </e:property>
                for prop2 in prop1:
                    # <Ble>cont</Ble>
                    changed_vars[prop2.tag] = prop2.text

        log.debug('Event changed vars: %s', changed_vars)

        if self.observer and 'sid' in headers:
        
            seq_method = getattr(self.observer, '_on_event_seq', None)
            if callable(seq_method) and 'seq' in headers:
                self.observer._on_event_seq(headers['sid'], headers['seq'], changed_vars)
            else:            
                self.observer._on_event(headers['sid'], changed_vars)

            for id, dev in self.observer._known_devices.items():

                log.debug('id: %s - dev: %s', id, dev)
            
                service = self._find_service(dev, headers['sid'])
                if service != None:
                    service._on_event(changed_vars)
                    return

    def _find_service(self, device, subscription_id):
        """ Method to find a service with a specific subscription
        id on the given device or on it children devices.

        @param device: instance of a device
        @param subscription_id: the id to compare with the service

        @type device: RootDevice or Device
        @type subscription_id: str

        @return: if found, the service
        @rtype: Service or None
        """
        for k, service in device.services.items():
            if service.event_sid == subscription_id:
                return service
        for k, child_dev in device.devices.iteritems():
            service = self._find_service(child_dev, subscription_id)
            if service:
                return service
        return None


class EventListenerServer(object):
    """ EventListener server. Wraps BRisa's web server and listens for events.
    """

    def __init__(self, observer):
        """ Constructor for the EventListenerServer class.

        @param observer: observer that implements the _on_event() method
        """
        self.srv = None
        self.event_listener = EventListener(observer)

    def host(self):
        """ Returns a tuple in the form (host, port) where the server is being
        hosted at.

        @return: the host and port of the server host
        @rtype: tuple
        """
        if not self.srv:
            self.srv = webserver.WebServer()
            self.srv.start()
        return (self.srv.get_host(), self.srv.get_port())

    def start(self, event_host=None):
        if not self.srv:
            self.srv = webserver.WebServer()
            self.srv.start()
        if event_host:
            self.srv.listen_url = 'http://%s:%d' % event_host
        self.srv.add_resource(self.event_listener)

    def stop(self):
        """ Stops the EventListenerServer. For restarting after stopping with
        this method use EventListenerServer.srv.start().
        """
        if self.srv:
            self.srv.stop()

    def is_running(self):
        if self.srv:
            return self.srv.is_running()
        else:
            return False

    def destroy(self):
        if self.is_running():
            self.stop()
        self._cleanup()

    def _cleanup(self):
        self.srv = None
        self.event_listener.cleanup()
        self.event_listener = None
