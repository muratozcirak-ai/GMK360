using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GMK360.Web.Models;
using System.Collections.Generic;

namespace GMK360.Web.Controllers
{
    [AllowAnonymous]
    public class SolutionsController : Controller
    {
        // Merkezi bir veri mock'u veya DB'den çekilebilir. 
        // Şimdilik mimariyi oturtmak için Controller içinde yapılandırıyoruz.
        private SolutionViewModel GetSegmentData(string segmentId)
        {
            var data = new SolutionViewModel { SegmentId = segmentId };

            switch (segmentId.ToLowerInvariant())
            {
                case "bireysel-kiraci":
                    data.IdentityUserType = "PropertyOwner";
                    data.Title = "Bireysel ve Kiracı Takip";
                    data.Subtitle = "Ev sahipleri ve kiracılar için şeffaf, güvenilir dijital yönetim platformu.";
                    data.IconClass = "ph-house-line";
                    data.CoverImageUrl = "https://images.unsplash.com/photo-1560518883-ce09059eeffa?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80";
                    data.StandardFeatures = new List<string> { 
                        "En Fazla 2 Daire + 1 Dükkan Yönetimi",
                        "Yapay Zeka (AI) Mülk Değerleme Analizi", 
                        "Kira Takibi ve Dijital Dekont Üretimi", 
                        "Uçtan Uca Sözleşme Saklama (Dijital Kasa)", 
                        "Fatura Okuma (Vision AI) ve Otomatik Düşüm",
                        "Kiracı Finansal Risk Puanı (E-Devlet)",
                        "Akıllı Vergi ve Beyanname Asistanı",
                        "Bölgesel Usta/Tadilat Teklifi Alma (Yakında)"
                    };
                    data.ProFeatures = new List<string> { 
                        "Sınırsız Sayıda Mülk Yönetimi", 
                        "Hukuki Danışmanlık ve İhtarname Desteği",
                        "Öncelikli 7/24 Telefon ve WhatsApp Desteği" 
                    };
                    break;
                case "bina-site-yonetimi":
                    data.IdentityUserType = "Corporate";
                    data.Title = "Bina ve Site Yönetimi";
                    data.Subtitle = "Yöneticiler için aidat, bakım ve karar defteri takip ekosistemi.";
                    data.IconClass = "ph-buildings";
                    data.CoverImageUrl = "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80";
                    data.StandardFeatures = new List<string> { 
                        "En Fazla 1 Bina veya Apartman (Sınırlı Daire)",
                        "Otomatik Aidat ve Gecikme Zammı Tahakkuku", 
                        "Gelir-Gider Paylaşım ve Bilanço Modülü", 
                        "Otonom AI Asistanı (WhatsApp Yanıtları)",
                        "Dijital Karar Defteri ve Arşivleme"
                    };
                    data.ProFeatures = new List<string> { 
                        "Sınırsız Sayıda Blok ve Site Yönetimi",
                        "Anında Banka Entegrasyonu (Açık Bankacılık)",
                        "Resmi Dijital Oylama (Blockchain Altyapısı)",
                        "Toplu SMS ve Çoklu Yönetici Yetkilendirmesi" 
                    };
                    break;
                case "esnaf-hizmet":
                    data.IdentityUserType = "ServiceProvider";
                    data.Title = "Esnaf, Kurumsal Hizmet ve Mimarlık";
                    data.Subtitle = "Ustalar, temizlik firmaları ve mimarlar için kapalı devre müşteri bulma platformu.";
                    data.IconClass = "ph-wrench";
                    data.CoverImageUrl = "https://images.unsplash.com/photo-1504307651254-35680f356f12?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80";
                    data.StandardFeatures = new List<string> { 
                        "Ayda Maksimum 3 Teklif Sunma Hakkı",
                        "Kurumsal Firma Profili ve Vitrin", 
                        "Bölgesel İş Taleplerini Görüntüleme", 
                        "Doğrulanmış Müşteri Puanlama Sistemi" 
                    };
                    data.ProFeatures = new List<string> { 
                        "Sınırsız İhale ve Teklif Sunma Modülü", 
                        "Uygulama İçi Öne Çıkan Profil (Sponsorlu)", 
                        "E-Fatura Kesme ve Tahsilat Garantisi" 
                    };
                    break;
                case "insaat-proje":
                    data.IdentityUserType = "Corporate";
                    data.Title = "İnşaat ve Proje Firmaları";
                    data.Subtitle = "Yeni konut projeleriniz için satış sonrası destek ve blok yönetimi.";
                    data.IconClass = "ph-crane";
                    data.CoverImageUrl = "https://images.unsplash.com/photo-1503387762-592deb58ef4e?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80";
                    data.StandardFeatures = new List<string> { 
                        "Tek (1) İnşaat Projesi Yönetimi",
                        "Demirbaş ve Hasar Tespit Modülü",
                        "Satış Sonrası Şikayet / Talep Kutusu", 
                        "Garanti Belgesi ve Proje Dosyası Arşivi" 
                    };
                    data.ProFeatures = new List<string> { 
                        "Sınırsız Sayıda Konut Projesi (Çoklu Şantiye)",
                        "Taşeron Hak Ediş ve Bütçe Takibi",
                        "Kurumsal Marka Özelleştirmesi (White-Label App)",
                        "Satış CRM Modülü (Lead Takibi)" 
                    };
                    break;
                case "gunluk-kiralama":
                    data.IdentityUserType = "CommercialRenter";
                    data.Title = "Günlük Kiralama ve Pansiyon";
                    data.Subtitle = "Airbnb ve günlük kiralık daireler için takvim ve müşteri yönetimi.";
                    data.IconClass = "ph-calendar-check";
                    data.CoverImageUrl = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80";
                    data.StandardFeatures = new List<string> { 
                        "Maksimum 2 Kiralık Ünite Kapasitesi",
                        "Yapay Zeka (AI) Destekli Dinamik Fiyatlandırma",
                        "Çoklu Platform Takvim Senkronizasyonu", 
                        "Dijital Giriş-Çıkış (Check-in / Check-out)" 
                    };
                    data.ProFeatures = new List<string> { 
                        "Sınırsız Ünite Yönetimi ve Analitikler",
                        "Otomatik Kimlik Bildirim Sistemi (KBS) Entegrasyonu", 
                        "Temizlik Görevlendirmesi ve Personel Takibi",
                        "Hasar Depozito ve Güvenli Tahsilat Yönetimi" 
                    };
                    break;
                case "emlak-danisman":
                    data.IdentityUserType = "Consultant";
                    data.Title = "Emlak ve Danışman Ofisleri";
                    data.Subtitle = "Portföyünüzü ve kiracı ilişkilerini profesyonelce yönetin.";
                    data.IconClass = "ph-handshake";
                    data.CoverImageUrl = "https://images.unsplash.com/photo-1560520653-9e0e4c89eb11?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80";
                    data.StandardFeatures = new List<string> { 
                        "En Fazla 5 Portföy (İlan) Yayınlama ve Takibi",
                        "Yapay Zeka ile Mülk Değerleme Analizi",
                        "Kiracı - Mal Sahibi Eşleştirme Motoru", 
                        "Dijital Matbu Evrak ve Sözleşme Hazırlama" 
                    };
                    data.ProFeatures = new List<string> { 
                        "Sınırsız Portföy ve Gelişmiş Müşteri CRM'i",
                        "GMK360 Blog Misafir Yazarlık (Ayrıcalıklı SEO)", 
                        "Gelişmiş Komisyon ve Ekip Performans Takibi", 
                        "Branding (Kişisel Marka Vitrini)" 
                    };
                    break;
                default:
                    return null;
            }

            // Mock Campaign
            data.ActiveCampaign = new Core.Entities.MarketingCampaign
            {
                Title = $"{data.Title} Sektörüne Özel Başlangıç Kampanyası!",
                Description = "Şimdi kayıt olun, ilk 1 ay Pro özelliklerin tamamını ücretsiz deneyimleyin. Taahhüt yok!",
                TargetSegment = segmentId,
                ActionUrl = $"/Account/Register?userType={data.IdentityUserType}&campaignId=1"
            };

            return data;
        }

        [Route("Cozumler/{segmentId}")]
        public IActionResult Detail(string segmentId)
        {
            var data = GetSegmentData(segmentId);
            if (data == null)
            {
                return RedirectToAction("Index", "Home");
            }
            
            return View("SegmentDetail", data);
        }
    }
}
