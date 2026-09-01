using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GMK360.Core.Interfaces;
using GMK360.Web.Models;
using GMK360.Core.Entities;
using GMK360.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    public class ReservationController : Controller
    {
        private readonly IShortTermRentalService _rentalService;
        private readonly ApplicationDbContext _context;

        public ReservationController(IShortTermRentalService rentalService, ApplicationDbContext context)
        {
            _rentalService = rentalService;
            _context = context;
        }

        public async Task<IActionResult> Calendar(int unitId, int? year, int? month)
        {
            var unit = await _context.BuildingUnits.FindAsync(unitId);
            if (unit == null || !unit.IsAvailableForDailyRent) return NotFound("Bu ünite günlük kiralama için uygun değil.");

            var targetYear = year ?? DateTime.Now.Year;
            var targetMonth = month ?? DateTime.Now.Month;
            
            var startDate = new DateTime(targetYear, targetMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Ayın tüm günlerini oluştur
            var days = new List<CalendarDayViewModel>();
            
            // O ayın rezervasyonlarını getir
            var reservations = await _context.Reservations
                .Where(r => r.UnitId == unitId && r.ReservationStatus != ReservationStatus.Cancelled)
                .Where(r => (r.StartDate <= endDate && r.EndDate >= startDate))
                .ToListAsync();

            for (int i = 1; i <= endDate.Day; i++)
            {
                var currentDate = new DateTime(targetYear, targetMonth, i);
                var activeRes = reservations.FirstOrDefault(r => currentDate >= r.StartDate && currentDate <= r.EndDate);

                days.Add(new CalendarDayViewModel
                {
                    Date = currentDate,
                    IsReserved = activeRes != null,
                    ReservationId = activeRes?.Id,
                    GuestName = activeRes?.TenantUserId // Normalde User nesnesine join atıp isim alınır
                });
            }

            var model = new DailyRentalCalendarViewModel
            {
                UnitId = unitId,
                UnitNumber = unit.UnitNumber,
                RoomLayout = unit.RoomLayout,
                Month = targetMonth,
                Year = targetYear,
                Days = days
            };

            return View(model);
        }
    }
}
