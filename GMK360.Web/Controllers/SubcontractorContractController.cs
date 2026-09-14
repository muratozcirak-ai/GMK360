using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Identity;
using GMK360.Core.Entities.Finance;

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

            var consultant = await _context.AgencyConsultants
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.IsActive);
                
            return consultant?.AgencyId;
        }

        [HttpGet]
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

            var payments = await _context.ProgressPayments
                .Where(p => p.SubcontractorContractId == id)
                .OrderByDescending(p => p.RequestDate)
                .ToListAsync();

            ViewBag.Payments = payments;
            ViewBag.TotalPaid = payments.Where(p => p.Status == ProgressPaymentStatus.Paid).Sum(p => p.ApprovedAmount);
            ViewBag.TotalApproved = payments.Where(p => p.Status == ProgressPaymentStatus.Approved || p.Status == ProgressPaymentStatus.Paid).Sum(p => p.ApprovedAmount);

            return View(contract);
        }

        [HttpPost]
        public async Task<IActionResult> AddPayment(int contractId, string title, decimal requestedAmount, string notes)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var contract = await _context.SubcontractorContracts.FirstOrDefaultAsync(c => c.Id == contractId && c.AgencyId == agencyId);
            if (contract == null) return NotFound();

            var payment = new ProgressPayment
            {
                SubcontractorContractId = contractId,
                PaymentTitle = title,
                RequestedAmount = requestedAmount,
                ApprovedAmount = requestedAmount, // By default, suggest the requested amount
                Notes = notes,
                RequestDate = DateTime.UtcNow,
                Status = ProgressPaymentStatus.Draft
            };

            _context.ProgressPayments.Add(payment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yeni Hakedi� / �deme Talebi ba�ar�yla olu�turuldu.";
            return RedirectToAction(nameof(Details), new { id = contractId });
        }


        [HttpPost]
        public async Task<IActionResult> ApprovePayment(int paymentId, decimal approvedAmount, string approvalNotes)
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var payment = await _context.ProgressPayments
                .Include(p => p.Contract)
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.Contract.AgencyId == agencyId);

            if (payment == null || payment.Status != ProgressPaymentStatus.Draft)
                return NotFound();

            payment.ApprovedAmount = approvedAmount;
            payment.Status = ProgressPaymentStatus.Approved;
            if (!string.IsNullOrEmpty(approvalNotes))
            {
                payment.Notes += $"\n[Onay Notu]: {approvalNotes}";
            }

            // Hakedi� onayland���nda, ilgili ta�eronun cari hesab�na 'Bor�' olarak yans�t!
            var contactId = payment.Contract.PhonebookContactId;
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

            // M�teahhit olarak biz bor�lan�yoruz. Bakiye art�yor.
            currentAccount.CurrentBalance += approvedAmount;

            var transaction = new SupplierAccountTransaction
            {
                SupplierCurrentAccountId = currentAccount.Id,
                TransactionDate = DateTime.UtcNow,
                Type = SupplierTransactionType.PurchaseInvoice, // Hakedi� faturas� mahiyetinde
                Amount = approvedAmount,
                BalanceAfterTransaction = currentAccount.CurrentBalance,
                Description = $"Hakedi� Onay�: {payment.Contract.Title} - {payment.PaymentTitle}",
                DocumentReference = $"Hakedi� #{payment.Id}",
                CreatedByUserId = _userManager.GetUserId(User) ?? ""
            };
            
            _context.SupplierAccountTransactions.Add(transaction);
            
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{approvedAmount:N2} TL'lik Hakedi� onayland� ve Cari Hesaba aktar�ld�.";
            return RedirectToAction(nameof(Details), new { id = payment.SubcontractorContractId });
        }
        public async Task<IActionResult> Index()
        {
            var agencyId = await GetCurrentAgencyId();
            if (agencyId == null) return Unauthorized();

            var contracts = await _context.SubcontractorContracts
                .Include(c => c.Project)
                .Include(c => c.PhonebookContact)
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

            // Sadece gerekli alanları validate etmek için clear navigation properties
            ModelState.Remove("Agency");
            ModelState.Remove("Project");
            ModelState.Remove("PhonebookContact");
            ModelState.Remove("Phases");
            ModelState.Remove("ProgressPayments");

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
    }
}






