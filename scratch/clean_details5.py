import io
import re

filepath = r'GMK360.Web\Views\ConstructionProject\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Delete Drafts Card
content = re.sub(r'<div class="card border-0 shadow-sm rounded-4 mb-4">\s*<div class="card-header bg-warning.*?</div>\s*</div>\s*</div>', '', content, flags=re.DOTALL)

# Delete Draft Modal
content = re.sub(r'<!-- Yeni Talep Modal -->.*?</div>\s*</div>\s*</div>', '', content, flags=re.DOTALL)

# Delete Dashboard Radar
content = re.sub(r'<!-- KOMUTA MERKEZ. \(DASHBOARD\) -->.*?</div>\s*</div>\s*</div>', '', content, flags=re.DOTALL)

# Let's just remove anything mentioning msg.PhaseTask or draft.Id
content = re.sub(r'draft\.Id', '0', content)
content = re.sub(r'draft\.Name', '""', content)
content = re.sub(r'draft\.Description', '""', content)
content = re.sub(r'msg\.SenderName', '""', content)
content = re.sub(r'msg\.Message', '""', content)
content = re.sub(r'msg\.PhaseTask\?\.Name', '""', content)
content = re.sub(r'msg\.SentAt\.ToString', '""', content)


with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
