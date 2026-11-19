using System.Collections.Generic;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Construction;

namespace GMK360.Web.Models
{
    public class FacilityWizardPageViewModel
    {
        public int BuildingId { get; set; }
        public int? HousingComplexId { get; set; }
        public string ManagementMode { get; set; } = "Apartman"; // "Apartman" | "Site"
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? BuildingAge { get; set; }
        public string? TaxNumber { get; set; }

        public int CityId { get; set; }
        public int DistrictId { get; set; }
        public int NeighborhoodId { get; set; }
        public int? StreetId { get; set; }
        public string StreetName { get; set; } = string.Empty;
        public string BuildingNumber { get; set; } = string.Empty;
        public string? ExtraAddressLabel { get; set; }
        public string? Ada { get; set; }
        public string? Parsel { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }

        public Building? RootBuilding { get; set; }
        public List<DocumentArchive> Photos { get; set; } = new List<DocumentArchive>();
        public DocumentArchive? DecisionDocument { get; set; }
        public List<FacilityStep2StructureDto> InitialStructures { get; set; } = new List<FacilityStep2StructureDto>();
    }

    public class FacilityStep1FormModel
    {
        public int BuildingId { get; set; }
        public string ManagementMode { get; set; } = "Apartman";
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? BuildingAge { get; set; }
        public string? TaxNumber { get; set; }

        public int CityId { get; set; }
        public int DistrictId { get; set; }
        public int NeighborhoodId { get; set; }
        public int? StreetId { get; set; }
        public string? StreetName { get; set; }
        public string BuildingNumber { get; set; } = string.Empty;
        public string? ExtraAddressLabel { get; set; }
        public string? Ada { get; set; }
        public string? Parsel { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
    }

    public class FacilityStep2Request
    {
        public int BuildingId { get; set; }
        public string ManagementMode { get; set; } = "Apartman";
        public List<FacilityStep2StructureDto> Structures { get; set; } = new List<FacilityStep2StructureDto>();
    }

    public class FacilityStep2StructureDto
    {
        public string LayoutPattern { get; set; } = "Tekil Yapı"; // "Tekil Yapı" | "Ortak Baza" | "Bitişik Nizam"
        public string BlockName { get; set; } = "Ana Bina";
        public double? BaseArea { get; set; }
        public int TotalFloors { get; set; } = 5;
        public int TotalApartments { get; set; } = 10;
        public int TotalShops { get; set; } = 0;
        public int BasementFloors { get; set; } = 1;
        public bool HasGroundFloor { get; set; } = true;
        public bool HasRoof { get; set; } = true;
        public string RoofType { get; set; } = "Düz Teras Çatı (Tabliye)";
        public List<FacilityStep2ChildBlockDto> Children { get; set; } = new List<FacilityStep2ChildBlockDto>();
    }

    public class FacilityStep2ChildBlockDto
    {
        public string BlockName { get; set; } = "A Blok";
        public int TotalFloors { get; set; } = 8;
        public int TotalApartments { get; set; } = 16;
        public int TotalShops { get; set; } = 0;
        public int BasementFloors { get; set; } = 0;
        public bool HasGroundFloor { get; set; } = false;
        public bool HasRoof { get; set; } = true;
        public string RoofType { get; set; } = "Düz Teras Çatı (Tabliye)";
    }
}
