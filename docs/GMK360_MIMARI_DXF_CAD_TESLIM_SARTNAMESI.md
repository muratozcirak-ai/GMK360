# GMK360 MİMARİ KAT PLANI VE CAD/DXF TESLİM ŞARTNAMESİ
**Doküman No:** GMK-CAD-SPEC-2026-V1  
**Hedef Kitle:** Proje Müellifi Mimarlık Ofisleri, Serbest Mimarlar, Şantiye Teknik Ofisleri  
**Amaç:** Mimari projelerin GMK360 Şantiye Yönetim, Metraj ve Tedarikçi/Taşeron B2B ihale sistemine sıfır kayıpla aktarılması için çizim ve teslim standartlarıdır.

---

## 1. TEMEL İLKE: KAT BAZINDA DIŞA AKTARIM (WBLOCK KURALI)

> [!IMPORTANT]
> **Tüm binayı tek bir dosyada teslim etmeyiniz!**
> GMK360 sistemi projeyi kat kat ve blok blok işler. Her bloğun her katı bağımsız bir dosya olarak kaydedilmelidir.

- **Dosya İsimlendirme Standardı:**
  - `[BLOK_ADI]_[KAT_ADI].dxf`
  - *Örnekler:*  
    - `E_Blok_1_Normal_Kat.dxf` (veya Tip Kat ise: `E_Blok_Tip_Kat.dxf`)  
    - `D_Blok_Zemin_Kat.dxf`  
    - `Ortak_Baza_Bodrum_1.dxf`  
    - `A_Blok_Cati_Kati.dxf`
- **Dosya Formatı:** AutoCAD 2018 (veya daha eski) **ASCII DXF** veya **DWG**.

---

## 2. MAHAL VE ALAN İSİMLENDİRME KURALLARI

> [!WARNING]
> **KRİTİK UYARI: ODA İSİMLERİ (YAZILAR) KESİNLİKLE YAZILMALIDIR!**
> Mimarlar bazen duvarları ve sınırları çizip içine oda adı yazmayı unutabilmektedir.
> GMK360 metraj ve ihale motorunun çalışabilmesi için **her odanın merkezinde oda adının (`SALON`, `MUTFAK`, `YATAK ODASI`, `BANYO` vb.) ve m² alanının yazılması ZORUNLUDUR.**
> Oda adı yazılmamış isimsiz kapalı poligonlar sistem tarafından mahal olarak tanınamaz.

1. **Kat Tiplerinin Ayrımı:**
   - **Bodrum Katlar (Otopark, Sığınak, Tesisat):** Ayrı dosya olarak teslim edilmelidir.
   - **Zemin Kat (Dükkan, Lobi, Bina Girişi):** Ayrı dosya olarak teslim edilmelidir.
   - **Normal Katlar (Tip Daireler):** Tek bir tip kat DXF'i yeterlidir; sistem tarafından diğer katlara çoğaltılır.
   - **Çatı Katı:** Ayrı dosya olarak teslim edilmelidir.

2. **Bağımsız Metin Kuralı:**  
   Her oda ve mahalin merkezinde bağımsız bir metin (`TEXT` veya `MTEXT`) yer almalıdır.

3. **Mahal İsimleri Standart Türkçe Olmalıdır:**
   - **Yaşam Alanları:** `SALON`, `YATAK ODASI 1`, `YATAK ODASI 2`, `ÇOCUK ODASI`, `OTURMA ODASI`, `GİYİNME ODASI`, `MUTFAK`
   - **Islak Hacimler:** `BANYO`, `EBEVEYN BANYO`, `WC`, `ALATURKA WC`, `ÇAMAŞIR ODASI`
   - **Açık Alanlar:** `BALKON 1`, `BALKON 2`, `TERAS`, `SUNDURMA`
   - **Ortak Alanlar:** `KAT HOLÜ`, `MERDİVEN EVİ`, `YANGIN GÜVENLİK HOLÜ`, `ASANSÖR HOLÜ`, `ŞAFT`

4. **Alan (m²) Gösterimi:**  
   Mahal yazısının hemen altında veya aynı metin içinde m² değeri belirtilmelidir:  
   *Örnek:* `SALON \P 28.50 m²` veya `BANYO: 5.40 m²`

---

## 3. DOĞRAMA, KAPI VE PENCERE STANDARTLARI

Sistem, fabrikasyon hazır kapılar ile atölye imalatı özel kapıları otomatik ayrıştırır:

