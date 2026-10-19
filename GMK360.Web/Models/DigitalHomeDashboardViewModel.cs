using System;
using System.Collections.Generic;
using GMK360.Core.Entities;

namespace GMK360.Web.Models
{
    public class AgendaEvent
    {
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string PropertyName { get; set; }
        public decimal Amount { get; set; }
        public bool IsIncome { get; set; }
        public string IconClass { get; set; }
        public string ColorClass { get; set; }
        public string StatusText { get; set; }
    }

    public class FinancialAgendaViewModel
    {
        public decimal TotalExpectedRentThisMonth { get; set; }
        public decimal TotalExpensesThisMonth { get; set; }
        public decimal NetCashFlow => TotalExpectedRentThisMonth - TotalExpensesThisMonth;

        public List<AgendaEvent> UpcomingEvents { get; set; } = new List<AgendaEvent>();
    }

    public class HouseholdBillItem
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty; // Elektrik, Doğalgaz, Su, Aidat, İnternet, TV, GSM
        public string Title { get; set; } = string.Empty; // Örn: Mutfak Doğalgaz (İGDAŞ), Kızın Telefonu
        public string SubscriberNo { get; set; } = string.Empty;
        public int DueDayOfMonth { get; set; } = 15;
        public decimal EstimatedMonthlyAmount { get; set; }
        public decimal? LastPaidAmount { get; set; }
        public bool IsPaidThisMonth { get; set; }
        public string IconClass { get; set; } = "ph-receipt";
        public string BadgeColor { get; set; } = "primary";
        public string ProviderName { get; set; } = string.Empty;
    }

    public class HouseholdRepairItem
    {
        public string IssueTitle { get; set; } = string.Empty; // Örn: Kombi Petek Temizliği, Mutfak Bataryası Kaçağı
        public string Category { get; set; } = string.Empty; // Tesisat, Kombi, Boya
        public string UrgencyLevel { get; set; } = "Normal"; // Acil, Bu Hafta, Sezonluk
        public string SuggestedActionUrl { get; set; } = "/Usta"; // GMK360 Usta Çağır Köprüsü
        public string ActionButtonText { get; set; } = "Usta Teklifi Al";
    }

    public class HouseholdBudgetCockpitViewModel
    {
        public bool IsSingleApartmentFreeUser { get; set; } = true;
        public string ApartmentName { get; set; } = "Evim";
        public decimal TotalMonthlyBills => Bills.Sum(b => b.EstimatedMonthlyAmount);
        public decimal PaidBillsThisMonth => Bills.Where(b => b.IsPaidThisMonth).Sum(b => b.EstimatedMonthlyAmount);
        public decimal RemainingBillsThisMonth => TotalMonthlyBills - PaidBillsThisMonth;
        public decimal WinterHeatingForecast => Bills.Where(b => b.Category == "Doğalgaz" || b.Category == "Elektrik").Sum(b => b.EstimatedMonthlyAmount) * 1.65m;
        public string SeasonalAdvice { get; set; } = "Kış sezonu yaklaşıyor: Kombi ve petek temizliği ile faturanızda %20-25 tasarruf sağlayabilirsiniz.";

        public List<HouseholdBillItem> Bills { get; set; } = new List<HouseholdBillItem>();
        public List<HouseholdRepairItem> ActiveRepairNeeds { get; set; } = new List<HouseholdRepairItem>();
    }

    public class DigitalHomeDashboardViewModel
    {
        public IEnumerable<Property> Properties { get; set; } = new List<Property>();
        public FinancialAgendaViewModel FinancialAgenda { get; set; } = new FinancialAgendaViewModel();
        public HouseholdBudgetCockpitViewModel HouseholdBudget { get; set; } = new HouseholdBudgetCockpitViewModel();
    }
}

