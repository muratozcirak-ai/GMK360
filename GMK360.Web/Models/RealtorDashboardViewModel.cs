using System;
using System.Collections.Generic;
using System.Linq;

namespace GMK360.Web.Models
{
    public class RealtorListingSummaryItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ListingType { get; set; } = "Satılık Daire"; // Satılık, Kiralık, Proje Portföyü
        public string Location { get; set; } = string.Empty; // İlçe / Mahalle
        public decimal Price { get; set; }
        public string RoomCount { get; set; } = "3+1";
        public int AreaM2 { get; set; } = 120;
        public DateTime ListedDate { get; set; }
        public int ViewCount { get; set; }
        public int ShowingCount { get; set; }
        public string Status { get; set; } = "Yayında"; // Yayında, Opsiyonlu, Tapu Aşamasında, Kiralandı
        public string AssignedConsultantName { get; set; } = "Ofis Portföyü";
        public string ImageUrl { get; set; } = "/images/default-property.jpg";
    }

    public class RealtorNegotiationSecretNote
    {
        public int Id { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public string BuyerPhone { get; set; } = string.Empty;
        public decimal BuyerOfferedPrice { get; set; }
        public string WhatIToldBuyer { get; set; } = string.Empty; // Alıcıya söylenen taktiksel fiyat ve taahhüt
        public decimal SellerBottomPrice { get; set; } // Satıcının gerçek taban / razı olduğu gizli rakam
        public string WhatIToldSeller { get; set; } = string.Empty; // Satıcıya iletilen durum ve pazarlık payı
        public string SecretNotes { get; set; } = string.Empty; // Masraf, kredi, hanımın görüşü vb.
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class RealtorShowingAppointment
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
        public string PropertyTitle { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string ShowingReportStatus { get; set; } = "İmzalandı"; // İmzalandı, Bekliyor, Dijital Onay
        public string ClientFeedback { get; set; } = string.Empty;
        public bool IsInterestedInRenovation { get; set; }
    }

    public class RealtorPassiveIncomeTracker
    {
        public int ReferredLandlordsCount { get; set; } = 6;
        public int TargetForFreePlan { get; set; } = 5;
        public bool IsPlanFreeNow => ReferredLandlordsCount >= TargetForFreePlan;
        public decimal AnnualRecurringEarnings => ReferredLandlordsCount * 900m; // Daire başına yıllık pay
        public decimal MonthlyPassiveAverage => AnnualRecurringEarnings / 12m;
        public decimal SymbolAnnualFee { get; set; } = 3500m; // Normal yıllık bedel
        public decimal NetAnnualPayment => IsPlanFreeNow ? 0m : SymbolAnnualFee;
        public string PartnerLevel { get; set; } = "GMK360 Elit Partner (Aidatsız & Kazandıran)";
    }

    public class RealtorRenovationAdvisoryQuote
    {
        public string PropertyTitle { get; set; } = string.Empty;
        public decimal EconomicPackageCost { get; set; } = 145000m;
        public decimal StandardPackageCost { get; set; } = 280000m;
        public decimal LuxuryPackageCost { get; set; } = 490000m;
        public string ScopeSummary { get; set; } = "Mutfak dolabı, tezgah, banyo seramikleri, armatürler, su tesisatı revizyonu ve tam boya badana.";
        public string NetworkSource { get; set; } = "GMK360 Akredite Usta & Nalbur Tedarik Ağı (Fiyat Garantili)";
    }

    public class RealtorBrokerTeamMember
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Title { get; set; } = "Kıdemli Danışman";
        public string SpecializedTerritory { get; set; } = string.Empty; // Örn: Kadıköy / Moda & Caddebostan
        public int ActiveListingsCount { get; set; }
        public int MonthlyDealsClosed { get; set; }
        public decimal CommissionSharePercent { get; set; } = 50m; // %50 Danışman / %50 Ofis
        public string Phone { get; set; } = string.Empty;
    }

    public class RealtorProjectPartnershipItem
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ConstructionCompanyName { get; set; } = string.Empty;
        public string Territory { get; set; } = string.Empty;
        public string EstimatedDelivery { get; set; } = "2027 1. Çeyrek";
        public string PartnershipModel { get; set; } = "Tüm Emlakçılara Açık Satış Ortaklığı"; // Açık Yetki veya VIP Tek Yetkili
        public decimal CommissionRate { get; set; } = 2.5m;
        public int PreDemandLeadsCount { get; set; }
        public string ProjectHighlight { get; set; } = "Mimari Lansman Öncesi Ön Talep Kataloğu";
    }

    public class RealtorDashboardViewModel
    {
        public string AgencyName { get; set; } = "Moda Gayrimenkul & Danışmanlık";
        public string BrokerName { get; set; } = "Emlak Danışmanı";
        public bool IsMegaBrokerOffice { get; set; } = true; // Remax / Kurumsal çok danışmanlı mod

        public int TotalListingsCount => Listings.Count;
        public int ActiveNegotiationsCount => NegotiationNotes.Count;
        public int UpcomingShowingsCount => ShowingAppointments.Count(a => a.AppointmentDate >= DateTime.UtcNow);

        public List<RealtorListingSummaryItem> Listings { get; set; } = new List<RealtorListingSummaryItem>();
        public List<RealtorNegotiationSecretNote> NegotiationNotes { get; set; } = new List<RealtorNegotiationSecretNote>();
        public List<RealtorShowingAppointment> ShowingAppointments { get; set; } = new List<RealtorShowingAppointment>();
        public RealtorPassiveIncomeTracker PassiveIncome { get; set; } = new RealtorPassiveIncomeTracker();
        public List<RealtorRenovationAdvisoryQuote> RenovationQuotes { get; set; } = new List<RealtorRenovationAdvisoryQuote>();
        public List<RealtorBrokerTeamMember> BrokerTeam { get; set; } = new List<RealtorBrokerTeamMember>();
        public List<RealtorProjectPartnershipItem> ProjectPartnerships { get; set; } = new List<RealtorProjectPartnershipItem>();
    }
}
