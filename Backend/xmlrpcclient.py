import xmlrpclib

s = xmlrpclib.ServerProxy('http://192.168.2.7:8000')

# Print list of available methods
print s.system.listMethods()

print s.EnqueueTrack("ZGID_2", {"URI":"bar"})

while (1):
    print s.PollForEvents(5)

    
