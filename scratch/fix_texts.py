with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

# Change ESKİ (MEVCUT) BİNALAR -> ESKİ (MEVCUT) YAPILAR
content = content.replace('ESKİ (MEVCUT) BİNALAR', 'ESKİ (MEVCUT) YAPILAR')
content = content.replace('YENİ (YAPILACAK) BİNALAR', 'YENİ (YAPILACAK) YAPILAR')

# Change "Yıkılacak Mevcut Blok (Bina) Sayısı" -> "Yıkılacak Mevcut Yapı Sayısı"
content = content.replace('Yıkılacak Mevcut Blok (Bina) Sayısı', 'Yıkılacak Mevcut Yapı Sayısı')

# Change "İnşa Edilecek Yeni Blok Sayısı" -> "İnşa Edilecek Yeni Yapı Sayısı"
content = content.replace('İnşa Edilecek Yeni Blok Sayısı', 'İnşa Edilecek Yeni Yapı Sayısı')

with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "w", encoding="utf-8") as f:
    f.write(content)
print("Bina/Blok texts changed to Yapı")
