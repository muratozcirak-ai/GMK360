import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Replace the mangled onclick
target = "onclick=\"alert(\\'Geliştirme aşamasında. Yakında C# backend ile bağlanacaktır.\\'); var m = bootstrap.Modal.getInstance(document.getElementById(\\'mergeBlocksModal\\')); if(m) m.hide();\""
# Actually let's use regex again to be safe
pattern = re.compile(r'onclick="alert[^"]+"')
match = pattern.search(content)

if match:
    new_onclick = "onclick=\"alert('Geliştirme aşamasında. Yakında C# backend ile bağlanacaktır.'); var m = bootstrap.Modal.getInstance(document.getElementById('mergeBlocksModal')); if(m) m.hide();\""
    content = content[:match.start()] + new_onclick + content[match.end():]
    
with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("Button JS quotes fixed!")
