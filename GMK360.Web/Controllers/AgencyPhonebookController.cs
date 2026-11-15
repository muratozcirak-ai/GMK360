using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Construction;
using GMK360.Core.Entities.Finance;
using GMK360.Core.Entities.Identity;

namespace GMK360.Web.Controllers
{
    [Authorize(Roles = "InsaatFirmasi,Corporate,Admin")]
    public class AgencyPhonebookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GMK360.Web.Services.B2BSupplierSyncService _supplierSyncService;

        public AgencyPhonebookController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, GMK360.Web.Services.B2BSupplierSyncService supplierSyncService)
        {
            _context = context;
            _userManager = userManager;
            _supplierSyncService = supplierSyncService;
        }

        private async Task<int?> GetUserAgencyIdAsync()
        {
            var userId = _userManager.GetUserId(User);
            var consultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == userId);
            return consultant?.AgencyId;
        }

        public async Task<IActionResult> Index(byte? type)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var currentUserId = _userManager.GetUserId(User) ?? "";
            await _supplierSyncService.AutoSyncExistingNetworkContactsAsync(agencyId.Value, currentUserId);

            var query = _context.AgencyPhonebooks.Where(p => p.AgencyId == agencyId && !p.IsDeleted);
            
            if (type.HasValue)
            {
                query = query.Where(p => p.ContactType == type.Value);
                ViewBag.ActiveFilter = type.Value;
            }
            else
            {
                ViewBag.ActiveFilter = 0;
            }

            var contacts = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

            // Altın Kural: B2BNetworkConnections (Pazar Yeri Bağlantıları) üzerinden gelen firmaları da listeye dahil et
            var b2bConnections = await _context.B2BNetworkConnections
                .Include(c => c.B2bCompany)
                .Where(c => c.AgencyId == agencyId && c.IsActive && !c.IsDeleted)
                .ToListAsync();

            foreach (var conn in b2bConnections)
            {
                // B2bCompany nesnesini AgencyPhonebook nesnesi gibi (sanal olarak) View'a gönderiyoruz
                if (conn.B2bCompany != null)
                {
                    byte contactType = 2; // Default: Taşeron Firma
                    if (conn.B2bCompany.IsEngineering) contactType = 8; // Test Laboratuvarı / OSGB
                    else if (conn.B2bCompany.IsSupplier) contactType = 3; // Tedarikçi
                    else if (conn.B2bCompany.IsSubcontractor) contactType = 1; // Usta
                    
                    // Sadece seçili filtreye uygun olanları ve listede mükerrer olmayanları ekle
                    if ((!type.HasValue || type.Value == contactType || type.Value == 0) &&
                        !contacts.Any(c => c.Name.Equals(conn.B2bCompany.Name, StringComparison.OrdinalIgnoreCase) || c.Name.Equals(conn.B2bCompany.Name + " (B2B)", StringComparison.OrdinalIgnoreCase)))
                    {
                        contacts.Add(new AgencyPhonebook
                        {
                            Id = -conn.B2bCompany.Id, // Negatif ID veriyoruz ki Pazar Yeri kaydı olduğu anlaşılsın
                            AgencyId = agencyId.Value,
                            Name = conn.B2bCompany.Name + " (B2B)",
                            ContactType = contactType,
                            PhoneNumber = "B2B Pazar Yeri",
                            Notes = "Pazar Yeri (B2B) üzerinden bağlandı. " + conn.ConnectionNotes
                        });
                    }
                }
            }

            // Her firmanın cari özetini ve sözleşme geçmişini yükle (Çalıştık / Çalışıyoruz / Henüz Çalışmadık ayrımı için)
            var accounts = await _context.SupplierCurrentAccounts
                .Include(a => a.Transactions)
                .Include(a => a.Payments)
                .Where(a => a.AgencyId == agencyId && !a.IsDeleted)
                .ToListAsync();

            var contracts = await _context.SubcontractorContracts
                .Include(c => c.Project)
                .Where(c => c.AgencyId == agencyId && !c.IsDeleted)
                .ToListAsync();

            ViewBag.AccountsByContactId = accounts
                .GroupBy(a => a.PhonebookContactId)
                .ToDictionary(g => g.Key, g => g.First());

            ViewBag.ContractsByContactId = contracts
                .GroupBy(c => c.PhonebookContactId)
                .ToDictionary(g => g.Key, g => g.ToList());

            return View(contacts.OrderBy(c => c.Name).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            AgencyPhonebook? contact = null;

            // Negatif ID ise B2B Pazar Yeri kaydını gerçek rehber kaydına dönüştür veya bul
            if (id < 0)
            {
                int b2bId = Math.Abs(id);
                var conn = await _context.B2BNetworkConnections
                    .Include(c => c.B2bCompany)
                    .FirstOrDefaultAsync(c => c.B2bCompanyId == b2bId && c.AgencyId == agencyId);
                var b2bCompany = conn?.B2bCompany;
                if (b2bCompany != null)
                {
                    contact = await _context.AgencyPhonebooks
                        .Include(c => c.Branches)
                        .Include(c => c.Contacts)
                        .FirstOrDefaultAsync(c => c.AgencyId == agencyId && !c.IsDeleted && c.Name.Contains(b2bCompany.Name));

                    if (contact == null)
                    {
                        byte contactType = 2;
                        if (b2bCompany.IsEngineering) contactType = 8;
                        else if (b2bCompany.IsSupplier) contactType = 3;
                        else if (b2bCompany.IsSubcontractor) contactType = 1;

                        contact = new AgencyPhonebook
                        {
                            AgencyId = agencyId.Value,
                            Name = b2bCompany.Name,
                            ContactType = contactType,
                            PhoneNumber = "B2B Pazar Yeri",
                            Notes = "B2B Pazar Yeri üzerinden rehber arşivine eklendi.",
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.AgencyPhonebooks.Add(contact);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            else
            {
                contact = await _context.AgencyPhonebooks
                    .Include(c => c.Branches)
                    .Include(c => c.Contacts)
                    .FirstOrDefaultAsync(c => c.Id == id && c.AgencyId == agencyId && !c.IsDeleted);
            }

            if (contact == null) return NotFound();

            // Bu firmaya ait cari hesap yoksa otomatik oluştur ki hemen alacak/verecek takip edilebilsin
            var account = await _context.SupplierCurrentAccounts
                .Include(a => a.Transactions)
                    .ThenInclude(t => t.Project)
                .Include(a => a.Payments)
                .FirstOrDefaultAsync(a => a.AgencyId == agencyId && a.PhonebookContactId == contact.Id && !a.IsDeleted);

            if (account == null)
            {
                account = new SupplierCurrentAccount
                {
                    AgencyId = agencyId.Value,
                    PhonebookContactId = contact.Id,
                    CurrentBalance = 0,
                    CreatedAt = DateTime.UtcNow
                };
                _context.SupplierCurrentAccounts.Add(account);
                await _context.SaveChangesAsync();
            }

            var contracts = await _context.SubcontractorContracts
                .Include(c => c.Project)
                .Include(c => c.Hakedisler)
                .Where(c => c.AgencyId == agencyId && c.PhonebookContactId == contact.Id && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var workers = await _context.AgencyWorkers
                .Where(w => w.AgencyId == agencyId && w.SubcontractorContactId == contact.Id && !w.IsDeleted)
                .OrderBy(w => w.FirstName)
                .ToListAsync();

            var projects = await _context.ConstructionProjects
                .Where(p => p.AgencyId == agencyId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.Account = account;
            ViewBag.Contracts = contracts;
            ViewBag.Workers = workers;
            ViewBag.Projects = projects;

            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AgencyPhonebook model)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            model.AgencyId = agencyId.Value;
            model.CreatedAt = System.DateTime.UtcNow;
            ModelState.Remove("Agency");
            ModelState.Remove("Branches");
            ModelState.Remove("Contacts");
            
            if (ModelState.IsValid)
            {
                _context.AgencyPhonebooks.Add(model);
                await _context.SaveChangesAsync();

                // Otomatik cari kartını da hazırla
                var existsAcc = await _context.SupplierCurrentAccounts.AnyAsync(a => a.AgencyId == agencyId && a.PhonebookContactId == model.Id);
                if (!existsAcc)
                {
                    _context.SupplierCurrentAccounts.Add(new SupplierCurrentAccount
                    {
                        AgencyId = agencyId.Value,
                        PhonebookContactId = model.Id,
                        CurrentBalance = 0
                    });
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Firma / Ekip rehber arşivine eklendi ve 360° Cari Dashboardu açıldı.";
            }
            else
            {
                TempData["ErrorMessage"] = "Lütfen zorunlu alanları doldurun.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContact(
            int id,
            string name,
            byte contactType,
            string phoneNumber,
            string? mobilePhone2,
            string? landlinePhone,
            string? email,
            string? authorizedPerson,
            string? authorizedPersonRole,
            string? taxOffice,
            string? taxNumber,
            string? iban,
            string? city,
            string? district,
            string? address,
            string? websiteUrl,
            string? tags,
            string? notes)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var contact = await _context.AgencyPhonebooks.FirstOrDefaultAsync(c => c.Id == id && c.AgencyId == agencyId);
            if (contact == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(name)) contact.Name = name.Trim();
            contact.ContactType = contactType;
            if (!string.IsNullOrWhiteSpace(phoneNumber)) contact.PhoneNumber = phoneNumber.Trim();
            contact.MobilePhone2 = mobilePhone2?.Trim();
            contact.LandlinePhone = landlinePhone?.Trim();
            contact.Email = email?.Trim();
            contact.AuthorizedPerson = authorizedPerson?.Trim();
            contact.AuthorizedPersonRole = authorizedPersonRole?.Trim();
            contact.TaxOffice = taxOffice?.Trim();
            contact.TaxNumber = taxNumber?.Trim();
            contact.Iban = iban?.Trim();
            contact.City = city?.Trim();
            contact.District = district?.Trim();
            contact.Address = address?.Trim();
            contact.WebsiteUrl = websiteUrl?.Trim();
            contact.Tags = tags?.Trim();
            contact.Notes = notes?.Trim();

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Firma künyesi, adres, vergi ve kurumsal hafıza notları güncellendi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuickLedgerEntry(
            int contactId,
            string entryKind, // "debt" (Alım/Hakediş/Fatura -> Borcumuz Artar) veya "payment" (Ödeme Çıkışı -> Borcumuz Düşer)
            decimal amount,
            int? projectId,
            string? paymentMethod,
            DateTime? dueDate,
            string? referenceNo,
            string description)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var contact = await _context.AgencyPhonebooks.FirstOrDefaultAsync(c => c.Id == contactId && c.AgencyId == agencyId);
            if (contact == null) return NotFound();

            var account = await _context.SupplierCurrentAccounts
                .FirstOrDefaultAsync(a => a.AgencyId == agencyId && a.PhonebookContactId == contactId && !a.IsDeleted);

            if (account == null)
            {
                account = new SupplierCurrentAccount
                {
                    AgencyId = agencyId.Value,
                    PhonebookContactId = contactId,
                    CurrentBalance = 0
                };
                _context.SupplierCurrentAccounts.Add(account);
                await _context.SaveChangesAsync();
            }

            if (entryKind == "payment")
            {
                account.CurrentBalance -= amount;
                var parsedMethod = Enum.TryParse<PaymentMethod>(paymentMethod, out var pm) ? pm : PaymentMethod.BankTransfer;

                var payment = new SupplierPayment
                {
                    SupplierCurrentAccountId = account.Id,
                    Amount = amount,
                    Method = parsedMethod,
                    DueDate = dueDate,
                    CheckNumber = referenceNo,
                    Notes = description,
                    PaymentDate = DateTime.UtcNow,
                    Status = dueDate.HasValue && dueDate.Value.Date > DateTime.UtcNow.Date ? PaymentStatus.Pending : PaymentStatus.Completed
                };
                _context.SupplierPayments.Add(payment);

                var tx = new SupplierAccountTransaction
                {
                    SupplierCurrentAccountId = account.Id,
                    ProjectId = projectId,
                    Amount = amount,
                    Type = SupplierTransactionType.PaymentMade,
                    DocumentReference = referenceNo,
                    Description = description + (dueDate.HasValue ? $" (Vade: {dueDate.Value:dd.MM.yyyy})" : ""),
                    TransactionDate = DateTime.UtcNow,
                    BalanceAfterTransaction = account.CurrentBalance,
                    CreatedByUserId = _userManager.GetUserId(User) ?? ""
                };
                _context.SupplierAccountTransactions.Add(tx);
                TempData["SuccessMessage"] = $"₺{amount:N2} tutarındaki ödeme firma carisine işlendi.";
            }
            else
            {
                account.CurrentBalance += amount;
                var txType = entryKind == "open" ? SupplierTransactionType.OpenAccountPurchase : SupplierTransactionType.PurchaseInvoice;

                var tx = new SupplierAccountTransaction
                {
                    SupplierCurrentAccountId = account.Id,
                    ProjectId = projectId,
                    Amount = amount,
                    Type = txType,
                    DocumentReference = referenceNo,
                    Description = description,
                    TransactionDate = DateTime.UtcNow,
                    BalanceAfterTransaction = account.CurrentBalance,
                    CreatedByUserId = _userManager.GetUserId(User) ?? ""
                };
                _context.SupplierAccountTransactions.Add(tx);
                TempData["SuccessMessage"] = $"₺{amount:N2} tutarındaki iş/fatura/veresiye kaydı firma carisine işlendi.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = contactId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBranch(int contactId, string branchName, string? city, string? district, string? address)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var contact = await _context.AgencyPhonebooks.FirstOrDefaultAsync(c => c.Id == contactId && c.AgencyId == agencyId);
            if (contact == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(branchName))
            {
                _context.Add(new AgencyPhonebookBranch
                {
                    AgencyPhonebookId = contactId,
                    BranchName = branchName.Trim(),
                    City = city?.Trim(),
                    District = district?.Trim(),
                    Address = address?.Trim()
                });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Şube / Depo / Santral adresi eklendi.";
            }
            return RedirectToAction(nameof(Details), new { id = contactId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddContactPerson(int contactId, string fullName, string? departmentOrRole, string? phoneNumber, string? email)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var contact = await _context.AgencyPhonebooks.FirstOrDefaultAsync(c => c.Id == contactId && c.AgencyId == agencyId);
            if (contact == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                _context.Add(new AgencyPhonebookContact
                {
                    AgencyPhonebookId = contactId,
                    FullName = fullName.Trim(),
                    DepartmentOrRole = departmentOrRole?.Trim(),
                    PhoneNumber = phoneNumber?.Trim(),
                    Email = email?.Trim()
                });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Firma yetkilisi / irtibat kişisi eklendi.";
            }
            return RedirectToAction(nameof(Details), new { id = contactId });
        }

        [HttpPost]
        public async Task<IActionResult> SendInvite(int id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            var contact = await _context.AgencyPhonebooks.FirstOrDefaultAsync(c => c.Id == id && c.AgencyId == agencyId);
            
            if (contact == null) return NotFound();

            TempData["SuccessMessage"] = $"{contact.Name} adlı kişiye sisteme katılım daveti gönderildi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            var contact = await _context.AgencyPhonebooks.FirstOrDefaultAsync(c => c.Id == id && c.AgencyId == agencyId);
            
            if (contact != null)
            {
                _context.AgencyPhonebooks.Remove(contact);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kayıt silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

