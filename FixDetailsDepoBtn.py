import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Details.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

old_btn = '''        <a asp-action="ManagePhases" asp-route-id="@Model.Id" class="btn btn-danger rounded-pill px-4 shadow-sm text-white fw-bold">
            <i class="bi bi-kanban me-2"></i> Şantiye Aşamaları ve Bütçe (ERP)
        </a>'''

new_btn = '''        <a asp-controller="Inventory" asp-action="Index" class="btn btn-info text-white rounded-pill px-3 shadow-sm">
            <i class="bi bi-boxes me-1"></i> Şantiye Deposu
        </a>
        <a asp-action="ManagePhases" asp-route-id="@Model.Id" class="btn btn-danger rounded-pill px-4 shadow-sm text-white fw-bold">
            <i class="bi bi-kanban me-2"></i> Şantiye Aşamaları ve Bütçe (ERP)
        </a>'''

content = content.replace(old_btn, new_btn)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
