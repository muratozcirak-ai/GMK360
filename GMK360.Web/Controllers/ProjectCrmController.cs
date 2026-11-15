using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using GMK360.Core.Entities.Identity;

namespace GMK360.Web.Controllers
{
    public class CrmCustomerExtendedMeta
    {
        public string IdentityNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public string District { get; set; } = "";
        public int? BuildingUnitId { get; set; }
        public string PurchasedUnitSummary { get; set; } = "";
        public decimal ContractPrice { get; set; }
        public decimal DownPayment { get; set; }
        public string TitleDeedStatus { get; set; } = "Kat İrtifakı / Sözleşme Aşamasında";
        public string TurnkeyDeliveryStatus { get; set; } = "İnşaat Devam Ediyor";
        public string SnagListNotes { get; set; } = "";
        public string GeneralNotes { get; set; } = "";
        public List<CrmInstallmentItem> Installments { get; set; } = new List<CrmInstallmentItem>();
    }

    public class CrmInstallmentItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public int InstallmentNo { get; set; }
        public string Title { get; set; } = "";
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
        public string PaymentMethod { get; set; } = "Banka Havale/EFT";
    }

    [Authorize]
    public class ProjectCrmController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectCrmController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetUserAgencyIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            var agencyConsultant = await _context.AgencyConsultants
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserId == user.Id && !a.IsDeleted);

            if (agencyConsultant != null && agencyConsultant.AgencyId > 0)
                return agencyConsultant.AgencyId;

            var agencyClaim = User.FindFirst("AgencyId")?.Value;
            if (int.TryParse(agencyClaim, out int parsedAgencyId) && parsedAgencyId > 0)
                return parsedAgencyId;

            if (User.IsInRole("Admin"))
            {
                var defaultAgency = await _context.Agencies.OrderBy(a => a.Id).FirstOrDefaultAsync();
                return defaultAgency?.Id ?? 1;
            }

            var fallbackProjAgency = await _context.ConstructionProjects.Where(p => !p.IsDeleted && p.AgencyId > 0).Select(p => (int?)p.AgencyId).FirstOrDefaultAsync();
            return fallbackProjAgency ?? 1;
        }

        public static CrmCustomerExtendedMeta ParseCrmMeta(string? rawNotes)
        {
            if (string.IsNullOrWhiteSpace(rawNotes))
                return new CrmCustomerExtendedMeta();
            if (rawNotes.TrimStart().StartsWith("{"))
            {
                try
                {
                    return JsonSerializer.Deserialize<CrmCustomerExtendedMeta>(rawNotes) ?? new CrmCustomerExtendedMeta();
                }
                catch { }
            }
            return new CrmCustomerExtendedMeta { GeneralNotes = rawNotes };
        }

        public async Task<IActionResult> SalesPool()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var units = await _context.BuildingUnits
                .Include(u => u.Building)
                    .ThenInclude(b => b.ConstructionProject)
                .Include(u => u.Building)
                    .ThenInclude(b => b.ParentBuilding)
                .Where(u => u.Building.ConstructionProject.AgencyId == agencyId && !u.Building.ConstructionProject.IsDeleted)
                .OrderByDescending(u => u.Building.ConstructionProject.Id)
                .ThenBy(u => u.Building.Id)
                .ThenBy(u => u.FloorLevel)
                .ThenBy(u => u.DoorNumber)
                .ToListAsync();

            var agencyUserIds = await _context.AgencyConsultants
                .AsNoTracking()
                .Where(c => c.AgencyId == agencyId.Value && !c.IsDeleted)
                .Select(c => c.UserId)
                .ToListAsync();
            var currentUserId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(currentUserId) && !agencyUserIds.Contains(currentUserId))
                agencyUserIds.Add(currentUserId);

            ViewBag.CrmContactCount = await _context.CrmContacts.CountAsync(c => agencyUserIds.Contains(c.ApplicationUserId) && !c.IsDeleted);
            ViewBag.Projects = await _context.ConstructionProjects.Where(p => p.AgencyId == agencyId.Value && !p.IsDeleted).OrderByDescending(p => p.Id).ToListAsync();
            return View(units);
        }

        public async Task<IActionResult> Index()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var agencyUserIds = await _context.AgencyConsultants
                .AsNoTracking()
                .Where(c => c.AgencyId == agencyId.Value && !c.IsDeleted)
                .Select(c => c.UserId)
                .ToListAsync();
            var currentUserId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(currentUserId) && !agencyUserIds.Contains(currentUserId))
                agencyUserIds.Add(currentUserId);

            var contacts = await _context.CrmContacts
                .Where(c => agencyUserIds.Contains(c.ApplicationUserId) && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var metaDict = new Dictionary<int, CrmCustomerExtendedMeta>();
            foreach (var c in contacts)
            {
                metaDict[c.Id] = ParseCrmMeta(c.Notes);
            }

            ViewBag.CrmMeta = metaDict;
            ViewBag.AvailableUnitCount = await _context.BuildingUnits
                .CountAsync(u => u.Building.ConstructionProject.AgencyId == agencyId.Value && !u.Building.ConstructionProject.IsDeleted && string.IsNullOrEmpty(u.OwnerName));

            var availableUnits = await _context.BuildingUnits
                .Include(u => u.Building).ThenInclude(b => b.ConstructionProject)
                .Where(u => u.Building.ConstructionProject.AgencyId == agencyId.Value && !u.Building.ConstructionProject.IsDeleted)
                .OrderBy(u => u.Building.ConstructionProject.Name)
                .ThenBy(u => u.DoorNumber)
                .ToListAsync();
            ViewBag.AllUnits = availableUnits;

            return View(contacts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string FirstName,
            string LastName,
            string PhoneNumber,
            string? Email,
            string ContactType,
            string? IdentityNumber,
            string? City,
            string? District,
            string? Address,
            int? BuildingUnitId,
            decimal ContractPrice = 0,
            decimal DownPayment = 0,
            int InstallmentCount = 0,
            string? Notes = null)
        {
            var userId = _userManager.GetUserId(User) ?? "";
            string unitSummary = "";

            if (BuildingUnitId.HasValue && BuildingUnitId.Value > 0)
            {
                var unit = await _context.BuildingUnits
                    .Include(u => u.Building).ThenInclude(b => b.ConstructionProject)
                    .FirstOrDefaultAsync(u => u.Id == BuildingUnitId.Value);
                if (unit != null)
                {
                    unitSummary = $"{unit.Building?.ConstructionProject?.Name} - {unit.Building?.BlockName} Kat:{unit.FloorLevel} Daire:{unit.DoorNumber}";
                    if (ContactType == "Daire Sahibi")
                    {
                        unit.OwnerName = $"{FirstName} {LastName}".Trim();
                        unit.OwnerPhone = PhoneNumber;
                        unit.SalesStatus = "Satıldı";
                        if (ContractPrice > 0) unit.ListPrice = ContractPrice;
                    }
                    else if (ContactType == "Opsiyonlu")
                    {
                        unit.SalesStatus = "Opsiyonlu";
                        if (ContractPrice > 0) unit.ListPrice = ContractPrice;
                    }
                }
            }

            var meta = new CrmCustomerExtendedMeta
            {
                IdentityNumber = IdentityNumber ?? "",
                City = City ?? "",
                District = District ?? "",
                Address = Address ?? "",
                BuildingUnitId = BuildingUnitId,
                PurchasedUnitSummary = unitSummary,
                ContractPrice = ContractPrice,
                DownPayment = DownPayment,
                GeneralNotes = Notes ?? ""
            };

            if (DownPayment > 0)
            {
                meta.Installments.Add(new CrmInstallmentItem
                {
                    InstallmentNo = 0,
                    Title = "Peşinat / Kapora Tahsilatı",
                    DueDate = DateTime.Today,
                    Amount = DownPayment,
                    IsPaid = true,
                    PaidAt = DateTime.Today,
                    PaymentMethod = "Banka / Nakit"
                });
            }

            decimal remaining = ContractPrice - DownPayment;
            if (remaining > 0 && InstallmentCount > 0)
            {
                decimal perInst = Math.Round(remaining / InstallmentCount, 2);
                for (int i = 1; i <= InstallmentCount; i++)
                {
                    meta.Installments.Add(new CrmInstallmentItem
                    {
                        InstallmentNo = i,
                        Title = $"{i}. Taksit / Senet",
                        DueDate = DateTime.Today.AddMonths(i),
                        Amount = i == InstallmentCount ? (remaining - (perInst * (InstallmentCount - 1))) : perInst,
                        IsPaid = false
                    });
                }
            }

            var contact = new CrmContact
            {
                ApplicationUserId = userId,
                FirstName = FirstName ?? "",
                LastName = LastName ?? "",
                PhoneNumber = PhoneNumber ?? "",
                Email = Email ?? "",
                ContactType = ContactType ?? "Potansiyel Alici",
                Notes = JsonSerializer.Serialize(meta)
            };

            _context.CrmContacts.Add(contact);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{contact.FirstName} {contact.LastName} için 360° Müşteri, Sözleşme & Senet/Taksit dosyası başarıyla oluşturuldu.";
            return RedirectToAction(nameof(CustomerDetails), new { id = contact.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SellOrReserveUnit(
            int unitId,
            string actionType,
            string buyerFirstName,
            string buyerLastName,
            string buyerPhone,
            string? buyerIdentityNo,
            string? buyerAddress,
            decimal salePrice,
            decimal downPayment,
            int installmentCount,
            string? salesNotes)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var unit = await _context.BuildingUnits
                .Include(u => u.Building).ThenInclude(b => b.ConstructionProject)
                .FirstOrDefaultAsync(u => u.Id == unitId && u.Building.ConstructionProject.AgencyId == agencyId.Value);

            if (unit == null)
                return RedirectToAction(nameof(SalesPool));

            unit.ListPrice = salePrice > 0 ? salePrice : unit.ListPrice;
            if (actionType == "Satis")
            {
                unit.SalesStatus = "Satıldı";
                unit.OwnerName = $"{buyerFirstName} {buyerLastName}".Trim();
                unit.OwnerPhone = buyerPhone;
            }
            else
            {
                unit.SalesStatus = "Opsiyonlu";
            }

            var unitSummary = $"{unit.Building?.ConstructionProject?.Name} - {unit.Building?.BlockName} Kat:{unit.FloorLevel} No:{unit.DoorNumber}";
            var meta = new CrmCustomerExtendedMeta
            {
                IdentityNumber = buyerIdentityNo ?? "",
                Address = buyerAddress ?? "",
                BuildingUnitId = unit.Id,
                PurchasedUnitSummary = unitSummary,
                ContractPrice = salePrice,
                DownPayment = downPayment,
                GeneralNotes = salesNotes ?? ""
            };

            if (downPayment > 0)
            {
                meta.Installments.Add(new CrmInstallmentItem
                {
                    InstallmentNo = 0,
                    Title = actionType == "Satis" ? "Satış Peşinatı" : "Opsiyon Kaporası",
                    DueDate = DateTime.Today,
                    Amount = downPayment,
                    IsPaid = true,
                    PaidAt = DateTime.Today,
                    PaymentMethod = "Banka / Nakit"
                });
            }

            decimal remaining = salePrice - downPayment;
            if (remaining > 0 && installmentCount > 0)
            {
                decimal perInst = Math.Round(remaining / installmentCount, 2);
                for (int i = 1; i <= installmentCount; i++)
                {
                    meta.Installments.Add(new CrmInstallmentItem
                    {
                        InstallmentNo = i,
                        Title = $"{i}. Senet / Taksit",
                        DueDate = DateTime.Today.AddMonths(i),
                        Amount = i == installmentCount ? (remaining - (perInst * (installmentCount - 1))) : perInst,
                        IsPaid = false
                    });
                }
            }

            var contact = new CrmContact
            {
                ApplicationUserId = _userManager.GetUserId(User) ?? "",
                FirstName = buyerFirstName ?? "",
                LastName = buyerLastName ?? "",
                PhoneNumber = buyerPhone ?? "",
                ContactType = actionType == "Satis" ? "Daire Sahibi" : "Opsiyonlu",
                Notes = JsonSerializer.Serialize(meta)
            };

            _context.CrmContacts.Add(contact);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{unitSummary} bağımsız bölümü {contact.FirstName} {contact.LastName} adına {(actionType == "Satis" ? "SATILDI" : "OPSİYONLANDI")} ve müşteri taksit/senet cari dosyası açıldı.";
            return RedirectToAction(nameof(CustomerDetails), new { id = contact.Id });
        }

        [HttpGet]
        public async Task<IActionResult> CustomerDetails(int id)
        {
            var contact = await _context.CrmContacts.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (contact == null)
                return RedirectToAction(nameof(Index));

            var meta = ParseCrmMeta(contact.Notes);
            ViewBag.Meta = meta;
            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkInstallmentPaid(int contactId, string installmentId, string paymentMethod)
        {
            var contact = await _context.CrmContacts.FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);
            if (contact == null) return RedirectToAction(nameof(Index));

            var meta = ParseCrmMeta(contact.Notes);
            var inst = meta.Installments.FirstOrDefault(x => x.Id == installmentId);
            if (inst != null)
            {
                inst.IsPaid = true;
                inst.PaidAt = DateTime.Today;
                inst.PaymentMethod = paymentMethod ?? "Banka EFT";
                contact.Notes = JsonSerializer.Serialize(meta);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{inst.Title} (₺{inst.Amount:N2}) tahsilatı müşteri carisine işlendi.";
            }

            return RedirectToAction(nameof(CustomerDetails), new { id = contactId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCustomInstallment(int contactId, string title, DateTime dueDate, decimal amount, bool isPaid)
        {
            var contact = await _context.CrmContacts.FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);
            if (contact == null) return RedirectToAction(nameof(Index));

            var meta = ParseCrmMeta(contact.Notes);
            int nextNo = meta.Installments.Any() ? meta.Installments.Max(x => x.InstallmentNo) + 1 : 1;
            meta.Installments.Add(new CrmInstallmentItem
            {
                InstallmentNo = nextNo,
                Title = string.IsNullOrWhiteSpace(title) ? $"{nextNo}. Senet / Ara Ödeme" : title,
                DueDate = dueDate,
                Amount = amount,
                IsPaid = isPaid,
                PaidAt = isPaid ? DateTime.Today : null
            });

            contact.Notes = JsonSerializer.Serialize(meta);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Yeni senet / taksit kalemi müşteri vade planına eklendi.";
            return RedirectToAction(nameof(CustomerDetails), new { id = contactId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCustomerDossier(
            int contactId,
            string firstName,
            string lastName,
            string phoneNumber,
            string? email,
            string contactType,
            string? identityNumber,
            string? city,
            string? district,
            string? address,
            string? titleDeedStatus,
            string? turnkeyDeliveryStatus,
            string? snagListNotes,
            string? generalNotes)
        {
            var contact = await _context.CrmContacts.FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted);
            if (contact == null) return RedirectToAction(nameof(Index));

            var meta = ParseCrmMeta(contact.Notes);
            contact.FirstName = firstName ?? contact.FirstName;
            contact.LastName = lastName ?? contact.LastName;
            contact.PhoneNumber = phoneNumber ?? contact.PhoneNumber;
            contact.Email = email ?? contact.Email;
            contact.ContactType = contactType ?? contact.ContactType;

            meta.IdentityNumber = identityNumber ?? "";
            meta.City = city ?? "";
            meta.District = district ?? "";
            meta.Address = address ?? "";
            meta.TitleDeedStatus = titleDeedStatus ?? meta.TitleDeedStatus;
            meta.TurnkeyDeliveryStatus = turnkeyDeliveryStatus ?? meta.TurnkeyDeliveryStatus;
            meta.SnagListNotes = snagListNotes ?? "";
            meta.GeneralNotes = generalNotes ?? "";

            contact.Notes = JsonSerializer.Serialize(meta);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Müşteri kimlik, adres, tapu devri ve anahtar teslim tutanağı bilgileri güncellendi.";
            return RedirectToAction(nameof(CustomerDetails), new { id = contactId });
        }
    }
}
