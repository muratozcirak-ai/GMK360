using GMK360.Core.Entities.Construction;
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
    public class BuildingManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BuildingManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --- BÖLÜM 1: İNŞAAT FİRMASI DAVET GÖNDERİYOR ---
        [Authorize]
        [HttpPost("BuildingManagement/SendInvite")]
        public async Task<IActionResult> SendInvite(int projectId, string managerName, string managerPhone, string managerEmail)
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null) return Unauthorized();

            var consultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == user.Id && c.IsActive);
            int agencyId = consultant?.AgencyId ?? 1; // Default to 1 if admin for testing

            var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId);
            if (project == null) return NotFound("Proje bulunamadı.");

            // Generate Token
            string token = Guid.NewGuid().ToString("N");

            var invite = new ProjectManagementInvitation
            {
                AgencyId = agencyId,
                ProjectId = projectId,
                ManagerName = managerName,
                ManagerPhone = managerPhone,
                ManagerEmail = managerEmail,
                InvitationToken = token,
                Status = "Bekliyor",
                SentAt = DateTime.UtcNow
            };

            _context.ProjectManagementInvitations.Add(invite);
            await _context.SaveChangesAsync();

            // Gerçek bir senaryoda burada SMS atılır. 
            // Biz kullanıcıya linki TempData ile ekranda göstereceğiz (Truva Atı Linki).
            string inviteLink = $"{Request.Scheme}://{Request.Host}/BuildingManagement/Portal?token={token}";
            TempData["SuccessMessage"] = $"Yöneticiye Davet Başarıyla Oluşturuldu! Lütfen şu linki WhatsApp üzerinden yöneticiye iletin: {inviteLink}";

            return RedirectToAction("Details", "ConstructionProject", new { id = projectId });
        }


        // --- BÖLÜM 2: MÜŞTERİ / BİNA YÖNETİCİSİ PORTALI (TRUVA ATI EKRANI) ---
        [AllowAnonymous]
        [HttpGet("BuildingManagement/Portal")]
        public async Task<IActionResult> Portal(string token)
        {
            if (string.IsNullOrEmpty(token)) return NotFound("Geçersiz veya eksik davet bağlantısı.");

            var invite = await _context.ProjectManagementInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
            if (invite == null) return NotFound("Geçersiz davet bağlantısı.");

            var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == invite.ProjectId);
            var agency = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == invite.AgencyId);

            ViewBag.ProjectName = project?.Name ?? "Belirtilmeyen Proje";
            ViewBag.AgencyName = agency?.CompanyName ?? "İnşaat Firması";
            ViewBag.Token = token;
            ViewBag.ManagerName = invite.ManagerName;
            ViewBag.Status = invite.Status;

            return View(invite);
        }

        [AllowAnonymous]
        [HttpPost("BuildingManagement/AcceptInvite")]
        public async Task<IActionResult> AcceptInvite(string token)
        {
            var invite = await _context.ProjectManagementInvitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
            if (invite == null) return NotFound();

            invite.Status = "Kabul Edildi";
            invite.AcceptedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tebrikler! Müşteri profiliniz oluşturuldu. Artık projenizin şantiye aşamalarını takip edebilir ve iç mekan malzeme seçimlerinizi yapabilirsiniz.";
            return RedirectToAction("Portal", new { token = token });
        }
    }
}
