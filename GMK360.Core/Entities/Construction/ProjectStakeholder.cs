using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities.Construction
{
    public enum StakeholderRole
    {
        Landowner = 1,       // Arsa Sahibi / Hak Sahibi
        Representative = 2,  // Yasal Temsilci / Avukat
        Consultant = 3,      // Proje Takipçisi / Müşavir
        Other = 4
    }

    public class ProjectStakeholder : BaseEntity
    {
        [Required]
        public int ProjectId { get; set; }
        
        [ForeignKey("ProjectId")]
        public ConstructionProject Project { get; set; }

        [Required]
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public StakeholderRole Role { get; set; } = StakeholderRole.Landowner;

        [Column(TypeName = "decimal(5,2)")]
        public decimal? SharePercentage { get; set; } // Sadece Arsa Sahibi için

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(50)]
        public string? Iban { get; set; }

        [MaxLength(200)]
        public string? UnitNumbers { get; set; } // Örn: "Daire 2, Daire 5, Daire 8"

        public int? UnitCount { get; set; } = 1;

        [MaxLength(50)]
        public string? ShareRatio { get; set; } // Örn: "1/5 (%20)"

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlyRentAllowance { get; set; } = 0; // Aylık Kira Yardımı

        [Column(TypeName = "decimal(18,2)")]
        public decimal? RelocationAllowance { get; set; } = 0; // Taşınma Yardımı

        [MaxLength(50)]
        public string? PaymentStatus { get; set; } = "Bekliyor"; // "Bekliyor", "Ödendi"

        public int? PaymentDay { get; set; } = 5; // Örn: 5 (Her ayın 5'i)

        [MaxLength(100)]
        public string? BankName { get; set; }

        [MaxLength(500)]
        public string? ReceiptUrl { get; set; } // Yüklenen Dekont Linki

        public DateTime? LastPaymentDate { get; set; }

        public DateTime? RentStartDate { get; set; } // Kira Başlangıç / Tahliye Tarihi
        public DateTime? RentEndDate { get; set; }   // Taahhüt Edilen Bitiş / Teslim Tarihi
    }
}
