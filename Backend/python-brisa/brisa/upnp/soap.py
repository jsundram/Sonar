# Licensed under the MIT license
# http://opensource.org/licenses/mit-license.php or see LICENSE file.
# Copyright 2007-2008 Brisa Team <brisa-develop@garage.maemo.org>
#
# Copyright 2001 - Cayce Ullman <http://pywebsvcs.sourceforge.net>
# Copyright 2001 - Brian Matthews <http://pywebsvcs.sourceforge.net>
# Copyright 2001-2003 - Pfizer <http://pywebsvcs.sourceforge.net>
# Copyright 2007-2008 - Frank Scholz <coherence@beebits.net>

""" Parses and builds SOAP calls transparently.
"""

import httplib
import exceptions

from xml.etree import ElementTree

from brisa.core.network import parse_xml, parse_url
from brisa.upnp.upnp_defaults import map_upnp_value, map_upnp_type


# SOAP constants


NS_SOAP_ENV = "{http://schemas.xmlsoap.org/soap/envelope/}"
NS_SOAP_ENC = "{http://schemas.xmlsoap.org/soap/encoding/}"
NS_XSI = "{http://www.w3.org/1999/XMLSchema-instance}"
NS_XSD = "{http://www.w3.org/1999/XMLSchema}"

SOAP_ENCODING = "http://schemas.xmlsoap.org/soap/encoding/"

UPNPERRORS = {401: 'Invalid Action',
              402: 'Invalid Args',
              501: 'Action Failed',
              600: 'Argument Value Invalid',
              601: 'Argument Value Out of Range',
              602: 'Optional Action Not Implemented',
              603: 'Out Of Memory',
              604: 'Human Intervention Required',
              605: 'String Argument Too Long',
              606: 'Action Not Authorized',
              607: 'Signature Failure',
              608: 'Signature Missing',
              609: 'Not Encrypted',
              610: 'Invalid Sequence',
              611: 'Invalid Control URL',
              612: 'No Such Session', }


def build_soap_error(status, description='without words'):
    """ Builds an UPnP SOAP error message.

    @param status: error code
    @param description: error default description

    @type status: integer
    @type description: string

    @return: soap call representing the error
    @rtype: string
    """
    root = ElementTree.Element('s:Fault')
    ElementTree.SubElement(root, 'faultcode').text = 's:Client'
    ElementTree.SubElement(root, 'faultstring').text = 'UPnPError'
    e = ElementTree.SubElement(root, 'detail')
    e = ElementTree.SubElement(e, 'UPnPError')
    e.attrib['xmlns'] = 'urn:schemas-upnp-org:control-1-0'
    ElementTree.SubElement(e, 'errorCode').text = str(status)
    ElementTree.SubElement(e, 'errorDescription').text = UPNPERRORS.get(status,
                                                                   description)

    return build_soap_call(None, root, encoding=None)


def build_soap_call(method, arguments, encoding=SOAP_ENCODING,
                    envelope_attrib=None, typed=None):
    """ Builds a soap call.

    @param method: method for the soap call. If set to None, the method element
    will be omitted and arguments will be added directly to the body (error
    message)
    @param arguments: arguments for the call
    @param encoding: encoding for the call
    @param envelope_attrib: envelope attribute
    @param typed: True if typed

    @type method: string or None
    @type arguments: dict or ElementTree.Element
    @type encoding: string
    @type envelope_attrib: list
    @type typed: boolean or None

    @return: soap call
    @rtype: string
    """
    envelope = ElementTree.Element("s:Envelope")
    if envelope_attrib:
        for n in envelope_attrib:
            envelope.attrib.update({n[0]: n[1]})
    else:
        envelope.attrib.update({'s:encodingStyle':
                                "http://schemas.xmlsoap.org/soap/encoding/"})
        envelope.attrib.update({'xmlns:s':
                                "http://schemas.xmlsoap.org/soap/envelope/"})

    body = ElementTree.SubElement(envelope, "s:Body")

    if method:
        re = ElementTree.SubElement(body, method)
        if encoding:
            re.set("%sencodingStyle" % NS_SOAP_ENV, encoding)
    else:
        re = body

    # append the arguments
    if isinstance(arguments, dict):

        for arg_name, arg_val in arguments.iteritems():
            arg_type = map_upnp_type(arg_val)
            arg_val = map_upnp_value(arg_val)

            e = ElementTree.SubElement(re, arg_name)
            if typed and arg_type:
                if not isinstance(type, ElementTree.QName):
                    arg_type = ElementTree.QName(
                                "http://www.w3.org/1999/XMLSchema", arg_type)
                e.set('%stype' % NS_XSI, arg_type)
            e.text = arg_val
    else:
        re.append(arguments)

    preamble = """<?xml version="1.0" encoding="utf-8"?>"""
    return '%s%s' % (preamble, ElementTree.tostring(envelope, 'utf-8'))


