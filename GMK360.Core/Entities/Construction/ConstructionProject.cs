using System;
using System.Collections.Generic;
using GMK360.Core.Entities.Enums;

namespace GMK360.Core.Entities.Construction
{
        public static class ProjectConstants
    {
        public const byte StatusTeklif = 1;
        public const byte StatusAktif = 2;
        public const byte StatusTamamlandi = 3;
    }

    public class ConstructionProject : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        
        public int AgencyId { get; set; }
        public Agency Agency { get; set; }

        public string? CoverImageUrl { get; set; }
        public string? CurrentStateImageUrl { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Address { get; set; }
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
        public int? NeighborhoodId { get; set; }
        public int? StreetId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        // 0=Upcoming, 1=Ongoing, 2=Completed
        public byte StatusId { get; set; } = ProjectConstants.StatusTeklif;

        public DateTime SelectionDeadline { get; set; }

        public virtual ICollection<ProjectPhase> Phases { get; set; }
        public virtual ICollection<ProjectMaterialCatalog> MaterialCatalogs { get; set; }
        public ProjectLifecycleStatus LifecycleStatus { get; set; } = ProjectLifecycleStatus.UnderConstruction;
        public bool IsPublishedOnWeb { get; set; } = false;
        public virtual ICollection<ConstructionTask> Tasks { get; set; }
        
        // Arsa ve Peyzaj Bilgileri
        public double? TotalLandArea { get; set; } // Toplam Arsa Alanı (m2)
        public double? LandscapeArea { get; set; } // Peyzaj Alanı (m2)

        // Projeye ait bloklar / binalar
        public virtual ICollection<Building> Blocks { get; set; }
        public virtual ICollection<UnitTemplate> UnitTemplates { get; set; } = new List<UnitTemplate>();
        
        // Açık Alanlar ve Sosyal Donatılar
        public virtual ICollection<ProjectAmenity> Amenities { get; set; } = new List<ProjectAmenity>();

        // Proje Çalışanları (Şantiye İzolasyonu için)
        public virtual ICollection<ProjectAssignment> Assignments { get; set; } = new List<ProjectAssignment>();

        public int? TargetTotalApartments { get; set; }
        public int? TargetTotalShops { get; set; }
    }
}


