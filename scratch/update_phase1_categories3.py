import codecs
import re

path = 'GMK360.Web/Views/PhaseOne/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'var subCategories = new List<string> \{\s*"1\.1 Yıkım ve Hafriyat Öncesi Hazırlık",\s*"1\.2 Arazi Ölçümü ve Çevre Güvenliği",\s*"1\.3 Geçici Şantiye Yapıları \(Yaşam Alanı\)",\s*"1\.4 Şantiye Elektriği ve Suyu"\s*\};'
replacement = '''var subCategories = new List<string> {
        "1.1 Yıkım ve Hafriyat Öncesi Hazırlık",
        "1.2 Arazi Ölçümü ve Çevre Güvenliği",
        "1.3 Geçici Şantiye Yapıları (Yaşam Alanı Kurulumu)",
        "1.4 Şantiye Elektriği ve Suyu"
    };'''
content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)