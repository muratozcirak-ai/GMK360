using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Core.Entities.Construction;
using GMK360.Core.Entities.Construction.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers
{
    public partial class ConstructionProjectController : Controller
    {
        [HttpGet("ConstructionProject/Emergencies/{id}")]
        public async Task<IActionResult> Emergencies(int id)
        {
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (project == null) return NotFound();

            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null || (project.AgencyId != agencyId.Value && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin")))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            ViewData["ProjectId"] = id;
            ViewData["ProjectName"] = project.Name;

            var today = DateTime.Today;

            var fieldObstaclesAndNeeds = await _context.SiteDailyLogs
                .AsNoTracking()
                .Where(l => l.ProjectId == id && !l.IsDeleted &&
                    (!string.IsNullOrWhiteSpace(l.ObstaclesAndDelays) || !string.IsNullOrWhiteSpace(l.MaterialNeeds)))
                .OrderByDescending(l => l.LogDate)
                .Take(30)
                .ToListAsync();

            var overdueScheduleTasks = await _context.ProjectScheduleTasks
                .AsNoTracking()
                .Where(t => t.ProjectId == id && !t.IsDeleted && !t.IsCancelled &&
                    t.Status != "Completed" && t.PlannedEndDate.HasValue && t.PlannedEndDate.Value.Date < today)
                .OrderBy(t => t.PlannedEndDate)
                .ToListAsync();

            var legalDocumentIssues = await _context.ProjectLegalDocuments
                .AsNoTracking()
                .Where(d => d.ConstructionProjectId == id && !d.IsDeleted && d.IsActive &&
                    (d.Status != "Tamamlandı" && d.Status != "Onaylandı") &&
                    (!string.IsNullOrWhiteSpace(d.IssueNotes) || d.Status == "Sorunlu" || d.Status == "Reddedildi" || d.Status == "Bekliyor"))
                .OrderBy(d => d.DisplayOrder)
                .Take(30)
                .ToListAsync();

            var unsafeIsgAudits = await _context.IsgAuditForms
                .AsNoTracking()
                .Where(i => i.ProjectId == id && !i.IsDeleted && !i.IsSiteSafe)
                .OrderByDescending(i => i.AuditDate)
                .Take(20)
                .ToListAsync();

            var vm = new ProjectEmergenciesViewModel
            {
                Project = project,
                FieldObstaclesAndNeeds = fieldObstaclesAndNeeds,
                OverdueScheduleTasks = overdueScheduleTasks,
                LegalDocumentIssues = legalDocumentIssues,
                UnsafeIsgAudits = unsafeIsgAudits
            };

            return View(vm);
        }

        [HttpPost("ConstructionProject/AddEmergencyLog")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmergencyLog(int projectId, string? obstaclesAndDelays, string? materialNeeds, string? weatherCondition)
        {
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);

            if (project == null) return NotFound();

            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null || (project.AgencyId != agencyId.Value && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin")))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (string.IsNullOrWhiteSpace(obstaclesAndDelays) && string.IsNullOrWhiteSpace(materialNeeds))
            {
                TempData["ErrorMessage"] = "Lütfen en az bir acil engel, gecikme veya acil malzeme ihtiyacı açıklaması giriniz.";
                return RedirectToAction(nameof(Emergencies), new { id = projectId });
            }

            var user = await _userManager.GetUserAsync(User);
            var log = new SiteDailyLog
            {
                ProjectId = projectId,
                AgencyId = project.AgencyId,
                LogDate = DateTime.Now,
                ReporterUserId = user?.Id,
                WeatherCondition = string.IsNullOrWhiteSpace(weatherCondition) ? "Acil Saha Bildirimi" : weatherCondition.Trim(),
                GeneralProgress = "Acil Durum & Kritik Uyarılar ekranından bildirildi.",
                ObstaclesAndDelays = obstaclesAndDelays?.Trim(),
                MaterialNeeds = materialNeeds?.Trim(),
                IsReadByAdmin = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.SiteDailyLogs.Add(log);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Acil saha uyarısı / malzeme ihtiyacı kaydedildi.";
            return RedirectToAction(nameof(Emergencies), new { id = projectId });
        }
    }

    public class ProjectEmergenciesViewModel
    {
        public ConstructionProject Project { get; set; } = null!;
        public List<SiteDailyLog> FieldObstaclesAndNeeds { get; set; } = new();
        public List<ProjectScheduleTask> OverdueScheduleTasks { get; set; } = new();
        public List<ProjectLegalDocument> LegalDocumentIssues { get; set; } = new();
        public List<IsgAuditForm> UnsafeIsgAudits { get; set; } = new();
    }
}
