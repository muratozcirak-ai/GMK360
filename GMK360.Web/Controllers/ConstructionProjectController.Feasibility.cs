using GMK360.Core.DTOs.Construction;
﻿﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Construction;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
namespace GMK360.Web.Controllers
{
    public partial class ConstructionProjectController : Controller
    {
        // GET: /ConstructionProject/Feasibility/5
        // KÜNYE İLE RESMİ BÜROKRASİ ARASINDAKİ HIZLI ARSA ÖN ETÜDÜ & SAHA KEŞİF MASASI
        public async Task<IActionResult> Feasibility(int? id)
        {
            if (id == null) return NotFound();
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .Include(p => p.BinaYoneticisi)
                .Include(p => p.Agency)
                .FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
                        // Ortak evrak yolu eşleştirmesi (varsa)
            if (project.PartnerList.Count == 1 && string.IsNullOrWhiteSpace(project.PartnerList[0].ContractDocumentPath) && !string.IsNullOrWhiteSpace(project.PartnerContractDocumentPath))
            {
                project.PartnerList[0].ContractDocumentPath = project.PartnerContractDocumentPath;
                project.PartnerList[0].ContractDocumentName = System.IO.Path.GetFileName(project.PartnerContractDocumentPath);
            }
            ViewData["ProjectId"] = project.Id;
            ViewData["ProjectName"] = project.Name;
            ViewData["ProjectStatus"] = project.Status;
            if (project.CityId.HasValue)
            {
                var city = await _context.Cities.FirstOrDefaultAsync(c => c.Id == project.CityId.Value);
                ViewBag.CityName = city?.Name;
            }
            if (project.DistrictId.HasValue)
            {
                var dist = await _context.Districts.FirstOrDefaultAsync(d => d.Id == project.DistrictId.Value);
                ViewBag.DistrictName = dist?.Name;
            }
            if (project.NeighborhoodId.HasValue)
            {
                var neigh = await _context.Neighborhoods.FirstOrDefaultAsync(n => n.Id == project.NeighborhoodId.Value);
                ViewBag.NeighborhoodName = neigh?.Name;
                ViewBag.AiReport = neigh?.AiAnalysisReport;
                ViewBag.AiUpdatedAt = neigh?.AiAnalysisUpdatedAt;
            }
            // Eski Projelerden Referans Maliyet Hatırlatıcısı
            var lastProject = await _context.ConstructionProjects
                .Where(p => p.AgencyId == agencyId.Value && p.Id != id.Value && !p.IsDeleted && (p.Status == GMK360.Core.Entities.Construction.ProjectStatus.Aktif_Santiye || p.Status == GMK360.Core.Entities.Construction.ProjectStatus.Tamamlandi_Teslim))
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();
            ViewBag.LastProjectName = lastProject?.Name ?? "Önceki Referans Proje";
            decimal baselineGovCost = 18500m;
            decimal baselineFirmCost = 25000m;
            try
            {
                var strategyFile = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", "strategy", $"strategy_baseline_{agencyId.Value}.json");
                if (System.IO.File.Exists(strategyFile))
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(await System.IO.File.ReadAllTextAsync(strategyFile));
                    if (doc.RootElement.TryGetProperty("GovUnitCostPerSqm", out var gEl) && gEl.TryGetDecimal(out var gVal) && gVal > 0)
                        baselineGovCost = gVal;
                    if (doc.RootElement.TryGetProperty("FirmUnitCostPerSqm", out var fEl) && fEl.TryGetDecimal(out var fVal) && fVal > 0)
                        baselineFirmCost = fVal;
                }
            }
            catch { }
            ViewBag.MinistryUnitCost = baselineGovCost;
            ViewBag.MinistryCostPerM2 = (double)baselineGovCost;
            ViewBag.DefaultFirmUnitCostPerSqm = baselineFirmCost;
            ViewBag.LastExcavationCostPerM3 = 280m; // 280 ₺/m³ hafriyat kazı ve nakliye
            ViewBag.LastRoughConstructionCostPerM2 = 9800m; // 9.800 ₺/m² kaba inşaat
            if (project.Latitude.HasValue && project.Longitude.HasValue)
            {
                try
                {
                    var tkgmService = HttpContext.RequestServices.GetRequiredService<GMK360.Core.Interfaces.ITkgmService>();
                    var parcelRes = await tkgmService.GetParcelByCoordinatesAsync(project.Latitude.Value, project.Longitude.Value);
                    if (parcelRes != null && parcelRes.Success && !string.IsNullOrWhiteSpace(parcelRes.GeoJson))
                    {
                        ViewBag.TkgmPolygonGeoJson = parcelRes.GeoJson;
                    }
                }
                catch { }
            }
            return View(project);
        }
        // GET: /ConstructionProject/CadastralMap/5
        [HttpGet]
        public async Task<IActionResult> CadastralMap(int id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
            if (project.CityId.HasValue)
            {
                var city = await _context.Cities.FirstOrDefaultAsync(c => c.Id == project.CityId.Value);
                ViewBag.CityName = city?.Name;
            }
            if (project.DistrictId.HasValue)
            {
                var dist = await _context.Districts.FirstOrDefaultAsync(d => d.Id == project.DistrictId.Value);
                ViewBag.DistrictName = dist?.Name;
            }
            if (project.NeighborhoodId.HasValue)
            {
                var neigh = await _context.Neighborhoods.FirstOrDefaultAsync(n => n.Id == project.NeighborhoodId.Value);
                ViewBag.NeighborhoodName = neigh?.Name;
            }
            if (project.StreetId.HasValue)
            {
                var street = await _context.Streets.FirstOrDefaultAsync(s => s.Id == project.StreetId.Value);
                ViewBag.StreetName = street?.Name;
            }
            return View(project);
        }
        // POST: /ConstructionProject/UpdateProjectDecision
        [HttpPost]
        public async Task<IActionResult> UpdateProjectDecision(int projectId, int status, string? reason)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return Json(new { success = false, message = "Proje bulunamadı." });
            project.Status = (GMK360.Core.Entities.Construction.ProjectStatus)status;
            if (!string.IsNullOrWhiteSpace(reason))
            {
                var cleanReason = reason.Trim();
                project.Description = string.IsNullOrWhiteSpace(project.Description)
                    ? $"[Saha Kararı ({DateTime.Now:dd.MM.yyyy})]: {cleanReason}"
                    : $"{project.Description}\n[Saha Kararı ({DateTime.Now:dd.MM.yyyy})]: {cleanReason}";
            }
            await _context.SaveChangesAsync();
            return Json(new { 
                success = true, 
                newStatus = (int)project.Status, 
                statusName = project.Status.ToString() 
            });
        }
        // GET: /ConstructionProject/Workspace/5
        public async Task<IActionResult> Workspace(int? id, string? tab = "kunye")
        {
            if (id == null) return NotFound();
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .Include(p => p.Blocks).ThenInclude(b => b.ChildBuildings).ThenInclude(c => c.Units)
                .Include(p => p.Blocks).ThenInclude(b => b.Units)
                .Include(p => p.BinaYoneticisi)
                .FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
            ViewBag.ActiveTab = string.IsNullOrWhiteSpace(tab) ? "kunye" : tab.ToLower();
            // 1. Hak Sahipleri
            var stakeholders = await _context.ProjectStakeholders
                .Include(s => s.User)
                .Where(s => s.ProjectId == id && !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
            ViewBag.Stakeholders = stakeholders;
            // 1.1 Kira & Taşınma Bedel Politikası
            GMK360.Core.DTOs.Construction.ProjectRentPolicyDto rentPolicy = null;
            if (!string.IsNullOrWhiteSpace(project.RentPolicyJson))
            {
                try
                {
                    rentPolicy = System.Text.Json.JsonSerializer.Deserialize<GMK360.Core.DTOs.Construction.ProjectRentPolicyDto>(project.RentPolicyJson);
                }
                catch { }
            }
            if (rentPolicy == null)
            {
                rentPolicy = new GMK360.Core.DTOs.Construction.ProjectRentPolicyDto
                {
                    FlatStartDate = project.StartDate > DateTime.MinValue ? project.StartDate : DateTime.Today,
                    FlatEndDate = project.EndDate > DateTime.MinValue ? project.EndDate : DateTime.Today.AddMonths(18),
                    ShopStartDate = project.StartDate > DateTime.MinValue ? project.StartDate : DateTime.Today,
                    ShopEndDate = project.EndDate > DateTime.MinValue ? project.EndDate : DateTime.Today.AddMonths(18)
                };
            }
            ViewBag.RentPolicy = rentPolicy;
            // 2. Çizimler / DWG & Paftalar
            var drawings = await _context.DocumentArchives
                .Where(d => d.ProjectId == id && !d.IsDeleted)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
            ViewBag.Drawings = drawings;
            // 3. Konum SelectList'leri
            var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Cities = new SelectList(cities, "Id", "Name", project.CityId);
            if (project.CityId.HasValue)
            {
                var districts = await _context.Districts.Where(d => d.CityId == project.CityId.Value).OrderBy(d => d.Name).ToListAsync();
                ViewBag.Districts = new SelectList(districts, "Id", "Name", project.DistrictId);
            }
            else
            {
                ViewBag.Districts = new SelectList(Enumerable.Empty<SelectListItem>());
            }
            if (project.DistrictId.HasValue)
            {
                var neighborhoods = await _context.Neighborhoods.Where(n => n.DistrictId == project.DistrictId.Value).OrderBy(n => n.Name).ToListAsync();
                ViewBag.Neighborhoods = new SelectList(neighborhoods, "Id", "Name", project.NeighborhoodId);
            }
            else
            {
                ViewBag.Neighborhoods = new SelectList(Enumerable.Empty<SelectListItem>());
            }
            if (project.NeighborhoodId.HasValue)
            {
                var streets = await _context.Streets.Where(s => s.NeighborhoodId == project.NeighborhoodId.Value).OrderBy(s => s.Name).ToListAsync();
                ViewBag.Streets = new SelectList(streets, "Id", "Name", project.StreetId);
            }
            else
            {
                ViewBag.Streets = new SelectList(Enumerable.Empty<SelectListItem>());
            }
            var tapuDoc = await _context.DocumentArchives
                .Where(d => d.ProjectId == id && (d.Title == "Tapu Belgesi" || (d.Title != null && d.Title.Contains("Tapu"))) && !d.IsDeleted)
                .OrderByDescending(d => d.UploadDate)
                .FirstOrDefaultAsync();
            ViewBag.TapuDocumentUrl = tapuDoc?.DocumentUrl;

            var binaFotoDoc = await _context.DocumentArchives
                .Where(d => d.ProjectId == id && (d.Category == "Bina Görselleri" || (d.Title != null && (d.Title.Contains("Bina Cephe") || d.Title.Contains("Sokak Foto")))) && !d.IsDeleted)
                .OrderByDescending(d => d.UploadDate)
                .FirstOrDefaultAsync();
            ViewBag.CurrentStateImageUrl = !string.IsNullOrWhiteSpace(project.CurrentStateImageUrl) ? project.CurrentStateImageUrl : binaFotoDoc?.DocumentUrl;

            var imarDoc = await _context.DocumentArchives
                .Where(d => d.ProjectId == id && (d.Title == "İmar Durum Belgesi (İmar Çapı)" || (d.Title != null && d.Title.Contains("İmar"))) && !d.IsDeleted)
                .OrderByDescending(d => d.UploadDate)
                .FirstOrDefaultAsync();
            ViewBag.ImarCapiDocumentUrl = imarDoc?.DocumentUrl ?? rentPolicy.ImarCapiDocumentUrl;
            var kararDoc = await _context.DocumentArchives
                .Where(d => d.ProjectId == id && (d.Title == "Apartman Karar Defteri / Ön Uzlaşı" || (d.Title != null && d.Title.Contains("Karar Defteri"))) && !d.IsDeleted)
                .OrderByDescending(d => d.UploadDate)
                .FirstOrDefaultAsync();
            ViewBag.KararDefteriDocumentUrl = kararDoc?.DocumentUrl ?? rentPolicy.KararDefteriDocumentUrl;
            return View(project);
        }
        // POST: /ConstructionProject/UpdateProjectKunye
        [HttpPost]
        public async Task<IActionResult> UpdateProjectKunye(
            int id,
            string name,
            string? description,
            string? projectType,
            int? projectOriginId,
            int? cityId,
            int? districtId,
            int? neighborhoodId,
            int? streetId,
            string? address,
            string? ada,
            string? parsel,
            double? totalLandArea,
            double? latitude,
            double? longitude,
            int? buildingAge,
            int? eskiKatSayisi,
            int? eskiDaireSayisi,
            int? eskiDukkanSayisi,
            double? eskiToplamMetrekare,
            DateTime startDate,
            DateTime? endDate,
            ProjectStatus status,
            IFormFile? currentStateImageFile,
            IFormFile? tapuDocumentFile,
            string? buildingOrderType,
            int? ownerPreConsensusRate,
            IFormFile? imarCapiDocumentFile,
            IFormFile? kararDefteriDocumentFile)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
            project.Name = string.IsNullOrWhiteSpace(name) ? project.Name : name.Trim();
            project.Description = description?.Trim();
            project.ProjectType = projectType?.Trim();
            project.ProjectOriginId = projectOriginId ?? 1;
            project.CityId = cityId;
            project.DistrictId = districtId;
            project.NeighborhoodId = neighborhoodId;
            project.StreetId = streetId;
            project.Address = address?.Trim() ?? "";
            project.Ada = ada?.Trim();
            project.Parsel = parsel?.Trim();
            project.TotalLandArea = totalLandArea;
            project.Latitude = latitude;
            project.Longitude = longitude;
            project.BuildingAge = buildingAge;
            project.EskiKatSayisi = eskiKatSayisi;
            project.EskiDaireSayisi = eskiDaireSayisi;
            project.EskiDukkanSayisi = eskiDukkanSayisi;
            project.EskiToplamMetrekare = eskiToplamMetrekare;
            project.StartDate = startDate;
            project.EndDate = endDate;
            project.Status = status;
            // Fotoğraf yükleme (4. Altın Evrak: Bina 4 Cephe ve Sokak Fotoğrafları)
            if (currentStateImageFile != null && currentStateImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", $"Agency_{agencyId.Value}", $"Project_{project.Id}", "images");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var extension = Path.GetExtension(currentStateImageFile.FileName).ToLower();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
                if (allowed.Contains(extension))
                {
                    var fileName = $"mevcut_durum_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await currentStateImageFile.CopyToAsync(stream);
                    }
                    project.CurrentStateImageUrl = $"/uploads/Agency_{agencyId.Value}/Project_{project.Id}/images/{fileName}";
                }
            }
            // 1. Altın Evrak: Tapu Belgesi yükleme
            if (tapuDocumentFile != null && tapuDocumentFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", $"Agency_{agencyId.Value}", $"Project_{project.Id}", "documents");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var extension = Path.GetExtension(tapuDocumentFile.FileName).ToLower();
                var standardizedName = $"Tapu_Belgesi_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                var filePath = Path.Combine(uploadsFolder, standardizedName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await tapuDocumentFile.CopyToAsync(stream);
                }
                var archive = new GMK360.Core.Entities.DocumentArchive
                {
                    AgencyId = agencyId.Value,
                    ProjectId = project.Id,
                    Title = "Tapu Belgesi",
                    Category = "Resmi Evraklar",
                    SourceModule = "Construction",
                    DocumentUrl = $"/uploads/Agency_{agencyId.Value}/Project_{project.Id}/documents/{standardizedName}",
                    FileName = standardizedName,
                    FileExtension = extension,
                    Year = DateTime.Now.Year,
                    UploadDate = DateTime.UtcNow,
                    Status = "Tamamlandı"
                };
                _context.DocumentArchives.Add(archive);
            }

            // 4 Altın Evrak & İlk Kabul Verileri (RentPolicyJson İçinde Senkronizasyon)
            var policy = project.RentAndRontgenPolicy;
            if (!string.IsNullOrWhiteSpace(buildingOrderType))
            {
                policy.BuildingOrderType = buildingOrderType.Trim();
                if (buildingOrderType.Contains("Ayrık", StringComparison.OrdinalIgnoreCase))
                    policy.AdjacentBuildingRisk = "AyrikRiskYok";
                else if (buildingOrderType.Contains("İki", StringComparison.OrdinalIgnoreCase))
                    policy.AdjacentBuildingRisk = "IkiYandanBitisikEskiBina";
                else
                    policy.AdjacentBuildingRisk = "TekYandanBitisik";
            }
            if (ownerPreConsensusRate.HasValue)
            {
                policy.OwnerPreConsensusRate = Math.Clamp(ownerPreConsensusRate.Value, 0, 100);
            }

            var allowedDocExts = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };

            // 2. Altın Evrak: Belediye Güncel İmar Durum Belgesi (İmar Çapı)
            if (imarCapiDocumentFile != null && imarCapiDocumentFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", $"Agency_{agencyId.Value}", $"Project_{project.Id}", "documents");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var extension = Path.GetExtension(imarCapiDocumentFile.FileName).ToLower();
                if (allowedDocExts.Contains(extension))
                {
                    var standardizedName = $"Imar_Capi_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                    var filePath = Path.Combine(uploadsFolder, standardizedName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imarCapiDocumentFile.CopyToAsync(stream);
                    }
                    var docUrl = $"/uploads/Agency_{agencyId.Value}/Project_{project.Id}/documents/{standardizedName}";
                    policy.ImarCapiDocumentUrl = docUrl;
                    _context.DocumentArchives.Add(new GMK360.Core.Entities.DocumentArchive
                    {
                        AgencyId = agencyId.Value,
                        ProjectId = project.Id,
                        Title = "İmar Durum Belgesi (İmar Çapı)",
                        Category = "Resmi Evraklar",
                        SourceModule = "Construction",
                        DocumentUrl = docUrl,
                        FileName = standardizedName,
                        FileExtension = extension,
                        Year = DateTime.Now.Year,
                        UploadDate = DateTime.UtcNow,
                        Status = "Tamamlandı"
                    });
                }
            }

            // 3. Altın Evrak: Apartman Karar Defteri / Malik Ön Uzlaşı Belgesi
            if (kararDefteriDocumentFile != null && kararDefteriDocumentFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", $"Agency_{agencyId.Value}", $"Project_{project.Id}", "documents");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var extension = Path.GetExtension(kararDefteriDocumentFile.FileName).ToLower();
                if (allowedDocExts.Contains(extension))
                {
                    var standardizedName = $"Karar_Defteri_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                    var filePath = Path.Combine(uploadsFolder, standardizedName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await kararDefteriDocumentFile.CopyToAsync(stream);
                    }
                    var docUrl = $"/uploads/Agency_{agencyId.Value}/Project_{project.Id}/documents/{standardizedName}";
                    policy.KararDefteriDocumentUrl = docUrl;
                    _context.DocumentArchives.Add(new GMK360.Core.Entities.DocumentArchive
                    {
                        AgencyId = agencyId.Value,
                        ProjectId = project.Id,
                        Title = "Apartman Karar Defteri / Ön Uzlaşı",
                        Category = "Resmi Evraklar",
                        SourceModule = "Construction",
                        DocumentUrl = docUrl,
                        FileName = standardizedName,
                        FileExtension = extension,
                        Year = DateTime.Now.Year,
                        UploadDate = DateTime.UtcNow,
                        Status = "Tamamlandı"
                    });
                }
            }

            project.RentPolicyJson = System.Text.Json.JsonSerializer.Serialize(policy);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Proje künyesi, saha keşfi ve 4 Altın Evrak bilgileri başarıyla güncellendi.";
            return RedirectToAction(nameof(Workspace), new { id = id, tab = "kunye" });
        }
        // POST: /ConstructionProject/UpdateProjectPartners
        [HttpPost]
                // POST: /ConstructionProject/SaveProjectPartner
        // POST: /ConstructionProject/SaveProjectPartner
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProjectPartner(
            int projectId,
            string? partnerId,
            string? companyName,
            string firstName,
            string? lastName,
            decimal sharePercentage,
            string? phone,
            string? email,
            Microsoft.AspNetCore.Http.IFormFile? contractFile)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
            var partners = project.PartnerList;
            var existing = !string.IsNullOrWhiteSpace(partnerId) 
                ? partners.FirstOrDefault(p => p.Id == partnerId) 
                : null;
            decimal otherPartnersTotal = partners.Where(p => p.Id != partnerId).Sum(p => p.SharePercentage);
            if (otherPartnersTotal + sharePercentage > 100m)
            {
                TempData["ErrorMessage"] = $"Ortakların toplam hissesi %100'ü geçemez! Kalan verilebilir en fazla hisse: %{(100m - otherPartnersTotal):0.#}";
                return RedirectToAction(nameof(Workspace), new { id = projectId });
            }
            var partner = existing;
            if (partner == null)
            {
                partner = new GMK360.Core.DTOs.Construction.ProjectPartnerDto
                {
                    Id = Guid.NewGuid().ToString("N")
                };
                partners.Add(partner);
            }
            partner.CompanyName = string.IsNullOrWhiteSpace(companyName) ? null : companyName.Trim();
            partner.FirstName = (firstName ?? "").Trim();
            partner.LastName = (lastName ?? "").Trim();
            partner.SharePercentage = Math.Max(0m, sharePercentage);
            partner.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
            partner.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
            // Evrak / Sözleşme doğrudan bu ortağa kaydedilir
            if (contractFile != null && contractFile.Length > 0)
            {
                try
                {
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "projects", projectId.ToString(), "partners");
                    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
                    var ext = Path.GetExtension(contractFile.FileName).ToLowerInvariant();
                    var cleanName = (partner.FirstName + "_" + partner.LastName).Trim().Replace(" ", "_");
                    var fileName = $"sozlesme_{cleanName}_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";
                    var fullPath = Path.Combine(uploadsDir, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await contractFile.CopyToAsync(stream);
                    }
                    var relativePath = $"/uploads/projects/{projectId}/partners/{fileName}";
                    partner.ContractDocumentPath = relativePath;
                    partner.ContractDocumentName = contractFile.FileName;
                    // Projenin resmi evrak arşivine de otomatik kaydet
                    var docTitle = $"Ortaklık Sözleşmesi - {partner.FullName}";
                    var archiveDoc = await _context.ProjectLegalDocuments
                        .FirstOrDefaultAsync(d => d.ConstructionProjectId == projectId && d.DocumentName == docTitle);
                    if (archiveDoc == null)
                    {
                        archiveDoc = new GMK360.Core.Entities.Construction.ProjectLegalDocument
                        {
                            ConstructionProjectId = projectId,
                            DocumentName = docTitle,
                            Stage = "Sözleşmeler & Ortaklık",
                            Status = "Tamamlandı",
                            CompletedDate = DateTime.Now,
                            FilePath = relativePath,
                            OriginalLocation = "Dijital / Hissedar Künyesi",
                            IsCustom = true,
                            IsActive = true
                        };
                        _context.ProjectLegalDocuments.Add(archiveDoc);
                    }
                    else
                    {
                        archiveDoc.FilePath = relativePath;
                        archiveDoc.Status = "Tamamlandı";
                        archiveDoc.CompletedDate = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Evrak yüklenirken hata oluştu: {ex.Message}";
                }
            }
            project.IsJointVenture = partners.Count > 0;
            project.PartnersJson = System.Text.Json.JsonSerializer.Serialize(partners);
            decimal totalPartnerShare = partners.Sum(p => p.SharePercentage);
            project.PartnerSharePercentage = totalPartnerShare;
            project.OurSharePercentage = 100m - totalPartnerShare;
            if (partners.Count > 0)
            {
                project.PartnerCompanyName = partners[0].CompanyName ?? partners[0].FullName;
                project.PartnerContactPerson = partners[0].FullName;
                project.PartnerPhone = partners[0].Phone;
                project.PartnerEmail = partners[0].Email;
                project.ProjectOwnerContact = partners.Count == 1
                    ? $"Ortak Girişim (%{project.OurSharePercentage:0.#} Biz / %{totalPartnerShare:0.#} {partners[0].DisplayName})"
                    : $"Çoklu Ortaklık (%{project.OurSharePercentage:0.#} Biz / {partners.Count} Ortak: %{totalPartnerShare:0.#})";
            }
            else
            {
                project.PartnerCompanyName = null;
                project.PartnerContactPerson = null;
                project.PartnerPhone = null;
                project.PartnerEmail = null;
                project.PartnersJson = null;
                project.ProjectOwnerContact = "Tekil Yüklenici (%100)";
            }
            // Ortak bilgilerini Merkezi Rehbere (AgencyPhonebooks) otomatik senkronize et (Toplantı & Saha konularında çağrılabilsin)
            if (!string.IsNullOrWhiteSpace(partner.FullName) && !string.IsNullOrWhiteSpace(partner.Phone))
            {
                var pPhoneClean = partner.Phone.Trim();
                var existingPb = await _context.AgencyPhonebooks
                    .FirstOrDefaultAsync(pb => pb.AgencyId == agencyId.Value && !pb.IsDeleted && (pb.PhoneNumber == pPhoneClean || pb.Name == partner.FullName));
                var matchedUser = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == pPhoneClean);
                if (existingPb == null)
                {
                    _context.AgencyPhonebooks.Add(new AgencyPhonebook
                    {
                        AgencyId = agencyId.Value,
                        Name = !string.IsNullOrWhiteSpace(partner.CompanyName) ? $"{partner.FullName} ({partner.CompanyName})" : partner.FullName,
                        PhoneNumber = pPhoneClean,
                        Email = partner.Email,
                        ContactType = 2, // Firma / Ortak
                        Tags = $"Proje Ortağı (%{partner.SharePercentage:0.#})",
                        Notes = $"{project.Name} projesi hissedarı / ortağı olarak künyeden otomatik eklendi.",
                        LinkedUserId = matchedUser?.Id,
                        IsRegistered = matchedUser != null,
                        CreatedAt = DateTime.Now,
                        IsDeleted = false
                    });
                }
                else if (matchedUser != null && string.IsNullOrEmpty(existingPb.LinkedUserId))
                {
                    existingPb.LinkedUserId = matchedUser.Id;
                    existingPb.IsRegistered = true;
                }
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = existing != null 
                ? "Ortak bilgileri güncellendi ve toplantı rehberiyle senkronize edildi." 
                : "Yeni ortak eklendi ve toplantı rehberine otomatik tanımlandı.";
            return RedirectToAction(nameof(Workspace), new { id = projectId });
        }
        // POST: /ConstructionProject/DeleteProjectPartner
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProjectPartner(int projectId, string partnerId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
            var partners = project.PartnerList;
            partners.RemoveAll(p => p.Id == partnerId);
            project.IsJointVenture = partners.Count > 0;
            project.PartnersJson = partners.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(partners) : null;
            decimal totalPartnerShare = partners.Sum(p => p.SharePercentage);
            project.PartnerSharePercentage = totalPartnerShare;
            project.OurSharePercentage = 100m - totalPartnerShare;
            if (partners.Count > 0)
            {
                project.PartnerCompanyName = partners[0].CompanyName ?? partners[0].FullName;
                project.PartnerContactPerson = partners[0].FullName;
                project.PartnerPhone = partners[0].Phone;
                project.PartnerEmail = partners[0].Email;
                project.ProjectOwnerContact = partners.Count == 1
                    ? $"Ortak Girişim (%{project.OurSharePercentage:0.#} Biz / %{totalPartnerShare:0.#} {partners[0].DisplayName})"
                    : $"Çoklu Ortaklık (%{project.OurSharePercentage:0.#} Biz / {partners.Count} Ortak: %{totalPartnerShare:0.#})";
            }
            else
            {
                project.PartnerCompanyName = null;
                project.PartnerContactPerson = null;
                project.PartnerPhone = null;
                project.PartnerEmail = null;
                project.PartnersJson = null;
                project.ProjectOwnerContact = "Tekil Yüklenici (%100)";
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Ortak başarıyla silindi.";
            return RedirectToAction(nameof(Workspace), new { id = projectId });
        }
        // POST: /ConstructionProject/UploadPartnerContract
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPartnerContract(int projectId, Microsoft.AspNetCore.Http.IFormFile contractDocument)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
            if (contractDocument != null && contractDocument.Length > 0)
            {
                try
                {
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "projects", projectId.ToString(), "contracts");
                    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
                    var ext = Path.GetExtension(contractDocument.FileName).ToLowerInvariant();
                    var fileName = $"ortaklik_sozlesmesi_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";
                    var fullPath = Path.Combine(uploadsDir, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await contractDocument.CopyToAsync(stream);
                    }
                    var relativePath = $"/uploads/projects/{projectId}/contracts/{fileName}";
                    project.PartnerContractDocumentPath = relativePath;
                    var archiveDoc = await _context.ProjectLegalDocuments
                        .FirstOrDefaultAsync(d => d.ConstructionProjectId == projectId && d.DocumentName == "Ortaklık / Hissedarlık Sözleşmesi");
                    if (archiveDoc == null)
                    {
                        archiveDoc = new GMK360.Core.Entities.Construction.ProjectLegalDocument
                        {
                            ConstructionProjectId = projectId,
                            DocumentName = "Ortaklık / Hissedarlık Sözleşmesi",
                            Stage = "Sözleşmeler & Ortaklık",
                            Status = "Tamamlandı",
                            CompletedDate = DateTime.Now,
                            FilePath = relativePath,
                            OriginalLocation = "Dijital / Hissedar Künyesi",
                            IsCustom = true,
                            IsActive = true
                        };
                        _context.ProjectLegalDocuments.Add(archiveDoc);
                    }
                    else
                    {
                        archiveDoc.FilePath = relativePath;
                        archiveDoc.Status = "Tamamlandı";
                        archiveDoc.CompletedDate = DateTime.Now;
                    }
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Ortaklık sözleşmesi yüklendi ve projenin resmi evrak arşivine kaydedildi.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Sözleşme yüklenirken hata oluştu: {ex.Message}";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Lütfen yüklenecek bir sözleşme dosyası seçiniz.";
            }
            return RedirectToAction(nameof(Workspace), new { id = projectId });
        }
