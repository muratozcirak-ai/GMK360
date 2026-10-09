import re

with open(r'GMK360.Web\Views\Shared\_ConstructionLayout.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Remove the Hub button from top bar
hub_pattern = r'<!-- Tüm Uygulamalar \(Hub\) Butonu -->\s*<a href="/Dashboard/Hub" class="btn btn-dark.*?</a>\s*'

content = re.sub(hub_pattern, '', content, flags=re.DOTALL)

with open(r'GMK360.Web\Views\Shared\_ConstructionLayout.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
