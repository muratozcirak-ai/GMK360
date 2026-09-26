import sys

# Read file as utf-8
with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    text = f.read()

# See if we can fix a known string
# Let's find the word 'Çevre Özellikleri' which is currently corrupted
import re
match = re.search(r'Konum Verileri ve (.*?)zellikleri', text)
if match:
    corrupted = match.group(1)
    print("Corrupted:", repr(corrupted))
    try:
        fixed = corrupted.encode('cp1252').decode('utf-8')
        print("Fixed with cp1252:", fixed)
    except Exception as e:
        print("Error cp1252:", e)
        
    try:
        fixed = corrupted.encode('latin1').decode('utf-8')
        print("Fixed with latin1:", fixed)
    except Exception as e:
        print("Error latin1:", e)
