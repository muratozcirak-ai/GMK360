# GMK360 — PROJE ANAYASASI & MİMARİ KURALLAR

> Bu dosya, yeni açılan her sohbette geçmiş bağlamı sıfırdan kaybetmemek ve çalışan modülleri bozmamak için bağlayıcı referanstır.
> **Yeni Sohbette Kullanım:** `@PROJE_ANAYASASI.md` ve üzerinde çalışacağın ilgili tek dosyayı (ör. `@BuildingManagerController.cs` veya `@Sakinler.cshtml`) etiketleyerek görevi ver.

---

## 1. Altın Kurallar (Kesin Yasaklar & Teslimat Standardı)

1. **Asla Baştan Yazma ve Silme:** Sadece istenen yere müdahale edilir. Mevcut çalışan Controller/Entity/Service dosyalarını veya sınıfları komple silip baştan yazmak yasaktır.
2. **GET, POST, Action ve Linkler Kutsaldır:** Mevcut Controller metotları (GET, POST, Action), route'lar ve sayfa linkleri kullanıcının açık onayı olmadan silinmez, adı değiştirilmez veya üzerine yazılmaz; sadece istenen yeni blok/parametre geriye dönük uyumlu şekilde eklenir.
3. **Nokta Atışı Ekleme ve Akış Koruma:** İstenen ekleme hangi kod bloğunun (süslü parantezin) içindeyse yalnızca oraya uygulanır. Sistemin mevcut çalışma akışı kırılmaz.
4. **Hardcoded Enum Kullanma (Dinamik Lookup Kuralı):** Yeni kategori, gider grubu, tip ve durum listeleri hardcoded `enum` yerine veritabanı Lookup tablolarından (`DefinitionCategories` / `DefinitionValues` veya `ExpenseCategories`) dinamik gelir.
5. **Fiziksel Silme Yasaktır (Soft-Delete & Ters Kayıt):**
   - Genel tablolarda `IsDeleted = true` (Soft-Delete) kullanılır, fiziksel `Remove` yapılmaz.
   - Muhasebe/Gider kayıtlarında (`BuildingExpense`) silme tamamen kapalıdır; iptal için `ReversalOfExpenseId` ile **Ters Kayıt (Reversal)** açılır.
6. **Modül İzolasyonu (İnşaat/Şantiye vs Bina Yönetimi):**
   - `ConstructionProjectController` (İnşaat/Şantiye) ile `BuildingManagerController` (Bina & Site Yönetimi) birbirinden ayrıdır.
   - Paylaşılan `Building`, `BuildingUnit`, `UnitSpace`, `SpaceFixture` tablolarına eklenen yeni kolonlar mutlaka **nullable veya varsayılan değerli (additive)** olmalı, mevcut kolonlar silinmemelidir.
7. **Türkçe Karakter Hassasiyeti:** Veritabanı değerleri, UI metinleri ve kod isimlendirmelerinde UTF-8 / Türkçe karakter kurallarına dikkat edilir, karakter bozulmasına izin verilmez.
8. **Açıklamalı ve Yer Tutuculu Teslimat:** Kod paylaşımında değişmeyen kısımlar `// ... mevcut kodlar korunacak ...` şeklinde geçilir; yapılan ekleme net açıklamayla sunulur.

---

## 2. Teknik Ortam, PowerShell ve VDS Kuralları

- **PowerShell:** Komut birleştirmede `&&` kullanılmaz (noktalı virgül `;` veya ayrı komut kullanılır). Dosya/kod aramalarında `Select-String` kullanılır.
- **Yerel Test Hesabı:** `ibrahinsancak@gmail.com`
- **Veritabanı / VDS Güncelleme Kuralı:**
  - Şema değişikliği yapılan her adımda EF Core migration (`dotnet ef migrations add ...`) yanında mutlaka `GMK360.Data/Migrations/Scripts/YYYYMMDD_<Ad>_VdsSafe.sql` şeklinde **idempotent (`IF COL_LENGTH(...) IS NULL`)** VDS SQL scripti üretilir.
  - Lokal `LocalDB` güncellenir, `dotnet build` **0 Hata** doğrulanır, ardından commit + push yapılır.

---

## 3. Tamamlanan Modüller (Tekrara Düşme / Bozma)

1. **Onboarding & Bina/Blok İskeleti:** Onboarding Adım 2 sadece yapı iskeletidir; dış alan/arazi oto-kontrol, çoklu kat birim üretici ve yaşayan demirbaş ağacı `Detail.cshtml` üzerinde bağımsız çalışır (`4ae467c`, `aeecc95`).
2. **Dönemsel Cari İzolasyonu & Aza Onayı:** `ResidencyPeriods` (Malik/Kiracı aktif/arşiv dönem ayrımı, kıstelyevm, borçlu devir kilidi) ve 2 Aza Multi-Sig onay kuyruğu (`FacilityGovernanceService.cs`).
3. **Personel, SGK, Puantaj & Kıdem Fonu:** Kadro personeli, bayram çift yevmiye (+1 yevmiye), yıllık izin çakışma kontrolü, aylık SGK ödeme ve Malik Kıdem Fonu (`FacilityPayrollAndBudgetService.cs`, `Accounting.cshtml`).
4. **Saha PWA (3 Rol / Görev) & Telefon+PIN Girişi:** `Account/QuickPinLogin` → `FacilityPortal` (Yönetici / Malik-Kiracı / Görevli), fotoğraflı arıza/temizlik talebi ve kanıt fotoğrafı (`FacilityTaskDispatchService.cs`).
5. **KMK Md. 20 Aidat & Gider Motoru:** Birim katsayısı, sabit aidat, asansör/merdiven muafiyeti, çöp/temizlik %, malik/kiracı cari ayrımı, tekil/sabit ve **Sabit Bütçe Karma Dağıtım** simülasyon önizlemesi (`5763384`, `Aidatlar.cshtml`).
6. **Malik & Kiracı Ayrı PWA PIN + 2 Sekmeli Sakin Modalı:** `BuildingUnit.OwnerQuickPinHash` ve `TenantQuickPinHash` ile ayrı giriş ve davet kopyalama; `Sakinler.cshtml` modalında *Sakinler & PIN* ve *Tapu & Aidat Kuralları* ayrımı.
7. **5 Dinamik Gider Türü (Lookup) & Birleşik Muhasebe Masası:** `FACILITY_EXPENSE_GROUP` (`DefinitionSeeder.cs`) ile 5 dinamik gider türü (`Personel`, `FaturaAbonelik`, `BakimOnarim`, `DemirbasYatirim`, `YonetimIdari`) ve tek *Muhasebe, Gider & Kasa* ekranı (`Accounting.cshtml`, `/BuildingManager/Giderler` uyumlu yönlendirmeli).

---

## 4. Aktif Yol Haritası (Sıradaki Adımlar)

- **Canlı VDS Güncelleme & Test:**
  1. VDS SQL: `GMK360.Data/Migrations/Scripts/20261122_AddOwnerTenantPinsAndExpenseGroup_VdsSafe.sql` (ve önceki `20261121_AddDuesRulesAndUnitExemptions_VdsSafe.sql` henüz çalıştırılmadıysa o da).
  2. VDS üzerinde `git pull` / publish sonrası `gmk360.com` ve `Aydemir Apartmanı` üzerinde:
     - Malik ve Kiracı için ayrı Telefon + PIN girişi testi
     - 5 gider türüyle sabit gider / abonelik + dükkan muafiyeti/artışı + Karma Dağıtım Önizleme + toplu aidat kesme ve tahsilat testi.
