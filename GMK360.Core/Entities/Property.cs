using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Enums;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class Property : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "TRY"; // TRY, USD, EUR, GBP
        public int NetArea { get; set; }
        public int GrossArea { get; set; }
        public string? Side { get; set; } // Örn: "Avrupa Yakası", "Anadolu Yakası" (Arama optimizasyonu için)
        public bool IsBoosted { get; set; } // Doping/Vitrin (Sponsorlu)
        public DateTime? PromotedEndDate { get; set; } // Doping bitiş tarihi
        public string ICalUrl { get; set; } // Airbnb / Booking iCal Senkronizasyon Linki
        
        // Video Özellikleri
        public string VideoFilePath { get; set; } // Sunucuya yüklenen .mp4 dosya yolu
        public string VideoUrl { get; set; } // YouTube / Vimeo vb. dış link
        
        // Yaşam Döngüsü (Satıldı, Yayında vb.)
        public ListingState State { get; set; } = ListingState.Active;
        public int DraftStep { get; set; } = 0; // Hangi adımda taslak olarak kaydedildi? (0 = Taslak değil/tamamlanmış)
        
        // Bireysel Dijital Evim (Özel Takip) İçin
        public ManagementRole? ManagementRole { get; set; } // Sahibiyim veya Kiracıyım
        public bool IsResident { get; set; } = true; // Bu evde fiilen yaşıyor mu?

        // Kiracı Bilgileri (Eğer mülk sahibi yatırımcıysa ve kiraladıysa)
        public string? TenantName { get; set; }
        public string? TenantPhone { get; set; }
        public string? TenantEmail { get; set; }
        public string? TenantIdentityNumber { get; set; }
        public OccupantType? OccupantType { get; set; } // Oturma Durumu (Muafiyet Kontrolü için)
        public decimal? PropertyTaxBaseValue { get; set; } // Emlak Vergisi Rayiç Bedeli (NonExemptResident Emsal Kira için)
        public decimal? RentAmount { get; set; } // Normal Kiracı Aylık Kira Bedeli
        public bool IsUtilitiesOnOwner { get; set; } = false; // Faturalar mal sahibi üzerinde mi?
        public string? LeaseContractFilePath { get; set; } // Kira Sözleşmesi Dosyası
        public DateTime? ContractStartDate { get; set; } // Sözleşme Başlangıç Tarihi
        public DateTime? ContractEndDate { get; set; } // Sözleşme Bitiş Tarihi

        // Dinamik Tanımlamalar (Enum yerine DB'den)
        public int StatusId { get; set; } // Satılık, Kiralık vs.
        public DefinitionValue Status { get; set; }

        public int TypeId { get; set; } // Konut, İş Yeri vs.
        public DefinitionValue Type { get; set; }

        public int? SubTypeId { get; set; } // Daire, Villa, Ofis vs.
        public DefinitionValue SubType { get; set; }

        public int? FromWhomId { get; set; } // Sahibinden, Emlakçıdan vs.
        public DefinitionValue FromWhom { get; set; }
        
        // 1. AİDAT (Sadece Konut/İşyeri için doldurulur)
        public decimal? Dues { get; set; } // Aidat Tutarı (Aylık)
        
        // TEMEL ÖZELLİKLER (Arama filtrelerinde en çok kullanılanlar)
        public string? RoomCount { get; set; } // Örn: "3+1", "2+1"
        public int? BathroomCount { get; set; } // Banyo Sayısı
        public int? BalconyCount { get; set; } // Balkon Sayısı
        public int? WcCount { get; set; } // Tuvalet (WC) Sayısı (Banyosuz)
        public string? BuildingAge { get; set; } // Örn: "0", "1-5", "5-10"
        public int? HeatingId { get; set; } // Isıtma Sistemi (DefinitionValue FK)
        public DefinitionValue Heating { get; set; }
        public bool IsFurnished { get; set; } // Eşyalı mı?

        // KALICI BİNAYA BAĞLANTI (Master-Detail)
        public int BuildingId { get; set; } // Zorunlu Çatı Bağlantısı
        public Building Building { get; set; }
        
        [NotMapped]
        public Building Complex { get => Building; set => Building = value; }
        
        [NotMapped]
        public int? ComplexId { get => BuildingId; set { if (value.HasValue) BuildingId = value.Value; } }
        
        [NotMapped]
        public string BuildingNumber { get => Building?.BuildingNumber; set { if (Building != null) Building.BuildingNumber = value; } }
        
        [NotMapped]
        public string BlockName { get => Building?.BlockName; set { if (Building != null) Building.BlockName = value; } }
        
        [NotMapped]
        public int? TotalFloors { get => Building?.TotalFloors; set { if (Building != null) Building.TotalFloors = value; } }
        
        [NotMapped]
        public bool HasBlock { get => Building?.HasBlock ?? false; set { if (Building != null) Building.HasBlock = value; } }
        
        [NotMapped]
        public double Latitude { get => Building?.Latitude ?? 0; set { if (Building != null) Building.Latitude = value; } }
        
        [NotMapped]
        public double Longitude { get => Building?.Longitude ?? 0; set { if (Building != null) Building.Longitude = value; } }
        
        public bool HideLocation { get; set; } = false; // Haritada tam konumu gizle (Sadece bölge çemberi göster)

        // 2. ARSA VE HİSSE DETAYLARI (Sadece Arsa İlanlarında doldurulur)
        public string? DeedStatus { get; set; } // Örn: "Müstakil Tapu", "Hisseli Tapu", "Tahsis"
        public string? BlockNumber { get; set; } // Ada Numarası
        public string? ParcelNumber { get; set; } // Parsel Numarası
        public double? TotalAreaSqm { get; set; } // Arsanın Toplam Metrekaresi (Örn: 5000)
        public double? ShareAreaSqm { get; set; } // Satılan Hisseye Düşen Metrekare (Örn: 250)

        // BAĞIMSIZ BÖLÜM (DAİRE/DÜKKAN) DETAYLARI
        public string? DoorNumber { get; set; } // İç Kapı / Daire No (Eski UnitNumber yerine)
        
        [NotMapped]
        public string? UnitNumber { get => DoorNumber; set => DoorNumber = value; }
        
        public int? FloorNumber { get; set; } // Bulunduğu Kat
        public string? Facade { get; set; } // Cephe (Örn: Kuzey, Doğu)
        
        // DEVRE MÜLK İÇİN ÖZEL ALANLAR (Sadece devre mülk ise doldurulur)
        public bool IsTimeshare { get; set; } // Devre mülk mü?
        public DateTime? TimeshareStartDate { get; set; } // Örn: 1 Ağustos
        public DateTime? TimeshareEndDate { get; set; } // Örn: 15 Ağustos
        public string? TimesharePeriod { get; set; } // Örn: "Kırmızı Dönem"

        public string UserId { get; set; } // İlanı giren kullanıcı (Danışman veya Bireysel)
        public ApplicationUser User { get; set; }

        public string? OwnerUserId { get; set; } // Mülkün gerçek sahibi (PropertyOwner rolündeki kullanıcı - opsiyonel)
        public ApplicationUser OwnerUser { get; set; }

        public int? AgencyId { get; set; }
        public Agency Agency { get; set; }
        
        public bool ShowAgencyInfo { get; set; } = true;

        public string? TrackingAgentName { get; set; } // Hangi emlakçı takip ediyor? (Kişisel not)

        public ICollection<PropertyFeature> Features { get; set; }
        public ICollection<PropertyPriceHistory> PriceHistories { get; set; }
        
        // Istatistik / Analiz
        public int ViewCount { get; set; } // İlan kaç kez görüntülendi

        // EİDS Yetki Belgesi ve Pazar Analizi
        public bool HasAuthorization { get; set; }
        public string? AuthorizationDocumentNo { get; set; }
        public string? OwnerIdNumber { get; set; }
        public System.DateTime? AuthorizationEndDate { get; set; } // Yetkinin biteceği tarih
        public bool ExternalMarketAlert { get; set; } // Dış sitelerde kopyası bulundu mu? (Admin İncelemesi İçin)
        public bool IsEidsVerified { get; set; } = false;

        // Eşya detayları
        public string? FurnitureDetails { get; set; }

        public ICollection<PropertyImage> Images { get; set; }
        public ICollection<PropertyTranslation> Translations { get; set; }
        public ICollection<PropertyReservation> Reservations { get; set; }
        public virtual ICollection<EidsValidationLog> EidsLogs { get; set; } = new List<EidsValidationLog>();
        
        // B2B & Finansal Eklentiler
        public virtual ICollection<PropertyPayment> Payments { get; set; } = new List<PropertyPayment>();
        public virtual ICollection<PropertyLiability> Liabilities { get; set; } = new List<PropertyLiability>();
    }
}
