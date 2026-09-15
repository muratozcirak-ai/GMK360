import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\SubcontractorContractController.cs'

content = '''using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Identity;
using GMK360.Core.Entities.Finance;
using GMK360.Core.Entities.Construction;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class SubcontractorContractController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubcontractorContractController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetCurrentAgencyId()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            var consultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == user.Id);
            return consultant?.AgencyId;
        }

        public async Task<IActionResult> ProjectContracts(int id)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var contracts = await _context.SubcontractorContracts
                .Include(c => c.PhonebookContact)
                .Include(c => c.Project)
                .Where(c => c.AgencyId == agencyId && c.ProjectId == id && c.IsActive && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.ProjectId = id;
            return View(contracts);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var contract = await _context.SubcontractorContracts
                .Include(c => c.PhonebookContact)
                .Include(c => c.Project)
                .FirstOrDefaultAsync(c => c.Id == id && c.AgencyId == agencyId);

            if (contract == null) return NotFound();

            var hakedisler = await _context.SubcontractorHakedisler
                .Where(h => h.ContractId == id)
                .OrderByDescending(h => h.HakedisNo)
                .ToListAsync();

            ViewBag.Hakedisler = hakedisler;
            var totalClaim = hakedisler.Sum(h => h.ClaimAmount);
            var totalDeduction = hakedisler.Sum(h => h.DeductionAmount);
            ViewBag.TotalPaid = totalClaim - totalDeduction;
            ViewBag.TotalApproved = totalClaim - totalDeduction;

            ViewBag.Templates = await _context.DocumentTemplates.Where(t => t.AgencyId == agencyId.Value && !t.IsDeleted).ToListAsync();

            return View(contract);
        }

        [HttpPost]
        public async Task<IActionResult> AddHakedis(int contractId, decimal claimAmount, decimal deductionAmount, string deductionReason, string description)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var contract = await _context.SubcontractorContracts
                .Include(c => c.Hakedisler)
                .FirstOrDefaultAsync(c => c.Id == contractId && c.AgencyId == agencyId.Value);

            if (contract == null) return NotFound();

            var pastClaimsTotal = contract.Hakedisler.Sum(h => h.ClaimAmount);
            if (pastClaimsTotal + claimAmount > contract.TotalAmount)
            {
                TempData["ErrorMessage"] = $"Dikkat: Bu hakediş ile sözleşme bedeli aşılmaktadır! Maksimum eklenebilir brüt tutar: {(contract.TotalAmount - pastClaimsTotal).ToString("N2")}";
                return RedirectToAction("Index");
            }

            var nextNo = contract.Hakedisler.Any() ? contract.Hakedisler.Max(h => h.HakedisNo) + 1 : 1;

            var hakedis = new SubcontractorHakedis
            {
                ContractId = contract.Id,
                HakedisNo = nextNo,
                HakedisDate = DateTime.Now,
                Description = description,
                ClaimAmount = claimAmount,
                DeductionAmount = deductionAmount,
                DeductionReason = deductionReason,
                IsApproved = true // Otomatik onaylı varsayıyoruz
            };

            _context.SubcontractorHakedisler.Add(hakedis);
            
            // Cari hesaba yansit
            var contactId = contract.PhonebookContactId;
            var currentAccount = await _context.SupplierCurrentAccounts
                .FirstOrDefaultAsync(a => a.AgencyId == agencyId && a.PhonebookContactId == contactId);

            if (currentAccount == null)
            {
                currentAccount = new SupplierCurrentAccount
                {
                    AgencyId = agencyId.Value,
                    PhonebookContactId = contactId,
                    CurrentBalance = 0
                };
                _context.SupplierCurrentAccounts.Add(currentAccount);
                await _context.SaveChangesAsync();
            }

            decimal net = claimAmount - deductionAmount;
            currentAccount.CurrentBalance += net;

            var transaction = new SupplierAccountTransaction
            {
                SupplierCurrentAccountId = currentAccount.Id,
                TransactionDate = DateTime.Now,
                Type = SupplierTransactionType.PurchaseInvoice, 
                Amount = net,
                BalanceAfterTransaction = currentAccount.CurrentBalance,
                Description = $"{nextNo}. Hakediş: {contract.Title}",
                CreatedByUserId = _userManager.GetUserId(User) ?? ""
            };
            
            _context.SupplierAccountTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{nextNo}. Hakediş başarıyla eklendi.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var contracts = await _context.SubcontractorContracts
                .Include(c => c.Project)
                .Include(c => c.PhonebookContact)
                .Include(c => c.Hakedisler)
                .Where(c => c.AgencyId == agencyId && c.IsActive && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(contracts);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            ViewBag.Projects = await _context.ConstructionProjects
                .Where(p => p.AgencyId == agencyId && !p.IsDeleted)
                .ToListAsync();

            ViewBag.PhonebookContacts = await _context.B2BNetworkContacts
                .Where(s => s.OwnerAgencyId == agencyId && !s.IsDeleted)
                .ToListAsync();

            return View(new SubcontractorContract { ContractDate = DateTime.UtcNow, Currency = "TRY", IsExpenseContract = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubcontractorContract model)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            ModelState.Remove("Agency");
            ModelState.Remove("Project");
            ModelState.Remove("PhonebookContact");
            ModelState.Remove("Phases");
            ModelState.Remove("Hakedisler");

            if (ModelState.IsValid)
            {
                model.AgencyId = agencyId.Value;
                model.CreatedAt = DateTime.UtcNow;
                
                _context.SubcontractorContracts.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Projects = await _context.ConstructionProjects
                .Where(p => p.AgencyId == agencyId && !p.IsDeleted)
                .ToListAsync();

            ViewBag.PhonebookContacts = await _context.B2BNetworkContacts
                .Where(s => s.OwnerAgencyId == agencyId && !s.IsDeleted)
                .ToListAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> PrintContract(int contractId, int templateId)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var contract = await _context.SubcontractorContracts
                .Include(c => c.Project)
                .Include(c => c.PhonebookContact)
                .FirstOrDefaultAsync(c => c.Id == contractId && c.AgencyId == agencyId.Value);
            
            if (contract == null) return NotFound("Sözleşme bulunamadı.");

            var template = await _context.DocumentTemplates
                .FirstOrDefaultAsync(t => t.Id == templateId && t.AgencyId == agencyId.Value);

            if (template == null) return NotFound("Şablon bulunamadı.");

            // Replace variables
            string finalHtml = template.HtmlContent ?? "";
            
            string companyName = contract.PhonebookContact?.Name ?? "_______________";
            string projectName = contract.Project?.Name ?? "_______________";
            string totalAmount = contract.TotalAmount.ToString("N2") + " " + contract.Currency;
            string jobDesc = contract.Description ?? "_______________";
            string contractDate = contract.ContractDate.ToString("dd.MM.yyyy");

            finalHtml = finalHtml.Replace("{{FirmaAdi}}", companyName)
                                 .Replace("{{ProjeAdi}}", projectName)
                                 .Replace("{{Tutar}}", totalAmount)
                                 .Replace("{{IsTanimi}}", jobDesc)
                                 .Replace("{{Tarih}}", contractDate);

            ViewBag.PrintContent = finalHtml;
            ViewBag.Title = $"{companyName} - {template.TemplateName}";
            
            return View("PrintPreview", contract);
        }
    }
}
'''

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
