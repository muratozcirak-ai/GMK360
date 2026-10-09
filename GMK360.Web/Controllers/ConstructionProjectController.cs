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

    public class SmartTemplateItem

    {

        public string Type { get; set; }

        public string Category { get; set; }

        public string Name { get; set; }

        public string Unit { get; set; }

    }



    public class SmartTemplateSubmitModel

    {

        public int SpaceId { get; set; }

        public List<string> Types { get; set; }

        public List<string> Categories { get; set; }

        public List<string> Names { get; set; }

        public List<double> Quantities { get; set; }

        public List<string> Units { get; set; }

    }



    [Authorize(Roles = "InsaatFirmasi,Admin,Corporate,CompanyEmployee")]
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
            
            // ADMIN OVERRIDE FOR TESTING/CREATING PROJECTS
            if (agencyConsultant == null && User.IsInRole("Admin"))
            {
                var defaultAgency = await _context.Agencies.OrderBy(a => a.Id).FirstOrDefaultAsync();
                return defaultAgency?.Id ?? 1;
            }
            
            return agencyConsultant?.AgencyId;
        }



        // GET: ConstructionProject

                private async Task<GMK360.Core.Entities.AgencyConsultant?> GetCurrentStaffAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            return await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == user.Id && c.IsActive);
        }

                public async Task<IActionResult> Index()
        {
            var staff = await GetCurrentStaffAsync();
            if (staff != null && staff.Role == GMK360.Core.Entities.AgencyRole.Driver)
            {
                return Forbid(); // ÅofÃ¶r ÅŸantiyeleri gÃ¶remez!
            }

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null)

            {

                // Admin tmn grebilir

                if (User.IsInRole("Admin"))

                {

                    return View(await _context.ConstructionProjects

                .Include(c => c.Amenities).ToListAsync());

                }

                return RedirectToAction("SetupCorporateProfile", "CustomerDashboard");

            }



            var projects = await _context.ConstructionProjects

                

                .Include(c => c.Amenities)

                .Where(p => p.AgencyId == agencyId)

                .ToListAsync();



            return View(projects);

        }



        // GET: ConstructionProject/Details/5

        public async Task<IActionResult> Details(int? id)

        {

            if (id == null) return NotFound();



                        ViewBag.GlobalRules = await _context.ModuleDocumentRules
                .Include(r => r.Prerequisites)
                .ThenInclude(p => p.PrerequisiteTemplate)
                .Where(r => r.TargetModule == "Construction")
                .ToListAsync();

            ViewBag.Phase0Docs = await _context.ProjectLegalDocuments

                .Include(d => d.SystemTemplate)

                .Where(d => d.ConstructionProjectId == id)

                .ToListAsync();
            ViewBag.Stakeholders = await _context.ProjectStakeholders
                .Include(s => s.User)
                .Where(s => s.ProjectId == id && !s.IsDeleted)
                .ToListAsync();




            
            // Fetch images from DocumentArchive dynamically if they exist (in case user uploaded manually)
            var sahaGorseli = await _context.DocumentArchives
                .Where(d => d.SourceModule == "Construction" && d.ProjectId == id && (d.Category.Contains("Saha") || d.Title.Contains("Mevcut")))
                .OrderByDescending(d => d.Id)
                .FirstOrDefaultAsync();
            if (sahaGorseli != null) {
                ViewBag.CurrentStateImageUrl = sahaGorseli.DocumentUrl;
            }
            
            var projeGorseli = await _context.DocumentArchives
                .Where(d => d.SourceModule == "Construction" && d.ProjectId == id && (d.Category.Contains("Proje") || (d.Category.Contains("Bina") && !d.Title.Contains("Mevcut"))))
                .OrderByDescending(d => d.Id)
                .FirstOrDefaultAsync();
            if (projeGorseli != null) {
                ViewBag.CoverImageUrl = projeGorseli.DocumentUrl;
            }
            
            var project = await _context.ConstructionProjects

                

                

                

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
        [HttpPost]
        public async Task<IActionResult> InitProject(string projectName, int statusId, string projectType)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var project = new GMK360.Core.Entities.Construction.ConstructionProject
            {
                Name = projectName,
                AgencyId = agencyId.Value,
                Status = (GMK360.Core.Entities.Construction.ProjectStatus)statusId,
                ProjectType = projectType,
                Address = "", // DB requires not null
                Description = "", // DB requires not null
                
                IsDataLocked = false
            };

            _context.ConstructionProjects.Add(project);
            await _context.SaveChangesAsync();

            // Create physical folders (Cabinet)
            string webRoot = _hostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string baseFolder = Path.Combine(webRoot, "uploads", $"Agency_{agencyId.Value}", $"Project_{project.Id}");
            Directory.CreateDirectory(Path.Combine(baseFolder, "images"));
            Directory.CreateDirectory(Path.Combine(baseFolder, "documents"));

            return RedirectToAction("Create", new { id = project.Id });
        }


        [Authorize(Roles = "InsaatFirmasi,Admin,Corporate")]
        public async Task<IActionResult> Create(int? id = null) { 

            int? projectId = id; 

            var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync(); 

            ViewBag.Cities = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(cities, "Id", "Name"); 

            ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>()); 

            ViewBag.Neighborhoods = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>()); 

            ViewBag.Streets = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>()); 

            

            var model = new GMK360.Web.Models.CreateProjectWizardViewModel();



            if (projectId.HasValue) {

                var draft = await _context.ConstructionProjects.Include(p => p.Blocks).Include(p => p.KatMalikleri).ThenInclude(o => o.Contact).FirstOrDefaultAsync(p => p.Id == projectId.Value);

                if (draft != null) {
                    model.DraftProjectId = draft.Id;
                    model.Name = draft.Name;
                    model.Description = draft.Description;
                    model.Address = draft.Address;
                    model.StartDate = draft.StartDate;
                    model.EndDate = draft.EndDate ?? default(DateTime);
                    model.StatusId = (int)draft.Status;
                    model.ProjectType = draft.ProjectType;
                    model.ProjectOriginId = draft.ProjectOriginId ?? 0;
                    model.IsNewDesignForExisting = draft.IsNewDesignForExisting;
                    model.EskiKatSayisi = draft.EskiKatSayisi;
                    model.Ada = draft.Ada;
                    model.Parsel = draft.Parsel;
                    model.TotalLandArea = draft.TotalLandArea;
                    model.EskiDaireSayisi = draft.EskiDaireSayisi;
                    model.EskiDukkanSayisi = draft.EskiDukkanSayisi;
                    model.CityId = draft.CityId ?? 0;
                    model.DistrictId = draft.DistrictId ?? 0;
                    model.NeighborhoodId = draft.NeighborhoodId ?? 0;
                    model.StreetId = draft.StreetId ?? 0;
                    model.Latitude = draft.Latitude?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "";
                    model.Longitude = draft.Longitude?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "";

                    ViewBag.CoverImageUrl = draft.CoverImageUrl;
                    if (draft.CityId.HasValue && draft.CityId.Value > 0) {
                        var dists = _context.Districts.Where(d => d.CityId == draft.CityId.Value).OrderBy(d => d.Name).ToList();
                        ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(dists, "Id", "Name", draft.DistrictId);
                    }
                    if (draft.DistrictId.HasValue && draft.DistrictId.Value > 0) {
                        var neighs = _context.Neighborhoods.Where(n => n.DistrictId == draft.DistrictId.Value).OrderBy(n => n.Name).ToList();
                        ViewBag.Neighborhoods = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(neighs, "Id", "Name", draft.NeighborhoodId);
                    }
                    if (draft.NeighborhoodId.HasValue && draft.NeighborhoodId.Value > 0) {
                        var sts = _context.Streets.Where(s => s.NeighborhoodId == draft.NeighborhoodId.Value).OrderBy(s => s.Name).ToList();
                        ViewBag.Streets = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(sts, "Id", "Name", draft.StreetId);
                    }
                    var currentStateDoc = _context.DocumentArchives.FirstOrDefault(d => d.SourceModule == "Construction" && d.ProjectId == draft.Id && (d.Category.Contains("Saha") || d.Title.Contains("Mevcut")));
                    if (currentStateDoc != null) {
                        ViewBag.CurrentStateImageUrl = currentStateDoc.DocumentUrl;
                    }
                    var tapuDoc = _context.DocumentArchives.FirstOrDefault(d => d.SourceModule == "Construction" && d.ProjectId == draft.Id && d.Category == "Resmi Evraklar");
                    if (tapuDoc != null) {
                        ViewBag.TapuDocumentUrl = tapuDoc.DocumentUrl;
                    }




                    model.TotalLandArea = draft.TotalLandArea;

                    model.TargetTotalApartments = draft.TargetTotalApartments;

                    model.TargetTotalShops = draft.TargetTotalShops;

                    model.Latitude = draft.Latitude?.ToString(System.Globalization.CultureInfo.InvariantCulture);

                    model.Longitude = draft.Longitude?.ToString(System.Globalization.CultureInfo.InvariantCulture);

                    model.CityId = draft.CityId ?? 0;

                    model.DistrictId = draft.DistrictId ?? 0;

                    model.NeighborhoodId = draft.NeighborhoodId ?? 0;

                    model.StreetId = draft.StreetId;



                    if (draft.KatMalikleri != null && draft.KatMalikleri.Any()) {
                        model.Owners = draft.KatMalikleri.Select(km => new GMK360.Web.Models.WizardOwnerItem {
                            FirstName = km.Contact.FirstName,
                            LastName = km.Contact.LastName,
                            PhoneNumber = km.Contact.PhoneNumber,
                            Email = km.Contact.Email,
                            BlockName = km.BlockName,
                            UnitType = km.UnitType,
                            FlatNumber = km.FlatNumber,
                            IsRepresentative = km.IsCommitteeMember
                        }).ToList();
                    }
                    if (draft.Blocks != null && draft.Blocks.Any()) {
                        model.ExistingBlocks = draft.Blocks.Where(b => b.IsExistingBuilding && b.ParentBuildingId == null).Select(b => new GMK360.Web.Models.WizardBlockItem {
                            BlockName = b.BlockName,
                            BaseArea = b.BaseArea,
                            TotalFloors = b.TotalFloors ?? 0,
                            TotalApartments = b.TotalApartments,
                            TotalShops = b.TotalShops,
                            BuildingAge = b.BuildingAge,
                            BasementFloors = b.BasementFloors,
                            HasGroundFloor = b.HasGroundFloor,
                            HasRoof = b.HasRoof,
                            LayoutPattern = b.LayoutPattern,
                            SubBlocks = draft.Blocks.Where(cb => cb.ParentBuildingId == b.Id).Select(cb => new GMK360.Web.Models.WizardBlockItem {
                                BlockName = cb.BlockName,
                                TotalFloors = cb.TotalFloors ?? 0,
                                TotalApartments = cb.TotalApartments,
                                TotalShops = cb.TotalShops,
                                BasementFloors = cb.BasementFloors,
                                HasGroundFloor = cb.HasGroundFloor,
                                HasRoof = cb.HasRoof
                            }).ToList()
                        }).ToList();
                        model.TargetBlocks = draft.Blocks.Where(b => !b.IsExistingBuilding && b.ParentBuildingId == null).Select(b => new GMK360.Web.Models.WizardBlockItem {
                            BlockName = b.BlockName,
                            BaseArea = b.BaseArea,
                            TotalFloors = b.TotalFloors ?? 0,
                            TotalApartments = b.TotalApartments,
                            TotalShops = b.TotalShops,
                            BuildingAge = b.BuildingAge,
                            BasementFloors = b.BasementFloors,
                            HasGroundFloor = b.HasGroundFloor,
                            HasRoof = b.HasRoof,
                            LayoutPattern = b.LayoutPattern,
                            SubBlocks = draft.Blocks.Where(cb => cb.ParentBuildingId == b.Id).Select(cb => new GMK360.Web.Models.WizardBlockItem {
                                BlockName = cb.BlockName,
                                TotalFloors = cb.TotalFloors ?? 0,
                                TotalApartments = cb.TotalApartments,
                                TotalShops = cb.TotalShops,
                                BasementFloors = cb.BasementFloors,
                                HasGroundFloor = cb.HasGroundFloor,
                                HasRoof = cb.HasRoof
                            }).ToList()
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

                if (agencyId == null) return Json(new { success = false, message = "Yetkisiz eriÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒâ€šÃ‚Â¼ÃƒÆ’Ã¢â‚¬Â¦Ãƒâ€¦Ã‚Â¸im." });



                if (model.DraftProjectId <= 0) return Json(new { success = false, message = "Proje ID bulunamadÃƒÆ’Ã¢â‚¬ÂÃƒâ€šÃ‚Â±. LÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¼tfen ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¶nce projeyi baÃƒÆ’Ã¢â‚¬Â¦Ãƒâ€¦Ã‚Â¸latÃƒÆ’Ã¢â‚¬ÂÃƒâ€šÃ‚Â±n." });
                var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == model.DraftProjectId);
                if (project == null) return Json(new { success = false, message = "Proje bulunamadÃƒÆ’Ã¢â‚¬ÂÃƒâ€šÃ‚Â±." });
                if (project.IsDataLocked) return Json(new { success = false, message = "Bu proje bÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¼tÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§e kontrolÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¼ iÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§in kilitlenmiÃƒÆ’Ã¢â‚¬Â¦Ãƒâ€¦Ã‚Â¸tir, veriler deÃƒÆ’Ã¢â‚¬ÂÃƒâ€¦Ã‚Â¸iÃƒÆ’Ã¢â‚¬Â¦Ãƒâ€¦Ã‚Â¸tirilemez." });

                // Update fields
                project.Description = model.Description;
                project.Address = model.Address;
                project.StartDate = model.StartDate;
                if (model.EndDate > DateTime.MinValue) project.EndDate = model.EndDate;
                // project.Status removed to prevent unwanted status changes.
                project.ProjectType = model.ProjectType;
                project.ProjectOriginId = model.ProjectOriginId > 0 ? model.ProjectOriginId : null;
                project.IsNewDesignForExisting = model.IsNewDesignForExisting;
                project.EskiKatSayisi = model.EskiKatSayisi;
                project.EskiDaireSayisi = model.EskiDaireSayisi;
                project.EskiDukkanSayisi = model.EskiDukkanSayisi;
                project.CityId = model.CityId > 0 ? model.CityId : null;
                project.DistrictId = model.DistrictId > 0 ? model.DistrictId : null;
                project.NeighborhoodId = model.NeighborhoodId > 0 ? model.NeighborhoodId : null;
                project.StreetId = model.StreetId > 0 ? model.StreetId : null;
                project.Ada = model.Ada;
                project.Parsel = model.Parsel;
                project.TotalLandArea = model.TotalLandArea;

                if (!string.IsNullOrEmpty(model.Latitude)) {
                    if (double.TryParse(model.Latitude.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lat)) {
                        project.Latitude = lat;
                    }
                }
                if (!string.IsNullOrEmpty(model.Longitude)) {
                    if (double.TryParse(model.Longitude.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lng)) {
                        project.Longitude = lng;
                    }
                }
                
                await _context.SaveChangesAsync();

                string baseFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", $"Agency_{agencyId.Value}", $"Project_{project.Id}");
                string imagesFolder = Path.Combine(baseFolder, "images");
                string docsFolder = Path.Combine(baseFolder, "documents");
                Directory.CreateDirectory(imagesFolder);
                Directory.CreateDirectory(docsFolder);

                // Helper to sanitize project name for filenames
                string safeProjName = string.Join("_", project.Name.Split(Path.GetInvalidFileNameChars()));
                safeProjName = safeProjName.Replace(" ", "_");

                // --- 1. COVER IMAGE (GELECEGİ HALİ) ---
                if (model.CoverImageFile != null && model.CoverImageFile.Length > 0)
                {
                    // Find and remove old archive and file if exists
                    var oldArchive = await _context.DocumentArchives.FirstOrDefaultAsync(d => d.ProjectId == project.Id && d.Title == "Proje Görseli (Geleceği Hali)");
                    if (oldArchive != null)
                    {
                        var oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, oldArchive.DocumentUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                        _context.DocumentArchives.Remove(oldArchive);
                    }

                    string extension = Path.GetExtension(model.CoverImageFile.FileName);
                    string standardizedName = $"{safeProjName}_GelecekHali_1{extension}";
                    string filePath = Path.Combine(imagesFolder, standardizedName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CoverImageFile.CopyToAsync(fileStream);
                    }
                    var archive = new GMK360.Core.Entities.DocumentArchive
                    {
                        AgencyId = agencyId.Value,
                        ProjectId = project.Id,
                        Title = "Proje Görseli (Geleceği Hali)",
                        Category = "Bina Görselleri",
                        SourceModule = "Construction",
                        DocumentUrl = $"/uploads/Agency_{agencyId.Value}/Project_{project.Id}/images/{standardizedName}",
                        FileName = standardizedName,
                        FileExtension = extension,
                        UploadDate = DateTime.UtcNow,
                        Status = "Tamamlandı"
                    };
                    _context.DocumentArchives.Add(archive);
                    project.CoverImageUrl = archive.DocumentUrl;
                }

                // --- 2. CURRENT STATE IMAGE (MEVCUT DURUM / İLK HALİ) ---
                if (model.CurrentStateImageFile != null && model.CurrentStateImageFile.Length > 0)
                {
                    var oldArchive = await _context.DocumentArchives.FirstOrDefaultAsync(d => d.ProjectId == project.Id && d.Title == "Mevcut Durum Görseli (İlk Hali)");
                    if (oldArchive != null)
                    {
                        var oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, oldArchive.DocumentUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                        _context.DocumentArchives.Remove(oldArchive);
                    }

                    string extension = Path.GetExtension(model.CurrentStateImageFile.FileName);
                    string standardizedName = $"{safeProjName}_EskiHali_1{extension}";
                    string filePath = Path.Combine(imagesFolder, standardizedName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CurrentStateImageFile.CopyToAsync(fileStream);
                    }
                    var archive = new GMK360.Core.Entities.DocumentArchive
                    {
                        AgencyId = agencyId.Value,
                        ProjectId = project.Id,
                        Title = "Mevcut Durum Görseli (İlk Hali)",
                        Category = "Bina Görselleri",
                        SourceModule = "Construction",
                        DocumentUrl = $"/uploads/Agency_{agencyId.Value}/Project_{project.Id}/images/{standardizedName}",
                        FileName = standardizedName,
                        FileExtension = extension,
                        UploadDate = DateTime.UtcNow,
                        Status = "Tamamlandı"
                    };
                    _context.DocumentArchives.Add(archive);
                    project.CurrentStateImageUrl = archive.DocumentUrl;
                }

                // --- 3. TAPU DOCUMENT (TAPU BELGESİ) ---
                if (model.TapuDocumentFile != null && model.TapuDocumentFile.Length > 0)
                {
                    var oldArchive = await _context.DocumentArchives.FirstOrDefaultAsync(d => d.ProjectId == project.Id && d.Title == "Tapu Belgesi");
                    if (oldArchive != null)
                    {
                        var oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, oldArchive.DocumentUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                        _context.DocumentArchives.Remove(oldArchive);
                    }

                    string extension = Path.GetExtension(model.TapuDocumentFile.FileName);
                    string standardizedName = $"{safeProjName}_TapuBelgesi{extension}";
                    string filePath = Path.Combine(docsFolder, standardizedName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.TapuDocumentFile.CopyToAsync(fileStream);
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
                        UploadDate = DateTime.UtcNow,
                        Status = "Tamamlandı"
                    };
                    _context.DocumentArchives.Add(archive);
                }
                
                await _context.SaveChangesAsync();

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

                var existingBlocks = project.Blocks?.ToList() ?? new System.Collections.Generic.List<GMK360.Core.Entities.Building>();
                var parentBlocks = new System.Collections.Generic.List<GMK360.Web.Models.WizardBlockItem>();
                if (model.ExistingBlocks != null) { foreach(var b in model.ExistingBlocks) { b.IsExistingBuilding = true; parentBlocks.Add(b); } }
                if (model.TargetBlocks != null) { foreach(var b in model.TargetBlocks) { b.IsExistingBuilding = false; parentBlocks.Add(b); } }

                var allCurrentNames = new System.Collections.Generic.List<string>();
                
                int blockCounter = 1;
                foreach (var pb in parentBlocks) {
                    allCurrentNames.Add(pb.BlockName);
                    var existingParent = existingBlocks.FirstOrDefault(eb => eb.BlockName == pb.BlockName && eb.IsExistingBuilding == pb.IsExistingBuilding && eb.ParentBuildingId == null);
                    
                    if (existingParent != null) {
                        existingParent.BaseArea = pb.BaseArea;
                        existingParent.BasementFloors = pb.BasementFloors;
                        existingParent.TotalFloors = pb.TotalFloors;
                        existingParent.TotalUnits = pb.TotalApartments + pb.TotalShops;
                        existingParent.TotalApartments = pb.TotalApartments;
                        existingParent.TotalShops = pb.TotalShops;
                        existingParent.HasGroundFloor = pb.HasGroundFloor;
                        existingParent.HasRoof = pb.HasRoof;
                        existingParent.BuildingAge = pb.BuildingAge;
                        existingParent.LayoutPattern = pb.LayoutPattern;
                    } else {
                        existingParent = new GMK360.Core.Entities.Building {
                            Name = project.Name + " - " + pb.BlockName,
                            BlockName = pb.BlockName,
                            BuildingNumber = blockCounter.ToString(),
                            StreetName = "Belirtilmedi",
                            IsExistingBuilding = pb.IsExistingBuilding,
                            BaseArea = pb.BaseArea,
                            BasementFloors = pb.BasementFloors,
                            TotalFloors = pb.TotalFloors,
                            TotalUnits = pb.TotalApartments + pb.TotalShops,
                            TotalApartments = pb.TotalApartments,
                            TotalShops = pb.TotalShops,
                            HasGroundFloor = pb.HasGroundFloor,
                            HasRoof = pb.HasRoof,
                            BuildingAge = pb.BuildingAge,
                            LayoutPattern = pb.LayoutPattern,
                            ConstructionProjectId = project.Id,
                            CityId = project.CityId ?? 34,
                            DistrictId = project.DistrictId ?? 1,
                            NeighborhoodId = project.NeighborhoodId ?? 1,
                            StreetId = project.StreetId,
                            
                            ManagerUserId = _userManager.GetUserId(User) ?? ""
                        };
                        _context.Buildings.Add(existingParent);
                        existingBlocks.Add(existingParent); // Add to local list for child linking
                    }
                    
                    // İşlem bittikten sonra Parent ID almak için kaydetmemiz gerekmiyor çünkü Entity Framework navigasyon üzerinden id çözer
                    if (pb.SubBlocks != null && pb.SubBlocks.Any()) {
                        foreach (var sb in pb.SubBlocks) {
                            allCurrentNames.Add(sb.BlockName);
                            // Ebeveyn bağlantısını kontrol et.
                            var existingChild = existingBlocks.FirstOrDefault(eb => eb.BlockName == sb.BlockName && eb.IsExistingBuilding == pb.IsExistingBuilding && (eb.ParentBuilding == existingParent || eb.ParentBuildingId == existingParent.Id));
                            
                            if (existingChild != null) {
                                existingChild.TotalFloors = sb.TotalFloors;
                                existingChild.TotalUnits = sb.TotalApartments + sb.TotalShops;
                                existingChild.TotalApartments = sb.TotalApartments;
                                existingChild.TotalShops = sb.TotalShops;
                                existingChild.BasementFloors = sb.BasementFloors;
                                existingChild.HasGroundFloor = sb.HasGroundFloor;
                                existingChild.HasRoof = sb.HasRoof;
                            } else {
                                existingChild = new GMK360.Core.Entities.Building {
                                    Name = project.Name + " - " + sb.BlockName,
                                    BlockName = sb.BlockName,
                                    BuildingNumber = blockCounter.ToString() + "-Sub",
                                    StreetName = "Belirtilmedi",
                                    IsExistingBuilding = pb.IsExistingBuilding,
                                    TotalFloors = sb.TotalFloors,
                                    TotalUnits = sb.TotalApartments + sb.TotalShops,
                                    TotalApartments = sb.TotalApartments,
                                    TotalShops = sb.TotalShops,
                                    BasementFloors = sb.BasementFloors,
                                    HasGroundFloor = sb.HasGroundFloor,
                                    HasRoof = sb.HasRoof,
                                    ConstructionProjectId = project.Id,
                                    ParentBuilding = existingParent, // Navigation mapping
                                    CityId = project.CityId ?? 34,
                                    DistrictId = project.DistrictId ?? 1,
                                    NeighborhoodId = project.NeighborhoodId ?? 1,
                                    StreetId = project.StreetId,
                                    
                                    ManagerUserId = _userManager.GetUserId(User) ?? ""
                                };
                                _context.Buildings.Add(existingChild);
                                existingBlocks.Add(existingChild);
                            }
                        }
                    }
                    blockCounter++;
                }
                
                // Silme işlemi
                var submittedExistingParentNames = parentBlocks.Where(pb => pb.IsExistingBuilding).Select(pb => pb.BlockName).ToList();
        var submittedExistingSubNames = parentBlocks.Where(pb => pb.IsExistingBuilding).SelectMany(pb => pb.SubBlocks?.Select(sb => sb.BlockName) ?? new string[0]).ToList();
        
        var submittedNewParentNames = parentBlocks.Where(pb => !pb.IsExistingBuilding).Select(pb => pb.BlockName).ToList();
        var submittedNewSubNames = parentBlocks.Where(pb => !pb.IsExistingBuilding).SelectMany(pb => pb.SubBlocks?.Select(sb => sb.BlockName) ?? new string[0]).ToList();

        var blocksToRemove = existingBlocks.Where(b => 
            (b.IsExistingBuilding && b.ParentBuildingId == null && !submittedExistingParentNames.Contains(b.BlockName)) || 
            (b.IsExistingBuilding && b.ParentBuildingId != null && !submittedExistingSubNames.Contains(b.BlockName)) ||
            (!b.IsExistingBuilding && b.ParentBuildingId == null && !submittedNewParentNames.Contains(b.BlockName)) ||
            (!b.IsExistingBuilding && b.ParentBuildingId != null && !submittedNewSubNames.Contains(b.BlockName))
        ).ToList();
                if (blocksToRemove.Any()) {
                    _context.Buildings.RemoveRange(blocksToRemove);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });

            } catch (Exception ex) {
                return Json(new { success = false, message = ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "") });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveSingleOwner(int projectId, string firstName, string lastName, string phone, string email, string blockName, string unitType, string flatNumber, bool isRep)
        {
            try
            {
                var crmContact = new GMK360.Core.Entities.CrmContact
                {
                    FirstName = firstName ?? "",
                    LastName = lastName ?? "",
                    PhoneNumber = phone,
                    Email = email,
                    ContactType = isRep ? "Proje Temsilcisi" : "Kat Maliki",
                    ApplicationUserId = _userManager.GetUserId(User)
                };
                _context.CrmContacts.Add(crmContact);
                await _context.SaveChangesAsync();

                // ÃƒÆ’Ã¢â‚¬ÂÃƒâ€šÃ‚Â°nÃƒÆ’Ã¢â‚¬Â¦Ãƒâ€¦Ã‚Â¸aat Rehberine de ekle (AgencyPhonebook)
                var agencyId = await GetUserAgencyIdAsync();
                int actualAgencyId = agencyId ?? 1;
                  if (true)
                {
                    var phonebook = new GMK360.Core.Entities.Construction.AgencyPhonebook
                    {
                        AgencyId = actualAgencyId,
                        Name = $"{crmContact.FirstName} {crmContact.LastName}".Trim(),
                        PhoneNumber = phone ?? "",
                        Email = email,
                        ContactType = 7, // 7 = MÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¼ÃƒÆ’Ã¢â‚¬Â¦Ãƒâ€¦Ã‚Â¸teri & Kat Maliki
                        IsRegistered = false
                    };
                    _context.AgencyPhonebooks.Add(phonebook);
                    await _context.SaveChangesAsync();
                }

                var projectOwner = new GMK360.Core.Entities.Construction.ProjectOwner
                {
                    ConstructionProjectId = projectId,
                    ContactId = crmContact.Id,
                    BlockName = blockName,
                    UnitType = unitType,
                    FlatNumber = flatNumber,
                    IsCommitteeMember = isRep
                };
                _context.ProjectOwners.Add(projectOwner);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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

                    TempData["ErrorMessage"] = "Proje ID bulunamadÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Å“ÃƒÂ¢Ã¢â€šÂ¬Ã¢â€Â¢. LÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒâ€¦Ã¢â‚¬Å“ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â¢Ãƒâ€šÃ‚Âtfen iÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒâ€šÃ‚Â¼ÃƒÆ’Ã¢â‚¬Â¦Ãƒâ€¦Ã‚Â¸leminizi baÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒâ€šÃ‚Â¼ÃƒÆ’Ã¢â‚¬Â¦Ãƒâ€¦Ã‚Â¸tan yapÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Å“ÃƒÂ¢Ã¢â€šÂ¬Ã¢â€Â¢n.";

                    return RedirectToAction(nameof(Index));

                }



                var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == model.DraftProjectId);
                if (project == null)
                {
                    TempData["ErrorMessage"] = "Proje bulunamadÃƒÆ’Ã¢â‚¬ÂÃƒâ€šÃ‚Â±.";
                    return RedirectToAction(nameof(Index));
                }

                // Mevcut kayÃƒÆ’Ã¢â‚¬ÂÃƒâ€šÃ‚Â±tlarÃƒÆ’Ã¢â‚¬ÂÃƒâ€šÃ‚Â± temizle (Geri dÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¶nÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¼p gÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¼ncelleyenler iÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â§in mÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¼kemmel senkronizasyon)
                var existingOwners = await _context.ProjectOwners.Where(po => po.ConstructionProjectId == project.Id).ToListAsync();
                if(existingOwners.Any()) {
                    _context.ProjectOwners.RemoveRange(existingOwners);
                    await _context.SaveChangesAsync();
                }

                if (model.Owners != null && model.Owners.Any())
                {
                    foreach (var owner in model.Owners)
                    {
                        var crmContact = new GMK360.Core.Entities.CrmContact
                        {
                            FirstName = owner.FirstName ?? "",
                            LastName = owner.LastName ?? "",
                            PhoneNumber = owner.PhoneNumber,
                            Email = owner.Email,
                            ContactType = owner.IsRepresentative ? "Proje Temsilcisi" : "Kat Maliki",
                            ApplicationUserId = _userManager.GetUserId(User)
                        };
                        _context.CrmContacts.Add(crmContact);
                        await _context.SaveChangesAsync();

                        // ÃƒÆ’Ã¢â‚¬ÂÃƒâ€šÃ‚Â°nÃƒÆ’Ã¢â‚¬Â¦Ãƒâ€¦Ã‚Â¸aat Rehberine de ekle (AgencyPhonebook)
                        int actualAgencyId = agencyId ?? 1;
                  if (true)
                        {
                            var phonebook = new GMK360.Core.Entities.Construction.AgencyPhonebook
                            {
                                AgencyId = actualAgencyId,
                                Name = $"{crmContact.FirstName} {crmContact.LastName}".Trim(),
                                PhoneNumber = owner.PhoneNumber ?? "",
                                Email = owner.Email,
                                ContactType = 7, // 7 = MÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¼ÃƒÆ’Ã¢â‚¬Â¦Ãƒâ€¦Ã‚Â¸teri & Kat Maliki
                                IsRegistered = false
                            };
                            _context.AgencyPhonebooks.Add(phonebook);
                            await _context.SaveChangesAsync();
                        }

                        var projectOwner = new GMK360.Core.Entities.Construction.ProjectOwner
                        {
                            ConstructionProjectId = project.Id,
                            ContactId = crmContact.Id,
                            BlockName = owner.BlockName,
                            UnitType = owner.UnitType,
                            FlatNumber = owner.FlatNumber,
                            IsCommitteeMember = owner.IsRepresentative
                        };
                        _context.ProjectOwners.Add(projectOwner);



                    }
                }

                // If project status is still Aday, leave it. Otherwise set to 1.
                // Wait, if it was 0, it should remain 0! The form posts StatusId.
                // project.Status removed to prevent unwanted status changes.
                await _context.SaveChangesAsync();




                TempData["SuccessMessage"] = "Şantiye başarıyla başlatıldı ve bloklar oluşturuldu.";

                return RedirectToAction(nameof(Details), new { id = project.Id });

            }

            catch (Exception ex)

            {

                TempData["ErrorMessage"] = "Bir hata oluÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒâ€šÃ‚Â¼ÃƒÆ’Ã¢â‚¬Â¦Ãƒâ€¦Ã‚Â¸tu: " + ex.Message;

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

                project.Status = GMK360.Core.Entities.Construction.ProjectStatus.Projelendirme_Teklif; // 0 = Upcoming

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

                Description = string.IsNullOrWhiteSpace(description) ? "-" : description,

                Status = 0,

                CreatedAt = DateTime.UtcNow

            };

            

            _context.ConstructionTasks.Add(task);

            await _context.SaveChangesAsync();

            

            TempData["SuccessMessage"] = "Şantiye başarıyla başlatıldı ve bloklar oluşturuldu.";

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

            

            TempData["SuccessMessage"] = $"{companyName} ({phoneNumber}) firmasna teklif daveti SMS olarak gnderildi!";

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

                TempData["SuccessMessage"] = "Şantiye başarıyla başlatıldı ve bloklar oluşturuldu.";

            }

            return RedirectToAction("ManageBlock", new { id = buildingId });

        }



        
        [HttpPost]
        public async Task<IActionResult> BulkGenerateEmptyUnits(int projectId, List<string> targetFloors, string category, int count)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Json(new { success = false, message = "Yetkisiz işlem." });

            var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId);
            if (project == null && !User.IsInRole("Admin")) return Json(new { success = false, message = "Proje bulunamadı." });

                        int addedCount = 0;
            try {
                foreach (var target in targetFloors)
                {
                    var parts = target.Split('|');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int buildingId) && int.TryParse(parts[1], out int floorLevel))
                    {
                        var b = await _context.Buildings.FirstOrDefaultAsync(x => x.Id == buildingId && x.ConstructionProjectId == projectId);
                        if (b != null)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var unit = new GMK360.Core.Entities.BuildingUnit
                                {
                                    BuildingId = buildingId,
                                    FloorLevel = floorLevel,
                                    Category = category,
                                    DoorNumber = "-", 
                                    FacadeDirection = "-",
                                    RoomLayout = category == "Dükkan" ? "Dükkan" : (category == "Ofis" ? "Ofis" : "Mesken")
                                };
                                _context.BuildingUnits.Add(unit);
                                addedCount++;
                            }
                        }
                    }
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"{addedCount} adet {category} başarıyla üretildi!" });
            } catch (Exception ex) {
                return Json(new { success = false, message = "Sunucu Hatası: " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message) });
            }
        }

        public async Task<IActionResult> ManageBlock(int id)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            var block = await _context.Buildings

                .Include(b => b.ConstructionProject)

                .Include(b => b.Units).ThenInclude(u => u.ParentUnit).Include(b => b.ParentBuilding).Include(b => b.ChildBuildings).ThenInclude(c => c.Units)

                .FirstOrDefaultAsync(b => b.Id == id);



            if (block == null) return NotFound();

            

            if (!User.IsInRole("Admin") && block.ConstructionProject.AgencyId != agencyId)

            {

                return Unauthorized();

            }



            // Mimari grselleri ve kat planlarn getir

            string floorEntityType = "BuildingFloor_" + id;

            var architectureDocs = await _context.DmsDocuments

                .Where(d => (d.EntityType == "Building" && d.EntityId == id) ||

                            (d.EntityType == floorEntityType))

                .ToListAsync();



            ViewBag.BlockImages = architectureDocs.Where(d => d.EntityType == "Building").ToList();

            ViewBag.FloorPlans = architectureDocs.Where(d => d.EntityType == floorEntityType).ToList();



            return View(block);

        }



                // --- SOSYAL DONATILAR VE DIÃƒÆ’Ã¢â‚¬Â¦Ãƒâ€šÃ‚Â ALANLAR (AMENITIES) ---

                [HttpPost]

        public async Task<IActionResult> UpdateProjectLandArea(int projectId, double? totalLandArea, double? landscapeArea, bool isExisting = false)

        {

            var project = await _context.ConstructionProjects.FindAsync(projectId);

            if (project != null)

            {

                project.TotalLandArea = totalLandArea;

                project.LandscapeArea = landscapeArea;

                _context.Update(project);

                await _context.SaveChangesAsync();

            }

            return RedirectToAction(nameof(Amenities), new { projectId = projectId, isExisting = isExisting });

        }

        public async Task<IActionResult> Amenities(int projectId, bool isExisting = false)

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

        public async Task<IActionResult> EditAmenity(int projectId, int amenityId, string name, double? squareMeters, string description, bool isExisting = false)

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

            return RedirectToAction(nameof(Amenities), new { projectId = projectId, isExisting = isExisting });

        }

        [HttpPost]

        public async Task<IActionResult> AddAmenity(int projectId, string name, string type, double? squareMeters, string description, bool isExisting = false)

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



            TempData["SuccessMessage"] = "Şantiye başarıyla başlatıldı ve bloklar oluşturuldu.";

            return RedirectToAction(nameof(Amenities), new { projectId = projectId, isExisting = isExisting });

        }



        [HttpPost]

        public async Task<IActionResult> DeleteAmenity(int amenityId, int projectId, bool isExisting = false)

        {

            var amenity = await _context.ProjectAmenities.FindAsync(amenityId);

            if (amenity != null)

            {

                _context.ProjectAmenities.Remove(amenity);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Şantiye başarıyla başlatıldı ve bloklar oluşturuldu.";

            }

            return RedirectToAction(nameof(Amenities), new { projectId = projectId, isExisting = isExisting });

        }



        [HttpPost]

        public async Task<IActionResult> ToggleAmenityStatus(int amenityId, int projectId, bool isExisting = false)

        {

            var amenity = await _context.ProjectAmenities.FindAsync(amenityId);

            if (amenity != null)

            {

                amenity.IsCompleted = !amenity.IsCompleted;

                _context.ProjectAmenities.Update(amenity);

                await _context.SaveChangesAsync();

            }

            return RedirectToAction(nameof(Amenities), new { projectId = projectId, isExisting = isExisting });

        }



        // --- DAIRE TÃƒÆ’Ã¢â‚¬ÂÃƒâ€šÃ‚Â°PLERÃƒÆ’Ã¢â‚¬ÂÃƒâ€šÃ‚Â° (ÃƒÆ’Ã¢â‚¬Â¦Ãƒâ€šÃ‚ÂABLONLAR) ---

        

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



            TempData["SuccessMessage"] = "Şantiye başarıyla başlatıldı ve bloklar oluşturuldu.";

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



            TempData["SuccessMessage"] = "Şantiye başarıyla başlatıldı ve bloklar oluşturuldu.";

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

                TempData["SuccessMessage"] = "Şantiye başarıyla başlatıldı ve bloklar oluşturuldu.";

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



            TempData["SuccessMessage"] = "Şantiye başarıyla başlatıldı ve bloklar oluşturuldu.";

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



                    // Hedef kattaki mevcut birimleri alalm ki akma kontrol yapabilelim

                    var existingTargetUnits = await _context.BuildingUnits

                        .Where(u => u.BuildingId == buildingId && u.FloorLevel == targetLevel)

                        .ToListAsync();



                    var idMapping = new Dictionary<int, GMK360.Core.Entities.BuildingUnit>();

                    var clonedUnits = new List<GMK360.Core.Entities.BuildingUnit>();



                    // 1. Aama: Birimleri kopyala (akma varsa atla)

                    foreach (var u in sourceUnits)

                    {

                        // akma Kontrol: Ayn isimde ve kategoride birim hedef katta varsa ATLA

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

                            OwnerName = null // Sahiplik bo kalr

                        };

                        

                        clonedUnits.Add(newUnit);

                        idMapping.Add(u.Id, newUnit);

                    }



                    // nce veritabanna kaydedelim ki yeni ID'ler olusun

                    _context.BuildingUnits.AddRange(clonedUnits);

                    await _context.SaveChangesAsync();



                    // 2. Aama: Eklentilerin ParentUnitId'lerini (Bal olduu daireyi) AKILLICA eletir!

                    foreach (var u in sourceUnits)

                    {

                        if (u.Category == "Eklenti" && u.ParentUnitId.HasValue)

                        {

                            // Eer bu eklentinin bal olduu ana daire ayn kattaysa (kopyalananlar arasndaysa)

                            if (idMapping.ContainsKey(u.ParentUnitId.Value))

                            {

                                // Yeni eklentiyi bul

                                var newEklenti = idMapping[u.Id];

                                // Yeni ana daireyi bul

                                var newParentDaire = idMapping[u.ParentUnitId.Value];

                                

                                // Yeni eklentiyi yeni daireye bala!

                                newEklenti.ParentUnitId = newParentDaire.Id;

                            }

                        }

                    }

                    

                    // Balantlar gncelle

                    await _context.SaveChangesAsync();

                }

            }



            return RedirectToAction(nameof(ManageBlock), new { id = buildingId });

        }



        

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> EditBuildingUnit(int unitId, string DoorNumber, string OwnerName, string UnitStructure, double? GrossSquareMeters, string FacadeDirection)
        {
            var unit = await _context.BuildingUnits.FindAsync(unitId);
            if (unit == null) return NotFound();

            unit.DoorNumber = string.IsNullOrWhiteSpace(DoorNumber) ? unit.DoorNumber : DoorNumber;
            unit.OwnerName = string.IsNullOrWhiteSpace(OwnerName) ? null : OwnerName;
            
            if (!string.IsNullOrWhiteSpace(UnitStructure))
            {
                unit.RoomLayout = UnitStructure;
            }
            if (GrossSquareMeters.HasValue)
            {
                unit.GrossSquareMeters = GrossSquareMeters.Value;
            }
            if (!string.IsNullOrWhiteSpace(FacadeDirection))
            {
                unit.FacadeDirection = FacadeDirection;
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

                // Silmeden nce bal eklentileri (ParentUnitId'si bu olanlar) boa karalm veya silelim

                // Gvenli olmas iin ParentUnitId'lerini null yapalm

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



            // Binalar ve niteleri sil

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

                ManagerUserId = _userManager.GetUserId(User), // Yetkili atamas (imdilik ekleyen kii)

                CityId = 1, // Geici varsaylan

                DistrictId = 1,

                NeighborhoodId = 1,

                StreetName = "-",

                BuildingNumber = "-",

                CreatedAt = DateTime.UtcNow

            };



            _context.Buildings.Add(building);

            await _context.SaveChangesAsync();



            TempData["SuccessMessage"] = $"{blockName} baaryla eklendi.";

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



        // --- PUANTAJ (TIMESHEET) BLM ---

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



            // SMS Gnderim Simlasyonu

            var worker = await _userManager.FindByIdAsync(workerId);

            var phone = worker?.PhoneNumber ?? "Belirtilmemi";

            var approvalLink = Url.Action("TimesheetApprove", "Pwa", new { token = token }, Request.Scheme);

            

            TempData["SuccessMessage"] = $"Puantaj eklendi. {phone} numarasna onay SMS'i gnderiliyor: {approvalLink}";



            return RedirectToAction("Details", new { id = projectId });

        }

            // --- DAIRE ICI ALAN YONETIMI ---

        

        // --- PROJE FAZLARI VE AAMALAR ---

        



        [HttpPost]

        

        [HttpPost]

        

        [HttpPost]

        public async Task<IActionResult> AddCustomLegalDocument(int projectId, string customName, string appliedTo, string institutionPhone, string trackingPerson, string stage)

        {

            var project = await _context.ConstructionProjects.FindAsync(projectId);

            if(project == null) return NotFound();



            var doc = new GMK360.Core.Entities.Construction.ProjectLegalDocument

            {

                ConstructionProjectId = projectId,

                DocumentName = customName,
                Stage = stage,

                

                InstitutionContact = institutionPhone,

                AssignedUserId = trackingPerson,

                IsCustom = true,

                Status = "OnaylandÃƒâ€Ã‚Â±"

            };



            _context.ProjectLegalDocuments.Add(doc);

            await _context.SaveChangesAsync();



            // Istihbarat / Bildirim loglamasi eklenebilir

            

            return RedirectToAction(nameof(Details), new { id = projectId });

        }



        [HttpPost]

        public async Task<IActionResult> UpdateLegalDocumentStatus(int documentId, string statusStr, string appliedTo, string trackingPerson, string institutionContact, string notes, DateTime? expiryDate)

        {

            var doc = await _context.ProjectLegalDocuments.FindAsync(documentId);

            if(doc == null) return NotFound();

            

            doc.Status = statusStr ?? "Bekliyor";

            

            if(trackingPerson != null) doc.AssignedUserId = trackingPerson;

            if(institutionContact != null) doc.InstitutionContact = institutionContact;

            if(notes != null) doc.IssueNotes = notes;

            

            

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = doc.ConstructionProjectId });

        }



        



        



        



        



        



        

        



        



        



        

        

        



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



        [HttpGet]



        [HttpGet]

                public IActionResult GetSmartTemplate(string spaceType)

        {

            var items = new List<SmartTemplateItem>();

            if (string.IsNullOrEmpty(spaceType)) return Json(items);

            

            spaceType = spaceType.Trim().ToLower();



            if (spaceType.Contains("oda") || spaceType == "salon" || spaceType == "mutfak" || spaceType.Contains("dolam") || spaceType.Contains("hol") || spaceType.Contains("koridor") || spaceType.Contains("giri") || spaceType.Contains("antre"))

            {

                items.Add(new SmartTemplateItem { Type = "Measurement", Category = "Zemin", Name = "Zemin Alan", Unit = "m2" });

                items.Add(new SmartTemplateItem { Type = "Measurement", Category = "Tavan", Name = "Tavan Alan", Unit = "m2" });

                items.Add(new SmartTemplateItem { Type = "Measurement", Category = "Duvar", Name = "Net Duvar Alan", Unit = "m2" });

                items.Add(new SmartTemplateItem { Type = "Measurement", Category = "Sprgelik", Name = "Sprgelik", Unit = "mt" });

                items.Add(new SmartTemplateItem { Type = "Measurement", Category = "Duvar", Name = " Kap Boluu", Unit = "m2" });

                items.Add(new SmartTemplateItem { Type = "Measurement", Category = "Duvar", Name = "Pencere Boluu", Unit = "m2" });

                items.Add(new SmartTemplateItem { Type = "Measurement", Category = "Dier", Name = "Pencere Mermeri (Denizlik)", Unit = "mt" });

                items.Add(new SmartTemplateItem { Type = "Measurement", Category = "Tavan", Name = "Perdelik", Unit = "mt" });

                items.Add(new SmartTemplateItem { Type = "Measurement", Category = "Tavan", Name = "Kartonpiyer / Stropiyer", Unit = "mt" });

            }

            else if (spaceType == "balkon" || spaceType.Contains("teras"))

            {

                items.Add(new SmartTemplateItem { Type = "Measurement", Category = "Zemin", Name = "Zemin Alan", Unit = "m2" });

                items.Add(new SmartTemplateItem { Type = "Measurement", Category = "Tavan", Name = "Tavan Alan", Unit = "m2" });

                items.Add(new SmartTemplateItem { Type = "Measurement", Category = "Sprgelik", Name = "Balkon Sprgelii", Unit = "mt" });

                items.Add(new SmartTemplateItem { Type = "Measurement", Category = "Duvar", Name = "Korkuluk / Kpete", Unit = "mt" });

                items.Add(new SmartTemplateItem { Type = "Measurement", Category = "Dier", Name = "Parapet / Damlalk Mermeri", Unit = "mt" });

            }

            else if (spaceType.Contains("slak") || spaceType.Contains("banyo") || spaceType.Contains("wc"))

            {

                items.Add(new SmartTemplateItem { Type = "Measurement", Category = "Zemin", Name = "Zemin Seramik / Fayans", Unit = "m2" });

                items.Add(new SmartTemplateItem { Type = "Measurement", Category = "Duvar", Name = "Duvar Seramik / Fayans", Unit = "m2" });

                items.Add(new SmartTemplateItem { Type = "Measurement", Category = "Tavan", Name = "Asma Tavan Alan", Unit = "m2" });

                items.Add(new SmartTemplateItem { Type = "Measurement", Category = "Duvar", Name = " Kap Boluu", Unit = "m2" });

                

                if (spaceType.Contains("banyo") || spaceType.Contains("slak"))

                {

                    items.Add(new SmartTemplateItem { Type = "Measurement", Category = "Zemin", Name = "Du / Kvet Alan (zolasyon)", Unit = "m2" });

                }

            }



            return Json(items);

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> SaveSmartTemplate(SmartTemplateSubmitModel model)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            var space = await _context.UnitSpaces

                .Include(s => s.BuildingUnit)

                .FirstOrDefaultAsync(s => s.Id == model.SpaceId);



            if (space == null) return Unauthorized();



            for (int i = 0; i < model.Quantities.Count; i++)

            {

                if (model.Quantities[i] > 0)

                {

                    if (model.Types[i] == "Measurement")

                    {

                        _context.SpaceMeasurements.Add(new GMK360.Core.Entities.SpaceMeasurement

                        {

                            UnitSpaceId = model.SpaceId,

                            Category = model.Categories[i],

                            Description = model.Names[i],

                            Quantity = model.Quantities[i],

                            Unit = model.Units[i]

                        });

                    }

                    else if (model.Types[i] == "Fixture")

                    {

                        _context.SpaceFixtures.Add(new GMK360.Core.Entities.SpaceFixture

                        {

                            UnitSpaceId = model.SpaceId,

                            Category = model.Categories[i],

                            ItemName = model.Names[i],

                            Quantity = model.Quantities[i],

                            Unit = model.Units[i],

                            IsCustomizable = false // Binaya ait sabitler genelde deitirilemez

                        });

                    }

                }

            }



            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Şantiye başarıyla başlatıldı ve bloklar oluşturuldu.";

            

            return RedirectToAction(nameof(ManageUnitSpaces), new { id = space.BuildingUnitId });

        }



        public async Task<IActionResult> GetSpaceMeasurements(int spaceId)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            var measurements = await _context.SpaceMeasurements

                .Where(m => m.UnitSpaceId == spaceId)

                .Select(m => new {

                    id = m.Id,

                    category = m.Category,

                    description = m.Description,

                    quantity = m.Quantity,

                    unit = m.Unit,

                    width = m.Width,

                    length = m.Length,

                    height = m.Height

                })

                .ToListAsync();



            return Json(measurements);

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

                public async Task<IActionResult> AddSpaceMeasurement(int spaceId, string category, string description, double quantity, string unit)

        {

            try

            {

                var agencyId = await GetUserAgencyIdAsync();

                if (agencyId == null) return Unauthorized();



                var space = await _context.UnitSpaces

                    .Include(s => s.BuildingUnit)

                    .FirstOrDefaultAsync(s => s.Id == spaceId);



                if (space == null) return Unauthorized();



                var measurement = new GMK360.Core.Entities.SpaceMeasurement

                {

                    UnitSpaceId = spaceId,

                    Category = category,

                    Description = string.IsNullOrWhiteSpace(description) ? "-" : description,

                    Quantity = quantity,

                    Unit = unit

                };



                _context.SpaceMeasurements.Add(measurement);

                await _context.SaveChangesAsync();



                TempData["SuccessMessage"] = "Şantiye başarıyla başlatıldı ve bloklar oluşturuldu.";

                return RedirectToAction(nameof(ManageUnitSpaces), new { id = space.BuildingUnitId });

            }

            catch (Exception ex)

            {

                // Hata yakalama

                TempData["ErrorMessage"] = "l kaydedilirken bir hata olutu: " + ex.Message;

                var spaceFallback = await _context.UnitSpaces.FindAsync(spaceId);

                if (spaceFallback != null)

                    return RedirectToAction(nameof(ManageUnitSpaces), new { id = spaceFallback.BuildingUnitId });

                return RedirectToAction(nameof(Index));

            }

        }



        [HttpGet]

        

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> UpdateSpaceMeasurement(int id, double quantity, double? width, double? length, double? height)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            var measurement = await _context.SpaceMeasurements.FindAsync(id);

            if (measurement == null) return NotFound();



            measurement.Width = width;

            measurement.Length = length;

            measurement.Height = height;



            if (width.HasValue && width > 0 && length.HasValue && length > 0)

            {

                measurement.Quantity = width.Value * length.Value;

                if (height.HasValue && height > 0) measurement.Quantity *= height.Value;

            }

            else

            {

                measurement.Quantity = quantity;

            }



            await _context.SaveChangesAsync();



            // Zek 1: Zemin deiirse Tavan da eitle

            if ((measurement.Category != null && measurement.Category.ToLower().Contains("zemin")) || 

                (measurement.Description != null && measurement.Description.ToLower().Contains("zemin")))

            {

                var tavan = _context.SpaceMeasurements.FirstOrDefault(x => x.UnitSpaceId == measurement.UnitSpaceId && 

                    ((x.Category != null && x.Category.ToLower().Contains("tavan")) || (x.Description != null && x.Description.ToLower().Contains("tavan"))));

                if (tavan != null)

                {

                    tavan.Quantity = measurement.Quantity;

                }

            }



            // Zek 2: Pencere deiirse Denizlik/Mermer'i ve Perdelik'i TM pencereleri toplayarak hesapla

            if (measurement.Description != null && measurement.Description.ToLower().Contains("pencere"))

            {

                // O mahalde adnda 'pencere' geen ve 'mermer' GEMEYEN tm boluklar bul

                var pencereler = _context.SpaceMeasurements

                    .Where(x => x.UnitSpaceId == measurement.UnitSpaceId && 

                                x.Description.ToLower().Contains("pencere") && 

                                !x.Description.ToLower().Contains("mermer"))

                    .ToList();



                double totalWidth = pencereler.Sum(x => x.Width ?? 0);

                int windowCount = pencereler.Count(x => (x.Width ?? 0) > 0);



                var mermer = _context.SpaceMeasurements.FirstOrDefault(x => x.UnitSpaceId == measurement.UnitSpaceId && x.Description.ToLower().Contains("mermer"));

                if (mermer != null)

                {

                    mermer.Quantity = totalWidth;

                }

                

                var perdelik = _context.SpaceMeasurements.FirstOrDefault(x => x.UnitSpaceId == measurement.UnitSpaceId && x.Description.ToLower().Contains("perdelik"));

                if (perdelik != null)

                {

                    // Her pencere iin 40 cm (0.40) pay ekliyoruz

                    perdelik.Quantity = totalWidth + (windowCount * 0.40);

                }

            }



            await _context.SaveChangesAsync();

            return Ok(new { quantity = measurement.Quantity });

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> UpdateSpaceFixture(int id, double quantity)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            var fixture = await _context.SpaceFixtures.FindAsync(id);

            if (fixture == null) return NotFound();



            fixture.Quantity = quantity;

            await _context.SaveChangesAsync();

            return Ok();

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteUnitSpace(int id)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            var space = await _context.UnitSpaces

                .Include(s => s.Measurements)

                .Include(s => s.Fixtures)

                .FirstOrDefaultAsync(s => s.Id == id);



            if (space == null) return NotFound();



            var unitId = space.BuildingUnitId;

            

            _context.SpaceMeasurements.RemoveRange(space.Measurements);

            _context.SpaceFixtures.RemoveRange(space.Fixtures);

            _context.UnitSpaces.Remove(space);

            

            await _context.SaveChangesAsync();



            return RedirectToAction(nameof(ManageUnitSpaces), new { id = unitId });

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteSpaceFixture(int id)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            var fixture = await _context.SpaceFixtures.FindAsync(id);

            if (fixture == null) return NotFound();



            _context.SpaceFixtures.Remove(fixture);

            await _context.SaveChangesAsync();



            return Ok();

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteSpaceMeasurement(int id)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            var measurement = await _context.SpaceMeasurements.FindAsync(id);

            if (measurement == null) return NotFound();



            _context.SpaceMeasurements.Remove(measurement);

            await _context.SaveChangesAsync();



            return Ok();

        }



        [HttpGet]

        public async Task<IActionResult> GetSpaceFixtures(int spaceId)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            var space = await _context.UnitSpaces

                .Include(s => s.BuildingUnit)

                    .ThenInclude(u => u.Building)

                        .ThenInclude(b => b.ConstructionProject)

                .FirstOrDefaultAsync(s => s.Id == spaceId);



            if (space == null || (!User.IsInRole("Admin") && space.BuildingUnit.Building.ConstructionProject.AgencyId != agencyId))

                return Unauthorized();



            var fixtures = await _context.SpaceFixtures

                .Include(f => f.Supplier)

                .Include(f => f.TechnicalService)

                .Where(f => f.UnitSpaceId == spaceId)

                .Select(f => new {

                    id = f.Id,

                    itemName = f.ItemName,

                    category = f.Category,

                    quantity = f.Quantity,

                    unit = f.Unit,

                    supplierName = f.SupplierId != null ? f.Supplier.BusinessName : f.SupplierName,

                    technicalServiceName = f.TechnicalServiceId != null ? f.TechnicalService.BusinessName : f.TechnicalServiceName,

                    supplierId = f.SupplierId,

                    technicalServiceId = f.TechnicalServiceId,

                    warrantyExpiryDate = f.WarrantyExpiryDate,

                    maintenanceNotes = f.MaintenanceNotes,

                    isCustomizable = f.IsCustomizable

                })

                .ToListAsync();



            return Json(fixtures);

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> AddSpaceFixture(int spaceId, string itemName, string category, double quantity, string unit, string supplierName, string technicalServiceName, string maintenanceNotes, bool isCustomizable = false)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            var space = await _context.UnitSpaces

                .Include(s => s.BuildingUnit)

                    .ThenInclude(u => u.Building)

                        .ThenInclude(b => b.ConstructionProject)

                .FirstOrDefaultAsync(s => s.Id == spaceId);



            if (space == null || (!User.IsInRole("Admin") && space.BuildingUnit.Building.ConstructionProject.AgencyId != agencyId))

                return Unauthorized();



            // Check if supplier is registered (Shadow / CRM Logic) - for now just saving names

            // Later we will implement full Shadow Account creation here if needed.

            

            var fixture = new GMK360.Core.Entities.SpaceFixture

            {

                UnitSpaceId = spaceId,

                ItemName = itemName,

                Category = category,

                Quantity = quantity,

                Unit = unit,

                SupplierName = supplierName,

                TechnicalServiceName = technicalServiceName,

                MaintenanceNotes = maintenanceNotes,

                IsCustomizable = isCustomizable

            };



            _context.SpaceFixtures.Add(fixture);

            await _context.SaveChangesAsync();



            return RedirectToAction(nameof(ManageUnitSpaces), new { id = space.BuildingUnitId });

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> AddUnitSpace(int unitId, string spaceName, string spaceType, double? squareMeters)

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

                Name = spaceName,

                Type = spaceType,

                SquareMeters = squareMeters

            };





            _context.UnitSpaces.Add(space);

            await _context.SaveChangesAsync();



            // Auto-Seed Smart Template

            try

            {

                var templateResult = GetSmartTemplate(spaceType) as JsonResult;

                if (templateResult != null && templateResult.Value is List<SmartTemplateItem> items && items.Count > 0)

                {

                    foreach (var item in items)

                    {

                        if (item.Type == "Measurement")

                        {

                            _context.SpaceMeasurements.Add(new GMK360.Core.Entities.SpaceMeasurement

                            {

                                UnitSpaceId = space.Id,

                                Category = item.Category,

                                Description = item.Name,

                                Quantity = 0, // Starts at 0

                                Unit = item.Unit

                            });

                        }

                        else if (item.Type == "Fixture")

                        {

                            _context.SpaceFixtures.Add(new GMK360.Core.Entities.SpaceFixture

                            {

                                UnitSpaceId = space.Id,

                                Category = item.Category,

                                ItemName = item.Name,

                                Quantity = 0, // Starts at 0

                                Unit = item.Unit,

                                IsCustomizable = false

                            });

                        }

                    }

                    await _context.SaveChangesAsync();

                }

            }

            catch { /* Ignore seeding errors */ }



            return RedirectToAction(nameof(ManageUnitSpaces), new { id = unitId });



        }

    

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> CleanEmptyMeasurements(int spaceId)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            var empties = await _context.SpaceMeasurements

                .Where(m => m.UnitSpaceId == spaceId && (m.Quantity == 0))

                .ToListAsync();



            if (empties.Any())

            {

                _context.SpaceMeasurements.RemoveRange(empties);

                await _context.SaveChangesAsync();

            }



            return Ok(new { deletedCount = empties.Count });

        }
        [HttpPost]
        public async Task<IActionResult> ChangeProjectStatus(int id, string newStatus)
        {
            var project = await _context.ConstructionProjects.FindAsync(id);
            if (project != null && Enum.TryParse(typeof(GMK360.Core.Entities.Construction.ProjectStatus), newStatus, out var statusObj))
            {
                project.Status = (GMK360.Core.Entities.Construction.ProjectStatus)statusObj;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Şantiye başarıyla başlatıldı ve bloklar oluşturuldu.";
            }
            return RedirectToAction("Details", new { id = id });
        }
        [HttpGet]
        public async Task<IActionResult> GetOldBuildingStats(int projectId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var project = await _context.ConstructionProjects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId);

            if (project == null) return NotFound();

            return Json(new {
                toplamKat = project.EskiKatSayisi,
                bodrumKatSayisi = project.EskiBodrumKatSayisi,
                daireSayisi = project.EskiDaireSayisi,
                dukkanSayisi = project.EskiDukkanSayisi,
                arsaAlani = project.TotalLandArea,
                tabanOturumu = project.EskiBinaOturumAlani,
                binaYasi = project.BuildingAge
            });
        }

        [HttpGet]
        public async Task<IActionResult> BudgetDashboard(int id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var project = await _context.ConstructionProjects
                
                .Include(p => p.BudgetItems)
                .FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == agencyId);

            if (project == null) return NotFound();

            // Sadece bÃƒÆ’Ã‚Â¼tÃƒÆ’Ã‚Â§e sayfasÃƒâ€Ã‚Â±nÃƒâ€Ã‚Â±n ÃƒÆ’Ã‚Â§ÃƒÆ’Ã‚Â¶kmemesi iÃƒÆ’Ã‚Â§in temel modeli gÃƒÆ’Ã‚Â¶nderiyoruz.
            return View(project);
        }

        
        [HttpPost]
        public async Task<IActionResult> AddBudgetItem(int projectId, string itemName, decimal amount, string description)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var item = new GMK360.Core.Entities.Construction.ConstructionBudgetItem
            {
                ConstructionProjectId = projectId,
                ItemName = itemName ?? "Masraf",
                ActualTotalCost = amount,
                Description = description,
                QuoteStatus = GMK360.Core.Entities.Construction.BudgetQuoteStatus.ActualInvoiced,
                IsUnplannedExtra = true,
                CreatedAt = System.DateTime.UtcNow,
                IsDeleted = false
            };
            
            _context.ConstructionBudgetItems.Add(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Şantiye başarıyla başlatıldı ve bloklar oluşturuldu.";
            return RedirectToAction(nameof(BudgetDashboard), new { id = projectId });
        }



        // --- YENÃƒâ€Ã‚Â° PAYDAÃƒâ€¦Ã‚Â (GÃƒÆ’Ã¢â‚¬â€œLGE KULLANICI) EKLEME ---
        [HttpPost]
        public async Task<IActionResult> AddStakeholder(int projectId, int role, decimal? sharePercentage, string firstName, string lastName, string phone, string email)
        {
            try
            {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null) 
                {
                    TempData["ErrorMessage"] = "Yetki HatasÃƒâ€Ã‚Â±: Ãƒâ€¦Ã‚Âantiye yÃƒÆ’Ã‚Â¶neticisi veya admin yetkiniz bulunamadÃƒâ€Ã‚Â±.";
                    return RedirectToAction("Details", new { id = projectId });
                }

                var existingUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone || (!string.IsNullOrEmpty(email) && u.Email == email));

                if (existingUser == null)
                {
                    string cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
                    if (!cleanPhone.StartsWith("+90") && !cleanPhone.StartsWith("0")) cleanPhone = "0" + cleanPhone;
                    
                    string generatedUserName = string.IsNullOrEmpty(email) ? cleanPhone : email;
                    string generatedEmail = string.IsNullOrEmpty(email) ? $"{cleanPhone}@gmk360.local" : email;
                    
                    existingUser = new ApplicationUser
                    {
                        UserName = generatedUserName,
                        Email = generatedEmail,
                        FirstName = firstName,
                        LastName = lastName,
                        PhoneNumber = phone,
                        EmailConfirmed = false,
                        PhoneNumberConfirmed = false
                    };

                    var result = await _userManager.CreateAsync(existingUser, "Shadow.User2026!");
                    if (!result.Succeeded)
                    {
                        TempData["ErrorMessage"] = "GÃƒÆ’Ã‚Â¶lge kullanÃƒâ€Ã‚Â±cÃƒâ€Ã‚Â± oluÃƒâ€¦Ã…Â¸turulurken hata oluÃƒâ€¦Ã…Â¸tu.";
                        return RedirectToAction("Details", new { id = projectId });
                    }
                    await _userManager.AddToRoleAsync(existingUser, "Musteri");
                }

                var existingStakeholder = await _context.ProjectStakeholders
                    .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.UserId == existingUser.Id);

                if (existingStakeholder == null)
                {
                    var stakeholder = new GMK360.Core.Entities.Construction.ProjectStakeholder
                    {
                        ProjectId = projectId,
                        UserId = existingUser.Id,
                        Role = (GMK360.Core.Entities.Construction.StakeholderRole)role,
                        SharePercentage = (role == 1) ? sharePercentage : null,
                        Notes = "Firma tarafÃƒâ€Ã‚Â±ndan davet edildi (GÃƒÆ’Ã‚Â¶lge Onay Bekliyor)"
                    };
                    
                    _context.ProjectStakeholders.Add(stakeholder);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"{firstName} {lastName} projeye baÃƒâ€¦Ã…Â¸arÃƒâ€Ã‚Â±yla eklendi! Sisteme giriÃƒâ€¦Ã…Â¸ yapmasÃƒâ€Ã‚Â± iÃƒÆ’Ã‚Â§in SMS gÃƒÆ’Ã‚Â¶nderimi tetiklendi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Bu kiÃƒâ€¦Ã…Â¸i zaten bu projenin paydaÃƒâ€¦Ã…Â¸Ãƒâ€Ã‚Â±!";
                }

                return RedirectToAction("Details", new { id = projectId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Sistem HatasÃƒâ€Ã‚Â±: LÃƒÆ’Ã‚Â¼tfen bilgileri kontrol edin.";
                return RedirectToAction("Details", new { id = projectId });
            }
        }
    
        [HttpPost]
        public async Task<IActionResult> EditStakeholder(int stakeholderId, GMK360.Core.Entities.Construction.StakeholderRole role, decimal? sharePercentage)
        {
            int projectId = 0;
            try
            {
                var st = await _context.ProjectStakeholders.FirstOrDefaultAsync(s => s.Id == stakeholderId);
                if (st == null)
                {
                    TempData["ErrorMessage"] = "PaydaÃƒâ€¦Ã…Â¸ bulunamadÃƒâ€Ã‚Â±.";
                    return RedirectToAction("Index");
                }
                
                projectId = st.ProjectId;
                st.Role = role;
                st.SharePercentage = sharePercentage;
                
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Şantiye başarıyla başlatıldı ve bloklar oluşturuldu.";
                return RedirectToAction("Details", new { id = projectId });
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "PaydaÃƒâ€¦Ã…Â¸ dÃƒÆ’Ã‚Â¼zenlenirken hata oluÃƒâ€¦Ã…Â¸tu: " + ex.Message;
                return projectId > 0 ? RedirectToAction("Details", new { id = projectId }) : RedirectToAction("Index");
            }
        }

    
        [HttpPost]
        public async Task<IActionResult> SyncAllPhases(int projectId)
        {
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project == null) return NotFound();

            var templates = await _context.SystemPhaseTemplates
                .Where(t => !t.IsDeleted)
                .ToListAsync();

            var existingItems = await _context.ConstructionBudgetItems
                .Where(b => b.ConstructionProjectId == projectId)
                .ToListAsync();

            var newItems = new List<ConstructionBudgetItem>();

            foreach (var template in templates)
            {
                bool exists = existingItems.Any(e => 
                    e.PhaseCategory == template.PhaseCategory && 
                    e.SubCategory == template.SubCategory && 
                    e.ItemName == template.ItemName);

                if (!exists)
                {
                    newItems.Add(new ConstructionBudgetItem
                    {
                        ConstructionProjectId = projectId,
                        PhaseCategory = template.PhaseCategory,
                        SubCategory = template.SubCategory,
                        ItemName = template.ItemName,
                        Quantity = 1,
                        Unit = "Adet",
                        PlannedUnitPrice = 0
                    });
                }
            }

            if (newItems.Any())
            {
                _context.ConstructionBudgetItems.AddRange(newItems);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = $"BaÃƒâ€¦Ã…Â¸arÃƒâ€Ã‚Â±yla {newItems.Count} kalem havuza gÃƒÆ’Ã‚Â¶re senkronize edildi!";
            return RedirectToAction("Details", new { id = projectId });
        }

    }
}
