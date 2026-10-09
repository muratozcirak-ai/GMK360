# GMK360 - TEMEL VİZYON VE ALTIN KURALLAR

## 1. ALTIN KURAL: "ÖNCE VİTRİN, SONRA MOTOR" (Vitrin-First Development)
Yeni bir modül, özellik veya yapı geliştirileceği zaman ASLA doğrudan veritabanı veya arka plan kodlamasına başlanmaz.
Önce o modülün **Müşteri İniş Sayfası (Landing Page / Vitrin)** ve **Pazarlama Vaatleri (Sloganları)** yazılır. 
- "Müşteriye ne vaat ediyoruz?"
- "Bu ekranı gören esnaf/usta/müşavir neden heyecanlanacak?"
Bu soruların cevabı vitrine döküldükten sonra, arka plan (Backend) kodları ve Yapılacaklar Listesi (To-Do) tamamen bu vitrindeki vaatleri gerçekleştirmek üzere inşa edilir.

## 2. KARMAŞA YOK, BAKKAL DEFTERİ VAR (Sıfır Sürtünme)
Esnaf, taşeron veya mülk sahibi; stok kodu, KDV oranı, fatura tipi gibi karmaşık muhasebe terimleriyle boğulmaz. 
Her şey "Bakkal Defteri" sadeliğinde olmalıdır. Kullanıcı sisteme serbest metin ("Ahmet ustaya 500 TL verildi") girebilmeli, arka plandaki ERP ve Yapay Zeka bu ham veriyi profesyonel muhasebeye kendi kendine çevirmelidir.

## 3. HAVUÇ KURALI (İş Ekosistemi)
Kullanıcıya sadece bir "Yönetim Paneli" veya "Kasa Defteri" satılmaz. Sistemin asıl kancası (Havuç); kullanıcının bu ağda "Yeni İş Fırsatları", "Dev Projelerden İhaleler" ve "Yeni Müşteriler" bulacak olmasıdır. Yazılım bir külfet değil, para kazandıran bir asistandır.

## 4. DAĞINIK ZENGİNLİKLERİN YÖNETİMİ (VIP Katman)
Site yönetimi tek bir dev yapıyı yönetmekken; asıl para, farklı yerlerdeki dağınık mülkleri yöneten Mali Müşavirler ve Varlık Yöneticilerindedir (Family Office). Onlara her zaman otopilot (GMSİ hesaplama, OCR kontrat okuyucu) destekli elit bir kokpit sunulur.

## 5. YAZILIMCI (DEVELOPER) ALTIN KURALLARI
1. **Çalışan Koda Dokunma:** Halihazırda çalışan GET ve POST metodlarına, view'lara kesinlikle dokunulmaz. 
2. **Sil Baştan Yok, Nokta Atışı Çözüm:** Bir hata çıktığında "kodu komple silip baştan yazmak" YASAKTIR. Hata nerede ise tam o satır bulunup nokta atışı çözülür, böylece projedeki bağlantılı diğer yerler bozulmaz.
3. **Ezbere İsimlendirme Yok:** Linkler, veritabanı tablo isimleri veya modeller asla "tahmin edilerek" yazılmaz. Mutlaka mevcut kod tabanı/veritabanı kontrol edilir, doğru isim teyit edildikten sonra kod yazılır.
