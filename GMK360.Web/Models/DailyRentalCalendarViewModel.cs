using System;
using System.Collections.Generic;

namespace GMK360.Web.Models
{
    public class DailyRentalCalendarViewModel
    {
        public int UnitId { get; set; }
        public string UnitNumber { get; set; }
        public string RoomLayout { get; set; } // Örn: "2+1"
        
        public int Month { get; set; }
        public int Year { get; set; }

        public List<CalendarDayViewModel> Days { get; set; } = new List<CalendarDayViewModel>();
    }

    public class CalendarDayViewModel
    {
        public DateTime Date { get; set; }
        public bool IsReserved { get; set; }
        public string CssClass => IsReserved ? "bg-danger text-white" : "bg-success text-white";
        public int? ReservationId { get; set; }
        public string GuestName { get; set; } // Opsiyonel, sadece yetkili görebilir
    }
}
