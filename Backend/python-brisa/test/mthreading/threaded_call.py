# Licensed under the MIT license
# http://opensource.org/licenses/mit-license.php or see LICENSE file.
# Copyright 2007-2008 Brisa Team <brisa-develop@garage.maemo.org>

import time
import unittest

from brisa.core.threaded_call import *


class ThreadedCallTestCase(unittest.TestCase):

    def setUp(self):
        test_function = lambda: True
        self.t1 = ThreadedCall(test_function, delay=4)
        self.t2 = ThreadedCall(test_function, delay=4)
        self.t3 = ThreadedCall(test_function, delay=4)

    def tearDown(self):
        del self.t1, self.t2, self.t3

    def runTest(self):
        self.t1.start()
        self.t2.start()
        self.t3.start()

        self.assertEquals(self.t1.result, None)
        self.assertEquals(self.t2.result, None)
        self.assertEquals(self.t3.result, None)

        time.sleep(1)
        self.t2.stop()
        self.t3.stop()
        time.sleep(5)

        self.assertEquals(self.t1.result, True)
        self.assertEquals(self.t2.result, None)
        self.assertEquals(self.t3.result, None)


def threaded_call_test_suite():
    tests = [ThreadedCallTestCase()]
    suite = unittest.TestSuite(tests)
    return suite
