using GMK360.Core.Entities.Construction;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using GMK360.Core.Entities.Identity;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class InventoryReceiptsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        
                        private async Task<int?> GetUserAgencyIdAsync()
        {
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")) 
                return 1;
                
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            
            var consultant = await _context.Set<GMK360.Core.Entities.AgencyConsultant>().FirstOrDefaultAsync(a => a.UserId == user.Id);
            return consultant?.AgencyId;
        }

        public InventoryReceiptsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var receipts = await _context.InventoryReceipts
                .Include(r => r.Warehouse)
                .Include(r => r.Items).ThenInclude(i => i.MaterialCatalog)
                .Include(r => r.AssignedUser)
                .Where(r => r.AgencyId == agencyId.Value)
                .OrderByDescending(r => r.ReceiptDate)
                .ToListAsync();

            ViewBag.Warehouses = await _context.Warehouses.Where(w => w.AgencyId == agencyId.Value).ToListAsync();
            ViewBag.SupplierAccounts = await _context.SupplierCurrentAccounts.Include(a => a.PhonebookContact).Where(a => a.AgencyId == agencyId.Value).ToListAsync();
            ViewBag.Catalogs = await _context.MaterialCatalogs.Where(c => c.AgencyId == agencyId.Value).OrderBy(c => c.Name).ToListAsync();
            ViewBag.Consultants = await _context.Set<GMK360.Core.Entities.AgencyConsultant>()
                .Include(c => c.User)
                .Where(c => c.AgencyId == agencyId.Value && c.User != null)
                .ToListAsync();

            if (Request.Query.ContainsKey("debug")) 
            {
                return Json(receipts.Select(r => new {
                    r.Id,
                    r.DocumentNumber,
                    r.SupplierName,
                    WarehouseId = r.WarehouseId,
                    WarehouseName = r.Warehouse?.Name,
                    r.Status,
                    r.AssignedUserId,
                    AssignedUserName = r.AssignedUser?.DisplayName
                }));
            }

            return View(receipts);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int warehouseId, int supplierCurrentAccountId, string documentNumber, DateTime receiptDate, string notes, int[] materialCatalogIds, decimal[] quantities, decimal[] unitPrices, string paymentMethod, DateTime? dueDate)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var receipt = new InventoryReceipt
            {
                AgencyId = agencyId.Value,
                WarehouseId = warehouseId,
                SupplierName = "Cari ID: " + supplierCurrentAccountId,
                DocumentNumber = documentNumber ?? "-",
                ReceiptDate = receiptDate,
                Notes = notes ?? "",
                Status = "Draft"
            };

            _context.InventoryReceipts.Add(receipt);
            await _context.SaveChangesAsync();

            if (materialCatalogIds != null && quantities != null)
            {
                for (int i = 0; i < materialCatalogIds.Length; i++)
                {
                    if (i < quantities.Length && materialCatalogIds[i] > 0 && quantities[i] > 0)
                    {
                        var item = new InventoryReceiptItem
                        {
                            InventoryReceiptId = receipt.Id,
                            MaterialCatalogId = materialCatalogIds[i],
                            Quantity = quantities[i]
                        };
                        _context.InventoryReceiptItems.Add(item);
                    }
                }
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Ýrsaliye taslaðý baþarýyla oluþturuldu. Stoða iþlemek için onaylamanýz gerekmektedir.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            var receipt = await _context.InventoryReceipts
                .Include(r => r.Items).ThenInclude(i => i.MaterialCatalog)
                .FirstOrDefaultAsync(r => r.Id == id && r.AgencyId == agencyId);

            if (receipt == null) return NotFound();
            if (receipt.Status == "Approved")
            {
                TempData["ErrorMessage"] = "Bu irsaliye zaten onaylanmýþ ve stoða iþlenmiþ.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var item in receipt.Items)
            {
                var invItem = await _context.InventoryItems
                    .Include(i => i.Transactions)
                    .FirstOrDefaultAsync(i => i.WarehouseId == receipt.WarehouseId && i.MaterialCatalogId == item.MaterialCatalogId);

                if (invItem != null)
                {
                    invItem.Quantity += item.Quantity;
                }
                else
                {
                    invItem = new InventoryItem
                    {
                        WarehouseId = receipt.WarehouseId,
                        MaterialCatalogId = item.MaterialCatalogId,
                        Name = item.MaterialCatalog.Name,
                        ItemType = item.MaterialCatalog.Type,
                        EntryMethod = InventoryEntryMethod.SatinAlma,
                        Quantity = item.Quantity,
                        Unit = item.MaterialCatalog.DefaultUnit,
                        CurrentStatus = "Depoda",
                        Transactions = new List<InventoryTransaction>()
                    };
                    _context.InventoryItems.Add(invItem);
                }

                invItem.Transactions.Add(new InventoryTransaction
                {
                    
                    Type = TransactionType.Giris,
                    Quantity = item.Quantity,
                    TransactionDate = DateTime.UtcNow,
                    Description = $"Ýrsaliye Kabul: {receipt.DocumentNumber} - {receipt.SupplierName}",
                    HandledByUserId = _userManager.GetUserId(User) ?? ""
                });
            }

            receipt.Status = "Approved";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ýrsaliye baþarýyla onaylandý ve malzemeler ilgili depo/þantiye stoðuna EKLENDÝ.";
            return RedirectToAction(nameof(Index));
        }
    }
}







