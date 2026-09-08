using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Core.Entities.Identity;
using GMK360.Core.Entities.Construction;
using GMK360.Data.Contexts;
using System.Linq;
using System.Threading.Tasks;
using System;
using GMK360.Core.Entities;

namespace GMK360.Web.Controllers
{
    [Authorize(Roles = "InsaatFirmasi,Admin,Corporate")]
    public class ConstructionProjectController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IWebHostEnvironment _hostEnvironment;

        public ConstructionProjectController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }

        private async Task<int?> GetUserAgencyIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            var agencyConsultant = await _context.AgencyConsultants
                .FirstOrDefaultAsync(a => a.UserId == user.Id);
            
            return agencyConsultant?.AgencyId;
        }

        // GET: ConstructionProject
        public async Task<IActionResult> Index()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null)
            {
                // Admin tümünü görebilir
                if (User.IsInRole("Admin"))
                {
                    return View(await _context.ConstructionProjects.Include(c => c.Phases)
                .Include(c => c.Amenities).ToListAsync());
                }
                return RedirectToAction("SetupCorporateProfile", "CustomerDashboard");
            }

            var projects = await _context.ConstructionProjects
                .Include(c => c.Phases)
                .Include(c => c.Amenities)
                .Where(p => p.AgencyId == agencyId)
                .ToListAsync();

            return View(projects);
        }

        // GET: ConstructionProject/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.ConstructionProjects
                .Include(c => c.Phases)
                .Include(c => c.Amenities)
                .Include(c => c.Tasks)
                .Include(c => c.Blocks)
                    .ThenInclude(b => b.Units)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null) return NotFound();

            var agencyId = await GetUserAgencyIdAsync();
            if (!User.IsInRole("Admin") && project.AgencyId != agencyId)
            {
                return Unauthorized();
            }

            return View(project);
        }

        // GET: ConstructionProject/Create
        public async Task<IActionResult> Create(int? id = null) { 
            int? projectId = id; 
            var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync(); 
            ViewBag.Cities = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(cities, "Id", "Name"); 
            ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>()); 
            ViewBag.Neighborhoods = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>()); 
            ViewBag.Streets = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>()); 
            
            var model = new GMK360.Web.Models.CreateProjectWizardViewModel();

            if (projectId.HasValue) {
                var draft = await _context.ConstructionProjects.Include(p => p.Blocks).FirstOrDefaultAsync(p => p.Id == projectId.Value);
                if (draft != null) {
                    model.DraftProjectId = draft.Id;
                    model.Name = draft.Name;
                    model.Description = draft.Description;
                    model.Address = draft.Address;
                    model.StartDate = draft.StartDate;
                    model.EndDate = draft.EndDate ?? default(DateTime);
                    ViewBag.CoverImageUrl = draft.CoverImageUrl;
                    
                    var currentStateDoc = _context.DmsDocuments.FirstOrDefault(d => d.EntityType == "ConstructionProject" && d.EntityId == draft.Id && d.Title == "Mevcut Durum Görseli (İlk Hali)");
                    if (currentStateDoc != null) {
                        ViewBag.CurrentStateImageUrl = currentStateDoc.DocumentUrl;
                    }

                    model.TotalLandArea = draft.TotalLandArea;
                    model.TargetTotalApartments = draft.TargetTotalApartments;
                    model.TargetTotalShops = draft.TargetTotalShops;
                    model.Latitude = draft.Latitude;
                    model.Longitude = draft.Longitude;
                    model.CityId = draft.CityId ?? 0;
                    model.DistrictId = draft.DistrictId ?? 0;
                    model.NeighborhoodId = draft.NeighborhoodId ?? 0;
                    model.StreetId = draft.StreetId;

                    if (draft.Blocks != null && draft.Blocks.Any()) {
                        model.Blocks = draft.Blocks.Select(b => new GMK360.Web.Models.WizardBlockItem {
                            BlockName = b.BlockName,
                            BaseArea = b.BaseArea,
                            TotalFloors = b.TotalFloors ?? 0,
                            BasementFloors = b.BasementFloors,
                            TotalApartments = b.TotalApartments > 0 ? b.TotalApartments : b.TotalUnits, // Geriye dönük uyumluluk
                              TotalShops = b.TotalShops,
                            HasGroundFloor = b.HasGroundFloor,
                            HasRoof = b.HasRoof
                        }).ToList();
                    }

                    if (draft.CityId.HasValue) {
                        var districts = await _context.Districts.Where(d => d.CityId == draft.CityId).ToListAsync();
                        ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(districts, "Id", "Name", draft.DistrictId);
                    }
                    if (draft.DistrictId.HasValue) {
                        var hoods = await _context.Neighborhoods.Where(n => n.DistrictId == draft.DistrictId).ToListAsync();
                        ViewBag.Neighborhoods = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(hoods, "Id", "Name", draft.NeighborhoodId);
                    }
                    if (draft.NeighborhoodId.HasValue) {
                        var streets = await _context.Streets.Where(s => s.NeighborhoodId == draft.NeighborhoodId).ToListAsync();
                        ViewBag.Streets = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(streets, "Id", "Name", draft.StreetId);
                    }
                }
            }

            return View(model); 
        }

        // POST: ConstructionProject/Create
        // WIZARD CREATE ACTION
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        [HttpPost]
        public async Task<IActionResult> SaveStep1([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)
        {
            try {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null) return Json(new { success = false, message = "Yetkisiz erişim." });

                GMK360.Core.Entities.Construction.ConstructionProject project;
                if (model.DraftProjectId > 0)
                {
                    project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == model.DraftProjectId);
                    if (project == null) return Json(new { success = false, message = "Proje bulunamadı." });
                }
                else
                {
                    project = new GMK360.Core.Entities.Construction.ConstructionProject { AgencyId = agencyId.Value, Status = 0, CreatedAt = DateTime.UtcNow };
                    _context.ConstructionProjects.Add(project);
                }

                project.Name = model.Name;
                project.Description = model.Description;
                project.Address = model.Address ?? "Adres belirtilmedi";
                
                if (model.StartDate.Year > 1) project.StartDate = model.StartDate;
                if (model.EndDate.Year > 1) project.EndDate = model.EndDate;
                
                project.TotalLandArea = model.TotalLandArea;
                project.CityId = model.CityId;
                project.DistrictId = model.DistrictId;
                project.NeighborhoodId = model.NeighborhoodId;
                project.StreetId = model.StreetId;
                project.TargetTotalApartments = model.TargetTotalApartments;
                project.TargetTotalShops = model.TargetTotalShops;
                project.Latitude = model.Latitude;
                project.Longitude = model.Longitude;

                if (model.CoverImageFile != null && model.CoverImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "projects", "covers");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.CoverImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CoverImageFile.CopyToAsync(fileStream);
                    }
                    project.CoverImageUrl = "/uploads/projects/covers/" + uniqueFileName;
                }

                await _context.SaveChangesAsync();

                if (model.CurrentStateImageFile != null && model.CurrentStateImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "projects", "current");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.CurrentStateImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CurrentStateImageFile.CopyToAsync(fileStream);
                    }
                    var defaultFolder = await _context.DmsFolders.FirstOrDefaultAsync();
                    if (defaultFolder != null) {
                        var dmsDoc = new GMK360.Core.Entities.DmsDocument
                        {
                            Title = "Mevcut Durum Görseli (İlk Hali)",
                            DocumentUrl = "/uploads/projects/current/" + uniqueFileName,
                            FileExtension = Path.GetExtension(model.CurrentStateImageFile.FileName),
                            FileSizeBytes = model.CurrentStateImageFile.Length,
                            EntityType = "ConstructionProject",
                            EntityId = project.Id, 
                            UploadDate = DateTime.UtcNow,
                            UploadedByUserId = _userManager.GetUserId(User) ?? "",
                            FolderId = defaultFolder.Id,
                            PhysicalLocationNote = ""
                        };
                        _context.DmsDocuments.Add(dmsDoc);
                        await _context.SaveChangesAsync();
                    }
                }

                return Json(new { success = true, draftId = project.Id, projectId = project.Id });
            } catch (Exception ex) {
                return Json(new { success = false, message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "") });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveStep2([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)
        {
            try {
                if (model.DraftProjectId == 0) return Json(new { success = false, message = "Proje ID bulunamadı." });
                var project = await _context.ConstructionProjects.Include(p => p.Blocks).FirstOrDefaultAsync(p => p.Id == model.DraftProjectId);
                if (project == null) return Json(new { success = false, message = "Proje bulunamadı." });

                if (model.Blocks != null)
                {
                    var existingBlocks = project.Blocks?.ToList() ?? new System.Collections.Generic.List<GMK360.Core.Entities.Building>();
                    var currentBlockNames = model.Blocks.Select(b => b.BlockName).ToList();

                    // Sadece formda olmayan eski blokları sil
                    var blocksToRemove = existingBlocks.Where(b => !currentBlockNames.Contains(b.BlockName)).ToList();
                    if (blocksToRemove.Any()) {
                        _context.Buildings.RemoveRange(blocksToRemove);
                    }

                    int blockCounter = 1;
                    foreach (var b in model.Blocks)
                    {
                        var existingBlock = existingBlocks.FirstOrDefault(eb => eb.BlockName == b.BlockName);
                        if (existingBlock != null)
                        {
                            // Varsa SADECE GÜNCELLE (Lifecycle kuralı)
                            existingBlock.BaseArea = b.BaseArea;
                            existingBlock.BasementFloors = b.BasementFloors;
                            existingBlock.TotalFloors = b.TotalFloors;
                            existingBlock.TotalUnits = b.TotalApartments + b.TotalShops;
                            existingBlock.TotalApartments = b.TotalApartments;
                            existingBlock.TotalShops = b.TotalShops;
                            existingBlock.HasRoof = b.HasRoof;
                            existingBlock.HasGroundFloor = b.HasGroundFloor;
                            
                            // Eğer Dükkan Sayısı kolonu eklenirse buraya da eklenecek
                        }
                        else
                        {
                            // Yoksa YENİ EKLE
                            var building = new GMK360.Core.Entities.Building
                            {
                                Name = project.Name + " - " + b.BlockName,
                                BlockName = b.BlockName,
                                BuildingNumber = blockCounter.ToString(),
                                StreetName = "Belirtilmedi",
                                BaseArea = b.BaseArea,
                                BasementFloors = b.BasementFloors,
                                TotalFloors = b.TotalFloors,
                                TotalUnits = b.TotalApartments + b.TotalShops,
                                TotalApartments = b.TotalApartments,
                                TotalShops = b.TotalShops,
                                HasBlock = true,
                                HasRoof = b.HasRoof,
                                HasGroundFloor = b.HasGroundFloor,
                                ConstructionProjectId = project.Id,
                                CityId = project.CityId ?? 34,
                                DistrictId = project.DistrictId ?? 1,
                                NeighborhoodId = project.NeighborhoodId ?? 1,
                                StreetId = project.StreetId,
                                CreatedAt = DateTime.UtcNow,
                                ManagerUserId = _userManager.GetUserId(User) ?? ""
                            };
                            _context.Buildings.Add(building);
                        }
                        blockCounter++;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            } catch (Exception ex) {
                return Json(new { success = false, message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "") });
            }
        }
        public async Task<IActionResult> CreateWizard([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)
        {
            try
            {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null) return Unauthorized();

                if (model.DraftProjectId <= 0)
                {
                    TempData["ErrorMessage"] = "Proje ID bulunamadı. Lütfen işleminizi baştan yapın.";
                    return RedirectToAction(nameof(Index));
                }

                var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == model.DraftProjectId);
                if (project == null)
                {
                    TempData["ErrorMessage"] = "Proje bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                project.Status = 1; // 1 = Devam Ediyor
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Şantiye başarıyla başlatıldı ve bloklar oluşturuldu.";
                return RedirectToAction(nameof(Details), new { id = project.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Address,StartDate,EndDate,CoverImageUrl")] ConstructionProject project)
        {
            if (ModelState.IsValid)
            {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null) return Unauthorized();

                project.AgencyId = agencyId.Value;
                project.Status = 0; // 0 = Upcoming
                project.CreatedAt = DateTime.UtcNow;

                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }
        
        
        
        // POST: ConstructionProject/AddConstructionTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddConstructionTask(int projectId, string title, string description)
        {
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project == null) return NotFound();
            
            var task = new GMK360.Core.Entities.Construction.ConstructionTask
            {
                ConstructionProjectId = projectId,
                Title = title,
                Description = description,
                Status = 0,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.ConstructionTasks.Add(task);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Yeni aşama başarıyla eklendi.";
            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        // POST: ConstructionProject/SendTaskInvite
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendTaskInvite(int projectId, int taskId, string companyName, string phoneNumber, string note)
        {
            var invite = new GMK360.Core.Entities.Construction.ConstructionTaskInvite
            {
                ConstructionTaskId = taskId,
                CompanyName = companyName,
                PhoneNumber = phoneNumber,
                Note = note,
                Status = 0, // Bekliyor
                SentByUserId = _userManager.GetUserId(User) ?? "",
                InviteDate = DateTime.UtcNow
            };
            
            _context.ConstructionTaskInvites.Add(invite);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"{companyName} ({phoneNumber}) firmasına teklif daveti SMS olarak gönderildi!";
            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        // GET: ConstructionProject/ManageBlock/5
                [HttpPost]
        public async Task<IActionResult> UpdateBlockSkeleton(int buildingId, int TotalFloors, int BasementFloors, bool HasGroundFloor = false, bool HasRoof = false)
        {
            var building = await _context.Buildings.FindAsync(buildingId);
            if (building != null)
            {
                building.TotalFloors = TotalFloors;
                building.BasementFloors = BasementFloors;
                building.HasGroundFloor = HasGroundFloor;
                building.HasRoof = HasRoof;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Bina iskeleti (kat yapÄ±larÄ±) baÅŸarÄ±yla gÃ¼ncellendi.";
            }
            return RedirectToAction("ManageBlock", new { id = buildingId });
        }

        public async Task<IActionResult> ManageBlock(int id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var block = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .Include(b => b.Units).ThenInclude(u => u.ParentUnit).Include(b => b.ParentBuilding)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (block == null) return NotFound();
            
            if (!User.IsInRole("Admin") && block.ConstructionProject.AgencyId != agencyId)
            {
                return Unauthorized();
            }

            // Mimari görselleri ve kat planlarını getir
            string floorEntityType = "BuildingFloor_" + id;
            var architectureDocs = await _context.DmsDocuments
                .Where(d => (d.EntityType == "Building" && d.EntityId == id) ||
                            (d.EntityType == floorEntityType))
                .ToListAsync();

            ViewBag.BlockImages = architectureDocs.Where(d => d.EntityType == "Building").ToList();
            ViewBag.FloorPlans = architectureDocs.Where(d => d.EntityType == floorEntityType).ToList();

            return View(block);
        }

                // --- SOSYAL DONATILAR VE DIÅ ALANLAR (AMENITIES) ---
                [HttpPost]
        public async Task<IActionResult> UpdateProjectLandArea(int projectId, double? totalLandArea, double? landscapeArea)
        {
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project != null)
            {
                project.TotalLandArea = totalLandArea;
                project.LandscapeArea = landscapeArea;
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Amenities), new { projectId = projectId });
        }
        public async Task<IActionResult> Amenities(int projectId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var project = await _context.ConstructionProjects
                .Include(p => p.Amenities)
                .Include(p => p.Blocks)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null || project.AgencyId != agencyId)
                return NotFound();

            return View(project);
        }

        [HttpPost]
                [HttpPost]
        public async Task<IActionResult> EditAmenity(int projectId, int amenityId, string name, double? squareMeters, string description)
        {
            var amenity = await _context.ProjectAmenities.FindAsync(amenityId);
            if (amenity != null)
            {
                amenity.Name = name;
                amenity.SquareMeters = squareMeters;
                amenity.Description = description;
                _context.Update(amenity);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Amenities), new { projectId = projectId });
        }
        [HttpPost]
        public async Task<IActionResult> AddAmenity(int projectId, string name, string type, double? squareMeters, string description)
        {
            var amenity = new GMK360.Core.Entities.Construction.ProjectAmenity
            {
                ConstructionProjectId = projectId,
                Name = name,
                Type = type,
                SquareMeters = squareMeters,
                Description = description
            };

            _context.ProjectAmenities.Add(amenity);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yeni Sosyal DonatÄ± / AÃ§Ä±k Alan eklendi.";
            return RedirectToAction(nameof(Amenities), new { projectId = projectId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAmenity(int amenityId, int projectId)
        {
            var amenity = await _context.ProjectAmenities.FindAsync(amenityId);
            if (amenity != null)
            {
                _context.ProjectAmenities.Remove(amenity);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "AÃ§Ä±k alan baÅŸarÄ±yla silindi.";
            }
            return RedirectToAction(nameof(Amenities), new { projectId = projectId });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAmenityStatus(int amenityId, int projectId)
        {
            var amenity = await _context.ProjectAmenities.FindAsync(amenityId);
            if (amenity != null)
            {
                amenity.IsCompleted = !amenity.IsCompleted;
                _context.ProjectAmenities.Update(amenity);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Amenities), new { projectId = projectId });
        }

        // --- DAIRE TÄ°PLERÄ° (ÅABLONLAR) ---
        
        public async Task<IActionResult> Templates(int projectId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var project = await _context.ConstructionProjects
                .Include(p => p.UnitTemplates)
                .ThenInclude(t => t.Spaces)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return NotFound();
            if (!User.IsInRole("Admin") && project.AgencyId != agencyId) return Unauthorized();

            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> AddTemplate(int projectId, string name, string roomLayout, string description)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var template = new GMK360.Core.Entities.UnitTemplate
            {
                ConstructionProjectId = projectId,
                Name = name,
                RoomLayout = roomLayout,
                Description = description
            };

            _context.UnitTemplates.Add(template);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yeni Daire Tipi Åablonu baÅŸarÄ±yla oluÅŸturuldu.";
            return RedirectToAction(nameof(Templates), new { projectId = projectId });
        }

        public async Task<IActionResult> TemplateSpaces(int templateId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var template = await _context.UnitTemplates
                .Include(t => t.ConstructionProject)
                .Include(t => t.Spaces)
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null) return NotFound();
            if (!User.IsInRole("Admin") && template.ConstructionProject.AgencyId != agencyId) return Unauthorized();

            return View(template);
        }

        [HttpPost]
        public async Task<IActionResult> AddTemplateSpace(int templateId, string name, string type, double? squareMeters)
        {
            var space = new GMK360.Core.Entities.UnitTemplateSpace
            {
                UnitTemplateId = templateId,
                Name = name,
                Type = type,
                SquareMeters = squareMeters
            };

            _context.UnitTemplateSpaces.Add(space);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Åablona yeni alan eklendi.";
            return RedirectToAction(nameof(TemplateSpaces), new { templateId = templateId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTemplateSpace(int spaceId, int templateId)
        {
            var space = await _context.UnitTemplateSpaces.FindAsync(spaceId);
            if (space != null)
            {
                _context.UnitTemplateSpaces.Remove(space);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Alan ÅŸablondan silindi.";
            }
            return RedirectToAction(nameof(TemplateSpaces), new { templateId = templateId });
        }

                [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBlockDetails(int Id, string BlockName, double? BaseArea, List<string> SelectedFeatures, string TechnicalFeatures, string Description, string InsulationType, int? ElevatorCount, string ParkingType)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var building = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .FirstOrDefaultAsync(b => b.Id == Id);

            if (building == null || building.ConstructionProject?.AgencyId != agencyId)
                return NotFound();

            building.BlockName = BlockName;
            building.BaseArea = BaseArea;
            
            // Combine predefined checkboxes into a comma-separated string
            string combinedFeatures = "";
            if (SelectedFeatures != null && SelectedFeatures.Count > 0)
            {
                combinedFeatures = string.Join(", ", SelectedFeatures);
            }
            
            // If they also typed custom technical features, append them
            if (!string.IsNullOrEmpty(TechnicalFeatures))
            {
                if (!string.IsNullOrEmpty(combinedFeatures)) combinedFeatures += ", ";
                combinedFeatures += TechnicalFeatures;
            }
            
            building.TechnicalFeatures = combinedFeatures;
            building.Description = Description;
            building.InsulationType = InsulationType;
            building.ElevatorCount = ElevatorCount;
            building.ParkingType = ParkingType;

            _context.Update(building);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Blok özellikleri başarıyla güncellendi.";
            return RedirectToAction(nameof(ManageBlock), new { id = Id });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBuildingUnit(int buildingId, int FloorLevel, string FloorName, string DoorNumber, string RoomCount, string UnitStructure, double? GrossSquareMeters, string FacadeDirection, string Category, int? ParentUnitId, string OwnerName, int Quantity = 1)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var building = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (building == null || building.ConstructionProject?.AgencyId != agencyId)
                return NotFound();

            string combinedRoomLayout = UnitStructure;

            for(int i = 0; i < Quantity; i++) 
            {
                string finalDoorName = DoorNumber;
                if(Quantity > 1) {
                    finalDoorName = $"{DoorNumber} {i + 1}";
                }
                
                var newUnit = new GMK360.Core.Entities.BuildingUnit
                {
                    BuildingId = buildingId,
                    FloorLevel = FloorLevel,
                    FloorName = string.IsNullOrEmpty(FloorName) ? $"{FloorLevel}. Kat" : FloorName,
                    DoorNumber = finalDoorName,
                    RoomLayout = combinedRoomLayout,
                    GrossSquareMeters = GrossSquareMeters,
                    FacadeDirection = string.IsNullOrEmpty(FacadeDirection) ? "-" : FacadeDirection,
                    Category = string.IsNullOrEmpty(Category) ? "Daire" : Category,
                    ParentUnitId = Category == "Eklenti" ? ParentUnitId : null,
                    IsCustomizable = Category != null && !Category.StartsWith("OrtakAlan"),
                    OwnerName = OwnerName
                };
                _context.BuildingUnits.Add(newUnit);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageBlock), new { id = buildingId });
        }

        [HttpGet]
        public async Task<IActionResult> GetFloorUnits(int buildingId, int floorLevel)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var units = await _context.BuildingUnits
                .Where(u => u.BuildingId == buildingId && u.FloorLevel == floorLevel)
                .Select(u => new {
                    id = u.Id,
                    doorNumber = u.DoorNumber,
                    category = u.Category,
                    roomLayout = u.RoomLayout
                })
                .ToListAsync();

            return Json(units);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CopyFloor(int buildingId, int sourceFloorLevel, string targetFloorLevels, List<int> selectedUnitIds)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var building = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (building == null || building.ConstructionProject?.AgencyId != agencyId)
                return NotFound();

            var sourceUnitsQuery = _context.BuildingUnits
                .Where(u => u.BuildingId == buildingId && u.FloorLevel == sourceFloorLevel);
                
            if (selectedUnitIds != null && selectedUnitIds.Any()) {
                sourceUnitsQuery = sourceUnitsQuery.Where(u => selectedUnitIds.Contains(u.Id));
            }
            
            var sourceUnits = await sourceUnitsQuery.ToListAsync();

            if (!sourceUnits.Any())
                return RedirectToAction(nameof(ManageBlock), new { id = buildingId });

            var targetFloorsStr = targetFloorLevels.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var fStr in targetFloorsStr)
            {
                if (int.TryParse(fStr.Trim(), out int targetLevel))
                {
                    if (targetLevel == sourceFloorLevel) continue;

                    // Hedef kattaki mevcut birimleri alalım ki çakışma kontrolü yapabilelim
                    var existingTargetUnits = await _context.BuildingUnits
                        .Where(u => u.BuildingId == buildingId && u.FloorLevel == targetLevel)
                        .ToListAsync();

                    var idMapping = new Dictionary<int, GMK360.Core.Entities.BuildingUnit>();
                    var clonedUnits = new List<GMK360.Core.Entities.BuildingUnit>();

                    // 1. Aşama: Birimleri kopyala (Çakışma varsa atla)
                    foreach (var u in sourceUnits)
                    {
                        // Çakışma Kontrolü: Aynı isimde ve kategoride birim hedef katta varsa ATLA
                        bool alreadyExists = existingTargetUnits.Any(tu => 
                            tu.DoorNumber.ToLower() == u.DoorNumber.ToLower() && 
                            tu.Category == u.Category);
                            
                        if (alreadyExists) continue;

                        var newUnit = new GMK360.Core.Entities.BuildingUnit
                        {
                            BuildingId = buildingId,
                            FloorLevel = targetLevel,
                            FloorName = targetLevel == 0 ? "Zemin Kat" : (targetLevel < 0 ? $"{targetLevel}. Bodrum" : $"{targetLevel}. Kat"),
                            DoorNumber = u.DoorNumber,
                            RoomLayout = u.RoomLayout,
                            GrossSquareMeters = u.GrossSquareMeters,
                            FacadeDirection = u.FacadeDirection,
                            UnitTypeId = u.UnitTypeId,
                            Category = u.Category,
                            IsCustomizable = u.IsCustomizable,
                            OwnerName = null // Sahiplik boş kalır
                        };
                        
                        clonedUnits.Add(newUnit);
                        idMapping.Add(u.Id, newUnit);
                    }

                    // Önce veritabanına kaydedelim ki yeni ID'ler oluşsun
                    _context.BuildingUnits.AddRange(clonedUnits);
                    await _context.SaveChangesAsync();

                    // 2. Aşama: Eklentilerin ParentUnitId'lerini (Bağlı olduğu daireyi) AKILLICA eşleştir!
                    foreach (var u in sourceUnits)
                    {
                        if (u.Category == "Eklenti" && u.ParentUnitId.HasValue)
                        {
                            // Eğer bu eklentinin bağlı olduğu ana daire aynı kattaysa (kopyalananlar arasındaysa)
                            if (idMapping.ContainsKey(u.ParentUnitId.Value))
                            {
                                // Yeni eklentiyi bul
                                var newEklenti = idMapping[u.Id];
                                // Yeni ana daireyi bul
                                var newParentDaire = idMapping[u.ParentUnitId.Value];
                                
                                // Yeni eklentiyi yeni daireye bağla!
                                newEklenti.ParentUnitId = newParentDaire.Id;
                            }
                        }
                    }
                    
                    // Bağlantıları güncelle
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(ManageBlock), new { id = buildingId });
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBuildingUnit(int unitId, string DoorNumber, string OwnerName, string UnitStructure)
        {
            var unit = await _context.BuildingUnits.FindAsync(unitId);
            if (unit == null) return NotFound();

            unit.DoorNumber = string.IsNullOrWhiteSpace(DoorNumber) ? unit.DoorNumber : DoorNumber;
            unit.OwnerName = string.IsNullOrWhiteSpace(OwnerName) ? null : OwnerName;
            if (!string.IsNullOrWhiteSpace(UnitStructure))
            {
                unit.RoomLayout = UnitStructure;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageBlock), new { id = unit.BuildingId });
        }
[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUnit(int unitId, int buildingId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var unit = await _context.BuildingUnits
                .Include(u => u.Building)
                .ThenInclude(b => b.ConstructionProject)
                .FirstOrDefaultAsync(u => u.Id == unitId);

            if (unit != null && unit.Building.ConstructionProject.AgencyId == agencyId)
            {
                // Silmeden önce bağlı eklentileri (ParentUnitId'si bu olanlar) boşa çıkaralım veya silelim
                // Güvenli olması için ParentUnitId'lerini null yapalım
                var children = await _context.BuildingUnits.Where(u => u.ParentUnitId == unitId).ToListAsync();
                foreach (var child in children)
                {
                    child.ParentUnitId = null;
                }
                
                _context.BuildingUnits.Remove(unit);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ManageBlock), new { id = buildingId });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.ConstructionProjects.FindAsync(id);
            if (project == null) return NotFound();
            
            var agencyId = await GetUserAgencyIdAsync();
            if (!User.IsInRole("Admin") && project.AgencyId != agencyId)
            {
                return Unauthorized();
            }

            return View(project);
        }

        // POST: ConstructionProject/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Address,StartDate,EndDate,Status,CoverImageUrl")] ConstructionProject project)
        {
            if (id != project.Id) return NotFound();

            var agencyId = await GetUserAgencyIdAsync();
            if (!User.IsInRole("Admin") && project.AgencyId != agencyId)
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProject = await _context.ConstructionProjects.FindAsync(id);
                    if (existingProject == null) return NotFound();

                    existingProject.Name = project.Name;
                    existingProject.Description = project.Description;
                    existingProject.Address = project.Address;
                    existingProject.StartDate = project.StartDate;
                    existingProject.EndDate = project.EndDate;
                    existingProject.Status = project.Status;
                    
                    if (!string.IsNullOrEmpty(project.CoverImageUrl))
                    {
                        existingProject.CoverImageUrl = project.CoverImageUrl;
                    }
                    
                    existingProject.UpdatedAt = DateTime.UtcNow;

                    _context.Update(existingProject);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConstructionProjectExists(project.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var project = await _context.ConstructionProjects
                .Include(p => p.Blocks).ThenInclude(b => b.Units)
                .Include(p => p.Phases)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (project == null) return NotFound();

            if (!User.IsInRole("Admin") && project.AgencyId != agencyId)
            {
                return Unauthorized();
            }

            // DmsDocuments silme
            var relatedDocs = await _context.DmsDocuments.Where(d => d.EntityType == "ConstructionProject" && d.EntityId == id).ToListAsync();
            _context.DmsDocuments.RemoveRange(relatedDocs);

            // Binaları ve üniteleri sil
            if (project.Blocks != null)
            {
                foreach(var block in project.Blocks)
                {
                    if (block.Units != null) _context.RemoveRange(block.Units);
                }
                _context.Buildings.RemoveRange(project.Blocks);
            }

            _context.ConstructionProjects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ConstructionProjectExists(int id)
        {
            return _context.ConstructionProjects.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBlock(int projectId, string blockName, int totalFloors, int basementFloors)
        {
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project == null) return NotFound();

            var agencyId = await GetUserAgencyIdAsync();
            if (!User.IsInRole("Admin") && project.AgencyId != agencyId) return Unauthorized();

            var building = new Building
            {
                ConstructionProjectId = projectId,
                Name = blockName,
                BlockName = blockName,
                TotalFloors = totalFloors,
                ManagerUserId = _userManager.GetUserId(User), // Yetkili ataması (şimdilik ekleyen kişi)
                CityId = 1, // Geçici varsayılan
                DistrictId = 1,
                NeighborhoodId = 1,
                StreetName = "-",
                BuildingNumber = "-",
                CreatedAt = DateTime.UtcNow
            };

            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{blockName} başarıyla eklendi.";
            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUnit(int projectId, int buildingId, string unitNumber, string unitType)
        {
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            var building = await _context.Buildings.FindAsync(buildingId);
            
            if (project == null || building == null || building.ConstructionProjectId != projectId) return NotFound();

            var agencyId = await GetUserAgencyIdAsync();
            if (!User.IsInRole("Admin") && project.AgencyId != agencyId) return Unauthorized();

            var unit = new BuildingUnit
            {
                BuildingId = buildingId,
                UnitNumber = unitNumber,
                RoomLayout = unitType,
                OwnerName = "",
                OwnerPhone = "",
                OwnerEmail = "",
                TenantName = "",
                TenantPhone = "",
                TenantEmail = ""
            };

            _context.BuildingUnits.Add(unit);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        // --- PUANTAJ (TIMESHEET) BÖLÜMÜ ---
        [HttpPost]
        public async Task<IActionResult> AddTimesheet(int projectId, string workerId, DateTime workDate, string shiftType, decimal? hours)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project == null) return NotFound();

            var token = Guid.NewGuid().ToString("N");

            var timesheet = new ConstructionTimesheet
            {
                ConstructionProjectId = projectId,
                WorkerId = workerId,
                RecordedById = user.Id,
                WorkDate = workDate,
                ShiftType = shiftType,
                Hours = hours,
                ApprovalStatus = 0, // Bekliyor
                PwaAccessToken = token,
                CreatedAt = DateTime.UtcNow
            };

            _context.ConstructionTimesheets.Add(timesheet);
            await _context.SaveChangesAsync();

            // SMS Gönderim Simülasyonu
            var worker = await _userManager.FindByIdAsync(workerId);
            var phone = worker?.PhoneNumber ?? "Belirtilmemiş";
            var approvalLink = Url.Action("TimesheetApprove", "Pwa", new { token = token }, Request.Scheme);
            
            TempData["SuccessMessage"] = $"Puantaj eklendi. {phone} numarasına onay SMS'i gönderiliyor: {approvalLink}";

            return RedirectToAction("Details", new { id = projectId });
        }
            // --- DAIRE ICI ALAN YONETIMI ---
        public async Task<IActionResult> ManageUnitSpaces(int id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var unit = await _context.BuildingUnits
                .Include(u => u.Building)
                    .ThenInclude(b => b.ConstructionProject)
                .Include(u => u.Spaces)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unit == null) return NotFound();

            if (!User.IsInRole("Admin") && unit.Building.ConstructionProject.AgencyId != agencyId)
            {
                return Unauthorized();
            }

            return View(unit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUnitSpace(int unitId, string name, string spaceType, double? squareMeters)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var unit = await _context.BuildingUnits
                .Include(u => u.Building)
                    .ThenInclude(b => b.ConstructionProject)
                .FirstOrDefaultAsync(u => u.Id == unitId);

            if (unit == null || (!User.IsInRole("Admin") && unit.Building.ConstructionProject.AgencyId != agencyId))
                return Unauthorized();

            var space = new GMK360.Core.Entities.UnitSpace
            {
                BuildingUnitId = unitId,
                Name = name,
                Type = spaceType,
                SquareMeters = squareMeters
            };

            _context.UnitSpaces.Add(space);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageUnitSpaces), new { id = unitId });
        }
    }
}




