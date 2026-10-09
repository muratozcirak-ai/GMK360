using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GMK360.Web.Models;

namespace GMK360.Web.Controllers;

public class HomeController : Controller
{
            public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Index() {
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

    // --- GMK360 & GMK360 KatÄ±lÄ±m Hunisi Actions ---
    public IActionResult EvSahibiKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "Ev Sahibi",
        ThemeColor = "#8b5cf6",
        HeroTitle = "MÃ¼lkÃ¼nÃ¼zÃ¼ ve Finansal SÃ¼reÃ§lerinizi Tek Ekrandan YÃ¶netin",
        HeroSubtitle = "KiralarÄ±nÄ±zÄ± takip edin, tadilatlarÄ±nÄ±zÄ± planlayÄ±n ve vergilerinizi kolayca hesaplayÄ±n. Dijital mÃ¼lk yÃ¶netimi artÄ±k Ã§ok kolay.",
        HeroImage = "https://images.unsplash.com/photo-1560518883-ce09059eeffa?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        Features = new List<FeatureItem> {
            new FeatureItem { IconClass = "fas fa-chart-line", Title = "Gelir Takibi", Description = "Kira gelirlerinizi grafikler Ã¼zerinden anlÄ±k takip edin." },
            new FeatureItem { IconClass = "fas fa-hammer", Title = "Tadilat Planlama", Description = "Evinizdeki bakÄ±m ve onarÄ±mlar iÃ§in anÄ±nda usta bulun." },
            new FeatureItem { IconClass = "fas fa-file-invoice-dollar", Title = "Vergi YÃ¶netimi", Description = "Kira vergilerinizi otomatik hesaplayÄ±n ve beyanname hatÄ±rlatÄ±cÄ±larÄ± kurun." }
        }
    });

    public IActionResult KiraciKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "KiracÄ±",
        ThemeColor = "#3b82f6",
        HeroTitle = "Kira ve Aidat Ã–demelerinizi DÃ¼zenli Tutun",
        HeroSubtitle = "Ev sahibi ile dijital kontrat imzalayÄ±n, Ã¶demelerinizi takip edin ve arÄ±za taleplerinizi anÄ±nda iletin.",
        HeroImage = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        Features = new List<FeatureItem> {
            new FeatureItem { IconClass = "fas fa-credit-card", Title = "Online Ã–deme", Description = "Kira ve aidat Ã¶demelerinizi tek tÄ±kla gÃ¼venle yapÄ±n." },
            new FeatureItem { IconClass = "fas fa-tools", Title = "ArÄ±za Talebi", Description = "Evinizdeki sorunlarÄ± ev sahibine dijital ortamda fotoÄŸrafla iletin." },
            new FeatureItem { IconClass = "fas fa-file-contract", Title = "Dijital Kontrat", Description = "Kira sÃ¶zleÅŸmenizi e-imza ile imzalayÄ±p gÃ¼venle saklayÄ±n." }
        }
    });

    public IActionResult BireyselIlanSahibiKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "Bireysel Ä°lan Sahibi",
        ThemeColor = "#06b6d4",
        HeroTitle = "MÃ¼lkÃ¼nÃ¼zÃ¼ AracÄ± Olmadan DoÄŸrudan SatÄ±n veya KiralayÄ±n",
        HeroSubtitle = "Ä°lanÄ±nÄ±zÄ± Ã¼cretsiz oluÅŸturun, alÄ±cÄ± ve kiracÄ±larla doÄŸrudan iletiÅŸime geÃ§erek komisyon Ã¶demeyin.",
        HeroImage = "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        Features = new List<FeatureItem> {
            new FeatureItem { IconClass = "fas fa-bullhorn", Title = "Kolay Ä°lan YÃ¶netimi", Description = "FotoÄŸraf ve detaylarÄ± ekleyerek ilanÄ±nÄ±zÄ± saniyeler iÃ§inde yayÄ±na alÄ±n." },
            new FeatureItem { IconClass = "fas fa-comments", Title = "DoÄŸrudan Ä°letiÅŸim", Description = "Platform iÃ§i mesajlaÅŸma ile potansiyel mÃ¼ÅŸterilerle pazarlÄ±k yapÄ±n." },
            new FeatureItem { IconClass = "fas fa-tags", Title = "SÄ±fÄ±r Komisyon", Description = "SatÄ±ÅŸ veya kiralama iÅŸlemlerinizi tamamen masrafsÄ±z tamamlayÄ±n." }
        }
    });

    public IActionResult BinaYoneticisiKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "Bina YÃ¶neticisi",
        ThemeColor = "#10b981",
        HeroTitle = "Bina BÃ¼tÃ§esini ve OperasyonlarÄ± Åeffaf YÃ¶netin",
        HeroSubtitle = "Aidat toplamayÄ± otomatikleÅŸtirin, gider pusulalarÄ±nÄ± dijitalleÅŸtirin ve bina sakinleriyle kolayca iletiÅŸim kurun.",
        HeroImage = "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        Features = new List<FeatureItem> {
            new FeatureItem { IconClass = "fas fa-wallet", Title = "Otomatik Aidat", Description = "Sakinlere otomatik SMS gÃ¶ndererek aidat tahsilatÄ±nÄ± hÄ±zlandÄ±rÄ±n." },
            new FeatureItem { IconClass = "fas fa-chart-pie", Title = "Gider Takibi", Description = "AsansÃ¶r, temizlik ve bakÄ±m giderlerinizi anlÄ±k olarak raporlayÄ±n." },
            new FeatureItem { IconClass = "fas fa-bullhorn", Title = "Duyuru Sistemi", Description = "Bina toplantÄ± kararlarÄ±nÄ± ve duyurularÄ± tÃ¼m apartmanla dijital paylaÅŸÄ±n." }
        }
    });

    public IActionResult SiteYoneticisiKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "Site YÃ¶neticisi",
        ThemeColor = "#059669",
        HeroTitle = "BÃ¼yÃ¼k Site OperasyonlarÄ±nÄ±zÄ± DijitalleÅŸtirin",
        HeroSubtitle = "GÃ¼venlik, peyzaj, personel yÃ¶netimi ve sosyal tesis rezervasyonlarÄ±nÄ± tek platformdan kusursuz yÃ¶netin.",
        HeroImage = "https://images.unsplash.com/photo-1494526585095-c41746248156?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        Features = new List<FeatureItem> {
            new FeatureItem { IconClass = "fas fa-shield-alt", Title = "GÃ¼venlik Entegrasyonu", Description = "Misafir araÃ§ kayÄ±tlarÄ± ve yaya giriÅŸlerini sistem Ã¼zerinden takip edin." },
            new FeatureItem { IconClass = "fas fa-users", Title = "Personel YÃ¶netimi", Description = "BahÃ§Ä±van, temizlik ve teknik ekibinizin mesailerini kontrol edin." },
            new FeatureItem { IconClass = "fas fa-swimming-pool", Title = "Tesis Rezervasyonu", Description = "Havuz, tenis kortu gibi ortak alanlar iÃ§in randevu sistemi sunun." }
        }
    });

    public IActionResult UstaKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "Usta",
        ThemeColor = "#f59e0b",
        HeroTitle = "Ã‡evrenizdeki Ä°ÅŸ FÄ±rsatlarÄ±nÄ± AnÄ±nda YakalayÄ±n",
        HeroSubtitle = "Tadilat, onarÄ±m ve boya badana taleplerine anÄ±nda teklif verin, iÅŸ aÄŸÄ±nÄ±zÄ± gÃ¼venle bÃ¼yÃ¼tÃ¼n.",
        HeroImage = "https://images.unsplash.com/photo-1504307651254-35680f356f12?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        Features = new List<FeatureItem> {
            new FeatureItem { IconClass = "fas fa-bell", Title = "AnlÄ±k Ä°ÅŸ Bildirimi", Description = "BÃ¶lgenizdeki boya, tesisat veya montaj taleplerini anÄ±nda gÃ¶rÃ¼n." },
            new FeatureItem { IconClass = "fas fa-handshake", Title = "HÄ±zlÄ± Teklif Verme", Description = "Ä°ÅŸverenlere online fiyat teklifi ileterek iÅŸleri kolayca baÄŸlayÄ±n." },
            new FeatureItem { IconClass = "fas fa-star", Title = "Puan ve Yorum Sistemi", Description = "YaptÄ±ÄŸÄ±nÄ±z baÅŸarÄ±lÄ± iÅŸlerle puanÄ±nÄ±zÄ± artÄ±rÄ±p profilinizi Ã¶ne Ã§Ä±karÄ±n." }
        }
    });

    public IActionResult EsnafKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "Hizmet SaÄŸlayÄ±cÄ±",
        ThemeColor = "#ea580c",
        HeroTitle = "Sitenizin ve BinanÄ±zÄ±n Ã‡Ã¶zÃ¼m OrtaklarÄ±",
        HeroSubtitle = "Temizlik, taÅŸÄ±macÄ±lÄ±k, ilaÃ§lama, peyzaj veya gÃ¼venlik... Milyonlarca mÃ¼ÅŸteriye ve site yÃ¶netimine profesyonel hizmet sunun.",
        HeroImage = "https://images.unsplash.com/photo-1581578731548-c64695cc6952?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        Features = new List<FeatureItem> {
            new FeatureItem { IconClass = "fas fa-broom", Title = "Hizmet OdaklÄ± Profil", Description = "VerdiÄŸiniz hizmetleri (Temizlik, Nakliye vb.), referanslarÄ±nÄ±zÄ± ve sertifikalarÄ±nÄ±zÄ± sergileyin." },
            new FeatureItem { IconClass = "fas fa-handshake", Title = "YÃ¶netimlerle DoÄŸrudan Ä°ÅŸ", Description = "Bina ve site yÃ¶netimlerinden gelen periyodik bakÄ±m/temizlik taleplerine anÄ±nda teklif verin." },
            new FeatureItem { IconClass = "fas fa-star", Title = "GÃ¼ven ve Ä°tibar", Description = "MÃ¼ÅŸteri yorumlarÄ± ile sektÃ¶rdeki puanÄ±nÄ±zÄ± yÃ¼kseltip daha fazla dÃ¼zenli iÅŸ alÄ±n." }
        }
    });

    public IActionResult GunlukKiralayanKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "GÃ¼nlÃ¼k Kiralayan",
        ThemeColor = "#ec4899",
        HeroTitle = "Tatil Evlerinizi YÃ¼ksek KÃ¢rla, SÄ±fÄ±r Komisyonla YÃ¶netin",
        HeroSubtitle = "VillanÄ±zÄ±, yazlÄ±ÄŸÄ±nÄ±zÄ± veya stÃ¼dyo dairenizi doÄŸrudan misafirlerle buluÅŸturun, rezervasyon takvimini dijitalden yÃ¶netin.",
        HeroImage = "https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        Features = new List<FeatureItem> {
            new FeatureItem { IconClass = "fas fa-calendar-check", Title = "Takvim YÃ¶netimi", Description = "FarklÄ± platformlardaki takvimlerinizi tek merkezde eÅŸitleyin (iCal)." },
            new FeatureItem { IconClass = "fas fa-percent", Title = "SÄ±fÄ±r Komisyon", Description = "Kiralama iÅŸlemlerinden komisyon Ã¶demeden gelirinizi artÄ±rÄ±n." },
            new FeatureItem { IconClass = "fas fa-id-card", Title = "KBS Entegrasyonu", Description = "Emniyet Kimlik Bildirim Sistemi'ne verileri otomatik yollayÄ±n." }
        }
    });

    public IActionResult EmlakOfisiKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "Emlak Ofisi",
        ThemeColor = "#2563eb",
        HeroTitle = "Kurumsal Emlak Ofisinizi Dijitale TaÅŸÄ±yÄ±n",
        HeroSubtitle = "TÃ¼m danÄ±ÅŸmanlarÄ±nÄ±zÄ±, yetki belgelerinizi, ilan portfÃ¶yÃ¼nÃ¼zÃ¼ ve mÃ¼ÅŸteri havuzunuzu tek panelden yÃ¶netin.",
        HeroImage = "https://images.unsplash.com/photo-1552581234-26160f608093?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        Features = new List<FeatureItem> {
            new FeatureItem { IconClass = "fas fa-users-cog", Title = "DanÄ±ÅŸman YÃ¶netimi", Description = "Alt danÄ±ÅŸmanlarÄ±nÄ±za yetki verin, performanslarÄ±nÄ± anlÄ±k Ã¶lÃ§Ã¼n." },
            new FeatureItem { IconClass = "fas fa-folder-open", Title = "PortfÃ¶y Havuzu", Description = "TÃ¼m ilanlarÄ± ofis Ã§atÄ±sÄ± altÄ±nda toplayÄ±n ve hÄ±zlÄ±ca paylaÅŸÄ±n." },
            new FeatureItem { IconClass = "fas fa-file-signature", Title = "Yasal SÃ¶zleÅŸmeler", Description = "TaÅŸÄ±nmaz ticareti yetki sÃ¶zleÅŸmelerinizi e-imza ile dijitalde tutun." }
        }
    });

    public IActionResult EmlakDanismaniKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "Emlak DanÄ±ÅŸmanÄ±",
        ThemeColor = "#8b5cf6",
        HeroTitle = "Yeni Nesil AraÃ§larla Emlak SatÄ±ÅŸlarÄ±nÄ±zÄ± KatlayÄ±n",
        HeroSubtitle = "Yapay zeka destekli gayrimenkul deÄŸerleme, mÃ¼ÅŸteri eÅŸleÅŸtirme (Lead) ve akÄ±llÄ± CRM araÃ§larÄ± elinizin altÄ±nda.",
        HeroImage = "https://images.unsplash.com/photo-1573496799515-eebbb63814f2?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        Features = new List<FeatureItem> {
            new FeatureItem { IconClass = "fas fa-robot", Title = "Yapay Zeka DeÄŸerleme", Description = "BÃ¶lge bazlÄ± istatistiklerle saniyeler iÃ§inde fiyat raporu oluÅŸturun." },
            new FeatureItem { IconClass = "fas fa-address-book", Title = "AkÄ±llÄ± CRM", Description = "AlÄ±cÄ± ve satÄ±cÄ± veri tabanÄ±nÄ±zÄ± tutun, gÃ¶rÃ¼ÅŸme geÃ§miÅŸini kaydedin." },
            new FeatureItem { IconClass = "fas fa-magic", Title = "Lead (MÃ¼ÅŸteri) EÅŸleÅŸtirme", Description = "Sistemdeki kiralÄ±k/satÄ±lÄ±k arayan talepleriyle portfÃ¶yÃ¼nÃ¼zÃ¼ otomatik eÅŸleyin." }
        }
    });

    public IActionResult KurumsalFirmaKatilim() => View("RoleLanding", new RoleLandingViewModel {
        RoleName = "Ä°nÅŸaat / Kurumsal Firma",
        ThemeColor = "#475569",
        HeroTitle = "SektÃ¶rÃ¼n Zirvesine Ã‡Ä±kÄ±n",
        HeroSubtitle = "Kendi alt alan adÄ±nÄ±z (subdomain), kendi logonuz ve kurumsal renklerinizle dijital maÄŸazanÄ±zÄ± dakikalar iÃ§inde kurun. MÃ¼ÅŸterilerinizi sadece kendi ilanlarÄ±nÄ±za odaklayÄ±n.",
        HeroImage = "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80",
        RegisterUrl = "/Account/RegisterCorporate",
        Features = new List<FeatureItem>
        {
            new FeatureItem { IconClass = "fas fa-store", Title = "White-Label MaÄŸaza", Description = "Size Ã¶zel tasarlanmÄ±ÅŸ tamamen kurumsal bir arayÃ¼z." },
            new FeatureItem { IconClass = "fas fa-link", Title = "Size Ã–zel Subdomain", Description = "firmaadi.gmk360.com adresi ile itibarÄ±nÄ±zÄ± artÄ±rÄ±n." },
            new FeatureItem { IconClass = "fas fa-bullseye", Title = "MÃ¼ÅŸteri Ä°zolasyonu", Description = "ZiyaretÃ§ileriniz rakip ilanlarÄ± veya esnaflarÄ± gÃ¶rmez, tamamen size odaklanÄ±r." },
            new FeatureItem { IconClass = "fas fa-infinity", Title = "SÄ±nÄ±rsÄ±z Vitrin", Description = "SÄ±nÄ±rsÄ±z sayÄ±da ilan ve proje ekleme imkanÄ±." }
        }
    });


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("Home/Error/{statusCode?}")]
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

    public IActionResult Faq()
    {
        return View();
    }

    public IActionResult Legal(string doc)
    {
        ViewBag.DocType = doc;
        return View();
    }
}


