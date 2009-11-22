# Licensed under the MIT license
# http://opensource.org/licenses/mit-license.php or see LICENSE file.
# Copyright 2007-2008 Brisa Team <brisa-develop@garage.maemo.org>

from distutils.core import setup


long_description = '''
BRisa is an UPnP framework written in Python. It provides facilities for
creating UPnP devices and control points. The Python-BRisa API comprehends
internet messaging protocols and basic services (TCP, UDP, HTTP, SOAP, SSDP),
networking facilities, threading management, logging, configurations, web
server and more. Python-BRisa's runs on the maemo plataform, Linux and on Mac
with a few tweaks. We would really appreciate community feedback, bug reports
and feature requests concerning attempts to run it on other systems.
'''
version = '0.10.1'


def main():
    setup(
          name='python-brisa',
          version=version,
          description='UPnP framework',
          long_description=long_description,
          author='BRisa Team',
          author_email='brisa-develop@garage.maemo.org',
          url='https://garage.maemo.org/projects/brisa/',
          download_url='https://garage.maemo.org/projects/brisa/',
          license='MIT',
          maintainer='Leandro Melo de Sales (leandroal)',
          maintainer_email='leandro@embedded.ufcg.edu.br',
          platforms='any',
          keywords=['UPnP', 'Control Point', 'DLNA', 'Maemo', 'Device',
                    'Service'],
          scripts=['bin/brisa-conf'],
          packages=['brisa',
                    'brisa/core',
                    'brisa/core/reactors',
                    'brisa/upnp',
                    'brisa/upnp/didl',
                    'brisa/upnp/control_point',
                    'brisa/upnp/device',
                    'brisa/upnp/services',
                    'brisa/upnp/services/cds',
                    'brisa/upnp/services/connmgr',
                    'brisa/upnp/services/xmls',
                    'brisa/utils'],
          package_data={'brisa/upnp/services/xmls': ['*.xml']},
          classifiers=['Development Status :: 4 - Beta',
                       'Environment :: Other Environment',
                       'Intended Audience :: Developers',
                       'Intended Audience :: End Users/Desktop',
                       'License :: OSI Approved :: MIT License',
                       'Natural Language :: English',
                       'Operating System :: POSIX :: Linux',
                       'Programming Language :: Python',
                       'Topic :: Multimedia'])


if __name__ == '__main__':
    main()
