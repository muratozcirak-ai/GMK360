import codecs
import re

path = 'GMK360.Web/Views/PhaseOne/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'var subCategories = new List<string> \{\s*"1\.1 Şantiye Elektriği İşleri",\s*"1\.2 Şantiye Suyu İşleri",\s*"1\.3 Arazi Ölçümü ve Çevre Güvenliği",\s*"1\.4 Geçici Şantiye Yapıları",\s*"1\.5 Yıkım ve Hafriyat Hazırlığı"\s*\};'
replacement = '''var subCategories = new List<string> {
        "1.1 Yıkım ve Hafriyat Öncesi Hazırlık",
        "1.2 Arazi Ölçümü ve Çevre Güvenliği",
        "1.3 Geçici Şantiye Yapıları (Yaşam Alanı)",
        "1.4 Şantiye Elektriği ve Suyu"
    };'''
content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)