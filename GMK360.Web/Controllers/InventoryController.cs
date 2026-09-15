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
            
            // Otomatik Merkez Depo Olu�turma
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

            // Geriye d�n�k: �antiyeler i�in otomatik depo kontrol�
            if (agencyId.HasValue)
            {
                var projects = await _context.ConstructionProjects
                    .Where(p => p.AgencyId == agencyId.Value && p.Status == GMK360.Core.Entities.Construction.ProjectStatus.Aktif_Santiye) // Devam eden projeler
                    .ToListAsync();

                foreach (var proj in projects)
                {
                    if (!warehouses.Any(w => w.ConstructionProjectId == proj.Id))
                    {
                        var santiyeDepo = new Warehouse
                        {
                            AgencyId = agencyId.Value,
                            ConstructionProjectId = proj.Id,
                            Name = proj.Name + " �antiyesi Deposu",
                            Type = WarehouseType.Santiye,
                            Address = proj.Address ?? "",
                            Description = proj.Name + " �antiye sahas� genel deposu (Konteyner).",
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
                    TempData["ErrorMessage"] = "Depo bulunamad� veya yetkiniz yok.";
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

                // Transaction (�lk Giri�)
                var transaction = new InventoryTransaction
                {
                    InventoryItemId = item.Id,
                    Type = TransactionType.Giris,
                    Quantity = quantity,
                    TransactionDate = System.DateTime.UtcNow,
                    Description = "Ge�mi�ten Devir (Faturas�z Stok Giri�i)",
                    HandledByUserId = user?.Id ?? ""
                };
                
                _context.InventoryTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"{name} ba�ar�yla {warehouse.Name} adl� depoya Devir olarak eklendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Demirba�/Sarf eklenirken hata olu�tu: " + ex.Message;
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
                    TempData["ErrorMessage"] = "Bu malzeme zaten katalo�unuzda mevcut.";
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

                TempData["SuccessMessage"] = $"{name} ba�ar�yla malzeme katalo�una eklendi.";
                return RedirectToAction(nameof(MaterialCatalog));
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Hata olu�tu: " + ex.Message;
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
                    TempData["ErrorMessage"] = "Kaynak malzeme bulunamad�.";
                    return RedirectToAction(nameof(Index));
                }

                if (quantity <= 0 || quantity > sourceItem.Quantity)
                {
                    TempData["ErrorMessage"] = "Ge�ersiz miktar. Stoktan fazla transfer yap�lamaz.";
                    return RedirectToAction(nameof(Index));
                }

                var targetWarehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == targetWarehouseId && w.AgencyId == agencyId.Value);
                if (targetWarehouse == null)
                {
                    TempData["ErrorMessage"] = "Hedef depo bulunamad�.";
                    return RedirectToAction(nameof(Index));
                }

                // 1. Kaynak depodan d��
                sourceItem.Quantity -= quantity;
                sourceItem.UpdatedAt = System.DateTime.UtcNow;
                _context.InventoryItems.Update(sourceItem);

                var sourceTxn = new InventoryTransaction
                {
                    InventoryItemId = sourceItem.Id,
                    Type = GMK360.Core.Entities.Construction.TransactionType.Cikis,
                    Quantity = quantity,
                    TransactionDate = System.DateTime.UtcNow,
                    Description = $"{targetWarehouse.Name} deposuna transfer (��k��).",
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
                    Description = $"{sourceItem.Warehouse.Name} deposundan transfer (Giri�).",
                    HandledByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                };
                _context.InventoryTransactions.Add(targetTxn);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"{sourceItem.Name} ({quantity} {sourceItem.Unit}), {targetWarehouse.Name} deposuna ba�ar�yla sevk edildi.";
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Transfer s�ras�nda hata olu�tu: " + ex.Message;
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

