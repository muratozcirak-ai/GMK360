import re

with open(r'GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Fix the header title
# It was: <h2 class="fw-bold mb-1"><i class="bi bi-layers text-primary me-2"></i>@Model.BlockName Yapı Yönetimi</h2>
content = content.replace('<h2 class="fw-bold mb-1"><i class="bi bi-layers text-primary me-2"></i>@Model.BlockName Yapı Yönetimi</h2>', '<h2 class="fw-bold mb-1"><i class="bi bi-layers text-primary me-2"></i>Yapı Yönetimi</h2>')

# Fix ViewData title just in case
content = content.replace('ViewData["Title"] = "Yapı Yönetimi: " + Model.BlockName;', 'ViewData["Title"] = "Yapı Yönetimi";')

with open(r'GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("Title fixed to just Yapı Yönetimi")
