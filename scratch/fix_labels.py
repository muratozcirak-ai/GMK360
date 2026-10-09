import re

with open('GMK360.Web/Views/ConstructionProject/Create.cshtml', 'r', encoding='utf-8') as f:
    content = f.read()

# Replace labels
content = content.replace('Yıkılacak Mevcut Blok (Bina) Sayısı', 'Yıkılacak Mevcut Yapı Sayısı')
content = content.replace('İnşa Edilecek Yeni Blok (Giriş) Sayısı', 'Yapılacak Yeni Yapı Sayısı')

# For the dynamically generated ones:
content = content.replace('. Eski Blok', '. Eski Yapı')
content = content.replace('Eski Blok Adı', 'Eski Yapı Adı')

content = content.replace('. Yeni Blok / Yapı', '. Yeni Yapı')
content = content.replace('Yapı / Blok Adı', 'Yeni Yapı Adı')

# And for the first default ones (if they are hardcoded in razor loop)
content = content.replace('@(i+1). Eski Blok', '@(i+1). Eski Yapı')
content = content.replace('@(i+1). Yeni Blok / Yapı', '@(i+1). Yeni Yapı')

# Let's also check if he wants "Yapılacak Yeni Yapı Sayısı" instead of "İnşa Edilecek Yeni Blok (Giriş) Sayısı"
# Wait, I already did that replacement.

with open('GMK360.Web/Views/ConstructionProject/Create.cshtml', 'w', encoding='utf-8') as f:
    f.write(content)
print("Labels changed successfully!")
