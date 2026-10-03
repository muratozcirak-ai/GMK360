import codecs
import re

path = 'GMK360.Web/Controllers/PhaseTwoController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'\[HttpPost\("PhaseTwo/SelectStrategy"\)\].*?\[HttpPost\("PhaseTwo/UpdatePrice"\)\]'
replacement = '''[HttpPost("PhaseTwo/UpdatePrice")]'''
content = re.sub(target, replacement, content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)