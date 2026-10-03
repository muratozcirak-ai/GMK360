import codecs
import re

path = 'GMK360.Web/Views/PhaseOne/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'var subCategories = new List<string> \{\s*"1\.1 İdari ve Altyapı Hazırlıkları",\s*"1\.2 Mobilizasyon ve Güvenlik",\s*"1\.3 Yıkım Operasyonu",\s*"1\.4 Zemin Hazırlığı"\s*\};'
replacement = '''var subCategories = new List<string> {
        "1.1 Şantiye Elektriği İşleri",
        "1.2 Şantiye Suyu İşleri",
        "1.3 Arazi Ölçümü ve Çevre Güvenliği",
        "1.4 Geçici Şantiye Yapıları",
        "1.5 Yıkım ve Hafriyat Hazırlığı"
    };'''
content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)