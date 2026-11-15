using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using GMK360.Core.DTOs.Construction;
using GMK360.Core.Entities.Construction;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Finance;
using GMK360.Core.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class FinanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FinanceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetUserAgencyIdAsync()
        {
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser != null)
            {
                var myConsultant = await _context.AgencyConsultants.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == loggedInUser.Id && c.IsActive);
                if (myConsultant != null && myConsultant.AgencyId > 0)
                    return myConsultant.AgencyId;
            }
            var agencyClaim = User.FindFirst("AgencyId")?.Value;
            if (int.TryParse(agencyClaim, out int parsedAgencyId) && parsedAgencyId > 0)
                return parsedAgencyId;

            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
            {
                var firstProjAgency = await _context.ConstructionProjects
                    .Where(p => !p.IsDeleted && p.AgencyId > 0)
                    .Select(p => (int?)p.AgencyId)
                    .FirstOrDefaultAsync();
                return firstProjAgency ?? 1;
            }

            // (kullanıcı ajans kontrolü)
                
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            var consultant = await _context.AgencyConsultants
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.IsActive);
                
            var fallbackProjAgency = await _context.ConstructionProjects
                .Where(p => !p.IsDeleted && p.AgencyId > 0)
                .Select(p => (int?)p.AgencyId)
                .FirstOrDefaultAsync();
            return consultant?.AgencyId ?? fallbackProjAgency ?? 1;
        }

        // Finans & Cari Master Paneli
        public async Task<IActionResult> Index(int? projectId = null, string? monthFilter = null, string? tab = "expected")
        {
            if (!await GMK360.Web.Helpers.StaffPermissionHelper.HasUserPermissionAsync(_context, User, Request, GMK360.Web.Helpers.StaffPermissionHelper.PermFinance))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var projects = await _context.ConstructionProjects
                .Where(p => p.AgencyId == agencyId.Value && !p.IsDeleted)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            var latestRate = await _context.ExchangeRates
                .OrderByDescending(r => r.Date)
                .FirstOrDefaultAsync();
            decimal usdRate = (latestRate?.UsdRate > 0) ? latestRate.UsdRate : 49.6540m;
            decimal eurRate = (latestRate?.EurRate > 0) ? latestRate.EurRate : 55.5997m;

            var allExpectedOutflows = new List<ProjectExpectedTransactionDto>();
            var allActualOutflows = new List<ProjectAccountTransactionDto>();
            var projectNames = projects.ToDictionary(p => p.Id, p => p.Name);

            var targetProjects = projectId.HasValue
                ? projects.Where(p => p.Id == projectId.Value).ToList()
                : projects;

            foreach (var proj in targetProjects)
            {
                var projDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "projects", proj.Id.ToString());

                var expectedPath = Path.Combine(projDir, "expected_transactions.json");
                if (System.IO.File.Exists(expectedPath))
                {
                    try
                    {
                        var json = await System.IO.File.ReadAllTextAsync(expectedPath);
                        var list = JsonSerializer.Deserialize<List<ProjectExpectedTransactionDto>>(json) ?? new List<ProjectExpectedTransactionDto>();
                        foreach (var item in list.Where(x => x.Direction == "Outflow" && !x.IsCompleted))
                        {
                            item.ProjectId = proj.Id;
                            allExpectedOutflows.Add(item);
                        }
                    }
                    catch { }
                }

                var actualPath = Path.Combine(projDir, "account_transactions.json");
                if (System.IO.File.Exists(actualPath))
                {
                    try
                    {
                        var json = await System.IO.File.ReadAllTextAsync(actualPath);
                        var list = JsonSerializer.Deserialize<List<ProjectAccountTransactionDto>>(json) ?? new List<ProjectAccountTransactionDto>();
                        foreach (var item in list.Where(x => x.TransactionType == "Cikis"))
                        {
                            item.ProjectId = proj.Id;
                            allActualOutflows.Add(item);
                        }
                    }
                    catch { }
                }
            }

            // Firma İçi Cari Hesaplar (Taşeron & Tedarikçi), Kasa/Banka ve Garaj Hareketlerini de Firma Finans Havuzuna Dahil Et
            var currentAgency = await _context.Agencies.AsNoTracking().FirstOrDefaultAsync(a => a.Id == agencyId.Value);
            ViewBag.CompanyName = currentAgency?.CompanyName ?? "İnşaat Firmamız";

            var supplierAccounts = await _context.SupplierCurrentAccounts
                .AsNoTracking()
                .Include(a => a.PhonebookContact)
                .Include(a => a.Transactions)
                .Include(a => a.Payments)
                .Where(a => a.AgencyId == agencyId.Value && !a.IsDeleted)
                .OrderByDescending(a => a.CurrentBalance)
                .ToListAsync();

            foreach (var acc in supplierAccounts)
            {
                string supName = acc.PhonebookContact?.Name ?? $"Cari #{acc.Id}";
                string? supIban = acc.PhonebookContact?.Iban;

                if (acc.Payments != null)
                {
                    foreach (var p in acc.Payments.Where(x => x.Status == PaymentStatus.Pending))
                    {
                        if (!projectId.HasValue)
                        {
                            allExpectedOutflows.Add(new ProjectExpectedTransactionDto
                            {
                                Id = "sup_pay_" + p.Id,
                                ProjectId = 0,
                                Direction = "Outflow",
                                PartyName = supName,
                                RecipientIban = supIban,
                                Date1 = p.DueDate ?? p.PaymentDate,
                                AccountType = p.Method == PaymentMethod.Cash ? "Elden" : "Resmi",
                                TargetAccountName = p.Method == PaymentMethod.Check ? $"Çek ({p.CheckNumber})" : "Firma Banka / Kasa",
                                Amount = p.Amount,
                                Currency = "TRY",
                                CategoryOrPurpose = "Vadeli Çek / Tedarikçi Ödemesi",
                                Description = p.Notes
                            });
                        }
                    }
                }

                if (acc.Transactions != null)
                {
                    foreach (var tx in acc.Transactions.Where(t => t.Type == SupplierTransactionType.PaymentMade))
                    {
                        if (!projectId.HasValue || tx.ProjectId == projectId.Value)
                        {
                            allActualOutflows.Add(new ProjectAccountTransactionDto
                            {
                                Id = "sup_tx_" + tx.Id,
                                ProjectId = tx.ProjectId ?? 0,
                                AccountName = "Firma Cari / Banka-Kasa",
                                TransactionType = "Cikis",
                                Amount = tx.Amount,
                                AmountTL = tx.Amount,
                                Currency = "TRY",
                                AccountKind = "Resmi",
                                TransactionDate = tx.TransactionDate,
                                CategoryOrPurpose = "Taşeron / Tedarikçi Cari Ödemesi",
                                PartyName = supName,
                                Description = tx.Description
                            });
                        }
                    }
                }
            }

            if (!projectId.HasValue)
            {
                var companyCashTxs = await _context.Set<AgencyCashTransaction>()
                    .AsNoTracking()
                    .Include(c => c.SupplierCurrentAccount).ThenInclude(s => s.PhonebookContact)
                    .Include(c => c.AgencyWorker)
                    .Include(c => c.AgencyConsultant).ThenInclude(ac => ac.User)
                    .Where(c => c.AgencyId == agencyId.Value)
                    .OrderByDescending(c => c.PaymentDate)
                    .ToListAsync();

                foreach (var c in companyCashTxs)
                {
                    string party = c.SupplierCurrentAccount?.PhonebookContact?.Name
                                   ?? (c.AgencyWorker != null ? $"{c.AgencyWorker.FirstName} {c.AgencyWorker.LastName} (Usta)" : null)
                                   ?? (c.AgencyConsultant?.User != null ? $"{c.AgencyConsultant.User.FirstName} {c.AgencyConsultant.User.LastName} (Personel)" : "Firma İçi Genel Gider");

                    allActualOutflows.Add(new ProjectAccountTransactionDto
                    {
                        Id = "cash_" + c.Id,
                        ProjectId = 0,
                        AccountName = c.Method == PaymentMethod.Cash ? "Firma Nakit Kasa" : "Firma Banka Hesabı",
                        TransactionType = "Cikis",
                        Amount = c.Amount,
                        AmountTL = c.Amount,
                        Currency = "TRY",
                        AccountKind = c.Method == PaymentMethod.Cash ? "Elden" : "Resmi",
                        TransactionDate = c.PaymentDate,
                        CategoryOrPurpose = c.TransactionType == AgencyCashTransactionType.WorkerAdvance ? "Usta Avans / Yevmiye" :
                                            c.TransactionType == AgencyCashTransactionType.ConsultantAdvance ? "Personel Maaş / Avans" :
                                            c.TransactionType == AgencyCashTransactionType.SupplierPayment ? "Tedarikçi Ödemesi" : "Firma İçi Genel Gider",
                        PartyName = party,
                        Description = c.Description
                    });
                }
            }

            decimal ToTL(decimal amount, string? currency)
            {
                if (string.Equals(currency, "USD", StringComparison.OrdinalIgnoreCase)) return Math.Round(amount * usdRate, 2);
                if (string.Equals(currency, "EUR", StringComparison.OrdinalIgnoreCase)) return Math.Round(amount * eurRate, 2);
                return amount;
            }

            var today = DateTime.Today;
            var nextMonthDate = today.AddMonths(1);
            var trCulture = new CultureInfo("tr-TR");

            decimal thisMonthExpectedTL = allExpectedOutflows
                .Where(x => (x.Date1.Year == today.Year && x.Date1.Month == today.Month) || x.Date1.Date < new DateTime(today.Year, today.Month, 1))
                .Sum(x => ToTL(x.Amount, x.Currency));

            int thisMonthExpectedCount = allExpectedOutflows
                .Count(x => (x.Date1.Year == today.Year && x.Date1.Month == today.Month) || x.Date1.Date < new DateTime(today.Year, today.Month, 1));

            decimal nextMonthExpectedTL = allExpectedOutflows
                .Where(x => x.Date1.Year == nextMonthDate.Year && x.Date1.Month == nextMonthDate.Month)
                .Sum(x => ToTL(x.Amount, x.Currency));

            int nextMonthExpectedCount = allExpectedOutflows
                .Count(x => x.Date1.Year == nextMonthDate.Year && x.Date1.Month == nextMonthDate.Month);

            decimal totalExpectedOutflowTL = allExpectedOutflows.Sum(x => ToTL(x.Amount, x.Currency));
            decimal totalActualOutflowTL = allActualOutflows.Sum(x => x.AmountTL ?? ToTL(x.Amount, x.Currency));

            var monthlyBreakdown = allExpectedOutflows
                .GroupBy(x => x.Date1.ToString("yyyy-MM"))
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var dt = DateTime.ParseExact(g.Key + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    return (Key: g.Key, Label: dt.ToString("MMMM yyyy", trCulture), Count: g.Count(), TotalTL: g.Sum(x => ToTL(x.Amount, x.Currency)));
                })
                .ToList();

            var filteredExpected = allExpectedOutflows.AsEnumerable();
            var filteredActual = allActualOutflows.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(monthFilter))
            {
                if (monthFilter == "this_month")
                {
                    filteredExpected = filteredExpected.Where(x => (x.Date1.Year == today.Year && x.Date1.Month == today.Month) || x.Date1.Date < new DateTime(today.Year, today.Month, 1));
                    filteredActual = filteredActual.Where(x => x.TransactionDate.Year == today.Year && x.TransactionDate.Month == today.Month);
                }
                else if (monthFilter == "next_month")
                {
                    filteredExpected = filteredExpected.Where(x => x.Date1.Year == nextMonthDate.Year && x.Date1.Month == nextMonthDate.Month);
                    filteredActual = filteredActual.Where(x => x.TransactionDate.Year == nextMonthDate.Year && x.TransactionDate.Month == nextMonthDate.Month);
                }
                else if (monthFilter.Length == 7 && monthFilter[4] == '-')
                {
                    filteredExpected = filteredExpected.Where(x => x.Date1.ToString("yyyy-MM") == monthFilter);
                    filteredActual = filteredActual.Where(x => x.TransactionDate.ToString("yyyy-MM") == monthFilter);
                }
            }

            ViewBag.Projects = projects;
            ViewBag.ProjectNames = projectNames;
            ViewBag.SelectedProjectId = projectId;
            ViewBag.SelectedMonthFilter = monthFilter ?? "";
            ViewBag.ActiveTab = string.IsNullOrWhiteSpace(tab) ? "expected" : tab;
            ViewBag.UsdRate = usdRate;
            ViewBag.EurRate = eurRate;

            ViewBag.ThisMonthExpectedTL = thisMonthExpectedTL;
            ViewBag.ThisMonthExpectedCount = thisMonthExpectedCount;
            ViewBag.NextMonthExpectedTL = nextMonthExpectedTL;
            ViewBag.NextMonthExpectedCount = nextMonthExpectedCount;
            ViewBag.TotalExpectedOutflowTL = totalExpectedOutflowTL;
            ViewBag.TotalExpectedCount = allExpectedOutflows.Count;
            ViewBag.TotalActualOutflowTL = totalActualOutflowTL;
            ViewBag.TotalActualCount = allActualOutflows.Count;
            ViewBag.MonthlyBreakdown = monthlyBreakdown;

            ViewBag.ExpectedOutflows = filteredExpected.OrderBy(x => x.Date1).ToList();
            ViewBag.ActualOutflows = filteredActual.OrderByDescending(x => x.TransactionDate).ThenByDescending(x => x.CreatedAt).ToList();

            // 360° Firma İçi ERP/SRM/CRM Canlı Bilanço Özeti (Taşeron Carileri, Personel/Usta Yevmiye & Avans, Araç Filo Giderleri, Daire Satış Gelirleri)
            ViewBag.SupplierAccounts = supplierAccounts;
            ViewBag.TotalSupplierDebt = supplierAccounts.Sum(a => a.CurrentBalance);
            ViewBag.SupplierAccountCount = supplierAccounts.Count;

            var workerTimesheets = await _context.Set<DailyTimesheet>()
                .AsNoTracking()
                .Where(t => t.AgencyId == agencyId.Value && !t.IsDeleted)
                .ToListAsync();
            decimal workerEarned = workerTimesheets.Sum(t => t.EarnedWage);
            decimal workerPaid = workerTimesheets.Sum(t => t.AdvancePayment);
            ViewBag.TotalWorkerNetPayable = workerEarned - workerPaid;
            ViewBag.TotalWorkerPaid = workerPaid;

            var fleetExpenses = await _context.VehicleExpenses
                .AsNoTracking()
                .Where(v => v.AgencyId == agencyId.Value)
                .SumAsync(v => (decimal?)v.Amount) ?? 0m;
            ViewBag.TotalFleetExpenses = fleetExpenses;

            var projIds = projects.Select(p => p.Id).ToList();
            var soldUnitsTotal = await _context.BuildingUnits
                .AsNoTracking()
                .Where(u => u.Building.ConstructionProjectId.HasValue && projIds.Contains(u.Building.ConstructionProjectId.Value) && u.SalesStatus == "Satıldı")
                .SumAsync(u => (decimal?)u.ListPrice) ?? 0m;
            ViewBag.TotalSoldUnitsRevenue = soldUnitsTotal;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddInHouseFinanceEntry(string entryType, int? supplierAccountId, int? projectId, decimal amount, PaymentMethod method, DateTime? dueDate, string? referenceNo, string description)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            if (amount <= 0)
            {
                TempData["ErrorMessage"] = "Lütfen sıfırdan büyük bir tutar giriniz.";
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User) ?? "";

            if ((entryType == "supplier_debt" || entryType == "supplier_payment") && supplierAccountId.HasValue && supplierAccountId.Value > 0)
            {
                var acc = await _context.SupplierCurrentAccounts
                    .Include(a => a.PhonebookContact)
                    .FirstOrDefaultAsync(a => a.Id == supplierAccountId.Value && a.AgencyId == agencyId.Value);

                if (acc != null)
                {
                    if (entryType == "supplier_debt")
                    {
                        acc.CurrentBalance += amount;
                        _context.SupplierAccountTransactions.Add(new SupplierAccountTransaction
                        {
                            SupplierCurrentAccountId = acc.Id,
                            ProjectId = projectId,
                            Amount = amount,
                            Type = SupplierTransactionType.PurchaseInvoice,
                            DocumentReference = referenceNo,
                            Description = description,
                            TransactionDate = DateTime.UtcNow,
                            BalanceAfterTransaction = acc.CurrentBalance,
                            CreatedByUserId = userId
                        });
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = $"{acc.PhonebookContact?.Name} firma carisine ₺{amount:N2} alım/fatura borcu işlendi.";
                        return RedirectToAction(nameof(Index), new { tab = "cariler" });
                    }
                    else
                    {
                        acc.CurrentBalance -= amount;
                        _context.SupplierPayments.Add(new SupplierPayment
                        {
                            SupplierCurrentAccountId = acc.Id,
                            Amount = amount,
                            Method = method,
                            DueDate = dueDate,
                            CheckNumber = referenceNo,
                            Notes = description,
                            PaymentDate = DateTime.UtcNow,
                            Status = (dueDate.HasValue && dueDate.Value.Date > DateTime.Today) ? PaymentStatus.Pending : PaymentStatus.Completed,
                            HandledByUserId = userId
                        });
                        _context.SupplierAccountTransactions.Add(new SupplierAccountTransaction
                        {
                            SupplierCurrentAccountId = acc.Id,
                            ProjectId = projectId,
                            Amount = amount,
                            Type = SupplierTransactionType.PaymentMade,
                            DocumentReference = referenceNo,
                            Description = description + (dueDate.HasValue ? $" (Vade: {dueDate.Value:dd.MM.yyyy})" : ""),
                            TransactionDate = DateTime.UtcNow,
                            BalanceAfterTransaction = acc.CurrentBalance,
                            CreatedByUserId = userId
                        });
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = $"{acc.PhonebookContact?.Name} carisine ₺{amount:N2} ödeme kaydedildi ve bakiyeden düşüldü.";
                        return RedirectToAction(nameof(Index), new { tab = "actual" });
                    }
                }
            }

            // Genel Firma İçi Gider (Kira, Muhasebe, Vergi, Ofis, Yakıt vb.)
            _context.Set<AgencyCashTransaction>().Add(new AgencyCashTransaction
            {
                AgencyId = agencyId.Value,
                TransactionType = AgencyCashTransactionType.GeneralExpense,
                Amount = amount,
                Description = description,
                Method = method,
                PaymentDate = DateTime.UtcNow,
                Status = PaymentStatus.Completed,
                HandledByUserId = userId
            });
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Firma içi gider (₺{amount:N2}) kasa/banka hareketlerine işlendi.";
            return RedirectToAction(nameof(Index), new { tab = "actual" });
        }

        [NonAction]
        public async Task<IActionResult> OldIndexUnused()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            return View();
        }

        // Ortak Kasa ve Banka Akışı
        public async Task<IActionResult> Cashflow()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var cashTransactions = await _context.Set<AgencyCashTransaction>()
                .Include(c => c.SupplierCurrentAccount).ThenInclude(s => s.PhonebookContact)
                .Include(c => c.AgencyWorker)
                .Include(c => c.AgencyConsultant).ThenInclude(ac => ac.User)
                .Where(c => c.AgencyId == agencyId.Value)
                .OrderByDescending(c => c.PaymentDate)
                .ToListAsync();

            // Hesaplamalar
            ViewBag.TotalCashOut = cashTransactions.Where(c => c.TransactionType == AgencyCashTransactionType.WorkerAdvance || 
                                                               c.TransactionType == AgencyCashTransactionType.ConsultantAdvance ||
                                                               c.TransactionType == AgencyCashTransactionType.SupplierPayment ||
                                                               c.TransactionType == AgencyCashTransactionType.GeneralExpense)
                                                   .Sum(c => c.Amount); // Varsayılan olarak hep çıkış kabul ediyoruz basitlik için, ama eğer gelir varsa çıkartılır.

            return View(cashTransactions);
        }

        [HttpPost]
        public async Task<IActionResult> AddExpense(decimal amount, string description, PaymentMethod method)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var expense = new AgencyCashTransaction
            {
                AgencyId = agencyId.Value,
                TransactionType = AgencyCashTransactionType.GeneralExpense,
                Amount = amount,
                Description = description,
                Method = method,
                PaymentDate = DateTime.UtcNow,
                Status = PaymentStatus.Completed,
                HandledByUserId = _userManager.GetUserId(User)
            };

            _context.Set<AgencyCashTransaction>().Add(expense);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Gider kasaya işlendi.";
            return RedirectToAction(nameof(Cashflow));
        }

        // Projeye Bağlı Olmayan Şirket Genel Giderleri & Araç/Makine Özeti
        public async Task<IActionResult> CompanyExpenses()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var vehicles = await _context.CompanyVehicles
                .Include(v => v.Expenses)
                .Where(v => v.AgencyId == agencyId.Value && v.Status == "Aktif")
                .ToListAsync();

            decimal totalFleetExpense = vehicles.Sum(v => v.Expenses?.Sum(e => e.Amount) ?? 0m);

            var rentalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "garage", $"rental_machinery_{agencyId.Value}.json");
            var rentalLogs = new List<RentalMachineryLogItem>();
            if (System.IO.File.Exists(rentalPath))
            {
                try
                {
                    var json = await System.IO.File.ReadAllTextAsync(rentalPath);
                    rentalLogs = JsonSerializer.Deserialize<List<RentalMachineryLogItem>>(json) ?? new List<RentalMachineryLogItem>();
                }
                catch { }
            }

            ViewBag.VehicleCount = vehicles.Count;
            ViewBag.TotalFleetExpense = totalFleetExpense;
            ViewBag.RentalCount = rentalLogs.Count;
            ViewBag.TotalRentalExpense = rentalLogs.Sum(x => x.TotalAmount);

            return View();
        }

        public async Task<IActionResult> UpcomingPayments()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            // Sadece vadesi gelmiş ve henüz ödenmemiş tedarikçi ödemeleri (çek/senet vb.)
            var payments = await _context.SupplierPayments
                .Include(p => p.SupplierCurrentAccount).ThenInclude(s => s.PhonebookContact)
                .Where(p => p.SupplierCurrentAccount.AgencyId == agencyId.Value && p.Status == PaymentStatus.Pending)
                .OrderBy(p => p.DueDate)
                .ToListAsync();

            return View(payments);
        }
    }
}
