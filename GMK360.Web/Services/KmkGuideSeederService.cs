using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Services
{
    /// <summary>
    /// Merkezi KMK Mevzuat &amp; Bina/Site Yöneticisi El Kitabı içeriklerini veritabanına idempotent biçimde ekler.
    /// İçerikler yalnızca Süper Admin tarafından merkezi olarak düzenlenir; site yöneticileri ve ziyaretçiler için salt okunurdur.
    /// </summary>
    public static class KmkGuideSeederService
    {
        public const string PublicCategory = "KMK Bilgilendirme (Genel)";
        public const string HandbookCategory = "Yönetici El Kitabı & Yasal Prosedürler";

        private static readonly SemaphoreSlim _seedLock = new SemaphoreSlim(1, 1);

        public static async Task EnsureKmkGuidesSeededAsync(ApplicationDbContext context)
        {
            if (await context.Articles.AsNoTracking().AnyAsync(a => a.Slug == "bina-acil-durum-toplanma-ve-yangin-yonetmeligi"))
                return;

            await _seedLock.WaitAsync();
            try
            {
                var existingSlugs = await context.Articles
                    .AsNoTracking()
                    .Select(a => a.Slug)
                    .ToListAsync();
                var slugSet = new HashSet<string>(existingSlugs, StringComparer.OrdinalIgnoreCase);

                var baseTime = new DateTime(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc);
                var items = BuildDefaultArticles(baseTime);
                var toInsert = items.Where(x => !slugSet.Contains(x.Slug)).ToList();
                if (toInsert.Any())
                {
                    context.Articles.AddRange(toInsert);
                    await context.SaveChangesAsync();
                }
            }
            finally
            {
                _seedLock.Release();
            }
        }

        private static List<Article> BuildDefaultArticles(DateTime baseTime) => new()
        {
            // 1. HALKA AÇIK REHBERLER (SEO & BİLGİLENDİRME)
            new Article
            {
                Title = "Yeni Seçilen Bina & Site Yöneticisi İçin İlk 10 Adım Rehberi",
                Slug = "kmk-yeni-yonetici-ilk-adimlar",
                Category = PublicCategory,
                IsMembersOnly = false,
                IsPublished = true,
                PublishedAt = baseTime.AddHours(1),
                SeoTags = "bina yöneticisi, yeni yönetici, devir teslim, potansiyel vergi numarası, apartman banka hesabı",
                HtmlContent = @"
<p class=""lead"">Kat Malikleri Kurulu tarafından yeni yönetici seçildiniz. İlk günlerde atacağınız doğru hukuki ve mali adımlar, yıl boyunca hem sizi kişisel sorumluluktan korur hem de komşularınızla güven bağını sağlamlaştırır.</p>
<div class=""alert alert-info border-0 rounded-3""><strong>Temel Kural (KMK Md. 35 &amp; 38):</strong> Yönetici, kat maliklerine karşı aynen bir vekil gibi sorumludur. Devir-teslimi tutanakla almadan ve resmi defterleri kontrol etmeden tek kuruş harcama yapmayınız.</div>
<h5 class=""fw-bold mt-4"">1. Toplantı Kararını Noter Defterine İşletip İmzaları Tamamlayın</h5>
<p>Yönetici seçildiğinize dair karar, toplantıda hazır bulunan kat malikleri tarafından noter onaylı karar defterine yazılmalı ve imzalanmalıdır. Bu sayfa banka hesabı açma, vergi dairesi ve noter işlemlerinde yetki belgeniz olacaktır.</p>
<h5 class=""fw-bold mt-4"">2. Eski Yönetimden Evrak ve Kasa Devir-Teslim Tutanağı Alın</h5>
<ul>
  <li>Noter onaylı <strong>Karar Defteri</strong> ve (varsa) İşletme Defteri</li>
  <li>Kasa nakit mevcudu ve banka hesap cüzdanları / dekontları</li>
  <li>Apartman / Site yönetim planı sureti ve varsa devam eden icra/dava dosyaları</li>
  <li>Kapıcı/güvenlik SGK dosyaları, bakım sözleşmeleri (asansör, hidrofor, jeneratör)</li>
  <li>Borçlu-alacaklı kesin hesap mizanı</li>
</ul>
<h5 class=""fw-bold mt-4"">3. Vergi Dairesinden Potansiyel Vergi Kimlik Numarası (VKN) &amp; Banka Hesabı</h5>
<p>Karar defterinin noter onaylı suretiyle Vergi Dairesi'ne giderek apartman/site adına <strong>Potansiyel Vergi Numarası</strong> alın. Aidatların kesinlikle yöneticinin şahsi IBAN'ına değil, apartman adına açılan <strong>kurumsal banka hesabına</strong> yatırılmasını sağlayın.</p>
<h5 class=""fw-bold mt-4"">4. İşletme Projesi (Bütçe) Hazırlayın ve Tebliğ Edin (KMK Md. 37)</h5>
<p>Yıllık gelir-gider bütçesini belirleyip kat maliklerine imza karşılığı veya taahhütlü/sistem üzerinden tebliğ edin. <strong>7 gün içinde itiraz edilmezse işletme projesi kesinleşir</strong> ve ödenmeyen aidatlar için yasal icra takibi yolu açılır.</p>
<h5 class=""fw-bold mt-4"">5. GMK360 Dijital İkizini Kurun</h5>
<p>Tüm daire/malik/kiracı listesini, arsa paylarını, işletme projesini ve ortak alan bakım takvimini sisteme yükleyerek şeffaf aidat takibi ve tek tıkla noter uyumlu karar arşivi başlatın.</p>"
            },
            new Article
            {
                Title = "Apartman & Site Karar Defteri Nasıl Tutulur? Noter, Yazıcı Çıktısı ve İmza Kuralları (KMK Md. 32-36)",
                Slug = "apartman-karar-defteri-nasil-tutulur",
                Category = PublicCategory,
                IsMembersOnly = false,
                IsPublished = true,
                PublishedAt = baseTime.AddHours(2),
                SeoTags = "karar defteri, noter tasdiki, müteharrik yaprak, yazıcı çıktısı yapıştırma, kapanış tasdiki",
                HtmlContent = @"
<p class=""lead"">634 sayılı Kat Mülkiyeti Kanunu'na (KMK Md. 32) göre Kat Malikleri Kurulu kararları, (1)'den başlayıp sırayla giden sayfa numaraları taşıyan ve <strong>her sayfası noter mührüyle tasdikli deftere</strong> yazılmak ve toplantıda bulunan bütün kat maliklerince imzalanmak zorundadır.</p>
<h5 class=""fw-bold mt-4"">1. Sadece Dijital ""e-Defter"" Yeterli midir?</h5>
<p><strong>Hayır.</strong> Anonim ve limited şirketlerdeki elektronik karar defteri uygulaması henüz apartman ve site yönetimleri için yürürlükte değildir. GMK360 gibi dijital sistemler oylamaları yönetir, taslak metinleri otomatik üretir ve dijital ikizi arşivler; ancak mahkeme ve icra dairelerinde tek bağlayıcı belge <strong>noter tasdikli fiziki defter + ıslak imzadır</strong>.</p>
<h5 class=""fw-bold mt-4"">2. Yazıcıdan Çıktı Alıp Deftere Yapıştırmak Geçerli midir?</h5>
<p>Kanunda el yazısı zorunluluğu yoktur. İki yöntem hukuken kabul edilir:</p>
<ul>
  <li><strong>Yöntem A — Klasik Ciltli Deftere Yapıştırma:</strong> Bilgisayardan çıktı alınıp sayfaya sağlamca yapıştırılabilir. <strong>Kritik Şart:</strong> Sonradan değiştirilme şüphesini önlemek için, atılan imzaların (en azından divan ve katılımcı imzalarının) <strong>yarısı çıktı kağıdının üzerinde, yarısı doğrudan defter yaprağına taşacak şekilde</strong> atılmalı ve defter köşesindeki noter mührü kapanmamalıdır.</li>
  <li><strong>Yöntem B — Müteharrik Yaprak (En Temiz Profesyonel Yol):</strong> Yıl başında 50 veya 100 adet numaralı A4 yaprak notere ""Müteharrik Yaprak Karar Defteri"" olarak onaylatılır. Karar alındıkça ilgili noter mühürlü A4 yaprağı yazıcıya konularak GMK360 çıktısı doğrudan üzerine basılır ve altına ıslak imza alınır.</li>
</ul>
<h5 class=""fw-bold mt-4"">3. Altın Yazım Kuralları</h5>
<table class=""table table-bordered table-sm mt-2"">
  <thead class=""table-light""><tr><th>Kural</th><th>Doğru Uygulama</th></tr></thead>
  <tbody>
    <tr><td><strong>Silinti / Daksil Yasağı</strong></td><td>Asla daksil sürmeyin veya sayfa yırtmayın. Hatalı satırın üzeri okunacak şekilde tek çizgiyle çizilip yanına doğrusu yazılır ve yönetici/divan tarafından <strong>paraf</strong> edilir.</td></tr>
    <tr><td><strong>Boşluk Bırakmama</strong></td><td>Satır ve paragraflar arasında sonradan cümle eklenebilecek büyük boşluklar bırakmayın; kalan boşlukları diyagonal çizgiyle kapatın.</td></tr>
    <tr><td><strong>Muhalif Şerhi</strong></td><td>Karara katılmayan malikler imza bölümüne <em>""Karara muhalifim — Gerekçe: ...""</em> şerhi yazarak imzalayabilir (bu şerh iptal davası hakkı açısından önemlidir).</td></tr>
    <tr><td><strong>Ocak Kapanış Tasdiki (Md. 36)</strong></td><td>Her takvim yılının bitimini takip eden 1 ay içinde (<strong>en geç Ocak ayı sonuna kadar</strong>) karar defteri notere götürülerek kapanış/görülmüştür tasdiki yaptırılmalıdır.</td></tr>
  </tbody>
</table>"
            },
            new Article
            {
                Title = "İşletme Projesi, Aidat Toplama, %5 Gecikme Tazminatı ve KVKK Kuralları (KMK Md. 20 & 37)",
                Slug = "aidat-belirleme-isletme-projesi-ve-icra",
                Category = PublicCategory,
                IsMembersOnly = false,
                IsPublished = true,
                PublishedAt = baseTime.AddHours(3),
                SeoTags = "aidat icra takibi, işletme projesi, yüzde 5 gecikme faizi, kvkk borçlu ifşası",
                HtmlContent = @"
<p class=""lead"">Site yönetimlerinde en çok uyuşmazlık yaşanan konu aidat belirleme, gecikme tazminatı ve ödeme yapmayan sakinlere uygulanacak yasal yaptırımlardır.</p>
<h5 class=""fw-bold mt-4"">1. Kesinleşmiş İşletme Projesi Olmadan İcra Olur mu?</h5>
<p>KMK Md. 37 uyarınca yönetim kurulunun veya yöneticinin hazırladığı yıllık <strong>İşletme Projesi</strong> kat maliklerine tebliğ edilir. Tebliğden itibaren <strong>7 gün içinde itiraz edilmezse</strong> proje kesinleşir ve İcra İflas Kanunu Md. 68/1 anlamında resmi belge sayılır. İtiraz varsa konu Kat Malikleri Kurulu'nda karara bağlanır.</p>
<h5 class=""fw-bold mt-4"">2. Aylık %5 Gecikme Tazminatı ve Müteselsil Sorumluluk (KMK Md. 20 &amp; 22)</h5>
<ul>
  <li>Gününde ödenmeyen aidat veya avans borcu için gün hesabı ile <strong>aylık %5 gecikme tazminatı</strong> uygulanır.</li>
  <li>Dairede kiracı oturuyorsa, kiracı aidattan <strong>kira bedeliyle sınırlı olmak üzere</strong> mülk sahibiyle birlikte müteselsilen sorumludur. Ancak kiracı ödemezse doğrudan kat malikine başvurulabilir; bu nedenle büyük demirbaş/tadilat borçları her zaman tapu sahibine borçlandırılmalıdır.</li>
</ul>
<div class=""alert alert-warning border-0 rounded-3 mt-3""><strong>KVKK Uyarısı (Çok Önemli!):</strong> Ödemeyen daire sahiplerinin isimlerini veya daire borçlarını apartman panosuna asmak, WhatsApp grubunda tüm siteyle paylaşmak ya da tüm sakinlere açık ekranda göstermek <strong>KVKK açısından kişisel veri ihlali ve suç teşkil edebilir</strong>. Borçlu listelerini yalnızca Yönetici, Yönetim Kurulu (Azalar) ve Denetim Kurulu görebilir.</div>"
            },
            new Article
            {
                Title = "Çok Bloklu Sitelerde Blok Gideri ile Site Ortak Gideri Nasıl Ayrılır? (KMK Md. 69)",
                Slug = "kat-mulkiyeti-blok-giderleri-kmk-69",
                Category = PublicCategory,
                IsMembersOnly = false,
                IsPublished = true,
                PublishedAt = baseTime.AddHours(4),
                SeoTags = "KMK 69, toplu yapı, blok gideri, çatı akıntısı, asansör masrafı",
                HtmlContent = @"
<p class=""lead"">5 bloklu bir sitede A Bloğun çatısı aktığında veya asansörü arızalandığında masrafı tüm site mi öder, yoksa sadece A Blok malikleri mi? Cevap <strong>Yönetim Planı</strong> ve <strong>KMK Madde 69</strong>'dadır.</p>
<h5 class=""fw-bold mt-4"">1. Ana Kural: Blok Ortak Gideri Sadece O Bloğa Aittir</h5>
<p>Toplu yapılarda (çok bloklu sitelerde) yalnızca tek bir bloğa özgülenmiş ortak yerlerin (o bloğun çatısı, dış cephesi, asansörü, merdiven otomatı ve hidroforu) bakım ve onarım giderleri kural olarak <strong>sadece o bloktaki kat malikleri</strong> tarafından karşılanır. Diğer bloklara borç çıkarılmaz ve sadece o bloğun kat malikleri karar/oylama sürecine katılır.</p>
<h5 class=""fw-bold mt-4"">2. Site Geneli Ortak Giderler</h5>
<p>Bahçe/peyzaj, güvenlik kulübesi, site içi yollar, ortak havuz, merkezi kazan dairesi veya blokların ortak oturduğu kapalı otopark bazası gibi tüm bloklara hizmet veren alanların giderleri tüm site malikleri arasında paylaştırılır.</p>
<h5 class=""fw-bold mt-4"">3. İstisna: Site Yönetim Planı Hükmü</h5>
<p>Tapuda tescilli Site Yönetim Planı'nda açıkça <em>""Tüm blokların çatı ve asansör yenileme giderleri site genel bütçesinden karşılanır""</em> şeklinde özel bir madde varsa, yönetim planı esas alınır.</p>"
            },

            // 2. ÜYELERE ÖZEL YÖNETİCİ EL KİTABI & YASAL PROSEDÜRLER (KİTAP BÖLÜMLERİ)
            new Article
            {
                Title = "Bölüm 1: Genel Kurul Çağrısı, 15 Gün Tebliğ Şartı, Yetersayı ve Vekâlet Kuralları (KMK Md. 29-31)",
                Slug = "genel-kurul-toplanti-usulleri-ve-vekalet",
                Category = HandbookCategory,
                IsMembersOnly = true,
                IsPublished = true,
                PublishedAt = baseTime.AddHours(5),
                SeoTags = "genel kurul çağrısı, 15 gün tebliğ, toplantı yetersayısı, vekalet sınırı",
                HtmlContent = @"
<p class=""lead"">Toplantıda alınan kararların mahkemede usul yönünden iptal edilmemesi için toplantı çağrısının, bekleme süresinin ve vekâlet sayılarının kanuna birebir uygun olması gerekir.</p>
<h5 class=""fw-bold mt-4"">1. Olağan ve Olağanüstü Toplantı Zamanı (Md. 29)</h5>
<ul>
  <li><strong>Olağan Toplantı:</strong> Yönetim planında yazılı ayda (genellikle Ocak ayında), tarih yazılı değilse her takvim yılının ilk ayı içinde yapılır.</li>
  <li><strong>Olağanüstü Toplantı:</strong> Önemli bir sebebin çıkması halinde yönetici, denetçi veya kat maliklerinin <strong>1/3'ünün istemi</strong> üzerine çağrı yapılır.</li>
</ul>
<h5 class=""fw-bold mt-4"">2. Çağrı Usulü ve 15 Gün Kuralı</h5>
<p>Olağanüstü toplantılarda toplantı tarihinden <strong>en az 15 gün önce</strong> bütün kat maliklerine imzalattırılacak bir çağrı belgesiyle veya taahhütlü mektupla bildirim zorunludur. Çağrıda gündem, tarih, saat, yer ve ilk toplantıda yeter sayı sağlanamazsa <strong>2. toplantının yeri ve saati</strong> mutlaka belirtilir. 1. ve 2. toplantı arasında <strong>en az 7 gün, en fazla 15 gün</strong> olmalıdır.</p>
<h5 class=""fw-bold mt-4"">3. Yetersayı ve Vekâlet Sınırı (KMK Md. 30-31)</h5>
<ul>
  <li><strong>1. Toplantı:</strong> Tüm kat maliklerinin hem <strong>sayı</strong> hem de <strong>arsa payı</strong> bakımından yarısından fazlası (%50+1) hazır bulunmalıdır.</li>
  <li><strong>2. Toplantı:</strong> Yeter sayı aranmaksızın toplantıya katılanlarla açılır ve kararlar katılanların salt çoğunluğuyla alınır (ancak kanunun özel nitelikli çoğunluk aradığı 4/5 ve oy birliği maddeleri saklıdır).</li>
  <li><strong>Vekâlet Kısıtı (Çok Önemli):</strong> Bir kişi, oy sayısının <strong>%5'inden fazlasını</strong> kullanmak üzere vekil tayin edilemez. <strong>40 ve daha az</strong> bağımsız bölümü olan yapılarda ise bir kişi <strong>en fazla 2 kişiye</strong> vekâlet edebilir!</li>
</ul>"
            },
            new Article
            {
                Title = "Bölüm 2: Konulara Göre Yasal Çoğunluk Tablosu & Birebir Örnek Karar Metinleri",
                Slug = "kmk-konulara-gore-oy-oranlari-ve-ornek-kararlar",
                Category = HandbookCategory,
                IsMembersOnly = true,
                IsPublished = true,
                PublishedAt = baseTime.AddHours(6),
                SeoTags = "örnek kararlar, oy oranları, çoğunluk tablosu, KMK madde 30, madde 42, madde 28",
                HtmlContent = @"
<p class=""lead"">Aşağıdaki tablo, hangi konuda kaç oy gerektiğini ve karar defterine yazacağınız hukuken sağlam örnek metinleri içerir.</p>
<h5 class=""fw-bold mt-3"">Yasal Oy Oranları Matrisi</h5>
<table class=""table table-bordered table-sm align-middle"">
  <thead class=""table-dark""><tr><th>Karar Konusu</th><th>Dayanak Madde</th><th>Gerekli Çoğunluk</th></tr></thead>
  <tbody>
    <tr><td>Yönetici ve Denetçi Seçimi</td><td>KMK Md. 34 &amp; 41</td><td><strong>Sayı ve Arsa Payı Çoğunluğu (&gt;%50)</strong></td></tr>
    <tr><td>İşletme Projesi, Aidat Belirleme, Normal Bakım</td><td>KMK Md. 30 &amp; 37</td><td><strong>Salt Çoğunluk (&gt;%50)</strong></td></tr>
    <tr><td>Faydalı Yenilik (Mantolama, Isı Yalıtım, Kamera, Güneş Enerjisi)</td><td>KMK Md. 42</td><td><strong>Sayı ve Arsa Payı Çoğunluğu (&gt;%50)</strong></td></tr>
    <tr><td>Dış Cephe Rengi, Mimari Proje Değişikliği, Sabit Tesis</td><td>KMK Md. 19</td><td><strong>Tüm Maliklerin 4/5 Çoğunluğu (%80)</strong></td></tr>
    <tr><td>Yönetim Planı Değişikliği</td><td>KMK Md. 28</td><td><strong>Tüm Maliklerin 4/5 Çoğunluğu (%80)</strong></td></tr>
    <tr><td>Kat İlavesi, Bağımsız Bölüm Bölünmesi, Temlik/Devir</td><td>KMK Md. 44 &amp; 45</td><td><strong>Tüm Maliklerin Oy Birliği (%100)</strong></td></tr>
  </tbody>
</table>
<h5 class=""fw-bold mt-4"">Örnek Karar 1 — Yönetici Seçimi ve Banka/İcra Yetkisi</h5>
<div class=""p-3 bg-light border rounded-3 small"">
  <em>""Kat Malikleri Kurulumuz .../.../2026 tarihinde salt çoğunlukla toplanarak gündem maddelerini görüşmüştür. Yapılan oylama sonucunda KMK Md. 34 uyarınca sayı ve arsa payı çoğunluğu sağlanarak Daire ... maliki [Adı Soyadı] 1 (bir) yıl süreyle yönetici; [Adı Soyadı] ise denetçi olarak seçilmiştir. Yöneticiye; site adına banka hesabı açmaya, internet bankacılığı kullanmaya, vergi/SGK işlemlerini yürütmeye ve ödemeyen malik/kiracılar hakkında icra takibi ile dava açmaya tam yetki verilmesine oy çokluğu/oy birliği ile karar verilmiştir.""</em>
</div>
<h5 class=""fw-bold mt-4"">Örnek Karar 2 — Aylık Aidat ve İşletme Bütçesi Belirlenmesi</h5>
<div class=""p-3 bg-light border rounded-3 small"">
  <em>""2026 yılı İşletme Projesi kapsamında aylık toplam gider bütçesi dikkate alınarak, 01.01.2026 tarihinden geçerli olmak üzere daire başına aylık işletme aidatının 1.500,00 TL (Binbeşyüz Türk Lirası), kapıcı kıdem tazminatı/demirbaş fonunun ise malik başına aylık 200,00 TL olarak tahakkuk ettirilmesine; son ödeme tarihini geçen borçlara KMK Md. 20/c gereği aylık %5 gecikme tazminatı uygulanmasına karar verilmiştir.""</em>
</div>
<h5 class=""fw-bold mt-4"">Örnek Karar 3 — Sadece Tek Bloğu İlgilendiren Tadilat (KMK Md. 69)</h5>
<div class=""p-3 bg-light border rounded-3 small"">
  <em>""Site Yönetim Planı'nda aksi hüküm bulunmadığından KMK Md. 69 uyarınca A Blok çatı akıntısı ve izolasyon onarım işinin [Firma Adı]'na KDV dahil 85.000,00 TL bedelle yaptırılmasına; bu demirbaş giderinin yalnızca A Blok kat maliklerinden eşit / arsa payı oranında 2 taksitte tahsil edilmesine A Blok kat maliklerince karar verilmiştir.""</em>
</div>"
            },
            new Article
            {
                Title = "Bölüm 3: Karar Defteri Sayfası ve İhtarname Formları Nasıl Doldurulur? (Adım Adım Örnek)",
                Slug = "karar-defteri-formu-nasil-doldurulur",
                Category = HandbookCategory,
                IsMembersOnly = true,
                IsPublished = true,
                PublishedAt = baseTime.AddHours(7),
                SeoTags = "form nasıl doldurulur, karar defteri sayfası, başkan azalar, esas evrak no, ihtarname örneği",
                HtmlContent = @"
<p class=""lead"">Resmi noter karar defterinin her sayfası, üstteki evrak künyesinden alttaki imza bloklarına kadar standart bir düzen taşır. GMK360 Karar Defteri modülü bu yapının birebir dijital ikizidir.</p>
<h5 class=""fw-bold mt-4"">1. Karar Defteri Sayfası Alanlarının Anlamı</h5>
<ul>
  <li><strong>Sayfa No (Sağ/Sol Üst Köşe):</strong> Noterin mühürlediği sıra numarasıdır. Sistemde ve çıktıda asla atlanmaz.</li>
  <li><strong>Karar Sıra No:</strong> Yıl + sıra formatında tutulur (Örn: <code>2026/01</code>, <code>2026/02</code>).</li>
  <li><strong>Karara Esas Olan Evrak (Tarih / No / Nereden Geldiği):</strong> Bu karar bir dilekçeye, firma teklifine veya resmi tebligata dayanıyorsa onun bilgisi yazılır (Örn: <code>14.11.2026 | TEK-04 | Özkan Çatı İzolasyon</code>).</li>
  <li><strong>Mevzuun Mahiyeti ve Hülâsası:</strong> Gündemin madde madde kısa özeti yazılır (1. Aidat artışı, 2. Çatı onarımı vb.).</li>
  <li><strong>Başkanın ve Azaların Adı Soyadı:</strong> Divan/Yönetim Kurulu Başkanı ile hazır bulunan azaların/üyelerin tam adları yazılır.</li>
  <li><strong>Toplantı Başlangıç &amp; Bitiş Saati:</strong> Karar metninin üstünde açılış saati, son satırda ise <em>""Toplantı saat ...'da sona erdi""</em> ibaresi yer alır.</li>
  <li><strong>Katılımcı Listesi (Hazirun) &amp; İmzalar:</strong> Toplantıya bizzat veya vekaleten katılanların adları liste halinde alt kısma açılır ve karşısına ıslak imza alınır.</li>
</ul>
<h5 class=""fw-bold mt-4"">2. Aidat Gecikmesi İçin Örnek Resmi İhtarname Formu</h5>
<div class=""p-3 bg-light border rounded-3 small"">
  <strong>KONU:</strong> Birikmiş Aidat / Ortak Gider Borcunun Ödenmesi İhtarı (KMK Md. 20 &amp; 37)<br/>
  <strong>MUHATAP:</strong> Sayın [Kat Maliki / Kiracı Adı Soyadı] — ... Blok Daire No: ...<br/><br/>
  Yönetim kurulumuzun ... tarihli ve .../... sayılı kesinleşmiş işletme projesi uyarınca tarafınıza tahakkuk eden <strong>... dönemlerine ait toplam ... TL asıl aidat alacağı</strong> ile KMK Md. 20 gereği işleyen %5 gecikme tazminatı henüz banka hesabımıza yatırılmamıştır.<br/>
  İşbu ihtarın tebliğinden itibaren <strong>7 (yedi) gün</strong> içinde apartman/site banka hesabına (TR...) ödeme yapmanızı; aksi halde hakkınızda ilamsız icra takibi başlatılacağını ve yasal vekalet/takip masraflarının tarafınıza yansıtılacağını ihtaren bildiririz.
</div>"
            },
            new Article
            {
                Title = "Bölüm 4: Kasa, Fatura Belgeleme, Kıdem Tazminatı Fonu ve Denetim Kurulu Raporu (Md. 36-41)",
                Slug = "denetim-kurulu-gider-belgeleme-ve-personel",
                Category = HandbookCategory,
                IsMembersOnly = true,
                IsPublished = true,
                PublishedAt = baseTime.AddHours(8),
                SeoTags = "denetim kurulu, gider faturası, kıdem tazminatı fonu, kapıcı SGK, ibra",
                HtmlContent = @"
<p class=""lead"">Yönetimin yıl sonunda Kat Malikleri Kurulu tarafından <strong>ibra edilebilmesi (aklanması)</strong> için tüm harcamaların belgeli olması ve denetim kurulunun düzenli rapor hazırlaması gerekir.</p>
<h5 class=""fw-bold mt-4"">1. Fatura, Makbuz ve Karar İlişkisi</h5>
<ul>
  <li>Yönetim harcamaları mutlaka <strong>e-Arşiv/e-Fatura, perakende satış fişi veya gider pusulası</strong> ile belgelenmelidir.</li>
  <li>Yüksek tutarlı demirbaş veya plan dışı harcamalarda faturanın üzerine veya GMK360 Muhasebe modülüne <strong>ilgili Karar Defteri Sayfa ve Karar No</strong> bilgisinin bağlanması denetimde yöneticinin elini güçlendirir.</li>
  <li>Tüm fatura ve dekontlar kanunen <strong>en az 5 yıl</strong> (SGK özlük dosyaları 10 yıl) saklanmalıdır.</li>
</ul>
<h5 class=""fw-bold mt-4"">2. Kapıcı / Site Personeli ve Kıdem Tazminatı Fonu</h5>
<p>Site veya apartmanda sigortalı personel çalışıyorsa SGK aylık bildirgeleri zamanında verilmeli, her ay kıdem tazminatı karşılığı ayrılmalıdır. Yargıtay uygulamalarına göre <strong>personel kıdem tazminatı ve yıllık izin/demirbaş fonları mülk sahiplerine (kat maliklerine)</strong>, aylık maaş ve SGK cari giderleri ise işletme gideri olarak oturanlara yansıtılır.</p>
<h5 class=""fw-bold mt-4"">3. Denetçinin Görevleri (KMK Md. 41)</h5>
<p>Denetçi (veya Denetim Kurulu), yönetim planında süre yazmıyorsa <strong>en az 3 ayda bir</strong> yönetimin hesap ve defterlerini inceler; haklı bir sebep varsa sıklaştırabilir ve takvim yılı sonunda Kat Malikleri Kurulu'na yazılı <strong>Denetim Raporu</strong> sunar.</p>"
            },
            new Article
            {
                Title = "Bölüm 5: Kiracı & Mülk Sahibi Hakları, TÜFE Kira Artışı, 2 Haklı İhtar ve Tahliye Rehberi (TBK)",
                Slug = "kiraci-malik-haklari-tufe-ve-tahliye",
                Category = HandbookCategory,
                IsMembersOnly = true,
                IsPublished = true,
                PublishedAt = baseTime.AddHours(9),
                SeoTags = "kiracı hakları, TÜFE kira artışı, tahliye taahhütnamesi, iki haklı ihtar, zorunlu arabuluculuk",
                HtmlContent = @"
<p class=""lead"">Site yönetimi sadece ortak alanlardan değil; malik-kiracı arasındaki gider paylaşımı, aidat borcundan doğan sorumluluk ve kira/tahliye süreçlerindeki temel kurallardan da doğrudan etkilenir.</p>
<h5 class=""fw-bold mt-4"">1. Hangi Masraf Kiracının, Hangisi Ev Sahibinindir?</h5>
<ul>
  <li><strong>Kiracı (Oturan):</strong> Temizlik, ortak elektrik-su-doğalgaz, personel maaş/SGK, güvenlik, bahçe bakım ve rutin asansör bakım ücretleri gibi günlük <strong>İşletme Giderleri</strong>.</li>
  <li><strong>Kat Maliki (Ev Sahibi):</strong> Çatı yenileme, dış cephe mantolama, asansör motor değişimi, bina sigortası ve personel kıdem tazminatı fonu gibi <strong>Demirbaş ve Yatırım Giderleri</strong>.</li>
</ul>
<h5 class=""fw-bold mt-4"">2. Kira Artışı ve Yasal Bildirim (TBK Md. 344)</h5>
<p>Konut ve çatılı iş yeri kiralarında artış tavanı bir önceki kira yılının <strong>12 aylık TÜFE ortalamasını</strong> geçemez. Sözleşmede özel artış hükmü bulunmuyorsa artış talebinin yeni dönemden önce kiracıya yazılı bildirilmesi gerekir.</p>
<h5 class=""fw-bold mt-4"">3. İki Haklı İhtar ve Tahliye Taahhütnamesi Kuralları (TBK Md. 352)</h5>
<ul>
  <li><strong>Tahliye Taahhütnamesi:</strong> Geçerli olabilmesi için mutlaka <strong>kiralananın tesliminden (kira sözleşmesi başlangıcından) sonraki bir tarihte</strong> düzenlenip imzalanmış olması gerekir; aynı gün imzalanan taahhütler mahkemede geçersiz sayılabilir.</li>
  <li><strong>İki Haklı İhtar:</strong> Kiracı bir kira yılı içinde iki farklı aya ait kirayı ödemeyip yazılı iki haklı ihtara muhatap kalırsa, kira yılının bitiminden itibaren <strong>1 ay içinde</strong> tahliye davası açılabilir.</li>
  <li><strong>Zorunlu Arabuluculuk:</strong> Kira ve kat mülkiyeti uyuşmazlıklarının büyük çoğunluğunda dava açmadan önce <strong>arabulucuya başvuru dava şartıdır</strong>.</li>
</ul>"
            },
            new Article
            {
                Title = "Binalarda Acil Durum Toplanma Alanı, Yangın Güvenliği ve Sığınak Yükümlülükleri",
                Slug = "bina-acil-durum-toplanma-ve-yangin-yonetmeligi",
                Category = PublicCategory,
                IsMembersOnly = false,
                IsPublished = true,
                PublishedAt = baseTime.AddHours(10),
                SeoTags = "acil durum toplanma alanı, yangın tüpü dolumu, yangın dolabı hortumu, sığınak yönetmeliği, ana doğalgaz vanası",
                HtmlContent = @"
<p class=""lead"">Deprem, yangın ve su/gaz kaçağı gibi acil anlarda bina sakinlerinin ilk 60 saniyede doğru yere yönelmesi hayati önem taşır. <strong>Binaların Yangından Korunması Hakkında Yönetmelik</strong> ve <strong>Sığınak Yönetmeliği</strong> uyarınca yöneticilerin temel güvenlik bilgilendirmelerini görünür kılması gerekir.</p>
<h5 class=""fw-bold mt-4"">1. Her Sakinin Bilmesi Gereken 4 Kritik Nokta</h5>
<ul>
  <li><strong>📍 Acil Durum Toplanma Alanı:</strong> Deprem veya tahliye sonrası site bahçesinde binaların yıkılma/cam düşme mesafesi dışında kalan açık alan veya belediye/AFAD mahalle toplanma noktası.</li>
  <li><strong>🧯 Yangın Dolabı, Hortum &amp; Tüpler:</strong> Her kat sahanlığındaki yangın dolapları, kuru tozlu (KKT) yangın tüplerinin yeri ve yıllık dolum/basınç kontrolleri (basınç göstergesi yeşil bölgede olmalı).</li>
  <li><strong>🔧 Ana Doğalgaz, Su Vanası &amp; Elektrik Şalteri:</strong> Giriş cephesindeki sarı ana gaz kesme kutusu ve bodrumdaki ana su/elektrik panosunun yeri kapıcı ve tüm kat sakinlerince bilinmelidir.</li>
  <li><strong>🚪 Sığınak &amp; Kaçış Merdivenleri:</strong> Sığınak alanları özel depo/hurdalık olarak kilitli bırakılmamalı; yangın çıkış kapıları hiçbir zaman kilitlenmemeli ve önüne eşya yığılmamalıdır.</li>
</ul>
<div class=""alert alert-danger border-0 rounded-3 mt-3""><strong>GMK360 Kolaylığı:</strong> Bina detayından tanımladığınız toplanma yeri, yangın dolabı/hortumu ve ana vana bilgileri tüm sakinlerin telefonundaki <code>/FacilityPortal</code> ana ekranında sabit <em>""Acil Durum &amp; Güvenlik Noktaları""</em> kartı olarak görünür.</div>"
            }
        };
    }
}
