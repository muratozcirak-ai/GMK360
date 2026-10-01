using GMK360.Core.Entities;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers
{
    [Authorize]
    [Route("Landlord")]
    public class LandlordDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LandlordDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Ev sahibinin mülklerini bul
            var myProperties = await _context.Properties
                .Include(p => p.Building)
                .Where(p => p.UserId == user.Id || p.OwnerUserId == user.Id)
                .ToListAsync();

            var propertyIds = myProperties.Select(p => p.Id).ToList();

            // Mülkler için kira sözleşmelerini (veya basitçe kiracı bilgilerini) getir.
            // CRM'den bağımsız olarak, Property tablosundaki basit "TenantName" yapısını da kullanabiliriz.
            // Bireysel mülk sahibi için CrmRentalTracking ağır kaçabilir, ama ikisini de hibrit gösterebiliriz.

            // Şimdilik Property içindeki Tenant Name ve Rent Amount alanlarını ana dashboardda listeleyelim.
            var totalMonthlyRentExpected = myProperties.Where(p => p.RentAmount.HasValue).Sum(p => p.RentAmount.Value);

            ViewBag.TotalProperties = myProperties.Count;
            ViewBag.TotalRented = myProperties.Count(p => !string.IsNullOrEmpty(p.TenantName));
            ViewBag.TotalMonthlyRent = totalMonthlyRentExpected;

            return View(myProperties);
        }

        [HttpPost("SaveTenant")]
        public async Task<IActionResult> SaveTenant([FromForm] int propertyId, [FromForm] string tenantName, [FromForm] string tenantPhone, [FromForm] decimal rentAmount, [FromForm] DateTime startDate, [FromForm] DateTime endDate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == propertyId && (p.UserId == user.Id || p.OwnerUserId == user.Id));
            if (property == null) return NotFound("Mülk bulunamadı veya yetkiniz yok.");

            property.TenantName = tenantName;
            property.TenantPhone = tenantPhone;
            property.RentAmount = rentAmount;
            property.ContractStartDate = startDate;
            property.ContractEndDate = endDate;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kiracı bilgileri başarıyla kaydedildi.";
            return RedirectToAction("Index");
        }

        [HttpPost("MarkRentPaid")]
        public async Task<IActionResult> MarkRentPaid([FromForm] int propertyId, [FromForm] string month)
        {
            // İleride PropertyFinancialRecord altyapısına bağlanacak. Şimdilik simüle ediyoruz.
            TempData["SuccessMessage"] = $"{month} ayı kirası tahsil edildi olarak işaretlendi.";
            return RedirectToAction("Index");
        }
    }
}
