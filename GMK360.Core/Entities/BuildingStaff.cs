using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    /// <summary>
    /// Bina / Site sabit personeli (Kapıcı, Güvenlik, Temizlik, Bahçıvan, Teknik) ve aylık tam bordro kalemleri.
    /// İşletme maliyeti oturan/kiracıya, aylık kıdem tazminatı karşılığı ise ayrı fonda Kat Maliki hesabına yansır.
    /// </summary>
    public class BuildingStaff : BaseEntity
    {
        public int BuildingId { get; set; }
        [ForeignKey("BuildingId")]
        public virtual Building Building { get; set; } = null!;

        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(450)]
        public string? UserId { get; set; }

        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        [MaxLength(256)]
        public string? QuickPinHash { get; set; }

        [MaxLength(11)]
        public string? TcIdentityNo { get; set; }

        [MaxLength(50)]
        public string? SgkNumber { get; set; }

        [Required, MaxLength(80)]
        public string RoleType { get; set; } = "Kapıcı (Vergi İstisnalı)"; // "Kapıcı (Vergi İstisnalı)", "Güvenlik", "Temizlik", "Bahçıvan", "Teknik"

        // Aylık Tam Bordro Kalemleri (İşletme / Aidat Kasası)
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SgkEmployerShare { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal IncomeAndStampTax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal FringeBenefits { get; set; } // Yol, yemek, yakacak/lojman desteği

        // Her ay ayrı fonda biriktirilen Kıdem Tazminatı Karşılığı (Malik / Demirbaş Hesabına)
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlySeveranceFund { get; set; }

        [NotMapped]
        public decimal TotalMonthlyOperatingCost =>
            NetSalary + SgkEmployerShare + IncomeAndStampTax + FringeBenefits;

        public DateTime HireDate { get; set; } = DateTime.Today;
        public DateTime? LeaveDate { get; set; }
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 4857 Sayılı İş Kanunu kıdem süresine göre yıllık izin hakkı (1-5 yıl: 14, 5-15 yıl: 20, 15+ yıl: 26 gün).
        /// </summary>
        [NotMapped]
        public int AnnualLeaveEntitlementDays
        {
            get
            {
                var refDate = LeaveDate ?? DateTime.Today;
                int years = refDate.Year - HireDate.Year;
                if (refDate.Date < HireDate.Date.AddYears(years)) years--;
                if (years < 1) return 0;
                if (years < 5) return 14;
                if (years < 15) return 20;
                return 26;
            }
        }

        public virtual ICollection<BuildingStaffLeave> Leaves { get; set; } = new List<BuildingStaffLeave>();
        public virtual ICollection<BuildingStaffMonthlyPayroll> MonthlyPayrolls { get; set; } = new List<BuildingStaffMonthlyPayroll>();
    }
}
