# Licensed under the MIT license
# http://opensource.org/licenses/mit-license.php or see LICENSE file.
# Copyright 2007-2008 Brisa Team <brisa-develop@garage.maemo.org>
#
# Simple example of ThreadManager's uses

from time import sleep, time
import sys

from brisa.threading import ThreadManager


def print_out(msg):
    print msg


def multi_param(a, b, c, d):
    print 'Got a=%d, b=%d, c=%s, d=%s' % (a, b, c, d)


def parallel():
    # After two seconds we will stop the main loop.
    ThreadManager().stop_main_loop()
    print 'Main loop quited'


if __name__ == "__main__":
    print "BRisa's ThreadManager example\n"

    # ThreadManager is a singleton, so you may
    # retrieve its reference as showed below:
    manager = ThreadManager()

    # We can run functions without blocking this
    # main loop using the method run_async_function.
    #
    # In this simple example we pass the function,
    # a tuple containing the parameters for the
    # function and a delay time. After the time has
    # passed, the function will be called with the
    # parameters specified.
    #
    # Note: if your function doesn't receive any
    # parameters, or you don't want a delay to
    # happen, then you can pass just the function.
    print 'First part expected results'
    print 'Expected: Got a=42 b=0 c=c d=d'
    print 'Expected: mymessage'
    print '\nResults'
    manager.run_async_function(print_out, ('mymessage', ), 1.5)
    manager.run_async_function(multi_param, (42, 0, 'c', 'd'))

    # Sleeping for two seconds not to mess with
    # the second part of the example.
    sleep(2)


    # Now we will schedule a function call for
    # 3 seconds from now, and run our blocking
    # main loop. That function will unblock
    # the main loop.

    print '\nScheduling stop in 3 seconds'
    manager.run_async_function(parallel, delay=3.0)

    # The idea of this part is that you can
    # stop the main loop from anywhere.

    # Also you can block anytime. We're also
    # doing here some time measurements.
    before = time()
    print 'Starting main loop'
    manager.main_loop()
    after = time()

    print '\nTime spent in main loop: %.1f / 3.0' % (after-before)

    # Note: stopping the main loop causes
    # ThreadManager to stop all his children
    # threads. This includes all ThreadObjects
    # but does *NOT* include these functions
    # started with run_async_function.

    sys.exit(0)
