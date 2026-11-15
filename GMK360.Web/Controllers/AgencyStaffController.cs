using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Identity;
using GMK360.Core.Entities.Finance;
using GMK360.Web.Helpers;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class AgencyStaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AgencyStaffController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetCurrentAgencyId()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            var consultant = await _context.AgencyConsultants
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.IsActive);

            if (consultant != null) return consultant.AgencyId;

            var fallback = await _context.Agencies.FirstOrDefaultAsync();
            return fallback?.Id;
        }

        [HttpGet]
        public async Task<IActionResult> Invite()
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Invite(
            string email,
            string firstName,
            string lastName,
            string role,
            string phoneNumber,
            string? identityNumber,
            decimal monthlySalary = 0,
            string? iban = null,
            string? address = null,
            string? city = null,
            string? district = null,
            string? bloodGroup = null,
            string? emergencyContactName = null,
            string? emergencyContactPhone = null,
            string? titleOrPosition = null,
            string[]? selectedPermissions = null)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(role))
            {
                ModelState.AddModelError("", "Lütfen gerekli tüm alanları doldurun.");
                return View();
            }

            var parsedRole = Enum.TryParse<GMK360.Core.Entities.AgencyRole>(role, out var r) ? r : GMK360.Core.Entities.AgencyRole.SiteManager;

            var hrDossier = new StaffHrDossier
            {
                Address = address?.Trim(),
                City = city?.Trim(),
                District = district?.Trim(),
                BloodGroup = bloodGroup?.Trim(),
                EmergencyContactName = emergencyContactName?.Trim(),
                EmergencyContactPhone = emergencyContactPhone?.Trim(),
                TitleOrPosition = titleOrPosition?.Trim()
            };

            // 1. Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                existingUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    DisplayName = $"{firstName} {lastName}".Trim(),
                    PhoneNumber = phoneNumber,
                    TcIdentityNo = identityNumber?.Trim(),
                    UserType = UserType.Corporate,
                    HasMapConsent = true,
                    MapConsentDate = DateTime.UtcNow,
                    RecoveryAnswer = StaffPermissionHelper.SerializeHrDossier(hrDossier),
                    ActiveModules = selectedPermissions != null && selectedPermissions.Any()
                        ? GMK360.Web.Helpers.StaffPermissionHelper.BuildUpdatedActiveModulesString("construction", selectedPermissions)
                        : "construction",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };
                var result = await _userManager.CreateAsync(existingUser, "TempPass123!*");
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View();
                }

                await _userManager.AddToRoleAsync(existingUser, "InsaatFirmasi");
            }
            else
            {
                if (selectedPermissions != null && selectedPermissions.Any())
                {
                    existingUser.ActiveModules = GMK360.Web.Helpers.StaffPermissionHelper.BuildUpdatedActiveModulesString(existingUser.ActiveModules, selectedPermissions);
                }
                existingUser.RecoveryAnswer = StaffPermissionHelper.SerializeHrDossier(hrDossier);
                if (!string.IsNullOrWhiteSpace(identityNumber)) existingUser.TcIdentityNo = identityNumber.Trim();
                await _userManager.UpdateAsync(existingUser);
            }

            // 2. Add to AgencyConsultants with specific Title/Role
            var existingConsultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == existingUser.Id && c.AgencyId == agencyId);
            if (existingConsultant == null)
            {
                var newConsultant = new GMK360.Core.Entities.AgencyConsultant
                {
                    AgencyId = agencyId.Value,
                    UserId = existingUser.Id,
                    Role = parsedRole,
                    StartDate = DateTime.UtcNow,
                    IsActive = true,
                    MonthlySalary = monthlySalary,
                    IBAN = iban?.Trim(),
                    IdentityNumber = identityNumber?.Trim()
                };
                _context.AgencyConsultants.Add(newConsultant);
                await _context.SaveChangesAsync();
            }
            else
            {
                existingConsultant.Role = parsedRole;
                existingConsultant.IsActive = true;
                if (monthlySalary > 0) existingConsultant.MonthlySalary = monthlySalary;
                if (!string.IsNullOrWhiteSpace(iban)) existingConsultant.IBAN = iban.Trim();
                if (!string.IsNullOrWhiteSpace(identityNumber)) existingConsultant.IdentityNumber = identityNumber.Trim();
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"{firstName} {lastName} özlük dosyası, maaş ve sol menü ekran yetkileriyle birlikte kaydedildi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var staff = await _context.AgencyConsultants
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id && a.AgencyId == agencyId);

            if (staff == null) return NotFound();

            var payrolls = await _context.AgencyStaffPayrolls
                .Where(p => p.AgencyConsultantId == id && !p.IsDeleted)
                .OrderByDescending(p => p.Period)
                .ToListAsync();

            var advances = await _context.AgencyStaffAdvances
                .Where(a => a.AgencyConsultantId == id && !a.IsDeleted)
                .OrderByDescending(a => a.RequestDate)
                .ToListAsync();

            var staffDocs = await _context.DocumentArchives
                .Where(d => d.AgencyId == agencyId && !d.IsDeleted && d.SourceModule == $"Staff_{id}")
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();

            ViewBag.Payrolls = payrolls;
            ViewBag.Advances = advances;
            ViewBag.StaffDocuments = staffDocs;
            ViewBag.HrDossier = StaffPermissionHelper.GetHrDossier(staff.User?.RecoveryAnswer);

            return View(staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoleAndPermissions(int id, string role, string[]? selectedPermissions, bool resetToRoleDefault = false)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var staff = await _context.AgencyConsultants
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id && a.AgencyId == agencyId);
            if (staff == null) return NotFound();

            if (Enum.TryParse<GMK360.Core.Entities.AgencyRole>(role, out var newRole))
            {
                staff.Role = newRole;
            }

            if (staff.User != null)
            {
                if (resetToRoleDefault)
                {
                    var roleDefaults = GMK360.Web.Helpers.StaffPermissionHelper.GetDefaultPermissionsForRole(staff.Role);
                    staff.User.ActiveModules = GMK360.Web.Helpers.StaffPermissionHelper.BuildUpdatedActiveModulesString(staff.User.ActiveModules, roleDefaults);
                }
                else
                {
                    staff.User.ActiveModules = GMK360.Web.Helpers.StaffPermissionHelper.BuildUpdatedActiveModulesString(staff.User.ActiveModules, selectedPermissions);
                }
                await _userManager.UpdateAsync(staff.User);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{staff.User?.FirstName} {staff.User?.LastName} için görev unvanı ve sol menü ekran yetkileri güncellendi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFinance(int id, decimal monthlySalary, string iban, string identityNumber)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var staff = await _context.AgencyConsultants.FirstOrDefaultAsync(a => a.Id == id && a.AgencyId == agencyId);
            if (staff == null) return NotFound();

            staff.MonthlySalary = monthlySalary;
            staff.IBAN = iban;
            staff.IdentityNumber = identityNumber;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Maaş ve banka bilgileri güncellendi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateHrDossier(
            int id,
            string? firstName,
            string? lastName,
            string? phoneNumber,
            string? identityNumber,
            string? iban,
            decimal monthlySalary = 0,
            string? address = null,
            string? city = null,
            string? district = null,
            string? bloodGroup = null,
            string? emergencyContactName = null,
            string? emergencyContactPhone = null,
            string? titleOrPosition = null,
            string? notes = null)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var staff = await _context.AgencyConsultants
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id && a.AgencyId == agencyId);
            if (staff == null) return NotFound();

            staff.IdentityNumber = identityNumber?.Trim();
            staff.IBAN = iban?.Trim();
            if (monthlySalary >= 0) staff.MonthlySalary = monthlySalary;

            if (staff.User != null)
            {
                if (!string.IsNullOrWhiteSpace(firstName)) staff.User.FirstName = firstName.Trim();
                if (!string.IsNullOrWhiteSpace(lastName)) staff.User.LastName = lastName.Trim();
                staff.User.DisplayName = $"{staff.User.FirstName} {staff.User.LastName}".Trim();
                if (!string.IsNullOrWhiteSpace(phoneNumber)) staff.User.PhoneNumber = phoneNumber.Trim();
                if (!string.IsNullOrWhiteSpace(identityNumber)) staff.User.TcIdentityNo = identityNumber.Trim();

                var dossier = new StaffHrDossier
                {
                    Address = address?.Trim(),
                    City = city?.Trim(),
                    District = district?.Trim(),
                    BloodGroup = bloodGroup?.Trim(),
                    EmergencyContactName = emergencyContactName?.Trim(),
                    EmergencyContactPhone = emergencyContactPhone?.Trim(),
                    TitleOrPosition = titleOrPosition?.Trim(),
                    Notes = notes?.Trim()
                };
                staff.User.RecoveryAnswer = StaffPermissionHelper.SerializeHrDossier(dossier);
                await _userManager.UpdateAsync(staff.User);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Personel özlük, ikametgah adresi ve iletişim bilgileri güncellendi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadStaffDocument(int id, string documentType, IFormFile? file, string? notes)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var staff = await _context.AgencyConsultants
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id && a.AgencyId == agencyId);
            if (staff == null) return NotFound();

            if (file != null && file.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "archive", agencyId.Value.ToString());
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString("N").Substring(0, 8) + "_" + Path.GetFileName(file.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var docTitle = $"[{documentType}] {staff.User?.FirstName} {staff.User?.LastName}" + (!string.IsNullOrWhiteSpace(notes) ? $" - {notes.Trim()}" : "");
                var archive = new DocumentArchive
                {
                    AgencyId = agencyId.Value,
                    FileName = file.FileName,
                    DocumentUrl = $"/uploads/archive/{agencyId.Value}/{uniqueFileName}",
                    FileExtension = Path.GetExtension(file.FileName).ToLowerInvariant(),
                    Category = "Merkez Personel & İK",
                    Year = DateTime.Now.Year,
                    Title = docTitle,
                    SourceModule = $"Staff_{id}",
                    ProjectId = null,
                    UploadDate = DateTime.Now
                };

                _context.DocumentArchives.Add(archive);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{documentType} belgesi personel özlük dosyasına ve Merkez İK Arşivine eklendi.";
            }
            else
            {
                TempData["SuccessMessage"] = "Lütfen yüklenecek belge dosyasını seçin.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStaffDocument(int id, int docId)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var doc = await _context.DocumentArchives.FirstOrDefaultAsync(d => d.Id == docId && d.AgencyId == agencyId && d.SourceModule == $"Staff_{id}");
            if (doc != null)
            {
                doc.IsDeleted = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Özlük belgesi kaldırıldı.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePdksMonthlyPayroll(
            int id,
            string period,
            decimal baseSalary,
            int normalDays,
            int holidayDoubleDays,
            decimal overtimeDaysOrHoursPay,
            int unpaidLeaveDays,
            decimal advanceDeduction,
            string? calendarSummary)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var staff = await _context.AgencyConsultants.FirstOrDefaultAsync(a => a.Id == id && a.AgencyId == agencyId);
            if (staff == null) return NotFound();

            if (baseSalary > 0 && staff.MonthlySalary != baseSalary)
            {
                staff.MonthlySalary = baseSalary;
            }

            decimal dailyWage = Math.Round(baseSalary / 30m, 2);
            // Bayram / Resmi Tatil Çift Yevmiye: Maaşın içinde 1x zaten var, çalışılan her bayram günü için +1x ilave yevmiye eklenir (Toplam 2x Çift Yevmiye)
            decimal holidayExtraPay = Math.Round(holidayDoubleDays * dailyWage, 2);
            decimal totalBonus = Math.Round(holidayExtraPay + overtimeDaysOrHoursPay, 2);
            decimal unpaidDeduction = Math.Round(unpaidLeaveDays * dailyWage, 2);
            decimal totalDeduction = Math.Round(unpaidDeduction + advanceDeduction, 2);
            decimal netPayable = Math.Round(baseSalary + totalBonus - totalDeduction, 2);

            var existingPayroll = await _context.AgencyStaffPayrolls
                .FirstOrDefaultAsync(p => p.AgencyConsultantId == id && p.Period == period && !p.IsDeleted);

            string summaryNote = $"Normal: {normalDays}g | Bayram Çift Yevmiye (2x): {holidayDoubleDays}g (+{holidayExtraPay:N2}₺) | Mesai/Prim: +{overtimeDaysOrHoursPay:N2}₺ | Kesinti: {unpaidLeaveDays}g (-{unpaidDeduction:N2}₺)";
            if (!string.IsNullOrWhiteSpace(calendarSummary))
            {
                summaryNote += $" || {calendarSummary}";
            }

            if (existingPayroll == null)
            {
                existingPayroll = new AgencyStaffPayroll
                {
                    AgencyConsultantId = id,
                    Period = period,
                    BaseSalary = baseSalary,
                    BonusAmount = totalBonus,
                    DeductionAmount = totalDeduction,
                    NetPayableAmount = netPayable,
                    IsPaid = false,
                    GenerationDate = DateTime.UtcNow,
                    Notes = summaryNote
                };
                _context.AgencyStaffPayrolls.Add(existingPayroll);
            }
            else
            {
                existingPayroll.BaseSalary = baseSalary;
                existingPayroll.BonusAmount = totalBonus;
                existingPayroll.DeductionAmount = totalDeduction;
                existingPayroll.NetPayableAmount = netPayable;
                existingPayroll.Notes = summaryNote;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{period} dönemi Basit PDKS & Bayram Çift Yevmiye hesabı kaydedildi. Net Hakediş: ₺{netPayable:N2}";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public IActionResult SwitchRolePreview(string? mode, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(mode) || mode.Equals("clear", StringComparison.OrdinalIgnoreCase) || mode.Equals("owner", StringComparison.OrdinalIgnoreCase))
            {
                Response.Cookies.Delete("gmk360_role_preview");
                TempData["SuccessMessage"] = "👑 Firma Sahibi / Genel Müdür (Tam Yetki) görünümüne dönüldü.";
            }
            else
            {
                Response.Cookies.Append("gmk360_role_preview", mode.Trim().ToLowerInvariant(), new Microsoft.AspNetCore.Http.CookieOptions
                {
                    HttpOnly = true,
                    IsEssential = true,
                    Expires = DateTimeOffset.UtcNow.AddHours(8)
                });
                var label = mode.ToLowerInvariant() switch
                {
                    "engineer" => "📐 Teknik Ofis / Mühendis (Finans & Kurumsal Web Gizli)",
                    "formen" => "👷 Şantiye Şefi / Saha Formeni (Sadece Saha & Demirbaş)",
                    _ => mode
                };
                TempData["SuccessMessage"] = $"👁️ Rol Ön İzleme Aktif: Şu an sistemi [{label}] gözüyle görüyorsunuz.";
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Construction", "Dashboard");
        }

        public async Task<IActionResult> Index()
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var staff = await _context.AgencyConsultants
                .Include(c => c.User)
                .Where(c => c.AgencyId == agencyId)
                .ToListAsync();

            return View(staff);
        }
    }
}
