using System;
using System.Collections.Generic;

namespace GMK360.Web.Models
{
    public class ProjectShowcaseItem
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ConstructionCompany { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string ArchitecturalConcept { get; set; } = string.Empty;
        public int TotalBlocks { get; set; } = 4;
        public int TotalUnits { get; set; } = 48;
        public string UnitTypes { get; set; } = "2+1, 3+1, 4+1";
        public string EstimatedCompletionDate { get; set; } = "2027 1. Çeyrek";
        public string SocialFacilities { get; set; } = "Kapalı Otopark, Havuz, Fitness, Güvenlik, Çocuk Parkı";
        public string StructuralSafety { get; set; } = "C35/45 Beton, Radye Jeneral Temel, Deprem Yönetmeliği Uyumlu";
        public string RealtorAuthorityModel { get; set; } = "Bölgedeki Tüm Onaylı Emlakçılara Açık Bilgilendirme";
        public int PreDemandCount { get; set; } = 24;
        public string HeroImageUrl { get; set; } = "/images/project-concept-1.jpg";
    }

    public class ProjectPreDemandRequest
    {
        public int ProjectId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string InterestedUnitType { get; set; } = "3+1";
        public string PaymentPreference { get; set; } = "Peşin / Erken Rezervasyon";
        public string Notes { get; set; } = string.Empty;
    }

    public class ProjectShowcaseIndexViewModel
    {
        public List<ProjectShowcaseItem> Projects { get; set; } = new List<ProjectShowcaseItem>();
    }
}
