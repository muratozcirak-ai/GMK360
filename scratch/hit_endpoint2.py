import urllib.request
import ssl

req = urllib.request.Request('http://localhost:5248/PhaseOne/ForceSeedRemainingPhases')
try:
    response = urllib.request.urlopen(req)
    print('Response:', response.read().decode('utf-8'))
except Exception as e:
    print('Error:', e)