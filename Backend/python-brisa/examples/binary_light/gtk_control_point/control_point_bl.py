from brisa.upnp.control_point.control_point import ControlPoint


class ControlPointBL(ControlPoint):
    sp_ns = ('u', 'urn:schemas-upnp-org:service:SwitchPower:1')
    current_server = None


    def get_switch_service(self, device):
        return device.services[self.sp_ns[1]]


    def set_target(self, target):
        try:
            cmd = {True: '1', False: '0'}.get(target, '')
            service = self.get_switch_service(self.current_server)
            service.SetTarget(NewTargetValue=cmd)
        except Exception, e:
            if not self.current_server:
                raise Exception('BinaryLight Server not selected. Select it' \
                                'setting current_server to the device you'\
                                'want.')
            else:
                raise Exception('Error trying to retrieve information: %s' %\
                                e.message)


    def get_target(self):
        try:
            service = self.get_switch_service(self.current_server)
            status_response = service.GetTarget()
            return True if status_response['RetTargetValue'] == '1' else False
        except Exception, e:
            if not self.current_server:
                raise Exception('BinaryLight Server not selected. Select it' \
                                'setting current_server to the device you'\
                                'want.')
            else:
                raise Exception('Error during renew: %s' \
                                % e.message)


    def get_status(self):
        try:
            service = self.get_switch_service(self.current_server)
            status_response = service.GetStatus()
            return True if status_response['ResultStatus'] == '1' else False
        except Exception, e:
            if not self.current_server:
                raise Exception('BinaryLight Server not selected. Select it' \
                                'setting current_server to the device you' \
                                'want.')
            else:
                raise Exception('Error during renew: %s' % e.message)
