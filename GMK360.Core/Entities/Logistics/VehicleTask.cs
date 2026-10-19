using System;
namespace GMK360.Core.Entities.Logistics
{
    public class VehicleTask : BaseEntity
    {
        public int VehicleId { get; set; }
        public CompanyVehicle Vehicle { get; set; }
        
        public int AgencyId { get; set; }
        public string? DriverName { get; set; }
        public int? DestinationProjectId { get; set; }
        public string TaskDescription { get; set; }
        public DateTime TaskDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Bekliyor"; // Bekliyor, Yolda, Tamamlandı
        
        // Araç KM ve Saat Takibi (Hafriyat ve İş Makinası için)
        public string? StartTime { get; set; } // Örn: 09:00
        public string? EndTime { get; set; } // Örn: 12:30
        public int? StartKm { get; set; }
        public int? EndKm { get; set; }
        public decimal? WorkingHours { get; set; } // Bu görev/sefer kaç saat sürdü?

        // 🚚 Damperli Kamyon Döküm Seferi Bilgileri
        public string? TaskCategory { get; set; } // "Döküm Seferi", "Makine Puantajı", "Genel Görev"
        public string? DumpSiteName { get; set; } // Döküm Sahası (Cebeci, Kemerburgaz, Şile vb.)
        public string? MaterialType { get; set; } // Yıkım Molozu, Temel Hafriyatı, Asfalt vb.
        public int? TripNumber { get; set; } // Günün kaçıncı seferi (1, 2, 3...)
        public string? TicketNumber { get; set; } // Kantar Fiş No / İrsaliye No
        public decimal? Tonnage { get; set; } // Net Tonaj
        public string? TicketPhotoPath { get; set; } // Kantar / Sayaç Fişi Fotoğrafı

        // 🚜 İş Makinası Çalışma Saati & Sayaç Puantajı
        public decimal? StartWorkHour { get; set; } // Başlangıç Sayacı
        public decimal? EndWorkHour { get; set; } // Bitiş Sayacı
        public decimal? FuelLiters { get; set; } // Tüketilen Mazot (Litre)
    }
}