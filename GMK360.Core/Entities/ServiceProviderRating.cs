using System;
using System.ComponentModel.DataAnnotations;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class ServiceProviderRating : BaseEntity
    {
        public int ServiceProviderId { get; set; }
        public ServiceProvider ServiceProvider { get; set; }

        public string UserId { get; set; } // Puanı veren Müşteri
        public ApplicationUser User { get; set; }

        [Required(ErrorMessage = "Puan vermek zorunludur.")]
        [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
        public int Rating { get; set; } // 1-5 arası puan
        
        [Required(ErrorMessage = "Yorum yazmak zorunludur.")]
        [MaxLength(1000)]
        public string Comment { get; set; } = null!;

        public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsApproved { get; set; } = false; // Yorumlar Admin onayına düşsün
    }
}
