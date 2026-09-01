using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class AgreementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AgreementController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingAgreements()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "User not found" });

            // Kullanıcının daha önce onayladığı sözleşme ID'leri
            var acceptedIds = await _context.UserAgreementAcceptances
                .Where(x => x.UserId == userId)
                .Select(x => x.LegalAgreementId)
                .ToListAsync();

            // Aktif, zorunlu ve henüz onaylanmamış sözleşmeler
            var pendingAgreements = await _context.LegalAgreements
                .Where(x => x.IsActive && x.IsRequired && !acceptedIds.Contains(x.Id))
                .Select(x => new
                {
                    id = x.Id,
                    title = x.Title,
                    content = x.Content,
                    version = x.Version
                })
                .ToListAsync();

            return Json(new { success = true, agreements = pendingAgreements });
        }

        [HttpPost]
        public async Task<IActionResult> AcceptAgreement(int agreementId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false });

            var agreement = await _context.LegalAgreements.FindAsync(agreementId);
            if (agreement == null || !agreement.IsActive)
                return Json(new { success = false, message = "Sözleşme bulunamadı." });

            var existing = await _context.UserAgreementAcceptances
                .FirstOrDefaultAsync(x => x.UserId == userId && x.LegalAgreementId == agreementId);

            if (existing != null)
                return Json(new { success = true }); // Zaten onaylanmış

            var acceptance = new UserAgreementAcceptance
            {
                UserId = userId,
                LegalAgreementId = agreementId,
                AcceptedAt = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            };

            _context.UserAgreementAcceptances.Add(acceptance);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
