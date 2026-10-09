import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

content = content.replace('</div>\n\n        </div>\n    </div>\n</div>\n\n<!-- DETAYLI MOD', '</div>\n\n<!-- DETAYLI MOD')

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
