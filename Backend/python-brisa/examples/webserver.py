# Licensed under the MIT license
# http://opensource.org/licenses/mit-license.php or see LICENSE file.
# Copyright 2007-2008 Brisa Team <brisa-develop@garage.maemo.org>

from brisa.core.reactors import install_default_reactor
reactor = install_default_reactor()

import os
import cherrypy

from brisa.core.webserver import WebServer, StaticFile, CustomResource


# Setup webserver
webserver = WebServer()


# Write and serve a sample file
f = open('/tmp/hello_world', 'w')
f.write('Hello World!')
f.close()

webserver.add_static_file(StaticFile('hello_world', '/tmp/hello_world'))


# Serve a resource


class Greeter(CustomResource):

    def get_render(self, uri, params):
        return self

    def say_hello(self, name):
        return 'Hello %s!' % name

    def render(self, uri, request, response):
        params = cherrypy.request.params

        if 'name' in params:
            # http://addr:port/Greet?name=Someone
            return self.say_hello(params['name'])
        else:
            return 'Hello!'

webserver.add_resource(Greeter('Greet'))


# Start the webserver
webserver.start()

print 'Webserver listening on', webserver.get_listen_url()
print 'File URL: %s/hello_world' % webserver.get_listen_url()
print 'Res URL: %s/Greet' % webserver.get_listen_url()
print 'Res test URL: %s/Greet?name=you' % webserver.get_listen_url()


# Block so that the program doesn't quit

reactor.add_after_stop_func(webserver.stop)
reactor.main()

os.remove('/tmp/hello_world')
