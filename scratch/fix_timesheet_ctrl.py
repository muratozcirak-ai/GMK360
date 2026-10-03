import codecs
import re

path = 'GMK360.Web/Controllers/DailyTimesheetsController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('"ProjectName"', '"Name"')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Fixed ProjectName to Name in SelectList.')