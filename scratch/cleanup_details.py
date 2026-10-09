import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# 1. Remove the yellow warning alert for Tevhit
alert_pattern = r'<!-- Tevhit Uyarısı -->.*?</div>'
content = re.sub(alert_pattern, '', content, flags=re.DOTALL)
# Also remove the hardcoded alert if it wasn't wrapped in comment
alert_pattern2 = r'<div class="alert alert-warning border-0 bg-warning bg-opacity-10 rounded-4 p-3 mb-4 d-flex align-items-center justify-content-between shadow-sm">.*?</div>\s*</div>'
content = re.sub(alert_pattern2, '', content, flags=re.DOTALL)

# 2. Remove the mergeBlocksModal
modal_pattern = r'<!-- Tevhit \(Birleştirme\) Modalı -->.*?</div>\s*</div>\s*</div>'
content = re.sub(modal_pattern, '', content, flags=re.DOTALL)

# 3. Clean up any remaining Tevhit buttons or alerts manually by checking strings
if 'Tevhit (Birleştirme) Gerekli Olabilir!' in content:
    content = re.sub(r'<div class="alert alert-warning.*?Tevhit \(Birleştirme\).*?</div>\s*</div>', '', content, flags=re.DOTALL)

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("Details.cshtml cleaned up successfully.")
