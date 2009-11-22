# Licensed under the MIT license
# http://opensource.org/licenses/mit-license.php or see LICENSE file.
# Copyright 2007-2008 Brisa Team <brisa-develop@garage.maemo.org>

import os

from unittest import TestCase, TestSuite, TextTestRunner
from tempfile import mkstemp

from brisa.core.config import ConfigurationManager


class ConfigurationManagerTest(TestCase):

    test_section_name = 'test_section'
    test_parameter_name = 'test_parameter'
    test_parameter_value = 'test_value'
    test_parameter_value1 = 'test_value1'
    default_config = {test_section_name:
                        {test_parameter_name: test_parameter_value}}
    manager = None

    def getTestParameterValue(self):
        return self.manager.get_parameter(self.test_section_name,
                                          self.test_parameter_name)

    def runTest(self):
        """ Tests the ConfigurationManager state and direct access features.
        """
        self.testConstruction()
        self.testAccessChange()
        self.testTargets()

    def testConstruction(self):
        """ Test if the ConfigurationManager construction is valid.
        """
        # Test if State feature is activated by default
        self.failUnless(self.manager.get_direct_access() == False,
                          'State feature not activated by default')
        self.manager.save()

    def testAccessChange(self):
        """ Simple tests for changes in the direct access flag.
        """
        for b in [False, True, False]:
            self.manager.set_direct_access(b)
            self.assertEquals(self.manager.get_direct_access(), b)

    def testTargets(self):
        """ Tests gets and sets applied on the state or on the persistence.

        These tests assures the operation is applied on the storage selected
        (state or persistence).
        """
        # Store on the persistence: value1
        self.manager.set_direct_access(True)
        self.manager.set_parameter(self.test_section_name,
                                   self.test_parameter_name,
                                   self.test_parameter_value1)

        # Store on the state: value
        self.manager.set_direct_access(False)
        self.manager.set_parameter(self.test_section_name,
                                   self.test_parameter_name,
                                   self.test_parameter_value)

        # Check state = value
        self.assertEquals(self.getTestParameterValue(),
                          self.test_parameter_value)

        # Check persistence = value1
        self.manager.set_direct_access(True)
        self.assertEquals(self.getTestParameterValue(),
                          self.test_parameter_value1)


class FilePersistenceTest(ConfigurationManagerTest):

    def setUp(self):
        self.tmp_file = mkstemp()[1]
        self.manager = ConfigurationManager(self.tmp_file, self.default_config)

    def tearDown(self):
        os.remove(self.tmp_file)


def config_test_suite():
    return TestSuite([FilePersistenceTest()])


if __name__ == "__main__":
    TextTestRunner(verbosity=2).run(config_test_suite())
