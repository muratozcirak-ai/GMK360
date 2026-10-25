using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Construction;
using GMK360.Web.Models;

namespace GMK360.Web.Controllers
{
    public partial class ConstructionProjectController : Controller
    {
        // GET: ConstructionProject/UnitMaterialSelection/5
        [HttpGet]
        public async Task<IActionResult> UnitMaterialSelection(int id)
        {
            var unit = await _context.BuildingUnits
                .Include(u => u.Building)
                    .ThenInclude(b => b.ConstructionProject)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unit == null || unit.Building?.ConstructionProject == null)
            {
                TempData["ErrorMessage"] = "Daire veya bağlı olduğu proje bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var project = unit.Building.ConstructionProject;
            var agencyId = await GetUserAgencyIdAsync();
            if (!User.IsInRole("Admin") && project.AgencyId != agencyId)
            {
                return Unauthorized();
            }

            ViewBag.ProjectId = project.Id;
            ViewBag.BuildingId = unit.BuildingId;

            var catalogItems = await _context.ProjectMaterialCatalogs
                .Where(c => c.ConstructionProjectId == project.Id)
                .OrderBy(c => c.Category)
                .ThenBy(c => c.PriceDifference)
                .ToListAsync();

            var selections = await _context.UnitMaterialSelections
                .Include(s => s.ProjectMaterialCatalog)
                .Include(s => s.SelectedByUser)
                .Where(s => s.BuildingUnitId == id)
                .ToListAsync();

            var groupedCatalog = catalogItems
                .GroupBy(c => c.Category)
                .ToDictionary(g => g.Key, g => g.ToList());

            var selectionsByCategory = new Dictionary<string, UnitMaterialSelection>();
            foreach (var s in selections)
            {
                string cat = s.Category ?? s.ProjectMaterialCatalog?.Category ?? "Genel";
                selectionsByCategory[cat] = s;
            }

            var vm = new UnitMaterialSelectionViewModel
            {
                Unit = unit,
                CatalogItems = catalogItems,
                Selections = selections,
                GroupedCatalog = groupedCatalog,
                SelectionsByCategory = selectionsByCategory,
                TotalCategoriesCount = groupedCatalog.Keys.Count,
                ApprovedCategoriesCount = selections.Count(s => s.IsConfirmedViaSms),
                TotalPriceDifference = selections.Sum(s => s.ApprovedPriceDifference ?? (s.ProjectMaterialCatalog?.PriceDifference ?? 0m))
            };

            return View(vm);
        }

        // POST: ConstructionProject/SendMaterialSelectionOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMaterialSelectionOtp([FromBody] MaterialOtpSendRequest req, [FromServices] GMK360.Web.Services.Sms.ISmsService smsService)
        {
            if (req == null || req.UnitId <= 0 || req.CatalogId <= 0 || string.IsNullOrWhiteSpace(req.Phone))
            {
                return Json(new { success = false, message = "Eksik parametre gönderildi." });
            }

            var catalog = await _context.ProjectMaterialCatalogs.FindAsync(req.CatalogId);
            if (catalog == null)
            {
                return Json(new { success = false, message = "Seçilen malzeme kataloğu bulunamadı." });
            }

            var unit = await _context.BuildingUnits.FindAsync(req.UnitId);
            if (unit == null)
            {
                return Json(new { success = false, message = "Daire bulunamadı." });
            }

            string phone = req.Phone.Trim().Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "");
            string otp = new Random().Next(1000, 9999).ToString();

            var selection = await _context.UnitMaterialSelections
                .Include(s => s.ProjectMaterialCatalog)
                .FirstOrDefaultAsync(s => s.BuildingUnitId == req.UnitId &&
                    (s.Category == catalog.Category || (s.ProjectMaterialCatalog != null && s.ProjectMaterialCatalog.Category == catalog.Category)));

            if (selection == null)
            {
                selection = new UnitMaterialSelection
                {
                    BuildingUnitId = req.UnitId,
                    ProjectMaterialCatalogId = req.CatalogId,
                    Category = catalog.Category,
                    CreatedAt = DateTime.Now
                };
                _context.UnitMaterialSelections.Add(selection);
            }
            else
            {
                selection.ProjectMaterialCatalogId = req.CatalogId;
                selection.Category = catalog.Category;
            }

            selection.VerificationPhone = phone;
            selection.OtpCode = otp;
            selection.SelectionStatus = "SmsBekliyor";
            selection.IsConfirmedViaSms = false;
            selection.SelectionDate = DateTime.Now;

            await _context.SaveChangesAsync();

            // SMS Gönderimi
            string smsMsg = $"GMK360 Malzeme Tercih Doğrulama Kodunuz: {otp}. Seçilen: {catalog.MaterialName}. Onaylamak için lütfen bu kodu ekrana giriniz.";
            await smsService.SmsGonderAsync(phone, smsMsg);

            return Json(new
            {
                success = true,
                message = $"{phone} numarasına 4 haneli SMS doğrulama kodu gönderildi.",
                otpPreview = otp
            });
        }

