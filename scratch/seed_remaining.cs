using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Construction;
using System.Linq;

namespace GMK360.Web.Controllers
{
    public partial class PhaseOneController
    {
        [Microsoft.AspNetCore.Mvc.HttpGet("PhaseOne/ForceSeedRemainingPhases")]
        public Microsoft.AspNetCore.Mvc.IActionResult ForceSeedRemainingPhases()
        {
            var targetCategories = new[] { 4, 5, 6, 7, 8, 9 };
            _context.SystemPhaseTemplates.RemoveRange(_context.SystemPhaseTemplates.Where(t => targetCategories.Contains((int)t.PhaseCategory)));
            _context.SaveChanges();

            var templates = new SystemPhaseTemplate[]
            {
                // FAZ 3
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.KabaInsaatKarkas, SubCategory = "3.1 Kolon, Kiriş ve Döşeme", ItemName = "3.1.A: Kolon, Kiriş ve Döşeme İmalatları (Kalıp, Demir, Beton)" },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.KabaInsaatKarkas, SubCategory = "3.2 Duvar ve Lento İşleri", ItemName = "3.2.A: Tuğla, Gazbeton veya Bims Duvar Örümleri ve Lento İşleri" },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.KabaInsaatKarkas, SubCategory = "3.3 Özel İmalatlar", ItemName = "3.3.A: Asansör Kuyusu, Merdiven ve Çelik Konstrüksiyon İşleri (Varsa)" },

                // FAZ 4
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.CatiVeDisCephe, SubCategory = "4.1 Çatı", ItemName = "4.1.A: Çatı Konstrüksiyonu (Ahşap/Çelik) ve Kaplaması (Kiremit, Şıngıl, Membran vb.)" },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.CatiVeDisCephe, SubCategory = "4.2 Dış Cephe", ItemName = "4.2.A: Dış Cephe Isı Yalıtımı (Mantolama) ve Dış Cephe Boyası/Kaplaması" },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.CatiVeDisCephe, SubCategory = "4.3 İzolasyon", ItemName = "4.3.A: Teras, Çatı Deresi ve Tüm Islak Hacimlerin (Banyo/Balkon) Su Yalıtımları" },

                // FAZ 5
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.InceIslerIcMekan, SubCategory = "5.1 Sıva ve Boya", ItemName = "5.1.A: İç Cephe Kaba Sıva, Alçı Sıva, Kartonpiyer ve Boya İşleri" },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.InceIslerIcMekan, SubCategory = "5.2 Zemin Kaplamaları", ItemName = "5.2.A: Zemin Kaplamaları (Şap dökümü, Seramik, Parke, Mermer işleri)" },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.InceIslerIcMekan, SubCategory = "5.3 Doğrama ve Ahşap", ItemName = "5.3.A: Doğrama ve Ahşap İşleri (Dış Pencereler, İç Kapılar, Çelik Kapı, Mutfak ve Banyo Dolapları)" },

                // FAZ 6 (Mekanik)
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.MekanikTesisatVeMakine, SubCategory = "6.1 Sıhhi Tesisat", ItemName = "6.1.A: Temiz ve Pis Su Tesisatı Altyapısı ile Vitrifiye Montajı" },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.MekanikTesisatVeMakine, SubCategory = "6.2 İklimlendirme", ItemName = "6.2.A: Isıtma, Soğutma ve Havalandırma (Yerden Isıtma, Petek, Kombi veya VRF)" },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.MekanikTesisatVeMakine, SubCategory = "6.3 Doğalgaz ve Yangın", ItemName = "6.3.A: Doğalgaz Tesisatı ve Yangın Tesisatı (Şaft İçi Borulama ve Kolektörler)" },

                // FAZ 7 (Elektrik)
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.ElektrikVeZayifAkim, SubCategory = "7.1 Kuvvetli Akım", ItemName = "7.1.A: Kuvvetli Akım Tesisatı (Ana panolar, kat panoları, kablolama, priz ve aydınlatma)" },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.ElektrikVeZayifAkim, SubCategory = "7.2 Zayıf Akım", ItemName = "7.2.A: Zayıf Akım Tesisatı (İnternet, Kamera, Diafon, Uydu, Yangın ihbar)" },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.ElektrikVeZayifAkim, SubCategory = "7.3 Topraklama ve Paratoner", ItemName = "7.3.A: Paratoner ve Temel Dışı Topraklama Sonlandırma İşleri" },

                // FAZ 8
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.PeyzajVeTeslim, SubCategory = "8.1 Altyapı Bağlantıları", ItemName = "8.1.A: Altyapı Son Bağlantıları (Belediye Rögar, Şebeke Suyu ve TEDAŞ nihai)" },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.PeyzajVeTeslim, SubCategory = "8.2 Çevre Düzenleme", ItemName = "8.2.A: Peyzaj, Yürüyüş Yolları, Açık/Kapalı Otopark Zeminleri ve Bahçe Duvarı" },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.PeyzajVeTeslim, SubCategory = "8.3 Temizlik", ItemName = "8.3.A: Şantiye İnce Temizliği (Teslimat öncesi profesyonel temizlik)" },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.PeyzajVeTeslim, SubCategory = "8.4 Teslimat ve İskan", ItemName = "8.4.A: İskan (Yapı Kullanım İzin Belgesi) Harçları ve Resmi Teslim İşlemleri" }
            };

            // Ensure IsQuoteRequired is false for all since it's just feasibility amount
            foreach(var item in templates) { item.IsQuoteRequired = false; }

            _context.SystemPhaseTemplates.AddRange(templates);
            _context.SaveChanges();
            return Content("Phases 3-8 Seeded C#");
        }
    }
}