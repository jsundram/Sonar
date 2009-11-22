# Licensed under the MIT license
# http://opensource.org/licenses/mit-license.php or see LICENSE file.
# Copyright 2007-2008 Brisa Team <brisa-develop@garage.maemo.org>

from brisa.core.reactors import install_default_reactor
reactor = install_default_reactor()

from unittest import TestSuite, TextTestRunner

from config import *
from mthreading import *


def all_tests():
    """ Returns a suite containing all tests.
    """
    suite = TestSuite([config_test_suite(), threaded_call_test_suite()])
    return suite


# This is the core Brisa's unit tests. Feel free to modify the __main__
# below for your tests with specific modules/test suites.

if __name__ == '__main__':
    TextTestRunner(verbosity=2).run(all_tests())
