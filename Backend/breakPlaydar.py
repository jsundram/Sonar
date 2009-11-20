from httplib import HTTPConnection

conn = HTTPConnection("localhost",60210)

params = None
headers = {"Range": "bytes=6960000-"}
sid="9A5BDE20-9D5D-499C-AEBF-AD4E611A4B00"

conn.request("GET", "/sid/" + sid, params, headers)
ret = conn.getresponse()
