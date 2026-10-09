import re
with open(r'GMK360.Web\Views\Modules\Insaat.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Make the hero background icon smaller so it doesn't force height
content = content.replace('font-size: 14rem;', 'font-size: 10rem;')

# General spacing reduction
content = content.replace('bg-navy py-5 position-relative', 'bg-navy py-4 position-relative')

with open(r'GMK360.Web\Views\Modules\Insaat.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
