using System.ComponentModel.DataAnnotations;

namespace GMK360.Web.Models
{
    public class OnboardingViewModel
    {
        [Required(ErrorMessage = "Lütfen kullanıcı tipini seçiniz.")]
        public string SelectedUserType { get; set; }

        [Required(ErrorMessage = "Lütfen cep telefonu numaranızı giriniz.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Lütfen adınızı giriniz.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Lütfen soyadınızı giriniz.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "TC Kimlik Numaranızı giriniz.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TCKN 11 haneli olmalıdır.")]
        public string TcIdentityNo { get; set; }

        [Required(ErrorMessage = "Doğum yılınızı giriniz.")]
        [Range(1900, 2100, ErrorMessage = "Geçerli bir doğum yılı giriniz.")]
        public int BirthYear { get; set; }

        public bool IsShadowAccount { get; set; }
    }
}

