import urllib.request
import time

url = 'http://localhost:5248/Seed/ImportSqlData'
success = False

for i in range(10):
    try:
        req = urllib.request.urlopen(url, timeout=30)
        print("Success: ", req.read().decode('utf-8'))
        success = True
        break
    except Exception as e:
        print(f"Attempt {i+1} failed: {e}")
        time.sleep(5)

if not success:
    print("Failed to reach the server.")
