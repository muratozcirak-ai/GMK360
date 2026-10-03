using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Construction;
using System.Linq;

namespace GMK360.Web.Controllers
{
    public partial class PhaseOneController
    {
        [Microsoft.AspNetCore.Mvc.HttpGet("PhaseOne/ForceSeedPhaseTwo")]
        public Microsoft.AspNetCore.Mvc.IActionResult ForceSeedPhaseTwo()
        {
            _context.SystemPhaseTemplates.RemoveRange(_context.SystemPhaseTemplates.Where(t => (int)t.PhaseCategory == 3));
            _context.SaveChanges();

            var templates = new SystemPhaseTemplate[]
            {
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.TemelVeAltyapi, SubCategory = "2.1 Hafriyat ve Zemin İksası (Destekleme)", ItemName = "2.1.A: Derin Hafriyat Kazısı ve Hafriyatın Döküm Sahasına Nakliyesi", IsQuoteRequired = true },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.TemelVeAltyapi, SubCategory = "2.1 Hafriyat ve Zemin İksası (Destekleme)", ItemName = "2.1.B: Fore Kazık / Mini Kazık veya İksa İşleri", IsQuoteRequired = true },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.TemelVeAltyapi, SubCategory = "2.2 Temel Altı Hazırlık ve Yalıtım (Bohçalama)", ItemName = "2.2.A: Zemin Tesviyesi ve Grobeton Dökümü", IsQuoteRequired = true },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.TemelVeAltyapi, SubCategory = "2.2 Temel Altı Hazırlık ve Yalıtım (Bohçalama)", ItemName = "2.2.B: Temel Altı Su ve Isı Yalıtımı İşleri (Membran / Sürme İzolasyon)", IsQuoteRequired = true },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.TemelVeAltyapi, SubCategory = "2.2 Temel Altı Hazırlık ve Yalıtım (Bohçalama)", ItemName = "2.2.C: Yalıtım Koruma Betonu veya Keçe Serimi", IsQuoteRequired = true },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.TemelVeAltyapi, SubCategory = "2.3 Temel Betonarme İmalatı", ItemName = "2.3.A: Temel Altı Topraklama Ağı ve Gider Boruları Rezervasyonları", IsQuoteRequired = true },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.TemelVeAltyapi, SubCategory = "2.3 Temel Betonarme İmalatı", ItemName = "2.3.B: Temel Kalıp Çakılması İşçiliği ve Malzemesi", IsQuoteRequired = true },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.TemelVeAltyapi, SubCategory = "2.3 Temel Betonarme İmalatı", ItemName = "2.3.C: Temel Demir Donatı İşçiliği ve Malzeme Tedariği", IsQuoteRequired = true },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.TemelVeAltyapi, SubCategory = "2.3 Temel Betonarme İmalatı", ItemName = "2.3.D: Temel Betonu (Hazır Beton) Tedariği ve Pompa Hizmeti", IsQuoteRequired = true }
            };
            _context.SystemPhaseTemplates.AddRange(templates);
            _context.SaveChanges();
            return Content("Phase 2 Seeded C#");
        }
    }
}