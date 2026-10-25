using System;
using System.Collections.Generic;

namespace GMK360.Core.DTOs
{
    public class FeasibilityPdfRequestDto
    {
        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? CompanyName { get; set; }
        public string? CityName { get; set; }
        public string? DistrictName { get; set; }
        public string? NeighborhoodName { get; set; }

        public string? Ada { get; set; }
        public string? Parsel { get; set; }
        public string? Pafta { get; set; }
        public string? Nitelik { get; set; }
        public string? Mevkii { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public double LandArea { get; set; }
        public double Emsal { get; set; }
        public double Taks { get; set; }
        public int TotalFloors { get; set; }
        public double FlatSqm { get; set; } = 100;

        public int OldUnits { get; set; }
        public int EstimatedUnits { get; set; }
        public int SurplusUnits { get; set; }

        public double TotalBuildingArea { get; set; }
        public double GroundBaseArea { get; set; }

        public string? ImarNotlari { get; set; }
        public string? MapImageBase64 { get; set; }

        public List<FeasibilityScenarioDto> Scenarios { get; set; } = new List<FeasibilityScenarioDto>();
        public List<FeasibilityDocumentStatusDto> Documents { get; set; } = new List<FeasibilityDocumentStatusDto>();
    }

    public class FeasibilityScenarioDto
    {
        public string Title { get; set; } = string.Empty;
        public int Floors { get; set; }
        public int UnitsPerFloor { get; set; }
        public int TotalUnits { get; set; }
        public int OwnerUnits { get; set; }
        public int ContractorUnits { get; set; }
        public double OwnerSharePercent { get; set; }
        public double UnitSqm { get; set; }
        public string UnitType { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal TotalRevenue { get; set; }
        public double TotalConstSqm { get; set; }
        public decimal FirmCost { get; set; }
        public decimal NetProfit { get; set; }
    }

    public class FeasibilityDocumentStatusDto
    {
        public string DocumentName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalFee { get; set; }
        public string? Institution { get; set; }
    }
}
