using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GMK360.Web.Models
{
    public class PropertyCreateViewModel
    {
        public int Id { get; set; } // Taslak ID'si
        public int DraftStep { get; set; } // Hangi adımda olduğu

        [Required(ErrorMessage = "İlan başlığı zorunludur.")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Range(1, double.MaxValue, ErrorMessage = "Geçerli bir fiyat giriniz.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "İlan durumu seçimi zorunludur.")]
        public int StatusId { get; set; }

        [Required(ErrorMessage = "İlan tipi seçimi zorunludur.")]
        public int TypeId { get; set; }

        [Required(ErrorMessage = "Alt tür seçimi zorunludur.")]
        public int SubTypeId { get; set; }

        public int? FromWhomId { get; set; }

        public bool ShowAgencyInfo { get; set; } = true;

        public string Category { get; set; }

        // Dinamik Seçim Kutuları (Dropdownlar)
        public Dictionary<int, int> DynamicSelects { get; set; } = new Dictionary<int, int>();

        // EİDS Yetki Belgesi Alanları
        public bool HasAuthorization { get; set; }
        public string AuthorizationDocumentNo { get; set; }
        public string OwnerIdNumber { get; set; }

        [Required(ErrorMessage = "Brüt m² zorunludur.")]
        public int GrossArea { get; set; }

        [Required(ErrorMessage = "Net m² zorunludur.")]
        public int NetArea { get; set; }

        [Required(ErrorMessage = "Dış Kapı / Bina No zorunludur.")]
        public string BuildingNumber { get; set; } // Dış Kapı / Bina No
        [Required(ErrorMessage = "İç Kapı / Daire No zorunludur.")]
        public string UnitNumber { get; set; } // İç Kapı / Daire No
        [Required(ErrorMessage = "Binadaki Kat Sayısı zorunludur.")]
        public int? TotalFloors { get; set; } // Binanın Toplam Kat Sayısı

        // DİNAMİK ÖZELLİKLER (Migration gerektirmeyen Anahtar-Değer yapısı)
        public Dictionary<int, string> DynamicFeatures { get; set; } = new Dictionary<int, string>();

        // Yakın Çevre Tesisleri (POI)
        public List<int> SelectedPoiIds { get; set; } = new List<int>();

        // Konum Bilgileri
        [Required(ErrorMessage = "İl seçimi zorunludur.")]
        public int CityId { get; set; }

        [Required(ErrorMessage = "İlçe seçimi zorunludur.")]
        public int DistrictId { get; set; }

        [Required(ErrorMessage = "Mahalle seçimi zorunludur.")]
        public int NeighborhoodId { get; set; }

        public int? StreetId { get; set; }

        public bool IsSite { get; set; } = true; // Gerçek bir site mi yoksa bağımsız bina mı?
        public int? ComplexId { get; set; } // Arsa durumunda zorunlu değil
        public string BuildingName { get; set; } // AJAX öncesi taslak

        public bool HasBlock { get; set; } // "Blok/Giriş var mı?"
        public string BlockName { get; set; } // Örn: "A Blok", "B Girişi"

        // Yeni Eklenenler (Aidat ve Arsa Hissesi)
        public decimal? Dues { get; set; } 
        public string DeedStatus { get; set; } 
        public string BlockNumber { get; set; } 
        public string ParcelNumber { get; set; } 
        public double? TotalAreaSqm { get; set; } 
        public double? ShareAreaSqm { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool HideLocation { get; set; } = false;

        public string RecaptchaToken { get; set; }
        public List<IFormFile> UploadedImages { get; set; }
        public IFormFile UploadedVideo { get; set; }
        public string VideoUrl { get; set; }
        
        public List<PropertyImageDto> ProcessedImages { get; set; } = new List<PropertyImageDto>();

        public List<int> SelectedFeatureIds { get; set; } = new List<int>();
        
        // Yeni Dinamik Matris Özellikleri Listesi
        public List<MatrixFeatureDto> MatrixFeatures { get; set; } = new List<MatrixFeatureDto>();
        public List<MatrixFeatureDto> MatrixFeaturesComplex { get; set; } = new List<MatrixFeatureDto>();

        // Yeni Eklenen Sabit Sütunlar
        public string RoomCount { get; set; }
        public int? BathroomCount { get; set; }
        public int? BalconyCount { get; set; }
        public int? WcCount { get; set; }
        public string BuildingAge { get; set; }
        public int? HeatingId { get; set; }
        public bool IsFurnished { get; set; }
        public string FurnitureDetails { get; set; } // Eşya detayları


        public int? FloorNumber { get; set; } // Zorunlu Değil
        public string Facade { get; set; } // Cephe

        // Devre Mülk Alanları
        public bool IsTimeshare { get; set; }
        public System.DateTime? TimeshareStartDate { get; set; }
        public System.DateTime? TimeshareEndDate { get; set; }
        public string TimesharePeriod { get; set; }

        [Required(ErrorMessage = "Para birimi seçilmelidir.")]
        public string Currency { get; set; } = "TRY";
    }

    public class MatrixFeatureDto
    {
        public int DefinitionValueId { get; set; }
        public bool IsSelected { get; set; }
        public int? Count { get; set; }
        public List<string> SubOptions { get; set; } = new List<string>();
        public string Note { get; set; }
    }

    public class PropertyImageDto
    {
        public string ImageUrl { get; set; }
        public string TempPath { get; set; } // Uploaded temp identifier
        public string RoomTag { get; set; }
        public int SortOrder { get; set; }
        public bool IsVideo { get; set; }
    }
}
