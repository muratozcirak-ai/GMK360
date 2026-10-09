import re

with open('GMK360.Web/Views/ConstructionProject/Create.cshtml', 'r', encoding='utf-8') as f:
    content = f.read()

# Fix prevStep(1)
content = content.replace('prevStep(1)', 'nextStep(1)')
content = content.replace('prevStep(2)', 'nextStep(2)')

# Add hidden fields for ProjectOriginId and Name to Step 1 form
hidden_fields = '''
<input type="hidden" asp-for="DraftProjectId" id="DraftProjectId" />
<input type="hidden" asp-for="ProjectOriginId" />
<input type="hidden" asp-for="Name" />
'''
# Replace the existing DraftProjectId hidden field with our expanded ones
content = re.sub(r'<input type="hidden" asp-for="DraftProjectId"[^>]*>', hidden_fields, content)

# Update texts
content = content.replace('Yıkılacak Mevcut Yapı (Fiziksel Kütle) Sayısı', 'Yıkılacak Mevcut Blok (Bina) Sayısı')
content = content.replace('İnşa Edilecek Yeni Blok Sayısı', 'İnşa Edilecek Yeni Blok (Giriş) Sayısı')
content = content.replace('Eski Blok / Yapı', 'Eski Blok')

with open('GMK360.Web/Views/ConstructionProject/Create.cshtml', 'w', encoding='utf-8') as f:
    f.write(content)

print("Create.cshtml fixed!")
