using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities.B2B;
using GMK360.Core.Entities.Construction;
using GMK360.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Helpers
{
    public static class PhaseCentralPoolHelper
    {
        public static (decimal Quantity, string Unit, decimal UnitPrice) GetCentralPoolReference(string? itemName, BudgetPhaseCategory category)
        {
            var n = (itemName ?? "").ToLowerInvariant();

            // FAZ 1: Yıkım ve Zemin Hazırlığı
            if (n.Contains("elektrik projesi")) return (1m, "Götürü", 35000m);
            if (n.Contains("geçici şantiye panosu") || n.Contains("şalter")) return (1m, "Adet", 48000m);
            if (n.Contains("pano kurulumu") || n.Contains("kablolama")) return (1m, "Götürü", 25000m);
            if (n.Contains("geçici su aboneliği")) return (1m, "Götürü", 18500m);
            if (n.Contains("şebeke bağlantı") || n.Contains("boru tedariği")) return (1m, "Götürü", 22000m);
            if (n.Contains("su deposu") || n.Contains("hidrofor")) return (1m, "Adet", 38000m);
            if (n.Contains("sınır tespiti") || n.Contains("aplikasyon")) return (1m, "Götürü", 28000m);
            if (n.Contains("çevre kapatma direk")) return (120m, "m2", 650m);
            if (n.Contains("trapez sac") || n.Contains("tel çit")) return (120m, "m2", 850m);
            if (n.Contains("konteyner zemin") || n.Contains("iş makinesi kiralama")) return (2m, "Gün", 14000m);
            if (n.Contains("yönetim ofisi konteyner")) return (1m, "Adet", 95000m);
            if (n.Contains("yatakhane") || n.Contains("wc/duş konteyner")) return (2m, "Adet", 110000m);
            if (n.Contains("mobil vinç")) return (1m, "Sefer", 16000m);
            if (n.Contains("yıkım ruhsatı") || n.Contains("asbest")) return (1m, "Götürü", 45000m);
            if (n.Contains("döküm sahası") || n.Contains("hafriyat öncesi")) return (1m, "Götürü", 85000m);

            // FAZ 2: Temel ve Alt Yapı
            if (n.Contains("hafriyat") || n.Contains("kazı")) return (1200m, "m3", 420m);
            if (n.Contains("iksa") || n.Contains("fore kazık") || n.Contains("zemin iyileştirme")) return (1m, "Götürü", 680000m);
            if (n.Contains("grobeton") || n.Contains("bohçalama") || n.Contains("temel yalıtım")) return (400m, "m2", 680m);
            if (n.Contains("radye") || n.Contains("temel demir") || n.Contains("temel beton")) return (1m, "Götürü", 1850000m);
            if (n.Contains("drenaj") || n.Contains("temel topraklama")) return (1m, "Götürü", 95000m);

            // FAZ 3: Kaba İnşaat (Karkas)
            if (n.Contains("kolon, kiriş") || n.Contains("kalıp, demir, beton")) return (1500m, "m2", 4800m);
            if (n.Contains("tuğla") || n.Contains("gazbeton") || n.Contains("bims") || n.Contains("duvar örüm")) return (2200m, "m2", 650m);
            if (n.Contains("asansör kuyusu") || n.Contains("merdiven") || n.Contains("çelik konstrüksiyon")) return (1m, "Götürü", 480000m);

            // FAZ 4: Çatı ve Dış Cephe
            if (n.Contains("çatı konstrüksiyon") || n.Contains("kiremit") || n.Contains("şıngıl")) return (450m, "m2", 1650m);
            if (n.Contains("dış cephe") || n.Contains("mantolama")) return (1800m, "m2", 980m);
            if (n.Contains("ıslak hacim") || n.Contains("su yalıtım") || n.Contains("teras")) return (600m, "m2", 520m);

            // FAZ 5: İnce İşler (İç Mekan)
            if (n.Contains("kaba sıva") || n.Contains("alçı sıva") || n.Contains("boya işleri")) return (4200m, "m2", 480m);
            if (n.Contains("zemin kaplama") || n.Contains("şap") || n.Contains("seramik") || n.Contains("parke")) return (1500m, "m2", 1150m);
            if (n.Contains("doğrama") || n.Contains("çelik kapı") || n.Contains("mutfak ve banyo dolap")) return (12m, "Adet", 185000m);

            // FAZ 6 & 7: Mekanik ve Elektrik
            if (n.Contains("sıhhi tesisat") || n.Contains("temiz ve pis su") || n.Contains("vitrifiye")) return (12m, "Adet", 75000m);
            if (n.Contains("ısıtma") || n.Contains("yerden ısıtma") || n.Contains("kombi") || n.Contains("iklimlendirme")) return (12m, "Adet", 95000m);
            if (n.Contains("doğalgaz") || n.Contains("yangın tesisatı")) return (1m, "Götürü", 420000m);
            if (n.Contains("kuvvetli akım") || n.Contains("ana panolar")) return (12m, "Adet", 82000m);
            if (n.Contains("zayıf akım") || n.Contains("kamera") || n.Contains("diafon")) return (1m, "Götürü", 290000m);
            if (n.Contains("paratoner") || n.Contains("topraklama sonlandırma")) return (1m, "Götürü", 85000m);

            // FAZ 8: Peyzaj ve Teslim
            if (n.Contains("altyapı son bağlantı") || n.Contains("rögar") || n.Contains("tedaş nihai")) return (1m, "Götürü", 195000m);
            if (n.Contains("peyzaj") || n.Contains("yürüyüş yol") || n.Contains("bahçe duvarı")) return (1m, "Götürü", 640000m);
            if (n.Contains("ince temizliği") || n.Contains("profesyonel temizlik")) return (1m, "Götürü", 95000m);
            if (n.Contains("iskan") || n.Contains("yapı kullanım izin")) return (1m, "Götürü", 260000m);

            return category switch
            {
                BudgetPhaseCategory.YikimVeZeminHazirligi => (1m, "Götürü", 35000m),
                BudgetPhaseCategory.TemelVeAltYapi => (1m, "Götürü", 250000m),
                BudgetPhaseCategory.KabaInsaatKarkas => (1m, "Götürü", 850000m),
                BudgetPhaseCategory.CatiVeDisCephe => (1m, "Götürü", 450000m),
                BudgetPhaseCategory.InceIslerIcMekan => (1m, "Götürü", 550000m),
                BudgetPhaseCategory.ElektrikVeZayifAkim => (1m, "Götürü", 320000m),
                BudgetPhaseCategory.MekanikTesisatVeMakine => (1m, "Götürü", 380000m),
                BudgetPhaseCategory.PeyzajVeTeslim => (1m, "Götürü", 180000m),
                _ => (1m, "Götürü", 50000m)
            };
        }

        public static async Task EnsurePhaseItemsPricedFromPoolAsync(ApplicationDbContext context, int projectId, BudgetPhaseCategory? category = null)
        {
            var query = context.ConstructionBudgetItems.Where(b => b.ConstructionProjectId == projectId);
            if (category.HasValue)
            {
                query = query.Where(b => b.PhaseCategory == category.Value);
            }

            var items = await query.ToListAsync();
            bool changed = false;
            foreach (var item in items)
            {
                if (item.PlannedUnitPrice <= 0)
                {
                    var (qty, unit, unitPrice) = GetCentralPoolReference(item.ItemName, item.PhaseCategory);
                    item.Quantity = qty;
                    item.Unit = unit;
                    item.PlannedUnitPrice = unitPrice;
                    if (string.IsNullOrEmpty(item.Description))
                    {
                        item.Description = "GMK360 Merkez Havuz Referans Birim Fiyatı";
                    }
                    changed = true;
                }
            }

            if (changed)
            {
                await context.SaveChangesAsync();
            }
        }

        public static async Task<int?> RequestQuoteForBudgetItemAsync(ApplicationDbContext context, int itemId, string sourceModule)
        {
            var item = await context.ConstructionBudgetItems
                .Include(i => i.ConstructionProject)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null) return null;

            var existing = await context.B2BQuoteRequests
                .FirstOrDefaultAsync(q => q.SourceModule == sourceModule && q.SourceReferenceId == itemId);

            if (existing == null)
            {
                existing = new B2BQuoteRequest
                {
                    RequesterAgencyId = item.ConstructionProject?.AgencyId ?? 1,
                    RequesterUserId = "ProjeYöneticisi",
                    SourceModule = sourceModule,
                    SourceReferenceId = item.Id,
                    Title = (item.ConstructionProject?.Name ?? "Şantiye") + " - " + item.ItemName + " (Fiyat Araştırması / İhale)",
                    Description = $"GMK360 Merkez Havuz Kalemi ({item.ItemName} - {item.Quantity:G29} {item.Unit}) için tedarikçi/taşeron fiyat araştırması ve teklif talebi.",
                    Deadline = DateTime.Now.AddDays(7),
                    Status = "Draft"
                };
                context.B2BQuoteRequests.Add(existing);
                item.QuoteStatus = BudgetQuoteStatus.EstimatedOrQuoted;
                await context.SaveChangesAsync();
            }

            return item.ConstructionProjectId;
        }
    }
}
