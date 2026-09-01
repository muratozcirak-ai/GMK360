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
                    return View(await _context.ConstructionProjects.Include(c => c.Phases).ToListAsync());
                }
                return RedirectToAction("SetupCorporateProfile", "CustomerDashboard");
            }

            var projects = await _context.ConstructionProjects
                .Include(c => c.Phases)
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: ConstructionProject/Create
        // WIZARD CREATE ACTION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWizard([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)
        {
            
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            string uploadedImageUrl = "";

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
                
                uploadedImageUrl = "/uploads/projects/covers/" + uniqueFileName;
            }

            var project = new GMK360.Core.Entities.Construction.ConstructionProject
            {
                Name = model.Name,
                Description = string.IsNullOrWhiteSpace(model.Description) ? "" : model.Description,
                Address = string.IsNullOrWhiteSpace(model.Address) ? "Adres belirtilmedi" : model.Address,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                CoverImageUrl = uploadedImageUrl,
                AgencyId = agencyId.Value,
                Status = 0, // 0 = Upcoming
                CreatedAt = DateTime.UtcNow
            };

            _context.Add(project);
            await _context.SaveChangesAsync();

            // DMS Sistemine Belge Ekleme (Proje Görseli)
            if (!string.IsNullOrEmpty(uploadedImageUrl))
            {
                var dmsDoc = new GMK360.Core.Entities.DmsDocument
                {
                    Title = "Proje Görseli",
                    DocumentUrl = uploadedImageUrl,
                    FileExtension = Path.GetExtension(model.CoverImageFile.FileName),
                    FileSizeBytes = model.CoverImageFile.Length,
                    EntityType = "ConstructionProject",
                    EntityId = project.Id,
                    UploadDate = DateTime.UtcNow,
                    UploadedByUserId = _userManager.GetUserId(User),
                    FolderId = 1 // Varsayılan veya DB'den gelen bir klasör
                };
                
                var defaultFolder = _context.DmsFolders.FirstOrDefault();
                if (defaultFolder != null)
                {
                    dmsDoc.FolderId = defaultFolder.Id;
                    _context.DmsDocuments.Add(dmsDoc);
                    await _context.SaveChangesAsync();
                }
            }

            
            if (model.Blocks != null && model.Blocks.Count > 0)
            {
                foreach (var b in model.Blocks)
                {
                    var building = new GMK360.Core.Entities.Building
                    {
                        Name = project.Name + " - " + b.BlockName,
                        BlockName = b.BlockName,
                        HasBlock = true,
                        TotalFloors = b.TotalFloors,
                        TotalUnits = b.TotalApartments + b.TotalShops,
                        ConstructionProjectId = project.Id,
                        CreatedAt = DateTime.UtcNow,
                        ManagerUserId = _userManager.GetUserId(User)
                    };
                    _context.Buildings.Add(building);
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
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
        
        
        // GET: ConstructionProject/ManageBlock/5
        public async Task<IActionResult> ManageBlock(int id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var block = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .Include(b => b.Units)
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
            ViewBag.FloorPlans = architectureDocs.Where(d => d.EntityType == "BuildingFloor").ToList();

            return View(block);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateUnits(int buildingId, int normalFloors, int basementFloors, bool hasGroundFloor, int unitsPerFloor, string roomLayout)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var block = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (block == null) return NotFound();

            if (!User.IsInRole("Admin") && block.ConstructionProject.AgencyId != agencyId)
            {
                return Unauthorized();
            }

            int doorCounter = 1;
            int totalUnitsCreated = 0;

            // 1. Bodrum Katları (Örn: -2, -1)
            for (int f = basementFloors; f >= 1; f--)
            {
                for (int u = 0; u < unitsPerFloor; u++)
                {
                    var unit = new GMK360.Core.Entities.BuildingUnit
                    {
                        BuildingId = buildingId,
                        DoorNumber = doorCounter.ToString(),
                        FloorLevel = -f,
                        FloorName = $"-{f}. Kat (Bodrum)",
                        RoomLayout = string.IsNullOrEmpty(roomLayout) ? "3+1" : roomLayout,
                        IsEmpty = true
                    };
                    _context.BuildingUnits.Add(unit);
                    doorCounter++;
                    totalUnitsCreated++;
                }
            }

            // 2. Zemin Kat (0)
            if (hasGroundFloor)
            {
                for (int u = 0; u < unitsPerFloor; u++)
                {
                    var unit = new GMK360.Core.Entities.BuildingUnit
                    {
                        BuildingId = buildingId,
                        DoorNumber = doorCounter.ToString(),
                        FloorLevel = 0,
                        FloorName = "Zemin Kat",
                        RoomLayout = string.IsNullOrEmpty(roomLayout) ? "3+1" : roomLayout,
                        IsEmpty = true
                    };
                    _context.BuildingUnits.Add(unit);
                    doorCounter++;
                    totalUnitsCreated++;
                }
            }

            // 3. Normal Katlar (1, 2, 3...)
            for (int f = 1; f <= normalFloors; f++)
            {
                for (int u = 0; u < unitsPerFloor; u++)
                {
                    var unit = new GMK360.Core.Entities.BuildingUnit
                    {
                        BuildingId = buildingId,
                        DoorNumber = doorCounter.ToString(),
                        FloorLevel = f,
                        FloorName = $"{f}. Kat",
                        RoomLayout = string.IsNullOrEmpty(roomLayout) ? "3+1" : roomLayout,
                        IsEmpty = true
                    };
                    _context.BuildingUnits.Add(unit);
                    doorCounter++;
                    totalUnitsCreated++;
                }
            }
            
            block.TotalFloors = normalFloors;
            block.BasementFloors = basementFloors;
            block.HasGroundFloor = hasGroundFloor;
            block.TotalUnits = (block.TotalUnits == 0) ? totalUnitsCreated : block.TotalUnits + totalUnitsCreated; // Varsa üstüne ekle
            
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{totalUnitsCreated} adet bağımsız bölüm başarıyla oluşturuldu.";
            return RedirectToAction("ManageBlock", new { id = buildingId });
        }

        
        // --- MİMARİ GÖRSELLER VE KAT PLANLARI YÖNETİMİ ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadArchitectureMedia(int buildingId, string entityType, int entityId, string title, IFormFile file)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var block = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (block == null || (!User.IsInRole("Admin") && block.ConstructionProject.AgencyId != agencyId))
            {
                return Unauthorized();
            }

            if (file != null && file.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "architecture");
                Directory.CreateDirectory(uploadsFolder);
                
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var doc = new GMK360.Core.Entities.DmsDocument
                {
                    Title = title ?? file.FileName,
                    DocumentUrl = "/uploads/architecture/" + uniqueFileName,
                    FileExtension = Path.GetExtension(file.FileName),
                    FileSizeBytes = file.Length,
                    EntityType = entityType, 
                    EntityId = entityId,
                    UploadedByUserId = _userManager.GetUserId(User),
                    UploadDate = DateTime.UtcNow
                };

                _context.DmsDocuments.Add(doc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{title} başarıyla yüklendi.";
            }

            return RedirectToAction("ManageBlock", new { id = buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteArchitectureMedia(int documentId, int buildingId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            var block = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (block == null || (!User.IsInRole("Admin") && block.ConstructionProject.AgencyId != agencyId))
            {
                return Unauthorized();
            }

            var doc = await _context.DmsDocuments.FindAsync(documentId);
            if (doc != null)
            {
                // Dosyayı sunucudan silme (opsiyonel)
                var filePath = Path.Combine(_hostEnvironment.WebRootPath, doc.DocumentUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.DmsDocuments.Remove(doc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Görsel başarıyla silindi.";
            }

            return RedirectToAction("ManageBlock", new { id = buildingId });
        }

        // GET: ConstructionProject/Edit/5
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

        private bool ConstructionProjectExists(int id)
        {
            return _context.ConstructionProjects.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBlock(int projectId, string blockName)
        {
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project == null) return NotFound();

            var agencyId = await GetUserAgencyIdAsync();
            if (!User.IsInRole("Admin") && project.AgencyId != agencyId) return Unauthorized();

            var building = new Building
            {
                ConstructionProjectId = projectId,
                Name = blockName,
                ManagerUserId = _userManager.GetUserId(User), // Yetkili ataması (şimdilik ekleyen kişi)
                Address = project.Address,
                CreatedAt = DateTime.UtcNow
            };

            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

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
    }
}