// POST: /ConstructionProject/UpdateProjectContact
        [HttpPost]
        public async Task<IActionResult> UpdateProjectContact(
            int id, 
            string? contactType, 
            string? firstName, 
            string? lastName, 
            string? phoneNumber, 
            string? email, 
            string? notes)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .Include(p => p.BinaYoneticisi)
                .FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
            var userId = _userManager.GetUserId(User) ?? "";
            if (project.BinaYoneticisi != null)
            {
                project.BinaYoneticisi.ContactType = contactType?.Trim() ?? "Bina Yöneticisi";
                project.BinaYoneticisi.FirstName = firstName?.Trim() ?? "";
                project.BinaYoneticisi.LastName = lastName?.Trim();
                project.BinaYoneticisi.PhoneNumber = phoneNumber?.Trim();
                project.BinaYoneticisi.Email = email?.Trim();
                project.BinaYoneticisi.Notes = notes?.Trim();
            }
            else if (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(phoneNumber))
            {
                var newContact = new GMK360.Core.Entities.CrmContact
                {
                    ApplicationUserId = userId,
                    ContactType = contactType?.Trim() ?? "Bina Yöneticisi",
                    FirstName = firstName?.Trim() ?? "",
                    LastName = lastName?.Trim(),
                    PhoneNumber = phoneNumber?.Trim(),
                    Email = email?.Trim(),
                    Notes = notes?.Trim()
                };
                _context.CrmContacts.Add(newContact);
                await _context.SaveChangesAsync();
                project.BinaYoneticisiId = newContact.Id;
            }
            // Proje Muhatabını Merkezi Rehbere (AgencyPhonebooks) otomatik senkronize et (Toplantı & Saha Masasında hazır çıksın)
            string fullContactName = $"{firstName?.Trim()} {lastName?.Trim()}".Trim();
            string cleanContactPhone = phoneNumber?.Trim() ?? "";
            if (!string.IsNullOrWhiteSpace(fullContactName) && !string.IsNullOrWhiteSpace(cleanContactPhone))
            {
                var existingPb = await _context.AgencyPhonebooks
                    .FirstOrDefaultAsync(pb => pb.AgencyId == agencyId.Value && !pb.IsDeleted && (pb.PhoneNumber == cleanContactPhone || pb.Name == fullContactName));
                var matchedUser = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == cleanContactPhone);
                if (existingPb == null)
                {
                    _context.AgencyPhonebooks.Add(new AgencyPhonebook
                    {
                        AgencyId = agencyId.Value,
                        Name = fullContactName,
                        PhoneNumber = cleanContactPhone,
                        Email = email?.Trim(),
                        ContactType = 4,
                        Tags = contactType?.Trim() ?? "Bina Yöneticisi",
                        Notes = $"{project.Name} proje künyesinden muhatap olarak otomatik eklendi. {notes?.Trim()}".Trim(),
                        LinkedUserId = matchedUser?.Id,
                        IsRegistered = matchedUser != null,
                        CreatedAt = DateTime.Now,
                        IsDeleted = false
                    });
                }
                else
                {
                    existingPb.PhoneNumber = cleanContactPhone;
                    if (!string.IsNullOrWhiteSpace(email)) existingPb.Email = email.Trim();
                    if (matchedUser != null && string.IsNullOrEmpty(existingPb.LinkedUserId))
                    {
                        existingPb.LinkedUserId = matchedUser.Id;
                        existingPb.IsRegistered = true;
                    }
                }
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Proje muhatap bilgileri güncellendi ve toplantı rehberiyle senkronize edildi.";
            return RedirectToAction(nameof(Workspace), new { id = id });
        }
        // POST: /ConstructionProject/UploadWorkspaceDrawing
        [HttpPost]
        public async Task<IActionResult> UploadWorkspaceDrawing(
            int projectId,
            string category,
            string title,
            int? buildingId,
            int? floorLevel,
            IFormFile? drawingFile)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            if (drawingFile == null || drawingFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen bir çizim / pafta dosyası seçin.";
                return RedirectToAction(nameof(Workspace), new { id = projectId, tab = "teknik" });
            }
            try
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "projects", projectId.ToString(), "drawings");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                string ext = Path.GetExtension(drawingFile.FileName).ToLower();
                string safeName = Path.GetFileNameWithoutExtension(drawingFile.FileName).Replace(" ", "_");
                string uniqueFileName = $"{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await drawingFile.CopyToAsync(stream);
                }
                var doc = new GMK360.Core.Entities.DocumentArchive
                {
                    ProjectId = projectId,
                    Title = string.IsNullOrWhiteSpace(title) ? drawingFile.FileName : title.Trim(),
                    Category = category ?? "Mimari",
                    DocumentUrl = $"/uploads/projects/{projectId}/drawings/{uniqueFileName}",
                    SourceModule = "Construction",
                    FileName = uniqueFileName,
                    FileExtension = ext,
                    UploadDate = DateTime.Now,
                    CreatedAt = DateTime.UtcNow
                };
                _context.DocumentArchives.Add(doc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{drawingFile.FileName} çizimi başarıyla teknik ofis arşivine yüklendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Dosya yüklenirken hata: " + ex.Message;
            }
            return RedirectToAction(nameof(Workspace), new { id = projectId, tab = "teknik" });
        }
        // GET: /ConstructionProject/Analysis/5
        public async Task<IActionResult> Analysis(int? id)
        {
            if (id == null) return NotFound();
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .Include(p => p.BinaYoneticisi)
                .FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
            ViewData["ProjectId"] = project.Id;
            ViewData["ProjectName"] = project.Name;
            ViewData["ProjectStatus"] = project.Status;
            // 1. Hak Sahipleri
            var stakeholders = await _context.ProjectStakeholders
                .Include(s => s.User)
                .Where(s => s.ProjectId == id && !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
            ViewBag.Stakeholders = stakeholders;
            // 2. Bloklar ve Bağımsız Bölümler (Eski Bina Durumu)
            var blocks = await _context.Buildings
                .Include(b => b.ChildBuildings).ThenInclude(c => c.Units)
                .Include(b => b.Units)
                .Where(b => b.ConstructionProjectId == id)
                .ToListAsync();
            ViewBag.Blocks = blocks;
            // 3. Tüm Aşamalar: Evrak, Bürokrasi ve Resmi Süreç Masası (Önkoşul Zinciri + Eski Proje / Havuz Fiyatları)
            var allRules = await _context.ModuleDocumentRules
                .Include(r => r.SystemLegalDocumentTemplate)
                .Include(r => r.Prerequisites)
                    .ThenInclude(p => p.PrerequisiteTemplate)
                .Where(r => r.TargetModule == "Construction")
                .OrderBy(r => r.DisplayOrder)
                .ToListAsync();

            var allDocs = await _context.ProjectLegalDocuments
                .Include(d => d.SystemTemplate)
                .Where(d => d.ConstructionProjectId == id)
                .OrderBy(d => d.DisplayOrder)
                .ToListAsync();

            // Eksik evrak veya önkoşul evraklarını otomatik senkronize et
            var existingTmplIds = allDocs.Where(d => d.SystemTemplateId != null).Select(d => d.SystemTemplateId!.Value).ToHashSet();
            bool anyDocAdded = false;
            foreach (var rule in allRules)
            {
                if (rule.SystemLegalDocumentTemplate != null && !existingTmplIds.Contains(rule.SystemLegalDocumentTemplateId))
                {
                    var newDoc = new ProjectLegalDocument
                    {
                        ConstructionProjectId = project.Id,
                        SystemTemplateId = rule.SystemLegalDocumentTemplateId,
                        DocumentName = rule.SystemLegalDocumentTemplate.Name,
                        Stage = rule.Stage,
                        Status = "Bekliyor",
                        DisplayOrder = rule.DisplayOrder,
                        EstimatedCost = 0,
                        ActualCost = 0,
                        DocumentFee = 0,
                        AdditionalCost = 0
                    };
                    _context.ProjectLegalDocuments.Add(newDoc);
                    allDocs.Add(newDoc);
                    existingTmplIds.Add(rule.SystemLegalDocumentTemplateId);
                    anyDocAdded = true;
                }

                if (rule.Prerequisites != null)
                {
                    int subOrder = 1;
                    foreach (var pre in rule.Prerequisites)
                    {
                        if (pre.PrerequisiteTemplate != null && !existingTmplIds.Contains(pre.PrerequisiteTemplateId))
                        {
                            var childDoc = new ProjectLegalDocument
                            {
                                ConstructionProjectId = project.Id,
                                SystemTemplateId = pre.PrerequisiteTemplateId,
                                DocumentName = pre.PrerequisiteTemplate.Name,
                                Stage = rule.Stage,
                                DisplayOrder = rule.DisplayOrder * 10 + subOrder++,
                                Status = "Bekliyor",
                                EstimatedCost = 0,
                                ActualCost = 0,
                                DocumentFee = 0,
                                AdditionalCost = 0
                            };
                            _context.ProjectLegalDocuments.Add(childDoc);
                            allDocs.Add(childDoc);
                            existingTmplIds.Add(pre.PrerequisiteTemplateId);
                            anyDocAdded = true;
                        }
                    }
                }
            }
            if (anyDocAdded)
            {
                await _context.SaveChangesAsync();
            }

            // Eski projelerde gerçekleşen fiyatları veya merkez havuz referans fiyatlarını çek
            var previousAgencyDocs = await _context.ProjectLegalDocuments
                .Include(d => d.ConstructionProject)
                .Where(d => d.ConstructionProjectId != project.Id
                    && d.ConstructionProject != null
                    && d.ConstructionProject.AgencyId == agencyId.Value
                    && ((d.DocumentFee ?? 0) > 0 || (d.AdditionalCost ?? 0) > 0 || (d.ActualCost ?? 0) > 0))
                .OrderByDescending(d => d.Id)
                .ToListAsync();

            var realizedDocIds = new HashSet<int>();
            bool anyPriceUpdated = false;
            foreach (var d in allDocs)
            {
                var oldDoc = previousAgencyDocs.FirstOrDefault(p =>
                    (d.SystemTemplateId.HasValue && p.SystemTemplateId == d.SystemTemplateId)
                    || string.Equals(p.DocumentName, d.DocumentName, StringComparison.OrdinalIgnoreCase));

                if (oldDoc != null)
                {
                    realizedDocIds.Add(d.Id);
                }

                if ((d.DocumentFee ?? 0) == 0 && (d.AdditionalCost ?? 0) == 0 && (d.EstimatedCost ?? 0) == 0)
                {
                    if (oldDoc != null)
                    {
                        decimal oldFee = (oldDoc.DocumentFee ?? 0) > 0 ? oldDoc.DocumentFee!.Value : (oldDoc.ActualCost ?? oldDoc.EstimatedCost ?? 0);
                        d.DocumentFee = oldFee;
                        d.AdditionalCost = oldDoc.AdditionalCost ?? 0;
                        d.EstimatedCost = oldFee + (oldDoc.AdditionalCost ?? 0);
                        if (string.IsNullOrWhiteSpace(d.InstitutionContact) && !string.IsNullOrWhiteSpace(oldDoc.InstitutionContact))
                        {
                            d.InstitutionContact = oldDoc.InstitutionContact;
                        }
                        anyPriceUpdated = true;
                    }
                    else
                    {
                        var n = (d.DocumentName ?? "").ToLowerInvariant();
                        decimal refPrice = 18000m;
                        if (n.Contains("karot") || n.Contains("riskli yapı")) refPrice = 28500m;
                        else if (n.Contains("zemin etüd") || n.Contains("geoteknik")) refPrice = 45000m;
                        else if (n.Contains("mimari proje") || n.Contains("mimari avan")) refPrice = 185000m;
                        else if (n.Contains("statik proje") || n.Contains("betonarme")) refPrice = 140000m;
                        else if (n.Contains("mekanik") || n.Contains("elektrik") || n.Contains("tesisat proje")) refPrice = 95000m;
                        else if (n.Contains("harita") || n.Contains("aplikasyon") || n.Contains("ifraz") || n.Contains("tevhid") || n.Contains("plankote")) refPrice = 32000m;
                        else if (n.Contains("noter") || n.Contains("kat karşılığı") || n.Contains("vekalet")) refPrice = 65000m;
                        else if (n.Contains("tapu") || n.Contains("takyidat") || n.Contains("kat irtifak") || n.Contains("şerh")) refPrice = 42000m;
                        else if (n.Contains("ruhsat") || n.Contains("belediye") || n.Contains("imar durumu")) refPrice = 120000m;
                        else if (n.Contains("yapı denetim")) refPrice = 160000m;
                        else if (n.Contains("iski") || n.Contains("tedaş") || n.Contains("igdaş") || n.Contains("ilişik") || n.Contains("abonelik") || n.Contains("altyapı")) refPrice = 18500m;
                        else if (n.Contains("asbest") || n.Contains("akustik") || n.Contains("ekb") || n.Contains("enerji kimlik")) refPrice = 22000m;
                        else if (n.Contains("isg") || n.Contains("osgb") || n.Contains("iş güvenliği")) refPrice = 24000m;
                        else if (n.Contains("iskan") || n.Contains("yapı kullanım") || n.Contains("cins tashihi")) refPrice = 110000m;

                        d.DocumentFee = refPrice;
                        d.EstimatedCost = refPrice;
                        anyPriceUpdated = true;
                    }
                }
            }
            if (anyPriceUpdated)
            {
                await _context.SaveChangesAsync();
            }

            var docIds = allDocs.Select(d => d.Id).ToList();
            var quoteRequests = await _context.B2BQuoteRequests
                .Include(q => q.Invites).ThenInclude(i => i.NetworkContact)
                .Where(q => q.SourceModule == "PhaseZeroDocument" && docIds.Contains(q.SourceReferenceId))
                .ToListAsync();

            ViewBag.Quotes = quoteRequests;
            ViewBag.RealizedDocIds = realizedDocIds;
            ViewBag.InitialDocs = allDocs;
            ViewBag.TotalOfficialFee = allDocs.Sum(d => d.DocumentFee ?? 0);
            ViewBag.TotalAdditionalFee = allDocs.Sum(d => d.AdditionalCost ?? 0);
            ViewBag.TotalInitialFee = allDocs.Sum(d => (d.DocumentFee ?? 0) + (d.AdditionalCost ?? 0));
            ViewBag.CompletedInitialCount = allDocs.Count(d => d.Status == "Tamamlandı" || d.Status == "Alındı");
            ViewBag.StageRules = allRules;
            if (project.CityId.HasValue)
            {
                var city = await _context.Cities.FirstOrDefaultAsync(c => c.Id == project.CityId.Value);
                ViewBag.CityName = city?.Name;
            }
            if (project.DistrictId.HasValue)
            {
                var dist = await _context.Districts.FirstOrDefaultAsync(d => d.Id == project.DistrictId.Value);
                ViewBag.DistrictName = dist?.Name;
            }
            if (project.NeighborhoodId.HasValue)
            {
                var neigh = await _context.Neighborhoods.FirstOrDefaultAsync(n => n.Id == project.NeighborhoodId.Value);
                ViewBag.NeighborhoodName = neigh?.Name;
            }
            var rentPolicy = project.RentAndRontgenPolicy;
            ViewBag.RentPolicy = rentPolicy;
            ViewBag.OnayliEmsal = rentPolicy.OnayliEmsal;
            ViewBag.TabanOturumuTaks = rentPolicy.TabanOturumuTaks;
            ViewBag.ImarNotlari = project.PublicDescription;
            return View(project);
        }
                        // POST: /ConstructionProject/SaveProjectRentPolicy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProjectRentPolicy(int projectId, GMK360.Core.DTOs.Construction.ProjectRentPolicyDto policy, bool applyToStakeholders = false)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
            if (!policy.FlatStartDate.HasValue) policy.FlatStartDate = DateTime.Today;
            if (!policy.FlatEndDate.HasValue) policy.FlatEndDate = policy.FlatStartDate.Value.AddMonths(18);
            if (!policy.ShopStartDate.HasValue) policy.ShopStartDate = DateTime.Today;
            if (!policy.ShopEndDate.HasValue) policy.ShopEndDate = policy.ShopStartDate.Value.AddMonths(18);
            var existingPolicy = project.RentAndRontgenPolicy;
            existingPolicy.FlatRelocationFee = policy.FlatRelocationFee;
            existingPolicy.FlatMonthlyRent = policy.FlatMonthlyRent;
            existingPolicy.FlatStartDate = policy.FlatStartDate;
            existingPolicy.FlatEndDate = policy.FlatEndDate;
            existingPolicy.FlatPaymentDay = policy.FlatPaymentDay;
            existingPolicy.FlatGraceDays = policy.FlatGraceDays;
            existingPolicy.ShopRelocationFee = policy.ShopRelocationFee;
            existingPolicy.ShopMonthlyRent = policy.ShopMonthlyRent;
            existingPolicy.ShopStartDate = policy.ShopStartDate;
            existingPolicy.ShopEndDate = policy.ShopEndDate;
            existingPolicy.ShopPaymentDay = policy.ShopPaymentDay;
            existingPolicy.ShopGraceDays = policy.ShopGraceDays;
            project.RentPolicyJson = System.Text.Json.JsonSerializer.Serialize(existingPolicy);
            if (applyToStakeholders)
            {
                var stakeholders = await _context.ProjectStakeholders
                    .Where(s => s.ProjectId == projectId && !s.IsDeleted)
                    .ToListAsync();
                Func<string, string> norm = u => {
                    if (string.IsNullOrWhiteSpace(u)) return "";
                    var s = u.Trim().ToLowerInvariant()
                        .Replace("ı", "i").Replace("ç", "c").Replace("ş", "s").Replace("ğ", "g").Replace("ü", "u").Replace("ö", "o").Replace(" ", "");
                    if (int.TryParse(s, out int n)) return $"daire{n}";
                    return s;
                };
                var unitGroups = stakeholders.GroupBy(s => norm(s.UnitNumbers)).ToList();
                foreach (var group in unitGroups)
                {
                    int count = group.Count();
                    if (count == 0) continue;
                    bool isShop = group.Any(s => (s.UnitNumbers ?? "").IndexOf("dükkan", StringComparison.OrdinalIgnoreCase) >= 0 || (s.UnitNumbers ?? "").IndexOf("dukkan", StringComparison.OrdinalIgnoreCase) >= 0);
                    foreach (var st in group)
                    {
                        if (isShop)
                        {
                            st.MonthlyRentAllowance = Math.Round(policy.ShopMonthlyRent / count, 2);
                            st.RelocationAllowance = Math.Round(policy.ShopRelocationFee / count, 2);
                            st.RentStartDate = policy.ShopStartDate;
                            st.RentEndDate = policy.ShopEndDate;
                            st.PaymentDay = policy.ShopPaymentDay;
                        }
                        else
                        {
                            st.MonthlyRentAllowance = Math.Round(policy.FlatMonthlyRent / count, 2);
                            st.RelocationAllowance = Math.Round(policy.FlatRelocationFee / count, 2);
                            st.RentStartDate = policy.FlatStartDate;
                            st.RentEndDate = policy.FlatEndDate;
                            st.PaymentDay = policy.FlatPaymentDay;
                        }
                        st.UpdatedAt = DateTime.UtcNow;
                    }
                }
                TempData["SuccessMessage"] = "Kira & taşınma bedelleri kaydedildi ve hak sahiplerinin ödeme takvimine dağıtıldı.";
            }
            else
            {
                TempData["SuccessMessage"] = "Kira & taşınma bedelleri kaydedildi.";
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Analysis), new { id = projectId });
        }
        // POST: /ConstructionProject/SaveUnitStakeholder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveUnitStakeholder(int projectId, int? stakeholderId, string unitNumber, string fullName, string phone, string? email, string? iban, string? notes)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone))
            {
                TempData["ErrorMessage"] = "LÃ¼tfen Hak Sahibi AdÄ± SoyadÄ± ve Telefon bilgisini eksiksiz giriniz.";
                return RedirectToAction(nameof(Analysis), new { id = projectId });
            }
            string cleanPhone = new string(phone.Where(char.IsDigit).ToArray());
            if (!cleanPhone.StartsWith("0") && cleanPhone.Length == 10) cleanPhone = "0" + cleanPhone;
            string firstName = fullName.Trim();
            string lastName = "";
            int lastSpace = firstName.LastIndexOf(' ');
            if (lastSpace > 0)
            {
                lastName = firstName.Substring(lastSpace + 1).Trim();
                firstName = firstName.Substring(0, lastSpace).Trim();
            }
            var user = await _userManager.Users.FirstOrDefaultAsync(u =>
                u.PhoneNumber == cleanPhone || u.PhoneNumber == phone || u.UserName == ("hak_" + cleanPhone));
            string userEmail = !string.IsNullOrWhiteSpace(email) ? email.Trim() : (cleanPhone + "@gmk360.local");
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = "hak_" + cleanPhone,
                    Email = userEmail,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = cleanPhone,
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false,
                    UserType = GMK360.Core.Entities.Identity.UserType.PropertyOwner,
                    IsShadowAccount = true
                };
                var createRes = await _userManager.CreateAsync(user, "Shadow.User2026!");
                if (!createRes.Succeeded)
                {
                    user.UserName = "hak_" + cleanPhone + "_" + Guid.NewGuid().ToString("N").Substring(0, 4);
                    await _userManager.CreateAsync(user, "Shadow.User2026!");
                }
            }
            else
            {
                user.FirstName = firstName;
                user.LastName = lastName;
                user.PhoneNumber = cleanPhone;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    user.Email = email.Trim();
                }
                await _userManager.UpdateAsync(user);
            }
            if (stakeholderId.HasValue && stakeholderId.Value > 0)
            {
                var existingSt = await _context.ProjectStakeholders
                    .FirstOrDefaultAsync(s => s.Id == stakeholderId.Value && s.ProjectId == projectId);
                if (existingSt != null)
                {
                    existingSt.UserId = user.Id;
                    existingSt.UnitNumbers = unitNumber?.Trim();
                    existingSt.Iban = iban?.Trim();
                    existingSt.Notes = notes?.Trim();
                    existingSt.UpdatedAt = DateTime.UtcNow;
                    existingSt.IsDeleted = false;
                }
            }
            else
            {
                var newSt = new ProjectStakeholder
                {
                    ProjectId = projectId,
                    UserId = user.Id,
                    Role = StakeholderRole.Landowner,
                    UnitNumbers = unitNumber?.Trim(),
                    UnitCount = 1,
                    Iban = iban?.Trim(),
                    Notes = notes?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                _context.ProjectStakeholders.Add(newSt);
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = unitNumber + " hak sahibi (" + fullName + ") kaydedildi.";
            return RedirectToAction(nameof(Analysis), new { id = projectId });
        }
        // POST: /ConstructionProject/DeleteUnitStakeholder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUnitStakeholder(int projectId, int stakeholderId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
            var st = await _context.ProjectStakeholders
                .FirstOrDefaultAsync(s => s.Id == stakeholderId && s.ProjectId == projectId);
            if (st != null)
            {
                st.IsDeleted = true;
                st.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hak sahibi kaydÄ± silindi.";
            }
            return RedirectToAction(nameof(Analysis), new { id = projectId });
        }
        // POST: /ConstructionProject/UpdateAnalysisImar
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateAnalysisImar(
            int id,
            double? totalLandArea,
            int? totalFloors,
            int? targetTotalApartments,
            decimal? averageFlatSalePrice,
            string? publicDescription,
            string? imarNotlari,
            int? unitsPerFloor,
            int? basementFloors,
            double? coreLossSqm,
            decimal? ownerSharePercent,
            decimal? firmUnitCostPerSqm,
            int? plannedNewShops,
            int? ownerGivenShops,
            decimal? estimatedShopSalePrice,
            bool? includeRentInFeasibility,
            int? rentDurationMonths,
            double? onayliEmsal,
            double? tabanOturumuTaks,
            double? targetFlatGrossSqm)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
            if (totalLandArea.HasValue && totalLandArea > 0) project.TotalLandArea = totalLandArea;
            if (totalFloors.HasValue) project.TotalFloors = totalFloors;
            if (targetTotalApartments.HasValue) project.TargetTotalApartments = targetTotalApartments;
            if (averageFlatSalePrice.HasValue) project.AverageFlatSalePrice = averageFlatSalePrice.Value;
            if (!string.IsNullOrWhiteSpace(publicDescription)) project.PublicDescription = publicDescription.Trim();
            else if (imarNotlari != null) project.PublicDescription = imarNotlari.Trim();

            var rentPolicy = project.RentAndRontgenPolicy;
            if (unitsPerFloor.HasValue && unitsPerFloor.Value > 0) rentPolicy.UnitsPerFloor = unitsPerFloor.Value;
            if (basementFloors.HasValue && basementFloors.Value >= 0) rentPolicy.BasementFloors = basementFloors.Value;
            if (coreLossSqm.HasValue && coreLossSqm.Value >= 0) rentPolicy.CoreLossSqm = (decimal)coreLossSqm.Value;
            if (ownerSharePercent.HasValue && ownerSharePercent.Value > 0) rentPolicy.OwnerSharePercent = ownerSharePercent.Value;
            if (firmUnitCostPerSqm.HasValue && firmUnitCostPerSqm.Value > 0) rentPolicy.FirmUnitCostPerSqm = firmUnitCostPerSqm.Value;
            if (plannedNewShops.HasValue && plannedNewShops.Value >= 0)
            {
                rentPolicy.PlannedNewShops = plannedNewShops.Value;
                project.TargetTotalShops = plannedNewShops.Value;
            }
            if (ownerGivenShops.HasValue && ownerGivenShops.Value >= 0) rentPolicy.OwnerGivenShops = ownerGivenShops.Value;
            if (estimatedShopSalePrice.HasValue && estimatedShopSalePrice.Value >= 0) rentPolicy.EstimatedShopSalePrice = estimatedShopSalePrice.Value;
            if (includeRentInFeasibility.HasValue) rentPolicy.IncludeRentInFeasibility = includeRentInFeasibility.Value;
            if (rentDurationMonths.HasValue && rentDurationMonths.Value > 0) rentPolicy.RentDurationMonths = rentDurationMonths.Value;
            if (onayliEmsal.HasValue && onayliEmsal.Value > 0) rentPolicy.OnayliEmsal = (decimal)onayliEmsal.Value;
            if (tabanOturumuTaks.HasValue && tabanOturumuTaks.Value > 0) rentPolicy.TabanOturumuTaks = (decimal)tabanOturumuTaks.Value;
            if (targetFlatGrossSqm.HasValue && targetFlatGrossSqm.Value > 0) rentPolicy.TargetFlatGrossSqm = (int)Math.Round(targetFlatGrossSqm.Value);

            project.RentPolicyJson = System.Text.Json.JsonSerializer.Serialize(rentPolicy);
            await _context.SaveChangesAsync();
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "İmar ve kapasite analizi başarıyla kaydedildi." });
            }
            TempData["SuccessMessage"] = "İmar ve kapasite analizi başarıyla kaydedildi.";
            return RedirectToAction(nameof(Analysis), new { id = id });
        }
        // POST: /ConstructionProject/SaveAnalysisRontgen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAnalysisRontgen(
            int projectId,
            string? titleDeedEncumbranceStatus,
            string? inheritanceStatus,
            string? hostileOwnerStatus,
            string? legalCheckNotes,
            string? adjacentBuildingRisk,
            int? adjacentBuildingAge,
            string? shoringRequirement,
            decimal? estimatedShoringCost,
            string? streetLogisticsStatus,
            string? demolitionExecutionMode,
            bool requireDemolitionShield = false,
            decimal? demolitionExtraPolicyCost = null,
            string? landSlopeStatus = null,
            double? retainingWallHeightMeters = null,
            decimal? estimatedRetainingWallCost = null,
            string? soilBearingStatus = null,
            int? estimatedPileCount = null,
            decimal? estimatedPilingCost = null,
            string? groundwaterStatus = null,
            decimal? estimatedGroundwaterCost = null,
            string? extraGroundCostPayer = null,
            string? coreTestStatus = null,
            string? regionalHarmonyStatus = null,
            string? goNoGoDecision = null,
            string? contractShieldClauses = null,
            bool proceedToProposal = false)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();

            var rentPolicy = project.RentAndRontgenPolicy;
            if (!string.IsNullOrWhiteSpace(titleDeedEncumbranceStatus)) rentPolicy.TitleDeedEncumbranceStatus = titleDeedEncumbranceStatus.Trim();
            if (!string.IsNullOrWhiteSpace(inheritanceStatus)) rentPolicy.InheritanceStatus = inheritanceStatus.Trim();
            if (!string.IsNullOrWhiteSpace(hostileOwnerStatus)) rentPolicy.HostileOwnerStatus = hostileOwnerStatus.Trim();
            rentPolicy.LegalCheckNotes = legalCheckNotes?.Trim();

            if (!string.IsNullOrWhiteSpace(adjacentBuildingRisk))
            {
                rentPolicy.AdjacentBuildingRisk = adjacentBuildingRisk.Trim();
            }
            if (adjacentBuildingAge.HasValue) rentPolicy.AdjacentBuildingAge = adjacentBuildingAge.Value;
            if (!string.IsNullOrWhiteSpace(shoringRequirement))
            {
                rentPolicy.ShoringRequirement = shoringRequirement.Trim();
            }
            if (estimatedShoringCost.HasValue)
            {
                rentPolicy.EstimatedShoringCost = estimatedShoringCost.Value;
            }
            if (!string.IsNullOrWhiteSpace(streetLogisticsStatus)) rentPolicy.StreetLogisticsStatus = streetLogisticsStatus.Trim();
            if (!string.IsNullOrWhiteSpace(demolitionExecutionMode)) rentPolicy.DemolitionExecutionMode = demolitionExecutionMode.Trim();
            rentPolicy.RequireDemolitionShield = requireDemolitionShield;
            if (demolitionExtraPolicyCost.HasValue) rentPolicy.DemolitionExtraPolicyCost = demolitionExtraPolicyCost.Value;

            if (!string.IsNullOrWhiteSpace(landSlopeStatus)) rentPolicy.LandSlopeStatus = landSlopeStatus.Trim();
            if (retainingWallHeightMeters.HasValue) rentPolicy.RetainingWallHeightMeters = (decimal)retainingWallHeightMeters.Value;
            if (estimatedRetainingWallCost.HasValue) rentPolicy.EstimatedRetainingWallCost = estimatedRetainingWallCost.Value;

            if (!string.IsNullOrWhiteSpace(soilBearingStatus)) rentPolicy.SoilBearingStatus = soilBearingStatus.Trim();
            if (estimatedPileCount.HasValue) rentPolicy.EstimatedPileCount = estimatedPileCount.Value;
            if (estimatedPilingCost.HasValue) rentPolicy.EstimatedPilingCost = estimatedPilingCost.Value;

            if (!string.IsNullOrWhiteSpace(groundwaterStatus)) rentPolicy.GroundwaterStatus = groundwaterStatus.Trim();
            if (estimatedGroundwaterCost.HasValue) rentPolicy.EstimatedGroundwaterCost = estimatedGroundwaterCost.Value;
            if (!string.IsNullOrWhiteSpace(extraGroundCostPayer)) rentPolicy.ExtraGroundCostPayer = extraGroundCostPayer.Trim();

            if (!string.IsNullOrWhiteSpace(coreTestStatus)) rentPolicy.CoreTestStatus = coreTestStatus.Trim();
            if (!string.IsNullOrWhiteSpace(regionalHarmonyStatus)) rentPolicy.RegionalHarmonyStatus = regionalHarmonyStatus.Trim();
            if (!string.IsNullOrWhiteSpace(goNoGoDecision)) rentPolicy.GoNoGoDecision = goNoGoDecision.Trim();
            if (contractShieldClauses != null) rentPolicy.ContractShieldClauses = contractShieldClauses.Trim();

            project.RentPolicyJson = System.Text.Json.JsonSerializer.Serialize(rentPolicy);
            await _context.SaveChangesAsync();

            if (proceedToProposal)
            {
                TempData["SuccessMessage"] = "Detaylı inceleme (Röntgen), GO/NO-GO kararı ve sözleşme kalkanı maddeleri kaydedilerek Teklif Masasına aktarıldı.";
                return RedirectToAction(nameof(Proposal), new { id = projectId });
            }

            TempData["SuccessMessage"] = "5 boyutlu proje röntgeni ve GO/NO-GO karar kartı başarıyla kaydedildi.";
            return RedirectToAction(nameof(Analysis), new { id = projectId });
        }
        // POST: /ConstructionProject/RefreshNeighborhoodAi
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> RefreshNeighborhoodAi(int projectId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null || !project.NeighborhoodId.HasValue)
                return Json(new { success = false, message = "Proje veya mahalle bilgisi bulunamadı." });
            var neigh = await _context.Neighborhoods.FirstOrDefaultAsync(n => n.Id == project.NeighborhoodId.Value);
            if (neigh == null) return Json(new { success = false, message = "Mahalle kaydı bulunamadı." });
            var dist = project.DistrictId.HasValue ? await _context.Districts.FirstOrDefaultAsync(d => d.Id == project.DistrictId.Value) : null;
            string districtName = dist?.Name ?? "Maltepe";
            var geminiService = HttpContext.RequestServices.GetService<GMK360.Core.Interfaces.IGeminiAiService>();
            if (geminiService == null) return Json(new { success = false, message = "Yapay zeka servisi hazır değil." });
            var report = await geminiService.AnalyzeNeighborhoodAsync(districtName, neigh.Name);
            neigh.AiAnalysisReport = report;
            neigh.AiAnalysisUpdatedAt = DateTime.UtcNow;
            _context.Neighborhoods.Update(neigh);
            await _context.SaveChangesAsync();
            return Json(new { 
                success = true, 
                report = report, 
                updatedAt = neigh.AiAnalysisUpdatedAt.Value.ToString("dd.MM.yyyy HH:mm") 
            });
        }
        // GET: /ConstructionProject/QueryTkgmParcel
        [HttpGet]
        public async Task<IActionResult> QueryTkgmParcel(int? projectId, double? lat, double? lng, long? mahalleId, long? neighborhoodId, string? ada, string? parsel)
        {
            mahalleId = mahalleId ?? neighborhoodId;
            var tkgmService = HttpContext.RequestServices.GetRequiredService<GMK360.Core.Interfaces.ITkgmService>();
            // 1. MahalleId + Ada + Parsel ile doğrudan sorgu
            if (mahalleId.HasValue && !string.IsNullOrWhiteSpace(ada) && !string.IsNullOrWhiteSpace(parsel))
            {
                var res = await tkgmService.GetParcelByAdaParselAsync(mahalleId.Value, ada, parsel);
                return FormatTkgmResponse(res);
            }
            // 2. Koordinat ile doğrudan sorgu
            if (lat.HasValue && lng.HasValue)
            {
                var res = await tkgmService.GetParcelByCoordinatesAsync(lat.Value, lng.Value);
                return FormatTkgmResponse(res);
            }
            // 3. Proje ID'den koordinat alarak sorgu
            if (projectId.HasValue)
            {
                var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == projectId.Value && !p.IsDeleted);
                if (project != null && project.Latitude.HasValue && project.Longitude.HasValue)
                {
                    var res = await tkgmService.GetParcelByCoordinatesAsync(project.Latitude.Value, project.Longitude.Value);
                    return FormatTkgmResponse(res);
                }
                return Json(new { success = false, message = "Projeye ait koordinat (Enlem / Boylam) bulunamadı. Lütfen haritadan konum belirleyiniz veya Ada/Parsel giriniz." });
            }
            return Json(new { success = false, message = "Sorgulama için koordinat veya Ada/Parsel bilgisi gereklidir." });
        }
        private IActionResult FormatTkgmResponse(GMK360.Core.DTOs.TkgmParcelResultDto res)
        {
            if (!res.Success)
            {
                return Json(new { success = false, message = res.Message ?? "Parsel bulunamadı." });
            }
            return Json(new
            {
                success = true,
                message = res.Message,
                parcel = res,
                ilAd = res.IlAd,
                ilceAd = res.IlceAd,
                mahalleAd = res.MahalleAd,
                ada = res.AdaNo,
                parsel = res.ParselNo,
                pafta = res.Pafta,
                nitelik = res.Nitelik,
                zeminKmdurum = res.ZeminKmdurum,
                alan = res.AreaSquareMeters,
                area = res.AreaSquareMeters,
                areaFormatted = res.AreaFormatted,
                centerLatitude = res.CenterLat,
                centerLongitude = res.CenterLng,
                latitude = res.CenterLat,
                longitude = res.CenterLng,
                geoJson = res.GeoJson
            });
        }
        // POST: /ConstructionProject/ApplyTkgmParcelData
        [HttpPost]
        public async Task<IActionResult> ApplyTkgmParcelData([FromBody] GMK360.Core.DTOs.ApplyTkgmDto dto)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            if (dto == null || dto.ProjectId <= 0)
                return Json(new { success = false, message = "Geçersiz proje bilgisi." });
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return Json(new { success = false, message = "Proje bulunamadı." });
            if (!string.IsNullOrWhiteSpace(dto.Ada)) project.Ada = dto.Ada.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Parsel)) project.Parsel = dto.Parsel.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Nitelik)) project.ProjectType = dto.Nitelik.Trim();
            var landArea = dto.TotalLandArea ?? dto.Area;
            if (landArea.HasValue && landArea.Value > 0)
                project.TotalLandArea = landArea.Value;
            if (dto.CenterLatitude.HasValue && dto.CenterLatitude.Value > 0) project.Latitude = dto.CenterLatitude.Value;
            if (dto.CenterLongitude.HasValue && dto.CenterLongitude.Value > 0) project.Longitude = dto.CenterLongitude.Value;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Parsel ve arsa verileri projeye başarıyla işlendi." });
        }
        // GET: /ConstructionProject/GetTkgmProvinces
        [HttpGet]
        public async Task<IActionResult> GetTkgmProvinces()
        {
            var tkgmService = HttpContext.RequestServices.GetRequiredService<GMK360.Core.Interfaces.ITkgmService>();
            var list = await tkgmService.GetProvincesAsync();
            return Json(list);
        }
        // GET: /ConstructionProject/GetTkgmDistricts
        [HttpGet]
        public async Task<IActionResult> GetTkgmDistricts(int provinceId)
        {
            var tkgmService = HttpContext.RequestServices.GetRequiredService<GMK360.Core.Interfaces.ITkgmService>();
            var list = await tkgmService.GetDistrictsAsync(provinceId);
            return Json(list);
        }
        // GET: /ConstructionProject/GetTkgmNeighborhoods
        [HttpGet]
        public async Task<IActionResult> GetTkgmNeighborhoods(int districtId)
        {
            var tkgmService = HttpContext.RequestServices.GetRequiredService<GMK360.Core.Interfaces.ITkgmService>();
            var list = await tkgmService.GetNeighborhoodsAsync(districtId);
            return Json(list);
        }
        // GET: /ConstructionProject/ExportFeasibilityPdf?projectId=5
        [HttpGet]
        public async Task<IActionResult> ExportFeasibilityPdf(int projectId)
        {
            return await GenerateFeasibilityPdfInternal(projectId, null, null, null, null, null, null);
        }
        // POST: /ConstructionProject/ExportFeasibilityPdfWithMap
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ExportFeasibilityPdfWithMap(
            [FromForm] int projectId,
            [FromForm] double? emsal,
            [FromForm] double? taks,
            [FromForm] int? totalFloors,
            [FromForm] double? flatSqm,
            [FromForm] string? imarNotlari,
            [FromForm] string? mapBase64,
            [FromForm] string? scenariosJson)
        {
            return await GenerateFeasibilityPdfInternal(projectId, emsal, taks, totalFloors, flatSqm, imarNotlari, mapBase64, scenariosJson);
        }
        private async Task<IActionResult> GenerateFeasibilityPdfInternal(
            int projectId,
            double? emsal,
            double? taks,
            int? totalFloors,
            double? flatSqm,
            string? imarNotlari,
            string? mapBase64,
            string? scenariosJson = null)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .Include(p => p.Agency)
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
            var city = project.CityId.HasValue ? await _context.Cities.FindAsync(project.CityId.Value) : null;
            var dist = project.DistrictId.HasValue ? await _context.Districts.FindAsync(project.DistrictId.Value) : null;
            var neigh = project.NeighborhoodId.HasValue ? await _context.Neighborhoods.FindAsync(project.NeighborhoodId.Value) : null;
            double calcLandArea = project.TotalLandArea ?? 850;
            double calcEmsal = emsal ?? 2.00;
            double calcTaks = taks ?? 0.35;
            int calcFloors = totalFloors ?? project.TotalFloors ?? 8;
            double calcFlatSqm = flatSqm ?? 100;
            int oldUnits = project.EskiDaireSayisi ?? 4;
            double totalBuildingArea = calcLandArea * calcEmsal;
            double groundBaseArea = calcLandArea * calcTaks;
            int estimatedUnits = calcFlatSqm > 0 ? (int)Math.Floor(totalBuildingArea / calcFlatSqm) : 0;
            int surplusUnits = estimatedUnits - oldUnits;
            var dto = new GMK360.Core.DTOs.FeasibilityPdfRequestDto
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                CompanyName = project.Agency?.CompanyName ?? "GMK360 İNŞAAT & MÜHENDİSLİK",
                CityName = city?.Name ?? "İstanbul",
                DistrictName = dist?.Name ?? "Maltepe",
                NeighborhoodName = neigh?.Name ?? "Atış Okulu",
                Ada = project.Ada,
                Parsel = project.Parsel,
                Pafta = "65/1",
                Nitelik = project.ProjectType ?? "Arsa",
                Latitude = project.Latitude,
                Longitude = project.Longitude,
                LandArea = calcLandArea,
                Emsal = calcEmsal,
                Taks = calcTaks,
                TotalFloors = calcFloors,
                FlatSqm = calcFlatSqm,
                OldUnits = oldUnits,
                EstimatedUnits = estimatedUnits,
                SurplusUnits = surplusUnits,
                TotalBuildingArea = totalBuildingArea,
                GroundBaseArea = groundBaseArea,
                ImarNotlari = imarNotlari,
                MapImageBase64 = mapBase64
            };
            if (!string.IsNullOrWhiteSpace(scenariosJson))
            {
                try
                {
                    var parsedScenarios = System.Text.Json.JsonSerializer.Deserialize<List<GMK360.Core.DTOs.FeasibilityScenarioDto>>(
                        scenariosJson, 
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (parsedScenarios != null && parsedScenarios.Count > 0)
                    {
                        dto.Scenarios = parsedScenarios;
                    }
                }
                catch { }
            }
            var pdfService = HttpContext.RequestServices.GetRequiredService<GMK360.Core.Interfaces.IConstructionFeasibilityPdfService>();
            var pdfBytes = pdfService.GenerateFeasibilityReport(dto);
            string safeAda = string.IsNullOrWhiteSpace(project.Ada) ? "Ada" : project.Ada;
            string safeParsel = string.IsNullOrWhiteSpace(project.Parsel) ? "Parsel" : project.Parsel;
            string fileName = $"GMK360_Imar_Fizibilite_{safeAda}_{safeParsel}_{DateTime.Now:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        // GET: /ConstructionProject/Proposal/5
        public async Task<IActionResult> Proposal(int? id)
        {
            if (id == null) return NotFound();
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .Include(p => p.BinaYoneticisi)
                .Include(p => p.Agency)
                .FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
            ViewData["ProjectId"] = project.Id;
            ViewData["ProjectName"] = project.Name;
            ViewData["ProjectStatus"] = project.Status;
            if (project.CityId.HasValue)
            {
                var city = await _context.Cities.FirstOrDefaultAsync(c => c.Id == project.CityId.Value);
                ViewBag.CityName = city?.Name;
            }
            if (project.DistrictId.HasValue)
            {
                var dist = await _context.Districts.FirstOrDefaultAsync(d => d.Id == project.DistrictId.Value);
                ViewBag.DistrictName = dist?.Name;
            }
            if (project.NeighborhoodId.HasValue)
            {
                var neigh = await _context.Neighborhoods.FirstOrDefaultAsync(n => n.Id == project.NeighborhoodId.Value);
                ViewBag.NeighborhoodName = neigh?.Name;
            }
            var stakeholders = await _context.ProjectStakeholders
                .Include(s => s.User)
                .Where(s => s.ProjectId == id && !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
            ViewBag.Stakeholders = stakeholders;
            ViewBag.RentPolicy = project.RentAndRontgenPolicy;
            return View(project);
        }
        // POST: /ConstructionProject/SaveProjectProposal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProjectProposal(
            int projectId,
            decimal? ownerSharePercent,
            decimal? flatMonthlyRent,
            decimal? flatRelocationFee,
            decimal? shopMonthlyRent,
            decimal? shopRelocationFee,
            int? rentDurationMonths,
            decimal? firmUnitCostPerSqm,
            decimal? bankGuaranteeRate,
            int? targetTotalApartments,
            int? plannedNewShops,
            int? ownerGivenShops,
            string? proposalSummaryText,
            string? contractShieldClauses,
            string? outerWallType,
            string? facadeInsulation,
            string? roofMaterial,
            string? innerWallType,
            string? plumbingType,
            string? foundationInsulation)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();

            var rentPolicy = project.RentAndRontgenPolicy;
            if (ownerSharePercent.HasValue) rentPolicy.OwnerSharePercent = ownerSharePercent.Value;
            if (flatMonthlyRent.HasValue) rentPolicy.FlatMonthlyRent = flatMonthlyRent.Value;
            if (flatRelocationFee.HasValue) rentPolicy.FlatRelocationFee = flatRelocationFee.Value;
            if (shopMonthlyRent.HasValue) rentPolicy.ShopMonthlyRent = shopMonthlyRent.Value;
            if (shopRelocationFee.HasValue) rentPolicy.ShopRelocationFee = shopRelocationFee.Value;
            if (rentDurationMonths.HasValue && rentDurationMonths.Value > 0) rentPolicy.RentDurationMonths = rentDurationMonths.Value;
            if (firmUnitCostPerSqm.HasValue && firmUnitCostPerSqm.Value > 0) rentPolicy.FirmUnitCostPerSqm = firmUnitCostPerSqm.Value;
            if (bankGuaranteeRate.HasValue && bankGuaranteeRate.Value >= 0) rentPolicy.BankGuaranteeRate = bankGuaranteeRate.Value;
            if (targetTotalApartments.HasValue && targetTotalApartments.Value > 0) project.TargetTotalApartments = targetTotalApartments.Value;
            if (plannedNewShops.HasValue && plannedNewShops.Value >= 0)
            {
                rentPolicy.PlannedNewShops = plannedNewShops.Value;
                project.TargetTotalShops = plannedNewShops.Value;
            }
            if (ownerGivenShops.HasValue && ownerGivenShops.Value >= 0) rentPolicy.OwnerGivenShops = ownerGivenShops.Value;
            if (proposalSummaryText != null) project.PublicDescription = proposalSummaryText.Trim();
            if (contractShieldClauses != null) rentPolicy.ContractShieldClauses = contractShieldClauses.Trim();

            // Ön DNA (Mühendislik Statik Yük & Mimari Ön Kararları) Kaydı
            if (!string.IsNullOrWhiteSpace(outerWallType)) project.OuterWallType = outerWallType.Trim();
            if (!string.IsNullOrWhiteSpace(facadeInsulation)) project.FacadeInsulation = facadeInsulation.Trim();
            if (!string.IsNullOrWhiteSpace(roofMaterial))
            {
                project.RoofMaterial = roofMaterial.Trim();
                project.RoofType = roofMaterial.Contains("Düz") || roofMaterial.Contains("Teras") ? "Düz_Teras" : "Kiremit";
            }
            if (!string.IsNullOrWhiteSpace(innerWallType)) project.InnerWallType = innerWallType.Trim();
            if (!string.IsNullOrWhiteSpace(plumbingType)) project.PlumbingType = plumbingType.Trim();
            if (!string.IsNullOrWhiteSpace(foundationInsulation)) project.FoundationInsulation = foundationInsulation.Trim();

            project.RentPolicyJson = System.Text.Json.JsonSerializer.Serialize(rentPolicy);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Resmi teklif parametreleri, Ön DNA (statik/mimari mühendislik kararları) ve sözleşme şartname maddeleri başarıyla kaydedildi.";
            return RedirectToAction(nameof(Proposal), new { id = projectId });
        }
        // GET: /ConstructionProject/Finance/5
        public async Task<IActionResult> Finance(int? id)
        {
            if (id == null) return NotFound();
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .Include(p => p.BinaYoneticisi)
                .Include(p => p.Agency)
                .FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
            ViewData["ProjectId"] = project.Id;
            ViewData["ProjectName"] = project.Name;
            ViewData["ProjectStatus"] = project.Status;
            if (project.CityId.HasValue)
            {
                var city = await _context.Cities.FirstOrDefaultAsync(c => c.Id == project.CityId.Value);
                ViewBag.CityName = city?.Name;
            }
            if (project.DistrictId.HasValue)
            {
                var dist = await _context.Districts.FirstOrDefaultAsync(d => d.Id == project.DistrictId.Value);
                ViewBag.DistrictName = dist?.Name;
            }
            if (project.NeighborhoodId.HasValue)
            {
                var neigh = await _context.Neighborhoods.FirstOrDefaultAsync(n => n.Id == project.NeighborhoodId.Value);
                ViewBag.NeighborhoodName = neigh?.Name;
            }
            var stakeholders = await _context.ProjectStakeholders
                .Include(s => s.User)
                .Where(s => s.ProjectId == id && !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
            ViewBag.Stakeholders = stakeholders;
            var legalDocs = await _context.ProjectLegalDocuments
                .Where(d => d.ConstructionProjectId == id)
                .ToListAsync();
            ViewBag.PaidFees = legalDocs.Where(d => d.Status == "Tamamlandı").Sum(d => (d.DocumentFee ?? 0) + (d.AdditionalCost ?? 0));
            decimal paidLegalFees = legalDocs.Where(d => d.Status == "Tamamlandı").Sum(d => (d.DocumentFee ?? 0) + (d.AdditionalCost ?? 0));
            decimal pendingFees = legalDocs.Where(d => d.Status != "Tamamlandı").Sum(d => (d.DocumentFee ?? 0) + (d.AdditionalCost ?? 0));
            // 1. Proje Kasaları & Alt Hesaplar (Resmi Banka + Şantiye Nakit + Banka Kiralık Kasa)
            var projectAccounts = await EnsureProjectAccountsAsync(project.Id, project.Name);
            var officialBank = projectAccounts.FirstOrDefault(a => a.IBAN != null && (a.IBAN.Contains("TRY") || a.IBAN.StartsWith("TR")) || a.BankName.Contains("Resmi")) ?? projectAccounts[0];
            var siteCashVault = projectAccounts.FirstOrDefault(a => a != officialBank && (a.IBAN != null && (a.IBAN.Contains("SNT") || a.IBAN.Contains("NAKIT")) || a.BankName.Contains("Şantiye"))) ?? (projectAccounts.Count > 1 ? projectAccounts[1] : officialBank);
            var safetyBox = projectAccounts.FirstOrDefault(a => a != officialBank && a != siteCashVault && (a.IBAN != null && (a.IBAN.Contains("USD") || a.IBAN.Contains("KIRALIK") || a.IBAN.Contains("KAS")) || a.BankName.Contains("Kiralık"))) ?? (projectAccounts.Count > 2 ? projectAccounts[2] : officialBank);
            ViewBag.ProjectAccount = officialBank;
            ViewBag.OfficialBank = officialBank;
            ViewBag.SiteCashVault = siteCashVault;
            ViewBag.SafetyBoxAccount = safetyBox;
            ViewBag.ProjectAccounts = projectAccounts;
            ViewBag.ProjectTransactions = await LoadProjectTransactionsAsync(project.Id);
            ViewBag.ProjectExpectedTransactions = await LoadProjectExpectedTransactionsAsync(project.Id);
            // 2. Güncel TCMB Döviz Kurları (USD, EUR) & Hareketli Çapraz Kur Takibi
            var latestRate = await _context.ExchangeRates
                .OrderByDescending(r => r.Date)
                .FirstOrDefaultAsync();
            decimal usdRate = (latestRate?.UsdRate > 0) ? latestRate.UsdRate : 49.6540m;
            decimal eurRate = (latestRate?.EurRate > 0) ? latestRate.EurRate : 55.5997m;
            DateTime rateDate = latestRate?.Date ?? DateTime.Today;
            ViewBag.UsdRate = usdRate;
            ViewBag.EurRate = eurRate;
            ViewBag.RateDate = rateDate;
            // Dinamik portföy toplamı: Kullanıcının tüm gerçek hesaplarını döviz kurlarıyla TL'ye çevirip topla
            decimal consolidatedTotalTL = 0m;
            decimal usdBalance = 0m;
            foreach (var acc in projectAccounts)
            {
                bool isUsd = (acc.IBAN != null && acc.IBAN.IndexOf("USD", StringComparison.OrdinalIgnoreCase) >= 0)
                          || (acc.BankName != null && (acc.BankName.IndexOf("Dolar", StringComparison.OrdinalIgnoreCase) >= 0 || acc.BankName.IndexOf("USD", StringComparison.OrdinalIgnoreCase) >= 0))
                          || (acc.AccountName != null && (acc.AccountName.IndexOf("Dolar", StringComparison.OrdinalIgnoreCase) >= 0 || acc.AccountName.IndexOf("USD", StringComparison.OrdinalIgnoreCase) >= 0));
                
                bool isEur = (acc.IBAN != null && acc.IBAN.IndexOf("EUR", StringComparison.OrdinalIgnoreCase) >= 0)
                          || (acc.BankName != null && (acc.BankName.IndexOf("Euro", StringComparison.OrdinalIgnoreCase) >= 0 || acc.BankName.IndexOf("EUR", StringComparison.OrdinalIgnoreCase) >= 0))
                          || (acc.AccountName != null && (acc.AccountName.IndexOf("Euro", StringComparison.OrdinalIgnoreCase) >= 0 || acc.AccountName.IndexOf("EUR", StringComparison.OrdinalIgnoreCase) >= 0));
                if (isUsd)
                {
                    usdBalance += acc.CurrentBalance;
                    consolidatedTotalTL += Math.Round(acc.CurrentBalance * usdRate, 2);
                }
                else if (isEur)
                {
                    consolidatedTotalTL += Math.Round(acc.CurrentBalance * eurRate, 2);
                }
                else
                {
                    consolidatedTotalTL += acc.CurrentBalance;
                }
            }
            decimal usdEquivalentTL = Math.Round(usdBalance * usdRate, 2);
            ViewBag.UsdBalance = usdBalance;
            ViewBag.UsdEquivalentTL = usdEquivalentTL;
            decimal officialBalance = officialBank?.CurrentBalance ?? 0m;
            decimal siteCashBalance = siteCashVault?.CurrentBalance ?? 0m;
            ViewBag.OfficialBankBalance = officialBalance;
            ViewBag.SiteCashBalance = siteCashBalance;
            ViewBag.ConsolidatedTotalTL = consolidatedTotalTL;
            // 3. Gerçek Masraf Kalemleri (Resmi vs Gayriresmi / KDV Ayrıştırmalı)
            var expenses = await _context.ConstructionProjectExpenses
                .Where(e => e.ProjectId == id && !e.IsDeleted)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
            ViewBag.Expenses = expenses;
            // Altın Kural: KDV'siz Net Rakamlar Esas Alınır
            var officialExpenses = expenses.Where(e => e.ApprovalType != "Unofficial").ToList();
            var unofficialExpenses = expenses.Where(e => e.ApprovalType == "Unofficial").ToList();
            decimal totalNetExpense = expenses.Sum(e => e.AmountExcludingVat ?? (e.VatRate > 0 ? (e.Amount / (1m + ((decimal)e.VatRate / 100m))) : e.Amount));
            decimal totalVat = expenses.Sum(e => e.VatAmount ?? (e.Amount - (e.AmountExcludingVat ?? (e.VatRate > 0 ? (e.Amount / (1m + ((decimal)e.VatRate / 100m))) : e.Amount))));
            decimal totalGrossExpense = expenses.Sum(e => e.Amount);
            decimal officialNetExpense = officialExpenses.Sum(e => e.AmountExcludingVat ?? (e.VatRate > 0 ? (e.Amount / (1m + ((decimal)e.VatRate / 100m))) : e.Amount));
            decimal officialVat = officialExpenses.Sum(e => e.VatAmount ?? (e.Amount - (e.AmountExcludingVat ?? (e.VatRate > 0 ? (e.Amount / (1m + ((decimal)e.VatRate / 100m))) : e.Amount))));
            decimal officialGrossExpense = officialExpenses.Sum(e => e.Amount);
            decimal unofficialNetExpense = unofficialExpenses.Sum(e => e.Amount);
            ViewBag.TotalNetExpense = totalNetExpense;
            ViewBag.TotalVat = totalVat;
            ViewBag.TotalGrossExpense = totalGrossExpense;
            ViewBag.PaidFees = paidLegalFees;
            ViewBag.PendingFees = pendingFees;
            decimal totalInitialNetCosts = totalNetExpense + paidLegalFees;
            ViewBag.TotalInitialNetCosts = totalInitialNetCosts;
            ViewBag.OfficialNetExpense = officialNetExpense;
            ViewBag.OfficialVat = officialVat;
            ViewBag.OfficialGrossExpense = officialGrossExpense;
            ViewBag.UnofficialNetExpense = unofficialNetExpense;
            // 4. Ortaklık & Sermaye Durumu (Net Avans / Eksiye Düşme Takibi)
            decimal ourAdvance = project.OurVaultAdvance ?? 0m;
            decimal partnerAdvance = project.PartnerVaultAdvance ?? 0m;
            decimal ourShareRatio = (project.OurSharePercentage > 0 ? project.OurSharePercentage : 100m) / 100m;
            decimal partnerShareRatio = (project.PartnerSharePercentage) / 100m;
            decimal ourCostShare = totalInitialNetCosts * ourShareRatio;
            decimal partnerCostShare = totalInitialNetCosts * partnerShareRatio;
            decimal ourNetBalance = ourAdvance - ourCostShare;
            decimal partnerNetBalance = partnerAdvance - partnerCostShare;
            ViewBag.OurAdvance = ourAdvance;
            ViewBag.PartnerAdvance = partnerAdvance;
            ViewBag.OurCostShare = ourCostShare;
            ViewBag.PartnerCostShare = partnerCostShare;
            ViewBag.OurNetBalance = ourNetBalance;
            ViewBag.PartnerNetBalance = partnerNetBalance;
            return View(project);
        }
        // Proje Alt Kasalarını Garantiye Alma (Resmi Banka + Şantiye Nakit + Banka Kiralık Kasası)
        public async Task<List<BankAccount>> EnsureProjectAccountsAsync(int projectId, string projectName)
        {
            var accounts = await _context.BankAccounts
                .Where(b => b.OwnerType == OwnerType.ConstructionProject && b.OwnerId == projectId && !b.IsDeleted)
                .OrderBy(b => b.Id)
                .ToListAsync();
            if (accounts.Any())
            {
                return accounts;
            }
            bool hasChanges = false;
            // 1. Resmi Proje Banka Hesabı (TL)
            var officialBank = accounts.FirstOrDefault(a => a.IBAN != null && (a.IBAN.Contains("TRY") || a.IBAN.StartsWith("TR")) || a.BankName.Contains("Resmi") || a.BankName.Contains("Banka"));
            if (officialBank == null)
            {
                officialBank = accounts.FirstOrDefault();
            }
            if (officialBank == null)
            {
                officialBank = new BankAccount
                {
                    OwnerType = OwnerType.ConstructionProject,
                    OwnerId = projectId,
                    BankName = "Resmi Proje Banka Hesabı",
                    AccountName = $"{projectName} - Vadesiz TL Hesabı",
                    IBAN = $"TR-PRJ-{projectId:D4}-TRY",
                    CurrentBalance = 0m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.BankAccounts.Add(officialBank);
                accounts.Add(officialBank);
                hasChanges = true;
            }
            // 2. Şantiye Nakit Kasası (TL)
            var siteCashVault = accounts.FirstOrDefault(a => a != officialBank && (a.IBAN != null && (a.IBAN.Contains("SNT") || a.IBAN.Contains("NAKIT")) || a.BankName.Contains("Şantiye")));
            if (siteCashVault == null)
            {
                siteCashVault = new BankAccount
                {
                    OwnerType = OwnerType.ConstructionProject,
                    OwnerId = projectId,
                    BankName = "Şantiye Nakit Kasası",
                    AccountName = $"{projectName} - Şantiye Kasası (TL)",
                    IBAN = $"KASA-SNT-{projectId:D4}-TL",
                    CurrentBalance = 0m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.BankAccounts.Add(siteCashVault);
                accounts.Add(siteCashVault);
                hasChanges = true;
            }
            // 3. Banka Kiralık Kasası (USD / Döviz Rezervi)
            var safetyBox = accounts.FirstOrDefault(a => a != officialBank && a != siteCashVault && (a.IBAN != null && (a.IBAN.Contains("USD") || a.IBAN.Contains("KIRALIK") || a.IBAN.Contains("KAS")) || a.BankName.Contains("Kiralık")));
            if (safetyBox == null)
            {
                safetyBox = new BankAccount
                {
                    OwnerType = OwnerType.ConstructionProject,
                    OwnerId = projectId,
                    BankName = "Banka Kiralık Kasası (USD)",
                    AccountName = $"{projectName} - Kiralık Kasa Rezervi (USD)",
                    IBAN = $"KASA-KAS-{projectId:D4}-USD",
                    CurrentBalance = 0m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.BankAccounts.Add(safetyBox);
                accounts.Add(safetyBox);
                hasChanges = true;
            }
            if (hasChanges)
            {
                await _context.SaveChangesAsync();
            }
            return accounts;
        }
        // Tekli Hesap İstekleri İçin Geriye Dönük Uyumluluk
        public async Task<BankAccount> EnsureProjectAccountAsync(int projectId, string projectName)
        {
            var accounts = await EnsureProjectAccountsAsync(projectId, projectName);
            return accounts.First();
        }
        // POST: /ConstructionProject/AddProjectExpense
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProjectExpense(
            int projectId,
            string title,
            int expenseType,
            string? description,
            string expenseDate,
            string? expenseTime,
            decimal netAmount,
            int vatRate,
            decimal? vatAmount,
            decimal? totalAmount,
            int? bankAccountId,
            string? paymentSource,
            string? documentNo,
            string? payeeOrSupplier,
            string? approvalType,
            bool isSplitPayment = false,
            decimal? officialPartAmount = null,
            int? officialVatRate = null,
            decimal? officialVatAmount = null,
            decimal? officialTotalAmount = null,
            int? officialAccountId = null,
            string? officialDocNo = null,
            decimal? unofficialPartAmount = null,
            int? unofficialAccountId = null,
            string? unofficialDocNo = null,
            IFormFile? documentFile = null)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["ErrorMessage"] = "Masraf başlığı zorunludur.";
                return RedirectToAction(nameof(Finance), new { id = projectId });
            }
            // AI Kuralları 8: Ayrık Tarih ve Saat Güvenli Birleştirme
            DateTime parsedDateTime = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(expenseDate) && DateTime.TryParse(expenseDate, out var dt))
            {
                parsedDateTime = dt;
            }
            if (!string.IsNullOrWhiteSpace(expenseTime) && TimeSpan.TryParse(expenseTime, out var ts))
            {
                parsedDateTime = parsedDateTime.Date.Add(ts);
            }
            // Belge / Fatura / Fiş Dosyası Yükleme
            string? photoPath = null;
            if (documentFile != null && documentFile.Length > 0)
            {
                var ext = Path.GetExtension(documentFile.FileName).ToLowerInvariant();
                var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".webp" };
                if (allowedExts.Contains(ext))
                {
                    var fileName = $"exp_{projectId}_{Guid.NewGuid():N}{ext}";
                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "expenses");
                    Directory.CreateDirectory(folder);
                    var fullPath = Path.Combine(folder, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await documentFile.CopyToAsync(stream);
                    }
                    photoPath = $"/uploads/expenses/{fileName}";
                }
            }
            var accounts = await EnsureProjectAccountsAsync(projectId, project.Name);
            var officialBank = accounts.FirstOrDefault(a => a.IBAN != null && (a.IBAN.Contains("TRY") || a.IBAN.StartsWith("TR")) || a.BankName.Contains("Resmi")) ?? accounts[0];
            var siteCashVault = accounts.FirstOrDefault(a => a != officialBank && (a.IBAN != null && (a.IBAN.Contains("SNT") || a.IBAN.Contains("NAKIT")) || a.BankName.Contains("Şantiye"))) ?? (accounts.Count > 1 ? accounts[1] : accounts[0]);
            if (isSplitPayment && (officialPartAmount.GetValueOrDefault() > 0 || unofficialPartAmount.GetValueOrDefault() > 0))
            {
                // DUAL SPLIT PAYMENT (Parçalı Hakediş Çıkışı: 350k Resmi Banka + 150k Elden Şantiye)
                decimal offNet = officialPartAmount.GetValueOrDefault();
                int offVatRate = officialVatRate.GetValueOrDefault(20);
                decimal offVat = officialVatAmount.HasValue && officialVatAmount.Value > 0 ? officialVatAmount.Value : Math.Round(offNet * offVatRate / 100m, 2);
                decimal offTotal = officialTotalAmount.HasValue && officialTotalAmount.Value > 0 ? officialTotalAmount.Value : (offNet + offVat);
                decimal unoffAmount = unofficialPartAmount.GetValueOrDefault();
                var targetOfficialAcc = accounts.FirstOrDefault(a => a.Id == (officialAccountId ?? officialBank.Id)) ?? officialBank;
                var targetUnofficialAcc = accounts.FirstOrDefault(a => a.Id == (unofficialAccountId ?? siteCashVault.Id)) ?? siteCashVault;
                // 1. Resmi Masraf Kaydı
                if (offTotal > 0)
                {
                    var officialExp = new ConstructionProjectExpense
                    {
                        AgencyId = agencyId.Value,
                        ProjectId = projectId,
                        Title = $"{title.Trim()} (Resmi Hakediş / Banka)",
                        ExpenseType = (ConstructionExpenseType)expenseType,
                        Description = $"{description?.Trim()} [Parçalı Hakediş - Resmi Kanat]".Trim(),
                        AmountExcludingVat = offNet,
                        VatRate = offVatRate,
                        VatAmount = offVat,
                        Amount = offTotal,
                        ExpenseDate = parsedDateTime,
                        PaymentSource = targetOfficialAcc.BankName,
                        DocumentNo = !string.IsNullOrWhiteSpace(officialDocNo) ? officialDocNo.Trim() : documentNo?.Trim(),
                        PayeeOrSupplier = payeeOrSupplier?.Trim(),
                        PhotoPath = photoPath,
                        IsPaid = true,
                        ApprovalStatus = "Approved",
                        ApprovalType = "Official",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.ConstructionProjectExpenses.Add(officialExp);
                    targetOfficialAcc.CurrentBalance -= offTotal;
                    targetOfficialAcc.UpdatedAt = DateTime.UtcNow;
                }
                // 2. Elden / Şantiye Nakit Kaydı
                if (unoffAmount > 0)
                {
                    var unofficialExp = new ConstructionProjectExpense
                    {
                        AgencyId = agencyId.Value,
                        ProjectId = projectId,
                        Title = $"{title.Trim()} (Elden Şantiye Nakit Payı)",
                        ExpenseType = (ConstructionExpenseType)expenseType,
                        Description = $"{description?.Trim()} [Parçalı Hakediş - Sözleşme Dışı Elden Nakit]".Trim(),
                        AmountExcludingVat = unoffAmount,
                        VatRate = 0,
                        VatAmount = 0m,
                        Amount = unoffAmount,
                        ExpenseDate = parsedDateTime,
                        PaymentSource = targetUnofficialAcc.BankName,
                        DocumentNo = !string.IsNullOrWhiteSpace(unofficialDocNo) ? unofficialDocNo.Trim() : $"ELDEN-{DateTime.Now:yyMMddHHmm}",
                        PayeeOrSupplier = payeeOrSupplier?.Trim(),
                        PhotoPath = null,
                        IsPaid = true,
                        ApprovalStatus = "Approved",
                        ApprovalType = "Unofficial", // GİZLENEBİLİR ELDEN MOD
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.ConstructionProjectExpenses.Add(unofficialExp);
                    targetUnofficialAcc.CurrentBalance -= unoffAmount;
                    targetUnofficialAcc.UpdatedAt = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Çift kanallı hakediş çıkışı yapıldı: Resmi Bankadan {offTotal:N2} ₺ ve Şantiye Kasasından {unoffAmount:N2} ₺ başarıyla düşüldü.";
                return RedirectToAction(nameof(Finance), new { id = projectId });
            }
            else
            {
                // NORMAL TEK KANALLI MASRAF ÇIKIŞI
                if (vatRate < 0) vatRate = 0;
                if (!vatAmount.HasValue || vatAmount.Value == 0)
                {
                    vatAmount = Math.Round(netAmount * vatRate / 100m, 2);
                }
                if (!totalAmount.HasValue || totalAmount.Value == 0)
                {
                    totalAmount = netAmount + (vatAmount ?? 0m);
                }
                var targetAcc = accounts.FirstOrDefault(a => a.Id == (bankAccountId ?? officialBank.Id)) ?? officialBank;
                string effApproval = !string.IsNullOrWhiteSpace(approvalType) ? approvalType : "Official";
                var expense = new ConstructionProjectExpense
                {
                    AgencyId = agencyId.Value,
                    ProjectId = projectId,
                    Title = title.Trim(),
                    ExpenseType = (ConstructionExpenseType)expenseType,
                    Description = description?.Trim(),
                    AmountExcludingVat = netAmount,
                    VatRate = vatRate,
                    VatAmount = vatAmount,
                    Amount = totalAmount.Value,
                    ExpenseDate = parsedDateTime,
                    PaymentSource = targetAcc.BankName,
                    DocumentNo = documentNo?.Trim(),
                    PayeeOrSupplier = payeeOrSupplier?.Trim(),
                    PhotoPath = photoPath,
                    IsPaid = true,
                    ApprovalStatus = "Approved",
                    ApprovalType = effApproval,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ConstructionProjectExpenses.Add(expense);
                targetAcc.CurrentBalance -= totalAmount.Value;
                targetAcc.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"'{expense.Title}' masrafı ({netAmount:N2} ₺ + %{vatRate} KDV) başarıyla kaydedildi.";
                return RedirectToAction(nameof(Finance), new { id = projectId });
            }
        }
        public async Task<IActionResult> AddProjectDeposit(
 
            int projectId,
 
            int bankAccountId,
 
            decimal amount,
 
            string? depositorName,
 
            DateTime? transactionDate,
 
            string? notes,
 
            IFormFile? receiptFile,
 
            string? depositCategory = null,
 
            string? accountKind = null)
 
        {
 
            var agencyId = await GetUserAgencyIdAsync();
 
            if (agencyId == null) return Unauthorized();
 
 
 
            var project = await _context.ConstructionProjects
 
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
 
            if (project == null) return NotFound();
 
 
 
            var account = await _context.BankAccounts
 
                .FirstOrDefaultAsync(b => b.Id == bankAccountId && b.OwnerType == OwnerType.ConstructionProject && b.OwnerId == projectId && !b.IsDeleted);
 
 
 
            if (account == null)
 
            {
 
                TempData["ErrorMessage"] = "Hedef kasa / hesap bulunamadı.";
 
                return RedirectToAction(nameof(Finance), new { id = projectId });
 
            }
 
 
 
            if (amount <= 0)
 
            {
 
                TempData["ErrorMessage"] = "Geçerli bir aktarım tutarı giriniz.";
 
                return RedirectToAction(nameof(Finance), new { id = projectId });
 
            }
 
 
 
            // Kasa Bakiyesini Arttır
 
            account.CurrentBalance += amount;
 
            account.UpdatedAt = DateTime.UtcNow;
 
 
 
            // Dosya Yükleme (Dekont)
 
            string? docPath = null;
 
            string? docName = null;
 
            if (receiptFile != null && receiptFile.Length > 0)
 
            {
 
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "projects", projectId.ToString(), "receipts");
 
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
 
                var ext = Path.GetExtension(receiptFile.FileName);
 
                var fileName = $"dekont_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{ext}";
 
                var fullPath = Path.Combine(uploadDir, fileName);
 
                using (var stream = new FileStream(fullPath, FileMode.Create))
 
                {
 
                    await receiptFile.CopyToAsync(stream);
 
                }
 
                docPath = $"/uploads/projects/{projectId}/receipts/{fileName}";
 
                docName = receiptFile.FileName;
 
            }
 
 
 
            // Ortak Avansı Takibi: YALNIZCA Ortak Sermaye / Avansı ise veya kategori belirtilmemişse güncellenir!
 
            // Daire satışı, Yıkım hurda ihalesi ve diğer proje gelirleri ortak borç/avansını etkilemez.
 
            string finalCategory = !string.IsNullOrWhiteSpace(depositCategory) ? depositCategory.Trim() : "Ortak Sermaye / Avansı";
 
            bool isPartnerAdvance = finalCategory.Contains("Ortak") || finalCategory.Contains("Sermaye") || finalCategory.Contains("Avans");
 
 
 
            if (isPartnerAdvance && !string.IsNullOrWhiteSpace(depositorName))
 
            {
 
                if (depositorName.Contains("Ahmet") || depositorName.Contains("Mustafa") || depositorName.Contains("Ortak") || depositorName.Contains("Partner"))
 
                {
 
                    project.PartnerVaultAdvance = (project.PartnerVaultAdvance ?? 0m) + amount;
 
                }
 
                else if (depositorName.Contains("Biz") || depositorName.Contains("Ana Yüklenici") || depositorName.Contains("Özkaynak"))
 
                {
 
                    project.OurVaultAdvance = (project.OurVaultAdvance ?? 0m) + amount;
 
                }
 
                project.UpdatedAt = DateTime.UtcNow;
 
            }
 
 
 
            await _context.SaveChangesAsync();
 
 
 
            string detectedCurrency = "TRY";
 
            string currencySymbol = "₺";
 
            var combinedAcc = $"{account.AccountName} {account.BankName} {account.IBAN}".ToUpperInvariant();
 
            if (combinedAcc.Contains("USD") || combinedAcc.Contains("DOLAR") || combinedAcc.Contains("$"))
 
            {
 
                detectedCurrency = "USD";
 
                currencySymbol = "$";
 
            }
 
            else if (combinedAcc.Contains("EUR") || combinedAcc.Contains("EURO") || combinedAcc.Contains("AVRO") || combinedAcc.Contains("€"))
 
            {
 
                detectedCurrency = "EUR";
 
                currencySymbol = "€";
 
            }
 
 
 
            var latestRate = await _context.ExchangeRates.OrderByDescending(r => r.Date).FirstOrDefaultAsync();
 
            decimal usdRate = latestRate?.UsdRate > 0 ? latestRate.UsdRate : 49.6540m;
 
            decimal eurRate = latestRate?.EurRate > 0 ? latestRate.EurRate : 55.5997m;
 
            decimal rate = detectedCurrency == "USD" ? usdRate : (detectedCurrency == "EUR" ? eurRate : 1m);
 
            decimal amountTL = Math.Round(amount * rate, 2);
 
            string accKind = !string.IsNullOrWhiteSpace(accountKind) ? accountKind.Trim() : ((account.IBAN != null && account.IBAN.StartsWith("TR")) || (account.BankName != null && (account.BankName.Contains("Resmi") || account.BankName.Contains("Banka"))) ? "Resmi" : "Elden");
 
 
 
            // Hareket Kaydı
 
            var transactions = await LoadProjectTransactionsAsync(projectId);
 
            transactions.Add(new ProjectAccountTransactionDto
 
            {
 
                ProjectId = projectId,
 
                BankAccountId = account.Id,
 
                AccountName = $"{account.BankName} - {account.AccountName}",
 
                TransactionType = "Giris",
 
                Amount = amount,
 
                Currency = detectedCurrency,
 
                ExchangeRate = rate > 1 ? rate : null,
 
                AmountTL = amountTL,
 
                AccountKind = accKind,
 
                TransactionDate = transactionDate ?? DateTime.Today,
 
                CategoryOrPurpose = finalCategory,
 
                PartyName = string.IsNullOrWhiteSpace(depositorName) ? "Proje Kasası" : depositorName.Trim(),
 
                Description = notes?.Trim(),
 
                ReceiptDocumentPath = docPath,
 
                ReceiptDocumentName = docName,
 
                BalanceAfter = account.CurrentBalance,
 
                CreatedAt = DateTime.UtcNow
 
            });
 
            await SaveProjectTransactionsAsync(projectId, transactions);
 
 
 
            TempData["SuccessMessage"] = $"'{account.AccountName}' hesabına {amount:N2} {currencySymbol} para girişi başarıyla işlendi.";
 
            return RedirectToAction(nameof(Finance), new { id = projectId });
 
        }
 
 
 
        // POST: /ConstructionProject/AddProjectWithdrawal
 
        [HttpPost]
 
        [ValidateAntiForgeryToken]
 
        public async Task<IActionResult> AddProjectWithdrawal(
 
            int projectId,
 
            int sourceBankAccountId,
 
            decimal amount,
 
            string outflowCategory,
 
            int? targetBankAccountId,
 
            string? recipientName,
 
            DateTime? transactionDate,
 
            string? notes,
 
            IFormFile? receiptFile,
 
            int vatRate = 0,
 
            string? accountKind = null,
 
            string? paymentMethod = "IBAN",
 
            string? recipientIban = null,
 
            string? recipientBank = null)
 
        {
 
            var agencyId = await GetUserAgencyIdAsync();
 
            if (agencyId == null) return Unauthorized();
 
 
 
            var project = await _context.ConstructionProjects
 
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
 
            if (project == null) return NotFound();
 
 
 
            var sourceAccount = await _context.BankAccounts
 
                .FirstOrDefaultAsync(b => b.Id == sourceBankAccountId && b.OwnerType == OwnerType.ConstructionProject && b.OwnerId == projectId && !b.IsDeleted);
 
 
 
            if (sourceAccount == null)
 
            {
 
                TempData["ErrorMessage"] = "Kaynak hesap / kasa bulunamadı.";
 
                return RedirectToAction(nameof(Finance), new { id = projectId });
 
            }
 
 
 
            if (amount <= 0)
 
            {
 
                TempData["ErrorMessage"] = "Geçerli bir çıkış tutarı giriniz.";
 
                return RedirectToAction(nameof(Finance), new { id = projectId });
 
            }
 
 
 
            // Dosya Yükleme (Dekont)
 
            string? docPath = null;
 
            string? docName = null;
 
            if (receiptFile != null && receiptFile.Length > 0)
 
            {
 
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "projects", projectId.ToString(), "receipts");
 
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
 
                var ext = Path.GetExtension(receiptFile.FileName);
 
                var fileName = $"cikis_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{ext}";
 
                var fullPath = Path.Combine(uploadDir, fileName);
 
                using (var stream = new FileStream(fullPath, FileMode.Create))
 
                {
 
                    await receiptFile.CopyToAsync(stream);
 
                }
 
                docPath = $"/uploads/projects/{projectId}/receipts/{fileName}";
 
                docName = receiptFile.FileName;
 
            }
 
 
 
            sourceAccount.CurrentBalance -= amount;
 
            sourceAccount.UpdatedAt = DateTime.UtcNow;
 
 
 
            string srcCurrency = "TRY";
 
            string srcSymbol = "₺";
 
            var combinedSrc = $"{sourceAccount.AccountName} {sourceAccount.BankName} {sourceAccount.IBAN}".ToUpperInvariant();
 
            if (combinedSrc.Contains("USD") || combinedSrc.Contains("DOLAR") || combinedSrc.Contains("$"))
 
            {
 
                srcCurrency = "USD";
 
                srcSymbol = "$";
 
            }
 
            else if (combinedSrc.Contains("EUR") || combinedSrc.Contains("EURO") || combinedSrc.Contains("AVRO") || combinedSrc.Contains("€"))
 
             {
 
                srcCurrency = "EUR";
 
                srcSymbol = "€";
 
            }
 
 
 
            BankAccount? targetAccount = null;
 
            if (outflowCategory == "Virman" && targetBankAccountId.HasValue)
 
            {
 
                targetAccount = await _context.BankAccounts
 
                    .FirstOrDefaultAsync(b => b.Id == targetBankAccountId.Value && b.OwnerType == OwnerType.ConstructionProject && b.OwnerId == projectId && !b.IsDeleted);
 
                if (targetAccount != null)
 
                {
 
                    targetAccount.CurrentBalance += amount;
 
                    targetAccount.UpdatedAt = DateTime.UtcNow;
 
                }
 
            }
 
 
 
            await _context.SaveChangesAsync();
 
 
 
            var latestRateW = await _context.ExchangeRates.OrderByDescending(r => r.Date).FirstOrDefaultAsync();
 
            decimal usdRateW = latestRateW?.UsdRate > 0 ? latestRateW.UsdRate : 49.6540m;
 
            decimal eurRateW = latestRateW?.EurRate > 0 ? latestRateW.EurRate : 55.5997m;
 
            decimal rateW = srcCurrency == "USD" ? usdRateW : (srcCurrency == "EUR" ? eurRateW : 1m);
 
            decimal amountTLW = Math.Round(amount * rateW, 2);
 
 
 
            decimal? vatAmount = null;
 
            decimal? netAmount = null;
 
            if (vatRate > 0)
 
            {
 
                vatAmount = Math.Round(amount - (amount / (1m + (vatRate / 100m))), 2);
 
                netAmount = amount - vatAmount;
 
             }
 
            else
 
            {
 
                netAmount = amount;
 
            }
 
 
 
            string accKindW = accountKind ?? ((sourceAccount.IBAN != null && sourceAccount.IBAN.StartsWith("TR")) || (sourceAccount.BankName != null && (sourceAccount.BankName.Contains("Resmi") || sourceAccount.BankName.Contains("Banka"))) ? "Resmi" : "Elden");
 
 
 
            // Hareket Kaydı (Kaynak Hesap İçin Çıkış)
 
            var transactions = await LoadProjectTransactionsAsync(projectId);
 
            string party = recipientName ?? (targetAccount != null ? targetAccount.AccountName : outflowCategory);
 
            
 
            transactions.Add(new ProjectAccountTransactionDto
 
            {
 
                ProjectId = projectId,
 
                BankAccountId = sourceAccount.Id,
 
                AccountName = $"{sourceAccount.BankName} - {sourceAccount.AccountName}",
 
                TransactionType = "Cikis",
 
                Amount = amount,
 
                Currency = srcCurrency,
 
                ExchangeRate = rateW > 1 ? rateW : null,
 
                AmountTL = amountTLW,
 
                AccountKind = accKindW,
 
                VatRate = vatRate,
 
                VatAmount = vatAmount,
 
                NetAmount = netAmount,
 
                TransactionDate = transactionDate ?? DateTime.Today,
 
                CategoryOrPurpose = outflowCategory,
 
                PartyName = party,
 
                TargetBankAccountId = targetAccount?.Id,
 
                TargetAccountName = targetAccount != null ? $"{targetAccount.BankName} - {targetAccount.AccountName}" : null,
 
                PaymentMethod = paymentMethod ?? "IBAN",
 
                RecipientIban = recipientIban?.Trim(),
 
                RecipientBank = recipientBank?.Trim(),
 
                Description = notes?.Trim(),
 
                ReceiptDocumentPath = docPath,
 
                ReceiptDocumentName = docName,
 
                BalanceAfter = sourceAccount.CurrentBalance,
 
                CreatedAt = DateTime.UtcNow
 
            });
 
 
 
            // Eğer virman ise hedef hesap için de Giriş kaydı oluştur
 
            if (targetAccount != null)
 
            {
 
                transactions.Add(new ProjectAccountTransactionDto
 
                {
 
                    ProjectId = projectId,
 
                    BankAccountId = targetAccount.Id,
 
                    AccountName = $"{targetAccount.BankName} - {targetAccount.AccountName}",
 
                    TransactionType = "Giris",
 
                    Amount = amount,
 
                    Currency = srcCurrency,
 
                    TransactionDate = transactionDate ?? DateTime.Today,
 
                    CategoryOrPurpose = "Virman Girişi",
 
                    PartyName = $"{sourceAccount.BankName} ({sourceAccount.AccountName})",
 
                    Description = $"Virman transferi: {notes?.Trim()}",
 
                    ReceiptDocumentPath = docPath,
 
                    ReceiptDocumentName = docName,
 
                    BalanceAfter = targetAccount.CurrentBalance,
 
                    CreatedAt = DateTime.UtcNow
 
                });
 
            }
 
 
 
            await SaveProjectTransactionsAsync(projectId, transactions);
 
 
 
            TempData["SuccessMessage"] = $"'{sourceAccount.AccountName}' hesabından {amount:N2} {srcSymbol} para çıkışı başarıyla işlendi.";
            return RedirectToAction(nameof(Finance), new { id = projectId });
 
        }
 
 
 
        // POST: /ConstructionProject/AddProjectBatchWithdrawals
 
        [HttpPost]
 
        [ValidateAntiForgeryToken]
 
        public async Task<IActionResult> AddProjectBatchWithdrawals([FromBody] ProjectBatchWithdrawalRequestDto req)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Json(new { success = false, message = "Oturum açmanız gerekiyor." });
 
            if (req == null || req.Items == null || req.Items.Count == 0)
            {
                return Json(new { success = false, message = "Kaydedilecek fatura/masraf satırı bulunamadı." });
             }
 
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == req.ProjectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return Json(new { success = false, message = "Proje bulunamadı." });
            var projectAccounts = await _context.BankAccounts
                .Where(b => b.OwnerType == OwnerType.ConstructionProject && b.OwnerId == req.ProjectId && !b.IsDeleted)
                .ToListAsync();
 
            if (projectAccounts.Count == 0) return Json(new { success = false, message = "Projeye ait kasa veya banka hesabı bulunamadı." });
 
            var defaultSourceAccount = (req.SourceBankAccountId.HasValue && req.SourceBankAccountId.Value > 0)
                ? projectAccounts.FirstOrDefault(b => b.Id == req.SourceBankAccountId.Value) ?? projectAccounts.First()
                : projectAccounts.First();
            var validItems = req.Items.Where(x => x.Amount > 0 && !string.IsNullOrWhiteSpace(x.RecipientName)).ToList();
            if (validItems.Count == 0)
            {
                return Json(new { success = false, message = "Lütfen tutar ve muhatap bilgisi girilmiş en az bir satır ekleyiniz." });
            }
 
            var latestRateW = await _context.ExchangeRates.OrderByDescending(r => r.Date).FirstOrDefaultAsync();
            decimal usdRateW = latestRateW?.UsdRate > 0 ? latestRateW.UsdRate : 49.6540m;
            decimal eurRateW = latestRateW?.EurRate > 0 ? latestRateW.EurRate : 55.5997m;
 
            var transactions = await LoadProjectTransactionsAsync(req.ProjectId);
            decimal totalDeducted = 0m;
 
            foreach (var item in validItems)
             {
                var account = (item.BankAccountId.HasValue && item.BankAccountId.Value > 0)
                    ? projectAccounts.FirstOrDefault(b => b.Id == item.BankAccountId.Value) ?? defaultSourceAccount
                    : defaultSourceAccount;
 
                string srcCurrency = "TRY";
                var combinedName = $"{account.AccountName} {account.BankName} {account.IBAN}".ToUpperInvariant();
                if (combinedName.Contains("USD") || combinedName.Contains("DOLAR") || combinedName.Contains("$")) srcCurrency = "USD";
                else if (combinedName.Contains("EUR") || combinedName.Contains("EURO") || combinedName.Contains("AVRO") || combinedName.Contains("€")) srcCurrency = "EUR";
 
                decimal rateW = srcCurrency == "USD" ? usdRateW : (srcCurrency == "EUR" ? eurRateW : 1m);
 
                decimal grossAmount = item.Amount;
                decimal? netAmount = null;
                decimal? vatAmount = null;
 
                bool isHaric = item.VatType != null && item.VatType.Equals("haric", StringComparison.OrdinalIgnoreCase);
                if (item.VatRate > 0)
                {
                    if (isHaric)
                    {
                        netAmount = item.Amount;
                        vatAmount = Math.Round(item.Amount * item.VatRate / 100m, 2);
                        grossAmount = item.Amount + vatAmount.Value;
                    }
                    else
                    {
                        grossAmount = item.Amount;
                        vatAmount = Math.Round(item.Amount - (item.Amount / (1m + (item.VatRate / 100m))), 2);
                        netAmount = grossAmount - vatAmount.Value;
                    }
                }
                else
                {
                    grossAmount = item.Amount;
                    netAmount = item.Amount;
                    vatAmount = 0m;
                }
 
                account.CurrentBalance -= grossAmount;
                account.UpdatedAt = DateTime.UtcNow;
                totalDeducted += grossAmount;
 
                decimal amountTLW = Math.Round(grossAmount * rateW, 2);
 
                bool isBankaResmi = (!string.IsNullOrWhiteSpace(account.IBAN) && account.IBAN.Trim().StartsWith("TR")) ||
                                    (!string.IsNullOrWhiteSpace(account.BankName) && (account.BankName.Contains("Resmi") || (!account.BankName.Contains("Kasa") && !account.BankName.Contains("Elden"))));
 
                string accKindW = isBankaResmi ? "Resmi" : "Elden";
                string payMethodW = !string.IsNullOrWhiteSpace(item.PaymentMethod) ? item.PaymentMethod : (isBankaResmi ? "IBAN" : "Elden");
 
                transactions.Add(new ProjectAccountTransactionDto
                {
                    ProjectId = req.ProjectId,
                    BankAccountId = account.Id,
                    AccountName = $"{account.BankName} - {account.AccountName}",
                    TransactionType = "Cikis",
                    Amount = grossAmount,
                    Currency = srcCurrency,
                    ExchangeRate = rateW > 1 ? rateW : null,
                    AmountTL = amountTLW,
                    AccountKind = accKindW,
                    VatRate = item.VatRate,
                    VatAmount = vatAmount,
                    NetAmount = netAmount,
                    TransactionDate = item.TransactionDate ?? DateTime.Today,
                    CategoryOrPurpose = !string.IsNullOrWhiteSpace(item.OutflowCategory) ? item.OutflowCategory.Trim() : "Şantiye Harcaması",
                    PartyName = item.RecipientName.Trim(),
                    PaymentMethod = payMethodW,
                    RecipientIban = item.RecipientIban?.Trim(),
                    RecipientBank = item.RecipientBank?.Trim(),
                    Description = item.Notes?.Trim(),
                    BalanceAfter = account.CurrentBalance,
                    CreatedAt = DateTime.UtcNow
                });
             }
 
            await _context.SaveChangesAsync();
            await SaveProjectTransactionsAsync(req.ProjectId, transactions);
            return Json(new { success = true, count = validItems.Count, totalAmount = totalDeducted, message = $"{validItems.Count} adet fatura/masraf kaydı başarıyla işlendi ve kasalardan düşüldü." });
        }
 
        // POST: /ConstructionProject/AddProjectBatchDeposits
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProjectBatchDeposits([FromBody] ProjectBatchDepositRequestDto req)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Json(new { success = false, message = "Oturum açmanız gerekiyor." });
 
            if (req == null || req.Items == null || req.Items.Count == 0)
            {
                return Json(new { success = false, message = "Kaydedilecek para girişi/tahsilat satırı bulunamadı." });
            }
 
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == req.ProjectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return Json(new { success = false, message = "Proje bulunamadı." });
            var projectAccounts = await _context.BankAccounts
                .Where(b => b.OwnerType == OwnerType.ConstructionProject && b.OwnerId == req.ProjectId && !b.IsDeleted)
                .ToListAsync();
 
            if (projectAccounts.Count == 0) return Json(new { success = false, message = "Projeye ait kasa veya banka hesabı bulunamadı." });
 
            var defaultTargetAccount = (req.TargetBankAccountId.HasValue && req.TargetBankAccountId.Value > 0)
                ? projectAccounts.FirstOrDefault(b => b.Id == req.TargetBankAccountId.Value) ?? projectAccounts.First()
                : projectAccounts.First();
            var validItems = req.Items.Where(x => x.Amount > 0 && !string.IsNullOrWhiteSpace(x.SourceName)).ToList();
            if (validItems.Count == 0)
             {
                return Json(new { success = false, message = "Lütfen tutar ve muhatap bilgisi girilmiş en az bir satır ekleyiniz." });
            }
 
            var latestRateD = await _context.ExchangeRates.OrderByDescending(r => r.Date).FirstOrDefaultAsync();
            decimal usdRateD = latestRateD?.UsdRate > 0 ? latestRateD.UsdRate : 49.6540m;
            decimal eurRateD = latestRateD?.EurRate > 0 ? latestRateD.EurRate : 55.5997m;
 
            var transactions = await LoadProjectTransactionsAsync(req.ProjectId);
            decimal totalAdded = 0m;
 
            foreach (var item in validItems)
            {
                var account = (item.BankAccountId.HasValue && item.BankAccountId.Value > 0)
                    ? projectAccounts.FirstOrDefault(b => b.Id == item.BankAccountId.Value) ?? defaultTargetAccount
                    : defaultTargetAccount;
 
                string accCurrency = "TRY";
                var combinedName = $"{account.AccountName} {account.BankName} {account.IBAN}".ToUpperInvariant();
                if (combinedName.Contains("USD") || combinedName.Contains("DOLAR") || combinedName.Contains("$")) accCurrency = "USD";
                else if (combinedName.Contains("EUR") || combinedName.Contains("EURO") || combinedName.Contains("AVRO") || combinedName.Contains("€")) accCurrency = "EUR";
 
                decimal rateD = accCurrency == "USD" ? usdRateD : (accCurrency == "EUR" ? eurRateD : 1m);
 
                decimal grossAmount = item.Amount;
                decimal? netAmount = null;
                decimal? vatAmount = null;
 
                bool isHaric = item.VatType != null && item.VatType.Equals("haric", StringComparison.OrdinalIgnoreCase);
 
                if (item.VatRate > 0)
                {
                    if (isHaric)
                    {
                        netAmount = item.Amount;
                        vatAmount = Math.Round(item.Amount * item.VatRate / 100m, 2);
                        grossAmount = item.Amount + vatAmount.Value;
                    }
                    else
                    {
                        grossAmount = item.Amount;
                        vatAmount = Math.Round(item.Amount - (item.Amount / (1m + (item.VatRate / 100m))), 2);
                        netAmount = grossAmount - vatAmount.Value;
                    }
                }
                else
                {
                    grossAmount = item.Amount;
                    netAmount = item.Amount;
                    vatAmount = 0m;
                }
 
                account.CurrentBalance += grossAmount;
                account.UpdatedAt = DateTime.UtcNow;
                totalAdded += grossAmount;
 
                decimal amountTLD = Math.Round(grossAmount * rateD, 2);
 
                bool isBankaResmi = (!string.IsNullOrWhiteSpace(account.IBAN) && account.IBAN.Trim().StartsWith("TR")) ||
                                    (!string.IsNullOrWhiteSpace(account.BankName) && (account.BankName.Contains("Resmi") || (!account.BankName.Contains("Kasa") && !account.BankName.Contains("Elden"))));
 
                string accKindD = isBankaResmi ? "Resmi" : "Elden";
                string payMethodD = !string.IsNullOrWhiteSpace(item.PaymentMethod) ? item.PaymentMethod : (isBankaResmi ? "IBAN" : "Elden");
 
                transactions.Add(new ProjectAccountTransactionDto
                {
                    ProjectId = req.ProjectId,
                    BankAccountId = account.Id,
                    AccountName = $"{account.BankName} - {account.AccountName}",
                    TransactionType = "Giris",
                    Amount = grossAmount,
                    Currency = accCurrency,
                    ExchangeRate = rateD > 1 ? rateD : null,
                    AmountTL = amountTLD,
                    AccountKind = accKindD,
                    VatRate = item.VatRate,
                    VatAmount = vatAmount,
                    NetAmount = netAmount,
                    TransactionDate = item.TransactionDate ?? DateTime.Today,
                    CategoryOrPurpose = !string.IsNullOrWhiteSpace(item.InflowCategory) ? item.InflowCategory.Trim() : "Ortak Sermaye / Avansı",
                    PartyName = item.SourceName.Trim(),
                    PaymentMethod = payMethodD,
                    RecipientIban = item.SenderIban?.Trim(),
                    RecipientBank = item.SenderBank?.Trim(),
                    Description = item.Notes?.Trim(),
                    BalanceAfter = account.CurrentBalance,
                    CreatedAt = DateTime.UtcNow
                });
            }
 
            await _context.SaveChangesAsync();
            await SaveProjectTransactionsAsync(req.ProjectId, transactions);
 
            return Json(new { success = true, count = validItems.Count, totalAmount = totalAdded, message = $"{validItems.Count} adet para girişi/tahsilat kaydı başarıyla işlendi ve kasalara eklendi." });
        }
 
        // GET: /ConstructionProject/GetAccountTransactions
 
        [HttpGet]
 
        public async Task<IActionResult> GetAccountTransactions(int projectId, int bankAccountId)
 
        {
 
            var all = await LoadProjectTransactionsAsync(projectId);
 
            var filtered = all.Where(t => t.BankAccountId == bankAccountId)
 
                              .OrderByDescending(t => t.TransactionDate)
 
                              .ThenByDescending(t => t.CreatedAt)
 
                              .Select(t => new {
 
                                  id = t.Id,
 
                                  date = t.TransactionDate.ToString("dd.MM.yyyy"),
 
                                  type = t.TransactionType,
 
                                  category = t.CategoryOrPurpose,
 
                                  party = t.PartyName,
 
                                  amount = t.Amount.ToString("N2"),
 
                                  balanceAfter = t.BalanceAfter.ToString("N2"),
 
                                  description = t.Description ?? "",
 
                                  receiptPath = t.ReceiptDocumentPath ?? "",
 
                                  receiptName = t.ReceiptDocumentName ?? ""
 
                              })
 
                              .ToList();
 
            return Json(new { success = true, transactions = filtered });
 
        }
 
 
 
        private string GetProjectTransactionsFilePath(int projectId)
 
         {
 
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "projects", projectId.ToString());
 
            if (!Directory.Exists(uploadDir))
 
            {
 
                Directory.CreateDirectory(uploadDir);
 
            }
 
            return Path.Combine(uploadDir, "account_transactions.json");
 
        }
 
 
 
        private async Task<List<ProjectAccountTransactionDto>> LoadProjectTransactionsAsync(int projectId)
 
        {
 
            var filePath = GetProjectTransactionsFilePath(projectId);
 
            if (!System.IO.File.Exists(filePath))
 
            {
 
                return new List<ProjectAccountTransactionDto>();
 
            }
 
            try
 
            {
 
                var json = await System.IO.File.ReadAllTextAsync(filePath);
 
                return System.Text.Json.JsonSerializer.Deserialize<List<ProjectAccountTransactionDto>>(json) 
 
                       ?? new List<ProjectAccountTransactionDto>();
 
            }
 
            catch
 
            {
 
                return new List<ProjectAccountTransactionDto>();
 
            }
 
        }
 
 
 
        private async Task SaveProjectTransactionsAsync(int projectId, List<ProjectAccountTransactionDto> list)
 
        {
 
            var filePath = GetProjectTransactionsFilePath(projectId);
 
            var json = System.Text.Json.JsonSerializer.Serialize(list, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
 
            await System.IO.File.WriteAllTextAsync(filePath, json);
 
        }
 
 
 
        private string GetProjectExpectedTransactionsFilePath(int projectId)
 
        {
 
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "projects", projectId.ToString());
 
            if (!Directory.Exists(uploadDir))
 
            {
 
                Directory.CreateDirectory(uploadDir);
 
            }
 
            return Path.Combine(uploadDir, "expected_transactions.json");
 
        }
 
 
 
        private async Task<List<ProjectExpectedTransactionDto>> LoadProjectExpectedTransactionsAsync(int projectId)
 
        {
 
            var filePath = GetProjectExpectedTransactionsFilePath(projectId);
 
            if (!System.IO.File.Exists(filePath))
 
             {
 
                return new List<ProjectExpectedTransactionDto>();
 
            }
 
            try
 
            {
 
                var json = await System.IO.File.ReadAllTextAsync(filePath);
 
                return System.Text.Json.JsonSerializer.Deserialize<List<ProjectExpectedTransactionDto>>(json) 
 
                       ?? new List<ProjectExpectedTransactionDto>();
 
             }
 
            catch
 
            {
 
                return new List<ProjectExpectedTransactionDto>();
 
            }
 
        }
 
 
 
        private async Task SaveProjectExpectedTransactionsAsync(int projectId, List<ProjectExpectedTransactionDto> list)
 
        {
 
            var filePath = GetProjectExpectedTransactionsFilePath(projectId);
 
            var json = System.Text.Json.JsonSerializer.Serialize(list, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
 
            await System.IO.File.WriteAllTextAsync(filePath, json);
 
        }
 
 
 
        // POST: /ConstructionProject/AddExpectedTransaction
 
        [HttpPost]
 
        [ValidateAntiForgeryToken]
 
        public async Task<IActionResult> AddExpectedTransaction(
 
            int projectId,
 
            string direction,
 
            string partyName,
 
            DateTime date1,
 
            DateTime? date2,
 
            string accountType,
 
            int? targetBankAccountId,
 
            decimal amount,
 
            string? currency,
 
            string categoryOrPurpose,
 
            string? notes,
 
            IFormFile? documentFile,
 
            string? paymentMethod = "IBAN",
 
            string? recipientIban = null,
 
            string? recipientBank = null)
 
        {
 
            var agencyId = await GetUserAgencyIdAsync();
 
            if (agencyId == null) return Unauthorized();
 
 
 
            var project = await _context.ConstructionProjects
 
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
 
            if (project == null) return NotFound();
 
 
 
            if (amount <= 0)
 
            {
 
                TempData["ErrorMessage"] = "Geçerli bir tutar giriniz.";
 
                return RedirectToAction(nameof(Finance), new { id = projectId });
 
            }
 
 
 
            string? docPath = null;
 
            if (documentFile != null && documentFile.Length > 0)
 
            {
 
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "projects", projectId.ToString(), "expected_docs");
 
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
 
                var ext = Path.GetExtension(documentFile.FileName);
 
                var fileName = $"beklenen_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{ext}";
 
                var fullPath = Path.Combine(uploadDir, fileName);
 
                using (var stream = new FileStream(fullPath, FileMode.Create))
 
                {
 
                    await documentFile.CopyToAsync(stream);
 
                }
 
                docPath = $"/uploads/projects/{projectId}/expected_docs/{fileName}";
 
            }
 
 
 
            string? targetAccName = null;
 
            if (targetBankAccountId.HasValue)
 
            {
 
                var acc = await _context.BankAccounts.FirstOrDefaultAsync(b => b.Id == targetBankAccountId.Value && b.OwnerId == projectId && !b.IsDeleted);
 
                if (acc != null) targetAccName = $"{acc.BankName} - {acc.AccountName}";
 
            }
 
 
 
            var list = await LoadProjectExpectedTransactionsAsync(projectId);
 
            list.Add(new ProjectExpectedTransactionDto
 
            {
 
                ProjectId = projectId,
 
                Direction = direction == "Inflow" ? "Inflow" : "Outflow",
 
                PartyName = string.IsNullOrWhiteSpace(partyName) ? (direction == "Inflow" ? "Gelir Kaynağı" : "Muhatap") : partyName.Trim(),
 
                Date1 = date1,
 
                Date2 = date2,
 
                AccountType = accountType == "Elden" ? "Elden" : "Resmi",
 
                TargetBankAccountId = targetBankAccountId,
 
                TargetAccountName = targetAccName,
 
                Amount = amount,
 
                Currency = string.IsNullOrWhiteSpace(currency) ? "TRY" : currency.Trim().ToUpperInvariant(),
 
                CategoryOrPurpose = categoryOrPurpose?.Trim() ?? "",
 
                PaymentMethod = paymentMethod ?? "IBAN",
 
                RecipientIban = recipientIban?.Trim(),
 
                RecipientBank = recipientBank?.Trim(),
 
                Description = notes?.Trim(),
 
                DocumentPath = docPath,
 
                IsCompleted = false,
 
                CreatedAt = DateTime.UtcNow
 
            });
 
 
 
            await SaveProjectExpectedTransactionsAsync(projectId, list);
 
            TempData["SuccessMessage"] = direction == "Inflow" ? "Beklenen tahsilat kaydı başarıyla eklendi." : "Beklenen ödeme kaydı başarıyla eklendi.";
 
            return RedirectToAction(nameof(Finance), new { id = projectId });
 
        }
 
 
 
        // POST: /ConstructionProject/CompleteExpectedTransaction
 
        [HttpPost]
 
        [ValidateAntiForgeryToken]
 
        public async Task<IActionResult> CompleteExpectedTransaction(
            int projectId, 
            string expectedId, 
            int? bankAccountId,
            bool sendSms = false,
            string? notifyPhone = null,
            bool sendEmail = false,
            string? notifyEmail = null,
            string? customMessage = null)
 
        {
 
            var agencyId = await GetUserAgencyIdAsync();
 
            if (agencyId == null) return Unauthorized();
 
 
 
            var project = await _context.ConstructionProjects
 
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
 
            if (project == null) return NotFound();
 
 
 
            var expectedList = await LoadProjectExpectedTransactionsAsync(projectId);
 
            var item = expectedList.FirstOrDefault(e => e.Id == expectedId && !e.IsCompleted);
 
            if (item == null)
 
            {
 
                TempData["ErrorMessage"] = "Beklenen işlem kaydı bulunamadı veya zaten tamamlanmış.";
 
                return RedirectToAction(nameof(Finance), new { id = projectId });
 
            }
 
 
 
            int targetAccId = bankAccountId ?? item.TargetBankAccountId ?? 0;
 
            var account = await _context.BankAccounts.FirstOrDefaultAsync(b => b.Id == targetAccId && b.OwnerId == projectId && !b.IsDeleted);
 
            if (account == null)
 
            {
 
                account = await _context.BankAccounts.FirstOrDefaultAsync(b => b.OwnerId == projectId && !b.IsDeleted);
 
            }
 
 
 
            if (account == null)
 
            {
 
                TempData["ErrorMessage"] = "İşlemin aktarılacağı aktif bir hesap/kasa bulunamadı.";
 
                return RedirectToAction(nameof(Finance), new { id = projectId });
 
            }
 
 
 
            // Kur bilgisi
 
            var latestRate = await _context.ExchangeRates.OrderByDescending(r => r.Date).FirstOrDefaultAsync();
 
            decimal usdRate = latestRate?.UsdRate > 0 ? latestRate.UsdRate : 49.6540m;
 
            decimal eurRate = latestRate?.EurRate > 0 ? latestRate.EurRate : 55.5997m;
 
            decimal rate = item.Currency == "USD" ? usdRate : (item.Currency == "EUR" ? eurRate : 1m);
 
            decimal amountTL = Math.Round(item.Amount * rate, 2);
 
 
 
            // Kasa bakiyesini güncelle ve hareket ekle
 
            var transactions = await LoadProjectTransactionsAsync(projectId);
 
 
 
            if (item.Direction == "Inflow")
 
            {
 
                account.CurrentBalance += item.Amount;
 
                account.UpdatedAt = DateTime.UtcNow;
 
 
 
                var newTx = new ProjectAccountTransactionDto
 
                {
 
                    ProjectId = projectId,
 
                    BankAccountId = account.Id,
 
                    AccountName = $"{account.BankName} - {account.AccountName}",
 
                    TransactionType = "Giris",
 
                    Amount = item.Amount,
 
                    Currency = item.Currency,
 
                    ExchangeRate = rate > 1 ? rate : null,
 
                    AmountTL = amountTL,
 
                    AccountKind = item.AccountType,
 
                    TransactionDate = DateTime.Today,
 
                    CategoryOrPurpose = item.CategoryOrPurpose,
 
                    PartyName = item.PartyName,
 
                    Description = $"{item.Description} (Beklenen girişten tamamlandı)".Trim(),
 
                    ReceiptDocumentPath = item.DocumentPath,
 
                    BalanceAfter = account.CurrentBalance,
 
                    CreatedAt = DateTime.UtcNow
 
                };
 
                transactions.Add(newTx);
 
                item.CompletedTransactionId = newTx.Id;
 
            }
 
            else
 
            {
 
                account.CurrentBalance -= item.Amount;
 
                account.UpdatedAt = DateTime.UtcNow;
 
 
 
                var newTx = new ProjectAccountTransactionDto
 
                {
 
                    ProjectId = projectId,
 
                    BankAccountId = account.Id,
 
                    AccountName = $"{account.BankName} - {account.AccountName}",
 
                    TransactionType = "Cikis",
 
                    Amount = item.Amount,
 
                    Currency = item.Currency,
 
                    ExchangeRate = rate > 1 ? rate : null,
 
                    AmountTL = amountTL,
 
                    AccountKind = item.AccountType,
 
                    TransactionDate = DateTime.Today,
 
                    CategoryOrPurpose = item.CategoryOrPurpose,
 
                    PartyName = item.PartyName,
 
                    PaymentMethod = item.PaymentMethod,
 
                    RecipientIban = item.RecipientIban,
 
                    RecipientBank = item.RecipientBank,
 
                    Description = $"{item.Description} (Beklenen çıkıştan ödendi)".Trim(),
 
                    ReceiptDocumentPath = item.DocumentPath,
 
                    BalanceAfter = account.CurrentBalance,
 
                    CreatedAt = DateTime.UtcNow
 
                };
 
                transactions.Add(newTx);
 
                item.CompletedTransactionId = newTx.Id;
 
            }
 
 
 
            item.IsCompleted = true;
 
            await _context.SaveChangesAsync();
 
            await SaveProjectTransactionsAsync(projectId, transactions);
 
            
            // Otomatik SMS / E-Posta Bildirimi
            if (sendSms && !string.IsNullOrWhiteSpace(notifyPhone))
            {
                var smsService = HttpContext.RequestServices.GetService<GMK360.Web.Services.Sms.ISmsService>();
                if (smsService != null)
                {
                    try
                    {
                        string textToSend = !string.IsNullOrWhiteSpace(customMessage) 
                            ? customMessage.Trim() 
                            : $"Sayın {item.PartyName}, {project.Name} projesi kapsamında {item.Amount:N2} {item.Currency} tutarındaki {item.CategoryOrPurpose} ödemeniz banka hesabınıza aktarılmıştır. GMK Yapı";
                        await smsService.SmsGonderAsync(notifyPhone.Trim(), textToSend);
                    }
                    catch { }
                }
            }
            if (sendEmail && !string.IsNullOrWhiteSpace(notifyEmail))
            {
                var emailService = HttpContext.RequestServices.GetService<GMK360.Core.Interfaces.IEmailService>();
                if (emailService != null)
                {
                    try
                    {
                        string emailBody = $"<h3>Ödeme Bilgilendirmesi</h3><p>Sayın <b>{item.PartyName}</b>,</p><p>{project.Name} projesi kapsamında <b>{item.Amount:N2} {item.Currency}</b> tutarındaki {item.CategoryOrPurpose} ödemeniz banka hesabınıza aktarılmıştır.</p><p>Detay: {item.Description}</p><br><p>Bilgilerinize sunarız.<br><b>GMK Yapı Yönetimi</b></p>";
                        await emailService.SendEmailAsync(notifyEmail.Trim(), $"{project.Name} - Kira / Ödeme Bilgilendirmesi", emailBody);
                    }
                    catch { }
                }
            }
await SaveProjectExpectedTransactionsAsync(projectId, expectedList);
 
 
 
            TempData["SuccessMessage"] = item.Direction == "Inflow" 
 
                ? $"'{item.PartyName}' tahsilatı başarıyla kasaya işlendi." 
 
                : $"'{item.PartyName}' ödemesi kasadan başarıyla düşüldü.";
 
 
 
            return RedirectToAction(nameof(Finance), new { id = projectId });
 
        }
 
 
 
        // POST: /ConstructionProject/DeleteExpectedTransaction
 
        [HttpPost]
 
        [ValidateAntiForgeryToken]
 
        public async Task<IActionResult> DeleteExpectedTransaction(int projectId, string expectedId)
 
        {
 
            var agencyId = await GetUserAgencyIdAsync();
 
            if (agencyId == null) return Unauthorized();
 
 
 
            var list = await LoadProjectExpectedTransactionsAsync(projectId);
 
            var item = list.FirstOrDefault(e => e.Id == expectedId);
 
            if (item != null)
 
            {
 
                list.Remove(item);
 
                await SaveProjectExpectedTransactionsAsync(projectId, list);
 
                TempData["SuccessMessage"] = "Kayıt başarıyla silindi.";
 
            }
 
 
 
            return RedirectToAction(nameof(Finance), new { id = projectId });
        }
        // POST: /ConstructionProject/ImportRentScheduleFromAnalysis
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportRentScheduleFromAnalysis(int projectId, int monthCount = 12, bool includeRelocation = true)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
            // 1. Hedef Banka Hesabı (Varsayılan TL Proje Hesabı)
            var account = await _context.BankAccounts
                .FirstOrDefaultAsync(b => b.OwnerType == OwnerType.ConstructionProject && b.OwnerId == projectId && !b.IsDeleted && (b.BankName.Contains("deniz") || b.AccountName.Contains("TL")));
            if (account == null)
            {
                account = await _context.BankAccounts.FirstOrDefaultAsync(b => b.OwnerType == OwnerType.ConstructionProject && b.OwnerId == projectId && !b.IsDeleted);
            }
            // 2. Hak Sahipleri
            var stakeholders = await _context.ProjectStakeholders
                .Include(s => s.User)
                .Where(s => s.ProjectId == projectId && !s.IsDeleted && (int)s.Role == 1)
                .ToListAsync();
            if (stakeholders.Count == 0)
            {
                TempData["ErrorMessage"] = "Ön Analiz aşamasında kayıtlı hak sahibi bulunamadı.";
                return RedirectToAction(nameof(Finance), new { id = projectId });
            }
            // 3. Kira & Taşınma Politikası
            var rentPolicy = new ProjectRentPolicyDto();
            if (!string.IsNullOrWhiteSpace(project.RentPolicyJson))
            {
                try { rentPolicy = System.Text.Json.JsonSerializer.Deserialize<ProjectRentPolicyDto>(project.RentPolicyJson) ?? new ProjectRentPolicyDto(); } catch { }
            }
            var expectedList = await LoadProjectExpectedTransactionsAsync(projectId);
            int addedCount = 0;
            DateTime startDate = DateTime.Today; // Başlangıç ayı (ör. Kasım 2026)
            int paymentDay = rentPolicy.FlatPaymentDay > 0 ? rentPolicy.FlatPaymentDay : 5;
            foreach (var sh in stakeholders)
            {
                string ownerName = sh.User?.DisplayName ?? (sh.User != null ? $"{sh.User.FirstName} {sh.User.LastName}".Trim() : "Hak Sahibi");
                if (string.IsNullOrWhiteSpace(ownerName)) ownerName = "Hak Sahibi";
                string phone = sh.User?.PhoneNumber ?? "";
                string email = sh.User?.Email ?? "";
                string iban = !string.IsNullOrWhiteSpace(sh.Iban) ? sh.Iban.Trim() : "";
                string bank = !string.IsNullOrWhiteSpace(sh.BankName) ? sh.BankName.Trim() : "Banka";
                string unitStr = !string.IsNullOrWhiteSpace(sh.UnitNumbers) ? sh.UnitNumbers.Trim().ToUpperInvariant() : "DAİRE";
                decimal monthlyRent = sh.MonthlyRentAllowance.GetValueOrDefault(rentPolicy.FlatMonthlyRent);
                decimal relocFee = sh.RelocationAllowance.GetValueOrDefault(rentPolicy.FlatRelocationFee);
                // 1. Taşınma Desteği (İlk Ay - Tek Seferlik)
                if (includeRelocation && relocFee > 0)
                {
                    DateTime relocDate = new DateTime(startDate.Year, startDate.Month, Math.Min(paymentDay, DateTime.DaysInMonth(startDate.Year, startDate.Month)));
                    string relocDesc = $"{unitStr} Malik Taşınma Yardımı - {ownerName}" + (!string.IsNullOrWhiteSpace(iban) ? $" (IBAN: {iban} - {bank})" : "") + (!string.IsNullOrWhiteSpace(phone) ? $" [Tel: {phone}]" : "") + (!string.IsNullOrWhiteSpace(email) ? $" [Mail: {email}]" : "");
                    bool alreadyExists = expectedList.Any(e => e.PartyName.Contains(ownerName) && e.CategoryOrPurpose == "Taşınma Desteği" && e.Date1 == relocDate);
                    if (!alreadyExists)
                    {
                        expectedList.Add(new ProjectExpectedTransactionDto
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            ProjectId = projectId,
                            Direction = "Outflow",
                            PartyName = $"{ownerName} ({unitStr})",
                            PaymentMethod = !string.IsNullOrWhiteSpace(iban) ? "IBAN" : "Elden",
                            RecipientIban = iban,
                            RecipientBank = bank,
                            RecipientPhone = phone,
                            RecipientEmail = email,
                            Date1 = relocDate,
                            AccountType = "Resmi",
                            TargetBankAccountId = account?.Id,
                            TargetAccountName = account != null ? (account.AccountName ?? account.BankName ?? "Proje Hesabı") : "Proje Hesabı",
                            Amount = relocFee,
                            Currency = "TRY",
                            CategoryOrPurpose = "Taşınma Desteği",
                            Description = "-",
                            IsCompleted = false,
                            CreatedAt = DateTime.UtcNow
                        });
                        addedCount++;
                    }
                }
                // 2. Aylık Kira Ödemeleri (monthCount ay boyunca - Vade Tarihi ve ID bazlı sıralanır)
                for (int m = 0; m < monthCount; m++)
                {
                    DateTime curMonth = startDate.AddMonths(m);
                    DateTime dueDate = new DateTime(curMonth.Year, curMonth.Month, Math.Min(paymentDay, DateTime.DaysInMonth(curMonth.Year, curMonth.Month)));
                    string monthName = curMonth.ToString("MMMM yyyy", new System.Globalization.CultureInfo("tr-TR"));
                    string rentDesc = $"{unitStr} {monthName} Kira Ödemesi - {ownerName}" + (!string.IsNullOrWhiteSpace(iban) ? $" (IBAN: {iban} - {bank})" : "") + (!string.IsNullOrWhiteSpace(phone) ? $" [Tel: {phone}]" : "") + (!string.IsNullOrWhiteSpace(email) ? $" [Mail: {email}]" : "");
                    bool alreadyExists = expectedList.Any(e => e.PartyName.Contains(ownerName) && e.CategoryOrPurpose.Contains("Kira") && e.Date1 == dueDate);
                    if (!alreadyExists)
                    {
                        expectedList.Add(new ProjectExpectedTransactionDto
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            ProjectId = projectId,
                            Direction = "Outflow",
                            PartyName = $"{ownerName} ({unitStr})",
                            PaymentMethod = !string.IsNullOrWhiteSpace(iban) ? "IBAN" : "Elden",
                            RecipientIban = iban,
                            RecipientBank = bank,
                            RecipientPhone = phone,
                            RecipientEmail = email,
                            Date1 = dueDate,
                            AccountType = "Resmi",
                            TargetBankAccountId = account?.Id,
                            TargetAccountName = account != null ? (account.AccountName ?? account.BankName ?? "Proje Hesabı") : "Proje Hesabı",
                            Amount = monthlyRent,
                            Currency = "TRY",
                            CategoryOrPurpose = $"Kira Desteği ({m + 1}. Ay)",
                            Description = "-",
                            IsCompleted = false,
                            CreatedAt = DateTime.UtcNow
                        });
                        addedCount++;
                    }
                }
            }
            await SaveProjectExpectedTransactionsAsync(projectId, expectedList);
            TempData["SuccessMessage"] = $"Ön Analiz'deki hak sahipleri için toplam {addedCount} adet kira ve taşınma ödemesi beklenenler takvimine başarıyla aktarıldı.";
            return RedirectToAction(nameof(Finance), new { id = projectId });
        }
        // POST: /ConstructionProject/SendTransactionNotification
 
        [HttpPost]
 
        [ValidateAntiForgeryToken]
 
        public async Task<IActionResult> SendTransactionNotification(int projectId, string recipientName, string? phone, string? email, string messageText, string channel = "sms")
 
        {
 
            var agencyId = await GetUserAgencyIdAsync();
 
            if (agencyId == null) return Unauthorized();
 
 
 
            if (string.IsNullOrWhiteSpace(messageText))
 
            {
 
                return Json(new { success = false, message = "Mesaj metni boş olamaz." });
 
            }
 
 
 
            bool smsSent = false;
 
            bool emailSent = false;
 
 
 
            if ((channel == "sms" || channel == "both") && !string.IsNullOrWhiteSpace(phone))
 
            {
 
                var smsService = HttpContext.RequestServices.GetService<GMK360.Web.Services.Sms.ISmsService>();
 
                if (smsService != null)
 
                 {
 
                    var smsRes = await smsService.SmsGonderAsync(phone.Trim(), messageText.Trim());
 
                    smsSent = smsRes != null && smsRes.Contains("Başarılı");
                }
 
             }
 
 
 
            if ((channel == "email" || channel == "both") && !string.IsNullOrWhiteSpace(email))
 
            {
 
                var emailService = HttpContext.RequestServices.GetService<GMK360.Core.Interfaces.IEmailService>();
 
                if (emailService != null)
 
                {
 
                    try
 
                    {
 
                        await emailService.SendEmailAsync(email.Trim(), "GMK360 Proje Finans Bildirimi", messageText.Trim());
 
                        emailSent = true;
 
                    }
 
                    catch { }
 
                }
 
            }
 
 
 
            return Json(new
 
            {
 
                success = true,
 
                message = "Bildirim başarıyla iletildi.",
 
                smsSent,
 
                emailSent
 
            });
 
        }
 
 
 
        // POST: /ConstructionProject/DeleteProjectExpense
 
 
 
 
 
 
 
        [HttpPost]
 
 
 
 
 
 
 
        [ValidateAntiForgeryToken]
 
 
 
 
 
 
 
        public async Task<IActionResult> DeleteProjectExpense(int id, int projectId)
 
 
 
 
 
 
 
        {
 
 
 
 
 
 
 
            var agencyId = await GetUserAgencyIdAsync();
 
 
 
 
 
 
 
            if (agencyId == null) return Unauthorized();
 
 
 
 
 
 
 
            var expense = await _context.ConstructionProjectExpenses
 
 
 
 
 
 
 
                .FirstOrDefaultAsync(e => e.Id == id && e.ProjectId == projectId && e.AgencyId == agencyId.Value && !e.IsDeleted);
 
 
 
 
 
 
 
            if (expense == null)
 
 
 
 
 
 
 
            {
 
 
 
 
 
 
 
                TempData["ErrorMessage"] = "Silinecek masraf kaydı bulunamadı.";
 
 
 
 
 
 
 
                return RedirectToAction(nameof(Finance), new { id = projectId });
 
 
 
 
 
 
 
            }
 
 
 
 
 
 
 
            expense.IsDeleted = true;
 
 
 
 
 
 
 
            expense.UpdatedAt = DateTime.UtcNow;
 
 
 
 
 
 
 
            var accounts = await EnsureProjectAccountsAsync(projectId, "Proje");
 
 
 
 
 
 
 
            var matchingAccount = accounts.FirstOrDefault(a => a.BankName == expense.PaymentSource) ?? accounts.FirstOrDefault();
 
 
 
 
 
 
 
            if (matchingAccount != null)
 
 
 
 
 
 
 
            {
 
 
 
 
 
 
 
                matchingAccount.CurrentBalance += expense.Amount;
 
 
 
 
 
 
 
                matchingAccount.UpdatedAt = DateTime.UtcNow;
 
 
 
 
 
 
 
            }
 
 
 
 
 
 
 
            await _context.SaveChangesAsync();
 
 
 
 
 
 
 
            TempData["SuccessMessage"] = "Masraf kaydı silindi ve tutar kasaya iade edildi.";
 
 
 
 
 
 
 
            return RedirectToAction(nameof(Finance), new { id = projectId });
 
 
 
 
 
 
 
        }
 
 
 
 
 
 
 
                // POST: /ConstructionProject/CreateProjectAccount
 
        [HttpPost]
 
        [ValidateAntiForgeryToken]
 
        public async Task<IActionResult> CreateProjectAccount(
 
            int projectId,
 
            string accountType,
 
            string? accountName,
 
            string? bankName,
 
            string? branchCode,
 
            string? iban,
 
            string currency,
 
            decimal openingBalance,
 
            string? accountKind = "Resmi")
 
        {
 
            var agencyId = await GetUserAgencyIdAsync();
 
            if (agencyId == null) return Unauthorized();
 
 
 
            var project = await _context.ConstructionProjects
 
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
 
            if (project == null) return NotFound();
 
 
 
            accountType = (accountType ?? "Banka Hesabı").Trim();
 
            var curr = string.IsNullOrWhiteSpace(currency) ? "TRY" : currency.Trim().ToUpperInvariant();
 
 
 
            string finalBankName;
 
            string finalAccountName;
 
            string finalIban;
 
 
 
            accountKind = string.IsNullOrWhiteSpace(accountKind) ? "Resmi" : accountKind.Trim();
 
 
 
            // Banka, Şube ve Kasa/IBAN bilgileri elden/özel kasada da korunur (banka kasalarında tutulduğu için)
 
            var bName = string.IsNullOrWhiteSpace(bankName) ? (accountKind == "Elden" ? "Özel Kasa" : "Banka") : bankName.Trim();
 
            if (!string.IsNullOrWhiteSpace(branchCode))
 
            {
 
                bName += $" (Şube: {branchCode.Trim()})";
 
            }
 
 
 
            if (accountKind == "Elden")
 
            {
 
                if (!bName.Contains("Özel") && !bName.Contains("Elden") && !bName.Contains("Kasa"))
 
                {
 
                    bName += " (Özel Kasa / Elden)";
 
                }
 
            }
 
 
 
            finalBankName = bName;
 
            finalAccountName = string.IsNullOrWhiteSpace(accountName) 
 
                ? (accountKind == "Elden" ? $"{bName} Özel Kasası" : $"{bName} Hesabı") 
 
                : accountName.Trim();
 
            finalIban = !string.IsNullOrWhiteSpace(iban) ? iban.Trim() : (accountKind == "Elden" ? "ÖZEL KASA" : "");
 
 
 
            if (curr == "USD")
 
            {
 
                if (!finalAccountName.Contains("USD", StringComparison.OrdinalIgnoreCase) && !finalAccountName.Contains("Dolar", StringComparison.OrdinalIgnoreCase) && !finalAccountName.Contains("$"))
 
                {
 
                    finalAccountName += " (USD)";
 
                }
 
            }
 
            else if (curr == "EUR")
 
            {
 
                if (!finalAccountName.Contains("EUR", StringComparison.OrdinalIgnoreCase) && !finalAccountName.Contains("Euro", StringComparison.OrdinalIgnoreCase) && !finalAccountName.Contains("€"))
 
                {
 
                    finalAccountName += " (EUR)";
 
                }
 
            }
 
 
 
            var account = new BankAccount
 
            {
 
                OwnerType = OwnerType.ConstructionProject,
 
                OwnerId = projectId,
 
                AccountName = finalAccountName,
 
                BankName = finalBankName,
 
                IBAN = finalIban,
 
                CurrentBalance = Math.Max(0m, openingBalance),
 
                IsActive = true,
 
                CreatedAt = DateTime.UtcNow
 
            };
 
 
 
            _context.BankAccounts.Add(account);
 
            await _context.SaveChangesAsync();
 
 
 
            TempData["SuccessMessage"] = $"'{account.AccountName}' hesabı/kasası başarıyla açıldı.";
 
            return RedirectToAction(nameof(Finance), new { id = projectId });
 
        }
 
 
 
        // POST: /ConstructionProject/UpdateProjectAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProjectAccount(int? id, int? accountId, int projectId, string accountName, string? bankName, string? branchCode, string? iban, string? currency = null, string? accountKind = "Resmi")
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
 
            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
 
            var targetId = id ?? accountId;
            if (!targetId.HasValue)
            {
                TempData["ErrorMessage"] = "Hesap ID bulunamadı.";
                return RedirectToAction(nameof(Finance), new { id = projectId });
            }
 
            var account = await _context.BankAccounts
                .FirstOrDefaultAsync(b => b.Id == targetId.Value && b.OwnerType == OwnerType.ConstructionProject && b.OwnerId == projectId && !b.IsDeleted);
 
            if (account != null)
            {
                var cleanName = string.IsNullOrWhiteSpace(accountName) ? account.AccountName : accountName.Trim();
                var curr = (currency ?? "").Trim().ToUpperInvariant();
 
                if (!string.IsNullOrWhiteSpace(curr))
                {
                    cleanName = cleanName.Replace(" (USD)", "", StringComparison.OrdinalIgnoreCase)
                                         .Replace(" [USD]", "", StringComparison.OrdinalIgnoreCase)
                                         .Replace(" (EUR)", "", StringComparison.OrdinalIgnoreCase)
                                         .Replace(" [EUR]", "", StringComparison.OrdinalIgnoreCase);
 
                    if (curr == "USD")
                    {
                        if (!cleanName.Contains("USD", StringComparison.OrdinalIgnoreCase) && !cleanName.Contains("Dolar", StringComparison.OrdinalIgnoreCase) && !cleanName.Contains("$"))
                        {
                            cleanName += " (USD)";
                        }
                    }
                    else if (curr == "EUR")
                    {
                        if (!cleanName.Contains("EUR", StringComparison.OrdinalIgnoreCase) && !cleanName.Contains("Euro", StringComparison.OrdinalIgnoreCase) && !cleanName.Contains("€"))
                        {
                            cleanName += " (EUR)";
                        }
                    }
                }
 
                account.AccountName = cleanName;
 
                accountKind = string.IsNullOrWhiteSpace(accountKind) ? "Resmi" : accountKind.Trim();
 
                if (accountKind.Equals("Elden", StringComparison.OrdinalIgnoreCase))
                {
                    var bName = !string.IsNullOrWhiteSpace(bankName) ? bankName.Trim() : "Özel Kasa";
                    if (!bName.Contains("Özel", StringComparison.OrdinalIgnoreCase) && 
                        !bName.Contains("Elden", StringComparison.OrdinalIgnoreCase) && 
                        !bName.Contains("Kasa", StringComparison.OrdinalIgnoreCase))
                    {
                        bName += " (Özel Kasa / Elden)";
                    }
                    if (!string.IsNullOrWhiteSpace(branchCode)) bName += $" (Şube: {branchCode.Trim()})";
                    account.BankName = bName;
 
                    if (string.IsNullOrWhiteSpace(iban))
                    {
                        account.IBAN = "ÖZEL KASA / ELDEN";
                    }
                    else
                    {
                        account.IBAN = iban.Trim();
                    }
                }
                else
                {
                    // Resmi Banka Hesabı
                    var bName = !string.IsNullOrWhiteSpace(bankName) ? bankName.Trim() : "Banka";
                    bName = bName.Replace(" (Özel Kasa / Elden)", "", StringComparison.OrdinalIgnoreCase)
                                 .Replace(" (Elden / Kasa)", "", StringComparison.OrdinalIgnoreCase)
                                 .Replace(" (Özel Kasa)", "", StringComparison.OrdinalIgnoreCase);
                    if (!string.IsNullOrWhiteSpace(branchCode)) bName += $" (Şube: {branchCode.Trim()})";
                    account.BankName = bName;
                    var cleanIban = (iban ?? "").Trim();
                    if (cleanIban.Equals("ÖZEL KASA / ELDEN", StringComparison.OrdinalIgnoreCase) || 
                        cleanIban.Equals("ÖZEL KASA", StringComparison.OrdinalIgnoreCase) ||
                        cleanIban.Equals("ELDEN", StringComparison.OrdinalIgnoreCase))
                    {
                        cleanIban = "";
                    }
                    account.IBAN = cleanIban;
                }
 
                account.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"'{account.AccountName}' hesabı ({accountKind}) başarıyla güncellendi.";
            }
 
            return RedirectToAction(nameof(Finance), new { id = projectId });
        }
 
        // POST: /ConstructionProject/DeleteProjectAccount
 
        [HttpPost]
 
        [ValidateAntiForgeryToken]
 
        public async Task<IActionResult> DeleteProjectAccount(int id, int projectId)
 
        {
 
            var agencyId = await GetUserAgencyIdAsync();
 
            if (agencyId == null) return Unauthorized();
 
 
 
            var project = await _context.ConstructionProjects
 
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
 
            if (project == null) return NotFound();
 
 
 
            var account = await _context.BankAccounts
 
                .FirstOrDefaultAsync(b => b.Id == id && b.OwnerType == OwnerType.ConstructionProject && b.OwnerId == projectId && !b.IsDeleted);
 
 
 
            if (account != null)
 
            {
 
                account.IsDeleted = true;
 
                account.UpdatedAt = DateTime.UtcNow;
 
                await _context.SaveChangesAsync();
 
                TempData["SuccessMessage"] = $"'{account.AccountName}' başarıyla silindi.";
 
            }
 
 
 
            return RedirectToAction(nameof(Finance), new { id = projectId });
 
        }
 
 
 
        // GET: /ConstructionProject/GetHistoricalExchangeRate
 
        [HttpGet]
 
        public async Task<IActionResult> GetHistoricalExchangeRate(string date)
 
        {
 
            DateTime targetDate = DateTime.Today;
 
            if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var parsed))
 
            {
 
                targetDate = parsed;
 
            }
 
 
 
            // O güne veya en yakın geçmiş tarihe ait TCMB kurunu bul
 
            var rate = await _context.ExchangeRates
 
                .Where(r => r.Date.Date <= targetDate.Date)
 
                .OrderByDescending(r => r.Date)
 
                .FirstOrDefaultAsync();
 
 
 
            if (rate == null)
 
            {
 
                rate = await _context.ExchangeRates.OrderBy(r => r.Date).FirstOrDefaultAsync();
 
            }
 
 
 
            decimal usd = rate?.UsdRate > 0 ? rate.UsdRate : 49.6540m;
 
            decimal eur = rate?.EurRate > 0 ? rate.EurRate : 55.5997m;
 
            string actualDate = rate?.Date.ToString("dd.MM.yyyy") ?? targetDate.ToString("dd.MM.yyyy");
 
 
 
            return Json(new
 
            {
 
                success = true,
 
                requestedDate = targetDate.ToString("dd.MM.yyyy"),
 
                rateDate = actualDate,
 
                usdRate = usd.ToString("N4"),
 
                eurRate = eur.ToString("N4")
 
            });
 
        }
        // GET: /ConstructionProject/Archive/5
        public async Task<IActionResult> Archive(int? id, string? category = null)
        {
            if (id == null) return NotFound();
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .Include(p => p.BinaYoneticisi)
                .Include(p => p.Agency)
                .FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
            ViewData["ProjectId"] = project.Id;
            ViewData["ProjectName"] = project.Name;
            ViewData["ProjectStatus"] = project.Status;
            if (project.CityId.HasValue)
            {
                var city = await _context.Cities.FirstOrDefaultAsync(c => c.Id == project.CityId.Value);
                ViewBag.CityName = city?.Name;
            }
            if (project.DistrictId.HasValue)
            {
                var dist = await _context.Districts.FirstOrDefaultAsync(d => d.Id == project.DistrictId.Value);
                ViewBag.DistrictName = dist?.Name;
            }
            if (project.NeighborhoodId.HasValue)
            {
                var neigh = await _context.Neighborhoods.FirstOrDefaultAsync(n => n.Id == project.NeighborhoodId.Value);
                ViewBag.NeighborhoodName = neigh?.Name;
            }
            var query = _context.DocumentArchives
                .Where(d => d.ProjectId == id && !d.IsDeleted);
            if (!string.IsNullOrWhiteSpace(category) && category != "Tümü")
            {
                query = query.Where(d => d.Category == category);
            }
            var docs = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();
            ViewBag.Documents = docs;
            ViewBag.ActiveCategory = category ?? "Tümü";
            return View(project);
        }
        // GET: /ConstructionProject/Meetings/5
        // ADAY PROJEYE AİT TOPLANTI VE GÖRÜŞMELER (MERKEZLE ENTEGRE, PROJE SİDEBAR'INI KORUR)
        public async Task<IActionResult> Meetings(int? id, byte? statusId)
        {
            if (id == null) return NotFound();
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            var project = await _context.ConstructionProjects
                .Include(p => p.BinaYoneticisi)
                .FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();
            ViewData["ProjectId"] = project.Id;
            ViewData["ProjectName"] = project.Name;
            ViewData["ProjectStatus"] = project.Status;
            ViewData["Title"] = project.Name + " - Görüşmeler ve Toplantılar";
            if (project.CityId.HasValue)
            {
                var city = await _context.Cities.FirstOrDefaultAsync(c => c.Id == project.CityId.Value);
                ViewBag.CityName = city?.Name;
            }
            if (project.DistrictId.HasValue)
            {
                var dist = await _context.Districts.FirstOrDefaultAsync(d => d.Id == project.DistrictId.Value);
                ViewBag.DistrictName = dist?.Name;
            }
            if (project.NeighborhoodId.HasValue)
            {
                var neigh = await _context.Neighborhoods.FirstOrDefaultAsync(n => n.Id == project.NeighborhoodId.Value);
                ViewBag.NeighborhoodName = neigh?.Name;
            }
            var query = _context.AgendaRecords
                .Include(a => a.Phonebook)
                .Include(a => a.Items)
                .Include(a => a.Participants)
                .Where(a => a.AgencyId == agencyId.Value && a.ProjectId == id && !a.IsDeleted);
            if (statusId.HasValue)
            {
                query = query.Where(a => a.StatusId == statusId.Value);
            }
            var records = await query.OrderByDescending(a => a.EventDate).ToListAsync();
            ViewBag.Meetings = records;
            ViewBag.SelectedStatusId = statusId;
            ViewBag.Phonebook = await _context.AgencyPhonebooks
                .Where(p => p.AgencyId == agencyId.Value && !p.IsDeleted)
                .OrderBy(p => p.Name)
                .ToListAsync();
            ViewBag.Stakeholders = await _context.ProjectStakeholders
                .Include(s => s.User)
                .Where(s => s.ProjectId == id && !s.IsDeleted)
                .ToListAsync();
            return View(project);
        }
        // POST: /ConstructionProject/CreateProjectMeeting
        // TARİH VE SAAT AYRI ALANLARDAN GELİR (ALTIN KURAL)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProjectMeeting(int projectId, string title, string? notes, string eventDate, string? eventTime, byte recordTypeId = AgendaConstants.TypeMusteriGorusmesi, int? phonebookId = null, bool openImmediately = false)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["ErrorMessage"] = "Lütfen toplantı veya görüşme başlığı giriniz.";
                return RedirectToAction(nameof(Meetings), new { id = projectId });
            }
            // Tarih ve saat ayrı alanlardan güvenli birleştirilir
            DateTime parsedDateTime = DateTime.Now;
            if (DateTime.TryParse(eventDate, out var d))
            {
                if (!string.IsNullOrWhiteSpace(eventTime) && TimeSpan.TryParse(eventTime, out var t))
                {
                    parsedDateTime = d.Date.Add(t);
                }
                else
                {
                    parsedDateTime = d.Date.Add(DateTime.Now.TimeOfDay);
                }
            }
            var record = new AgendaRecord
            {
                AgencyId = agencyId.Value,
                ProjectId = projectId,
                Title = title.Trim(),
                Notes = notes?.Trim() ?? string.Empty,
                EventDate = parsedDateTime,
                RecordTypeId = recordTypeId,
                StatusId = AgendaConstants.StatusPlanlandi,
                PhonebookId = phonebookId,
                ImageUrl = string.Empty,
                CreatedAt = DateTime.Now,
                IsDeleted = false,
                Participants = new System.Collections.Generic.List<AgendaParticipant>()
            };
            if (phonebookId.HasValue && phonebookId.Value > 0)
            {
                var pb = await _context.AgencyPhonebooks.FirstOrDefaultAsync(p => p.Id == phonebookId.Value && p.AgencyId == agencyId.Value);
                if (pb != null)
                {
                    record.Participants.Add(new AgendaParticipant
                    {
                        PhonebookId = pb.Id,
                        UserId = pb.LinkedUserId,
                        AccessToken = Guid.NewGuid().ToString("N")
                    });
                }
            }
            _context.AgendaRecords.Add(record);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Toplantı/konu kaydı başarıyla oluşturuldu.";
            if (openImmediately)
            {
                return RedirectToAction("MeetingRoom", "Agenda", new { id = record.Id });
            }
            return RedirectToAction(nameof(Meetings), new { id = projectId });
        }
    }
}
