# 🚨 GMK360 PROJESİ - AI KODLAMA ANAYASASI 🚨
Bu projede kod yazarken veya güncellerken KESİNLİKLE uyulması gereken kurallar şunlardır:

1. **ASLA DOSYAYI BAŞTAN YAZMA:** Bir Controller, Entity, Interface veya View dosyasında düzeltme istendiğinde, sınıfın tamamını heyecan yapıp sıfırdan oluşturma.

2. **ESKİ METODLARI KORU:** Benden aksine net bir talep gelmediği sürece; dosyada zaten var olan, çalışan GET, POST ve diğer Action metodlarını ASLA SİLME VE EZME.

3. **SADECE FARKI GÖSTER:** Sadece benden istenen düzeltmeyi, güncellemeyi veya yeni eklenen kod bloğunu yaz.

4. **YER TUTUCU KULLAN:** Değiştirmeyeceğin veya müdahale etmeyeceğin mevcut kod bloklarının yerine kodun tamamını yazmak yerine // ... mevcut kodlar korunacak ... yazıp geç.

5. **KONTROLLÜ DOKUNUŞ:** Görevin mevcudu yıkıp yeniden yapmak değil, mevcut mimariye nokta atışı modifikasyon yapmaktır. Her işlemden önce bu kuralları hatırla.

6. **GIT GÜNCELLEMESİ (COMMIT):** Her başarılı işlemden ve çalışan kod yazımından sonra, projenin mevcut sağlam halini güvence altına almak için derhal Git Commit (ve gerekiyorsa Push) yap. Sistem bozulursa en son sağlam noktaya (noktasız bile olsa) kayıpsız dönülebilmelidir.

7. **GET VE POST SİMETRİSİ (UI ODAKLI MANTIK):** GET ve POST metotları yazılırken referans noktası her zaman UI (Arayüz/Görsel) olmalıdır. 
   - GET metodu yazarken UI'da hangi alanlar (input, select) olduğuna bak, o verileri eksiksiz getir.
   - POST metodu yazarken sadece UI'dan gelen verileri yakala ve güncelle.
   - UI'da görünmeyen (Örn: Kim ekledi, Son düzenlenme tarihi vb.) sistem verilerini POST işlemi sırasında "boş geldi" sanıp ezme, sadece değişenleri güncelle.

8. **TARİH VE SAAT ASLA BİRLEŞTİRİLMEZ (HER ZAMAN AYRI İKİ INPUT):**
   - Formlarda `datetime-local` tek bir kutu olarak KESİNLİKLE KULLANILMAZ.
   - Tarih ve Saat her zaman iki bağımsız girdi alanıdır:
     - `Tarih`: `<input type="date" name="...Date" ...>`
     - `Saat`: `<input type="time" name="...Time" ...>`
   - Tablo ve listelerde de Tarih ve Saat gerekliyse ayrı kolonlar veya net ayrılmış iki rozet/bilgi olarak gösterilir.
   - Controller (Backend) tarafında tarih ve saat ayrı parametreler olarak yakalanır ve sunucuda güvenle birleştirilir.

9. **PROJE VE ÇALIŞMA ALANI BAĞLAMI ASLA BOZULMAZ (KULLANICI DIŞARI FIRLATILMAZ):**
   - Bir proje çalışma alanındayken (örneğin Aday Proje, Şantiye vb.), projenin sol menüsü, üst proje bilgileri ve bağlamı ASLA kaybedilmez.
   - Sol menüdeki hiçbir link kullanıcıyı projenin dışına (şirketin genel ana menüsüne) atamaz.
   - Tüm alt operasyonlar (Toplantılar, Künye, İnceleme, Finans, Arşiv vb.) proje çalışma alanının kendi sayfasında, projeye ait div'ler üzerinden yürütülür.
   - Merkezi sistemlerle (Ajanda, Kasa, Arşiv) entegre çalışılır; fakat kullanıcı daima projenin içindedir.

