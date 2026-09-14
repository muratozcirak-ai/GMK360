import urllib.request
import ssl

context = ssl._create_unverified_context()
req = urllib.request.Request("http://localhost:5248/Agenda/Create", method="GET")
try:
    response = urllib.request.urlopen(req, context=context)
    print(response.getcode())
except urllib.error.HTTPError as e:
    print(f"HTTP ERROR {e.code}")
    print(e.headers)
