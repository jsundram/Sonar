import pkgutil

#print 'develop __path__ before:'
#print __path__
#print

#print 'develop __name__:'
#print __name__
#print

# Ensure we can also reach modules in the system package
__path__ = pkgutil.extend_path(__path__, __name__)
#__path__.append('/usr/lib/python2.6/site-packages/brisa')

#print 'develop __path__ after append:'
#print __path__
#print

######################################################################
import brisa
######################################################################

# Make this directory come last when importing from this package
__path__.reverse()

#print 'develop __path__ after reverse:'
#print __path__
#print

# Make this package gain all the attributes of the system package
_path_prev = __path__
import __init__

#print '__init__ file:'
#print __init__.__file__
#print

globals().update(vars(__init__))
__path__ = _path_prev
del _path_prev

# Make this directory come first when importing from this package
__path__.reverse()

#print 'develop __path__ after:'
#print __path__
#print


