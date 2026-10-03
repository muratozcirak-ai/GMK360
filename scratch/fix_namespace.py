import codecs
import re

path = 'GMK360.Web/Controllers/PhaseOneController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'using GMK360\.Data\.Context;'
replacement = 'using GMK360.Data.Contexts;'
content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)