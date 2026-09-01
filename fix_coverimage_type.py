import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('<input asp-for="CoverImageFile" class="form-control rounded-3" placeholder="https://..." />', '<input asp-for="CoverImageFile" type="file" class="form-control rounded-3" accept="image/jpeg,image/png,application/pdf" />')
content = content.replace('<div class="form-text text-muted">Mimari resim veya 3D render görselini ekleyerek projenizi görselleştirin.</div>', '<div class="form-text text-muted">Mimari resim veya 3D render görselini bilgisayarınızdan seçin (JPG/PNG).</div>')

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated to type=file")
