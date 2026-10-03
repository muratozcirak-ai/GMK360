import codecs
import re

path = 'GMK360.Web/wwwroot/css/site.css'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Remove the rogue .badge rule
rogue_rule = '''
.badge {
    position: absolute;
    top: 1rem;
    left: 1rem;
}'''
content = content.replace(rogue_rule, '')

# Or if it's slightly different:
content = re.sub(r'\.badge\s*\{[^}]*position:\s*absolute;[^}]*\}', '', content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Removed absolute badge CSS.')