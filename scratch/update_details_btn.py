import io
import re

filepath = r'GMK360.Web\Views\ConstructionProject\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Rename the button
pattern1 = r'<button type="button" class="btn btn-sm btn-danger fw-bold rounded-pill text-nowrap shadow-sm" data-bs-toggle="modal" data-bs-target="#addCustomDocModal">\s*<i class="bi bi-plus-lg"></i> Özel Evrak Ekle\s*</button>'
replacement1 = r'<a href="/PhaseZero/Index/@Model.Id" class="btn btn-sm btn-danger fw-bold rounded-pill text-nowrap shadow-sm">\n                    <i class="bi bi-rocket-takeoff"></i> Faz 0\'ı Yönet\n                </a>'
content = re.sub(pattern1, replacement1, content)

# 2. Remove the "Yönet" column headers
content = content.replace('<th class="text-end pe-4" style="width: 100px;">İşlem</th>', '')

# 3. Remove the "Yönet" cells
content = re.sub(r'<td class="text-end pe-4">\s*<a href="javascript:void\(0\)".*?<i class="bi bi-pencil-square"></i> Yönet</a>\s*</td>', '', content, flags=re.DOTALL)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated Details.cshtml")
