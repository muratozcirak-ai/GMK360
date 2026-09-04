# GMK360 - PROJECT MEMORY & CONTEXT TRANSFER

Sevgili Yapay Zeka (Antigravity), bu dosyayı okuyorsan Murat Bey ile yeni bir sohbete başladın demektir. Aşağıdaki bilgiler bizim bugüne kadar kurduğumuz sistemin anayasasıdır. Hepsini hafızana al ve işlemlere buradan devam et:

## 1. Proje Altyapısı
* **Proje:** GMK360 (.NET Core MVC, Entity Framework Core)
* **Dizin:** `C:\Users\murat\source\repos\GMK360`
* **Mevcut Durum:** Veritabanı ve temel MVC yapıları kusursuz çalışıyor. Gereksiz eski projeler (`BSN360_KitchenCore`, `EmlakBurada` vb.) silindi. Lokal veritabanları temizlendi (sadece GMK360Db, BSN360Db ve EmlakBuradaDb duruyor). Tüm kodlar GitHub'a (ve locale zip olarak) yedeklendi.

## 2. İnşaat ve Şantiye Modülü (Şu anki odak noktamız)
* **Blok ve Kat Yönetimi (Tree View):** Binaların bodrum, zemin ve normal katlarını otomatik hesaplayan gelişmiş bir sihirbaz (Wizard) yazdık. `ManageBlock.cshtml` üzerinde katları Bootstrap Accordion ile "Ağaç (Tree View)" şeklinde listeliyoruz (Örn: -2 Bodrum, Zemin, 1. Kat).
* **Mimari Galeri (DMS Entegrasyonu):** Müteahhitler projelerine dış cephe renderları ve "Her Kata Özel" kat planları yükleyebiliyor. Bu dosyalar `DmsDocument` tablosunda `EntityType="Building"` ve `EntityType="BuildingFloor"` olarak tutuluyor.
* **Müşteri Malzeme Portalı:** Kentsel dönüşümde veya topraktan ev alan müşteriler, `CustomerPortal` üzerinden kendi evleri için laminat, fayans, mutfak dolabı gibi seçimleri (müteahhitin sunduğu katalogdan) yapabiliyorlar.

## 3. Usta, Taşeron ve Tedarikçi Ekosistemi (B2B & B2C Büyüme Stratejisi)
Bu platform sadece bir CRM değil, dev bir sektörel pazar yeridir. Stratejimiz şudur:
* **Ustalar (Freemium):** Boyacı, seramikçi gibi ustalar sisteme tamamen ÜCRETSİZ kayıt olur. Amaç sistemi iş gücüyle doldurmaktır.
* **Kurumsal Tedarikçiler (Ücretli):** Cam balkon, güneş enerjisi, duşakabin, mutfak dolabı gibi "Ürün + Montaj" yapanlar, sistemde B2B veya B2C teklif vermek için kurumsal/pro üye olurlar. Nalburlar (inşaat malzemesi satanlar) da buraya dahildir (Serbest yazıyla "Demir lazım" ihalelerine teklif verirler).
* **Taşeronlar (Ücretli):** Usta ekiplerini yöneten taşeronlar, puantaj tutmak ve "Hangi usta boşta?" diye sistemden SMS atmak için sistemi kullanır. Truva atımız onlardır, kendi ustalarını sisteme bedavaya kendileri eklerler.

## 4. Viral Büyüme ve Otokontrol (Growth Hacking)
* Ustalar için `ServiceProviderPortfolio` (Biten İşler/Referanslar) tablosu oluşturuldu.
* Usta referans eklerken `ReferenceName` ve `ReferencePhone` girer.
* Sistem o numaraya (ki bu kişi genelde müteahhit veya ev sahibidir) otomatik "Ahmet Usta referansınızı verdi, onaylıyor musunuz?" diye SMS atar (`VerificationToken` ile).
* **Viral Etki:** Linke tıklayıp onay veren müteahhit/ev sahibi, bizim sistemimizin reklamını görür ("Siz de şantiyenizi yönetmek ister misiniz?") ve sisteme kayıt olur. Bedava reklam!
* **Otokontrol:** Usta sahte numara girerse yeşil tik alabilir ama ilk gerçek işinde müteahhitten "1 Yıldız" yerse sahte referansları çöp olur. Sistem kendi kendini temizler.

## 5. Sıradaki Görev
Murat Bey şu an sistemi derleyip lokalde test aşamasına geçecek. Mimari sihirbazı, kat galerisini ve usta/taşeron mantığını test edip eksikleri çıkaracak. Sen ondan gelecek geri bildirimlere (feedback) göre UI (arayüz) düzeltmelerine ve yeni fonksiyonların kodlanmasına başlayacaksın.

**Murat Bey'e mesajın:** "Hoş geldiniz Murat Bey! Tüm proje hafızasını (Usta freemium mantığını, Tree View kat planlarını, viral SMS büyüme taktiğini) eksiksiz olarak yükledim. Testlerinize başlayabilirsiniz, neleri düzeltiyoruz?" de.
