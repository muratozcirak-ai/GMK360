using System.Collections.Generic;

namespace GMK360.Web.Models.Finance
{
    public class UnifiedFinanceDashboardViewModel
    {
        public List<WhiteCollarSalaryItem> WhiteCollars { get; set; } = new();
        public List<BlueCollarWageItem> BlueCollars { get; set; } = new();
        public List<GMK360.Core.Entities.Finance.AgencyStaffAdvance> PendingAdvances { get; set; } = new();
        
        public decimal TotalWhiteCollarNet { get; set; }
        public decimal TotalWhiteCollarSgk { get; set; }
        public decimal TotalBlueCollarWage { get; set; }
        public decimal TotalFieldExpenses { get; set; } // Elden Gider
        
        public decimal TotalEmployerCost => TotalWhiteCollarNet + TotalWhiteCollarSgk + TotalBlueCollarWage + TotalFieldExpenses;
    }

    public class WhiteCollarSalaryItem
    {
        public string FullName { get; set; }
        public string RoleName { get; set; }
        public decimal NetSalary { get; set; }
        public decimal SgkCost { get; set; }
        public decimal TotalCost => NetSalary + SgkCost;
    }

    public class BlueCollarWageItem
    {
        public string FullName { get; set; }
        public string Profession { get; set; }
        public int DaysWorked { get; set; }
        public decimal EarnedWages { get; set; } // Tam/Yarım gün toplam yevmiyeler
        public decimal TotalPendingFieldExpenses { get; set; } // Elden Gider Toplamı (mesai vs)
        public decimal TotalCost => EarnedWages + TotalPendingFieldExpenses;
    }
}