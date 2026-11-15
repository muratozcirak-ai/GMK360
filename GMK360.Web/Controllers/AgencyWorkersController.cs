using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Construction;
using GMK360.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace GMK360.Web.Controllers
{
    public class WorkerExtendedHrMeta
    {
        public string IdentityNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public string District { get; set; } = "";
        public string BloodGroup { get; set; } = "";
        public string Iban { get; set; } = "";
        public string TeamOrSubcontractorName { get; set; } = "";
        public string EmergencyContactName { get; set; } = "";
        public string EmergencyContactPhone { get; set; } = "";
        public List<WorkerDocRecord> Documents { get; set; } = new List<WorkerDocRecord>();
    }

    public class WorkerDocRecord
    {
        public string DocType { get; set; } = "";
        public string DocTitle { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string Notes { get; set; } = "";
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }

    [Authorize]
    public class AgencyWorkersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AgencyWorkersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int> ResolveCurrentAgencyIdAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userId))
            {
                var consultant = await _context.AgencyConsultants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
                if (consultant != null && consultant.AgencyId > 0)
                    return consultant.AgencyId;
            }

            var firstAgency = await _context.Agencies.AsNoTracking().OrderBy(a => a.Id).FirstOrDefaultAsync();
            return firstAgency?.Id ?? 1;
        }

        private static WorkerExtendedHrMeta ParseWorkerMeta(string? rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson))
                return new WorkerExtendedHrMeta();
            try
            {
                if (rawJson.TrimStart().StartsWith("{"))
                    return JsonSerializer.Deserialize<WorkerExtendedHrMeta>(rawJson) ?? new WorkerExtendedHrMeta();
            }
            catch { }
            return new WorkerExtendedHrMeta();
        }

        public async Task<IActionResult> Index()
        {
            var agencyId = await ResolveCurrentAgencyIdAsync();
            var workers = await _context.Set<AgencyWorker>()
                .Include(w => w.User)
                .Where(w => w.AgencyId == agencyId && !w.IsDeleted)
                .OrderByDescending(w => w.IsActive)
                .ThenBy(w => w.FirstName)
                .ToListAsync();

            var workerIds = workers.Select(w => w.Id).ToList();
            var timesheets = await _context.Set<DailyTimesheet>()
                .Include(t => t.ConstructionProject)
                .Where(t => workerIds.Contains(t.AgencyWorkerId) && !t.IsDeleted)
                .ToListAsync();

            var metaDict = new Dictionary<int, WorkerExtendedHrMeta>();
            var summaryDict = new Dictionary<int, (int WorkDays, int SiteCount, decimal TotalEarned, decimal TotalPaid, decimal NetBalance)>();

            foreach (var w in workers)
            {
                metaDict[w.Id] = ParseWorkerMeta(w.User?.RecoveryAnswer);
                var wTimesheets = timesheets.Where(t => t.AgencyWorkerId == w.Id).ToList();
                int workDays = wTimesheets.Count(t => t.AttendanceStatus != "Gelmedi" && t.AttendanceStatus != "Avans / Ödeme");
                int siteCount = wTimesheets.Where(t => t.ConstructionProjectId.HasValue).Select(t => t.ConstructionProjectId!.Value).Distinct().Count();
                decimal earned = wTimesheets.Sum(t => t.EarnedWage);
                decimal paid = wTimesheets.Sum(t => t.AdvancePayment);
                summaryDict[w.Id] = (workDays, siteCount, earned, paid, earned - paid);
            }

            ViewBag.WorkerMeta = metaDict;
            ViewBag.WorkerSummaries = summaryDict;
            ViewBag.Projects = await _context.ConstructionProjects
                .AsNoTracking()
                .Where(p => p.AgencyId == agencyId && !p.IsDeleted)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(workers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string firstName,
            string lastName,
            string phoneNumber,
            string profession,
            decimal dailyWage,
            string? identityNumber,
            string? address,
            string? city,
            string? district,
            string? bloodGroup,
            string? iban,
            string? teamOrSubcontractorName,
            string? emergencyContactPhone)
        {
            try
            {
                var agencyId = await ResolveCurrentAgencyIdAsync();
                var cleanPhone = (phoneNumber ?? "").Trim();
                if (string.IsNullOrEmpty(cleanPhone))
                {
                    TempData["ErrorMessage"] = "Telefon numarası zorunludur.";
                    return RedirectToAction(nameof(Index));
                }

                var user = await _userManager.FindByNameAsync(cleanPhone);
                var meta = new WorkerExtendedHrMeta
                {
                    IdentityNumber = identityNumber ?? "",
                    Address = address ?? "",
                    City = city ?? "",
                    District = district ?? "",
                    BloodGroup = bloodGroup ?? "",
                    Iban = iban ?? "",
                    TeamOrSubcontractorName = teamOrSubcontractorName ?? "",
                    EmergencyContactPhone = emergencyContactPhone ?? ""
                };

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = cleanPhone,
                        PhoneNumber = cleanPhone,
                        FirstName = firstName ?? "",
                        LastName = lastName ?? "",
                        UserType = UserType.ServiceProvider,
                        RecoveryAnswer = JsonSerializer.Serialize(meta)
                    };
                    await _userManager.CreateAsync(user, "MaviYaka123!");
                }
                else
                {
                    user.RecoveryAnswer = JsonSerializer.Serialize(meta);
                    await _userManager.UpdateAsync(user);
                }

                var worker = new AgencyWorker
                {
                    AgencyId = agencyId,
                    FirstName = firstName ?? "",
                    LastName = lastName ?? "",
                    PhoneNumber = cleanPhone,
                    Profession = profession ?? "Saha Ustası",
                    DefaultDailyWage = dailyWage,
                    NetDailyWage = dailyWage,
                    UserId = user.Id,
                    IsActive = true
                };

                _context.Add(worker);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"{worker.FullName} ({worker.Profession}) özlük kartı ve yevmiye/puantaj hesabı başarıyla oluşturuldu.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Hata: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var agencyId = await ResolveCurrentAgencyIdAsync();
            var worker = await _context.Set<AgencyWorker>()
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.Id == id && w.AgencyId == agencyId && !w.IsDeleted);

            if (worker == null)
                return RedirectToAction(nameof(Index));

            var meta = ParseWorkerMeta(worker.User?.RecoveryAnswer);
            var timesheets = await _context.Set<DailyTimesheet>()
                .Include(t => t.ConstructionProject)
                .Where(t => t.AgencyWorkerId == id && !t.IsDeleted)
                .OrderByDescending(t => t.WorkDate)
                .ThenByDescending(t => t.Id)
                .ToListAsync();

            var projects = await _context.ConstructionProjects
                .AsNoTracking()
                .Where(p => p.AgencyId == agencyId && !p.IsDeleted)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            ViewBag.Meta = meta;
            ViewBag.Timesheets = timesheets;
            ViewBag.Projects = projects;

            return View(worker);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateWorkerDetails(
            int id,
            string firstName,
            string lastName,
            string phoneNumber,
            string profession,
            decimal dailyWage,
            bool isActive,
            string? identityNumber,
            string? address,
            string? city,
            string? district,
            string? bloodGroup,
            string? iban,
            string? teamOrSubcontractorName,
            string? emergencyContactName,
            string? emergencyContactPhone)
        {
            var agencyId = await ResolveCurrentAgencyIdAsync();
            var worker = await _context.Set<AgencyWorker>()
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.Id == id && w.AgencyId == agencyId && !w.IsDeleted);

            if (worker == null)
                return RedirectToAction(nameof(Index));

            worker.FirstName = firstName ?? worker.FirstName;
            worker.LastName = lastName ?? worker.LastName;
            worker.PhoneNumber = phoneNumber ?? worker.PhoneNumber;
            worker.Profession = profession ?? worker.Profession;
            worker.DefaultDailyWage = dailyWage;
            worker.NetDailyWage = dailyWage;
            worker.IsActive = isActive;

            if (worker.User != null)
            {
                var meta = ParseWorkerMeta(worker.User.RecoveryAnswer);
                meta.IdentityNumber = identityNumber ?? "";
                meta.Address = address ?? "";
                meta.City = city ?? "";
                meta.District = district ?? "";
                meta.BloodGroup = bloodGroup ?? "";
                meta.Iban = iban ?? "";
                meta.TeamOrSubcontractorName = teamOrSubcontractorName ?? "";
                meta.EmergencyContactName = emergencyContactName ?? "";
                meta.EmergencyContactPhone = emergencyContactPhone ?? "";
                worker.User.RecoveryAnswer = JsonSerializer.Serialize(meta);
                worker.User.FirstName = worker.FirstName;
                worker.User.LastName = worker.LastName;
                worker.User.PhoneNumber = worker.PhoneNumber;
                await _userManager.UpdateAsync(worker.User);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{worker.FullName} özlük, adres ve yevmiye bilgileri güncellendi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWorkerTimesheetOrAdvance(
            int workerId,
            int? constructionProjectId,
            DateTime workDate,
            string attendanceStatus,
            decimal overtimeHours = 0,
            decimal advancePayment = 0,
            decimal pendingFieldExpense = 0,
            string? phaseName = null,
            string? notes = null)
        {
            var agencyId = await ResolveCurrentAgencyIdAsync();
            var worker = await _context.Set<AgencyWorker>()
                .FirstOrDefaultAsync(w => w.Id == workerId && w.AgencyId == agencyId && !w.IsDeleted);

            if (worker == null)
                return RedirectToAction(nameof(Index));

            decimal baseWage = worker.DefaultDailyWage;
            decimal hourlyRate = baseWage / 8m;
            decimal earnedWage = 0m;

            switch (attendanceStatus)
            {
                case "Tam Gün":
                    earnedWage = baseWage + (overtimeHours * hourlyRate * 1.5m);
                    break;
                case "Yarım Gün":
                    earnedWage = (baseWage * 0.5m) + (overtimeHours * hourlyRate * 1.5m);
                    break;
                case "Bayram / Resmi Tatil (2x Çift Yevmiye)":
                    earnedWage = (baseWage * 2.0m) + (overtimeHours * hourlyRate * 2.0m);
                    break;
                case "Fazla Mesaili (1.5x)":
                    earnedWage = baseWage + ((overtimeHours > 0 ? overtimeHours : 2m) * hourlyRate * 1.5m);
                    break;
                case "Ücretli İzin":
                    earnedWage = baseWage;
                    break;
                case "Gelmedi":
                case "Avans / Ödeme":
                default:
                    earnedWage = 0m;
                    break;
            }

            var record = new DailyTimesheet
            {
                AgencyId = agencyId,
                AgencyWorkerId = workerId,
                ConstructionProjectId = constructionProjectId,
                WorkDate = workDate,
                AttendanceStatus = attendanceStatus ?? "Tam Gün",
                OvertimeHours = overtimeHours,
                EarnedWage = Math.Round(earnedWage, 2),
                AdvancePayment = advancePayment,
                PendingFieldExpense = pendingFieldExpense,
                PhaseName = phaseName,
                FieldExpenseDescription = notes,
                Notes = notes,
                IsMorningPresent = attendanceStatus != "Gelmedi" && attendanceStatus != "Avans / Ödeme",
                IsAfternoonPresent = attendanceStatus == "Tam Gün" || attendanceStatus == "Bayram / Resmi Tatil (2x Çift Yevmiye)" || attendanceStatus == "Fazla Mesaili (1.5x)",
                RecordedByUserId = _userManager.GetUserId(User)
            };

            _context.Add(record);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{workDate:dd.MM.yyyy} tarihli puantaj / ödeme kaydı (Hak Edilen: ₺{record.EarnedWage:N2}, Ödenen Avans/Yevmiye: ₺{record.AdvancePayment:N2}) işlendi.";
            return RedirectToAction(nameof(Details), new { id = workerId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadWorkerDocument(int workerId, string docType, string? docNotes, IFormFile? file)
        {
            var agencyId = await ResolveCurrentAgencyIdAsync();
            var worker = await _context.Set<AgencyWorker>()
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.Id == workerId && w.AgencyId == agencyId && !w.IsDeleted);

            if (worker == null || worker.User == null)
                return RedirectToAction(nameof(Index));

            string savedRelativePath = "";
            if (file != null && file.Length > 0)
            {
                var ext = Path.GetExtension(file.FileName);
                var safeName = $"worker_{workerId}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "worker_docs");
                Directory.CreateDirectory(folder);
                var fullPath = Path.Combine(folder, safeName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                savedRelativePath = $"/uploads/worker_docs/{safeName}";
            }

            var meta = ParseWorkerMeta(worker.User.RecoveryAnswer);
            meta.Documents.RemoveAll(d => d.DocType == docType);
            meta.Documents.Add(new WorkerDocRecord
            {
                DocType = docType,
                DocTitle = docType,
                FilePath = savedRelativePath,
                Notes = docNotes ?? "",
                UploadedAt = DateTime.UtcNow
            });

            worker.User.RecoveryAnswer = JsonSerializer.Serialize(meta);
            await _userManager.UpdateAsync(worker.User);

            if (!string.IsNullOrEmpty(savedRelativePath))
            {
                _context.DocumentArchives.Add(new DocumentArchive
                {
                    AgencyId = agencyId,
                    FileName = file?.FileName ?? docType,
                    DocumentUrl = savedRelativePath,
                    FileExtension = Path.GetExtension(savedRelativePath).ToLowerInvariant(),
                    Category = "Saha Personel & İSG",
                    Year = DateTime.Now.Year,
                    Title = $"{worker.FullName} - {docType}" + (!string.IsNullOrWhiteSpace(docNotes) ? $" ({docNotes})" : ""),
                    SourceModule = $"Worker_{workerId}",
                    ProjectId = null,
                    UploadDate = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"{worker.FullName} için '{docType}' belgesi özlük dosyasına işlendi.";
            return RedirectToAction(nameof(Details), new { id = workerId });
        }
    }
}