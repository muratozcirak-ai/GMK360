using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    /// <summary>
    /// Bina personeli yıllık izin, ücretsiz izin, rapor ve yerine geçici ikame personel maliyeti takibi.
    /// </summary>
    public class BuildingStaffLeave : BaseEntity
    {
        public int BuildingStaffId { get; set; }
        [ForeignKey("BuildingStaffId")]
        public virtual BuildingStaff Staff { get; set; } = null!;

        public int LeaveYear { get; set; } = DateTime.Today.Year;

        [Required, MaxLength(40)]
        public string LeaveType { get; set; } = "YillikIzin"; // "YillikIzin", "UcretsizIzin", "Rapor", "BayramIzni"

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }

        [Required, MaxLength(30)]
        public string Status { get; set; } = "Planlandi"; // "Planlandi", "Onaylandi", "Tamamlandi"

        [Column(TypeName = "decimal(18,2)")]
        public decimal ReplacementCost { get; set; } // İzin sırasında geçici ikame personel ücreti

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
