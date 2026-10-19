using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Core.Entities;
using GMK360.Data.Contexts;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class DailyRentalAgendaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DailyRentalAgendaController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var isSuperAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

            // Kullanıcının veya sistemin günlük kiralık tesisini bul
            var query = _context.Buildings
                .Include(b => b.Units)
                .AsQueryable();

            Building? facility = null;
            if (!string.IsNullOrEmpty(userId))
            {
                facility = await query.FirstOrDefaultAsync(b => 
                    (b.ManagerUserId == userId || b.CreatedByUserId == userId) && 
                    (b.TechnicalFeatures == "DailyRental" || (b.Description != null && b.Description.Contains("Günlük Kiralık"))));
            }

            if (facility == null && isSuperAdmin)
            {
                facility = await query.FirstOrDefaultAsync(b => 
                    b.TechnicalFeatures == "DailyRental" || (b.Description != null && b.Description.Contains("Günlük Kiralık")));
            }

            // Eğer hiç tesis yoksa, kullanıcıyı 0'dan kurulum ve karşılama masasına yönlendir
            if (facility == null)
            {
                TempData["ErrorMessage"] = "Ajandayı kullanabilmek için önce bir konaklama tesisi ve en az bir oda tanımlamalısınız.";
                return RedirectToAction("Index", "DailyRental");
            }

            var units = facility.Units?
                .Where(u => u.IsAvailableForDailyRent || u.Category == "DailyRental")
                .OrderBy(u => u.DoorNumber)
                .ToList() ?? new List<BuildingUnit>();

            ViewBag.Facility = facility;
            ViewBag.Units = units;

            return View(units);
        }

        // Ajanda İçin Gerçek Odaları JSON Olarak Döndür
        [HttpGet]
        public async Task<IActionResult> GetResources()
        {
            var userId = GetUserId();
            var isSuperAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

            var query = _context.Buildings.Include(b => b.Units).AsQueryable();
            Building? facility = null;

            if (!string.IsNullOrEmpty(userId))
            {
                facility = await query.FirstOrDefaultAsync(b => 
                    (b.ManagerUserId == userId || b.CreatedByUserId == userId) && 
                    (b.TechnicalFeatures == "DailyRental" || (b.Description != null && b.Description.Contains("Günlük Kiralık"))));
            }

            if (facility == null && isSuperAdmin)
            {
                facility = await query.FirstOrDefaultAsync(b => 
                    b.TechnicalFeatures == "DailyRental" || (b.Description != null && b.Description.Contains("Günlük Kiralık")));
            }

            if (facility == null || facility.Units == null)
                return Json(new List<object>());

            var resources = facility.Units
                .Where(u => u.IsAvailableForDailyRent || u.Category == "DailyRental")
                .OrderBy(u => u.DoorNumber)
                .Select(u => new
                {
                    id = u.Id.ToString(),
                    title = $"{u.DoorNumber} ({u.RoomLayout ?? "Standart"})"
                })
                .ToList();

            return Json(resources);
        }

        // Ajanda İçin Gerçek Rezervasyonları JSON Olarak Döndür
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var userId = GetUserId();
            var isSuperAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

            var query = _context.Buildings.Include(b => b.Units).AsQueryable();
            Building? facility = null;

            if (!string.IsNullOrEmpty(userId))
            {
                facility = await query.FirstOrDefaultAsync(b => 
                    (b.ManagerUserId == userId || b.CreatedByUserId == userId) && 
                    (b.TechnicalFeatures == "DailyRental" || (b.Description != null && b.Description.Contains("Günlük Kiralık"))));
            }

            if (facility == null && isSuperAdmin)
            {
                facility = await query.FirstOrDefaultAsync(b => 
                    b.TechnicalFeatures == "DailyRental" || (b.Description != null && b.Description.Contains("Günlük Kiralık")));
            }

            if (facility == null || facility.Units == null)
                return Json(new List<object>());

            var unitIds = facility.Units.Select(u => u.Id).ToList();

            var reservations = await _context.Reservations
                .Include(r => r.TenantUser)
                .Include(r => r.Guests)
                .Where(r => unitIds.Contains(r.UnitId) && r.ReservationStatus != ReservationStatus.Cancelled)
                .ToListAsync();

            var events = reservations.Select(r => {
                var g = r.Guests?.FirstOrDefault();
                var gTitle = g != null ? $"{g.FirstName} {g.LastName}".Trim() : (r.TenantUser?.UserName ?? "Misafir");
                var hasKbs = g != null && (!string.IsNullOrEmpty(g.TCIdentityNumberOrPassport) || g.IsReportedToKbs);

                return new
                {
                    id = r.Id.ToString(),
                    resourceId = r.UnitId.ToString(),
                    start = r.StartDate.ToString("yyyy-MM-dd"),
                    end = r.EndDate.ToString("yyyy-MM-dd"),
                    title = gTitle,
                    classNames = new[] { r.ReservationStatus == ReservationStatus.Confirmed ? "status-tamamlandi" : "status-kapora" },
                    extendedProps = new
                    {
                        kbs = hasKbs,
                        phone = r.TenantUser?.PhoneNumber ?? "-",
                        price = r.TotalPrice
                    }
                };
            }).ToList();

            return Json(events);
        }
    }
}
