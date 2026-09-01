using System.Collections.Generic;
using GMK360.Core.Entities;

namespace GMK360.Web.Models.Dashboard
{
    public class ManagementDashboardViewModel
    {
        public ManagementCompany Company { get; set; }
        
        // Özet İstatistikler
        public int TotalHousingComplexes { get; set; }
        public int TotalBuildings { get; set; }
        public int TotalUnits { get; set; }
        public int TotalEmptyUnits { get; set; }
        
        // Finansal Durum
        public decimal TotalMonthlyExpenses { get; set; }
        public decimal TotalCollectedDebts { get; set; }
        public decimal TotalUnpaidDebts { get; set; }
        
        // Listeler
        public List<HousingComplex> HousingComplexes { get; set; }
        public List<Building> IndependentBuildings { get; set; }
        
        // Son Hareketler / Talepler
        public List<RenovationRequest> ActiveMaintenanceRequests { get; set; }
    }
}
