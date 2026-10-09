import re

with open(r'GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

content = content.replace('<h2 class="fw-bold mb-1"><i class="bi bi-layers text-primary me-2"></i>@Model.BlockName Yönetimi</h2>', '<h2 class="fw-bold mb-1"><i class="bi bi-layers text-primary me-2"></i>@Model.BlockName Yapı Yönetimi</h2>')
content = content.replace('Blok Özellikleri', 'Yapı Özellikleri')
content = content.replace('Blok özelliklerini', 'Yapı özelliklerini')
content = content.replace('ViewData["Title"] = "Blok Yönetimi: " + Model.BlockName;', 'ViewData["Title"] = "Yapı Yönetimi: " + Model.BlockName;')

with open(r'GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("Terminology updated to Yapı Yönetimi / Yapı Özellikleri")
