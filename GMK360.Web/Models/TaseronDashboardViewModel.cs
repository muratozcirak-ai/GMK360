using System;
using System.Collections.Generic;

namespace GMK360.Web.Models
{
    public class TaseronDashboardViewModel
    {
        public int TaseronId { get; set; } = 1;
        public string CompanyTitle { get; set; } = "Kaya Kaba İnşaat & Kalıp-Demir Taşeronluğu";
        public string ManagerName { get; set; } = "Hasan Kaya (Kaya Usta)";
        public string PhoneNumber { get; set; } = "0533 888 77 66";
        public string CityName { get; set; } = "İstanbul";
        public string DistrictName { get; set; } = "Kadıköy / Ataşehir";
        public string Bio { get; set; } = "15 yıldır İstanbul genelinde konut, villa ve iş merkezi projelerinde kalıp, demir donatı, beton dökümü, duvar ve kaba yapı taşeronluğu.";
        
        // Çoklu Branş / Esnek Yetenek Havuzu
        public List<string> Branches { get; set; } = new List<string>
        {
            "Kalıp & Ahşap İşçiliği",
            "Nervürlü Demir Bağlama & Donatı",
            "Tuğla, Bims & Gazbeton Duvar",
            "Şap & Kaba Sıva",
            "Bahçe Düzenleme, Peyzaj & Bahçıvanlık",
            "Kırım & Moloz Hafriyat"
        };

        // Tek Sabit Aidat (250 TL/ay - tüm şehir ve şantiyelerden ihale alma)
        public bool IsCityWideSubscriptionActive { get; set; } = true;
        public decimal MonthlySubscriptionFee { get; set; } = 250m;
        public bool IsVerifiedSubcontractor { get; set; } = true;
        public int ExperienceYears { get; set; } = 15;
        public int TotalTeamSize => Workers.Count;

        // Şantiye Sözleşmeleri
        public List<TaseronContractViewModel> Contracts { get; set; } = new List<TaseronContractViewModel>();

        // Ekip Üyeleri (Her biri zanaatkâr/ustadır)
        public List<TaseronWorkerViewModel> Workers { get; set; } = new List<TaseronWorkerViewModel>();

        // Günlük Puantaj Kayıtları
        public List<TaseronPuantajEntryViewModel> RecentPuantajEntries { get; set; } = new List<TaseronPuantajEntryViewModel>();

        // Avans Defteri
        public List<TaseronAdvanceViewModel> Advances { get; set; } = new List<TaseronAdvanceViewModel>();

        // İskele & Ekipman Kiralama ve Sarf Malzeme
        public List<TaseronEquipmentExpenseViewModel> EquipmentExpenses { get; set; } = new List<TaseronEquipmentExpenseViewModel>();

        // Taşeronun Net Kâr Pusulası (Cebime Kalan Para)
        public decimal TotalContractVolume { get; set; }
        public decimal TotalCollectedHakedis { get; set; }
        public decimal CurrentPeriodHakedis { get; set; }
        public decimal TotalWorkerWagesThisPeriod { get; set; }
        public decimal TotalAdvancesGivenThisPeriod { get; set; }
        public decimal TotalEquipmentExpenseThisPeriod { get; set; }
        public decimal TotalSiteOverheadThisPeriod { get; set; } // Yemek, servis, barınma
        public decimal NetProfitThisPeriod { get; set; }
        public double NetProfitMarginPercent => CurrentPeriodHakedis > 0 ? (double)(NetProfitThisPeriod / CurrentPeriodHakedis * 100) : 0;
    }

    public class TaseronContractViewModel
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = null!;
        public string ContractorCompany { get; set; } = null!; // Müteahhit Firma
        public string SiteManagerContact { get; set; } = null!; // Şantiye Şefi & Tel
        public string WorkScope { get; set; } = null!; // İşin Kapsamı (Örn: 10 Kat Kalıp + Demir + Duvar)
        public string ContractType { get; set; } = "Metraj / Birim Fiyatlı"; // Metrajlı, Götürü Kat Başı
        public decimal UnitPrice { get; set; } // 380 TL/m²
        public double TotalQuantityMetraj { get; set; } // 5.000 m²
        public decimal TotalContractAmount { get; set; } // 1.900.000 TL
        public decimal CumulativeHakedisPaid { get; set; } // 850.000 TL
        public decimal CurrentPeriodHakedisClaim { get; set; } // 350.000 TL (2. Kat Tabliyesi Döküldü)
        
        // Müteahhit Kesinti Kalkanı
        public decimal RetentionGuaranteeRate { get; set; } = 5m; // %5 Teminat Kesintisi
        public decimal RetentionDeductionAmount => CurrentPeriodHakedisClaim * (RetentionGuaranteeRate / 100m);
        public decimal SiteElectricWaterDeduction { get; set; } = 2500m;
        public decimal NetPayableHakedis => CurrentPeriodHakedisClaim - (RetentionDeductionAmount + SiteElectricWaterDeduction);

        public string Status { get; set; } = "Devam Ediyor"; // Teklif Aşamasında, Devam Ediyor, Kabul / İskân Aşamasında
        public DateTime StartDate { get; set; }
        public DateTime EstimatedCompletionDate { get; set; }
        public string WhatsAppHakedisSummaryUrl { get; set; } = "";
    }

    public class TaseronWorkerViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string TradeRole { get; set; } = null!; // Demir Ustası, Kalıp Ustası, Kaynakçı
        public decimal DailyWage { get; set; } = 1500m; // Günlük Yevmiye
        public double DaysWorkedThisMonth { get; set; } = 24; // Bu ay çalıştığı gün
        public decimal TotalWagesEarned => (decimal)DaysWorkedThisMonth * DailyWage;
        public decimal TotalAdvancesTaken { get; set; } = 8000m; // Alınan Avans
        public decimal RemainingNetPayable => TotalWagesEarned - TotalAdvancesTaken; // Kalan Ödenecek
        public bool IsRegisteredFutureCraftsman { get; set; } = true;
    }

    public class TaseronPuantajEntryViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public int WorkerId { get; set; }
        public string WorkerName { get; set; } = null!;
        public string TradeRole { get; set; } = null!;
        public double WorkDayFactor { get; set; } = 1.0; // 1.0 = Tam Gün, 0.5 = Yarım Gün, 0.0 = Gelmedi
        public int OvertimeHours { get; set; } = 0; // Fazla Mesai (Saat)
        public string StatusText => WorkDayFactor == 1.0 ? "Tam Gün" : (WorkDayFactor == 0.5 ? "Yarım Gün" : "Gelmedi");
        public string? SiteLocation { get; set; }
        public string? Notes { get; set; }
    }

    public class TaseronAdvanceViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public int WorkerId { get; set; }
        public string WorkerName { get; set; } = null!;
        public decimal Amount { get; set; }
        public string PaymentType { get; set; } = "Elden Nakit Harcırah";
        public string? Notes { get; set; }
    }

    public class TaseronEquipmentExpenseViewModel
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = null!; // Teleskopik Direk & Çelik İskele Kirası
        public string SupplierName { get; set; } = null!; // Mega İskele Sanayi
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Category { get; set; } = "İskele & Makine Kiralama"; // Kiralama, Sarf Malzeme (Tel/Çivi/Paspayı), KKD Güvenlik
        public string Status { get; set; } = "Açık Hesap Borcu";
    }
}
