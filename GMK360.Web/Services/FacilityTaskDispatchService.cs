using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Services
{
    public sealed class QuickPinLoginResult
    {
        public bool Success { get; set; }
        public ApplicationUser? User { get; set; }
        public string RoleKind { get; set; } = string.Empty; // "Staff" | "Resident"
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// 3 Rol (Yönetici / Kat Maliki-Kiracı / Site Görevlisi) için fotoğraflı talep &amp; görev atama akışı,
    /// Temizlik/Çöp otomatik yönlendirme, kanıt fotoğrafı kayıt ve Telefon+PIN hızlı PWA girişi servisi.
    /// </summary>
    public class FacilityTaskDispatchService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public FacilityTaskDispatchService(
            ApplicationDbContext context,
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public static string NormalizePhoneLast10(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            return digits.Length > 10 ? digits[^10..] : digits;
        }

        public static string HashQuickPin(string pin)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes("GMK360_PIN_V1:" + (pin ?? "").Trim()));
            return Convert.ToHexString(bytes);
        }

        public static bool VerifyQuickPin(string pin, string? storedHash)
        {
            if (string.IsNullOrWhiteSpace(pin) || string.IsNullOrWhiteSpace(storedHash)) return false;
            return string.Equals(HashQuickPin(pin), storedHash, StringComparison.OrdinalIgnoreCase);
        }

        public async Task<string?> SaveTaskPhotoAsync(int buildingId, string stage, IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;
            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(ext))
            {
                ext = ".jpg";
            }

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folder = Path.Combine(webRoot, "uploads", "buildings", buildingId.ToString(), "tasks");
            Directory.CreateDirectory(folder);

            var name = $"{stage}_{buildingId}_{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(folder, name);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return $"/uploads/buildings/{buildingId}/tasks/{name}";
        }

        public async Task<BuildingTaskTicket> CreateTicketAsync(
            int buildingId,
            int? unitId,
            string? createdByUserId,
            string createdByRole,
            string category,
            string? locationNote,
            string description,
            IFormFile? beforePhoto,
            bool autoRouteCleaning,
            string priority = "Normal")
        {
            string? beforeUrl = await SaveTaskPhotoAsync(buildingId, "before", beforePhoto);

            int? assignedStaffId = null;
            string status = "Bekliyor";
            DateTime? assignedAt = null;

            // Temizlik/Cop kategorisi seçildiğinde bina temizlik/kapıcı görevlisine otomatik yönlendir
            if (autoRouteCleaning && string.Equals(category, "Temizlik/Cop", StringComparison.OrdinalIgnoreCase))
            {
                var cleaningStaff = await _context.BuildingStaffs
                    .Where(s => s.BuildingId == buildingId && s.IsActive && !s.IsDeleted
                        && (s.RoleType.Contains("Temizlik") || s.RoleType.Contains("Kapıcı")))
                    .OrderBy(s => s.Id)
                    .FirstOrDefaultAsync();

                if (cleaningStaff != null)
                {
                    assignedStaffId = cleaningStaff.Id;
                    status = "Atandi";
                    assignedAt = DateTime.UtcNow;
                }
            }

            var ticket = new BuildingTaskTicket
            {
                BuildingId = buildingId,
                BuildingUnitId = unitId,
                CreatedByUserId = createdByUserId,
                CreatedByRole = string.IsNullOrWhiteSpace(createdByRole) ? "Kiraci" : createdByRole.Trim(),
                Category = string.IsNullOrWhiteSpace(category) ? "Diger" : category.Trim(),
                LocationNote = locationNote?.Trim(),
                Description = description.Trim(),
                BeforePhotoUrl = beforeUrl,
                AssignedStaffId = assignedStaffId,
                Priority = string.Equals(priority, "Acil", StringComparison.OrdinalIgnoreCase) ? "Acil" : "Normal",
                Status = status,
                AssignedAt = assignedAt,
                CreatedAt = DateTime.UtcNow
            };

            _context.BuildingTaskTickets.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<bool> AssignTicketAsync(int ticketId, int staffId, string priority)
        {
            var ticket = await _context.BuildingTaskTickets.FirstOrDefaultAsync(t => t.Id == ticketId && !t.IsDeleted);
            if (ticket == null) return false;

            ticket.AssignedStaffId = staffId;
            ticket.Priority = string.Equals(priority, "Acil", StringComparison.OrdinalIgnoreCase) ? "Acil" : "Normal";
            ticket.Status = "Atandi";
            ticket.AssignedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteTicketAsync(int ticketId, string? completionNote, IFormFile? afterPhoto)
        {
            var ticket = await _context.BuildingTaskTickets.FirstOrDefaultAsync(t => t.Id == ticketId && !t.IsDeleted);
            if (ticket == null) return false;

            string? afterUrl = await SaveTaskPhotoAsync(ticket.BuildingId, "after", afterPhoto);
            if (!string.IsNullOrEmpty(afterUrl)) ticket.AfterPhotoUrl = afterUrl;
            ticket.StaffCompletionNote = completionNote?.Trim();
            ticket.Status = "Tamamlandi";
            ticket.CompletedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<QuickPinLoginResult> AuthenticateByPhoneAndPinAsync(string phone, string pin)
        {
            var normPhone = NormalizePhoneLast10(phone);
            if (normPhone.Length < 10 || string.IsNullOrWhiteSpace(pin) || pin.Trim().Length < 4)
            {
                return new QuickPinLoginResult { Success = false, Message = "Geçerli 10 haneli telefon numarası ve 4-6 haneli PIN giriniz." };
            }

            // 1. Site Görevlisi eşleşmesi
            var activeStaffs = await _context.BuildingStaffs
                .Include(s => s.Building)
                .Where(s => s.IsActive && !s.IsDeleted && s.QuickPinHash != null && s.PhoneNumber != null)
                .ToListAsync();

            var matchedStaff = activeStaffs.FirstOrDefault(s =>
                NormalizePhoneLast10(s.PhoneNumber) == normPhone && VerifyQuickPin(pin, s.QuickPinHash));

            if (matchedStaff != null)
            {
                var user = await EnsurePortalUserAsync(
                    matchedStaff.UserId,
                    $"staff_{normPhone}@gmk360.local",
                    matchedStaff.FullName,
                    normPhone,
                    "Personel",
                    "facility_staff");

                if (matchedStaff.UserId != user.Id)
                {
                    matchedStaff.UserId = user.Id;
                    await _context.SaveChangesAsync();
                }

                return new QuickPinLoginResult
                {
                    Success = true,
                    User = user,
                    RoleKind = "Staff",
                    Message = $"Hoş geldiniz {matchedStaff.FullName} (Görevli PWA)."
                };
            }

            // 2. Kişisel Profil Onay PIN'i eşleşmesi (Kullanıcının kendi profilinden belirlediği gizli PIN)
            var usersWithPersonalPin = await _context.Users
                .Where(u => u.IsTransactionPinSet && u.TransactionPinHash != null && u.PhoneNumber != null)
                .ToListAsync();

            var personalMatch = usersWithPersonalPin.FirstOrDefault(u =>
                NormalizePhoneLast10(u.PhoneNumber) == normPhone && VerifyQuickPin(pin, u.TransactionPinHash));

            if (personalMatch != null)
            {
                var relatedUnits = await _context.BuildingUnits
                    .Where(bu => bu.OwnerUserId == personalMatch.Id
                              || bu.TenantUserId == personalMatch.Id
                              || bu.OwnerPhone != null
                              || bu.TenantPhone != null)
                    .ToListAsync();

                var tenantUnit = relatedUnits.FirstOrDefault(bu =>
                    !bu.IsEmpty && (bu.TenantUserId == personalMatch.Id || NormalizePhoneLast10(bu.TenantPhone) == normPhone));
                var ownerUnit = relatedUnits.FirstOrDefault(bu =>
                    bu.OwnerUserId == personalMatch.Id || NormalizePhoneLast10(bu.OwnerPhone) == normPhone);

                string resolvedRole = tenantUnit != null ? "Tenant" : "Owner";
                var checkUnit = tenantUnit ?? ownerUnit;
                if (checkUnit == null || !await IsRoleArchivedOnlyAsync(checkUnit.Id, resolvedRole))
                {
                    return new QuickPinLoginResult
                    {
                        Success = true,
                        User = personalMatch,
                        RoleKind = resolvedRole,
                        Message = $"Hoş geldiniz {personalMatch.FullName} ({(resolvedRole == "Tenant" ? "Kiracı" : "Malik")})."
                    };
                }
            }

            // 3. Eski daire bazlı Kat Maliki / Kiracı ayrı PIN eşleşmesi (Geriye dönük uyumluluk)
            var unitsWithPin = await _context.BuildingUnits
                .Where(u => (u.OwnerQuickPinHash != null || u.TenantQuickPinHash != null || u.ResidentQuickPinHash != null)
                    && (u.TenantPhone != null || u.OwnerPhone != null))
                .ToListAsync();

            foreach (var u in unitsWithPin)
            {
                bool phoneMatchesTenant = !u.IsEmpty && NormalizePhoneLast10(u.TenantPhone) == normPhone;
                bool phoneMatchesOwner = NormalizePhoneLast10(u.OwnerPhone) == normPhone;
                if (!phoneMatchesTenant && !phoneMatchesOwner) continue;

                string? matchedRole = null;
                if (phoneMatchesTenant && VerifyQuickPin(pin, u.TenantQuickPinHash ?? u.ResidentQuickPinHash))
                {
                    bool tenantArchivedOnly = await IsRoleArchivedOnlyAsync(u.Id, "Tenant");
                    if (!tenantArchivedOnly) matchedRole = "Tenant";
                }

                if (matchedRole == null && phoneMatchesOwner && VerifyQuickPin(pin, u.OwnerQuickPinHash ?? u.ResidentQuickPinHash))
                {
                    bool ownerArchivedOnly = await IsRoleArchivedOnlyAsync(u.Id, "Owner");
                    if (!ownerArchivedOnly) matchedRole = "Owner";
                }

                if (matchedRole == null) continue;

                bool isTenantLogin = matchedRole == "Tenant";
                string displayName = isTenantLogin
                    ? (u.TenantName ?? $"Daire {u.DoorNumber} Kiracı")
                    : (u.OwnerName ?? $"Daire {u.DoorNumber} Malik");
                string? existingUserId = isTenantLogin ? u.TenantUserId : u.OwnerUserId;
                string? preferredEmail = isTenantLogin ? u.TenantEmail : u.OwnerEmail;
                string fallbackEmail = isTenantLogin
                    ? $"kiraci_{normPhone}@gmk360.local"
                    : $"malik_{normPhone}@gmk360.local";

                var user = await EnsurePortalUserAsync(
                    existingUserId,
                    !string.IsNullOrWhiteSpace(preferredEmail) ? preferredEmail.Trim() : fallbackEmail,
                    displayName,
                    normPhone,
                    "Sakin",
                    "facility_resident");

                if (isTenantLogin && u.TenantUserId != user.Id) u.TenantUserId = user.Id;
                if (!isTenantLogin && u.OwnerUserId != user.Id) u.OwnerUserId = user.Id;
                await _context.SaveChangesAsync();

                return new QuickPinLoginResult
                {
                    Success = true,
                    User = user,
                    RoleKind = matchedRole,
                    Message = $"Hoş geldiniz {displayName} ({(isTenantLogin ? "Kiracı" : "Malik")})."
                };
            }

            return new QuickPinLoginResult { Success = false, Message = "Telefon numarası veya PIN eşleşmedi." };
        }

        private async Task<bool> IsRoleArchivedOnlyAsync(int buildingUnitId, string roleType)
        {
            return await _context.ResidencyPeriods
                .AnyAsync(rp => rp.BuildingUnitId == buildingUnitId && rp.RoleType == roleType && rp.Status == "ARCHIVED")
                && !await _context.ResidencyPeriods
                    .AnyAsync(rp => rp.BuildingUnitId == buildingUnitId && rp.RoleType == roleType && rp.Status == "ACTIVE" && rp.Phone != null);
        }

        private async Task<ApplicationUser> EnsurePortalUserAsync(
            string? existingUserId,
            string fallbackEmail,
            string fullName,
            string phone,
            string roleName,
            string activeModule)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });
            }

            ApplicationUser? user = null;
            if (!string.IsNullOrWhiteSpace(existingUserId))
            {
                user = await _userManager.FindByIdAsync(existingUserId);
            }
            if (user == null && !string.IsNullOrWhiteSpace(fallbackEmail))
            {
                user = await _userManager.FindByEmailAsync(fallbackEmail);
            }
            if (user == null && !string.IsNullOrWhiteSpace(phone))
            {
                var altEmail = $"sakin_{NormalizePhoneLast10(phone)}@gmk360.local";
                if (!string.Equals(altEmail, fallbackEmail, StringComparison.OrdinalIgnoreCase))
                {
                    user = await _userManager.FindByEmailAsync(altEmail);
                }
            }

            if (user == null)
            {
                var names = (fullName ?? "Kullanıcı").Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                user = new ApplicationUser
                {
                    UserName = fallbackEmail,
                    Email = fallbackEmail,
                    EmailConfirmed = true,
                    PhoneNumber = phone,
                    PhoneNumberConfirmed = true,
                    FirstName = names.Length > 0 ? names[0] : "Sakin",
                    LastName = names.Length > 1 ? names[1] : "",
                    UserType = UserType.Individual,
                    ActiveModules = activeModule,
                    IsShadowAccount = false
                };
                await _userManager.CreateAsync(user, $"Gmk!{Guid.NewGuid():N}9a");
            }

            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                await _userManager.AddToRoleAsync(user, roleName);
            }

            return user;
        }

        public async Task<ApplicationUser?> ProvisionResidentProfileAsync(
            string? currentLinkedUserId,
            string? fullName,
            string? email,
            string? phone,
            string roleType,
            string creatorUserId,
            string? tcIdentityNo = null)
        {
            var cleanEmail = email?.Trim();
            var normPhone = NormalizePhoneLast10(phone);
            var cleanTc = GMK360.Web.Helpers.TcknHelper.IsValidTcIdentity(tcIdentityNo)
                ? GMK360.Web.Helpers.TcknHelper.Normalize(tcIdentityNo)
                : null;
            if (string.IsNullOrWhiteSpace(cleanEmail) && normPhone.Length < 10 && string.IsNullOrWhiteSpace(cleanTc))
            {
                return null;
            }

            ApplicationUser? user = null;
            if (!string.IsNullOrWhiteSpace(currentLinkedUserId))
            {
                user = await _userManager.FindByIdAsync(currentLinkedUserId);
            }
            if (user == null && !string.IsNullOrWhiteSpace(cleanTc))
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.TcIdentityNo == cleanTc);
            }
            if (user == null && !string.IsNullOrWhiteSpace(cleanEmail))
            {
                user = await _userManager.FindByEmailAsync(cleanEmail);
            }
            if (user == null && normPhone.Length >= 10)
            {
                var candidates = await _context.Users
                    .Where(u => u.PhoneNumber != null)
                    .ToListAsync();
                user = candidates.FirstOrDefault(u => NormalizePhoneLast10(u.PhoneNumber) == normPhone);
            }

            if (!await _roleManager.RoleExistsAsync("Sakin"))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = "Sakin" });
            }

            if (user == null)
            {
                var names = (fullName ?? "Daire Sakini").Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                var effectiveEmail = !string.IsNullOrWhiteSpace(cleanEmail)
                    ? cleanEmail
                    : $"sakin_{normPhone}@gmk360.local";

                user = new ApplicationUser
                {
                    UserName = effectiveEmail,
                    Email = effectiveEmail,
                    EmailConfirmed = false,
                    PhoneNumber = !string.IsNullOrWhiteSpace(phone) ? phone.Trim() : null,
                    PhoneNumberConfirmed = false,
                    PhoneVerifiedAt = null,
                    FirstName = names.Length > 0 ? names[0] : "Sakin",
                    LastName = names.Length > 1 ? names[1] : "",
                    DisplayName = !string.IsNullOrWhiteSpace(fullName) ? fullName.Trim() : null,
                    TcIdentityNo = cleanTc,
                    UserType = roleType == "Owner" ? UserType.PropertyOwner : UserType.Individual,
                    ActiveModules = "facility_resident",
                    IsShadowAccount = true,
                    MustChangePassword = true,
                    ShadowCreatorId = creatorUserId,
                    ActivationToken = Guid.NewGuid().ToString("N"),
                    ActivationTokenExpiresAt = DateTime.UtcNow.AddDays(14)
                };

                var createRes = await _userManager.CreateAsync(user, $"Gmk!{Guid.NewGuid():N}9a");
                if (!createRes.Succeeded)
                {
                    return null;
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(cleanEmail) && (string.IsNullOrWhiteSpace(user.Email) || user.Email.EndsWith("@gmk360.local", StringComparison.OrdinalIgnoreCase)))
                {
                    var dup = await _userManager.FindByEmailAsync(cleanEmail);
                    if (dup == null || dup.Id == user.Id)
                    {
                        user.Email = cleanEmail;
                        user.UserName = cleanEmail;
                        user.NormalizedEmail = _userManager.NormalizeEmail(cleanEmail);
                        user.NormalizedUserName = _userManager.NormalizeName(cleanEmail);
                        user.EmailConfirmed = false;
                    }
                }
                if (!string.IsNullOrWhiteSpace(phone) && string.IsNullOrWhiteSpace(user.PhoneNumber))
                {
                    user.PhoneNumber = phone.Trim();
                }
                if (!string.IsNullOrWhiteSpace(cleanTc) && string.IsNullOrWhiteSpace(user.TcIdentityNo))
                {
                    user.TcIdentityNo = cleanTc;
                }
                if ((user.IsShadowAccount || !user.IsTransactionPinSet || user.MustChangePassword) && string.IsNullOrWhiteSpace(user.ActivationToken))
                {
                    user.ActivationToken = Guid.NewGuid().ToString("N");
                    user.ActivationTokenExpiresAt = DateTime.UtcNow.AddDays(14);
                }
                if (string.IsNullOrWhiteSpace(user.ActiveModules))
                {
                    user.ActiveModules = "facility_resident";
                }
                await _userManager.UpdateAsync(user);
            }

            if (!await _userManager.IsInRoleAsync(user, "Sakin"))
            {
                await _userManager.AddToRoleAsync(user, "Sakin");
            }

            return user;
        }

        public static string GenerateTempPassword()
        {
            var num = RandomNumberGenerator.GetInt32(1000, 9999);
            const string letters = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            var c1 = letters[RandomNumberGenerator.GetInt32(letters.Length)];
            var c2 = char.ToLowerInvariant(letters[RandomNumberGenerator.GetInt32(letters.Length)]);
            return $"Gmk!{num}{c1}{c2}";
        }

        public async Task<(ApplicationUser? User, string? TempPassword, string? ErrorMessage)> ProvisionStaffAccountAsync(
            BuildingStaff staff,
            string? creatorUserId,
            bool resetTempPassword = true)
        {
            if (!await _roleManager.RoleExistsAsync("Personel"))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = "Personel" });
            }

            var cleanEmail = staff.Email?.Trim();
            var normPhone = NormalizePhoneLast10(staff.PhoneNumber);
            var cleanTc = GMK360.Web.Helpers.TcknHelper.IsValidTcIdentity(staff.TcIdentityNo)
                ? GMK360.Web.Helpers.TcknHelper.Normalize(staff.TcIdentityNo)
                : null;

            ApplicationUser? user = null;
            if (!string.IsNullOrWhiteSpace(staff.UserId))
            {
                user = await _userManager.FindByIdAsync(staff.UserId);
            }
            if (user == null && !string.IsNullOrWhiteSpace(cleanTc))
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.TcIdentityNo == cleanTc);
            }
            if (user == null && !string.IsNullOrWhiteSpace(cleanEmail))
            {
                user = await _userManager.FindByEmailAsync(cleanEmail);
            }
            if (user == null && normPhone.Length >= 10)
            {
                var candidates = await _context.Users.Where(u => u.PhoneNumber != null).ToListAsync();
                user = candidates.FirstOrDefault(u => NormalizePhoneLast10(u.PhoneNumber) == normPhone);
            }

            string? issuedTempPassword = null;

            if (user == null)
            {
                var names = (staff.FullName ?? "Tesis Personeli").Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                var effectiveEmail = !string.IsNullOrWhiteSpace(cleanEmail)
                    ? cleanEmail
                    : $"staff_{(!string.IsNullOrWhiteSpace(normPhone) ? normPhone : staff.Id.ToString())}_{staff.Id}@gmk360.local";

                if (await _userManager.FindByEmailAsync(effectiveEmail) != null)
                {
                    effectiveEmail = $"staff_{staff.Id}_{Guid.NewGuid():N}".Substring(0, 22) + "@gmk360.local";
                }

                issuedTempPassword = GenerateTempPassword();
                user = new ApplicationUser
                {
                    UserName = effectiveEmail,
                    Email = effectiveEmail,
                    EmailConfirmed = true,
                    PhoneNumber = !string.IsNullOrWhiteSpace(staff.PhoneNumber) ? staff.PhoneNumber.Trim() : null,
                    PhoneNumberConfirmed = false,
                    FirstName = names.Length > 0 ? names[0] : "Personel",
                    LastName = names.Length > 1 ? names[1] : "",
                    DisplayName = staff.FullName?.Trim(),
                    TcIdentityNo = cleanTc,
                    Address = staff.Address?.Trim(),
                    UserType = UserType.Individual,
                    ActiveModules = "facility_staff",
                    IsShadowAccount = true,
                    MustChangePassword = true,
                    ShadowCreatorId = creatorUserId,
                    ActivationToken = Guid.NewGuid().ToString("N"),
                    ActivationTokenExpiresAt = DateTime.UtcNow.AddDays(14)
                };

                var createRes = await _userManager.CreateAsync(user, issuedTempPassword);
                if (!createRes.Succeeded)
                {
                    return (null, null, string.Join(" ", createRes.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(cleanEmail) && (string.IsNullOrWhiteSpace(user.Email) || user.Email.EndsWith("@gmk360.local", StringComparison.OrdinalIgnoreCase)))
                {
                    var dup = await _userManager.FindByEmailAsync(cleanEmail);
                    if (dup == null || dup.Id == user.Id)
                    {
                        await _userManager.SetEmailAsync(user, cleanEmail);
                        await _userManager.SetUserNameAsync(user, cleanEmail);
                        user.EmailConfirmed = true;
                    }
                }
                if (!string.IsNullOrWhiteSpace(staff.PhoneNumber) && string.IsNullOrWhiteSpace(user.PhoneNumber))
                {
                    user.PhoneNumber = staff.PhoneNumber.Trim();
                }
                if (!string.IsNullOrWhiteSpace(cleanTc) && string.IsNullOrWhiteSpace(user.TcIdentityNo))
                {
                    user.TcIdentityNo = cleanTc;
                }
                if (string.IsNullOrWhiteSpace(user.ActiveModules))
                {
                    user.ActiveModules = "facility_staff";
                }
                else if (!user.ActiveModules.Contains("facility_staff", StringComparison.OrdinalIgnoreCase))
                {
                    user.ActiveModules = $"{user.ActiveModules},facility_staff";
                }

                if (resetTempPassword && (!user.IsTransactionPinSet || user.MustChangePassword || user.IsShadowAccount))
                {
                    issuedTempPassword = GenerateTempPassword();
                    await _userManager.RemovePasswordAsync(user);
                    await _userManager.AddPasswordAsync(user, issuedTempPassword);
                    user.MustChangePassword = true;
                }

                if (user.MustChangePassword || !user.IsTransactionPinSet || string.IsNullOrWhiteSpace(user.Address))
                {
                    user.ActivationToken = Guid.NewGuid().ToString("N");
                    user.ActivationTokenExpiresAt = DateTime.UtcNow.AddDays(14);
                }

                await _userManager.UpdateAsync(user);
            }

            if (!await _userManager.IsInRoleAsync(user, "Personel"))
            {
                await _userManager.AddToRoleAsync(user, "Personel");
            }

            staff.UserId = user.Id;
            if (!string.IsNullOrWhiteSpace(user.Address) && string.IsNullOrWhiteSpace(staff.Address))
            {
                staff.Address = user.Address;
            }
            if (user.IsTransactionPinSet && !string.IsNullOrWhiteSpace(user.TransactionPinHash))
            {
                staff.QuickPinHash = user.TransactionPinHash;
            }
            if (!string.IsNullOrWhiteSpace(user.Email) && !user.Email.EndsWith("@gmk360.local", StringComparison.OrdinalIgnoreCase))
            {
                staff.Email = user.Email;
            }
            await _context.SaveChangesAsync();

            return (user, issuedTempPassword, null);
        }

        public async Task SyncStaffProfileFromUserAsync(ApplicationUser user)
        {
            if (user == null) return;
            var staffRows = await _context.BuildingStaffs
                .Where(s => s.UserId == user.Id && !s.IsDeleted)
                .ToListAsync();
            if (!staffRows.Any()) return;

            foreach (var s in staffRows)
            {
                if (user.IsTransactionPinSet && !string.IsNullOrWhiteSpace(user.TransactionPinHash))
                {
                    s.QuickPinHash = user.TransactionPinHash;
                }
                if (!string.IsNullOrWhiteSpace(user.Address))
                {
                    s.Address = user.Address.Trim();
                }
                if (!string.IsNullOrWhiteSpace(user.Email) && !user.Email.EndsWith("@gmk360.local", StringComparison.OrdinalIgnoreCase))
                {
                    s.Email = user.Email.Trim();
                }
                if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
                {
                    s.PhoneNumber = user.PhoneNumber.Trim();
                }
                if (!string.IsNullOrWhiteSpace(user.TcIdentityNo) && string.IsNullOrWhiteSpace(s.TcIdentityNo))
                {
                    s.TcIdentityNo = user.TcIdentityNo;
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
