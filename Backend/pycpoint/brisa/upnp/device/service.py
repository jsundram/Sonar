# Licensed under the MIT license
# http://opensource.org/licenses/mit-license.php or see LICENSE file.
# Copyright 2007-2008 Brisa Team <brisa-develop@garage.maemo.org>

""" Device-side service implementation. Used for implementing and deploying
UPnP services.
"""

print "develop debug only device.service.py"

from os import path, mkdir

from brisa.core import log, config, failure, webserver
 
from brisa.upnp.base_service import BaseService, BaseStateVariable
from brisa.upnp.base_service_builder import BaseServiceBuilder
from brisa.upnp.device.action import Action, Argument
from brisa.upnp.device.event import EventController
from brisa.upnp.device.xml_gen import ServiceXMLBuilder
from brisa.upnp import soap


class ErrorCode(Exception):
    """ Wrapper for an error code. Contains a status attribute that corresponds
    with the error code.
    """

    def __init__(self, status):
        self.status = status


class InvalidService(Exception):
    pass


class StateVariable(BaseStateVariable):

    def __init__(self, service, name, send_events, multicast, data_type, values=[]):
        BaseStateVariable.__init__(self, service, name, send_events,
                                   multicast, data_type, values)


class ServiceBuilder(BaseServiceBuilder):

    def build(self):
        print ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>ServiceBuilder.build"
        try:
#            return BaseServiceBuilder.build(self)

            bsb = BaseServiceBuilder.build(self)
            print 'bsb'
            print bsb
            return bsb
        except:
            raise InvalidService('Invalid scpd.xml')

    def _create_argument(self, arg_name, arg_direction, arg_state_var):
        return Argument(arg_name, arg_direction, arg_state_var)

    def _create_action(self, name, args):
        print ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>ServiceBuilder.create_action"
        """ Creates an action and sets it run function to soap_ActionName(). """
        action = Action(self.service, name, args)
        action.run_function = getattr(self.service, "soap_%s" % name, None)
        return action

    def _create_state_var(self, name, send_events, multicast,
                          data_type, values):
        return StateVariable(self.service, name, send_events,
                             multicast, data_type, values)


class ServiceController(webserver.CustomResource):
    """ Wrapper for receiving soap calls and assigning them to correspondent
    methods. Extend UPnPPublisher and add the class to the web server as a
    resource and your methods will be exported.
    """
    encoding = "UTF-8"

    def __init__(self, service, service_type):
        webserver.CustomResource.__init__(self, 'control')
        self.service = service
        self.service_type = service_type

    def render(self, uri, request, response):
        """ Renders a request received.
        """
        data = request.read()
        headers = request.headers

        method_name, args, kwargs, ns = soap.parse_soap_call(data)
        try:
            headers['content-type'].index('text/xml')
        except:
            # Builds error if we don't have an content-type field with xml
            return self._build_error(failure.Failure(ErrorCode(415)), request,
                           method_name, response)

        function = self.lookup_function(method_name)

        if not function:
            return self._method_not_found(request, response, method_name)
        else:
            return self._get_call_response(request, response, method_name,
                                         function, *args, **kwargs)

        return ['']

    def lookup_function(self, function_name):
        """ Lookup published SOAP function.
        """
        log.info('Finding service action %s' % function_name)
        for action_name, action in self.service._actions.iteritems():
            if action_name == function_name:
                return action
        log.info('Action %s not founded' % function_name)
        return None

    def _get_call_response(self, request, response_obj, method_name,
                         function, *args, **kwargs):
        """ Performs the soap call, builds and returns a response.
        """
        result = function(*args, **kwargs)

        ns = self.service_type
        try:
            method = result.keys()[0]
            result = result[method]
        except AttributeError, IndexError:
            result = {}
            method = ''
        response = soap.build_soap_call("{%s}%s" % (ns, method),
                                        result, encoding=None)
        return self._build_response(request, response, response_obj)

    def _build_error(self, failure, request, method_name, response_obj):
        """ Builds an error based on the failure code.
        """
        e = failure.value
        status = 500

        if isinstance(e, ErrorCode):
            status = e.status
        else:
            failure.printTraceback()

        response = soap.build_soap_error(status)
        return self._build_response(request, response, response_obj,
                                   status=status)

    def _build_response(self, request, response, response_object, status=200):
        """ Builds a response for a call.
        """
        if status == 200:
            response_object.status = 200
        else:
            response_object.status = 500

        if self.encoding is not None:
            mime_type = 'text/xml; charset="%s"' % self.encoding
        else:
            mime_type = "text/xml"
        response_object.headers["Content-type"] = mime_type
        response_object.headers["Content-length"] = str(len(response))
        response_object.headers["EXT"] = ''
        response_object.body = response
        return response

    def _method_not_found(self, request, response_obj, method_name):
        """ Treats the method not found error.
        """
        response = soap.build_soap_error(401)
        return self._build_response(request, response, response_obj,
                                    status=401)


class Service(BaseService):

    def __init__(self, id, serv_type, url_base='',
                 scpd_xml_filepath='', presentation_controller=None):
        print ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>Service.__init__"
        BaseService.__init__(self, id, serv_type, url_base)

        self.control_controller = ServiceController(self, self.service_type)
        self.eventSub_controller = None
        self.presentation_controller = presentation_controller

        if not scpd_xml_filepath:
            self._generate_xml()
            self._create_xml = True
        else:
            self._xml_filepath = scpd_xml_filepath
            fd = open(self._xml_filepath, 'r')
            if not ServiceBuilder(self, fd).build():
                raise InvalidService('Error building the service %s' % id)
            fd.close()
            self._create_xml = False

    def publish(self, webserver):
        log.info('Publishing service %s' % self.id)

        if not len(self.get_variables()):
            raise InvalidService('The service must have one '\
                                        'or more state variables')

        res = webserver.CustomResource(self.id)
        if self._create_xml:
            ServiceXMLBuilder(self).generate_to_file(self._xml_filepath)
        f = webserver.StaticFile('scpd.xml', self._xml_filepath)
        res.add_static_file(f)

        for k in ['control', 'eventSub', 'presentation']:
            controller = getattr(self, '%s_controller' % k)
            if controller:
                res.add_resource(controller)

        webserver.add_resource(res)

    def add_state_variable(self, state_variable):
        if not self.eventSub_controller and state_variable.send_events:
            self.eventSub_controller = EventController(self)
        self._state_variables[state_variable.name] = state_variable

    def set_state_variable(self, name, value):
        state_variable = self._state_variables[name]
        state_variable.update(value)

    def add_action(self, action):
        action.service = self
        self._actions[action.name] = action

    def start(self, *args, **kwargs):
        pass

    def _generate_xml(self):
        self.xml_filename = '%s-scpd.xml' % self.id
        self.xml_filename = self.xml_filename.replace(' ', '')
        self._xml_filepath = path.join(config.manager.brisa_home, 'tmp_xml')
        if not path.exists(self._xml_filepath):
            mkdir(self._xml_filepath)
        self._xml_filepath = path.join(self._xml_filepath, self.xml_filename)
