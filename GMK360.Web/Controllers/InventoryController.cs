using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Identity;
using GMK360.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GMK360.Core.Entities.Construction;
using GMK360.Core.Entities.Identity;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InventoryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetUserAgencyIdAsync()
        {
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")) 
                return 1;
                
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            
            var consultant = await _context.Set<GMK360.Core.Entities.AgencyConsultant>().FirstOrDefaultAsync(a => a.UserId == user.Id);
            return consultant?.AgencyId;
        }

        public async Task<IActionResult> Index()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null && !User.IsInRole("Admin")) return Unauthorized();

            var query = _context.Warehouses.Include(w => w.Items).AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(w => w.AgencyId == agencyId);
            }

            var warehouses = await query.ToListAsync();
            
            // Otomatik Merkez Depo Oluþturma
            if (!warehouses.Any(w => w.Type == WarehouseType.Merkez) && agencyId.HasValue)
            {
                var merkezDepo = new Warehouse
                {
                    AgencyId = agencyId.Value,
                    Name = "Merkez Depo",
                    Type = WarehouseType.Merkez,
                    Address = "",
                    Description = "Merkez Depo (Ana Merkez)",
                    CreatedAt = System.DateTime.UtcNow
                };
                _context.Warehouses.Add(merkezDepo);
                await _context.SaveChangesAsync();
                
                warehouses.Add(merkezDepo);
            }

            // Geriye dönük: Þantiyeler için otomatik depo kontrolü
            if (agencyId.HasValue)
            {
                var projects = await _context.ConstructionProjects
                    .Where(p => p.AgencyId == agencyId.Value && p.StatusId == 1) // Devam eden projeler
                    .ToListAsync();

                foreach (var proj in projects)
                {
                    if (!warehouses.Any(w => w.ConstructionProjectId == proj.Id))
                    {
                        var santiyeDepo = new Warehouse
                        {
                            AgencyId = agencyId.Value,
                            ConstructionProjectId = proj.Id,
                            Name = proj.Name + " Þantiyesi Deposu",
                            Type = WarehouseType.Santiye,
                            Address = proj.Address ?? "",
                            Description = proj.Name + " þantiye sahasý genel deposu (Konteyner).",
                            CreatedAt = System.DateTime.UtcNow
                        };
                        _context.Warehouses.Add(santiyeDepo);
                        await _context.SaveChangesAsync();
                        warehouses.Add(santiyeDepo);
                    }
                }
            }

            ViewBag.Catalogs = await _context.MaterialCatalogs.Where(c => c.AgencyId == agencyId).OrderBy(c => c.Name).ToListAsync();
            return View(warehouses);
        }
    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDevir(int warehouseId, string name, InventoryItemType itemType, decimal quantity, string unit, string brand, string model, string serialNumber, string serviceContact, string supplierName)
        {
            try
            {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null) return Unauthorized();

                var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == warehouseId && (w.AgencyId == agencyId || User.IsInRole("Admin")));
                if (warehouse == null)
                {
                    TempData["ErrorMessage"] = "Depo bulunamadý veya yetkiniz yok.";
                    return RedirectToAction(nameof(Index));
                }

                var item = new InventoryItem
                {
                    WarehouseId = warehouseId,
                    Name = name,
                    ItemType = itemType,
                    EntryMethod = InventoryEntryMethod.Devir, // Legacy
                    Brand = brand ?? "",
                    Model = model ?? "",
                    SerialNumber = serialNumber ?? "",
                    SupplierName = string.IsNullOrWhiteSpace(supplierName) ? "Devir" : supplierName,
                    ServiceContact = serviceContact ?? "",
                    Quantity = quantity,
                    Unit = unit ?? "Adet",
                    CurrentStatus = "Depoda",
                    CreatedAt = System.DateTime.UtcNow
                };

                _context.InventoryItems.Add(item);
                await _context.SaveChangesAsync();

                var user = await _userManager.GetUserAsync(User);

                // Transaction (Ýlk Giriþ)
                var transaction = new InventoryTransaction
                {
                    InventoryItemId = item.Id,
                    Type = TransactionType.Giris,
                    Quantity = quantity,
                    TransactionDate = System.DateTime.UtcNow,
                    Description = "Geçmiþten Devir (Faturasýz Stok Giriþi)",
                    HandledByUserId = user?.Id ?? ""
                };
                
                _context.InventoryTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"{name} baþarýyla {warehouse.Name} adlý depoya Devir olarak eklendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Demirbaþ/Sarf eklenirken hata oluþtu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> MaterialCatalog()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var catalogs = await _context.MaterialCatalogs
                .Where(m => m.AgencyId == agencyId.Value)
                .OrderBy(m => m.Name)
                .ToListAsync();

            return View(catalogs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaterial(string name, InventoryItemType type, string defaultUnit, string defaultBrand)
        {
            try
            {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null) return Unauthorized();

                var exists = await _context.MaterialCatalogs.AnyAsync(m => m.AgencyId == agencyId.Value && m.Name.ToLower() == name.ToLower());
                if(exists){
                    TempData["ErrorMessage"] = "Bu malzeme zaten kataloðunuzda mevcut.";
                    return RedirectToAction(nameof(MaterialCatalog));
                }

                var mat = new MaterialCatalog
                {
                    AgencyId = agencyId.Value,
                    Name = name,
                    Type = type,
                    DefaultUnit = defaultUnit ?? "Adet",
                    DefaultBrand = defaultBrand ?? "",
                    CreatedAt = System.DateTime.UtcNow
                };

                _context.MaterialCatalogs.Add(mat);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"{name} baþarýyla malzeme kataloðuna eklendi.";
                return RedirectToAction(nameof(MaterialCatalog));
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Hata oluþtu: " + ex.Message;
                return RedirectToAction(nameof(MaterialCatalog));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferItem(int sourceItemId, int targetWarehouseId, decimal quantity)
        {
            try
            {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null) return Unauthorized();

                var sourceItem = await _context.InventoryItems
                    .Include(i => i.Warehouse)
                    .Include(i => i.MaterialCatalog)
                    .FirstOrDefaultAsync(i => i.Id == sourceItemId && i.Warehouse.AgencyId == agencyId.Value);

                if (sourceItem == null)
                {
                    TempData["ErrorMessage"] = "Kaynak malzeme bulunamadý.";
                    return RedirectToAction(nameof(Index));
                }

                if (quantity <= 0 || quantity > sourceItem.Quantity)
                {
                    TempData["ErrorMessage"] = "Geçersiz miktar. Stoktan fazla transfer yapýlamaz.";
                    return RedirectToAction(nameof(Index));
                }

                var targetWarehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == targetWarehouseId && w.AgencyId == agencyId.Value);
                if (targetWarehouse == null)
                {
                    TempData["ErrorMessage"] = "Hedef depo bulunamadý.";
                    return RedirectToAction(nameof(Index));
                }

                // 1. Kaynak depodan düþ
                sourceItem.Quantity -= quantity;
                sourceItem.UpdatedAt = System.DateTime.UtcNow;
                _context.InventoryItems.Update(sourceItem);

                var sourceTxn = new InventoryTransaction
                {
                    InventoryItemId = sourceItem.Id,
                    Type = GMK360.Core.Entities.Construction.TransactionType.Cikis,
                    Quantity = quantity,
                    TransactionDate = System.DateTime.UtcNow,
                    Description = $"{targetWarehouse.Name} deposuna transfer (Çýkýþ).",
                    HandledByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                };
                _context.InventoryTransactions.Add(sourceTxn);

                // 2. Hedef depoya ekle
                var targetItem = await _context.InventoryItems.FirstOrDefaultAsync(i => i.WarehouseId == targetWarehouseId && i.MaterialCatalogId == sourceItem.MaterialCatalogId);
                int targetItemIdToLink = 0;

                if (targetItem != null)
                {
                    targetItem.Quantity += quantity;
                    targetItem.UpdatedAt = System.DateTime.UtcNow;
                    _context.InventoryItems.Update(targetItem);
                    targetItemIdToLink = targetItem.Id;
                }
                else
                {
                    var newItem = new InventoryItem
                    {
                        WarehouseId = targetWarehouseId,
                        MaterialCatalogId = sourceItem.MaterialCatalogId,
                        Name = sourceItem.Name,
                        ItemType = sourceItem.ItemType,
                        EntryMethod = GMK360.Core.Entities.Construction.InventoryEntryMethod.Devir,
                        Brand = sourceItem.Brand,
                        Model = sourceItem.Model,
                        SerialNumber = "", // Serial num cannot be bulk transferred easily but let's clear it
                        ServiceContact = sourceItem.ServiceContact,
                        SupplierName = sourceItem.SupplierName,
                        Quantity = quantity,
                        Unit = sourceItem.Unit,
                        CreatedAt = System.DateTime.UtcNow,
                        CurrentStatus = "Depoda"
                    };
                    _context.InventoryItems.Add(newItem);
                    await _context.SaveChangesAsync(); // get ID
                    targetItemIdToLink = newItem.Id;
                }

                var targetTxn = new InventoryTransaction
                {
                    InventoryItemId = targetItemIdToLink,
                    Type = GMK360.Core.Entities.Construction.TransactionType.Giris,
                    Quantity = quantity,
                    TransactionDate = System.DateTime.UtcNow,
                    Description = $"{sourceItem.Warehouse.Name} deposundan transfer (Giriþ).",
                    HandledByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                };
                _context.InventoryTransactions.Add(targetTxn);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"{sourceItem.Name} ({quantity} {sourceItem.Unit}), {targetWarehouse.Name} deposuna baþarýyla sevk edildi.";
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Transfer sýrasýnda hata oluþtu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> MigrateLegacyItems()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Content("Unauthorized");

            var legacyItems = await _context.InventoryItems.Where(i => i.MaterialCatalogId == null).ToListAsync();
            int count = 0;
            foreach (var item in legacyItems)
            {
                var catalog = await _context.MaterialCatalogs.FirstOrDefaultAsync(c => c.AgencyId == agencyId && c.Name == item.Name);
                if (catalog == null)
                {
                    catalog = new MaterialCatalog
                    {
                        AgencyId = agencyId.Value,
                        Name = item.Name,
                        Type = item.ItemType,
                        DefaultUnit = item.Unit ?? "Adet",
                        DefaultBrand = item.Brand ?? "",
                        CreatedAt = System.DateTime.UtcNow,
                        IsDeleted = false
                    };
                    _context.MaterialCatalogs.Add(catalog);
                    await _context.SaveChangesAsync();
                }
                
                item.MaterialCatalogId = catalog.Id;
                _context.InventoryItems.Update(item);
                count++;
            }
            await _context.SaveChangesAsync();
            return Content($"Migrated {count} items.");
        }
}
}
