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
using GMK360.Core.Entities.Identity;
using GMK360.Core.Entities.B2B;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class DemolitionSalvageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DemolitionSalvageController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetUserAgencyIdAsync()
        {
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")) return 1;
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            var consultant = await _context.Set<GMK360.Core.Entities.AgencyConsultant>().FirstOrDefaultAsync(a => a.UserId == user.Id);
            return consultant?.AgencyId ?? 1;
        }

        // GET: /DemolitionSalvage/Index
        public async Task<IActionResult> Index(int? projectId)
        {
            var agencyId = await GetUserAgencyIdAsync() ?? 1;

            var projects = await _context.ConstructionProjects
                .Include(p => p.Blocks)
                .Where(p => p.AgencyId == agencyId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.Projects = projects;

            // Seçili proje yoksa ilk kentsel dönüşüm / eski bina içeren projeyi veya ilk projeyi seçelim
            int selectedProjId = projectId ?? (projects.FirstOrDefault(p => p.Blocks != null && p.Blocks.Any(b => b.IsExistingBuilding))?.Id ?? projects.FirstOrDefault()?.Id ?? 0);
            ViewBag.SelectedProjectId = selectedProjId;

            var selectedProject = projects.FirstOrDefault(p => p.Id == selectedProjId);
            ViewBag.SelectedProject = selectedProject;

            var items = new List<DemolitionSalvageItem>();
            if (selectedProjId > 0)
            {
                items = await _context.DemolitionSalvageItems
                    .Where(i => i.ConstructionProjectId == selectedProjId)
                    .OrderBy(i => i.ItemCategory)
                    .ThenBy(i => i.ItemName)
                    .ToListAsync();
            }

            // İstatistikler
            ViewBag.TotalItemsCount = items.Count;
            ViewBag.TotalEstimatedValue = items.Sum(i => (i.EstimatedQuantity * (i.EstimatedUnitPrice ?? 0)));
            ViewBag.TotalRealizedRevenue = items.Where(i => i.Status == "Satıldı").Sum(i => (i.SoldTotalPrice ?? 0));
            ViewBag.PendingDepotCount = items.Count(i => i.Status == "Söküldü / Depoda");

            // Bu projeye ait açık ihaleler var mı?
            ViewBag.ActiveTenders = await _context.B2BQuoteRequests
                .Include(q => q.Invites).ThenInclude(i => i.NetworkContact)
                .Where(q => q.SourceModule == "DemolitionSalvage" && q.SourceReferenceId == selectedProjId)
                .ToListAsync();

            return View(items);
        }

        // POST: /DemolitionSalvage/AutoGenerateEstimate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AutoGenerateEstimate(int projectId)
        {
            var project = await _context.ConstructionProjects
                .Include(p => p.Blocks)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return NotFound();

            // Zaten eklenmiş kalemler varsa mükerrer yapmayalım
            bool hasExisting = await _context.DemolitionSalvageItems.AnyAsync(i => i.ConstructionProjectId == projectId);
            if (hasExisting)
            {
                TempData["ErrorMessage"] = "Bu proje için söküm envanteri zaten mevcut.";
                return RedirectToAction(nameof(Index), new { projectId });
            }

            var eskiBloklar = project.Blocks?.Where(b => b.IsExistingBuilding).ToList();
            int daireSayisi = eskiBloklar?.Sum(b => b.TotalApartments) ?? project.EskiDaireSayisi ?? 8;
            int dukkanSayisi = eskiBloklar?.Sum(b => b.TotalShops) ?? project.EskiDukkanSayisi ?? 1;
            double toplamM2 = project.EskiToplamMetrekare ?? ((daireSayisi * 110) + (dukkanSayisi * 150));
            if (toplamM2 <= 0) toplamM2 = 1000;

            var templates = new List<DemolitionSalvageItem>
            {
                new DemolitionSalvageItem
                {
                    ConstructionProjectId = projectId,
                    ItemCategory = "Isıtma & Tesisat",
                    ItemName = "Panel Radyatör Petek (Muhtelif Ölçü)",
                    EstimatedQuantity = Math.Max(10, daireSayisi * 5 + dukkanSayisi * 2),
                    Unit = "Adet",
                    EstimatedUnitPrice = 750,
                    Status = "Binada Takılı",
                    Notes = "Demirdöküm / ECA standart kalorifer petekleri"
                },
                new DemolitionSalvageItem
                {
                    ConstructionProjectId = projectId,
                    ItemCategory = "Isıtma & Tesisat",
                    ItemName = "Kombi & Hermetik Şofben",
                    EstimatedQuantity = Math.Max(2, daireSayisi),
                    Unit = "Adet",
                    EstimatedUnitPrice = 3500,
                    Status = "Binada Takılı",
                    Notes = "Doğalgaz kombileri sökülüp test edilecek"
                },
                new DemolitionSalvageItem
                {
                    ConstructionProjectId = projectId,
                    ItemCategory = "Isıtma & Tesisat",
                    ItemName = "Hurda Tesisat Bakırı & Pirinç Vana",
                    EstimatedQuantity = Math.Max(20, daireSayisi * 8),
                    Unit = "Kg",
                    EstimatedUnitPrice = 280,
                    Status = "Binada Takılı",
                    Notes = "Su ve kalorifer tesisat bakır boruları"
                },
                new DemolitionSalvageItem
                {
                    ConstructionProjectId = projectId,
                    ItemCategory = "Kapı & Pencere",
                    ItemName = "Çelik Daire Giriş Kapısı",
                    EstimatedQuantity = Math.Max(2, daireSayisi + dukkanSayisi),
                    Unit = "Adet",
                    EstimatedUnitPrice = 2000,
                    Status = "Binada Takılı",
                    Notes = "Kasaları ile birlikte hasarsız sökülecek"
                },
                new DemolitionSalvageItem
                {
                    ConstructionProjectId = projectId,
                    ItemCategory = "Kapı & Pencere",
                    ItemName = "İç Oda Kapısı (Ahşap / Amerikan Panel)",
                    EstimatedQuantity = Math.Max(8, daireSayisi * 4),
                    Unit = "Adet",
                    EstimatedUnitPrice = 500,
                    Status = "Binada Takılı",
                    Notes = "Menteşe ve kolları sağlam olanlar ayrılacak"
                },
                new DemolitionSalvageItem
                {
                    ConstructionProjectId = projectId,
                    ItemCategory = "Kapı & Pencere",
                    ItemName = "PVC Çift Cam Doğrama & Pencere",
                    EstimatedQuantity = Math.Max(8, daireSayisi * 4 + dukkanSayisi * 2),
                    Unit = "Takım",
                    EstimatedUnitPrice = 1200,
                    Status = "Binada Takılı",
                    Notes = "Profil ve camlar çıkma olarak satılabilir"
                },
                new DemolitionSalvageItem
                {
                    ConstructionProjectId = projectId,
                    ItemCategory = "Mutfak & Banyo",
                    ItemName = "Mutfak Dolabı & Mermer Tezgah",
                    EstimatedQuantity = Math.Max(2, daireSayisi),
                    Unit = "Takım",
                    EstimatedUnitPrice = 3000,
                    Status = "Binada Takılı",
                    Notes = "Sağlam gövdeler ikinci el için ayrılacak"
                },
                new DemolitionSalvageItem
                {
                    ConstructionProjectId = projectId,
                    ItemCategory = "Yapısal Hurda",
                    ItemName = "Hurda İnşaat Demiri (Yıkımdan Çıkacak)",
                    EstimatedQuantity = Math.Max(5, (decimal)Math.Round(toplamM2 * 0.015, 1)),
                    Unit = "Ton",
                    EstimatedUnitPrice = 14500,
                    Status = "Binada Takılı",
                    Notes = "Beton kırımı sonrası ayıklanacak nervürlü/düz inşaat demiri"
                },
                new DemolitionSalvageItem
                {
                    ConstructionProjectId = projectId,
                    ItemCategory = "Yapısal Hurda",
                    ItemName = "Çatı Sacı & Profil Demir",
                    EstimatedQuantity = Math.Max(500, (decimal)Math.Round(toplamM2 * 0.3, 0)),
                    Unit = "Kg",
                    EstimatedUnitPrice = 16,
                    Status = "Binada Takılı",
                    Notes = "Çatı karkasından çıkacak sac ve profiller"
                }
            };

            _context.DemolitionSalvageItems.AddRange(templates);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Eski binadaki {daireSayisi} daireye göre tahmini 9 kalem söküm ve hurda envanteri oluşturuldu!";
            return RedirectToAction(nameof(Index), new { projectId });
        }

        // POST: /DemolitionSalvage/AddItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(
            int projectId, 
            string itemCategory, 
            string itemName, 
            decimal quantity, 
            string unit, 
            decimal? estimatedUnitPrice, 
            string? notes)
        {
            if (projectId <= 0 || string.IsNullOrWhiteSpace(itemName)) return BadRequest();

            var item = new DemolitionSalvageItem
            {
                ConstructionProjectId = projectId,
                ItemCategory = string.IsNullOrWhiteSpace(itemCategory) ? "Diğer" : itemCategory.Trim(),
                ItemName = itemName.Trim(),
                EstimatedQuantity = quantity > 0 ? quantity : 1,
                Unit = string.IsNullOrWhiteSpace(unit) ? "Adet" : unit.Trim(),
                EstimatedUnitPrice = estimatedUnitPrice,
                Status = "Binada Takılı",
                Notes = notes?.Trim()
            };

            _context.DemolitionSalvageItems.Add(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{item.ItemName} söküm envanterine eklendi.";
            return RedirectToAction(nameof(Index), new { projectId });
        }

        // POST: /DemolitionSalvage/MarkAsSold
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsSold(int itemId, decimal soldTotalPrice, string buyerName, string? notes)
        {
            var item = await _context.DemolitionSalvageItems.FindAsync(itemId);
            if (item == null) return NotFound();

            item.Status = "Satıldı";
            item.SoldTotalPrice = soldTotalPrice;
            item.BuyerName = string.IsNullOrWhiteSpace(buyerName) ? "Hurdacı" : buyerName.Trim();
            item.SoldDate = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(notes))
            {
                item.Notes = (item.Notes != null ? item.Notes + " | " : "") + notes.Trim();
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{item.ItemName} satışı ({soldTotalPrice:N2} ₺) kaydedildi ve hurda gelirine işlendi.";
            return RedirectToAction(nameof(Index), new { projectId = item.ConstructionProjectId });
        }

        // POST: /DemolitionSalvage/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int itemId, string status)
        {
            var item = await _context.DemolitionSalvageItems.FindAsync(itemId);
            if (item == null) return NotFound();

            item.Status = status;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{item.ItemName} durumu '{status}' olarak güncellendi.";
            return RedirectToAction(nameof(Index), new { projectId = item.ConstructionProjectId });
        }

        // POST: /DemolitionSalvage/CreateScrapSaleTender
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateScrapSaleTender(int projectId)
        {
            var agencyId = await GetUserAgencyIdAsync() ?? 1;
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            var quoteRequest = new B2BQuoteRequest
            {
                RequesterAgencyId = agencyId,
                RequesterUserId = user?.Id ?? "ProjeYöneticisi",
                SourceModule = "DemolitionSalvage",
                SourceReferenceId = projectId,
                Title = project.Name + " - Eski Bina Söküm & Hurda Malzeme Satış İhalesi (+Gelir)",
                Description = project.Name + " şantiyesindeki eski binadan çıkacak kapı, pencere, petek, kombi ve hurda demirlerin satışı için teklif alınacaktır. En yüksek teklifi veren hurdacı/alımcı kazanır.",
                Deadline = DateTime.UtcNow.AddDays(7),
                Status = "Draft"
            };

            _context.B2BQuoteRequests.Add(quoteRequest);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Söküm ve Hurda Satış İhalesi açıldı! Şimdi ilgili hurdacı ve alıcıları davet edebilirsiniz.";
            return RedirectToAction("Details", "B2BPurchasing", new { id = quoteRequest.Id });
        }

        // POST: /DemolitionSalvage/CreateDemolitionTender
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDemolitionTender(int projectId)
        {
            var agencyId = await GetUserAgencyIdAsync() ?? 1;
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            var quoteRequest = new B2BQuoteRequest
            {
                RequesterAgencyId = agencyId,
                RequesterUserId = user?.Id ?? "ProjeYöneticisi",
                SourceModule = "DemolitionSalvage",
                SourceReferenceId = projectId,
                Title = project.Name + " - Hurda Karşılığı Yıkım ve Hafriyat İhalesi",
                Description = project.Name + " projesinde bina yıkımı ve temel hafriyatı işi. Çıkacak hurda yıkımcıya kalacak olup, müteahhide ödenecek üste tutar veya talep edilecek bedel teklif edilecektir.",
                Deadline = DateTime.UtcNow.AddDays(7),
                Status = "Draft"
            };

            _context.B2BQuoteRequests.Add(quoteRequest);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Hurda Karşılığı Yıkım İhalesi açıldı! Yıkım firmalarını davet edebilirsiniz.";
            return RedirectToAction("Details", "B2BPurchasing", new { id = quoteRequest.Id });
        }

        // POST: /DemolitionSalvage/DeleteItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int itemId)
        {
            var item = await _context.DemolitionSalvageItems.FindAsync(itemId);
            if (item == null) return NotFound();

            int pId = item.ConstructionProjectId;
            _context.DemolitionSalvageItems.Remove(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kalem silindi.";
            return RedirectToAction(nameof(Index), new { projectId = pId });
        }

        // GET: /DemolitionSalvage/GetWizardDefaults
        [HttpGet]
        public async Task<IActionResult> GetWizardDefaults(int projectId, int? buildingId)
        {
            var project = await _context.ConstructionProjects.Include(p => p.Blocks).FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return NotFound(new { success = false, message = "Proje bulunamadı." });

            Building? building = null;
            if (buildingId.HasValue)
            {
                building = await _context.Buildings.Include(b => b.ChildBuildings).FirstOrDefaultAsync(b => b.Id == buildingId.Value);
            }
            else
            {
                building = project.Blocks?.FirstOrDefault(b => b.IsExistingBuilding);
            }

            int apartments = 0;
            int shops = 0;
            decimal baseArea = 0;
            int floors = 0;

            if (building != null)
            {
                if (building.ChildBuildings != null && building.ChildBuildings.Any())
                {
                    apartments = building.ChildBuildings.Sum(c => c.TotalApartments);
                    shops = building.ChildBuildings.Sum(c => c.TotalShops);
                    baseArea = (decimal)(building.BaseArea ?? building.ChildBuildings.Sum(c => c.BaseArea ?? 0));
                    floors = building.ChildBuildings.Max(c => c.TotalFloors ?? 0);
                }
                else
                {
                    apartments = building.TotalApartments;
                    shops = building.TotalShops;
                    baseArea = (decimal)(building.BaseArea ?? 0);
                    floors = building.TotalFloors ?? 0;
                }
            }
            else
            {
                apartments = project.EskiDaireSayisi ?? 8;
                shops = project.EskiDukkanSayisi ?? 1;
                baseArea = (decimal)(project.EskiToplamMetrekare.HasValue ? project.EskiToplamMetrekare.Value / Math.Max(1, floors + 1) : 400);
            }

            decimal totalM2 = project.EskiToplamMetrekare.HasValue 
                ? (decimal)project.EskiToplamMetrekare.Value 
                : (baseArea * Math.Max(1, floors + 1));
            if (totalM2 <= 0) totalM2 = (apartments * 110m) + (shops * 140m);

            int normalFloors = building?.TotalFloors ?? (floors > 0 ? floors : 4);
            if (normalFloors <= 0) normalFloors = 4;

            return Json(new
            {
                success = true,
                projectId,
                buildingId = building?.Id,
                buildingName = building?.Name ?? project.Name,
                apartments = Math.Max(1, apartments),
                shops,
                floors = Math.Max(1, floors),
                normalFloors = normalFloors,
                baseArea = baseArea,
                totalM2 = Math.Round(totalM2, 0),
                roofArea = baseArea > 0 ? Math.Round(baseArea * 1.15m, 0) : 350m
            });
        }

        // POST: /DemolitionSalvage/SaveWizardEstimate (Grid-Based)
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveWizardEstimate([FromBody] SalvageWizardGridDto dto)
        {
            if (dto == null || dto.ProjectId <= 0)
                return Json(new { success = false, message = "Geçersiz proje bilgisi." });

            var project = await _context.ConstructionProjects.FindAsync(dto.ProjectId);
            if (project == null)
                return Json(new { success = false, message = "Proje bulunamadı." });

            // Mevcut kalemleri temizleyelim (yeni güncel envanter yazılacak)
            var existingItems = await _context.DemolitionSalvageItems
                .Where(i => i.ConstructionProjectId == dto.ProjectId)
                .ToListAsync();

            if (existingItems.Any())
            {
                _context.DemolitionSalvageItems.RemoveRange(existingItems);
            }

            var newItems = new List<DemolitionSalvageItem>();

            if (dto.Rows != null)
            {
                foreach (var row in dto.Rows)
                {
                    if (string.IsNullOrWhiteSpace(row.ItemName) || row.QuantityPerFloor <= 0)
                        continue;

                    int mult = Math.Max(1, row.Multiplier);
                    decimal totalQty = row.QuantityPerFloor * mult;

                    string displayName = string.IsNullOrWhiteSpace(row.ItemType) || row.ItemType == "Standart" || row.ItemType == "Yok" || row.ItemType == "Belirtilmedi"
                        ? row.ItemName
                        : $"{row.ItemName} ({row.ItemType})";

                    string notes = mult > 1 
                        ? $"{mult} kat x {row.QuantityPerFloor:N0} {row.Unit} (Kat başı ortalama)" 
                        : (row.Notes ?? "Bina genel / ortak alan varlığı");

                    newItems.Add(new DemolitionSalvageItem
                    {
                        ConstructionProjectId = dto.ProjectId,
                        ItemCategory = !string.IsNullOrWhiteSpace(row.Category) ? row.Category : "Söküm & Hurda",
                        ItemName = displayName,
                        EstimatedQuantity = totalQty,
                        Unit = !string.IsNullOrWhiteSpace(row.Unit) ? row.Unit : "Adet",
                        EstimatedUnitPrice = row.UnitPrice,
                        Status = "Binada Takılı",
                        Notes = notes
                    });
                }
            }

            // Yapısal Kırım Hurda İnşaat Demiri
            if (dto.CalculateRebarScrap && dto.TotalBuildingM2 > 0)
            {
                decimal totalTon = Math.Round((dto.TotalBuildingM2 * dto.RebarKgPerM2) / 1000m, 1);
                if (totalTon > 0)
                {
                    newItems.Add(new DemolitionSalvageItem
                    {
                        ConstructionProjectId = dto.ProjectId,
                        ItemCategory = "Yapısal Hurda",
                        ItemName = "Hurda İnşaat Demiri (Beton Kırımı)",
                        EstimatedQuantity = totalTon,
                        Unit = "Ton",
                        EstimatedUnitPrice = 14500m,
                        Status = "Binada Takılı",
                        Notes = $"Toplam {dto.TotalBuildingM2:N0} m2 x {dto.RebarKgPerM2} kg/m2 beton kırımından ayrışacak hurda demir"
                    });
                }
            }

            _context.DemolitionSalvageItems.AddRange(newItems);
            await _context.SaveChangesAsync();

            decimal grandTotal = newItems.Sum(i => i.EstimatedQuantity * (i.EstimatedUnitPrice ?? 0));

            return Json(new
            {
                success = true,
                message = "Söküm ve hurda envanteri başarıyla oluşturuldu!",
                totalItems = newItems.Count,
                grandTotal = grandTotal,
                redirectUrl = Url.Action("Index", "DemolitionSalvage", new { projectId = dto.ProjectId })
            });
        }
    }

    public class SalvageWizardGridDto
    {
        public int ProjectId { get; set; }
        public int? BuildingId { get; set; }
        public int FloorMultiplier { get; set; } = 1; // Normal Kat Sayısı
        
        public bool CalculateRebarScrap { get; set; } = true;
        public decimal TotalBuildingM2 { get; set; } = 0;
        public decimal RebarKgPerM2 { get; set; } = 25m;

        public List<SalvageGridRowDto> Rows { get; set; } = new();
    }

    public class SalvageGridRowDto
    {
        public string Category { get; set; } = "Genel";
        public string ItemName { get; set; } = "";
        public string? ItemType { get; set; }
        public decimal QuantityPerFloor { get; set; }
        public string Unit { get; set; } = "Adet";
        public int Multiplier { get; set; } = 1; // Kaç kat
        public decimal UnitPrice { get; set; }
        public string? Notes { get; set; }
    }
}


