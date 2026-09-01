using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GMK360.Web.Models
{
    public class SetupCorporateProfileViewModel
    {
        [Required(ErrorMessage = "Firma adı zorunludur.")]
        [Display(Name = "Firma Adı")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Vergi Numarası zorunludur.")]
        [Display(Name = "Vergi Numarası")]
        public string TaxNumber { get; set; }

        [Required(ErrorMessage = "Alt alan adı (Subdomain) zorunludur.")]
        [Display(Name = "Alt Alan Adı (Subdomain)")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Subdomain sadece küçük harf, rakam ve tire (-) içerebilir.")]
        public string Subdomain { get; set; }

        [Display(Name = "Firma Logosu (Maks. 2MB, JPG/PNG)")]
        public IFormFile LogoFile { get; set; }

        [Required(ErrorMessage = "İletişim numarası zorunludur.")]
        [Display(Name = "İletişim Numarası")]
        public string PhoneNumber { get; set; }
    }
}
