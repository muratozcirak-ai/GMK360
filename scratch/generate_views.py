import codecs
import re
import os

with codecs.open('GMK360.Web/Views/PhaseTwo/Index.cshtml', 'r', 'utf-8-sig') as f:
    template = f.read()

phases = [
    ('Three', 'FAZ 3: Kaba Yapı (Betonarme ve Duvar İşleri)', ['3.1 Kolon, Kiriş ve Döşeme', '3.2 Duvar ve Lento İşleri', '3.3 Özel İmalatlar']),
    ('Four', 'FAZ 4: Çatı ve İzolasyon İşleri', ['4.1 Çatı', '4.2 Dış Cephe', '4.3 İzolasyon']),
    ('Five', 'FAZ 5: İnce İşler (Mimari İmalatlar)', ['5.1 Sıva ve Boya', '5.2 Zemin Kaplamaları', '5.3 Doğrama ve Ahşap']),
    ('Six', 'FAZ 6: Mekanik ve Sıhhi Tesisat', ['6.1 Sıhhi Tesisat', '6.2 İklimlendirme', '6.3 Doğalgaz ve Yangın']),
    ('Seven', 'FAZ 7: Elektrik ve Zayıf Akım Tesisatı', ['7.1 Kuvvetli Akım', '7.2 Zayıf Akım', '7.3 Topraklama ve Paratoner']),
    ('Eight', 'FAZ 8: Çevre Düzenleme, Temizlik ve Teslimat', ['8.1 Altyapı Bağlantıları', '8.2 Çevre Düzenleme', '8.3 Temizlik', '8.4 Teslimat ve İskan'])
]

for p_name, p_title, p_subs in phases:
    os.makedirs(f'GMK360.Web/Views/Phase{p_name}', exist_ok=True)
    c = template.replace('/PhaseTwo/', f'/Phase{p_name}/')
    c = c.replace('FAZ 2: Hafriyat ve Temel İnşaatı (Kaba Yapı Başlangıcı)', p_title)
    
    # Replace subcategories
    subs_str = ',\n        '.join(f'"{s}"' for s in p_subs)
    target_subs = r'var subCategories = new List<string> \{.*?\};'
    replacement_subs = f'''var subCategories = new List<string> {{
        {subs_str}
    }};'''
    c = re.sub(target_subs, replacement_subs, c, flags=re.DOTALL)
    
    with codecs.open(f'GMK360.Web/Views/Phase{p_name}/Index.cshtml', 'w', 'utf-8-sig') as f:
        f.write(c)