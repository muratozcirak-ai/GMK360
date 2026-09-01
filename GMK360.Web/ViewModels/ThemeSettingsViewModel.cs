using System.ComponentModel.DataAnnotations;

namespace GMK360.Web.ViewModels
{
    public class ThemeSettingsViewModel
    {
        public int AgencyId { get; set; }

        [Display(Name = "Özel Alan Adı (Örn: www.ofisiniz.com)")]
        public string CustomDomain { get; set; }

        [Display(Name = "Alt Alan Adı (Subdomain)")]
        public string Subdomain { get; set; }

        [Display(Name = "Ana Renk")]
        public string ThemePrimaryColor { get; set; }

        [Display(Name = "İkincil Renk")]
        public string ThemeSecondaryColor { get; set; }

        [Display(Name = "Vurgu Rengi")]
        public string ThemeAccentColor { get; set; }

        [Display(Name = "Font Ailesi")]
        public string FontFamily { get; set; }

        [Display(Name = "Logo URL")]
        public string LogoUrl { get; set; }

        [Display(Name = "Footer Logo URL")]
        public string FooterLogoUrl { get; set; }

        [Display(Name = "Favicon URL")]
        public string FaviconUrl { get; set; }
    }
}
