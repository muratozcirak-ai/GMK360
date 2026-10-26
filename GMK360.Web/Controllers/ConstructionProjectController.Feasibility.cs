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
        public async Task<IActionResult> UpdateProjectPartners(
            int id, 
            bool isJointVenture,
            string? partnerCompanyName,
            string? partnerContactPerson,
            string? partnerPhone,
            string? partnerEmail,
            decimal? ourSharePercentage,
            decimal? partnerSharePercentage)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == agencyId.Value && !p.IsDeleted);

            if (project == null) return NotFound();

            project.IsJointVenture = isJointVenture;

            if (isJointVenture)
            {
                project.PartnerCompanyName = string.IsNullOrWhiteSpace(partnerCompanyName) ? null : partnerCompanyName.Trim();
                project.PartnerContactPerson = string.IsNullOrWhiteSpace(partnerContactPerson) ? null : partnerContactPerson.Trim();
                project.PartnerPhone = string.IsNullOrWhiteSpace(partnerPhone) ? null : partnerPhone.Trim();
                project.PartnerEmail = string.IsNullOrWhiteSpace(partnerEmail) ? null : partnerEmail.Trim();

                decimal ourRatio = ourSharePercentage ?? 50m;
                decimal partnerRatio = partnerSharePercentage ?? 50m;

                if (ourRatio < 0) ourRatio = 0;
                if (ourRatio > 100) ourRatio = 100;
                if (partnerRatio < 0) partnerRatio = 0;
                if (partnerRatio > 100) partnerRatio = 100;

                project.OurSharePercentage = ourRatio;
                project.PartnerSharePercentage = partnerRatio;
                project.ProjectOwnerContact = $"Ortak Girişim (%{ourRatio:0.#} Biz / %{partnerRatio:0.#} {project.PartnerCompanyName ?? "Harici Ortak"})";
            }
            else
            {
                project.PartnerCompanyName = null;
                project.PartnerContactPerson = null;
                project.PartnerPhone = null;
                project.PartnerEmail = null;
                project.OurSharePercentage = 100m;
                project.PartnerSharePercentage = 0m;
                project.ProjectOwnerContact = "Tekil Yüklenici (%100)";
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = isJointVenture 
                ? "Ortaklık ve hisse oranları başarıyla kaydedildi." 
                : "Proje tekil yüklenici (%100) olarak kaydedildi.";

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

            decimal usdBalance = safetyBox != null ? safetyBox.CurrentBalance : 0m;
            decimal usdEquivalentTL = Math.Round(usdBalance * usdRate, 2);
            ViewBag.UsdBalance = usdBalance;
            ViewBag.UsdEquivalentTL = usdEquivalentTL;

            decimal officialBalance = officialBank?.CurrentBalance ?? 0m;
            decimal siteCashBalance = siteCashVault?.CurrentBalance ?? 0m;
            decimal consolidatedTotalTL = officialBalance + siteCashBalance + usdEquivalentTL;

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

        // POST: /ConstructionProject/AddProjectDeposit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProjectDeposit(
            int projectId,
            int bankAccountId,
            decimal amount,
            string? currency,
            string? depositorName,
            string? notes)
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

            // Ortak Avansı Takibi
            if (!string.IsNullOrWhiteSpace(depositorName))
            {
                if (depositorName.Contains("Ortak") || depositorName.Contains("Partner"))
                {
                    project.PartnerVaultAdvance = (project.PartnerVaultAdvance ?? 0m) + amount;
                }
                else
                {
                    project.OurVaultAdvance = (project.OurVaultAdvance ?? 0m) + amount;
                }
                project.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            string currSymbol = (currency == "USD" || account.IBAN.Contains("USD") || account.BankName.Contains("USD")) ? "$" : "₺";
            TempData["SuccessMessage"] = $"{account.BankName} hesabına {amount:N2} {currSymbol} para aktarımı başarıyla işlendi.";
            return RedirectToAction(nameof(Finance), new { id = projectId });
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

        // POST: /ConstructionProject/UpdateProjectAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProjectAccount(int projectId, int? accountId, string bankName, string accountName, string iban, decimal openingBalance)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId.Value && !p.IsDeleted);
            if (project == null) return NotFound();

            BankAccount? account = null;
            if (accountId.HasValue)
            {
                account = await _context.BankAccounts
                    .FirstOrDefaultAsync(b => b.Id == accountId.Value && b.OwnerType == OwnerType.ConstructionProject && b.OwnerId == projectId && !b.IsDeleted);
            }
            if (account == null)
            {
                var accounts = await EnsureProjectAccountsAsync(projectId, project.Name);
                account = accounts.FirstOrDefault();
            }

            if (account != null)
            {
                account.BankName = string.IsNullOrWhiteSpace(bankName) ? account.BankName : bankName.Trim();
                account.AccountName = string.IsNullOrWhiteSpace(accountName) ? account.AccountName : accountName.Trim();
                account.IBAN = string.IsNullOrWhiteSpace(iban) ? account.IBAN : iban.Trim();
                account.CurrentBalance = openingBalance;
                account.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Kasa / hesap bilgileri başarıyla güncellendi.";
            return RedirectToAction(nameof(Finance), new { id = projectId });
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
