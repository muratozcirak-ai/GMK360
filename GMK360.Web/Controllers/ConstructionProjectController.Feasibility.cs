using System;



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



            ViewBag.MinistryUnitCost = 21500m; // 2026 ÇŞB III-B Grubu Yapı Yaklaşık Birim Fiyatı (₺/m²)



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



                .Where(d => d.ProjectId == id && d.Title == "Tapu Belgesi" && !d.IsDeleted)



                .OrderByDescending(d => d.UploadDate)



                .FirstOrDefaultAsync();



            ViewBag.TapuDocumentUrl = tapuDoc?.DocumentUrl;







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



            IFormFile? tapuDocumentFile)



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



            project.Address = address?.Trim();



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







            // Fotoğraf yükleme



            if (currentStateImageFile != null && currentStateImageFile.Length > 0)



            {



                var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", $"Agency_{agencyId.Value}", $"Project_{project.Id}", "images");



                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);







                var extension = Path.GetExtension(currentStateImageFile.FileName).ToLower();



                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };



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







            // Tapu Belgesi yükleme



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







            await _context.SaveChangesAsync();



            TempData["SuccessMessage"] = "Proje künyesi ve keşif bilgileri başarıyla güncellendi.";



            return RedirectToAction(nameof(Workspace), new { id = id, tab = "kunye" });



        }







        // POST: /ConstructionProject/UpdateProjectPartners



        [HttpPost]



        public async Task<IActionResult> UpdateProjectPartners(int id, string? projectOwnerContact)



        {



            var agencyId = await GetUserAgencyIdAsync();



            if (agencyId == null) return Unauthorized();







            var project = await _context.ConstructionProjects



                .FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == agencyId.Value && !p.IsDeleted);







            if (project == null) return NotFound();







            project.ProjectOwnerContact = string.IsNullOrWhiteSpace(projectOwnerContact) ? null : projectOwnerContact.Trim();



            await _context.SaveChangesAsync();







            TempData["SuccessMessage"] = "Hissedarlık bilgileri başarıyla güncellendi.";



            return RedirectToAction(nameof(Workspace), new { id = id });



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







            await _context.SaveChangesAsync();







            TempData["SuccessMessage"] = "Proje muhatap bilgileri başarıyla güncellendi.";



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







            // 3. 1. Aşama Bürokrasi Evrakları



            var initialDocs = await _context.ProjectLegalDocuments



                .Include(d => d.SystemTemplate)



                .Where(d => d.ConstructionProjectId == id && (d.Stage == "Ön Keşif" || d.Stage == "1. Aşama" || string.IsNullOrEmpty(d.Stage)))



                .ToListAsync();



            ViewBag.InitialDocs = initialDocs;



            ViewBag.TotalInitialFee = initialDocs.Sum(d => (d.DocumentFee ?? 0) + (d.AdditionalCost ?? 0));



            ViewBag.CompletedInitialCount = initialDocs.Count(d => d.Status == "Tamamlandı");







            var stageRules = await _context.ModuleDocumentRules



                .Include(r => r.Prerequisites)



                .ThenInclude(p => p.PrerequisiteTemplate)



                .Where(r => r.TargetModule == "Construction" && (r.Stage == "Ön Keşif" || r.Stage == "1. Aşama" || string.IsNullOrEmpty(r.Stage)))



                .ToListAsync();



            ViewBag.StageRules = stageRules;







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







            return View(project);



        }







        // POST: /ConstructionProject/UpdateAnalysisImar



        [HttpPost]



        public async Task<IActionResult> UpdateAnalysisImar(



            int id,



            double? totalLandArea,



            int? totalFloors,



            int? targetTotalApartments,



            decimal? averageFlatSalePrice,



            string? publicDescription)



        {



            var agencyId = await GetUserAgencyIdAsync();



            if (agencyId == null) return Unauthorized();







            var project = await _context.ConstructionProjects



                .FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == agencyId.Value && !p.IsDeleted);







            if (project == null) return NotFound();







            if (totalLandArea.HasValue && totalLandArea > 0) project.TotalLandArea = totalLandArea;



            project.TotalFloors = totalFloors;



            project.TargetTotalApartments = targetTotalApartments;



            if (averageFlatSalePrice.HasValue) project.AverageFlatSalePrice = averageFlatSalePrice.Value;



            project.PublicDescription = publicDescription?.Trim();







            await _context.SaveChangesAsync();







            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")



            {



                return Json(new { success = true, message = "İmar ve kapasite analizi başarıyla kaydedildi." });



            }







            TempData["SuccessMessage"] = "İmar ve kapasite analizi başarıyla kaydedildi.";



            return RedirectToAction(nameof(Analysis), new { id = id });



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







            return View(project);



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



            ViewBag.PendingFees = legalDocs.Where(d => d.Status != "Tamamlandı").Sum(d => (d.DocumentFee ?? 0) + (d.AdditionalCost ?? 0));







            return View(project);



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
        public async Task<IActionResult> CreateProjectMeeting(int projectId, string title, string? notes, string eventDate, string? eventTime, byte recordTypeId = AgendaConstants.TypeMusteriGorusmesi, int? phonebookId = null)
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
                IsDeleted = false
            };

            _context.AgendaRecords.Add(record);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Toplantı/görüşme kaydı başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Meetings), new { id = projectId });
        }

    }
}
