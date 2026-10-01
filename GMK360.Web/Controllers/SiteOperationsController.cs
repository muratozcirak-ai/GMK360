using GMK360.Core.Entities.Construction;
using GMK360.Core.Entities.Finance;
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
    [Route("SiteOperations")]
    public class SiteOperationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SiteOperationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetUserAgencyIdAsync()
        {
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")) return 1;
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            var consultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == user.Id && c.IsActive);
            return consultant?.AgencyId;
        }

        // --- ŞANTİYE JURNALİ (SEYİR DEFTERİ) ---
        [HttpPost("SaveLog")]
        public async Task<IActionResult> SaveLog([FromForm] int projectId, [FromForm] string weather, [FromForm] string progress, [FromForm] string obstacles, [FromForm] string needs)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var log = new SiteDailyLog
            {
                AgencyId = agencyId.Value,
                ProjectId = projectId,
                LogDate = DateTime.UtcNow,
                ReporterUserId = _userManager.GetUserId(User),
                WeatherCondition = weather,
                GeneralProgress = progress,
                ObstaclesAndDelays = obstacles,
                MaterialNeeds = needs
            };

            _context.SiteDailyLogs.Add(log);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Şantiye Jurnali başarıyla kaydedildi. Sorunlar merkeze iletildi.";
            return Redirect(Request.Headers["Referer"].ToString());
        }

        // --- ŞANTİYE ACİL NAKİT/AVANS TALEBİ ---
        [HttpPost("RequestCash")]
        public async Task<IActionResult> RequestCash([FromForm] int projectId, [FromForm] decimal amount, [FromForm] string reason)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var req = new ProjectCashRequest
            {
                AgencyId = agencyId.Value,
                ProjectId = projectId,
                RequestedByUserId = _userManager.GetUserId(User),
                Amount = amount,
                Reason = reason,
                RequestDate = DateTime.UtcNow,
                Status = "Bekliyor"
            };

            _context.ProjectCashRequests.Add(req);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Acil Nakit Talebi merkeze iletildi. Onaylanana kadar kırmızı bildirim olarak kalacaktır.";
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
