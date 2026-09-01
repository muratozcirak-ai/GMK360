using System.Collections.Generic;
using GMK360.Core.Entities;

namespace GMK360.Web.Models.Dashboard
{
    public class PropertyOwnerDashboardViewModel
    {
        public int TotalProperties { get; set; }
        public decimal TotalMonthlyIncome { get; set; }
        public decimal TotalMonthlyExpense { get; set; }
        public int PendingInvoices { get; set; }
        public int ActiveRenovationRequests { get; set; }
        
        public List<Property> RecentProperties { get; set; } = new List<Property>();
    }
}
