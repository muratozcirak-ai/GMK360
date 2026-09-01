using System.Collections.Generic;

namespace GMK360.Web.Models
{
    public class SolutionViewModel
    {
        public string SegmentId { get; set; } = null!;
        public string IdentityUserType { get; set; } = "Individual";
        public string Title { get; set; } = null!;
        public string Subtitle { get; set; } = null!;
        public string IconClass { get; set; } = null!;
        public string CoverImageUrl { get; set; } = null!;

        public string StandardPackageName { get; set; } = "Standart";
        public string StandardPackagePrice { get; set; } = "2 Ay Ücretsiz";
        public string StandardPackageSubtitle { get; set; } = "Tüm özellikleri risksiz deneyin.";
        
        public string ProPackageName { get; set; } = "Premium (Pro)";
        public string ProPackagePrice { get; set; } = "Pro";
        public string ProPackageSubtitle { get; set; } = "Geniş portföyler için uçtan uca yönetim.";
        
        // Standart Paket Özellikleri
        public List<string> StandardFeatures { get; set; } = new List<string>();
        
        // Pro Paket Özellikleri
        public List<string> ProFeatures { get; set; } = new List<string>();

        // Bu sektöre özel o anki aktif kampanyalar (Banner verisi)
        public Core.Entities.MarketingCampaign? ActiveCampaign { get; set; }
    }
}
