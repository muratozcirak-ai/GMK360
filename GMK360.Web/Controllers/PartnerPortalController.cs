using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities.Identity;
using GMK360.Core.Entities.Construction;
using GMK360.Core.Entities.Finance;
using GMK360.Data.Contexts;
using System;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class PartnerPortalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PartnerPortalController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<AgencyPhonebook?> GetPartnerContactAsync()
        {
            var userId = _userManager.GetUserId(User);
            return await _context.AgencyPhonebooks.FirstOrDefaultAsync(p => p.LinkedUserId == userId);
        }

        public async Task<IActionResult> Index()
        {
            var contact = await GetPartnerContactAsync();
            if (contact == null) return View("NotAPartner");

            var account = await _context.Set<SupplierCurrentAccount>()
                .Include(a => a.Transactions.OrderByDescending(t => t.TransactionDate).Take(5))
                .Include(a => a.Payments.OrderByDescending(p => p.PaymentDate).Take(5))
                .FirstOrDefaultAsync(a => a.PhonebookContactId == contact.Id);

            ViewBag.Contact = contact;
            ViewBag.Account = account;
            
            var contracts = await _context.Set<SubcontractorContract>()
                .Include(c => c.Project)
                .Where(c => c.PhonebookContactId == contact.Id)
                .ToListAsync();
            ViewBag.Contracts = contracts;

            if (contact.ContactType == 3)
            {
                return View("SupplierDashboard");
            }
            else
            {
                var workersCount = await _context.AgencyWorkers.CountAsync(w => w.SubcontractorContactId == contact.Id && w.IsActive);
                ViewBag.WorkersCount = workersCount;
                return View("SubcontractorDashboard");
            }
        }

        // TAŞERON ÖZEL: Ekiplerim (Workers)
        public async Task<IActionResult> Workers()
        {
            var contact = await GetPartnerContactAsync();
            if (contact == null) return Unauthorized();

            var workers = await _context.AgencyWorkers
                .Where(w => w.SubcontractorContactId == contact.Id && w.IsActive)
                .OrderBy(w => w.FirstName).ThenBy(w => w.LastName)
                .ToListAsync();

            ViewBag.Contact = contact;
            return View(workers);
        }

        [HttpPost]
        public async Task<IActionResult> AddWorker(string FirstName, string LastName, string IdentityNumber, string PhoneNumber, string Profession)
        {
            var contact = await GetPartnerContactAsync();
            if (contact == null) return Unauthorized();

            var worker = new AgencyWorker
            {
                AgencyId = contact.AgencyId,
                FirstName = FirstName,
                LastName = LastName,
                IdentityNumber = IdentityNumber,
                PhoneNumber = PhoneNumber,
                Profession = Profession,
                WorkerType = "Taşeron Personeli",
                SubcontractorName = contact.Name,
                SubcontractorContactId = contact.Id,
                IsActive = true
            };

            _context.AgencyWorkers.Add(worker);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Workers));
        }

        // TAŞERON ÖZEL: Kör Puantaj (Timesheets)
        public async Task<IActionResult> Timesheets(DateTime? date, int? projectId)
        {
            var contact = await GetPartnerContactAsync();
            if (contact == null) return Unauthorized();

            DateTime targetDate = date ?? DateTime.Today;
            ViewBag.TargetDate = targetDate;
            ViewBag.Contact = contact;

            var workers = await _context.AgencyWorkers
                .Where(w => w.SubcontractorContactId == contact.Id && w.IsActive)
                .OrderBy(w => w.FirstName).ToListAsync();

            var existingTimesheets = await _context.DailyTimesheets
                .Where(t => t.WorkDate.Date == targetDate.Date && workers.Select(w => w.Id).Contains(t.AgencyWorkerId))
                .ToDictionaryAsync(t => t.AgencyWorkerId);

            ViewBag.Workers = workers;
            ViewBag.ExistingTimesheets = existingTimesheets;
            ViewBag.CurrentProjectId = projectId;

            var activeContracts = await _context.Set<SubcontractorContract>()
                .Include(c => c.Project)
                .Where(c => c.PhonebookContactId == contact.Id && c.IsActive)
                .ToListAsync();
            
            ViewBag.Projects = activeContracts.Select(c => c.Project).Distinct().ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveTimesheets(DateTime targetDate, int projectId, int[] workerIds, string[] statuses, string[] notesList)
        {
            var contact = await GetPartnerContactAsync();
            if (contact == null) return Unauthorized();

            if (workerIds == null || workerIds.Length == 0)
                return RedirectToAction(nameof(Timesheets), new { date = targetDate.ToString("yyyy-MM-dd"), projectId });

            var workers = await _context.AgencyWorkers.Where(w => workerIds.Contains(w.Id) && w.SubcontractorContactId == contact.Id).ToDictionaryAsync(w => w.Id);

            for (int i = 0; i < workerIds.Length; i++)
            {
                var wId = workerIds[i];
                if (!workers.ContainsKey(wId)) continue;
                
                var worker = workers[wId];
                var status = statuses != null && i < statuses.Length ? statuses[i] : "Gelmedi";
                var note = notesList != null && i < notesList.Length ? notesList[i] : "";
                
                var existing = await _context.DailyTimesheets
                    .FirstOrDefaultAsync(t => t.AgencyWorkerId == wId && t.WorkDate.Date == targetDate.Date);

                // KÖR PUANTAJ: Taşeron veya Şef yevmiye görmez. Sistem net yevmiyeden hesaplar.
                decimal calculatedWage = 0;
                if(status == "Tam Gün") calculatedWage = worker.NetDailyWage;
                else if(status == "Yarım Gün") calculatedWage = worker.NetDailyWage / 2;

                if (existing != null)
                {
                    existing.AttendanceStatus = status;
                    existing.Notes = note;
                    existing.EarnedWage = calculatedWage;
                }
                else
                {
                    var newTimesheet = new DailyTimesheet
                    {
                        AgencyId = contact.AgencyId,
                        AgencyWorkerId = wId,
                        WorkDate = targetDate.Date,
                        AttendanceStatus = status,
                        EarnedWage = calculatedWage,
                        Notes = note
                    };
                    _context.DailyTimesheets.Add(newTimesheet);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Timesheets), new { date = targetDate.ToString("yyyy-MM-dd"), projectId });
        }
        
        // HAKEDİŞLER (Taşeron)
        public async Task<IActionResult> Contracts()
        {
            var contact = await GetPartnerContactAsync();
            if (contact == null) return Unauthorized();

            var contracts = await _context.Set<SubcontractorContract>()
                .Include(c => c.Project)
                .Include(c => c.Hakedisler.OrderByDescending(p => p.HakedisDate))
                .Where(c => c.PhonebookContactId == contact.Id)
                .ToListAsync();

            ViewBag.Contact = contact;
            return View(contracts);
        }
    }
}
