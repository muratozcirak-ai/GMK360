using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;

namespace GMK360.Web.Controllers
{
    public class PwaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PwaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Pwa/TimesheetApprove?token=XYZ
        [HttpGet]
        public async Task<IActionResult> TimesheetApprove(string token)
        {
            if (string.IsNullOrEmpty(token)) return NotFound("Geçersiz veya eksik bağlantı.");

            var timesheet = await _context.ConstructionTimesheets
                .Include(t => t.Worker)
                .Include(t => t.ConstructionProject)
                .FirstOrDefaultAsync(t => t.PwaAccessToken == token);

            if (timesheet == null)
                return NotFound("Kayıt bulunamadı veya bağlantı süresi dolmuş.");

            return View(timesheet);
        }

        // POST: /Pwa/ConfirmTimesheet
        [HttpPost]
        public async Task<IActionResult> ConfirmTimesheet(string token, bool isApproved, string note)
        {
            var timesheet = await _context.ConstructionTimesheets
                .FirstOrDefaultAsync(t => t.PwaAccessToken == token && t.ApprovalStatus == 0);

            if (timesheet == null)
                return Json(new { success = false, message = "Kayıt bulunamadı veya zaten işleme alınmış." });

            if (isApproved)
            {
                timesheet.ApprovalStatus = 1; // Onaylandı
                timesheet.ApprovedAt = DateTime.UtcNow;
            }
            else
            {
                timesheet.ApprovalStatus = 2; // Reddedildi
                timesheet.RejectionReason = note;
            }

            // Güvenlik: Tek kullanımlık token'ı temizle (veya sakla ama ApprovalStatus ile koru)
            // timesheet.PwaAccessToken = null; 

            _context.Update(timesheet);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = isApproved ? "Puantaj başarıyla onaylandı." : "Puantaj reddedildi." });
        }
    }
}

