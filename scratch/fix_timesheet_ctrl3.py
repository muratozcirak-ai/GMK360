import codecs
import re

path = 'GMK360.Web/Controllers/DailyTimesheetsController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('t.ProjectId == id', 't.ConstructionProjectId == id')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)