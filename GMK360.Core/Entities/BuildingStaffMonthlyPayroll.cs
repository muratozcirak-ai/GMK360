using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    /// <summary>
    /// Aylık puantaj, resmi/dini bayram çift yevmiye (+1) tahakkuku, maaş ve SGK ödeme eşleşmesi.
    /// </summary>
    public class BuildingStaffMonthlyPayroll : BaseEntity
    {
        public int BuildingStaffId { get; set; }
        [ForeignKey("BuildingStaffId")]
        public virtual BuildingStaff Staff { get; set; } = null!;

        public int Year { get; set; }
        public int Month { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseNetSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSgkEmployerShare { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseIncomeAndStampTax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseFringeBenefits { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlySeveranceFund { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal HolidayDaysWorked { get; set; } // Örn: 3.5 veya 4 gün

        [NotMapped]
        public decimal DailyWageRate => BaseNetSalary > 0 ? Math.Round(BaseNetSalary / 30m, 2) : 0m;

        [NotMapped]
        public decimal HolidayExtraPay => Math.Round(HolidayDaysWorked * DailyWageRate, 2);

        public int UnpaidLeaveDays { get; set; }

        [NotMapped]
        public decimal UnpaidLeaveDeduction => Math.Round(UnpaidLeaveDays * DailyWageRate, 2);

        [Column(TypeName = "decimal(18,2)")]
        public decimal ReplacementStaffCost { get; set; }

        [NotMapped]
        public decimal NetPayableToStaff => Math.Max(0m, BaseNetSalary + HolidayExtraPay - UnpaidLeaveDeduction);

        [NotMapped]
        public decimal TotalSiteCostForMonth =>
            NetPayableToStaff + BaseSgkEmployerShare + BaseIncomeAndStampTax + BaseFringeBenefits + ReplacementStaffCost;

        public bool IsPaid { get; set; } // Geriye donuk uyum / tam ödendi flag'i
        public DateTime? PaymentDate { get; set; }

        public bool IsSalaryPaid { get; set; }
        public DateTime? SalaryPaidAt { get; set; }

        public bool IsSgkPaid { get; set; }
        public DateTime? SgkPaidAt { get; set; }

        public int? PaidFromBankAccountId { get; set; }
        public int? OperatingExpenseId { get; set; }
        public int? SeveranceExpenseId { get; set; }
    }
}
