using System;
using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities
{
    public class SmsTemplate : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string TemplateCode { get; set; } // Örn: "USER_REGISTER", "NEW_OFFER", "REFERRAL_EARNING"

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } // Admin panel için açıklama: "Kullanıcı Kayıt Mesajı"

        [Required]
        [MaxLength(1000)]
        public string Body { get; set; } // Örn: "Sayın {FullName}, GMK360'ya hoşgeldiniz!"

        public bool IsActive { get; set; } = true;
    }
}
