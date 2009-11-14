from SimpleXMLRPCServer import SimpleXMLRPCServer
from SimpleXMLRPCServer import SimpleXMLRPCRequestHandler

# Restrict to a particular path.
class RequestHandler(SimpleXMLRPCRequestHandler):
    rpc_paths = ('/RPC2',)

# Create server
server = SimpleXMLRPCServer(("192.168.2.7", 8000),
                            requestHandler=RequestHandler)
server.register_introspection_functions()

def EnqueueTrack(szTrackURI, dictMD):
    print "Enqueued track at:", szTrackURI, "with MD:", dictMD
    return "Hey Jason, this is way groovy, no?"

# Register pow() function; this will use the value of
# pow.__name__ as the name, which is just 'pow'.
server.register_function(EnqueueTrack)

# Run the server's main loop
server.serve_forever()
