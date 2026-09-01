using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class FinancialController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FinancialController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Financial
        // Kullanıcının Mülk Portföyü ve Özetleri
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Kullanıcının ilan verdiği veya mülk sahibi olarak atandığı mülkler
            var properties = await _context.Properties
                .Include(p => p.Type) // Daire, İşyeri, Arsa vs.
                .Include(p => p.Payments) // Mülkün finansal kayıtları
                .Where(p => p.UserId == user.Id || p.OwnerUserId == user.Id)
                .ToListAsync();

            return View(properties);
        }

        // GET: /Financial/Ledger/{id}
        // Spesifik bir mülkün detaylı finansal defteri
        public async Task<IActionResult> Ledger(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var property = await _context.Properties
                .Include(p => p.Type)
                .Include(p => p.Payments)
                    .ThenInclude(pay => pay.LiabilityType)
                .FirstOrDefaultAsync(p => p.Id == id && (p.UserId == user.Id || p.OwnerUserId == user.Id));

            if (property == null) return NotFound();

            // Mülkün tipine göre olası Liability Type'ları (Gider Türleri) çekelim
            // Şimdilik sistemdeki tüm geçerli gider türlerini getiriyoruz, View'da mülk tipine göre filtreleyeceğiz.
            ViewBag.LiabilityTypes = await _context.LiabilityTypes.ToListAsync();

            return View(property);
        }

        // POST: /Financial/AddExpense
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExpense(int propertyId, int liabilityTypeId, decimal amount, DateTime dueDate, string period, string description, bool isPaid)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == propertyId && (p.UserId == user.Id || p.OwnerUserId == user.Id));
            if (property == null) return NotFound();

            var payment = new PropertyPayment
            {
                PropertyId = propertyId,
                LiabilityTypeId = liabilityTypeId,
                Amount = amount,
                DueDate = dueDate,
                Period = period ?? dueDate.ToString("yyyy/MM"),
                Description = description,
                IsPaid = isPaid,
                PaymentDate = isPaid ? DateTime.Now : (DateTime?)null,
                CreatedAt = DateTime.Now
            };

            _context.PropertyPayments.Add(payment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yeni gider/vergi kalemi başarıyla eklendi.";
            return RedirectToAction(nameof(Ledger), new { id = propertyId });
        }
        
        // POST: /Financial/MarkAsPaid/{paymentId}
        [HttpPost]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var payment = await _context.PropertyPayments
                .Include(p => p.Property)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (payment == null || (payment.Property.UserId != user.Id && payment.Property.OwnerUserId != user.Id))
                return NotFound();

            payment.IsPaid = true;
            payment.PaymentDate = DateTime.Now;
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Ledger), new { id = payment.PropertyId });
        }

        // POST: /Financial/DeleteExpense/{paymentId}
        [HttpPost]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var payment = await _context.PropertyPayments
                .Include(p => p.Property)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (payment == null || (payment.Property.UserId != user.Id && payment.Property.OwnerUserId != user.Id))
                return NotFound();

            int propertyId = payment.PropertyId;
            _context.PropertyPayments.Remove(payment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Gider kalemi başarıyla silindi.";
            return RedirectToAction(nameof(Ledger), new { id = propertyId });
        }
    }
}
