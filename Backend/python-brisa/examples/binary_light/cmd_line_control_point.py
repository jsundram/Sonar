# Simple UPnP control point using BRisa framework
# Features:
# - Searching for devices
# - Listing devices
# - Handling events (new device located, removed device)

from brisa.core.reactors import install_default_reactor
reactor = install_default_reactor()

from brisa.core.threaded_call import run_async_function

from brisa.upnp.control_point.control_point import ControlPoint


service = ('u', 'urn:schemas-upnp-org:service:SwitchPower:1')
binary_light_type = 'urn:schemas-upnp-org:device:BinaryLight:1'


def on_new_device(dev):
    """ Callback triggered when a new device is found.
    """
    print 'Got new device:', dev.udn
    print "Type 'list' to see the whole list"

    if not dev:
        return


def on_removed_device(udn):
    """ Callback triggered when a device leaves the network.
    """
    print 'Device is gone:', udn


def get_switch_service(device):
    return device.services[service[1]]


def create_control_point():
    """ Creates the control point and binds callbacks to device events.
    """
    c = ControlPoint()
    c.subscribe('new_device_event', on_new_device)
    c.subscribe('removed_device_event', on_removed_device)
    return c


def main():
    """ Main loop iteration receiving input commands.
    """
    c = create_control_point()
    c.start()
    run_async_function(_handle_cmds, (c, ))
    reactor.add_after_stop_func(c.stop)
    reactor.main()


def _exit(c):
    """ Stops the _handle_cmds loop
    """
    global running_handle_cmds
    running_handle_cmds = False
    

def _help(c):
    """ Prints the available commands that are used in '_handle_cmds' method.
    """
    print 'Available commands: '
    for x in ['exit', 'help', 'search', 'set_light <dev number>',
              'get_status', 'get_target', 'turn <on/off>', 'stop',
              'list']:
        print '\t%s' % x
    
    
def _search(c):
    """ Start searching for devices of type upnp:rootdevice and repeat
    search every 600 seconds (UPnP default)
    """
    c.start_search(600, 'upnp:rootdevice')


def _set_light(c, command):
    """ Gets the binary device by the number that is passed as parameter
    """
    try:
        devices = c.get_devices().values()
        c.current_server = devices[int(command)]
        if c.current_server and \
                    c.current_server.device_type != binary_light_type:
            print 'Please choose a BinaryLight device.'
            c.current_server = None
    except:
        print 'BinaryLight number not found. Please run list and '\
              'check again'
        c.current_server = None


def _get_status(c):
    """ Gets the binary light status and print if it's on or off.
    """
    try:
        service = get_switch_service(c.current_server)
        status_response = service.GetStatus()
        if status_response['ResultStatus'] == '1':
            print 'Binary light status is on'
        else:
            print 'Binary light status is off'
    except Exception, e:
        if not hasattr(c, 'current_server') or not c.current_server:
            print 'BinaryLight device not set.Please use set_light <n>'
        else:
            print 'Error in get_status():', e


def _get_target(c):
    """ Gets the binary light target and print if it's on or off.
    """
    try:
        service = get_switch_service(c.current_server)
        status_response = service.GetTarget()
        if status_response['RetTargetValue'] == '1':
            print 'Binary light target is on'
        else:
            print 'Binary light target is off'
    except Exception, e:
        if not hasattr(c, 'current_server') or not c.current_server:
            print 'BinaryLight device not set.Please use set_light <n>'
        else:
            print 'Error in get_target():', e


def _turn(c, command):
    """ Turns the binary device on or off
    """
    try:
        cmd = {'on': '1', 'off': '0'}.get(command, '')
        if not cmd:
            print 'Wrong usage. Please try turn on or turn off.'
        service = get_switch_service(c.current_server)
        service.SetTarget(NewTargetValue=cmd)
        print 'Turning binary light', command
    except Exception, e:
        if not hasattr(c, 'current_server') or not c.current_server:
            print 'BinaryLight device not set.Please use set_light <n>'
        else:
            print 'Error in set_status():', e


def _stop(c):
    """ Stop searching
    """
    c.stop_search()


def _list_devices(c):
    """ Lists the devices that are in network.
    """
    k = 0
    for d in c.get_devices().values():
        print 'Device no.:', k
        print 'UDN:', d.udn
        print 'Name:', d.friendly_name
        print 'Device type:', d.device_type
        print 'Services:', d.services.keys() # Only print services name
        print 'Embedded devices:', [dev.friendly_name for dev in \
             d.devices.values()] # Only print embedded devices names
        print
        k += 1


# Control the loop at _handle_cmds function
running_handle_cmds = True
commands = {'exit': _exit, 
            'help': _help,
            'search': _search,
            'stop': _stop,
            'list': _list_devices,
            'turn': _turn,
            'set_light': _set_light,
            'get_status': _get_status,
            'get_target': _get_target}


def _handle_cmds(c):
    while running_handle_cmds:
        try:
            input = raw_input('>>> ').strip()
            if len(input.split(" ")) > 0:
                try:
                    if len(input.split(" ")) > 1:
                        commands[input.split(" ")[0]](c, input.split(" ")[1])
                    else:
                        commands[input.split(" ")[0]](c)
                except KeyError, IndexError:
                    print 'Invalid command, try help'
                except TypeError:
                    print 'Wrong usage, try help to see'
        except KeyboardInterrupt, EOFError:
            c.stop()
            break

    # Stops the main loop
    reactor.main_quit()


if __name__ == '__main__':
    main()
