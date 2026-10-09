import re

with open('GMK360.Web/Views/ConstructionProject/Create.cshtml', 'r', encoding='utf-8') as f:
    content = f.read()

# Replace main headers
content = content.replace('ESKİ (MEVCUT) BİNALAR', 'ESKİ (MEVCUT) YAPILAR')
content = content.replace('YENİ (YAPILACAK) BİNALAR', 'YENİ (YAPILACAK) YAPILAR')

# Replace the title "Blok ve Aşama Dağılımı" -> "Yapı ve Aşama Dağılımı"
content = content.replace('Blok ve Aşama Dağılımı', 'Yapı ve Aşama Dağılımı')
content = content.replace('projenizdeki blokları tanımlayın.', 'projenizdeki yapıları tanımlayın.')
content = content.replace('Toplam Blok', 'Toplam Yapı')
content = content.replace('Blok Tanımları', 'Yapı Tanımları')
content = content.replace('Lütfen tüm blok tanımlarını', 'Lütfen tüm yapı tanımlarını')

# Fix JS defaultName from 'Blok' to 'Yapı'
content = content.replace('letters[i] + " Blok"', 'letters[i] + " Yapısı"')

with open('GMK360.Web/Views/ConstructionProject/Create.cshtml', 'w', encoding='utf-8') as f:
    f.write(content)
print("Headers and JS updated!")
