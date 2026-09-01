using System.ComponentModel.DataAnnotations;
using GMK360.Core.Entities.Identity;

namespace GMK360.Web.Models
{
    public class RegisterViewModel
    {
        // Ortak Alanlar
        [Display(Name = "Ad")]
        public string? FirstName { get; set; }

        [Display(Name = "Soyad")]
        public string? LastName { get; set; }

        // Bu alanlar artık temel kayıtta zorunlu değil (Onboarding'de doğrulanacak)
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik Numarası 11 haneli olmalıdır.")]
        [Display(Name = "TC Kimlik No")]
        public string? TcIdentityNo { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Geçerli bir tarih seçiniz.")]
        [Display(Name = "Doğum Tarihiniz")]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "E-Posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-Posta")]
        public string Email { get; set; }

        [Display(Name = "Cep Telefonu")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, ErrorMessage = "{0} en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Password", ErrorMessage = "Şifreler birbiriyle eşleşmiyor.")]
        public string ConfirmPassword { get; set; }

        // Bireysel mi Kurumsal mı?
        public UserType UserType { get; set; } // Individual veya Corporate

        // Kurumsal (Emlakçı) Özel Alanları
        [Display(Name = "Firma Adı (Örn: Remax Kadıköy)")]
        public string? CompanyName { get; set; }

        [Display(Name = "Vergi Numarası")]
        public string? TaxNumber { get; set; }

        [Display(Name = "Taşınmaz Ticareti Yetki Belgesi No")]
        public string? AuthorizationLicenseNumber { get; set; }

        [Display(Name = "Referans Kodunuz (Opsiyonel)")]
        public string? ReferralCode { get; set; }

        // Hizmet Veren (Usta) Özel Alanları
        public int? ServiceCategoryId { get; set; }

        // Davet/Shadow User Token (Gizli Alan)
        public string? ShadowToken { get; set; }

        // --- PAZARLAMA VE TAKİP ALANLARI ---
        public int? CampaignId { get; set; } // URL'den gelen kampanya ID'si
        public string? SelectedPlan { get; set; } // "standard" veya "pro"

    }
}
