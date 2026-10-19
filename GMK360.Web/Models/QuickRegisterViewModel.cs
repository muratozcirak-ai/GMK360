using System.ComponentModel.DataAnnotations;

namespace GMK360.Web.Models
{
    public class QuickRegisterViewModel
    {
        [Required(ErrorMessage = "Lütfen faaliyet alanınızı / rolünüzü seçiniz.")]
        public string SectorRole { get; set; } = "Insaat"; // Insaat, Yikim, BinaYonetimi, Tedarikci, Usta

        // İnşaat / Şantiye alt faaliyet kapsamları (3'ü birden veya seçmeli)
        public bool ScopeBuilding { get; set; } = true;    // Bina Yapımı & Müteahhitlik
        public bool ScopeDemolition { get; set; } = true;  // Yıkım & Söküm İşleri
        public bool ScopeExcavation { get; set; } = true;  // Temel Kazısı & Hafriyat

        [Required(ErrorMessage = "Firma veya işletme unvanı zorunludur.")]
        [Display(Name = "Firma / İşletme Adı")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yetkili kişi adı soyadı zorunludur.")]
        [Display(Name = "Yetkili Adı Soyadı")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [Display(Name = "Telefon Numarası")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-Posta Adresi")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre belirleyiniz.")]
        [StringLength(100, ErrorMessage = "{0} en az {2} karakter olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
