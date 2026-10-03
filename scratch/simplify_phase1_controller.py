import codecs
import re

path = 'GMK360.Web/Controllers/PhaseOneController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'\[HttpPost\("PhaseOne/SelectStrategy"\)\].*?\[HttpPost\("PhaseOne/UpdatePrice"\)\]'
replacement = '''[HttpPost("PhaseOne/UpdatePrice")]'''
content = re.sub(target, replacement, content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)