        // POST: ConstructionProject/ConfirmMaterialSelectionOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmMaterialSelectionOtp([FromBody] MaterialOtpVerifyRequest req)
        {
            if (req == null || req.UnitId <= 0 || req.CatalogId <= 0 || string.IsNullOrWhiteSpace(req.Otp))
            {
                return Json(new { success = false, message = "Eksik parametre gönderildi." });
            }

            var catalog = await _context.ProjectMaterialCatalogs.FindAsync(req.CatalogId);
            if (catalog == null)
            {
                return Json(new { success = false, message = "Katalog öğesi bulunamadı." });
            }

            var selection = await _context.UnitMaterialSelections
                .Include(s => s.ProjectMaterialCatalog)
                .FirstOrDefaultAsync(s => s.BuildingUnitId == req.UnitId &&
                    (s.Category == catalog.Category || (s.ProjectMaterialCatalog != null && s.ProjectMaterialCatalog.Category == catalog.Category)));

            if (selection == null || string.IsNullOrWhiteSpace(selection.OtpCode) || selection.OtpCode.Trim() != req.Otp.Trim())
            {
                return Json(new { success = false, message = "SMS doğrulama kodu hatalı veya uyuşmuyor!" });
            }

            var user = await _userManager.GetUserAsync(User);

            // Anayasa Madde 8: Date ve Time ayrı sütunlarda kaydedilir
            selection.IsConfirmedViaSms = true;
            selection.ConfirmedDate = DateTime.Today;
            selection.ConfirmedTime = DateTime.Now.TimeOfDay;
            selection.SelectionStatus = "MusteriOnayli";
            selection.IsApprovedByAdmin = true;
            if (user != null)
            {
                selection.SelectedByUserId = user.Id;
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Seçiminiz SMS ile başarıyla mühürlendi ve resmi kayıtlara alındı!",
                confirmedDate = selection.ConfirmedDate.Value.ToString("dd.MM.yyyy"),
                confirmedTime = selection.ConfirmedTime.Value.ToString(@"hh\:mm"),
                materialName = catalog.MaterialName
            });
        }

        // POST: ConstructionProject/SubmitCustomMaterialRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitCustomMaterialRequest(int unitId, string category, string customNotes, decimal? priceDifference, IFormFile? customImage)
        {
            var unit = await _context.BuildingUnits.FindAsync(unitId);
            if (unit == null) return NotFound("Daire bulunamadı.");

            string? imageUrl = null;
            if (customImage != null && customImage.Length > 0)
            {
                var uploadDir = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "materials", "requests");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(customImage.FileName)}";
                var filePath = Path.Combine(uploadDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await customImage.CopyToAsync(stream);
                }
                imageUrl = $"/uploads/materials/requests/{fileName}";
            }

            var selection = await _context.UnitMaterialSelections
                .Include(s => s.ProjectMaterialCatalog)
                .FirstOrDefaultAsync(s => s.BuildingUnitId == unitId &&
                    (s.Category == category || (s.ProjectMaterialCatalog != null && s.ProjectMaterialCatalog.Category == category)));

            if (selection == null)
            {
                selection = new UnitMaterialSelection
                {
                    BuildingUnitId = unitId,
                    Category = category,
                    CreatedAt = DateTime.Now
                };
                _context.UnitMaterialSelections.Add(selection);
            }

            selection.Category = category;
            selection.ProjectMaterialCatalogId = null;
            if (!string.IsNullOrEmpty(imageUrl)) selection.CustomRequestImageUrl = imageUrl;
            selection.CustomRequestNotes = customNotes;
            selection.ApprovedPriceDifference = priceDifference ?? 0m;
            selection.SelectionStatus = "FiyatBekliyor";
            selection.IsConfirmedViaSms = false;
            selection.SelectionDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{category}' için özel malzeme talebiniz kaydedildi. Müteahhit fiyat analizi incelemeye alındı.";
            return RedirectToAction(nameof(UnitMaterialSelection), new { id = unitId });
        }

        // POST: ConstructionProject/SubmitShowroomSelection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitShowroomSelection(int unitId, string category, string supplierName, string productCode, decimal? priceDifference)
        {
            var unit = await _context.BuildingUnits.FindAsync(unitId);
            if (unit == null) return NotFound("Daire bulunamadı.");

            var selection = await _context.UnitMaterialSelections
                .Include(s => s.ProjectMaterialCatalog)
                .FirstOrDefaultAsync(s => s.BuildingUnitId == unitId &&
                    (s.Category == category || (s.ProjectMaterialCatalog != null && s.ProjectMaterialCatalog.Category == category)));

            if (selection == null)
            {
                selection = new UnitMaterialSelection
                {
                    BuildingUnitId = unitId,
                    Category = category,
                    CreatedAt = DateTime.Now
                };
                _context.UnitMaterialSelections.Add(selection);
            }

            selection.Category = category;
            selection.ProjectMaterialCatalogId = null;
            selection.ShowroomSupplierName = supplierName;
            selection.ShowroomProductCode = productCode;
            selection.ApprovedPriceDifference = priceDifference ?? 0m;
            selection.SelectionStatus = "ShowroomSecimi";
            selection.IsConfirmedViaSms = false;
            selection.SelectionDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{category}' için showroom seçiminiz ({supplierName} - {productCode}) sisteme işlendi.";
            return RedirectToAction(nameof(UnitMaterialSelection), new { id = unitId });
        }

        // POST: ConstructionProject/DirectConfirmSelection (Hızlı Müteahhit Onayı)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DirectConfirmSelection(int unitId, int catalogId)
        {
            var catalog = await _context.ProjectMaterialCatalogs.FindAsync(catalogId);
            if (catalog == null) return NotFound();

            var selection = await _context.UnitMaterialSelections
                .Include(s => s.ProjectMaterialCatalog)
                .FirstOrDefaultAsync(s => s.BuildingUnitId == unitId &&
                    (s.Category == catalog.Category || (s.ProjectMaterialCatalog != null && s.ProjectMaterialCatalog.Category == catalog.Category)));

            if (selection == null)
            {
                selection = new UnitMaterialSelection
                {
                    BuildingUnitId = unitId,
                    ProjectMaterialCatalogId = catalogId,
                    Category = catalog.Category,
                    CreatedAt = DateTime.Now
                };
                _context.UnitMaterialSelections.Add(selection);
            }
            else
            {
                selection.ProjectMaterialCatalogId = catalogId;
                selection.Category = catalog.Category;
            }

            var user = await _userManager.GetUserAsync(User);
            selection.IsConfirmedViaSms = true;
            selection.ConfirmedDate = DateTime.Today;
            selection.ConfirmedTime = DateTime.Now.TimeOfDay;
            selection.SelectionStatus = "MusteriOnayli";
            selection.IsApprovedByAdmin = true;
            if (user != null) selection.SelectedByUserId = user.Id;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{catalog.MaterialName}' seçimi doğrudan onaylandı ve mühürlendi.";
            return RedirectToAction(nameof(UnitMaterialSelection), new { id = unitId });
        }

        // GET: ConstructionProject/UnitMaterialReport?buildingId=89
        [HttpGet]
        public async Task<IActionResult> UnitMaterialReport(int buildingId)
        {
            var building = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .Include(b => b.Units)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (building == null || building.ConstructionProject == null)
            {
                TempData["ErrorMessage"] = "Bina veya bağlı olduğu proje bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var agencyId = await GetUserAgencyIdAsync();
            if (!User.IsInRole("Admin") && building.ConstructionProject.AgencyId != agencyId)
            {
                return Unauthorized();
            }

            ViewBag.ProjectId = building.ConstructionProjectId;
            ViewBag.BuildingId = building.Id;

            var unitIds = building.Units.Select(u => u.Id).ToList();
            var selections = await _context.UnitMaterialSelections
                .Include(s => s.ProjectMaterialCatalog)
                .Include(s => s.SelectedByUser)
                .Where(s => unitIds.Contains(s.BuildingUnitId))
                .ToListAsync();

            var categories = await _context.ProjectMaterialCatalogs
                .Where(c => c.ConstructionProjectId == building.ConstructionProjectId)
                .Select(c => c.Category)
                .Distinct()
                .ToListAsync();

            if (!categories.Any())
            {
                categories = new List<string>
                {
                    "Salon & Odalar Zemin (Laminant Parke)",
                    "Mutfak Dolap Kapakları & Gövde",
                    "Mutfak Tezgahı & Alın Paneli",
                    "Banyo Zemin & Duvar Seramikleri",
                    "İç Oda Kapıları"
                };
            }

            var selectionsMap = new Dictionary<int, Dictionary<string, UnitMaterialSelection>>();
            foreach (var u in building.Units.OrderBy(u => u.FloorLevel).ThenBy(u => u.DoorNumber))
            {
                var map = new Dictionary<string, UnitMaterialSelection>();
                var uSelections = selections.Where(s => s.BuildingUnitId == u.Id).ToList();
                foreach (var s in uSelections)
                {
                    string cat = s.Category ?? s.ProjectMaterialCatalog?.Category ?? "Genel";
                    map[cat] = s;
                }
                selectionsMap[u.Id] = map;
            }

            var countsMap = new Dictionary<string, int>();
            foreach (var s in selections)
            {
                string mat = s.ProjectMaterialCatalog?.MaterialName
                    ?? (!string.IsNullOrEmpty(s.ShowroomProductCode) ? $"{s.ShowroomSupplierName} ({s.ShowroomProductCode})" : "Özel İstek");
                if (!countsMap.ContainsKey(mat)) countsMap[mat] = 0;
                countsMap[mat]++;
            }

            var vm = new UnitMaterialReportViewModel
            {
                Building = building,
                Units = building.Units.OrderBy(u => u.FloorLevel).ThenBy(u => u.DoorNumber).ToList(),
                AllSelections = selections,
                Categories = categories,
                SelectionsByUnitAndCategory = selectionsMap,
                SelectionsCountByMaterial = countsMap
            };

            return View(vm);
        }
    }
}
