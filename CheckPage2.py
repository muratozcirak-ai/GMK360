import urllib.request
import ssl

context = ssl._create_unverified_context()
try:
    response = urllib.request.urlopen("http://localhost:5248/Agenda/Create", context=context)
    print("SUCCESS")
    print(response.read().decode('utf-8')[:500])
except urllib.error.HTTPError as e:
    print(f"HTTP ERROR {e.code}")
    print(e.read().decode('utf-8'))
except Exception as e:
    print(f"ERROR: {e}")
