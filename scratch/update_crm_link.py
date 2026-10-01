import codecs
import re

path = 'GMK360.Web/Views/AdminCRM/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# We replace the href
content = re.sub(r'href="/AdminCRM/Detail/([^"]+)"', r'href="/AdminCRM/UserDetails/\1"', content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print("Updated Index.cshtml to point to UserDetails")