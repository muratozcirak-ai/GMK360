using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.DTOs
{
    public class KisiselVeriDTO
    {
        [Required(ErrorMessage = "TC Kimlik No zorunludur.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No tam olarak 11 hane olmalıdır.")]
        [RegularExpression(@"^[1-9]{1}[0-9]{10}$", ErrorMessage = "TC Kimlik No sadece rakamlardan oluşmalı ve 0 ile başlamamalıdır.")]
        public string TcKimlikNo { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad en az 2 karakter olmalıdır.")]
        [RegularExpression(@"^[a-zA-ZçÇğĞıİöÖşŞüÜ\s]+$", ErrorMessage = "Ad sadece harflerden oluşmalıdır.")]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad en az 2 karakter olmalıdır.")]
        [RegularExpression(@"^[a-zA-ZçÇğĞıİöÖşŞüÜ\s]+$", ErrorMessage = "Soyad sadece harflerden oluşmalıdır.")]
        public string Soyad { get; set; }
    }
}
