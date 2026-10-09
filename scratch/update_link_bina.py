import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# I will replace the 2nd card's link
content = re.sub(r'(<!-- 2\. Bina, Site & AVM Yönetimi -->.*?href=")(".*?class="text-decoration-none fw-bold text-info mt-auto d-inline-block stretched-link">Sistemi Keşfet)', r'\1/Modules/Bina" target="_blank\2', content, flags=re.DOTALL)

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
