using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using GMK360.Core.Entities.Finance;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class SupplierCurrentAccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GMK360.Web.Services.B2BSupplierSyncService _supplierSyncService;

        public SupplierCurrentAccountController(ApplicationDbContext context, GMK360.Web.Services.B2BSupplierSyncService supplierSyncService)
        {
            _context = context;
            _supplierSyncService = supplierSyncService;
        }

        private async Task<int?> GetCurrentAgencyIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var consultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == userId);
                if (consultant?.AgencyId != null) return consultant.AgencyId;
            }
            var claim = User.FindFirst("AgencyId")?.Value;
            if (int.TryParse(claim, out int parsedAgencyId) && parsedAgencyId > 0) return parsedAgencyId;
            var activeProjectAgencyId = await _context.ConstructionProjects
                .Where(p => !p.IsDeleted && p.AgencyId > 0)
                .OrderByDescending(p => p.Id)
                .Select(p => (int?)p.AgencyId)
                .FirstOrDefaultAsync();
            if (activeProjectAgencyId.HasValue) return activeProjectAgencyId.Value;
            return await _context.Agencies.OrderBy(a => a.Id).Select(a => (int?)a.Id).FirstOrDefaultAsync();
        }

        // GET: SupplierCurrentAccount
        public async Task<IActionResult> Index()
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            var query = _context.Set<SupplierCurrentAccount>()
                .Include(a => a.PhonebookContact)
                .Include(a => a.Payments)
                .Where(a => !a.IsDeleted);

            if (agencyId.HasValue)
            {
                query = query.Where(a => a.AgencyId == agencyId.Value);
            }

            var accounts = await query.ToListAsync();

            var contracts = agencyId.HasValue
                ? _supplierSyncService.LoadAgencySupplierContracts(agencyId.Value)
                : new System.Collections.Generic.List<GMK360.Web.Services.SupplierContractPackageDto>();
            ViewBag.SupplierContracts = contracts;

            ViewBag.UpcomingPayments = accounts
                .SelectMany(a => a.Payments.Where(p => p.Status == PaymentStatus.Pending && p.DueDate != null).Select(p => new {
                    Payment = p,
                    SupplierName = a.PhonebookContact?.Name ?? "Tedarikçi",
                    DaysLeft = (p.DueDate.Value.Date - DateTime.UtcNow.Date).Days
                }))
                .OrderBy(x => x.Payment.DueDate)
                .ToList();
            
            return View(accounts);
        }

        // GET: SupplierCurrentAccount/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var agencyId = await GetCurrentAgencyIdAsync();

            var account = await _context.Set<SupplierCurrentAccount>()
                .Include(a => a.PhonebookContact)
                .Include(a => a.Transactions)
                    .ThenInclude(t => t.Project)
                .Include(a => a.Payments)
                .FirstOrDefaultAsync(m => m.Id == id && (!agencyId.HasValue || m.AgencyId == agencyId.Value || User.IsInRole("Admin") || User.IsInRole("SuperAdmin")));

            if (account == null) return NotFound();

            var allContracts = _supplierSyncService.LoadAgencySupplierContracts(account.AgencyId);
            var accountContracts = allContracts
                .Where(c => c.SupplierCurrentAccountId == account.Id || c.PhonebookContactId == account.PhonebookContactId)
                .OrderByDescending(c => c.ApprovedAt)
                .ToList();
            ViewBag.AccountContracts = accountContracts;
            ViewBag.Warehouses = await _context.Warehouses.Where(w => w.AgencyId == account.AgencyId).ToListAsync();

            ViewBag.Projects = await _context.Set<GMK360.Core.Entities.Construction.ConstructionProject>()
                .Where(p => !p.IsDeleted && p.AgencyId == account.AgencyId)
                .ToListAsync();
            return View(account);
        }

        // POST: Add Open Account Transaction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOpenAccount(int SupplierCurrentAccountId, decimal Amount, string Description, int? ProjectId)
        {
            var account = await _context.Set<SupplierCurrentAccount>().FindAsync(SupplierCurrentAccountId);
            if (account == null) return NotFound();

            // Borcu artır (Bizim tedarikçiye olan borcumuz)
            account.CurrentBalance += Amount;

            var transaction = new SupplierAccountTransaction
            {
                SupplierCurrentAccountId = SupplierCurrentAccountId,
                Amount = Amount,
                Type = SupplierTransactionType.OpenAccountPurchase,
                Description = Description,
                ProjectId = ProjectId,
                TransactionDate = DateTime.UtcNow,
                BalanceAfterTransaction = account.CurrentBalance
            };

            _context.Add(transaction);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = SupplierCurrentAccountId });
        }

        // POST: Add Payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayment(int SupplierCurrentAccountId, decimal Amount, PaymentMethod Method, DateTime? DueDate, string ReferenceNumber, string BankName, string Description)
        {
            var account = await _context.Set<SupplierCurrentAccount>().FindAsync(SupplierCurrentAccountId);
            if (account == null) return NotFound();

            // Borcu düşür
            account.CurrentBalance -= Amount;

            var payment = new SupplierPayment
            {
                SupplierCurrentAccountId = SupplierCurrentAccountId,
                Amount = Amount,
                Method = Method,
                DueDate = DueDate,
                CheckNumber = ReferenceNumber,
                BankName = BankName,
                Notes = Description,
                PaymentDate = DateTime.UtcNow,
                Status = DueDate.HasValue ? PaymentStatus.Pending : PaymentStatus.Completed
            };

            _context.Add(payment);

            var transaction = new SupplierAccountTransaction
            {
                SupplierCurrentAccountId = SupplierCurrentAccountId,
                Amount = Amount,
                Type = SupplierTransactionType.PaymentMade,
                Description = Description + (DueDate.HasValue ? $" (Vade: {DueDate.Value.ToShortDateString()})" : ""),
                TransactionDate = DateTime.UtcNow,
                BalanceAfterTransaction = account.CurrentBalance
            };
            
            _context.Add(transaction);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = SupplierCurrentAccountId });
        }

        // POST: Anlaşma Şartları & Karma Ödeme Planı Dilimi Kaydet (Faturalı / Değerli Evrak / Elden / İş Bitimi Şartlı)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveContractPaymentSlice(
            int SupplierCurrentAccountId,
            string contractId,
            string? contractTermsNote,
            string? sliceTitle,
            string? paymentChannel,
            decimal sliceAmount = 0,
            DateTime? dueDate = null,
            string? checkOrRefNo = null,
            string? bankName = null,
            string? sliceStatus = "Bekliyor (Planlandı)")
        {
            var account = await _context.Set<SupplierCurrentAccount>()
                .Include(a => a.PhonebookContact)
                .FirstOrDefaultAsync(a => a.Id == SupplierCurrentAccountId);
            if (account == null) return NotFound();

            var contracts = _supplierSyncService.LoadAgencySupplierContracts(account.AgencyId);
            var contract = contracts.FirstOrDefault(c => c.ContractId == contractId);
            if (contract == null) return NotFound();

            if (contractTermsNote != null)
            {
                contract.ContractTermsNote = contractTermsNote.Trim();
            }

            if (sliceAmount > 0 || !string.IsNullOrWhiteSpace(sliceTitle))
            {
                var slice = new GMK360.Web.Services.SupplierPaymentSliceDto
                {
                    SliceId = Guid.NewGuid().ToString("N"),
                    Title = string.IsNullOrWhiteSpace(sliceTitle) ? "Anlaşma Ödeme Dilimi" : sliceTitle.Trim(),
                    PaymentChannel = string.IsNullOrWhiteSpace(paymentChannel) ? "Faturalı (Resmi Banka / Havale)" : paymentChannel.Trim(),
                    Amount = sliceAmount,
                    DueDate = dueDate,
                    CheckOrReferenceNo = (checkOrRefNo ?? "").Trim(),
                    BankName = (bankName ?? "").Trim(),
                    Status = string.IsNullOrWhiteSpace(sliceStatus) ? "Bekliyor (Planlandı)" : sliceStatus.Trim()
                };
                contract.PaymentSlices.Add(slice);

                // Eğer "Ödendi / Evrak Verildi" seçildiyse veya vade tarihi girildiyse Ödeme Takvimine (SupplierPayment) de yansıt
                if (sliceAmount > 0 && (dueDate.HasValue || slice.Status.Contains("Ödendi") || slice.Status.Contains("Verildi")))
                {
                    var method = PaymentMethod.BankTransfer;
                    if (slice.PaymentChannel.Contains("Çek") || slice.PaymentChannel.Contains("Senet") || slice.PaymentChannel.Contains("Evrak"))
                        method = PaymentMethod.Check;
                    else if (slice.PaymentChannel.Contains("Elden") || slice.PaymentChannel.Contains("Nakit"))
                        method = PaymentMethod.Cash;

                    bool isCompletedNow = slice.Status.Contains("Ödendi") && !dueDate.HasValue;

                    var payment = new SupplierPayment
                    {
                        SupplierCurrentAccountId = SupplierCurrentAccountId,
                        Amount = sliceAmount,
                        Method = method,
                        DueDate = dueDate,
                        CheckNumber = slice.CheckOrReferenceNo,
                        BankName = slice.BankName,
                        Notes = $"[{contract.PackageTitle}] {slice.Title} ({slice.PaymentChannel})",
                        PaymentDate = DateTime.UtcNow,
                        Status = isCompletedNow ? PaymentStatus.Completed : PaymentStatus.Pending
                    };
                    _context.Add(payment);

                    if (slice.Status.Contains("Ödendi") || slice.Status.Contains("Verildi"))
                    {
                        account.CurrentBalance -= sliceAmount;
                        _context.Add(new SupplierAccountTransaction
                        {
                            SupplierCurrentAccountId = SupplierCurrentAccountId,
                            Amount = sliceAmount,
                            Type = SupplierTransactionType.PaymentMade,
                            ProjectId = contract.ProjectId,
                            DocumentReference = slice.CheckOrReferenceNo,
                            Description = $"[{slice.PaymentChannel}] {slice.Title} - {contract.PackageTitle}" + (dueDate.HasValue ? $" (Vade: {dueDate.Value:dd.MM.yyyy})" : ""),
                            TransactionDate = DateTime.UtcNow,
                            BalanceAfterTransaction = account.CurrentBalance
                        });
                    }
                    await _context.SaveChangesAsync();
                }
            }

            _supplierSyncService.SaveAgencySupplierContracts(account.AgencyId, contracts);
            TempData["SuccessMessage"] = "🤝 Anlaşma şartları ve ödeme planı dilimi başarıyla kaydedildi!";
            return RedirectToAction(nameof(Details), new { id = SupplierCurrentAccountId });
        }

        // POST: Firma Carisinden Birim Fiyatlarla Doğrudan Sipariş Ver (Sipariş Miktarı x Birim Fiyat -> Cari Borç + Stok Kartı + Kara Defter)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrderFromContract(
            int SupplierCurrentAccountId,
            string contractId,
            int[] itemIndices,
            decimal[] orderQuantities,
            decimal[]? unitPrices = null,
            string paymentChannel = "Faturalı (Resmi Banka / Havale)",
            string orderSettlementMode = "Vadeli",
            DateTime? dueDate = null,
            string? checkOrRefNo = null,
            int? warehouseId = null,
            string? orderNotes = null)
        {
            var account = await _context.Set<SupplierCurrentAccount>()
                .Include(a => a.PhonebookContact)
                .FirstOrDefaultAsync(a => a.Id == SupplierCurrentAccountId);
            if (account == null) return NotFound();

            var contracts = _supplierSyncService.LoadAgencySupplierContracts(account.AgencyId);
            var contract = contracts.FirstOrDefault(c => c.ContractId == contractId);
            if (contract == null) return NotFound();

            if (itemIndices == null || orderQuantities == null || itemIndices.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen sipariş edilecek kalem miktarlarını giriniz.";
                return RedirectToAction(nameof(Details), new { id = SupplierCurrentAccountId });
            }

            decimal orderTotal = 0m;
            var orderedLines = new System.Collections.Generic.List<(GMK360.Web.Services.SupplierContractItemDto Item, decimal Qty, decimal UnitPrice, decimal LineTotal)>();

            for (int k = 0; k < itemIndices.Length && k < orderQuantities.Length; k++)
            {
                int idx = itemIndices[k];
                decimal qty = orderQuantities[k];
                if (idx >= 0 && idx < contract.Items.Count)
                {
                    var cItem = contract.Items[idx];
                    if (unitPrices != null && k < unitPrices.Length && unitPrices[k] > 0)
                    {
                        cItem.AgreedUnitPrice = unitPrices[k];
                    }
                    if (qty > 0)
                    {
                        decimal lineTotal = Math.Round(qty * cItem.AgreedUnitPrice, 2);
                        cItem.OrderedQuantity += qty;
                        orderTotal += lineTotal;
                        orderedLines.Add((cItem, qty, cItem.AgreedUnitPrice, lineTotal));
                    }
                }
            }

            if (orderedLines.Count == 0 || orderTotal <= 0)
            {
                _supplierSyncService.SaveAgencySupplierContracts(account.AgencyId, contracts);
                TempData["ErrorMessage"] = "Sipariş vermek için en az bir kaleme 0'dan büyük sipariş miktarı (ve birim fiyat) giriniz.";
                return RedirectToAction(nameof(Details), new { id = SupplierCurrentAccountId });
            }

            contract.OrderedTotalAmount += orderTotal;
            _supplierSyncService.SaveAgencySupplierContracts(account.AgencyId, contracts);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            // 1. Şantiye Deposu & Stok Kartı Açılışı (Sipariş verilen kalemler depoya işlenir)
            GMK360.Core.Entities.Construction.Warehouse? targetWarehouse = null;
            if (warehouseId.HasValue && warehouseId.Value > 0)
            {
                targetWarehouse = await _context.Warehouses.FindAsync(warehouseId.Value);
            }
            if (targetWarehouse == null && contract.ProjectId.HasValue)
            {
                targetWarehouse = await _context.Warehouses
                    .FirstOrDefaultAsync(w => w.AgencyId == account.AgencyId && w.ConstructionProjectId == contract.ProjectId.Value && !w.IsDeleted);
                if (targetWarehouse == null)
                {
                    var proj = await _context.ConstructionProjects.FindAsync(contract.ProjectId.Value);
                    if (proj != null)
                    {
                        targetWarehouse = new GMK360.Core.Entities.Construction.Warehouse
                        {
                            AgencyId = account.AgencyId,
                            ConstructionProjectId = proj.Id,
                            Name = $"{proj.Name} Şantiyesi Deposu",
                            Type = GMK360.Core.Entities.Construction.WarehouseType.Santiye,
                            Address = proj.Address ?? "",
                            Description = $"{proj.Name} Şantiye sahası malzeme ve yedek stok deposu.",
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Warehouses.Add(targetWarehouse);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            if (targetWarehouse != null)
            {
                foreach (var line in orderedLines)
                {
                    var invItem = await _context.InventoryItems
                        .FirstOrDefaultAsync(i => i.WarehouseId == targetWarehouse.Id && (i.MaterialCatalogId == line.Item.MaterialCatalogId || i.Name == line.Item.MaterialName));
                    if (invItem == null)
                    {
                        invItem = new GMK360.Core.Entities.Construction.InventoryItem
                        {
                            WarehouseId = targetWarehouse.Id,
                            MaterialCatalogId = line.Item.MaterialCatalogId > 0 ? line.Item.MaterialCatalogId : null,
                            Name = line.Item.MaterialName,
                            ItemType = GMK360.Core.Entities.Construction.InventoryItemType.SarfMalzeme,
                            EntryMethod = GMK360.Core.Entities.Construction.InventoryEntryMethod.SatinAlma,
                            Brand = contract.PackageTitle,
                            Model = $"Birim Fiyat: {line.UnitPrice:N2} ₺",
                            SerialNumber = "",
                            SupplierName = account.PhonebookContact?.Name ?? contract.SupplierName,
                            ServiceContact = "Şantiye Deposu",
                            Quantity = line.Qty,
                            Unit = line.Item.Unit,
                            CurrentStatus = "Cariden Sipariş Verildi / Depo Stoğu Açıldı",
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.InventoryItems.Add(invItem);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        invItem.Quantity += line.Qty;
                        invItem.CurrentStatus = "Cariden Sipariş Verildi / Stok Güncellendi";
                        invItem.UpdatedAt = DateTime.UtcNow;
                        _context.InventoryItems.Update(invItem);
                        await _context.SaveChangesAsync();
                    }

                    _context.InventoryTransactions.Add(new GMK360.Core.Entities.Construction.InventoryTransaction
                    {
                        InventoryItemId = invItem.Id,
                        Type = GMK360.Core.Entities.Construction.TransactionType.Giris,
                        Quantity = line.Qty,
                        TransactionDate = DateTime.UtcNow,
                        Description = $"[Cari Sipariş] {line.Item.MaterialName} ({line.Qty:N2} {line.Item.Unit} x {line.UnitPrice:N2} ₺ = {line.LineTotal:N2} ₺) - {account.PhonebookContact?.Name}",
                        HandledByUserId = userId
                    });
                }
            }

            // 2. Cari Ekstreye (SupplierAccountTransaction) ve Ödeme Takvimine (SupplierPayment) Yansıt
            string itemSummary = string.Join(", ", orderedLines.Select(l => $"{l.Item.MaterialName}: {l.Qty:N2} {l.Item.Unit} x {l.UnitPrice:N2} ₺"));
            string docRef = !string.IsNullOrWhiteSpace(checkOrRefNo) ? checkOrRefNo.Trim() : $"SIP-{DateTime.Now:MMddHHmm}";

            account.CurrentBalance += orderTotal;
            _context.Add(new SupplierAccountTransaction
            {
                SupplierCurrentAccountId = SupplierCurrentAccountId,
                Amount = orderTotal,
                Type = paymentChannel.Contains("Elden") ? SupplierTransactionType.OpenAccountPurchase : SupplierTransactionType.PurchaseInvoice,
                ProjectId = contract.ProjectId,
                DocumentReference = docRef,
                Description = $"[Sipariş - {paymentChannel}] {itemSummary}" + (!string.IsNullOrWhiteSpace(orderNotes) ? $" | Not: {orderNotes.Trim()}" : ""),
                TransactionDate = DateTime.UtcNow,
                BalanceAfterTransaction = account.CurrentBalance,
                CreatedByUserId = userId
            });

            bool isPaidNow = string.Equals(orderSettlementMode, "Pesin", StringComparison.OrdinalIgnoreCase);
            var payMethod = PaymentMethod.BankTransfer;
            if (paymentChannel.Contains("Çek") || paymentChannel.Contains("Senet") || paymentChannel.Contains("Evrak"))
                payMethod = PaymentMethod.Check;
            else if (paymentChannel.Contains("Elden") || paymentChannel.Contains("Nakit"))
                payMethod = PaymentMethod.Cash;

            if (isPaidNow)
            {
                account.CurrentBalance -= orderTotal;
                _context.Add(new SupplierAccountTransaction
                {
                    SupplierCurrentAccountId = SupplierCurrentAccountId,
                    Amount = orderTotal,
                    Type = SupplierTransactionType.PaymentMade,
                    ProjectId = contract.ProjectId,
                    DocumentReference = docRef,
                    Description = $"[Sipariş Peşin/Evrak Ödemesi - {paymentChannel}] {contract.PackageTitle}",
                    TransactionDate = DateTime.UtcNow,
                    BalanceAfterTransaction = account.CurrentBalance,
                    CreatedByUserId = userId
                });

                _context.Add(new SupplierPayment
                {
                    SupplierCurrentAccountId = SupplierCurrentAccountId,
                    Amount = orderTotal,
                    Method = payMethod,
                    DueDate = dueDate,
                    CheckNumber = docRef,
                    Notes = $"[Sipariş Ödemesi - {paymentChannel}] {itemSummary}",
                    PaymentDate = DateTime.UtcNow,
                    Status = (payMethod == PaymentMethod.Check && dueDate.HasValue) ? PaymentStatus.Pending : PaymentStatus.Completed
                });
            }
            else
            {
                var effectiveDue = dueDate ?? DateTime.UtcNow.AddDays(30);
                _context.Add(new SupplierPayment
                {
                    SupplierCurrentAccountId = SupplierCurrentAccountId,
                    Amount = orderTotal,
                    Method = payMethod,
                    DueDate = effectiveDue,
                    CheckNumber = docRef,
                    Notes = $"[Beklenen Sipariş Ödemesi - {paymentChannel}] {itemSummary}",
                    PaymentDate = DateTime.UtcNow,
                    Status = PaymentStatus.Pending
                });
            }

            await _context.SaveChangesAsync();

            // 3. Şantiye Kara Defterine Teslimat Takibi Görevi Aç
            await _supplierSyncService.CreateDeliveryAgendaRecordAsync(
                account.AgencyId,
                contract.ProjectId,
                account.PhonebookContactId,
                account.PhonebookContact?.Name ?? contract.SupplierName,
                $"{contract.PackageTitle} Parti Siparişi ({orderedLines.Count} Kalem)",
                orderTotal,
                $"{paymentChannel} - {(isPaidNow ? "Peşin / Evrak Verildi" : $"Vadeli ({(dueDate ?? DateTime.UtcNow.AddDays(30)):dd.MM.yyyy})")}",
                dueDate ?? DateTime.UtcNow.AddDays(3));

            TempData["SuccessMessage"] = $"🚚 {orderedLines.Count} kalem için toplam {orderTotal:N2} ₺ tutarındaki sipariş oluşturuldu! Stok kartları '{targetWarehouse?.Name ?? "Şantiye Deposu"}' içine işlendi, cari bakiye ve ödeme takvimi güncellendi.";
            return RedirectToAction(nameof(Details), new { id = SupplierCurrentAccountId });
        }
    }
}
