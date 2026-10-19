using System;
using System.Collections.Generic;
using GMK360.Core.Entities;

namespace GMK360.Web.Models
{
    public class LandlordDashboardViewModel
    {
        public List<Property> Properties { get; set; } = new List<Property>();
        public List<BuildingUnit> Units { get; set; } = new List<BuildingUnit>();

        // Portföy İstatistikleri
        public int TotalAssetsCount => UnitCards.Count;
        public int DistinctBuildingsCount { get; set; }
        public int RentedUnitsCount { get; set; }
        public int VacantUnitsCount { get; set; }
        public decimal TotalExpectedRent { get; set; }
        public decimal TotalCollectedRent { get; set; }
        public decimal TotalPendingRent { get; set; }

        // Binaların Yönetimlerinden Bu Ev Sahibine Gelen Demirbaş & Aidat Borçları
        public decimal TotalBuildingDebts { get; set; }
        public decimal TotalCapExPaidForTaxDeduction { get; set; } // GMSİ Gerçek Gider İndirimi matrahı

        // Her Mülkün / Dairenin Durum Kartı
        public List<LandlordUnitCardViewModel> UnitCards { get; set; } = new List<LandlordUnitCardViewModel>();
    }

    public class LandlordUnitCardViewModel
    {
        public int? UnitId { get; set; }
        public int? PropertyId { get; set; }
        public int BuildingId { get; set; }
        public string Title { get; set; }
        public string BuildingName { get; set; }
        public string BuildingAddress { get; set; }
        public string BlockAndDoor { get; set; }
        public string Category { get; set; } // Daire, Dukkan
        public string Layout { get; set; } // 3+1, 2+1
        public double SquareMeters { get; set; }
        
        // Kiracı Bilgileri
        public bool IsRented { get; set; }
        public string? TenantName { get; set; }
        public string? TenantPhone { get; set; }
        public decimal MonthlyRent { get; set; }
        public int RentDueDay { get; set; } = 5;
        public bool IsRentPaidThisMonth { get; set; }
        public int OverdueDays { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }

        // Bina Yönetimi Carisi (Bu daire için yönetimin kestiği borçlar)
        public decimal BuildingDebtAmount { get; set; }
        public string? BuildingDebtDescription { get; set; }
        public decimal PaidCapExAmount { get; set; }

        // Daire İçi Demirbaş Zimmeti (Kombi, Klima vb.)
        public List<SpaceFixture> PrivateFixtures { get; set; } = new List<SpaceFixture>();
        public string? MeterElectric { get; set; }
        public string? MeterWater { get; set; }
        public string? MeterGas { get; set; }

        // Emlak İlanı Var mı?
        public bool HasActiveListing { get; set; }
        public int? LinkedPropertyId { get; set; }
    }
}
