using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class ServiceProviderDocument : BaseEntity
    {
        [Required]
        public int ServiceProviderId { get; set; }
        
        [ForeignKey("ServiceProviderId")]
        public virtual ServiceProvider ServiceProvider { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; } = null!; // "Vergi Levhası", "Ustalık Belgesi", "Kimlik" vb.

        [Required]
        [MaxLength(500)]
        public string DocumentUrl { get; set; } = null!; // Dosyanın/Görselin yüklendiği adres

        public bool IsApproved { get; set; } = false; // Sistem Yöneticisi onayladı mı?

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
