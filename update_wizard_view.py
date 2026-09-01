import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Make form multipart/form-data
content = content.replace('<form id="wizardForm" asp-action="CreateWizard" method="post">', '<form id="wizardForm" asp-action="CreateWizard" method="post" enctype="multipart/form-data">')

# Replace CoverImageUrl field with CoverImageFile
file_replacement = """                    <div class="mb-4">
                        <label asp-for="CoverImageFile" class="form-label fw-bold">Mimari Çizim / Proje Görseli</label>
                        <input asp-for="CoverImageFile" type="file" class="form-control form-control-lg rounded-3" accept="image/jpeg,image/png,application/pdf" />
                        <div class="form-text text-muted">Mimari resim veya 3D render görselini seçerek projenizi görselleştirin.</div>
                    </div>"""

content = re.sub(r'<div class="mb-4">\s*<label asp-for="CoverImageUrl"[^>]+>.*?</label>\s*<input asp-for="CoverImageUrl"[^>]+/>\s*<div class="form-text text-muted">.*?</div>\s*</div>', file_replacement, content, flags=re.DOTALL)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated Wizard View for file upload.")
