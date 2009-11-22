# Licensed under the MIT license
# http://opensource.org/licenses/mit-license.php or see LICENSE file.
# Copyright 2007-2008 Brisa Team <brisa-develop@garage.maemo.org>

__all__ = ('MyService', 'MyServiceWithScpd')

import os.path
from os.path import abspath, dirname
from brisa.upnp.device.service import Service
from brisa.upnp.device.action import Action, Argument
from brisa.upnp.device.service import StateVariable

service_name = 'MyService'
service_type = 'urn:schemas-upnp-org:service:MyService:1'


def MyMethod(*args, **kwargs):
    # Pay attention to the case sensitive arguments used here
    # and in the xml file you create for the service
    inArg = kwargs['TextIn']
    return {'TextOut': inArg + "Out!!"}


class MyService(Service):
 
    def __init__(self):
        Service.__init__(self, service_name, service_type, '')

        varIn = StateVariable(self, "A_ARG_TYPE_Textin",
                              True, False, "string")
        varIn.subscribe_for_update(self.varUpdateCallback)
        varOut = StateVariable(self, "A_ARG_TYPE_Textout",
                               False, False, "string")
        varOut.subscribe_for_update(self.varUpdateCallback)
        self.add_state_variable(varIn)
        self.add_state_variable(varOut)

        argIn = Argument("TextIn", Argument.IN, varIn)
        argOut = Argument("TextOut", Argument.OUT, varOut)

        actionMyMethod = Action(self, "MyMethod", [argIn, argOut])
        actionMyMethod.run_function = MyMethod
        self.add_action(actionMyMethod)

    def varUpdateCallback(self, name, value):
        print name, 'was updated to', value


# This is just an example. It is not being used.
# Creating the same service with a scpd.xml.
class MyServiceWithScpd(Service):

    def __init__(self):
        Service.__init__(self, service_name, service_type, '',
                         os.path.join(abspath(dirname(__file__)),
                        'myservice-scpd.xml'))

    def soap_MyMethod(self, *args, **kwargs):
        return MyMethod(*args, **kwargs)
