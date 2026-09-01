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

    public class DigitalHomeDashboardViewModel
    {
        public IEnumerable<Property> Properties { get; set; } = new List<Property>();
        public FinancialAgendaViewModel FinancialAgenda { get; set; } = new FinancialAgendaViewModel();
    }
}
