using System;
using System.Collections.Generic;
using GMK360.Core.Entities.Construction;

namespace GMK360.Web.Models
{
    public class FeasibilityViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        
        // Old Building Info
        public int? EskiKatSayisi { get; set; }
        public int? EskiDaireSayisi { get; set; }
        public double? EskiToplamMetrekare { get; set; }
        
        // Estimated Costs
        public decimal YikimIzniVeHarclar { get; set; }
        public decimal CevreKapanmasiMaliyeti { get; set; }
        public decimal YikimHafriyatMaliyeti { get; set; }
        public decimal AsbestSokumMaliyeti { get; set; }
        public decimal YeniRuhsatHarci { get; set; }
        public decimal InsaatYapimMaliyeti { get; set; }
        
        public decimal ToplamMaliyet => YikimIzniVeHarclar + CevreKapanmasiMaliyeti + YikimHafriyatMaliyeti + AsbestSokumMaliyeti + YeniRuhsatHarci + InsaatYapimMaliyeti;
        
        // Contractor Revenue Estimate (Müteahhide Kalan Dairelerin Satış Değeri)
        public decimal SatisGeliriBeklentisi { get; set; }
        
        // Deficit (Zarar / Fark)
        public decimal ToplamFark => ToplamMaliyet > SatisGeliriBeklentisi ? ToplamMaliyet - SatisGeliriBeklentisi : 0;
        
        public List<ProjectOwnerViewModel> Owners { get; set; } = new List<ProjectOwnerViewModel>();
    }

    public class ProjectOwnerViewModel
    {
        public int OwnerId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string FlatNumber { get; set; }
        public double? LandShare { get; set; }
        public decimal CalculatedDebt { get; set; }
    }
}
