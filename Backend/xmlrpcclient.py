import xmlrpclib

s = xmlrpclib.ServerProxy('http://192.168.2.7:8000')

# Print list of available methods
print s.system.listMethods()

print s.EnqueueTrack("foo", "bar")