def __decode_result(element):
    """ Decodes the result out of an Element. Returns the text, if possible.

    @param element: element to decode the result
    @type element Element

    @return: text of the result
    @rtype: string
    """
    type = element.get('{http://www.w3.org/1999/XMLSchema-instance}type')
    if type is not None:
        try:
            prefix, local = type.split(":")
            if prefix == 'xsd':
                type = local
        except ValueError:
            pass

    if type == "integer" or type == "int":
        return int(element.text)
    if type == "float" or type == "double":
        return float(element.text)
    if type == "boolean":
        return element.text == "true"

    return element.text or ""


def parse_soap_call(data):
    """ Parses a soap call and returns a 4-tuple.

    @param data: raw soap XML call data
    @type data: string

    @return: 4-tuple (method_name, args, kwargs, namespace)
    @rtype: tuple
    """
    tree = parse_xml(data)
    body = tree.find('{http://schemas.xmlsoap.org/soap/envelope/}Body')
    method = body.getchildren()[0]
    method_name = method.tag
    ns = None

    if method_name.startswith('{') and method_name.rfind('}') > 1:
        ns, method_name = method_name[1:].split('}')

    args = []
    kwargs = {}
    for child in method.getchildren():
        kwargs[child.tag] = __decode_result(child)
        args.append(kwargs[child.tag])

    return method_name, args, kwargs, ns


class SOAPProxy(object):
    """ Proxy for making remote SOAP calls Based on twisted.web.soap.Proxy
    and SOAPpy.
    """

    def __init__(self, url, namespace=None):
        """ Constructor for the SOAPProxy class.

        @param url: remote SOAP server
        @param namespace: calls namespace

        @type url: string
        @type namespace: tuple
        """
        self.url = url
        self.namespace = namespace

    def call_remote(self, soapmethod, **kwargs):
        """ Performs a remote SOAP call.

        @param soapmethod: method to be called
        @param kwargs: args to be passed, can be named.

        @type soapmethod: string
        @type kwargs: dictionary

        @return: the result text of the soap call.
        @rtype: string
        """
        ns = self.namespace
        soapaction = '%s#%s' % (ns[1], soapmethod)
        payload = build_soap_call('{%s}%s' % (ns[1], soapmethod),
                                  kwargs, encoding=None)
        result = HTTPTransport().call(self.url, payload, ns,
                                      soapaction=soapaction, encoding='utf-8')
        _, _, res, _ = parse_soap_call(result)
        return res


class HTTPTransport(object):
    """ Wrapper class for a HTTP SOAP call. It contain the call() method that
    can perform calls and return the response payload.
    """

    def call(self, addr, data, namespace, soapaction=None, encoding=None):
        """ Builds and performs an HTTP request. Returns the response payload.

        @param addr: address to receive the request in the form
        schema://hostname:port
        @param data: data to be sent
        @param soapaction: soap action to be called
        @param encoding: encoding for the message

        @type addr: string
        @type data: string
        @type soapaction: string
        @type encoding: string

        @return: response payload
        @rtype: string
        """
        # Build a request
        addr = parse_url(addr)
        real_addr = '%s:%d' % (addr.hostname, addr.port)
        real_path = addr.path
        if addr.query != '':
            real_path += '?%s' % addr.query

        if addr.scheme == 'https':
            r = httplib.HTTPS(real_addr)
        else:
            r = httplib.HTTP(real_addr)

        r.putrequest("POST", real_path)
        r.putheader("Host", addr.hostname)
        r.putheader("User-agent", 'BRISA SERVER')
        t = 'text/xml'
        if encoding:
            t += '; charset="%s"' % encoding
        r.putheader("Content-type", t)
        r.putheader("Content-length", str(len(data)))

        # if user is not a user:passwd format
        if addr.username != None:
            val = base64.encodestring(addr.user)
            r.putheader('Authorization', 'Basic ' + val.replace('\012', ''))

        # This fixes sending either "" or "None"
        if soapaction:
            r.putheader("SOAPAction", '"%s"' % soapaction)
        else:
            r.putheader("SOAPAction", "")

        r.endheaders()
        r.send(data)

        #read response line
        code, msg, headers = r.getreply()

        content_type = headers.get("content-type", "text/xml")
        content_length = headers.get("Content-length")
        if content_length == None:
            data = r.getfile().read()
            message_len = len(data)
        else:
            message_len = int(content_length)
            data = r.getfile().read(message_len)

        def startswith(string, val):
            return string[0:len(val)] == val


        if code == 500 and not \
               (startswith(content_type, "text/xml") and message_len > 0):
            raise HTTPError(code, msg)

        if code not in (200, 500):
            raise HTTPError(code, msg)

        #return response payload
        return data.decode('utf-8')


class HTTPError(exceptions.Exception):
    """ Represents an error of a HTTP request.
    """

    def __init__(self, code, msg):
        """ Constructor for the HTTPError class.

        @param code: error code
        @param msg: error message

        @type code: string
        @type msg: string
        """
        self.code = code
        self.msg = msg

    def __repr__(self):
        return "<HTTPError %s %s>" % (self.code, self.msg)

    def __call___(self):
        return (self.code, self.msg, )
