using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GMK360.Web.Models
{
    // Günlük Kiralama & Butik İşletme Yönetim Masası Modeli
    public class DailyRentalWorkspaceViewModel
    {
        public bool HasFacility { get; set; }
        public int FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public string FacilityType { get; set; } = string.Empty; // Apart Otel, Butik Pansiyon, Müstakil Villa, Daire Grubu
        public string CityName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? KbsFacilityCode { get; set; } // Emniyet KBS kodu
        public string CheckInTime { get; set; } = "14:00";
        public string CheckOutTime { get; set; } = "11:00";
        public string? ContactPhone { get; set; }

        // Özet İstatistikler (Mock değil, kullanıcının gerçek verileri)
        public int TotalUnitsCount => Units.Count;
        public int OccupiedUnitsCount => Units.FindAll(u => u.IsOccupied).Count;
        public int AvailableUnitsCount => Units.FindAll(u => !u.IsOccupied).Count;
        public decimal OccupancyRate => TotalUnitsCount > 0 ? (decimal)OccupiedUnitsCount / TotalUnitsCount * 100 : 0;
        public int TodayCheckInsCount { get; set; }
        public int TodayCheckOutsCount { get; set; }

        // Tanımlı Gerçek Oda & Daireler
        public List<DailyRentalUnitItemViewModel> Units { get; set; } = new List<DailyRentalUnitItemViewModel>();

        // Yaklaşan Gerçek Rezervasyonlar
        public List<DailyRentalReservationItemViewModel> Reservations { get; set; } = new List<DailyRentalReservationItemViewModel>();
    }

    // Gerçek Oda / Daire Kart Modeli
    public class DailyRentalUnitItemViewModel
    {
        public int Id { get; set; }
        public string UnitName { get; set; } = string.Empty; // Örn: "Daire 1 (1+1 Deniz)", "Oda 101"
        public string RoomLayout { get; set; } = string.Empty; // 1+0, 1+1, 2+1, Standart Oda, Suit, Villa
        public int Capacity { get; set; } = 2; // Kaç kişilik
        public decimal DailyRate { get; set; } // Gecelik Fiyat ₺
        public decimal CleaningFee { get; set; } // Temizlik / Depozito ₺
        public bool IsOccupied { get; set; } // Dolu mu?
        public string StatusText => IsOccupied ? "Dolu / Konaklamada" : "Müsait / Temiz";
        public string StatusBadgeClass => IsOccupied ? "bg-danger text-white" : "bg-success text-white";
        public string? CurrentGuestName { get; set; }
        public DateTime? CurrentCheckOutDate { get; set; }
        public int? CurrentReservationId { get; set; }
    }

    // Gerçek Rezervasyon & KBS Bildirim Modeli
    public class DailyRentalReservationItemViewModel
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string GuestPhone { get; set; } = string.Empty;
        public string? GuestIdNumber { get; set; } // TC veya Pasaport
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalNights => Math.Max(1, (int)(EndDate.Date - StartDate.Date).TotalDays);
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Bekliyor"; // Bekliyor, Kapora Alındı, Tamamlandı, Giriş Yapıldı
        public bool IsKbsReported { get; set; }
    }

    // 0'dan Tesis & Oda Kurulum Sihirbazı Modeli (Setup Wizard)
    public class DailyRentalSetupWizardViewModel
    {
        // ADIM 1: Tesis / Yer Tanımlama
        [Required(ErrorMessage = "Tesis veya işletme adı zorunludur.")]
        [Display(Name = "Tesis / İşletme Adı")]
        public string FacilityName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tesis türü seçilmelidir.")]
        [Display(Name = "Tesis Türü")]
        public string FacilityType { get; set; } = "Apart Otel"; // Apart Otel, Butik Pansiyon, Villa, Daireler, Bungalov

        [Display(Name = "İl")]
        public string CityName { get; set; } = "Muğla";

        [Display(Name = "İlçe")]
        public string DistrictName { get; set; } = "Bodrum";

        [Display(Name = "Mahalle / Bölge")]
        public string NeighborhoodName { get; set; } = "Yalıkavak";

        [Display(Name = "Açık Adres")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "EGM / KBS Tesis Kodu (Opsiyonel)")]
        public string? KbsFacilityCode { get; set; }

        [Display(Name = "İletişim / Rezervasyon Telefonu")]
        public string? ContactPhone { get; set; }

        [Display(Name = "Giriş Saati (Check-in)")]
        public string CheckInTime { get; set; } = "14:00";

        [Display(Name = "Çıkış Saati (Check-out)")]
        public string CheckOutTime { get; set; } = "11:00";

        // ADIM 2: İlk Oda / Daire Birimleri Tanımlama
        public List<SetupUnitInputModel> Units { get; set; } = new List<SetupUnitInputModel>();
    }

    // Sihirbazda Girilen Oda/Daire Satırı
    public class SetupUnitInputModel
    {
        public string Name { get; set; } = string.Empty; // Örn: "Daire 1 (1+1)", "Oda 101"
        public string RoomLayout { get; set; } = "1+1"; // 1+0, 1+1, 2+1, Standart Oda, Suit, Villa
        public int Capacity { get; set; } = 2; // Kişi kapasitesi
        public decimal DailyPrice { get; set; } = 2000; // Gecelik Fiyat
        public decimal CleaningFee { get; set; } = 0; // Temizlik / Depozito
    }

    // Hızlı Rezervasyon Ekleme Modeli
    public class QuickReservationInputModel
    {
        [Required]
        public int UnitId { get; set; }

        [Required(ErrorMessage = "Misafir adı soyadı zorunludur.")]
        public string GuestName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        public string GuestPhone { get; set; } = string.Empty;

        public string? GuestIdNumber { get; set; } // TC Kimlik No / Pasaport No (KBS)

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

        public int GuestCount { get; set; } = 2;

        public decimal TotalPrice { get; set; }

        public string PaymentStatus { get; set; } = "Kapora"; // Bekliyor, Kapora, Tamamlandi

        public bool SendKbsNotification { get; set; } = true;
    }
}
