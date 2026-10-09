import re
with open(r'GMK360.Web\Views\Shared\_Layout.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

old_text = 'Made with <i class="bi bi-heart-fill text-danger mx-1"></i> by GMK Tech'
new_text = 'Made with <i class="bi bi-heart-fill text-danger mx-1"></i> by <strong>BSN 360</strong> <span style="font-size: 0.7rem; opacity: 0.7;">(Base Station Nexus 360)</span>'
content = content.replace(old_text, new_text)

with open(r'GMK360.Web\Views\Shared\_Layout.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
