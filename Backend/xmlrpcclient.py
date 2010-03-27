import xmlrpclib

s = xmlrpclib.ServerProxy('http://192.168.0.113:8000')

from time import sleep

# Print list of available methods
print s.system.listMethods()

hhids = s.SearchForHHIDs()
print hhids
print s.SubscribeToHH(hhids[0])
sleep(1)
zgids = s.GetAllZoneGroups()
print zgids
print s.EnqueueTrack(zgids[0],
                     {"Uri": "http://10.0.0.193:60210/sid/track.mp3?sid=809C275B-7738-4E76-8BED-8F01C4701721",
                      "Artist" : "Pink Floyd", "Album": "The Wall",
                      "Title": "Another Brick In The Wall (Part 2)",
                      "PlayTime": 242})

s.Play(zgids[0], -1)
