using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class Building
    {
        public int Id { get; set; }
        public string Name { get; set; } // Örn: Güneş Apartmanı, Vadi Evleri
        
        public int? HousingComplexId { get; set; } // Hangi Siteye Bağlı? (Bağımsız apartmansa null)
        public HousingComplex HousingComplex { get; set; }
        
        public string ManagerUserId { get; set; }
        public GMK360.Core.Entities.Identity.ApplicationUser ManagerUser { get; set; }
        
        public string? TaxNumber { get; set; }
        public int TotalUnits { get; set; }
        
        public bool HasBlock { get; set; } // "Blok/Giriş var mı?"
        public string BlockName { get; set; } // Örn: "A Blok", "B Girişi"
                public int? TotalFloors { get; set; } // Binadaki Toplam Normal Kat Sayısı
        public int BasementFloors { get; set; } = 0; // Bodrum kat sayısı (Örn: 2)
        public bool HasGroundFloor { get; set; } = true; // Zemin kat var mı?
        
        // Bu bina eğer bir inşaat projesinin parçası (Blok) ise
        public int? ConstructionProjectId { get; set; }
        public virtual GMK360.Core.Entities.Construction.ConstructionProject ConstructionProject { get; set; }
        
        // 5'Lİ TEKİL (UNIQUE) ADRES HİYERARŞİSİ
        public int CityId { get; set; }
        public virtual City City { get; set; }

        public int DistrictId { get; set; }
        public virtual District District { get; set; }

        public int NeighborhoodId { get; set; }
        public virtual Neighborhood Neighborhood { get; set; }

        public int? StreetId { get; set; }
        public virtual Street Street { get; set; } // Entity Navigation
        
        public string StreetName { get; set; } // Sokak / Cadde adı (Serbest metin gerekirse)
        public string BuildingNumber { get; set; } // Dış Kapı No
        
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool IsApproved { get; set; } = true;
        
        [NotMapped]
        public string Address { get => $"{StreetName} {BuildingNumber}"; set { } }
        
        public ICollection<ComplexFeature> Features { get; set; } = new List<ComplexFeature>();
        
        public bool HasBlocks { get; set; } = false;
        public ICollection<ComplexBlock> Blocks { get; set; } = new List<ComplexBlock>();
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<BuildingUnit> Units { get; set; } = new List<BuildingUnit>();
        public virtual ICollection<ManagementMember> ManagementMembers { get; set; } = new List<ManagementMember>();
        public virtual ICollection<BuildingExpense> Expenses { get; set; } = new List<BuildingExpense>();
        public virtual ICollection<BuildingManager> BuildingManagers { get; set; } = new List<BuildingManager>();
        
        // Multi-Step Onboarding için durum takibi:
        // 1: Bina Adresi girildi
        // 2: Daireler/Kişiler eklendi
        // 3: Sabit giderler/Sözleşmeler eklendi
        // 4: Karar defteri okundu
        // 5: İhale/Usta kurgusu tamamlandı (Tamamen aktif)
        public int OnboardingStep { get; set; } = 1;
    }
}
