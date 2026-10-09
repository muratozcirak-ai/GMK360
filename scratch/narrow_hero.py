import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

content = content.replace('min-height: 400px;', 'min-height: 320px;')
content = content.replace('<div class="py-4" style="background: url(', '<div class="py-3" style="background: url(')

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
