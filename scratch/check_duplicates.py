import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

count = content.count('Alınan Fiyat Teklifleri (Fizibiliteye Eklenebilir)')
print(f"Count of 'Alınan Fiyat Teklifleri': {count}")

# Print line numbers where it appears
lines = content.split('\n')
for i, line in enumerate(lines):
    if 'Alınan Fiyat Teklifleri' in line:
        print(f"Line {i}: {line.strip()[:100]}")