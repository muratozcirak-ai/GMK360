using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GMK360.Web.Models;

namespace GMK360.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult KurucuOfis()
    {
        return View();
    }

    
        [HttpGet("bina-yonetimi")]
        public IActionResult BinaYonetimi() => View();

        [HttpGet("dijital-evim")]
        public IActionResult DijitalEvim() => View();

        [HttpGet("usta-hizmetler")]
        public IActionResult UstaHizmetler() => View();

        [HttpGet("gunluk-kiralama")]
        public IActionResult GunlukKiralama() => View();

        public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult EvimiDegerlendir()
    {
        return View();
    }

    // --- GMK360 & GMK360 Katılım Hunisi Actions ---
    public IActionResult EvSahibiKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "Ev Sahibi",
        ThemeColor = "#8b5cf6",
        HeroTitle = "Mülkünüzü ve Finansal Süreçlerinizi Tek Ekrandan Yönetin",
        HeroSubtitle = "Kiralarınızı takip edin, tadilatlarınızı planlayın ve vergilerinizi kolayca hesaplayın. Dijital mülk yönetimi artık çok kolay.",
        HeroImage = "https://images.unsplash.com/photo-1560518883-ce09059eeffa?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        Features = new List<FeatureItem> {
            new FeatureItem { IconClass = "fas fa-chart-line", Title = "Gelir Takibi", Description = "Kira gelirlerinizi grafikler üzerinden anlık takip edin." },
            new FeatureItem { IconClass = "fas fa-hammer", Title = "Tadilat Planlama", Description = "Evinizdeki bakım ve onarımlar için anında usta bulun." },
            new FeatureItem { IconClass = "fas fa-file-invoice-dollar", Title = "Vergi Yönetimi", Description = "Kira vergilerinizi otomatik hesaplayın ve beyanname hatırlatıcıları kurun." }
        }
    });

    public IActionResult KiraciKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "Kiracı",
        ThemeColor = "#3b82f6",
        HeroTitle = "Kira ve Aidat Ödemelerinizi Düzenli Tutun",
        HeroSubtitle = "Ev sahibi ile dijital kontrat imzalayın, ödemelerinizi takip edin ve arıza taleplerinizi anında iletin.",
        HeroImage = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        Features = new List<FeatureItem> {
            new FeatureItem { IconClass = "fas fa-credit-card", Title = "Online Ödeme", Description = "Kira ve aidat ödemelerinizi tek tıkla güvenle yapın." },
            new FeatureItem { IconClass = "fas fa-tools", Title = "Arıza Talebi", Description = "Evinizdeki sorunları ev sahibine dijital ortamda fotoğrafla iletin." },
            new FeatureItem { IconClass = "fas fa-file-contract", Title = "Dijital Kontrat", Description = "Kira sözleşmenizi e-imza ile imzalayıp güvenle saklayın." }
        }
    });

    public IActionResult BireyselIlanSahibiKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "Bireysel İlan Sahibi",
        ThemeColor = "#06b6d4",
        HeroTitle = "Mülkünüzü Aracı Olmadan Doğrudan Satın veya Kiralayın",
        HeroSubtitle = "İlanınızı ücretsiz oluşturun, alıcı ve kiracılarla doğrudan iletişime geçerek komisyon ödemeyin.",
        HeroImage = "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        Features = new List<FeatureItem> {
            new FeatureItem { IconClass = "fas fa-bullhorn", Title = "Kolay İlan Yönetimi", Description = "Fotoğraf ve detayları ekleyerek ilanınızı saniyeler içinde yayına alın." },
            new FeatureItem { IconClass = "fas fa-comments", Title = "Doğrudan İletişim", Description = "Platform içi mesajlaşma ile potansiyel müşterilerle pazarlık yapın." },
            new FeatureItem { IconClass = "fas fa-tags", Title = "Sıfır Komisyon", Description = "Satış veya kiralama işlemlerinizi tamamen masrafsız tamamlayın." }
        }
    });

    public IActionResult BinaYoneticisiKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "Bina Yöneticisi",
        ThemeColor = "#10b981",
        HeroTitle = "Bina Bütçesini ve Operasyonları Şeffaf Yönetin",
        HeroSubtitle = "Aidat toplamayı otomatikleştirin, gider pusulalarını dijitalleştirin ve bina sakinleriyle kolayca iletişim kurun.",
        HeroImage = "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        Features = new List<FeatureItem> {
            new FeatureItem { IconClass = "fas fa-wallet", Title = "Otomatik Aidat", Description = "Sakinlere otomatik SMS göndererek aidat tahsilatını hızlandırın." },
            new FeatureItem { IconClass = "fas fa-chart-pie", Title = "Gider Takibi", Description = "Asansör, temizlik ve bakım giderlerinizi anlık olarak raporlayın." },
            new FeatureItem { IconClass = "fas fa-bullhorn", Title = "Duyuru Sistemi", Description = "Bina toplantı kararlarını ve duyuruları tüm apartmanla dijital paylaşın." }
        }
    });

    public IActionResult SiteYoneticisiKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "Site Yöneticisi",
        ThemeColor = "#059669",
        HeroTitle = "Büyük Site Operasyonlarınızı Dijitalleştirin",
        HeroSubtitle = "Güvenlik, peyzaj, personel yönetimi ve sosyal tesis rezervasyonlarını tek platformdan kusursuz yönetin.",
        HeroImage = "https://images.unsplash.com/photo-1494526585095-c41746248156?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        Features = new List<FeatureItem> {
            new FeatureItem { IconClass = "fas fa-shield-alt", Title = "Güvenlik Entegrasyonu", Description = "Misafir araç kayıtları ve yaya girişlerini sistem üzerinden takip edin." },
            new FeatureItem { IconClass = "fas fa-users", Title = "Personel Yönetimi", Description = "Bahçıvan, temizlik ve teknik ekibinizin mesailerini kontrol edin." },
            new FeatureItem { IconClass = "fas fa-swimming-pool", Title = "Tesis Rezervasyonu", Description = "Havuz, tenis kortu gibi ortak alanlar için randevu sistemi sunun." }
        }
    });

    public IActionResult UstaKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "Usta",
        ThemeColor = "#f59e0b",
        HeroTitle = "Çevrenizdeki İş Fırsatlarını Anında Yakalayın",
        HeroSubtitle = "Tadilat, onarım ve boya badana taleplerine anında teklif verin, iş ağınızı güvenle büyütün.",
        HeroImage = "https://images.unsplash.com/photo-1504307651254-35680f356f12?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        Features = new List<FeatureItem> {
            new FeatureItem { IconClass = "fas fa-bell", Title = "Anlık İş Bildirimi", Description = "Bölgenizdeki boya, tesisat veya montaj taleplerini anında görün." },
            new FeatureItem { IconClass = "fas fa-handshake", Title = "Hızlı Teklif Verme", Description = "İşverenlere online fiyat teklifi ileterek işleri kolayca bağlayın." },
            new FeatureItem { IconClass = "fas fa-star", Title = "Puan ve Yorum Sistemi", Description = "Yaptığınız başarılı işlerle puanınızı artırıp profilinizi öne çıkarın." }
        }
    });

    public IActionResult EsnafKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "Hizmet Sağlayıcı",
        ThemeColor = "#ea580c",
        HeroTitle = "Sitenizin ve Binanızın Çözüm Ortakları",
        HeroSubtitle = "Temizlik, taşımacılık, ilaçlama, peyzaj veya güvenlik... Milyonlarca müşteriye ve site yönetimine profesyonel hizmet sunun.",
        HeroImage = "https://images.unsplash.com/photo-1581578731548-c64695cc6952?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        Features = new List<FeatureItem> {
            new FeatureItem { IconClass = "fas fa-broom", Title = "Hizmet Odaklı Profil", Description = "Verdiğiniz hizmetleri (Temizlik, Nakliye vb.), referanslarınızı ve sertifikalarınızı sergileyin." },
            new FeatureItem { IconClass = "fas fa-handshake", Title = "Yönetimlerle Doğrudan İş", Description = "Bina ve site yönetimlerinden gelen periyodik bakım/temizlik taleplerine anında teklif verin." },
            new FeatureItem { IconClass = "fas fa-star", Title = "Güven ve İtibar", Description = "Müşteri yorumları ile sektördeki puanınızı yükseltip daha fazla düzenli iş alın." }
        }
    });

    public IActionResult GunlukKiralayanKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "Günlük Kiralayan",
        ThemeColor = "#ec4899",
        HeroTitle = "Tatil Evlerinizi Yüksek Kârla, Sıfır Komisyonla Yönetin",
        HeroSubtitle = "Villanızı, yazlığınızı veya stüdyo dairenizi doğrudan misafirlerle buluşturun, rezervasyon takvimini dijitalden yönetin.",
        HeroImage = "https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        Features = new List<FeatureItem> {
            new FeatureItem { IconClass = "fas fa-calendar-check", Title = "Takvim Yönetimi", Description = "Farklı platformlardaki takvimlerinizi tek merkezde eşitleyin (iCal)." },
            new FeatureItem { IconClass = "fas fa-percent", Title = "Sıfır Komisyon", Description = "Kiralama işlemlerinden komisyon ödemeden gelirinizi artırın." },
            new FeatureItem { IconClass = "fas fa-id-card", Title = "KBS Entegrasyonu", Description = "Emniyet Kimlik Bildirim Sistemi'ne verileri otomatik yollayın." }
        }
    });

    public IActionResult EmlakOfisiKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "Emlak Ofisi",
        ThemeColor = "#2563eb",
        HeroTitle = "Kurumsal Emlak Ofisinizi Dijitale Taşıyın",
        HeroSubtitle = "Tüm danışmanlarınızı, yetki belgelerinizi, ilan portföyünüzü ve müşteri havuzunuzu tek panelden yönetin.",
        HeroImage = "https://images.unsplash.com/photo-1552581234-26160f608093?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        Features = new List<FeatureItem> {
            new FeatureItem { IconClass = "fas fa-users-cog", Title = "Danışman Yönetimi", Description = "Alt danışmanlarınıza yetki verin, performanslarını anlık ölçün." },
            new FeatureItem { IconClass = "fas fa-folder-open", Title = "Portföy Havuzu", Description = "Tüm ilanları ofis çatısı altında toplayın ve hızlıca paylaşın." },
            new FeatureItem { IconClass = "fas fa-file-signature", Title = "Yasal Sözleşmeler", Description = "Taşınmaz ticareti yetki sözleşmelerinizi e-imza ile dijitalde tutun." }
        }
    });

    public IActionResult EmlakDanismaniKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "Emlak Danışmanı",
        ThemeColor = "#8b5cf6",
        HeroTitle = "Yeni Nesil Araçlarla Emlak Satışlarınızı Katlayın",
        HeroSubtitle = "Yapay zeka destekli gayrimenkul değerleme, müşteri eşleştirme (Lead) ve akıllı CRM araçları elinizin altında.",
        HeroImage = "https://images.unsplash.com/photo-1573496799515-eebbb63814f2?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        Features = new List<FeatureItem> {
            new FeatureItem { IconClass = "fas fa-robot", Title = "Yapay Zeka Değerleme", Description = "Bölge bazlı istatistiklerle saniyeler içinde fiyat raporu oluşturun." },
            new FeatureItem { IconClass = "fas fa-address-book", Title = "Akıllı CRM", Description = "Alıcı ve satıcı veri tabanınızı tutun, görüşme geçmişini kaydedin." },
            new FeatureItem { IconClass = "fas fa-magic", Title = "Lead (Müşteri) Eşleştirme", Description = "Sistemdeki kiralık/satılık arayan talepleriyle portföyünüzü otomatik eşleyin." }
        }
    });

    public IActionResult KurumsalFirmaKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "İnşaat / Kurumsal Firma",
        ThemeColor = "#475569",
        HeroTitle = "Sektörün Zirvesine Çıkın",
        HeroSubtitle = "Kendi alt alan adınız (subdomain), kendi logonuz ve kurumsal renklerinizle dijital mağazanızı dakikalar içinde kurun. Müşterilerinizi sadece kendi ilanlarınıza odaklayın.",
        HeroImage = "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        RegisterUrl = "/Account/RegisterCorporate",
        Features = new List<FeatureItem>
        {
            new FeatureItem { IconClass = "fas fa-store", Title = "White-Label Mağaza", Description = "Size özel tasarlanmış tamamen kurumsal bir arayüz." },
            new FeatureItem { IconClass = "fas fa-link", Title = "Size Özel Subdomain", Description = "firmaadi.gmk360.com adresi ile itibarınızı artırın." },
            new FeatureItem { IconClass = "fas fa-bullseye", Title = "Müşteri İzolasyonu", Description = "Ziyaretçileriniz rakip ilanları veya esnafları görmez, tamamen size odaklanır." },
            new FeatureItem { IconClass = "fas fa-infinity", Title = "Sınırsız Vitrin", Description = "Sınırsız sayıda ilan ve proje ekleme imkanı." }
        }
    });


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("Home/Error/{statusCode}")]
    public IActionResult Error(int? statusCode = null)
    {
        if (statusCode.HasValue)
        {
            if (statusCode.Value == 404)
            {
                return View("Error404");
            }
            if (statusCode.Value == 500)
            {
                return View("Error500");
            }
        }
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
