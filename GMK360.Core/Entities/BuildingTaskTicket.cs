using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    /// <summary>
    /// Fotoğraflı saha talep ve görev atama döngüsü (Sakin/Yönetici -> Görevli -> Kanıt Fotoğrafı -> Arşiv).
    /// </summary>
    public class BuildingTaskTicket : BaseEntity
    {
        public int BuildingId { get; set; }
        [ForeignKey("BuildingId")]
        public virtual Building Building { get; set; } = null!;

        public int? BuildingUnitId { get; set; }
        [ForeignKey("BuildingUnitId")]
        public virtual BuildingUnit? BuildingUnit { get; set; }

        [MaxLength(450)]
        public string? CreatedByUserId { get; set; }

        [Required, MaxLength(30)]
        public string CreatedByRole { get; set; } = "Kiraci"; // "Kiraci", "MalSahibi", "Yonetici"

        [Required, MaxLength(50)]
        public string Category { get; set; } = "Temizlik/Cop"; // "Temizlik/Cop", "Elektrik/Aydinlatma", "Asansor", "Guvenlik", "Bahce", "Diger"

        [MaxLength(250)]
        public string? LocationNote { get; set; } // Örn: "B Blok 3. Kat Merdiven Boşluğu"

        [Required, MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? BeforePhotoUrl { get; set; }

        public int? AssignedStaffId { get; set; }
        [ForeignKey("AssignedStaffId")]
        public virtual BuildingStaff? AssignedStaff { get; set; }

        [Required, MaxLength(20)]
        public string Priority { get; set; } = "Normal"; // "Normal", "Acil"

        [Required, MaxLength(30)]
        public string Status { get; set; } = "Bekliyor"; // "Bekliyor", "Atandi", "Tamamlandi", "Iptal"

        [MaxLength(500)]
        public string? AfterPhotoUrl { get; set; }

        [MaxLength(500)]
        public string? StaffCompletionNote { get; set; }

        public DateTime? AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
