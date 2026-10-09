import codecs

def fix_encoding(path):
    try:
        with codecs.open(path, 'r', 'utf-8-sig') as f:
            content = f.read()
            
        content = content.replace('Gtr', 'Götürü')
        content = content.replace('Ykm ve Zemin Hazrl', 'Yıkım ve Zemin Hazırlığı')
        content = content.replace('Temel ve Alt Yap', 'Temel ve Altyapı')
        content = content.replace('Kaba naat (Karkas)', 'Kaba İnşaat (Karkas)')
        content = content.replace('at ve D Cephe', 'Çatı ve Dış Cephe')
        content = content.replace('nce ler ( Mekan)', 'İnce İşler (İç Mekan)')
        content = content.replace('Elektrik ve Zayf Akm', 'Elektrik ve Zayıf Akım')
        content = content.replace('Dzenle', 'Düzenle')
        content = content.replace('Bte', 'Bütçe')
        content = content.replace('at', 'Çatı')
        content = content.replace('zolasyon', 'İzolasyon')
        content = content.replace('Prosedr', 'Prosedür')
        content = content.replace('?', '₺')
        
        with codecs.open(path, 'w', 'utf-8-sig') as f:
            f.write(content)
    except Exception as e:
        print(f"Error processing {path}: {e}")

fix_encoding('GMK360.Web/Controllers/ProjectFinanceController.cs')
fix_encoding('GMK360.Web/Views/ProjectFinance/Index.cshtml')