10. **EXCEL VE TASARIM ŞEMALARINDAKİ DİV HİYERARŞİSİ:**
    - Şemada veya Excel'de sol menünün karşısındaki her başlık/kutu sayfa içi bağımsız bir DİV / KART'tır.
    - "Yeni sekmede açılacak" olarak not düşülenler dahil olmak üzere her modül kendi iskelet div'iyle sayfada açık ve hazır bekletilir ("divleri aç bekle").
    - Dışarıda olması gerekenler başka bir div'in içine alt madde olarak hapsedilmez; her biri ana sayfada bağımsız birinci sınıf kart olarak yer alır.

11. **ŞANTİYE VE METRAJ MATRİSİ ANAYASASI (KİM SATAR vs. KİM YAPAR):**
    - **KİM SATAR (Kanat 1 - DrawingArchive / Satın Alma Matrisi):** Ekranda SADECE malzeme tedariki ve satın alma görünür (Branş, malzeme adı, ticari birim, piyasa birim fiyatı, spot araştır, anlaşmalı tedarikçi). Ekranda işçilik/taşeron kalabalığı KESİNLİKLE OLMAZ.
    - **KİM YAPAR (Arka Plan Bağlantısı):** Her malzeme arka planda (DTO/Model) `ProfessionName` (Usta / Taşeron ekibi) ile bağlıdır. Bu bağ sayesinde Kanat 2'de (Şantiye Masası / ProjectDNA / BuildingWorkflow) adam/gün, iş süresi ve taşeron hakedişi otomatik hesaplanır; satın alma ekranına amele yüklenmez.
    - **SEKTÖR VE BRANŞ GERÇEKLERİ (ASLA BİRBİRİNE KARIŞTIRILMAZ):**
      - *Alçı vs. Boya:* Dyo, Filli Boya alçı satmaz. Alçı (kaba/saten) `Alçı & Kuru Duvar` toptancısından, boyalar `Boya Toptancısı`ndan alınır.
      - *Banyo Tavanı:* Yeni binalarda banyo tavanı kesinlikle alçı/sıva/boya DEĞİLDİR; üst katın pis su (pimaş) tesisatına kolay müdahale için `60×60 Karopan / Klipin Modüler Asma Tavan (m²)` yapılır.
      - *Sıvacı vs. Şapçı:* Tamamen ayrı meslekler ve malzemelerdir. Sıvacı kaba/ince kum ve torba çimento alır; Şapçı 0-3 mm şap kumu, çimento, şap elyafı ve ano profili alır.
      - *Mobilya & Marangozluk:* Mutfak dolabı (mtül), Banyo Hilton takımı (Takım) ve Vestiyer (Takım) mobilyacının tek paketidir, darmadağın edilmez.
      - *Donatı Filesi vs. Sıva:* Sıva torba (25 kg), donatı filesi m² / rulo (160 gr alkali dayanımlı) satılır; asla tek kaleme hapsedilmez.
      - *Seramik vs. Boya:* Sıfır ticari bağı olan iki ayrı dünyadır; ayrı branşlardır.
    - **SATIN ALMA MATRİSİ ARAYÜZ PRENSİPLERİ:**
      - Kartlar varsayılan olarak kapalı (`collapse`) gelir; başlığa tıklanınca açılır.
      - Kart başlıklarında göz yoran hap rozetler, aç/kapa butonları olmaz.
      - Excel butonu yalındır: `<i class="bi bi-file-earmark-excel-fill"></i> Ver`.
      - Manuel "Fiyat & Firma Kaydet" butonu yoktur; onaylar B2B satın alma onay akışından otomatik beslenir.
      - Satır bazında gereksiz buton kalabalığı yerine en solda sadece `No` (Sıra No) bulunur; araştırma grup başlığındaki `Toplu Teklif İste / Araştır` butonuyla topluca yönetilir.
