using System.Collections.Generic;

namespace GMK360.Web.Models
{
    public class RoleLandingViewModel
    {
        public string RoleName { get; set; }
        public string HeroTitle { get; set; }
        public string HeroSubtitle { get; set; }
        public string HeroImage { get; set; }
        public string ThemeColor { get; set; } 
        public List<FeatureItem> Features { get; set; } = new List<FeatureItem>();
        public string RegisterUrl { get; set; } = "/Auth/Register";
    }

    public class FeatureItem
    {
        public string IconClass { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
