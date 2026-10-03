using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Construction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class DailyTimesheetsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DailyTimesheetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> ProjectTimesheet(int id, DateTime? date)
        {
            var targetDate = date ?? DateTime.Today;
            var project = await _context.ConstructionProjects.FindAsync(id);
            if (project == null) return NotFound();

            var workers = await _context.AgencyWorkers
                .Where(w => !w.IsDeleted)
                .OrderBy(w => w.FirstName)
                .ToListAsync();

            var existingTimesheets = await _context.Set<DailyTimesheet>()
                .Where(t => t.ConstructionProjectId == id && t.WorkDate.Date == targetDate.Date)
                .ToDictionaryAsync(t => t.AgencyWorkerId);

            var projects = await _context.ConstructionProjects
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            ViewBag.Project = project;
            ViewBag.TargetDate = targetDate;
            ViewBag.Workers = workers;
            ViewBag.ExistingTimesheets = existingTimesheets;
            ViewBag.Projects = projects;

            return View();
        }

        public async Task<IActionResult> Index(int? projectId, DateTime? workDate)
        {
            var date = workDate ?? DateTime.Today;
            
            // Tüm projeleri çek
            ViewBag.Projects = new SelectList(await _context.ConstructionProjects.Where(p => !p.IsDeleted).ToListAsync(), "Id", "Name", projectId);
            ViewBag.CurrentDate = date.ToString("yyyy-MM-dd");

            var query = _context.Set<DailyTimesheet>()
                .Include(t => t.AgencyWorker)
                .Where(t => t.WorkDate.Date == date.Date);

            if (projectId.HasValue)
            {
                query = query.Where(t => t.ConstructionProjectId == projectId.Value);
            }
            else
            {
                // Eğer proje seçilmemişse, boş liste döndür (performans için)
                return View(Enumerable.Empty<DailyTimesheet>());
            }

            var timesheets = await query.ToListAsync();
            
            ViewBag.CurrentProjectId = projectId;
            
            // Projedeki fazları çek
            var phases = await _context.Set<ConstructionBudgetItem>()
                .Where(b => b.ConstructionProjectId == projectId.Value)
                .Select(b => b.PhaseCategory.ToString())
                .Distinct()
                .ToListAsync();
            
            ViewBag.Phases = phases;

            return View(timesheets);
        }

        [HttpPost]
        public async Task<IActionResult> SaveTimesheets(int projectId, DateTime workDate, int[] workerIds, string[] attendance, decimal[] pendingFieldExpenses, string[] fieldExpenseDesc, string[] selectedPhases)
        {
            try
            {
                for (int i = 0; i < workerIds.Length; i++)
                {
                    var workerId = workerIds[i];
                    var status = attendance[i]; // Örn: Tam Gün, Yarım Gün, Gelmedi
                    var fieldExpense = pendingFieldExpenses[i];
                    var fieldDesc = fieldExpenseDesc[i];
                    var phase = selectedPhases[i];

                    // İlgili günün puantajı var mı bak
                    var record = await _context.Set<DailyTimesheet>()
                        .FirstOrDefaultAsync(t => t.WorkDate.Date == workDate.Date && t.AgencyWorkerId == workerId && t.ConstructionProjectId == projectId);
                    
                    if (record == null)
                    {
                        record = new DailyTimesheet
                        {
                            AgencyId = 1,
                            AgencyWorkerId = workerId,
                            ConstructionProjectId = projectId,
                            WorkDate = workDate,
                            AttendanceStatus = status,
                            PhaseName = phase,
                            PendingFieldExpense = fieldExpense,
                            FieldExpenseDescription = fieldDesc
                        };
                        _context.Add(record);
                    }
                    else
                    {
                        record.AttendanceStatus = status;
                        record.PhaseName = phase;
                        record.PendingFieldExpense = fieldExpense;
                        record.FieldExpenseDescription = fieldDesc;
                        _context.Update(record);
                    }
                }
                
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Puantaj ve Taslak Saha Giderleri (Elden Mesailer) başarıyla kaydedildi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index), new { projectId = projectId, workDate = workDate.ToString("yyyy-MM-dd") });
        }
    }
}