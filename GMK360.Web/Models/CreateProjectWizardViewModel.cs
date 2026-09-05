using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GMK360.Web.Models
{
    public class CreateProjectWizardViewModel
    {
        [Required(ErrorMessage = "Proje adı zorunludur.")]
        public int DraftProjectId { get; set; }

        [Required(ErrorMessage = "Proje adı zorunludur.")]
        public string Name { get; set; }

        public string Description { get; set; }
        
        [Required(ErrorMessage = "Adres zorunludur.")]
        public string Address { get; set; }

        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public IFormFile CoverImageFile { get; set; }
        public IFormFile CurrentStateImageFile { get; set; }
        
        // Lokasyon Hiyerarşisi
        [Required(ErrorMessage = "İl seçimi zorunludur.")]
        public int CityId { get; set; }
        [Required(ErrorMessage = "İlçe seçimi zorunludur.")]
        public int DistrictId { get; set; }
        [Required(ErrorMessage = "Mahalle seçimi zorunludur.")]
        public int NeighborhoodId { get; set; }
        public int? StreetId { get; set; }
        
        // Harita Koordinatları
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public double? TotalLandArea { get; set; }
        public int? TargetTotalApartments { get; set; }
        public int? TargetTotalShops { get; set; }

        public List<WizardBlockItem> Blocks { get; set; } = new List<WizardBlockItem>();
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
    }
}

