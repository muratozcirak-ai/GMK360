using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GMK360.Web.Models
{
    public class RegisterCorporateViewModel
    {
        [Required(ErrorMessage = "Firma Adı zorunludur.")]
        [Display(Name = "Firma Adı")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Vergi Dairesi zorunludur.")]
        [Display(Name = "Vergi Dairesi")]
        public string TaxOffice { get; set; }

        [Required(ErrorMessage = "Vergi Numarası zorunludur.")]
        [Display(Name = "Vergi Numarası")]
        public string TaxNumber { get; set; }

        // Yetkili Kişi (NVI Doğrulaması İçin)
        [Required(ErrorMessage = "Yetkili Adı zorunludur.")]
        [Display(Name = "Yetkili Adı")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Yetkili Soyadı zorunludur.")]
        [Display(Name = "Yetkili Soyadı")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Yetkili TCKN zorunludur.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TCKN 11 haneli olmalıdır.")]
        [Display(Name = "Yetkili TCKN")]
        public string TcIdentityNo { get; set; }

        [Required(ErrorMessage = "Yetkili Doğum Yılı zorunludur.")]
        [Display(Name = "Yetkili Doğum Yılı")]
        public int BirthYear { get; set; }

        [Required(ErrorMessage = "E-Posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-Posta")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [Display(Name = "Telefon Numarası")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, ErrorMessage = "{0} en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; }

        // White-Label Alanları
        [Required(ErrorMessage = "Alt alan adı (Subdomain) zorunludur.")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Alt alan adı sadece küçük harf, rakam ve tire (-) içerebilir.")]
        [Display(Name = "Alt Alan Adı (Subdomain)")]
        public string Subdomain { get; set; } // Örn: egeyapi

        [Display(Name = "Kurumsal Renk")]
        public string ThemePrimaryColor { get; set; } = "#0f172a";

        [Display(Name = "Firma Logosu")]
        public IFormFile LogoFile { get; set; }
    }
}
