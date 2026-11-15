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
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var consultant = await _context.Set<GMK360.Core.Entities.AgencyConsultant>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.UserId == user.Id && !a.IsDeleted);
                if (consultant != null && consultant.AgencyId > 0)
                    return consultant.AgencyId;
            }

            var agencyClaim = User.FindFirst("AgencyId")?.Value;
            if (int.TryParse(agencyClaim, out int parsedAgencyId) && parsedAgencyId > 0)
                return parsedAgencyId;

            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
            {
                var firstAgency = await _context.Agencies.AsNoTracking().OrderBy(a => a.Id).FirstOrDefaultAsync();
                return firstAgency?.Id ?? 1;
            }

            return null;
        }

        public async Task<IActionResult> Index(int? id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            // 1. Önce eksik depoları tespit edip oluşturalım
            if (agencyId.HasValue)
            {
                var allAgencyWarehouses = await _context.Warehouses
                    .Where(w => w.AgencyId == agencyId.Value)
                    .ToListAsync();

                // Merkez Depo kontrolü
                if (!allAgencyWarehouses.Any(w => w.Type == WarehouseType.Merkez && !w.IsDeleted))
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
                }

                // Sadece KESİNLEŞMİŞ AKTİF ŞANTİYELER için şantiye deposu açılır (Aday/Teklif projelerine depo açılmaz!)
                var activeSiteProjects = await _context.ConstructionProjects
                    .Where(p => p.AgencyId == agencyId.Value && !p.IsDeleted && p.Status == ProjectStatus.Aktif_Santiye)
                    .ToListAsync();

                foreach (var proj in activeSiteProjects)
                {
                    if (!allAgencyWarehouses.Any(w => w.ConstructionProjectId == proj.Id && !w.IsDeleted))
                    {
                        var santiyeDepo = new Warehouse
                        {
                            AgencyId = agencyId.Value,
                            ConstructionProjectId = proj.Id,
                            Name = proj.Name + " Şantiyesi Deposu",
                            Type = WarehouseType.Santiye,
                            Address = proj.Address ?? "",
                            Description = proj.Name + " Şantiye sahası genel deposu.",
                            CreatedAt = System.DateTime.UtcNow
                        };
                        _context.Warehouses.Add(santiyeDepo);
                        await _context.SaveChangesAsync();
                    }
                }
                ViewBag.Catalogs = await _context.MaterialCatalogs.Where(c => c.AgencyId == agencyId.Value && !c.IsDeleted).OrderBy(c => c.Name).ToListAsync();

                var staffNames = await _context.AgencyConsultants
                    .AsNoTracking()
                    .Include(c => c.User)
                    .Where(c => c.AgencyId == agencyId.Value && !c.IsDeleted && c.IsActive)
                    .Select(c => (c.User.FirstName + " " + c.User.LastName) + " (" + c.Role.ToString() + ")")
                    .ToListAsync();
                var workerNames = await _context.AgencyWorkers
                    .AsNoTracking()
                    .Where(w => w.AgencyId == agencyId.Value && !w.IsDeleted && w.IsActive)
                    .Select(w => w.FirstName + " " + w.LastName + " (" + w.Profession + ")")
                    .ToListAsync();
                ViewBag.CustodyPeople = staffNames.Concat(workerNames).Distinct().OrderBy(x => x).ToList();
            }

            // 2. Şimdi listelemek üzere filtreli sorguyu oluşturalım
            var activeProjectIds = await _context.ConstructionProjects
                .Where(p => (!agencyId.HasValue || p.AgencyId == agencyId.Value) && !p.IsDeleted && p.Status == ProjectStatus.Aktif_Santiye)
                .Select(p => p.Id)
                .ToListAsync();

            var query = _context.Warehouses
                .Include(w => w.Items)
                .Where(w => !w.IsDeleted && w.AgencyId == agencyId)
                .AsQueryable();

            if (id.HasValue)
            {
                query = query.Where(w => w.ConstructionProjectId == id.Value);
                var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == id.Value && p.AgencyId == agencyId);
                if (project != null)
                {
                    ViewData["ProjectId"] = project.Id;
                    ViewData["ProjectName"] = project.Name;
                }
            }
            else
            {
                // Ana menüde sadece Merkez Depo (Öz Varlık) + Kesinleşmiş Aktif Şantiyelerin Depoları (veya içinde mal olan depolar) gösterilir
                query = query.Where(w => w.Type == WarehouseType.Merkez
                    || (w.ConstructionProjectId.HasValue && activeProjectIds.Contains(w.ConstructionProjectId.Value))
                    || w.Items.Any(i => i.Quantity > 0));
            }

            var finalWarehouses = await query.OrderBy(w => w.Type == WarehouseType.Merkez ? 0 : 1).ThenBy(w => w.Name).ToListAsync();
            return View(finalWarehouses);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateItemCustodyAndStatus(int itemId, string currentStatus, string? custodianName, decimal? lostOrConsumedQty, string? notes)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var item = await _context.InventoryItems
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.Warehouse.AgencyId == agencyId.Value);

            if (item == null)
            {
                TempData["ErrorMessage"] = "Envanter kalemi bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            item.CurrentStatus = string.IsNullOrWhiteSpace(currentStatus) ? "Depoda" : currentStatus;
            if (custodianName != null)
            {
                item.ServiceContact = custodianName.Trim();
            }

            if (lostOrConsumedQty.HasValue && lostOrConsumedQty.Value > 0 && lostOrConsumedQty.Value <= item.Quantity)
            {
                item.Quantity -= lostOrConsumedQty.Value;
                _context.InventoryTransactions.Add(new InventoryTransaction
                {
                    InventoryItemId = item.Id,
                    Type = TransactionType.Cikis,
                    Quantity = lostOrConsumedQty.Value,
                    TransactionDate = System.DateTime.UtcNow,
                    Description = $"Zayi / Sarf / Fire Düşümü ({currentStatus}): {notes ?? "-"} (Sorumlu: {item.ServiceContact})",
                    HandledByUserId = _userManager.GetUserId(User) ?? ""
                });
            }
            else
            {
                _context.InventoryTransactions.Add(new InventoryTransaction
                {
                    InventoryItemId = item.Id,
                    Type = TransactionType.Giris,
                    Quantity = 0,
                    TransactionDate = System.DateTime.UtcNow,
                    Description = $"Zimmet / Durum Güncellemesi -> Durum: {item.CurrentStatus}, Zimmetli Sorumlu: {(string.IsNullOrEmpty(item.ServiceContact) ? "Depo" : item.ServiceContact)} ({notes ?? ""})",
                    HandledByUserId = _userManager.GetUserId(User) ?? ""
                });
            }

            item.UpdatedAt = System.DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{item.Name} zimmet ve durum bilgisi ({item.CurrentStatus} - {(string.IsNullOrEmpty(item.ServiceContact) ? "Depoda" : item.ServiceContact)}) güncellendi.";
            return RedirectToAction(nameof(Index));
        }
    }
}

