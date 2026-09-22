using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GMK360.Web.Models
{
    public class CreateProjectWizardViewModel
    {
        [Required(ErrorMessage = "Proje ad zorunludur.")]
        public int DraftProjectId { get; set; }

        [Required(ErrorMessage = "Proje ad zorunludur.")]
        public string Name { get; set; }

        public string Description { get; set; }
        public string? ProjectType { get; set; }
        public int StatusId { get; set; } = 1;
        
        // Kentsel Dönüşüm / Aday Proje Kayıtları
        public int? ProjectOriginId { get; set; }
        public string? ProjectOwnerContact { get; set; }
        public int? EskiKatSayisi { get; set; }
        public int? EskiDaireSayisi { get; set; }
        public double? EskiToplamMetrekare { get; set; }
        public int? EskiDukkanSayisi { get; set; }
        public int? EskiBodrumKatSayisi { get; set; }
        public bool EskiCatiKatiVarMi { get; set; }
        public int? BinaYoneticisiId { get; set; }
        
        public string? TemsilciAd { get; set; }
        public string? TemsilciSoyad { get; set; }
        public string? TemsilciTelefon { get; set; }
        
        [Required(ErrorMessage = "Adres zorunludur.")]
        public string Address { get; set; }

        public string Ada { get; set; }
        public string Parsel { get; set; }
        public bool IsNewDesignForExisting { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile TapuDocumentFile { get; set; }


        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public IFormFile CoverImageFile { get; set; }
        public IFormFile CurrentStateImageFile { get; set; }
        
        // Lokasyon Hiyerarisi
        [Required(ErrorMessage = "l seimi zorunludur.")]
        public int CityId { get; set; }
        [Required(ErrorMessage = "le seimi zorunludur.")]
        public int DistrictId { get; set; }
        [Required(ErrorMessage = "Mahalle seimi zorunludur.")]
        public int NeighborhoodId { get; set; }
        public int? StreetId { get; set; }
        
        // Harita Koordinatlar
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public double? TotalLandArea { get; set; }
        public int? TargetTotalApartments { get; set; }
        public int? TargetTotalShops { get; set; }

        public List<WizardBlockItem> Blocks { get; set; } = new List<WizardBlockItem>();
        public List<WizardBlockItem> ExistingBlocks { get; set; } = new List<WizardBlockItem>();
        public List<WizardBlockItem> TargetBlocks { get; set; } = new List<WizardBlockItem>();
        public List<WizardOwnerItem> Owners { get; set; } = new List<WizardOwnerItem>();
    }
    
    public class WizardOwnerItem
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string FlatNumber { get; set; }
          public string? BlockName { get; set; }
          public string? Email { get; set; }
          public string? UnitType { get; set; }
        public bool IsRepresentative { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class WizardBlockItem
    {
        public string BlockName { get; set; }
        public double? BaseArea { get; set; }
        public int BasementFloors { get; set; }
        public int TotalFloors { get; set; }
        public int TotalApartments { get; set; }
        public int TotalShops { get; set; }
        public bool HasRoof { get; set; }
        public bool HasGroundFloor { get; set; } = true;
        public bool IsExistingBuilding { get; set; } = false;
        public string? LayoutPattern { get; set; } // Ortak Baza, Bağımsız (Tek Temel) vb.
    }
}



