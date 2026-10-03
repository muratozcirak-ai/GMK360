import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

lines = content.split('\n')
for i in range(315, 360):
    if i < len(lines):
        print(f"{i}: {lines[i]}")