| Kapı Tipi | Standart Ölçü (En × Boy) | Projede Yazılacak Kod / Blok Adı | Açıklama |
| :--- | :---: | :--- | :--- |
| **Daire Giriş Kapısı** | 100 × 210 cm | `D0` veya `CELIK_KAPI_100` | Çelik Güvenlik Kapısı |
| **İç Oda Kapısı** | 90 × 210 cm | `D1` veya `KAPI_90` | Ahşap / Mobilya / Lake / Melamin |
| **Islak Hacim Kapısı** | 75 × 210 veya 80 × 210 cm | `D2` veya `KAPI_75` | Banyo / WC Kapısı |
| **Yangın Güvenlik Kapısı** | 100 × 210 cm | `DY` veya `YANGIN_KAPISI` | Panik Barlı, 60-90 dk Dayanımlı |
| **Balkon Kapısı** | 80 × 210 veya 140 × 210 cm | `DB` veya `BALKON_KAPISI` | PVC / Alüminyum Isıcamlı |
| **Özel İmalat Kapı** | Standart Dışı Ölçü | `DOZEL_[EN]x[BOY]` | Örn: `DOZEL_110x230` (Atölye Üretimi) |

- **Kapı Açılış Yayı:** Kapı kanadı ve açılış yayları (`ARC`) mimari çizimde kapalı veya açık olarak `KAPI` katmanında çizilmelidir.
- **Pencereler:** Pencere genişliği ve yüksekliği ölçülendirilmeli veya `P1 (140/140)`, `P2 (200/220 Fransız)` şeklinde etiketlenmelidir.

---

## 4. KAT ORTAK ALANLARI VE MERDİVENLER

1. **Merdiven Evi (`STAIR`):**  
   - Basamak çizgileri tek tek çizilmeli (16-18 rıht).  
   - Çıkış hattı ve çıkış oku (`UP`) belirtilmelidir.  
   - Sistem bu çizgilerden doğrudan basamak ve tırabzan metrajını çıkarır.
2. **Kat Holü:** Kat koridoru zemin sınırları net olmalıdır.
3. **Yangın Dolabı & Şaftlar:** Yangın dolabı yeri ve tesisat şaftları (`MEK_SAFT`, `ELEK_SAFT`) çizimde blok veya metin olarak gösterilmelidir.

---

## 5. KATMAN (LAYER) STANDARDI

Çizimlerin karmaşadan arındırılması için aşağıdaki temel katman adlandırmalarına uyulmalıdır:

| Eleman | Tavsiye Edilen Katman (Layer) Adı | Alternatif Layer |
| :--- | :--- | :--- |
| Duvarlar | `MIM_DUVAR` | `A-WALL`, `WALL` |
| İç & Dış Kapılar | `MIM_KAPI` | `A-DOOR`, `DOOR` |
| Pencereler & Camlar | `MIM_PENCERE` | `A-GLAZ`, `WINDOW` |
| Mahal Yazıları & m² | `MIM_MAHAL` | `A-AREA`, `YAZI`, `TEXT` |
| Merdiven & Basamaklar | `MIM_MERDIVEN` | `A-FLOR-STRS`, `STAIR` |
| Kolon & Perdeler | `MIM_KOLON` veya `STATIK` | `S-COLS` |
| Tesisat / Petek (Varsa) | `TES_ISITMA` | `M-HVAC` |

---

## 6. MİMAR İÇİN PRATİK AUTOCAD İPUCU (10 SANİYEDE DIŞA AKTARMA)

Mimarın tüm projeyi baştan sona bölmesine gerek yoktur. AutoCAD'de kat bazında dosya üretmek sadece 3 adımdır:

1. Ana mimari çizimde ilgili kat planının etrafını **pencere içine alarak seçin**.
2. Komut satırına **`WBLOCK`** yazıp `Enter`a basın.
3. Açılan pencerede:
   - Kayıt Formatı: **AutoCAD 2018 DXF** (veya DWG)
   - Dosya Adı: `E_Blok_1_Normal_Kat.dxf`
   - **OK** butonuna tıklayın!

> [!TIP]
> Çıktıyı kaydetmeden önce komut satırına **`PURGE`** yazarak çizimde kullanılmayan çöp katman ve blokları temizlemeniz, dosya boyutunu %80 küçültecek ve GMK360 aktarımını 1 saniyeye indirecektir.

---
**GMK360 İnşaat Yönetim ve Dijital Şantiye Teknolojileri**  
*Şantiye Masası & Satın Alma Yönetim Departmanı*
