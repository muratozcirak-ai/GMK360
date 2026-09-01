import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Details.cshtml"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Add button next to "Projelere Dön"
old_buttons = """    <a asp-action="Index" class="btn btn-outline-secondary rounded-pill px-4">
        <i class="bi bi-arrow-left me-2"></i> Projelere Dön
    </a>"""

new_buttons = """    <div>
        <a asp-action="Index" class="btn btn-outline-secondary rounded-pill px-3 me-2">
            <i class="bi bi-arrow-left me-1"></i> Projelere Dön
        </a>
        <a asp-controller="ProjectMaterial" asp-action="Index" asp-route-projectId="@Model.Id" class="btn btn-primary rounded-pill px-4 shadow-sm">
            <i class="bi bi-palette me-2"></i> Müşteri Malzeme Kataloğu
        </a>
    </div>"""

content = content.replace(old_buttons, new_buttons)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Added catalog link to Details.cshtml")
