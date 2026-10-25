using System;
using System.Collections.Generic;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Construction;

namespace GMK360.Web.Models
{
    public class UnitMaterialSelectionViewModel
    {
        public BuildingUnit Unit { get; set; } = null!;
        public List<ProjectMaterialCatalog> CatalogItems { get; set; } = new();
        public List<UnitMaterialSelection> Selections { get; set; } = new();
        public Dictionary<string, List<ProjectMaterialCatalog>> GroupedCatalog { get; set; } = new();
        public Dictionary<string, UnitMaterialSelection> SelectionsByCategory { get; set; } = new();
        public int TotalCategoriesCount { get; set; }
        public int ApprovedCategoriesCount { get; set; }
        public decimal TotalPriceDifference { get; set; }
    }

    public class MaterialOtpSendRequest
    {
        public int UnitId { get; set; }
        public int CatalogId { get; set; }
        public string Phone { get; set; } = string.Empty;
    }

    public class MaterialOtpVerifyRequest
    {
        public int UnitId { get; set; }
        public int CatalogId { get; set; }
        public string Otp { get; set; } = string.Empty;
    }

    public class UnitMaterialReportViewModel
    {
        public Building Building { get; set; } = null!;
        public List<BuildingUnit> Units { get; set; } = new();
        public List<UnitMaterialSelection> AllSelections { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public Dictionary<int, Dictionary<string, UnitMaterialSelection>> SelectionsByUnitAndCategory { get; set; } = new();
        public Dictionary<string, int> SelectionsCountByMaterial { get; set; } = new();
    }
}
