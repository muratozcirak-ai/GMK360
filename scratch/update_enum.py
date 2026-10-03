import codecs
import re

path = 'GMK360.Core/Entities/Construction/ConstructionProjectExpense.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'public enum ConstructionExpenseType\s*\{[^}]*\}'
replacement = '''public enum ConstructionExpenseType
    {
        SantiyeIasesi = 1,      // Şantiye Yemek/Çay/Su
        TemsilAgirlama = 2,     // Müşteri Yemek, Lansman, Kanepe vs
        DigerGenelGider = 3,    // Ofis, Kırtasiye, Ulaşım
        SirketIciYemek = 4,     // Şirket İçi Yemek
        Iletisim = 5,           // İletişim (Telefon / İnternet)
        Demirbas = 6,           // Demirbaş
        SarfMalzeme = 7,        // Sarf Malzeme
        Temizlik = 8            // Temizlik
    }'''

content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Updated enum.')