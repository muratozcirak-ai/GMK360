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

    public partial class ConstructionProjectController : Controller



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

            if (agencyConsultant == null && (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")))

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

                .Where(d => d.SourceModule == "Construction" && d.ProjectId == id && (d.Category.Contains("Saha") || d.Title.Contains("Mevcut") || d.Title.Contains("Arsa") || d.Title.Contains("Eski") || d.Title.Contains("Saha")))

                .OrderByDescending(d => d.Id)

                .FirstOrDefaultAsync();

            if (sahaGorseli != null) {

                ViewBag.CurrentStateImageUrl = sahaGorseli.DocumentUrl;

            }

            

            var projeGorseli = await _context.DocumentArchives

                .Where(d => d.SourceModule == "Construction" && d.ProjectId == id && (d.Category.Contains("Proje") || d.Title.Contains("Gelecek") || d.Title.Contains("Render") || (d.Category.Contains("Bina") && !d.Title.Contains("Mevcut") && !d.Title.Contains("Arsa") && !d.Title.Contains("Eski"))))

                .OrderByDescending(d => d.Id)

                .FirstOrDefaultAsync();

            if (projeGorseli != null) {

                ViewBag.CoverImageUrl = projeGorseli.DocumentUrl;

            }

            

            var project = await _context.ConstructionProjects



                



                



                



                .Include(c => c.Amenities)

                .Include(c => c.Tasks)

                .Include(c => c.SubProjects)

                .Include(c => c.ParentProject)

                .Include(c => c.Blocks)

                    .ThenInclude(b => b.Units)

                .FirstOrDefaultAsync(m => m.Id == id);







            if (project == null) return NotFound();







            var agencyId = await GetUserAgencyIdAsync();



            if (!User.IsInRole("Admin") && project.AgencyId != agencyId)



            {



                return Unauthorized();



            }



            bool hasDxfProfiles = await _context.DxfUnitTypeProfiles.AnyAsync(p => p.ProjectId == id);

            ViewBag.HasDxf = project.HasDxfDrawing || hasDxfProfiles;

            ViewBag.ExecutionMode = project.ProjectExecutionMode ?? ((project.HasDxfDrawing || hasDxfProfiles) ? "DxfEngineering" : "DocumentProcurement");



            return View(project);

        }







        // GET: ConstructionProject/Create

        [HttpPost]

        public async Task<IActionResult> InitProject(string projectName, int statusId, string projectType, int? projectOriginId = null)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            var project = new GMK360.Core.Entities.Construction.ConstructionProject

            {

                Name = projectName,

                AgencyId = agencyId.Value,

                Status = (GMK360.Core.Entities.Construction.ProjectStatus)statusId,

                ProjectType = projectType,

                ProjectOriginId = projectOriginId ?? 0,

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



            // Müstakil Yıkım & Hafriyat Şantiyesi

            if (projectOriginId == 5)

            {

                var currentUserId = _userManager.GetUserId(User) ?? "";

                var existingBlock = new GMK360.Core.Entities.Building

                {

                    ConstructionProjectId = project.Id,

                    Name = projectName + " (Yıkılacak Mevcut Yapı)",

                    BlockName = "A Blok (Yıkım)",

                    TotalFloors = 5,

                    ManagerUserId = currentUserId,

                    CityId = 34,

                    DistrictId = 1,

                    NeighborhoodId = 1,

                    StreetName = "-",

                    BuildingNumber = "-",

                    IsExistingBuilding = true,

                    CreatedAt = DateTime.UtcNow

                };

                _context.Buildings.Add(existingBlock);

                await _context.SaveChangesAsync();



                TempData["SuccessMessage"] = "Yıkım ve hafriyat şantiyesi başarıyla oluşturuldu. Söküm ve hurda envanter masasına aktarıldınız.";

                return RedirectToAction("Index", "DemolitionSalvage", new { projectId = project.Id });

            }



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

                    if (string.IsNullOrEmpty(ViewBag.CoverImageUrl)) {

                        var coverDoc = _context.DocumentArchives.FirstOrDefault(d => d.SourceModule == "Construction" && d.ProjectId == draft.Id && (d.Category.Contains("Proje") || d.Title.Contains("Gelecek") || d.Title.Contains("Render")));

                        if (coverDoc != null) ViewBag.CoverImageUrl = coverDoc.DocumentUrl;

                    }



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

                    var currentStateDoc = _context.DocumentArchives.FirstOrDefault(d => d.SourceModule == "Construction" && d.ProjectId == draft.Id && (d.Category.Contains("Saha") || d.Title.Contains("Mevcut") || d.Title.Contains("Arsa") || d.Title.Contains("Eski") || d.Title.Contains("Saha")));

                    if (currentStateDoc != null) {

                        ViewBag.CurrentStateImageUrl = currentStateDoc.DocumentUrl;

                    } else if (!string.IsNullOrEmpty(draft.CurrentStateImageUrl)) {

                        ViewBag.CurrentStateImageUrl = draft.CurrentStateImageUrl;

                    }

                    var tapuDoc = _context.DocumentArchives.FirstOrDefault(d => d.SourceModule == "Construction" && d.ProjectId == draft.Id && (d.Category == "Resmi Evraklar" || d.Title.Contains("Tapu")));

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

                            RoofType = b.RoofType,

                            LayoutPattern = b.LayoutPattern,

                            SubBlocks = draft.Blocks.Where(cb => cb.ParentBuildingId == b.Id).Select(cb => new GMK360.Web.Models.WizardBlockItem {

                                BlockName = cb.BlockName,

                                TotalFloors = cb.TotalFloors ?? 0,

                                TotalApartments = cb.TotalApartments,

                                TotalShops = cb.TotalShops,

                                BasementFloors = cb.BasementFloors,

                                HasGroundFloor = cb.HasGroundFloor,

                                HasRoof = cb.HasRoof,

                                RoofType = cb.RoofType

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

                            RoofType = b.RoofType,

                            LayoutPattern = b.LayoutPattern,

                            SubBlocks = draft.Blocks.Where(cb => cb.ParentBuildingId == b.Id).Select(cb => new GMK360.Web.Models.WizardBlockItem {

                                BlockName = cb.BlockName,

                                TotalFloors = cb.TotalFloors ?? 0,

                                TotalApartments = cb.TotalApartments,

                                TotalShops = cb.TotalShops,

                                BasementFloors = cb.BasementFloors,

                                HasGroundFloor = cb.HasGroundFloor,

                                HasRoof = cb.HasRoof,

                                RoofType = cb.RoofType

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



                if (agencyId == null) return Json(new { success = false, message = "Yetkisiz erişim." });



                if (model.DraftProjectId <= 0) return Json(new { success = false, message = "Proje ID bulunamadı. Lütfen önce projeyi başlatın." });

                var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == model.DraftProjectId);

                if (project == null) return Json(new { success = false, message = "Proje bulunamadı." });

                if (project.IsDataLocked) return Json(new { success = false, message = "Bu proje bütçe kontrolü için kilitlenmiştir, veriler değiştirilemez." });



                // Update fields safely - DB columns Address and Description do not allow nulls

                project.Description = model.Description ?? "";

                project.Address = !string.IsNullOrWhiteSpace(model.Address) ? model.Address.Trim() : (project.Address ?? "");

                project.StartDate = model.StartDate.Year > 1900 ? model.StartDate : (project.StartDate.Year > 1900 ? project.StartDate : DateTime.Today);

                if (model.EndDate > DateTime.MinValue && model.EndDate.Year > 1900) project.EndDate = model.EndDate;

                // project.Status removed to prevent unwanted status changes.

                if (!string.IsNullOrEmpty(model.ProjectType)) project.ProjectType = model.ProjectType;

                project.ProjectOriginId = model.ProjectOriginId > 0 ? model.ProjectOriginId : (project.ProjectOriginId ?? 0);

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



                string webRoot = _hostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                string baseFolder = Path.Combine(webRoot, "uploads", $"Agency_{agencyId.Value}", $"Project_{project.Id}");

                string imagesFolder = Path.Combine(baseFolder, "images");

                string docsFolder = Path.Combine(baseFolder, "documents");

                Directory.CreateDirectory(imagesFolder);

                Directory.CreateDirectory(docsFolder);



                // Helper to sanitize project name for filenames

                string safeProjName = string.Join("_", project.Name.Split(Path.GetInvalidFileNameChars()));

                safeProjName = safeProjName.Replace(" ", "_");



                // --- 1. COVER IMAGE (3D RENDER / GELECEĞİ HALİ) ---

                if (model.CoverImageFile != null && model.CoverImageFile.Length > 0)

                {

                    var oldArchives = await _context.DocumentArchives

                        .Where(d => d.ProjectId == project.Id && (d.Title.Contains("Gelecek") || d.Title.Contains("Render") || d.Title.Contains("Proje Görseli")))

                        .ToListAsync();

                    foreach (var oldArchive in oldArchives)

                    {

                        var oldFilePath = Path.Combine(webRoot, oldArchive.DocumentUrl.TrimStart('/'));

                        if (System.IO.File.Exists(oldFilePath)) { try { System.IO.File.Delete(oldFilePath); } catch {} }

                        _context.DocumentArchives.Remove(oldArchive);

                    }



                    string extension = Path.GetExtension(model.CoverImageFile.FileName);

                    if (string.IsNullOrEmpty(extension)) extension = ".jpg";

                    string standardizedName = $"{safeProjName}_3D_Render_GelecekHali{extension}";

                    string filePath = Path.Combine(imagesFolder, standardizedName);

                    

                    using (var fileStream = new FileStream(filePath, FileMode.Create))

                    {

                        await model.CoverImageFile.CopyToAsync(fileStream);

                    }

                    var archive = new GMK360.Core.Entities.DocumentArchive

                    {

                        AgencyId = agencyId.Value,

                        ProjectId = project.Id,

                        Title = "Proje Görseli (3D Render / Geleceği Hali)",

                        Category = "Bina Görselleri",

                        SourceModule = "Construction",

                        DocumentUrl = $"/uploads/Agency_{agencyId.Value}/Project_{project.Id}/images/{standardizedName}",

                        FileName = standardizedName,

                        FileExtension = extension,

                        Year = DateTime.Now.Year,

                        UploadDate = DateTime.UtcNow,

                        Status = "Tamamlandı"

                    };

                    _context.DocumentArchives.Add(archive);

                    project.CoverImageUrl = archive.DocumentUrl;

                }



                // --- 2. CURRENT STATE IMAGE (BOŞ ARSA VEYA YIKILACAK ESKİ YAPI) ---

                if (model.CurrentStateImageFile != null && model.CurrentStateImageFile.Length > 0)

                {

                    var oldArchives = await _context.DocumentArchives

                        .Where(d => d.ProjectId == project.Id && (d.Title.Contains("Mevcut") || d.Title.Contains("Eski") || d.Title.Contains("Arsa") || d.Title.Contains("Saha")))

                        .ToListAsync();

                    foreach (var oldArchive in oldArchives)

                    {

                        var oldFilePath = Path.Combine(webRoot, oldArchive.DocumentUrl.TrimStart('/'));

                        if (System.IO.File.Exists(oldFilePath)) { try { System.IO.File.Delete(oldFilePath); } catch {} }

                        _context.DocumentArchives.Remove(oldArchive);

                    }



                    string extension = Path.GetExtension(model.CurrentStateImageFile.FileName);

                    if (string.IsNullOrEmpty(extension)) extension = ".jpg";



                    bool isKentselDonusum = project.ProjectOriginId == 1;

                    string docTitle = isKentselDonusum ? "Yıkılacak Eski Yapı Görseli" : "Arsa / Saha Görseli (Boş Hali)";

                    string standardizedName = isKentselDonusum 

                        ? $"{safeProjName}_Yikilacak_Eski_Yapi{extension}" 

                        : $"{safeProjName}_Bos_Arsa_Saha{extension}";



                    string filePath = Path.Combine(imagesFolder, standardizedName);

                    

                    using (var fileStream = new FileStream(filePath, FileMode.Create))

                    {

                        await model.CurrentStateImageFile.CopyToAsync(fileStream);

                    }

                    var archive = new GMK360.Core.Entities.DocumentArchive

                    {

                        AgencyId = agencyId.Value,

                        ProjectId = project.Id,

                        Title = docTitle,

                        Category = "Bina Görselleri",

                        SourceModule = "Construction",

                        DocumentUrl = $"/uploads/Agency_{agencyId.Value}/Project_{project.Id}/images/{standardizedName}",

                        FileName = standardizedName,

                        FileExtension = extension,

                        Year = DateTime.Now.Year,

                        UploadDate = DateTime.UtcNow,

                        Status = "Tamamlandı"

                    };

                    _context.DocumentArchives.Add(archive);

                    project.CurrentStateImageUrl = archive.DocumentUrl;

                }



                // --- 3. TAPU DOCUMENT (TAPU BELGESİ) ---

                if (model.TapuDocumentFile != null && model.TapuDocumentFile.Length > 0)

                {

                    var oldArchives = await _context.DocumentArchives

                        .Where(d => d.ProjectId == project.Id && (d.Category == "Resmi Evraklar" || d.Title.Contains("Tapu")))

                        .ToListAsync();

                    foreach (var oldArchive in oldArchives)

                    {

                        var oldFilePath = Path.Combine(webRoot, oldArchive.DocumentUrl.TrimStart('/'));

                        if (System.IO.File.Exists(oldFilePath)) { try { System.IO.File.Delete(oldFilePath); } catch {} }

                        _context.DocumentArchives.Remove(oldArchive);

                    }



                    string extension = Path.GetExtension(model.TapuDocumentFile.FileName);

                    if (string.IsNullOrEmpty(extension)) extension = ".pdf";

                    string standardizedName = $"{safeProjName}_Tapu_Belgesi{extension}";

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

                        Year = DateTime.Now.Year,

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



                var existingBlocks = await _context.Buildings

                    .Where(b => b.ConstructionProjectId == model.DraftProjectId)

                    .ToListAsync();



                var parentBlocks = new System.Collections.Generic.List<GMK360.Web.Models.WizardBlockItem>();

                if (model.ExistingBlocks != null) { foreach(var b in model.ExistingBlocks) { b.IsExistingBuilding = true; parentBlocks.Add(b); } }

                if (model.TargetBlocks != null) { foreach(var b in model.TargetBlocks) { b.IsExistingBuilding = false; parentBlocks.Add(b); } }



                var keptExistingBuildingIds = new System.Collections.Generic.HashSet<int>();

                

                int blockCounter = 1;

                foreach (var pb in parentBlocks) {

                    var existingParent = existingBlocks.FirstOrDefault(eb => 

                        eb.ParentBuildingId == null && 

                        eb.IsExistingBuilding == pb.IsExistingBuilding && 

                        eb.BlockName == pb.BlockName && 

                        !keptExistingBuildingIds.Contains(eb.Id));

                    

                    if (existingParent != null) {

                        existingParent.BaseArea = pb.BaseArea;

                        existingParent.BasementFloors = pb.BasementFloors;

                        existingParent.TotalFloors = pb.TotalFloors;

                        existingParent.TotalUnits = pb.TotalApartments + pb.TotalShops;

                        existingParent.TotalApartments = pb.TotalApartments;

                        existingParent.TotalShops = pb.TotalShops;

                        existingParent.HasGroundFloor = pb.HasGroundFloor;

                        existingParent.HasRoof = pb.HasRoof;

                        existingParent.RoofType = pb.RoofType;

                        existingParent.BuildingAge = pb.BuildingAge;

                        existingParent.LayoutPattern = pb.LayoutPattern;

                        keptExistingBuildingIds.Add(existingParent.Id);

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

                            RoofType = pb.RoofType,

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

                    }

                    

                    if (pb.SubBlocks != null && pb.SubBlocks.Any()) {

                        foreach (var sb in pb.SubBlocks) {

                            GMK360.Core.Entities.Building? existingChild = null;

                            if (existingParent.Id > 0) {

                                existingChild = existingBlocks.FirstOrDefault(eb => 

                                    eb.ParentBuildingId == existingParent.Id && 

                                    eb.IsExistingBuilding == pb.IsExistingBuilding && 

                                    eb.BlockName == sb.BlockName && 

                                    !keptExistingBuildingIds.Contains(eb.Id));

                            }

                            

                            if (existingChild != null) {

                                existingChild.TotalFloors = sb.TotalFloors;

                                existingChild.TotalUnits = sb.TotalApartments + sb.TotalShops;

                                existingChild.TotalApartments = sb.TotalApartments;

                                existingChild.TotalShops = sb.TotalShops;

                                existingChild.BasementFloors = sb.BasementFloors;

                                existingChild.HasGroundFloor = sb.HasGroundFloor;

                                existingChild.HasRoof = sb.HasRoof;

                                existingChild.RoofType = sb.RoofType;

                                keptExistingBuildingIds.Add(existingChild.Id);

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

                                    RoofType = sb.RoofType,

                                    ConstructionProjectId = project.Id,

                                    ParentBuilding = existingParent,

                                    CityId = project.CityId ?? 34,

                                    DistrictId = project.DistrictId ?? 1,

                                    NeighborhoodId = project.NeighborhoodId ?? 1,

                                    StreetId = project.StreetId,

                                    ManagerUserId = _userManager.GetUserId(User) ?? ""

                                };

                                _context.Buildings.Add(existingChild);

                            }

                        }

                    }

                    blockCounter++;

                }

                

                // Güvenli silme: Bu projeye ait olup forma dahil edilmeyen tüm eski binalar silinir (önce alt bloklar, sonra ebeveyn)

                var blocksToRemove = existingBlocks

                    .Where(b => b.Id > 0 && !keptExistingBuildingIds.Contains(b.Id))

                    .OrderByDescending(b => b.ParentBuildingId.HasValue ? 1 : 0)

                    .ToList();



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



        



        



        



        // POST: ConstructionProject/CreateSubProjectFromAmenity

        [HttpPost]

        public async Task<IActionResult> CreateSubProjectFromAmenity([FromBody] CreateSubProjectDto dto)

        {

            if (dto == null || dto.ParentProjectId <= 0)

                return Json(new { success = false, message = "Geçersiz ana proje parametresi." });



            var parent = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == dto.ParentProjectId);

            if (parent == null)

                return Json(new { success = false, message = "Ana proje bulunamadı." });



            if (dto.AmenityId.HasValue && dto.AmenityId.Value > 0)

            {

                var existingSub = await _context.ConstructionProjects

                    .FirstOrDefaultAsync(p => p.ParentProjectId == dto.ParentProjectId && p.RelatedAmenityId == dto.AmenityId);



                if (existingSub != null)

                {

                    return Json(new { 

                        success = true, 

                        alreadyExists = true, 

                        subProjectId = existingSub.Id, 

                        subProjectName = existingSub.Name,

                        message = $"'{existingSub.Name}' için zaten açık bir alt proje mevcut.",

                        redirectUrl = $"/ConstructionProject/Details/{existingSub.Id}" 

                    });

                }

            }



            bool isDxfMode = dto.ExecutionMode == "DxfEngineering" || dto.HasDxf;

            string category = !string.IsNullOrWhiteSpace(dto.Category) ? dto.Category : "SosyalDonati_DisSaha";

            string projectName = !string.IsNullOrWhiteSpace(dto.CustomProjectName)

                ? dto.CustomProjectName

                : (!string.IsNullOrWhiteSpace(dto.AmenityName) ? $"{parent.Name} - {dto.AmenityName} İmalatı" : $"{parent.Name} - Alt İmalat Paketi");



            string description = !string.IsNullOrWhiteSpace(dto.Description)

                ? dto.Description

                : (isDxfMode 

                    ? $"{parent.Name} ana projesine bağlı, AutoCAD/DXF çizimi ve detaylı metrajı bulunan mühendislik alt projesidir."

                    : $"{parent.Name} ana projesine bağlı, teknik şartname, malzeme kataloğu ve taşeron teklif toplama (RFQ) odaklı satınalma paketidir.");



            var subProject = new ConstructionProject

            {

                Name = projectName,

                Description = description,

                ParentProjectId = parent.Id,

                RelatedAmenityId = (dto.AmenityId.HasValue && dto.AmenityId.Value > 0) ? dto.AmenityId : null,

                SubProjectCategory = category,

                ProjectType = isDxfMode ? "Mühendislik & Metraj Projesi" : "Teklif & Dökümantasyon Paketi",

                HasDxfDrawing = isDxfMode,

                ProjectExecutionMode = isDxfMode ? "DxfEngineering" : "DocumentProcurement",

                Status = ProjectStatus.Projelendirme_Teklif,

                LifecycleStatus = GMK360.Core.Entities.Enums.ProjectLifecycleStatus.UnderConstruction,

                AgencyId = parent.AgencyId,

                Address = parent.Address,

                CityId = parent.CityId,

                DistrictId = parent.DistrictId,

                NeighborhoodId = parent.NeighborhoodId,

                StartDate = DateTime.Today,

                SelectionDeadline = DateTime.Today.AddMonths(1),

                CreatedAt = DateTime.UtcNow

            };



            _context.ConstructionProjects.Add(subProject);

            await _context.SaveChangesAsync();



            if (isDxfMode)

            {

                var typeA = new GMK360.Core.Entities.Construction.Workflows.DxfUnitTypeProfile 

                { 

                    ProjectId = subProject.Id, 

                    ModelName = $"{subProject.Name} - Detay Planı", 

                    Category = "Özel İmalat", 

                    TotalGrossArea = 100, 

                    TotalNetArea = 85, 

                    TotalSpaceCount = 2 

                };

                _context.DxfUnitTypeProfiles.Add(typeA);

                await _context.SaveChangesAsync();



                _context.DxfUnitSpaces.Add(new GMK360.Core.Entities.Construction.Workflows.DxfUnitSpace

                {

                    DxfUnitTypeProfileId = typeA.Id,

                    SpaceName = "Ana İmalat Alanı",

                    SpaceGroup = "İmalat Sahası",

                    FloorArea = 85,

                    CeilingArea = 85,

                    GrossWallArea = 60,

                    NetWallArea = 50,

                    SkirtingLength = 30,

                    WindowSillLength = 0,

                    InteriorDoorCount = 1,

                    WindowCount = 0

                });

                await _context.SaveChangesAsync();

            }



            return Json(new { 

                success = true, 

                subProjectId = subProject.Id, 

                subProjectName = subProject.Name,

                isDxfMode = isDxfMode,

                message = $"'{subProject.Name}' ({ (isDxfMode ? "📐 Mühendislik & Metraj" : "📄 Teklif & Dökümantasyon") }) başarıyla açıldı!",

                redirectUrl = $"/ConstructionProject/Details/{subProject.Id}" 

            });

        }



        // POST: ConstructionProject/UploadProjectDxf

        [HttpPost]

        public async Task<IActionResult> UploadProjectDxf(int projectId, IFormFile? dxfFile)

        {

            var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return NotFound();



            if (dxfFile != null && dxfFile.Length > 0)

            {

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"Project_{projectId}", "dxf");

                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);



                var ext = Path.GetExtension(dxfFile.FileName).ToLowerInvariant();

                var uniqueName = $"drawing_{projectId}_{Guid.NewGuid().ToString("N").Substring(0, 8)}{ext}";

                var filePath = Path.Combine(uploadsFolder, uniqueName);



                using (var stream = new FileStream(filePath, FileMode.Create))

                {

                    await dxfFile.CopyToAsync(stream);

                }



                project.HasDxfDrawing = true;

                project.ProjectExecutionMode = "DxfEngineering";

                project.DxfFilePath = $"/uploads/Project_{projectId}/dxf/{uniqueName}";

                await _context.SaveChangesAsync();



                var hasProfiles = await _context.DxfUnitTypeProfiles.AnyAsync(p => p.ProjectId == projectId);

                if (!hasProfiles)

                {

                    var typeA = new GMK360.Core.Entities.Construction.Workflows.DxfUnitTypeProfile 

                    { 

                        ProjectId = projectId, 

                        ModelName = $"{project.Name} - Tip Planı", 

                        Category = "Özel İmalat", 

                        TotalGrossArea = 120, 

                        TotalNetArea = 100, 

                        TotalSpaceCount = 2 

                    };

                    _context.DxfUnitTypeProfiles.Add(typeA);

                    await _context.SaveChangesAsync();



                    _context.DxfUnitSpaces.Add(new GMK360.Core.Entities.Construction.Workflows.DxfUnitSpace

                    {

                        DxfUnitTypeProfileId = typeA.Id,

                        SpaceName = "Ana İmalat Alanı",

                        SpaceGroup = "İmalat Sahası",

                        FloorArea = 100,

                        CeilingArea = 100,

                        GrossWallArea = 70,

                        NetWallArea = 60,

                        SkirtingLength = 35,

                        WindowSillLength = 0,

                        InteriorDoorCount = 1,

                        WindowCount = 0

                    });

                    await _context.SaveChangesAsync();

                }



                TempData["SuccessMessage"] = "AutoCAD / DXF çizimi başarıyla yüklendi! Proje detaylı mühendislik ve metraj moduna yükseltildi.";

                return RedirectToAction(nameof(Details), new { id = projectId });

            }



            project.HasDxfDrawing = true;

            project.ProjectExecutionMode = "DxfEngineering";

            await _context.SaveChangesAsync();



            TempData["SuccessMessage"] = "Proje Detaylı Mühendislik ve Metraj moduna alındı.";

            return RedirectToAction(nameof(Details), new { id = projectId });

        }



        // POST: ConstructionProject/SetDocumentProcurementMode

        [HttpPost]

        public async Task<IActionResult> SetDocumentProcurementMode(int projectId)

        {

            var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return NotFound();



            project.HasDxfDrawing = false;

            project.ProjectExecutionMode = "DocumentProcurement";

            await _context.SaveChangesAsync();



            TempData["SuccessMessage"] = "Proje Teklif & Dökümantasyon (RFQ) moduna alındı.";

            return RedirectToAction(nameof(Details), new { id = projectId });

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

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> TransferToBuildingManagement(int buildingId)

        {

            var building = await _context.Buildings

                .Include(b => b.Units)

                .FirstOrDefaultAsync(b => b.Id == buildingId);



            if (building == null) return NotFound("Blok / Yapı bulunamadı.");



            var currentUserId = _userManager.GetUserId(User) ?? "";

            if (string.IsNullOrEmpty(building.ManagerUserId))

            {

                building.ManagerUserId = currentUserId;

            }

            if (string.IsNullOrEmpty(building.CreatedByUserId))

            {

                building.CreatedByUserId = currentUserId;

            }



            building.OnboardingStep = 5; // Tamamen aktif

            building.IsApproved = true;



            await _context.SaveChangesAsync();



            TempData["SuccessMessage"] = $"🎉 Tebrikler! '{building.Name}' bloğu inşaat aşamasından KMK 634 Sayılı Kanuna uygun Bina & Site Yönetimine başarıyla devredildi.";

            return RedirectToAction("Detail", "BuildingManager", new { id = building.Id });

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> BulkGenerateEmptyUnits(int projectId, List<string> targetFloors, string category, int count)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Json(new { success = false, message = "Yetkisiz işlem." });



            var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == projectId && p.AgencyId == agencyId);

            if (project == null && !User.IsInRole("Admin")) return Json(new { success = false, message = "Proje bulunamadı." });



            if (targetFloors == null || !targetFloors.Any())

            {

                return Json(new { success = false, message = "Lütfen en az bir kat seçin." });

            }



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

                            var existingFloorUnits = await _context.BuildingUnits

                                .Where(x => x.BuildingId == buildingId && x.FloorLevel == floorLevel)

                                .ToListAsync();



                            for (int i = 0; i < count; i++)

                            {

                                int seq = existingFloorUnits.Count + i + 1;

                                string doorNo = "-";

                                string layout = "Mesken";

                                string actualCategory = category;



                                if (category == "Dükkan")

                                {

                                    layout = "Dükkan";

                                    doorNo = $"Dükkan {seq}";

                                    actualCategory = "Dükkan";

                                }

                                else if (category == "Ofis")

                                {

                                    layout = "Ofis";

                                    doorNo = $"Ofis {seq}";

                                    actualCategory = "Ofis";

                                }

                                else if (category == "Depo" || category == "Eklenti")

                                {

                                    layout = "Depo / Ardiye";

                                    doorNo = count == 1 ? "Ardiye / Depo" : $"Depo {seq}";

                                    actualCategory = "Depo";

                                }

                                else if (category == "Sığınak" || category == "Su Deposu & Hidrofor" || category == "Kapalı Otopark" ||

                                         category == "Açık Otopark" || category == "Jeneratör Odası" || category == "Kazan Dairesi" ||

                                         category == "Yangın Pompa İstasyonu" || category == "Bina Giriş Holü" ||

                                         category == "Asansör Makine Dairesi" || category == "Elektrik & Pano Odası" ||

                                         category == "Ortak Alan (Genel)" || category.StartsWith("OrtakAlan"))

                                {

                                    actualCategory = "OrtakAlan";

                                    string cleanName = category == "Ortak Alan (Genel)" ? "Ortak Alan" : category;

                                    layout = cleanName;



                                    bool alreadyExistsWithExactName = existingFloorUnits.Any(u => u.DoorNumber.Equals(cleanName, StringComparison.OrdinalIgnoreCase));

                                    if (count == 1 && !alreadyExistsWithExactName)

                                    {

                                        doorNo = cleanName;

                                    }

                                    else

                                    {

                                        int commonCountInFloor = existingFloorUnits.Count(u => u.Category == "OrtakAlan" && u.DoorNumber.StartsWith(cleanName, StringComparison.OrdinalIgnoreCase));

                                        doorNo = $"{cleanName} {commonCountInFloor + i + 1}";

                                    }

                                }

                                else

                                {

                                    actualCategory = "Daire";

                                    layout = "Mesken";

                                    doorNo = "-";

                                }



                                var unit = new GMK360.Core.Entities.BuildingUnit

                                {

                                    BuildingId = buildingId,

                                    FloorLevel = floorLevel,

                                    FloorName = floorLevel == 0 ? "Zemin Kat" : (floorLevel < 0 ? $"{-floorLevel}. Bodrum" : $"{floorLevel}. Kat"),

                                    Category = actualCategory,

                                    DoorNumber = doorNo, 

                                    FacadeDirection = "-",

                                    RoomLayout = layout

                                };

                                _context.BuildingUnits.Add(unit);

                                addedCount++;

                            }

                        }

                    }

                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"{addedCount} adet birim/alan ({category}) başarıyla üretildi!" });

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



            if (block.ParentBuildingId.HasValue)

            {

                return RedirectToAction(nameof(ManageBlock), new { id = block.ParentBuildingId.Value });

            }



            if (!User.IsInRole("Admin") && block.ConstructionProject.AgencyId != agencyId)

            {

                return Unauthorized();

            }







            // Mimari görselleri ve kat planlarını getir (Ana blok ve alt kuleler dahil)

            var allBuildingIds = new List<int> { id };

            if (block.ChildBuildings != null && block.ChildBuildings.Any())

            {

                allBuildingIds.AddRange(block.ChildBuildings.Select(c => c.Id));

            }



            var floorEntityTypes = allBuildingIds.Select(bId => "BuildingFloor_" + bId).ToList();



            var architectureDocs = await _context.DmsDocuments

                .Where(d => (d.EntityType == "Building" && d.EntityId == id) ||

                            floorEntityTypes.Contains(d.EntityType))

                .ToListAsync();



            ViewBag.BlockImages = architectureDocs.Where(d => d.EntityType == "Building").ToList();

            ViewBag.FloorPlans = architectureDocs.Where(d => floorEntityTypes.Contains(d.EntityType)).ToList();



            ViewBag.Stakeholders = await _context.ProjectStakeholders

                .Include(s => s.User)

                .Where(s => s.ProjectId == block.ConstructionProjectId && !s.IsDeleted)

                .OrderBy(s => s.Role)

                .ThenBy(s => s.User.FirstName)

                .ToListAsync();



            ViewBag.ArchitecturalDwg = await _context.DocumentArchives

                .Where(d => d.ProjectId == block.ConstructionProjectId && !d.IsDeleted && (d.FileExtension.ToLower() == ".dwg" || d.FileExtension.ToLower() == ".dxf"))

                .OrderByDescending(d => d.Category == "Mimari")

                .ThenByDescending(d => d.Id)

                .FirstOrDefaultAsync();



            ViewBag.DxfProfiles = await _context.DxfUnitTypeProfiles

                .Include(p => p.Spaces)

                .Where(p => p.ProjectId == block.ConstructionProjectId)

                .ToListAsync();



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



            ViewBag.IsExisting = isExisting;

            return View(project);

        }



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

                TempData["SuccessMessage"] = "Dış alan donatısı başarıyla güncellendi.";

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

                Description = description,

                IsExisting = isExisting

            };



            _context.ProjectAmenities.Add(amenity);

            await _context.SaveChangesAsync();



            TempData["SuccessMessage"] = "Dış alan donatısı başarıyla eklendi.";

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

                TempData["SuccessMessage"] = "Dış alan donatısı silindi.";

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



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> UpdateBlockDetails(

            int Id, 

            string BlockName, 

            double? BaseArea, 

            int? TotalFloors,

            int? BasementFloors,

            bool? HasGroundFloor,

            bool? HasRoof,

            string? RoofType,

            List<string>? SelectedFeatures, 

            string? TechnicalFeatures, 

            string? Description, 

            string? InsulationType, 

            int? ElevatorCount, 

            string? ParkingType,

            int? KotOffset = null,

            string? returnUrl = null)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            var building = await _context.Buildings

                .Include(b => b.ConstructionProject)

                .FirstOrDefaultAsync(b => b.Id == Id);



            if (building == null || (!User.IsInRole("Admin") && building.ConstructionProject?.AgencyId != agencyId))

                return NotFound();



            building.BlockName = BlockName;

            building.Name = BlockName;

            building.BaseArea = BaseArea;



            if (TotalFloors.HasValue) building.TotalFloors = TotalFloors.Value;

            if (BasementFloors.HasValue) building.BasementFloors = BasementFloors.Value;

            if (HasGroundFloor.HasValue) building.HasGroundFloor = HasGroundFloor.Value;

            if (HasRoof.HasValue) building.HasRoof = HasRoof.Value;

            if (!string.IsNullOrEmpty(RoofType)) building.RoofType = RoofType;



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



            // Kot Offset (Arazi eğimi / kademe farkı)

            if (KotOffset.HasValue)

            {

                int offsetVal = KotOffset.Value;

                if (offsetVal != 0)

                {

                    if (string.IsNullOrEmpty(building.AttachedToBlock))

                    {

                        building.AttachedToBlock = $"KotOffset:{offsetVal}";

                    }

                    else

                    {

                        if (System.Text.RegularExpressions.Regex.IsMatch(building.AttachedToBlock, @"(?:Kot|Offset)\s*[:=]\s*[+-]?\d+"))

                        {

                            building.AttachedToBlock = System.Text.RegularExpressions.Regex.Replace(building.AttachedToBlock, @"(?:Kot|Offset)\s*[:=]\s*[+-]?\d+", $"KotOffset:{offsetVal}");

                        }

                        else

                        {

                            building.AttachedToBlock += $" (KotOffset:{offsetVal})";

                        }

                    }

                }

                else

                {

                    if (!string.IsNullOrEmpty(building.AttachedToBlock))

                    {

                        building.AttachedToBlock = System.Text.RegularExpressions.Regex.Replace(building.AttachedToBlock, @"\s*\(?KotOffset:[+-]?\d+\)?", "").Trim();

                    }

                }

            }



            _context.Update(building);

            await _context.SaveChangesAsync();



            TempData["SuccessMessage"] = $"{BlockName} özellikleri başarıyla güncellendi.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))

            {

                return Redirect(returnUrl);

            }

            return RedirectToAction(nameof(ManageBlock), new { id = building.ParentBuildingId ?? Id });

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteBlock(int id, int projectId)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == projectId && (p.AgencyId == agencyId || User.IsInRole("Admin")));

            if (project == null) return NotFound();



            var building = await _context.Buildings

                .Include(b => b.ChildBuildings)

                .Include(b => b.Units)

                    .ThenInclude(u => u.Spaces)

                        .ThenInclude(s => s.Measurements)

                .Include(b => b.Units)

                    .ThenInclude(u => u.Spaces)

                        .ThenInclude(s => s.Fixtures)

                .FirstOrDefaultAsync(b => b.Id == id && b.ConstructionProjectId == projectId);



            if (building == null) return NotFound();



            // Cascade delete child buildings/towers if any

            if (building.ChildBuildings != null && building.ChildBuildings.Any())

            {

                foreach (var child in building.ChildBuildings.ToList())

                {

                    var childUnits = await _context.BuildingUnits

                        .Include(u => u.Spaces).ThenInclude(s => s.Measurements)

                        .Include(u => u.Spaces).ThenInclude(s => s.Fixtures)

                        .Where(u => u.BuildingId == child.Id)

                        .ToListAsync();



                    foreach (var u in childUnits)

                    {

                        foreach (var s in u.Spaces)

                        {

                            _context.SpaceMeasurements.RemoveRange(s.Measurements);

                            _context.SpaceFixtures.RemoveRange(s.Fixtures);

                        }

                        _context.UnitSpaces.RemoveRange(u.Spaces);

                    }

                    _context.BuildingUnits.RemoveRange(childUnits);

                    _context.Buildings.Remove(child);

                }

            }



            if (building.Units != null && building.Units.Any())

            {

                foreach (var u in building.Units)

                {

                    foreach (var s in u.Spaces)

                    {

                        _context.SpaceMeasurements.RemoveRange(s.Measurements);

                        _context.SpaceFixtures.RemoveRange(s.Fixtures);

                    }

                    _context.UnitSpaces.RemoveRange(u.Spaces);

                }

                _context.BuildingUnits.RemoveRange(building.Units);

            }



            _context.Buildings.Remove(building);

            await _context.SaveChangesAsync();



            TempData["SuccessMessage"] = $"{building.BlockName} başarıyla silindi.";

            return RedirectToAction(nameof(Details), new { id = projectId });

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> AddChildBlock(

            int projectId,

            int parentBuildingId,

            string blockName,

            double? baseArea,

            int totalFloors,

            int basementFloors,

            bool hasGroundFloor,

            bool hasRoof,

            string? roofType,

            int? elevatorCount,

            string? parkingType,

            string? insulationType,

            string? technicalFeatures,

            string? description,

            int? kotOffset = null)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            var parent = await _context.Buildings

                .Include(b => b.ConstructionProject)

                .FirstOrDefaultAsync(b => b.Id == parentBuildingId && b.ConstructionProjectId == projectId);



            if (parent == null || (!User.IsInRole("Admin") && parent.ConstructionProject?.AgencyId != agencyId))

                return Unauthorized();



            var child = new GMK360.Core.Entities.Building

            {

                ConstructionProjectId = projectId,

                ParentBuildingId = parentBuildingId,

                Name = blockName,

                BlockName = blockName,

                BaseArea = baseArea,

                TotalFloors = totalFloors,

                BasementFloors = basementFloors,

                HasGroundFloor = hasGroundFloor,

                HasRoof = hasRoof,

                RoofType = roofType,

                ElevatorCount = elevatorCount,

                ParkingType = parkingType,

                InsulationType = insulationType,

                TechnicalFeatures = technicalFeatures,

                Description = description,

                IsExistingBuilding = parent.IsExistingBuilding,

                LayoutPattern = parent.LayoutPattern,

                AttachedToBlock = (kotOffset.HasValue && kotOffset.Value != 0) ? $"KotOffset:{kotOffset.Value}" : null

            };



            _context.Buildings.Add(child);

            await _context.SaveChangesAsync();



            TempData["SuccessMessage"] = $"{blockName} kulesi/bloğu başarıyla eklendi!";

            return RedirectToAction(nameof(Details), new { id = projectId });

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



            return RedirectToAction(nameof(ManageBlock), new { id = building.ParentBuildingId ?? buildingId });

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

                .Include(u => u.Spaces)

                    .ThenInclude(s => s.Measurements)

                .Include(u => u.Spaces)

                    .ThenInclude(s => s.Fixtures)

                        .ThenInclude(f => f.Options)

                .Where(u => u.BuildingId == buildingId && u.FloorLevel == sourceFloorLevel);



            if (selectedUnitIds != null && selectedUnitIds.Any()) {

                sourceUnitsQuery = sourceUnitsQuery.Where(u => selectedUnitIds.Contains(u.Id));

            }



            var sourceUnits = await sourceUnitsQuery.ToListAsync();



            if (!sourceUnits.Any())

                return RedirectToAction(nameof(ManageBlock), new { id = building.ParentBuildingId ?? buildingId });



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



                    // 1. Aşama: Birimleri kopyala (çakışma varsa atla)

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

                                var newEklenti = idMapping[u.Id];

                                var newParentDaire = idMapping[u.ParentUnitId.Value];

                                newEklenti.ParentUnitId = newParentDaire.Id;

                            }

                        }

                    }



                    // 3. Aşama: Mahaller (Spaces), Ölçüler (Measurements) ve Demirbaşları (Fixtures) kopyala

                    foreach (var u in sourceUnits)

                    {

                        if (idMapping.ContainsKey(u.Id))

                        {

                            var targetUnit = idMapping[u.Id];

                            if (u.Spaces != null && u.Spaces.Any())

                            {

                                foreach (var srcSpace in u.Spaces)

                                {

                                    var newSpace = new GMK360.Core.Entities.UnitSpace

                                    {

                                        BuildingUnitId = targetUnit.Id,

                                        Name = srcSpace.Name,

                                        Type = srcSpace.Type,

                                        SquareMeters = srcSpace.SquareMeters,

                                        PlanImagePath = srcSpace.PlanImagePath,

                                        Description = srcSpace.Description,

                                        Measurements = new List<GMK360.Core.Entities.SpaceMeasurement>(),

                                        Fixtures = new List<GMK360.Core.Entities.SpaceFixture>()

                                    };



                                    if (srcSpace.Measurements != null)

                                    {

                                        foreach (var m in srcSpace.Measurements)

                                        {

                                            newSpace.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement

                                            {

                                                Category = m.Category,

                                                Description = m.Description,

                                                Width = m.Width,

                                                Length = m.Length,

                                                Height = m.Height,

                                                Quantity = m.Quantity,

                                                Unit = m.Unit

                                            });

                                        }

                                    }



                                    if (srcSpace.Fixtures != null)

                                    {

                                        foreach (var f in srcSpace.Fixtures)

                                        {

                                            var newFixture = new GMK360.Core.Entities.SpaceFixture

                                            {

                                                Category = f.Category,

                                                ItemName = f.ItemName,

                                                Quantity = f.Quantity,

                                                Unit = f.Unit,

                                                StandardBrand = f.StandardBrand,

                                                StandardModel = f.StandardModel,

                                                SupplierId = f.SupplierId,

                                                SupplierName = f.SupplierName,

                                                TechnicalServiceId = f.TechnicalServiceId,

                                                TechnicalServiceName = f.TechnicalServiceName,

                                                MaintenanceNotes = f.MaintenanceNotes,

                                                EstimatedPrice = f.EstimatedPrice,

                                                TotalPrice = f.TotalPrice,

                                                IsCustomizable = f.IsCustomizable,

                                                WarrantyExpiryDate = f.WarrantyExpiryDate,

                                                NextMaintenanceDate = f.NextMaintenanceDate,

                                                Options = new List<GMK360.Core.Entities.MaterialOption>()

                                            };



                                            if (f.Options != null)

                                            {

                                                foreach (var opt in f.Options)

                                                {

                                                    newFixture.Options.Add(new GMK360.Core.Entities.MaterialOption

                                                    {

                                                        OptionName = opt.OptionName,

                                                        PriceDifference = opt.PriceDifference,

                                                        ImageUrl = opt.ImageUrl,

                                                        IsStandard = opt.IsStandard

                                                    });

                                                }

                                            }



                                            newSpace.Fixtures.Add(newFixture);

                                        }

                                    }



                                    _context.UnitSpaces.Add(newSpace);

                                }

                            }

                        }

                    }



                    // Bağlantıları ve mahalleri kaydet

                    await _context.SaveChangesAsync();

                }

            }



            return RedirectToAction(nameof(ManageBlock), new { id = building.ParentBuildingId ?? buildingId });



        }







        



        [HttpPost]





        

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteBuildingUnit(int unitId)

        {

            var unit = await _context.BuildingUnits.Include(u => u.Building).FirstOrDefaultAsync(u => u.Id == unitId);

            if (unit == null) return NotFound();



            var buildingId = unit.BuildingId;

            var redirectId = unit.Building.ParentBuildingId ?? unit.BuildingId;



            _context.BuildingUnits.Remove(unit);

            await _context.SaveChangesAsync();



            return RedirectToAction(nameof(ManageBlock), new { id = redirectId });

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> EditBuildingUnit(int unitId, string DoorNumber, string OwnerName, string UnitStructure, double? GrossSquareMeters, string FacadeDirection, decimal? ListPrice, string ListingType, string SalesStatus, string AssignedAgent, string SalesStage, string? Category = null, int? ParentUnitId = null, string? OwnerUserId = null)

        {

            var unit = await _context.BuildingUnits.Include(u => u.Building).FirstOrDefaultAsync(u => u.Id == unitId);

            if (unit == null) return NotFound();



            unit.DoorNumber = string.IsNullOrWhiteSpace(DoorNumber) ? unit.DoorNumber : DoorNumber;

            

            // Mülk Sahibi Eşleştirmesi

            if (!string.IsNullOrWhiteSpace(OwnerUserId) && OwnerUserId != "__CUSTOM__")

            {

                unit.OwnerUserId = OwnerUserId;

                var ownerUser = await _userManager.FindByIdAsync(OwnerUserId);

                if (ownerUser != null)

                {

                    unit.OwnerName = $"{ownerUser.FirstName} {ownerUser.LastName}".Trim();

                    unit.OwnerPhone = ownerUser.PhoneNumber;

                    unit.OwnerEmail = ownerUser.Email;

                }

            }

            else

            {

                unit.OwnerUserId = null;

                unit.OwnerName = string.IsNullOrWhiteSpace(OwnerName) ? null : OwnerName;

                if (string.IsNullOrWhiteSpace(unit.OwnerName))

                {

                    unit.OwnerPhone = null;

                    unit.OwnerEmail = null;

                }

            }

            

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

            unit.ListPrice = ListPrice;

            unit.ListingType = string.IsNullOrWhiteSpace(ListingType) ? null : ListingType;

            unit.SalesStatus = string.IsNullOrWhiteSpace(SalesStatus) ? null : SalesStatus;

            unit.AssignedAgent = string.IsNullOrWhiteSpace(AssignedAgent) ? null : AssignedAgent;

            unit.SalesStage = string.IsNullOrWhiteSpace(SalesStage) ? null : SalesStage;



            if (!string.IsNullOrWhiteSpace(Category))

            {

                unit.Category = Category;

            }

            if (ParentUnitId.HasValue)

            {

                unit.ParentUnitId = ParentUnitId.Value > 0 ? ParentUnitId.Value : null;

            }



            await _context.SaveChangesAsync();



            return RedirectToAction(nameof(ManageBlock), new { id = unit.Building.ParentBuildingId ?? unit.BuildingId });

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> BulkUpdateUnits(int buildingId, string unitIds, string? FacadeDirection, double? GrossSquareMeters, double? NetSquareMeters, string? RoomLayout, string? Category, string? ListingType)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            if (string.IsNullOrWhiteSpace(unitIds))

            {

                TempData["ErrorMessage"] = "Lütfen düzenlenecek en az bir daire/alan seçin.";

                return RedirectToAction(nameof(ManageBlock), new { id = buildingId });

            }



            var idList = unitIds.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)

                                .Select(s => int.TryParse(s.Trim(), out int id) ? id : 0)

                                .Where(id => id > 0)

                                .ToList();



            var units = await _context.BuildingUnits

                .Include(u => u.Building)

                .Where(u => idList.Contains(u.Id))

                .ToListAsync();



            foreach (var unit in units)

            {

                if (!string.IsNullOrWhiteSpace(FacadeDirection)) unit.FacadeDirection = FacadeDirection;

                if (GrossSquareMeters.HasValue && GrossSquareMeters.Value > 0) unit.GrossSquareMeters = GrossSquareMeters.Value;

                if (NetSquareMeters.HasValue && NetSquareMeters.Value > 0) unit.NetSquareMeters = NetSquareMeters.Value;

                if (!string.IsNullOrWhiteSpace(RoomLayout)) unit.RoomLayout = RoomLayout;

                if (!string.IsNullOrWhiteSpace(Category)) unit.Category = Category;

                if (!string.IsNullOrWhiteSpace(ListingType)) unit.ListingType = ListingType;

            }



            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{units.Count} adet bağımsız bölüm başarıyla güncellendi.";

            return RedirectToAction(nameof(ManageBlock), new { id = buildingId });

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> BulkAssignOwnership(int buildingId, List<int> unitIds, string assignmentType, string? stakeholderUserId, string? customOwnerName)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            if (unitIds == null || !unitIds.Any())

            {

                TempData["ErrorMessage"] = "Lütfen sahiplendirilecek daireleri seçin.";

                return RedirectToAction(nameof(ManageBlock), new { id = buildingId });

            }



            var units = await _context.BuildingUnits

                .Where(u => unitIds.Contains(u.Id))

                .ToListAsync();



            if (assignmentType == "Contractor")

            {

                foreach (var u in units)

                {

                    u.OwnerUserId = null;

                    u.OwnerName = null;

                    u.OwnerPhone = null;

                    u.ListingType = "Satılık";

                    u.SalesStatus = "Müsait";

                }

            }

            else if (assignmentType == "Stakeholder")

            {

                string? ownerName = customOwnerName;

                string? phone = null;



                if (!string.IsNullOrEmpty(stakeholderUserId))

                {

                    var user = await _userManager.FindByIdAsync(stakeholderUserId);

                    if (user != null)

                    {

                        ownerName = $"{user.FirstName} {user.LastName}".Trim();

                        phone = user.PhoneNumber;

                    }

                }



                foreach (var u in units)

                {

                    u.OwnerUserId = stakeholderUserId;

                    u.OwnerName = ownerName;

                    u.OwnerPhone = phone;

                    u.SalesStatus = "Hak Sahibi";

                    u.ListingType = null;

                }

            }



            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{units.Count} adet bağımsız bölümün mülkiyet ve hak sahipliği başarıyla güncellendi.";

            return RedirectToAction(nameof(ManageBlock), new { id = buildingId });

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> AutoPopulateBuildingFromDxf(int buildingId)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            var building = await _context.Buildings

                .Include(b => b.ConstructionProject)

                .Include(b => b.Units)

                .FirstOrDefaultAsync(b => b.Id == buildingId);



            if (building == null || (!User.IsInRole("Admin") && building.ConstructionProject.AgencyId != agencyId))

                return NotFound();



            int totalFloors = building.TotalFloors ?? 15;

            int basementFloors = building.BasementFloors;



            // 1. Bodrum Kat Alanları (-2, -1 vb.)

            for (int b = basementFloors; b >= 1; b--)

            {

                int floorKot = -b;

                var existingBasementUnits = building.Units.Where(u => u.FloorLevel == floorKot).ToList();

                if (!existingBasementUnits.Any())

                {

                    if (b == 2)

                    {

                        _context.BuildingUnits.Add(new GMK360.Core.Entities.BuildingUnit {

                            BuildingId = buildingId, FloorLevel = floorKot, FloorName = $"-{b}. Bodrum",

                            DoorNumber = "Kapalı Otopark", RoomLayout = "Kapalı Otopark (30 Araç)", Category = "OrtakAlan", FacadeDirection = "-", GrossSquareMeters = 420

                        });

                        _context.BuildingUnits.Add(new GMK360.Core.Entities.BuildingUnit {

                            BuildingId = buildingId, FloorLevel = floorKot, FloorName = $"-{b}. Bodrum",

                            DoorNumber = "Su Deposu & Yangın Pompası", RoomLayout = "Su Deposu & Yangın Pompası", Category = "OrtakAlan", FacadeDirection = "-", GrossSquareMeters = 60

                        });

                    }

                    else

                    {

                        _context.BuildingUnits.Add(new GMK360.Core.Entities.BuildingUnit {

                            BuildingId = buildingId, FloorLevel = floorKot, FloorName = $"-{b}. Bodrum",

                            DoorNumber = "Sığınak", RoomLayout = "Sığınak & Güvenlik Alanı", Category = "OrtakAlan", FacadeDirection = "-", GrossSquareMeters = 180

                        });

                        _context.BuildingUnits.Add(new GMK360.Core.Entities.BuildingUnit {

                            BuildingId = buildingId, FloorLevel = floorKot, FloorName = $"-{b}. Bodrum",

                            DoorNumber = "Jeneratör & Elektrik Pano Odası", RoomLayout = "Jeneratör & Pano Odası", Category = "OrtakAlan", FacadeDirection = "-", GrossSquareMeters = 50

                        });

                    }

                }

            }



            // 2. Zemin Kat (0)

            if (building.HasGroundFloor)

            {

                var existingGroundUnits = building.Units.Where(u => u.FloorLevel == 0).ToList();

                if (!existingGroundUnits.Any())

                {

                    _context.BuildingUnits.Add(new GMK360.Core.Entities.BuildingUnit {

                        BuildingId = buildingId, FloorLevel = 0, FloorName = "Zemin Kat",

                        DoorNumber = "Dükkan 1", RoomLayout = "Dükkan / Ticari", Category = "Dükkan", FacadeDirection = "Güney-Doğu", GrossSquareMeters = 180, NetSquareMeters = 160, ListingType = "Satılık", SalesStatus = "Müsait"

                    });

                    _context.BuildingUnits.Add(new GMK360.Core.Entities.BuildingUnit {

                        BuildingId = buildingId, FloorLevel = 0, FloorName = "Zemin Kat",

                        DoorNumber = "Dükkan 2", RoomLayout = "Dükkan / Ticari", Category = "Dükkan", FacadeDirection = "Güney-Batı", GrossSquareMeters = 140, NetSquareMeters = 125, ListingType = "Satılık", SalesStatus = "Müsait"

                    });

                    _context.BuildingUnits.Add(new GMK360.Core.Entities.BuildingUnit {

                        BuildingId = buildingId, FloorLevel = 0, FloorName = "Zemin Kat",

                        DoorNumber = "Bina Giriş Holü", RoomLayout = "Bina Giriş Holü & Güvenlik", Category = "OrtakAlan", FacadeDirection = "Kuzey", GrossSquareMeters = 70

                    });

                }

            }



            // 3. Normal Katlar (1 to totalFloors)

            int runningDoorNo = 1;

            for (int f = 1; f <= totalFloors; f++)

            {

                var floorUnits = building.Units.Where(u => u.FloorLevel == f && (u.Category == "Daire" || string.IsNullOrEmpty(u.Category) || u.Category == "Mesken")).OrderBy(u => u.Id).ToList();



                if (floorUnits.Count == 0)

                {

                    // 2 Daire oluştur (Tek ve Çift)

                    string d1 = runningDoorNo.ToString();

                    runningDoorNo++;

                    string d2 = runningDoorNo.ToString();

                    runningDoorNo++;



                    var u1 = new GMK360.Core.Entities.BuildingUnit {

                        BuildingId = buildingId, FloorLevel = f, FloorName = $"{f}. Kat",

                        DoorNumber = d1, RoomLayout = "3+1", Category = "Daire", FacadeDirection = "Kuzey-Doğu",

                        GrossSquareMeters = 135, NetSquareMeters = 112, ListingType = "Satılık", SalesStatus = "Müsait"

                    };

                    var u2 = new GMK360.Core.Entities.BuildingUnit {

                        BuildingId = buildingId, FloorLevel = f, FloorName = $"{f}. Kat",

                        DoorNumber = d2, RoomLayout = "2+1", Category = "Daire", FacadeDirection = "Güney-Batı",

                        GrossSquareMeters = 95, NetSquareMeters = 80, ListingType = "Satılık", SalesStatus = "Müsait"

                    };



                    _context.BuildingUnits.AddRange(u1, u2);

                }

                else

                {

                    // Mevcut olanları güncelle

                    foreach (var u in floorUnits)

                    {

                        if (u.DoorNumber == "-" || string.IsNullOrWhiteSpace(u.DoorNumber))

                        {

                            u.DoorNumber = runningDoorNo.ToString();

                            runningDoorNo++;

                        }

                        if (string.IsNullOrWhiteSpace(u.RoomLayout) || u.RoomLayout == "Mesken")

                        {

                            if (int.TryParse(u.DoorNumber, out int num) && num % 2 != 0)

                            {

                                u.RoomLayout = "3+1";

                                u.GrossSquareMeters = 135;

                                u.NetSquareMeters = 112;

                                u.FacadeDirection = "Kuzey-Doğu";

                            }

                            else

                            {

                                u.RoomLayout = "2+1";

                                u.GrossSquareMeters = 95;

                                u.NetSquareMeters = 80;

                                u.FacadeDirection = "Güney-Batı";

                            }

                        }

                    }

                }

            }



            // 4. Çatı Katı (Eğer HasRoof ise)

            if (building.HasRoof)

            {

                int roofFloor = totalFloors + 1;

                var existingRoof = building.Units.Where(u => u.FloorLevel == roofFloor).ToList();

                if (!existingRoof.Any())

                {

                    _context.BuildingUnits.Add(new GMK360.Core.Entities.BuildingUnit {

                        BuildingId = buildingId, FloorLevel = roofFloor, FloorName = "Çatı Katı",

                        DoorNumber = (runningDoorNo++).ToString(), RoomLayout = "4+1 Dubleks", Category = "Daire", FacadeDirection = "Kuzey-Doğu",

                        GrossSquareMeters = 210, NetSquareMeters = 175, ListingType = "Satılık", SalesStatus = "Müsait"

                    });

                    _context.BuildingUnits.Add(new GMK360.Core.Entities.BuildingUnit {

                        BuildingId = buildingId, FloorLevel = roofFloor, FloorName = "Çatı Katı",

                        DoorNumber = (runningDoorNo++).ToString(), RoomLayout = "4+1 Dubleks", Category = "Daire", FacadeDirection = "Güney-Batı",

                        GrossSquareMeters = 210, NetSquareMeters = 175, ListingType = "Satılık", SalesStatus = "Müsait"

                    });

                }

            }



            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "⚡ Mimari DWG / DXF iskeletine göre katlar, dükkanlar, daireler (3+1, 2+1, m², cephe) ve ortak alanlar başarıyla binaya yerleştirildi!";

            return RedirectToAction(nameof(ManageBlock), new { id = buildingId });

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> UploadFloorPlan(int buildingId, int floorLevel, IFormFile? planFile)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            var building = await _context.Buildings

                .Include(b => b.ConstructionProject)

                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (building == null) return NotFound();



            if (planFile != null && planFile.Length > 0)

            {

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"Agency_{agencyId}", $"Project_{building.ConstructionProjectId}", "floorplans");

                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);



                var ext = Path.GetExtension(planFile.FileName).ToLowerInvariant();

                var uniqueName = $"floorplan_b{buildingId}_f{floorLevel}_{Guid.NewGuid().ToString("N").Substring(0, 8)}{ext}";

                var filePath = Path.Combine(uploadsFolder, uniqueName);



                using (var stream = new FileStream(filePath, FileMode.Create))

                {

                    await planFile.CopyToAsync(stream);

                }



                var docUrl = $"/uploads/Agency_{agencyId}/Project_{building.ConstructionProjectId}/floorplans/{uniqueName}";

                string floorEntityType = "BuildingFloor_" + buildingId;



                var existingDoc = await _context.DmsDocuments

                    .FirstOrDefaultAsync(d => d.EntityType == floorEntityType && d.EntityId == floorLevel);



                if (existingDoc != null)

                {

                    existingDoc.DocumentUrl = docUrl;

                    existingDoc.FileExtension = ext;

                    existingDoc.FileSizeBytes = planFile.Length;

                    existingDoc.UploadDate = DateTime.UtcNow;

                    _context.Update(existingDoc);

                }

                else

                {

                    var dmsDoc = new GMK360.Core.Entities.DmsDocument

                    {

                        EntityType = floorEntityType,

                        EntityId = floorLevel,

                        Title = $"{building.BlockName} - Kat {floorLevel} Kat Planı",

                        DocumentUrl = docUrl,

                        FileExtension = ext,

                        FileSizeBytes = planFile.Length,

                        UploadDate = DateTime.UtcNow,

                        UploadedByUserId = _userManager.GetUserId(User) ?? ""

                    };

                    _context.DmsDocuments.Add(dmsDoc);

                }



                var archive = new GMK360.Core.Entities.DocumentArchive

                {

                    AgencyId = agencyId.Value,

                    ProjectId = building.ConstructionProjectId,

                    Title = $"{building.BlockName} - Kat {floorLevel} Kat Planı / Çizimi",

                    Category = "Çizimler",

                    SourceModule = "Construction",

                    DocumentUrl = docUrl,

                    FileName = uniqueName,

                    FileExtension = ext,

                    Year = DateTime.Now.Year,

                    UploadDate = DateTime.UtcNow,

                    Status = "Tamamlandı"

                };

                _context.DocumentArchives.Add(archive);



                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"{building.BlockName} - Kat {floorLevel} için plan başarıyla yüklendi.";

            }



            return RedirectToAction(nameof(ManageBlock), new { id = building.ParentBuildingId ?? building.Id });

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> CloneUnitSpaces(int sourceUnitId, List<int> targetUnitIds, bool overwrite = true)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Json(new { success = false, message = "Yetkisiz işlem." });



            if (targetUnitIds == null || !targetUnitIds.Any())

            {

                return Json(new { success = false, message = "Lütfen en az bir hedef daire seçin." });

            }



            var sourceUnit = await _context.BuildingUnits

                .Include(u => u.Building).ThenInclude(b => b.ConstructionProject)

                .Include(u => u.Spaces).ThenInclude(s => s.Measurements)

                .Include(u => u.Spaces).ThenInclude(s => s.Fixtures).ThenInclude(f => f.Options)

                .FirstOrDefaultAsync(u => u.Id == sourceUnitId);



            if (sourceUnit == null)

            {

                return Json(new { success = false, message = "Kaynak daire bulunamadı." });

            }



            if (!User.IsInRole("Admin") && sourceUnit.Building?.ConstructionProject?.AgencyId != agencyId)

            {

                return Json(new { success = false, message = "Bu projede yetkiniz yok." });

            }



            var targetUnits = await _context.BuildingUnits

                .Include(u => u.Spaces).ThenInclude(s => s.Measurements)

                .Include(u => u.Spaces).ThenInclude(s => s.Fixtures).ThenInclude(f => f.Options)

                .Where(u => targetUnitIds.Contains(u.Id))

                .ToListAsync();



            int clonedUnitsCount = 0;

            int clonedSpacesCount = 0;



            foreach (var target in targetUnits)

            {

                if (target.Id == sourceUnit.Id) continue;



                if (overwrite && target.Spaces != null && target.Spaces.Any())

                {

                    foreach (var sp in target.Spaces.ToList())

                    {

                        if (sp.Measurements != null && sp.Measurements.Any())

                        {

                            _context.RemoveRange(sp.Measurements);

                        }

                        if (sp.Fixtures != null && sp.Fixtures.Any())

                        {

                            foreach (var fix in sp.Fixtures)

                            {

                                if (fix.Options != null && fix.Options.Any())

                                {

                                    _context.RemoveRange(fix.Options);

                                }

                            }

                            _context.RemoveRange(sp.Fixtures);

                        }

                        _context.Remove(sp);

                    }

                }



                foreach (var srcSpace in sourceUnit.Spaces)

                {

                    var newSpace = new GMK360.Core.Entities.UnitSpace

                    {

                        BuildingUnitId = target.Id,

                        Name = srcSpace.Name,

                        Type = srcSpace.Type,

                        SquareMeters = srcSpace.SquareMeters,

                        PlanImagePath = srcSpace.PlanImagePath,

                        Description = srcSpace.Description,

                        Measurements = new List<GMK360.Core.Entities.SpaceMeasurement>(),

                        Fixtures = new List<GMK360.Core.Entities.SpaceFixture>()

                    };



                    if (srcSpace.Measurements != null)

                    {

                        foreach (var m in srcSpace.Measurements)

                        {

                            newSpace.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement

                            {

                                Category = m.Category,

                                Description = m.Description,

                                Width = m.Width,

                                Length = m.Length,

                                Height = m.Height,

                                Quantity = m.Quantity,

                                Unit = m.Unit

                            });

                        }

                    }



                    if (srcSpace.Fixtures != null)

                    {

                        foreach (var f in srcSpace.Fixtures)

                        {

                            var newFixture = new GMK360.Core.Entities.SpaceFixture

                            {

                                Category = f.Category,

                                ItemName = f.ItemName,

                                Quantity = f.Quantity,

                                Unit = f.Unit,

                                StandardBrand = f.StandardBrand,

                                StandardModel = f.StandardModel,

                                SupplierId = f.SupplierId,

                                SupplierName = f.SupplierName,

                                TechnicalServiceId = f.TechnicalServiceId,

                                TechnicalServiceName = f.TechnicalServiceName,

                                MaintenanceNotes = f.MaintenanceNotes,

                                EstimatedPrice = f.EstimatedPrice,

                                TotalPrice = f.TotalPrice,

                                IsCustomizable = f.IsCustomizable,

                                WarrantyExpiryDate = f.WarrantyExpiryDate,

                                NextMaintenanceDate = f.NextMaintenanceDate,

                                Options = new List<GMK360.Core.Entities.MaterialOption>()

                            };



                            if (f.Options != null)

                            {

                                foreach (var opt in f.Options)

                                {

                                    newFixture.Options.Add(new GMK360.Core.Entities.MaterialOption

                                    {

                                        OptionName = opt.OptionName,

                                        PriceDifference = opt.PriceDifference,

                                        ImageUrl = opt.ImageUrl,

                                        IsStandard = opt.IsStandard

                                    });

                                }

                            }



                            newSpace.Fixtures.Add(newFixture);

                        }

                    }



                    _context.Add(newSpace);

                    clonedSpacesCount++;

                }



                clonedUnitsCount++;

            }



            await _context.SaveChangesAsync();



            return Json(new { 

                success = true, 

                message = $"{clonedUnitsCount} adet hedef daireye toplam {clonedSpacesCount} mahal ve tüm ölçü/demirbaşlar başarıyla aktarıldı!" 

            });

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
                    .ThenInclude(s => s.Measurements)
                .Include(u => u.Spaces)
                    .ThenInclude(s => s.Fixtures)
                        .ThenInclude(f => f.Options)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unit == null) return NotFound();

            if (!User.IsInRole("Admin") && unit.Building.ConstructionProject.AgencyId != agencyId)
            {
                return Unauthorized();
            }

            var projectUnits = await _context.BuildingUnits
                .Include(u => u.Building)
                .Where(u => u.Building.ConstructionProjectId == unit.Building.ConstructionProjectId && u.Id != id)
                .OrderBy(u => u.Building.BlockName)
                .ThenBy(u => u.FloorLevel)
                .ThenBy(u => u.DoorNumber)
                .ToListAsync();

            ViewBag.ProjectUnits = projectUnits;
            ViewBag.ProjectId = unit.Building.ConstructionProjectId;

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



                    height = m.Height,
                    selectedMaterialOption = m.SelectedMaterialOption,
                    priceDifference = m.PriceDifference



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
        public async Task<IActionResult> PopulateDefaultUnitSpaces(int unitId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var unit = await _context.BuildingUnits
                .Include(u => u.Building)
                    .ThenInclude(b => b.ConstructionProject)
                .Include(u => u.Spaces)
                    .ThenInclude(s => s.Measurements)
                .Include(u => u.Spaces)
                    .ThenInclude(s => s.Fixtures)
                .FirstOrDefaultAsync(u => u.Id == unitId);

            if (unit == null || (!User.IsInRole("Admin") && unit.Building.ConstructionProject.AgencyId != agencyId))
                return NotFound();

            await PopulateUnitSpacesInternal(unitId, unit.RoomLayout ?? "3+1", unit.Category ?? "Daire");
            TempData["SuccessMessage"] = $"Tip plan standardına ({unit.RoomLayout}) göre mahaller ve detaylı metrajlar başarıyla oluşturuldu.";
            return RedirectToAction(nameof(ManageUnitSpaces), new { id = unitId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
                public async Task<IActionResult> BulkPopulateBuildingSpaces(int buildingId, bool overwrite = false)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var building = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .Include(b => b.Units)
                    .ThenInclude(u => u.Spaces)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (building == null || (!User.IsInRole("Admin") && building.ConstructionProject.AgencyId != agencyId))
                return NotFound();

            int populatedCount = 0;
            foreach (var u in building.Units)
            {
                if (overwrite || !u.Spaces.Any())
                {
                    await PopulateUnitSpacesInternal(u.Id, u.RoomLayout ?? "3+1", u.Category ?? "Daire");
                    populatedCount++;
                }
            }

            TempData["SuccessMessage"] = $"{populatedCount} adet bağımsız bölümün iç mekanları, detaylı metrajları ve demirbaş pasaportları tip planlara göre başarıyla oluşturuldu.";
            return RedirectToAction(nameof(ManageBlock), new { id = buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFloorFireStation(int buildingId, int floorLevel, string? extinguisherBrand, string? extinguisherModel, string? lastRefillDate, string? nextInspectionDate, string? serviceName, string? servicePhone, bool hasHoseCabinet = true, bool hasSmokeDetector = true)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var building = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .Include(b => b.Units)
                    .ThenInclude(u => u.Spaces)
                        .ThenInclude(s => s.Fixtures)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (building == null || (!User.IsInRole("Admin") && building.ConstructionProject.AgencyId != agencyId))
                return NotFound();

            var floorCommonUnit = building.Units.FirstOrDefault(u => u.FloorLevel == floorLevel && u.Category == "OrtakAlan" && u.DoorNumber.Contains("Yangın"));
            if (floorCommonUnit == null)
            {
                floorCommonUnit = new GMK360.Core.Entities.BuildingUnit
                {
                    BuildingId = buildingId,
                    FloorLevel = floorLevel,
                    FloorName = floorLevel == 0 ? "Zemin Kat" : (floorLevel < 0 ? $"{-floorLevel}. Bodrum" : $"{floorLevel}. Kat"),
                    DoorNumber = "Kat Yangın İstasyonu",
                    RoomLayout = "Yangın Güvenlik İstasyonu",
                    Category = "OrtakAlan",
                    FacadeDirection = "-",
                    IsAvailableForDailyRent = false,
                    IsEmpty = true
                };
                _context.BuildingUnits.Add(floorCommonUnit);
                await _context.SaveChangesAsync();
            }

            var fireSpace = floorCommonUnit.Spaces.FirstOrDefault(s => s.Name.Contains("Yangın"));
            if (fireSpace == null)
            {
                fireSpace = new GMK360.Core.Entities.UnitSpace
                {
                    BuildingUnitId = floorCommonUnit.Id,
                    Name = "Kat Yangın Güvenlik İstasyonu",
                    Type = "Tesisat",
                    SquareMeters = 2,
                    Description = "Kat holü yangın dolabı, 6kg yangın tüpü ve adresli ihbar istasyonu"
                };
                _context.UnitSpaces.Add(fireSpace);
                await _context.SaveChangesAsync();
            }

            var extFixture = fireSpace.Fixtures.FirstOrDefault(f => f.Category == "Yangın Güvenliği");
            if (extFixture == null)
            {
                extFixture = new GMK360.Core.Entities.SpaceFixture
                {
                    UnitSpaceId = fireSpace.Id,
                    Category = "Yangın Güvenliği",
                    ItemName = "6 kg KKT Yangın Söndürme Cihazı",
                    Quantity = 1,
                    Unit = "Adet"
                };
                _context.SpaceFixtures.Add(extFixture);
            }

            extFixture.StandardBrand = string.IsNullOrWhiteSpace(extinguisherBrand) ? "Yangınsan TSE" : extinguisherBrand;
            extFixture.StandardModel = string.IsNullOrWhiteSpace(extinguisherModel) ? "6 kg ABC Kuru Kimyevi Toz" : extinguisherModel;
            extFixture.TechnicalServiceName = string.IsNullOrWhiteSpace(serviceName) ? "Yetkili Yangın Güvenlik A.Ş." : serviceName;

            string notes = $"Yangın Hortum Dolabı: {(hasHoseCabinet ? "TS EN 671-1 20mt Makara (Mevcut)" : "Eksik")}, Duman Dedektörü: {(hasSmokeDetector ? "Adresli / Aktif" : "Yok")}";
            if (!string.IsNullOrWhiteSpace(servicePhone)) notes += $", Servis Tel: {servicePhone}";
            extFixture.MaintenanceNotes = notes;

            if (DateTime.TryParse(lastRefillDate, out DateTime lDate)) extFixture.WarrantyExpiryDate = lDate;
            if (DateTime.TryParse(nextInspectionDate, out DateTime nDate)) extFixture.NextMaintenanceDate = nDate;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{(floorLevel == 0 ? "Zemin Kat" : $"{floorLevel}. Kat")} Yangın Güvenlik Pasaportu başarıyla güncellendi.";
            return RedirectToAction(nameof(ManageBlock), new { id = buildingId });
        }



        private async Task PopulateUnitSpacesInternal(int unitId, string layout, string cat)
        {
            var existingSpaces = await _context.UnitSpaces
                .Include(s => s.Measurements)
                .Include(s => s.Fixtures)
                .Where(s => s.BuildingUnitId == unitId)
                .ToListAsync();

            if (existingSpaces.Any())
            {
                foreach (var sp in existingSpaces)
                {
                    _context.SpaceMeasurements.RemoveRange(sp.Measurements);
                    _context.SpaceFixtures.RemoveRange(sp.Fixtures);
                }
                _context.UnitSpaces.RemoveRange(existingSpaces);
                await _context.SaveChangesAsync();
            }

            var spacesToAdd = new List<GMK360.Core.Entities.UnitSpace>();

            if (cat == "Dükkan" || cat == "Ticari")
            {
                var sp1 = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Ana Satış Salonu", Type = "Ticari", SquareMeters = 140, Description = "Geniş vitrinli ana müşteri karşılama ve reyon alanı" };
                sp1.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "60x120 Rektifiye Granit Seramik", Quantity = 140, Unit = "m2" });
                sp1.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Süpürgelik", Description = "Granit Süpürgelik 10cm", Quantity = 52, Unit = "mt" });
                sp1.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Duvar", Description = "Alçı Sıva & Silinebilir Su Bazlı Boya", Quantity = 180, Unit = "m2" });
                sp1.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Tavan", Description = "Modüler Asma Tavan & LED Panel Aydınlatma", Quantity = 140, Unit = "m2" });
                sp1.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Yangın & Güvenlik", ItemName = "6 kg ABC KKT Yangın Söndürme Tüpü", Quantity = 2, Unit = "Adet",
                    StandardBrand = "Yangınsan TSE", StandardModel = "6 kg ABC Sürekli Basınçlı", SupplierName = "Yıldız Yangın Güvenlik Ltd.",
                    TechnicalServiceName = "Yıldız Yangın Servisi 0216 444 0 911", WarrantyExpiryDate = DateTime.Now.AddYears(4),
                    NextMaintenanceDate = DateTime.Now.AddYears(1), MaintenanceNotes = "Yıllık periyodik hidrostatik basınç ve vana kontrolü zorunludur.", IsCustomizable = false
                });

                var sp2 = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Depo & Mal Kabul", Type = "Depo", SquareMeters = 30, Description = "Arka servis kapılı stok ve depolama alanı" };
                sp2.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Epoksi Zemin Kaplama / Endüstriyel Şap", Quantity = 30, Unit = "m2" });

                var sp3 = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Personel WC & Mutfak", Type = "Islak Hacim", SquareMeters = 10, Description = "Personel lavabo ve çay ocağı alanı" };
                sp3.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Kaymaz Seramik", Quantity = 10, Unit = "m2" });
                sp3.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Tavan", Description = "60x60 Klipin Modüler Asma Tavan", Quantity = 10, Unit = "m2" });

                spacesToAdd.AddRange(new[] { sp1, sp2, sp3 });
            }
            else if (layout.Contains("Dubleks") || layout.Contains("4+1"))
            {
                // Dubleks / 4+1 Daire
                var salon = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Salon", Type = "Oda", SquareMeters = 38, Description = "Ferah panoramik salon" };
                salon.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Derzli Lamine Parke 10mm", Quantity = 38, Unit = "m2" });
                salon.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Süpürgelik", Description = "Lake Ahşap Süpürgelik 10cm", Quantity = 25, Unit = "mt" });
                salon.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Duvar", Description = "Alçı Sıva & İpeksi Mat Boya", Quantity = 78, Unit = "m2" });
                salon.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Yangın & Güvenlik", ItemName = "Optik Duman Dedektörü (Adresli)", Quantity = 1, Unit = "Adet",
                    StandardBrand = "Finder / Mavigard", StandardModel = "FDL-800 Adresli", TechnicalServiceName = "Yangın İhbar Santral Servisi 0216 444 0 911",
                    NextMaintenanceDate = DateTime.Now.AddYears(1), MaintenanceNotes = "Yılda bir kalibrasyon ve test butonu testi yapılmalıdır.", IsCustomizable = false
                });

                var mutfak = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Mutfak", Type = "Mutfak", SquareMeters = 16, Description = "Ada tezgahlı ankastre mutfak" };
                mutfak.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Granit Seramik 60x120", Quantity = 16, Unit = "m2" });
                mutfak.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Duvar", Description = "Tezgah Arası Porselen Plaka", Quantity = 4.8, Unit = "m2" });
                mutfak.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Isıtma & Mekanik", ItemName = "Yoğuşmalı Kombi 28 kW", Quantity = 1, Unit = "Adet",
                    StandardBrand = "Buderus", StandardModel = "Logamax Plus GB122i 28 kW", SupplierName = "Buderus Isıtma Sistemleri Bayi",
                    TechnicalServiceName = "Buderus Yetkili Servisi 444 5 474", WarrantyExpiryDate = DateTime.Now.AddYears(3),
                    NextMaintenanceDate = DateTime.Now.AddYears(1), MaintenanceNotes = "Yıllık periyodik servis bakımı zorunludur. Doğalgaz sızdırmazlık onaylıdır.", IsCustomizable = true
                });
                mutfak.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Yangın & Güvenlik", ItemName = "Doğalgaz / Karbonmonoksit Kaçak Dedektörü", Quantity = 1, Unit = "Adet",
                    StandardBrand = "Mavi Alarm", StandardModel = "GAS-3000 Selenoid Vana Entegre", TechnicalServiceName = "Bölge Gaz Dağıtım Yetkili Servisi",
                    WarrantyExpiryDate = DateTime.Now.AddYears(3), NextMaintenanceDate = DateTime.Now.AddYears(1),
                    MaintenanceNotes = "Gaz kaçağı anında selenoid vanayı otomatik kapatır. Yıllık dedektör sensör testi yapılmalıdır.", IsCustomizable = false
                });
                mutfak.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Mutfak Ankastre", ItemName = "Ankastre 3'lü Set (Ocak-Fırın-Davlumbaz)", Quantity = 1, Unit = "Set",
                    StandardBrand = "Siemens", StandardModel = "iQ300 Paslanmaz Çelik Set", SupplierName = "Siemens Beyaz Eşya Bayi",
                    TechnicalServiceName = "Siemens Müşteri Hizmetleri 444 6 688", WarrantyExpiryDate = DateTime.Now.AddYears(3),
                    MaintenanceNotes = "Yetkili servis tarafından montaj yapılmıştır. Garanti belgesi teslim tutanağına eklidir.", IsCustomizable = true
                });
                mutfak.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Yangın & Güvenlik", ItemName = "Yangın Battaniyesi 120x180 cm", Quantity = 1, Unit = "Adet",
                    StandardBrand = "Yangınline", StandardModel = "TSE EN 1869 Cam Elyaf", MaintenanceNotes = "Mutfak dolabı yanı acil askı aparatı.", IsCustomizable = false
                });

                var ebYatak = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Ebeveyn Yatak Odası & Süit", Type = "Oda", SquareMeters = 24, Description = "Özel banyo ve giyinme bölümlü süit" };
                ebYatak.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Derzli Lamine Parke", Quantity = 24, Unit = "m2" });

                var ebBanyo = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Ebeveyn Banyo", Type = "Islak Hacim", SquareMeters = 5, Description = "Duşakabinli ebeveyn banyosu" };
                ebBanyo.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Kaymaz Porselen Seramik", Quantity = 5, Unit = "m2" });
                ebBanyo.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Duvar", Description = "Seramik Duvar Kaplama", Quantity = 24, Unit = "m2" });
                ebBanyo.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Tavan", Description = "60x60 Klipin Modüler Asma Tavan", Quantity = 5, Unit = "m2" });
                ebBanyo.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Vitrifiye & Mobilya", ItemName = "Ebeveyn Hilton Banyo Dolabı 80cm", Quantity = 1, Unit = "Takım",
                    StandardBrand = "VitrA", StandardModel = "S20 Banyo Mobilyası", TechnicalServiceName = "VitrA Müşteri Hizmetleri 0850 311 70 70",
                    WarrantyExpiryDate = DateTime.Now.AddYears(5), IsCustomizable = true
                });

                var teras = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Panoramik Çatı Terası", Type = "Balkon", SquareMeters = 35, Description = "Geniş manzaralı açık teras alanı" };
                teras.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Dış Mekan Kaymaz Porselen & Su Yalıtımı", Quantity = 35, Unit = "m2" });

                var genelBanyo = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Genel Banyo & WC", Type = "Islak Hacim", SquareMeters = 6, Description = "Hilton dolaplı ana banyo" };
                genelBanyo.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "1. Sınıf Seramik", Quantity = 6, Unit = "m2" });
                genelBanyo.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Duvar", Description = "Porselen Seramik Duvar", Quantity = 28, Unit = "m2" });
                genelBanyo.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Tavan", Description = "60x60 Klipin Modüler Asma Tavan", Quantity = 6, Unit = "m2" });
                genelBanyo.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Vitrifiye & Mobilya", ItemName = "Hilton Banyo Dolabı 100cm", Quantity = 1, Unit = "Takım",
                    StandardBrand = "VitrA", StandardModel = "S50 Membran Gövde", TechnicalServiceName = "VitrA Müşteri Hizmetleri 0850 311 70 70",
                    WarrantyExpiryDate = DateTime.Now.AddYears(5), IsCustomizable = true
                });
                genelBanyo.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Vitrifiye & Mekanik", ItemName = "Gömme Rezervuar & Asma Klozet", Quantity = 1, Unit = "Set",
                    StandardBrand = "Geberit / VitrA", StandardModel = "Geberit Duofix & Rim-ex", TechnicalServiceName = "Geberit Teknik Destek 0216 340 80 80",
                    WarrantyExpiryDate = DateTime.Now.AddYears(10), IsCustomizable = true
                });

                var antre = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Antre & Giriş", Type = "Dolaşım", SquareMeters = 12, Description = "Giriş holü ve vestiyer nişi" };
                antre.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Granit Seramik 60x120", Quantity = 12, Unit = "m2" });
                antre.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Kapı & Güvenlik", ItemName = "Merkezi Kilitli Çelik Kapı", Quantity = 1, Unit = "Adet",
                    StandardBrand = "Kale Çelik Kapı", StandardModel = "14 Noktadan Merkezi Kilit", TechnicalServiceName = "Kale Çelik Eşya 444 0 243",
                    WarrantyExpiryDate = DateTime.Now.AddYears(5), MaintenanceNotes = "Kilit barelleri ve acil master anahtarlar tutanakla teslim edilmiştir.", IsCustomizable = false
                });
                antre.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Zayıf Akım & Diafon", ItemName = "Görüntülü Diafon 7 Inç Dokunmatik", Quantity = 1, Unit = "Adet",
                    StandardBrand = "Audio", StandardModel = "Bus Plus 7 Inç Dokunmatik", TechnicalServiceName = "Audio Elektronik Yetkili Servis 0216 528 88 88",
                    WarrantyExpiryDate = DateTime.Now.AddYears(3), MaintenanceNotes = "Bina girişi, güvenlik kamerası ve yangın santrali entegre.", IsCustomizable = false
                });

                spacesToAdd.AddRange(new[] { salon, mutfak, ebYatak, ebBanyo, teras, genelBanyo, antre });
            }
            else if (layout == "2+1")
            {
                // 2+1 Daire
                var salon = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Salon", Type = "Oda", SquareMeters = 26, Description = "Lamine parke, alçı sıva boya" };
                salon.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Derzli Lamine Parke 8mm", Quantity = 26, Unit = "m2" });
                salon.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Süpürgelik", Description = "Lake Ahşap Süpürgelik 8cm", Quantity = 21, Unit = "mt" });
                salon.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Duvar", Description = "Alçı Sıva & Su Bazlı Boya", Quantity = 56, Unit = "m2" });
                salon.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Yangın & Güvenlik", ItemName = "Optik Duman Dedektörü (Adresli)", Quantity = 1, Unit = "Adet",
                    StandardBrand = "Finder / Mavigard", StandardModel = "FDL-800 Adresli", TechnicalServiceName = "Yangın İhbar Santral Servisi 0216 444 0 911",
                    NextMaintenanceDate = DateTime.Now.AddYears(1), MaintenanceNotes = "Yılda bir kalibrasyon ve test butonu testi yapılmalıdır.", IsCustomizable = false
                });

                var mutfak = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Mutfak", Type = "Mutfak", SquareMeters = 11, Description = "Ankastre uyumlu mutfak" };
                mutfak.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Granit Seramik", Quantity = 11, Unit = "m2" });
                mutfak.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Duvar", Description = "Tezgah Arası Seramik", Quantity = 3.6, Unit = "m2" });
                mutfak.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Isıtma & Mekanik", ItemName = "Yoğuşmalı Kombi 24 kW", Quantity = 1, Unit = "Adet",
                    StandardBrand = "Buderus", StandardModel = "Logamax Plus GB122i 24 kW", SupplierName = "Buderus Isıtma Sistemleri Bayi",
                    TechnicalServiceName = "Buderus Yetkili Servisi 444 5 474", WarrantyExpiryDate = DateTime.Now.AddYears(3),
                    NextMaintenanceDate = DateTime.Now.AddYears(1), MaintenanceNotes = "Yıllık periyodik servis bakımı zorunludur. Doğalgaz sızdırmazlık onaylıdır.", IsCustomizable = true
                });
                mutfak.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Yangın & Güvenlik", ItemName = "Doğalgaz / Karbonmonoksit Kaçak Dedektörü", Quantity = 1, Unit = "Adet",
                    StandardBrand = "Mavi Alarm", StandardModel = "GAS-3000 Selenoid Vana Entegre", TechnicalServiceName = "Bölge Gaz Dağıtım Yetkili Servisi",
                    WarrantyExpiryDate = DateTime.Now.AddYears(3), NextMaintenanceDate = DateTime.Now.AddYears(1),
                    MaintenanceNotes = "Gaz kaçağı anında selenoid vanayı otomatik kapatır. Yıllık dedektör sensör testi yapılmalıdır.", IsCustomizable = false
                });
                mutfak.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Mutfak Ankastre", ItemName = "Ankastre 3'lü Set (Ocak-Fırın-Davlumbaz)", Quantity = 1, Unit = "Set",
                    StandardBrand = "Siemens", StandardModel = "iQ300 Paslanmaz Çelik Set", SupplierName = "Siemens Beyaz Eşya Bayi",
                    TechnicalServiceName = "Siemens Müşteri Hizmetleri 444 6 688", WarrantyExpiryDate = DateTime.Now.AddYears(3),
                    MaintenanceNotes = "Yetkili servis tarafından montaj yapılmıştır. Garanti belgesi teslim tutanağına eklidir.", IsCustomizable = true
                });
                mutfak.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Yangın & Güvenlik", ItemName = "Yangın Battaniyesi 120x180 cm", Quantity = 1, Unit = "Adet",
                    StandardBrand = "Yangınline", StandardModel = "TSE EN 1869 Cam Elyaf", MaintenanceNotes = "Mutfak dolabı yanı acil askı aparatı.", IsCustomizable = false
                });

                var yatak = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Yatak Odası", Type = "Oda", SquareMeters = 16, Description = "Geniş yatak odası" };
                yatak.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Lamine Parke", Quantity = 16, Unit = "m2" });
                yatak.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Süpürgelik", Description = "Lake Süpürgelik", Quantity = 16, Unit = "mt" });

                var cocuk = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Çocuk Odası", Type = "Oda", SquareMeters = 11, Description = "Çocuk / genç odası" };
                cocuk.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Lamine Parke", Quantity = 11, Unit = "m2" });
                cocuk.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Süpürgelik", Description = "Lake Süpürgelik", Quantity = 13.5, Unit = "mt" });

                var banyo = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Genel Banyo & WC", Type = "Islak Hacim", SquareMeters = 5, Description = "Duşakabin ve banyo dolabı" };
                banyo.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Kaymaz Banyo Seramiği", Quantity = 5, Unit = "m2" });
                banyo.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Duvar", Description = "Seramik Duvar Kaplama", Quantity = 25, Unit = "m2" });
                banyo.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Tavan", Description = "60x60 Klipin Modüler Asma Tavan", Quantity = 5, Unit = "m2" });
                banyo.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Vitrifiye & Banyo Mobilyası", ItemName = "Hilton Banyo Dolabı 85cm", Quantity = 1, Unit = "Takım",
                    StandardBrand = "VitrA", StandardModel = "S50 Banyo Mobilyası Takımı", TechnicalServiceName = "VitrA Müşteri Hizmetleri 0850 311 70 70",
                    WarrantyExpiryDate = DateTime.Now.AddYears(5), MaintenanceNotes = "Aynalı üst dolap, seramik lavabo ve alt çekmece ünitesi", IsCustomizable = true
                });
                banyo.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Vitrifiye & Mekanik", ItemName = "Gömme Rezervuar & Asma Klozet", Quantity = 1, Unit = "Set",
                    StandardBrand = "Geberit / VitrA", StandardModel = "Geberit Duofix & Rim-ex", TechnicalServiceName = "Geberit Destek 0216 340 80 80",
                    WarrantyExpiryDate = DateTime.Now.AddYears(10), IsCustomizable = true
                });
                banyo.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Armatür & Batarya", ItemName = "Termostatik Duş Kolonu & Lavabo Bataryası", Quantity = 1, Unit = "Set",
                    StandardBrand = "E.C.A.", StandardModel = "Niobe Serisi Krom", TechnicalServiceName = "E.C.A. Servis 444 0 322",
                    WarrantyExpiryDate = DateTime.Now.AddYears(5), IsCustomizable = true
                });

                var hol = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Antre & Hol", Type = "Dolaşım", SquareMeters = 9, Description = "Giriş holü ve vestiyer nişi" };
                hol.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Granit Seramik", Quantity = 9, Unit = "m2" });
                hol.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Kapı & Güvenlik", ItemName = "Merkezi Kilitli Çelik Kapı", Quantity = 1, Unit = "Adet",
                    StandardBrand = "Kale Çelik Kapı", StandardModel = "14 Noktadan Merkezi Kilit", TechnicalServiceName = "Kale Çelik Eşya 444 0 243",
                    WarrantyExpiryDate = DateTime.Now.AddYears(5), MaintenanceNotes = "Kilit barelleri ve acil master anahtarlar tutanakla teslim edilmiştir.", IsCustomizable = false
                });
                hol.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Zayıf Akım & Diafon", ItemName = "Görüntülü Diafon 7 Inç Dokunmatik", Quantity = 1, Unit = "Adet",
                    StandardBrand = "Audio", StandardModel = "Bus Plus 7 Inç Dokunmatik", TechnicalServiceName = "Audio Elektronik Yetkili Servis 0216 528 88 88",
                    WarrantyExpiryDate = DateTime.Now.AddYears(3), MaintenanceNotes = "Bina girişi, güvenlik kamerası ve yangın santrali entegre.", IsCustomizable = false
                });

                var balkon = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Balkon", Type = "Balkon", SquareMeters = 6, Description = "Korkuluklu balkon" };
                balkon.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Kaymaz Seramik", Quantity = 6, Unit = "m2" });

                spacesToAdd.AddRange(new[] { salon, mutfak, yatak, cocuk, banyo, hol, balkon });
            }
            else
            {
                // Standart 3+1 Daire
                var salon = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Salon", Type = "Oda", SquareMeters = 32, Description = "Lamine parke zemin, alçıpan asma tavan" };
                salon.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Lamine Parke Kaplama 10mm", Quantity = 32, Unit = "m2" });
                salon.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Süpürgelik", Description = "Lake Ahşap Süpürgelik 8cm", Quantity = 23.2, Unit = "mt" });
                salon.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Duvar", Description = "Alçı Sıva & Su Bazlı Boya", Quantity = 64.9, Unit = "m2" });
                salon.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Yangın & Güvenlik", ItemName = "Optik Duman Dedektörü (Adresli)", Quantity = 1, Unit = "Adet",
                    StandardBrand = "Finder / Mavigard", StandardModel = "FDL-800 Adresli", TechnicalServiceName = "Yangın İhbar Santral Servisi 0216 444 0 911",
                    NextMaintenanceDate = DateTime.Now.AddYears(1), MaintenanceNotes = "Yılda bir kalibrasyon ve test butonu testi yapılmalıdır.", IsCustomizable = false
                });

                var mutfak = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Mutfak", Type = "Mutfak", SquareMeters = 14, Description = "Granit seramik, ankastre mutfak ve kombi nişi" };
                mutfak.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Granit Seramik 60x120", Quantity = 14, Unit = "m2" });
                mutfak.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Süpürgelik", Description = "Granit Süpürgelik", Quantity = 15, Unit = "mt" });
                mutfak.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Duvar", Description = "Tezgah Arası Seramik", Quantity = 4.2, Unit = "m2" });

                mutfak.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Isıtma & Mekanik", ItemName = "Yoğuşmalı Kombi 24 kW", Quantity = 1, Unit = "Adet",
                    StandardBrand = "Buderus", StandardModel = "Logamax Plus GB122i 24 kW", SupplierName = "Buderus Isıtma Sistemleri Bayi",
                    TechnicalServiceName = "Buderus Yetkili Servisi 444 5 474", WarrantyExpiryDate = DateTime.Now.AddYears(3),
                    NextMaintenanceDate = DateTime.Now.AddYears(1), MaintenanceNotes = "Yıllık periyodik servis bakımı zorunludur. Doğalgaz sızdırmazlık onaylıdır.", IsCustomizable = true
                });
                mutfak.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Yangın & Güvenlik", ItemName = "Doğalgaz / Karbonmonoksit Kaçak Dedektörü", Quantity = 1, Unit = "Adet",
                    StandardBrand = "Mavi Alarm", StandardModel = "GAS-3000 Selenoid Vana Entegre", TechnicalServiceName = "Bölge Gaz Dağıtım Yetkili Servisi",
                    WarrantyExpiryDate = DateTime.Now.AddYears(3), NextMaintenanceDate = DateTime.Now.AddYears(1),
                    MaintenanceNotes = "Gaz kaçağı anında selenoid vanayı otomatik kapatır. Yıllık dedektör sensör testi yapılmalıdır.", IsCustomizable = false
                });
                mutfak.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Mutfak Ankastre", ItemName = "Ankastre 3'lü Set (Ocak-Fırın-Davlumbaz)", Quantity = 1, Unit = "Set",
                    StandardBrand = "Siemens", StandardModel = "iQ300 Paslanmaz Çelik Set", SupplierName = "Siemens Beyaz Eşya Bayi",
                    TechnicalServiceName = "Siemens Müşteri Hizmetleri 444 6 688", WarrantyExpiryDate = DateTime.Now.AddYears(3),
                    MaintenanceNotes = "Yetkili servis tarafından montaj yapılmıştır. Garanti belgesi teslim tutanağına eklidir.", IsCustomizable = true
                });
                mutfak.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Yangın & Güvenlik", ItemName = "Yangın Battaniyesi 120x180 cm", Quantity = 1, Unit = "Adet",
                    StandardBrand = "Yangınline", StandardModel = "TSE EN 1869 Cam Elyaf", MaintenanceNotes = "Mutfak dolabı yanı acil askı aparatı.", IsCustomizable = false
                });

                var ebYatak = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Ebeveyn Yatak Odası", Type = "Oda", SquareMeters = 18, Description = "Lamine parke, ebeveyn banyo geçişi" };
                ebYatak.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Lamine Parke Kaplama", Quantity = 18, Unit = "m2" });
                ebYatak.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Süpürgelik", Description = "Lake Süpürgelik 8cm", Quantity = 17.5, Unit = "mt" });

                var ebBanyo = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Ebeveyn Banyo", Type = "Islak Hacim", SquareMeters = 4, Description = "Klipin modüler asma tavan, seramik" };
                ebBanyo.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Kaymaz Porselen Seramik", Quantity = 4, Unit = "m2" });
                ebBanyo.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Duvar", Description = "Seramik Duvar Kaplama", Quantity = 22, Unit = "m2" });
                ebBanyo.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Tavan", Description = "60x60 Klipin Modüler Asma Tavan", Quantity = 4, Unit = "m2" });
                ebBanyo.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Vitrifiye & Banyo Mobilyası", ItemName = "Ebeveyn Hilton Dolabı 80cm", Quantity = 1, Unit = "Takım",
                    StandardBrand = "VitrA", StandardModel = "S20 Serisi Aynalı Banyo Dolabı", TechnicalServiceName = "VitrA Müşteri Hizmetleri 0850 311 70 70",
                    WarrantyExpiryDate = DateTime.Now.AddYears(5), IsCustomizable = true
                });
                ebBanyo.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Vitrifiye & Duş", ItemName = "Temperli Cam Duşakabin 90x90", Quantity = 1, Unit = "Adet",
                    StandardBrand = "Hüppe", StandardModel = "Aura 6mm Şeffaf Temperli", TechnicalServiceName = "Hüppe Montaj & Servis",
                    WarrantyExpiryDate = DateTime.Now.AddYears(5), IsCustomizable = true
                });

                var cocuk = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Çocuk Odası", Type = "Oda", SquareMeters = 12, Description = "Lamine parke, su bazlı boya" };
                cocuk.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Lamine Parke", Quantity = 12, Unit = "m2" });
                cocuk.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Süpürgelik", Description = "Lake Süpürgelik", Quantity = 14, Unit = "mt" });

                var misafir = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Misafir Odası", Type = "Oda", SquareMeters = 11, Description = "Lamine parke, alçı sıva boya" };
                misafir.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Lamine Parke", Quantity = 11, Unit = "m2" });
                misafir.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Süpürgelik", Description = "Lake Süpürgelik", Quantity = 13.5, Unit = "mt" });

                var genelBanyo = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Genel Banyo", Type = "Islak Hacim", SquareMeters = 6, Description = "Klipin modüler asma tavan, seramik zemin/duvar" };
                genelBanyo.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Kaymaz Banyo Seramiği", Quantity = 6, Unit = "m2" });
                genelBanyo.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Duvar", Description = "Seramik Duvar Kaplama", Quantity = 28, Unit = "m2" });
                genelBanyo.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Tavan", Description = "60x60 Klipin Modüler Asma Tavan", Quantity = 6, Unit = "m2" });
                
                var banyoHilton = new GMK360.Core.Entities.SpaceFixture {
                    Category = "Vitrifiye & Banyo Mobilyası", ItemName = "Hilton Banyo Dolabı 100cm", Quantity = 1, Unit = "Takım",
                    StandardBrand = "VitrA", StandardModel = "S50 Banyo Mobilyası Takımı",
                    TechnicalServiceName = "VitrA Yetkili Servisi 0850 311 70 70", WarrantyExpiryDate = DateTime.Now.AddYears(5),
                    MaintenanceNotes = "Aynalı üst dolap, seramik lavabo ve alt çekmece ünitesi", IsCustomizable = true
                };
                genelBanyo.Fixtures.Add(banyoHilton);
                genelBanyo.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Vitrifiye & Mekanik", ItemName = "Gömme Rezervuar & Asma Klozet", Quantity = 1, Unit = "Set",
                    StandardBrand = "Geberit / VitrA", StandardModel = "Geberit Duofix & Rim-ex", TechnicalServiceName = "Geberit Destek 0216 340 80 80",
                    WarrantyExpiryDate = DateTime.Now.AddYears(10), IsCustomizable = true
                });
                genelBanyo.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Armatür & Batarya", ItemName = "Termostatik Duş Kolonu & Lavabo Bataryası", Quantity = 1, Unit = "Set",
                    StandardBrand = "E.C.A.", StandardModel = "Niobe Serisi Krom", TechnicalServiceName = "E.C.A. Servis 444 0 322",
                    WarrantyExpiryDate = DateTime.Now.AddYears(5), IsCustomizable = true
                });

                var antre = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Antre & Hol", Type = "Dolaşım", SquareMeters = 12, Description = "Seramik zemin, vestiyer nişi" };
                antre.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Granit Seramik 60x120", Quantity = 12, Unit = "m2" });
                antre.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Süpürgelik", Description = "Lake Ahşap Süpürgelik", Quantity = 14, Unit = "mt" });
                antre.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Kapı & Güvenlik", ItemName = "Merkezi Kilitli Çelik Kapı", Quantity = 1, Unit = "Adet",
                    StandardBrand = "Kale Çelik Kapı", StandardModel = "14 Noktadan Merkezi Kilit", TechnicalServiceName = "Kale Çelik Eşya 444 0 243",
                    WarrantyExpiryDate = DateTime.Now.AddYears(5), MaintenanceNotes = "Kilit barelleri ve acil master anahtarlar tutanakla teslim edilmiştir.", IsCustomizable = false
                });
                antre.Fixtures.Add(new GMK360.Core.Entities.SpaceFixture {
                    Category = "Zayıf Akım & Diafon", ItemName = "Görüntülü Diafon 7 Inç Dokunmatik", Quantity = 1, Unit = "Adet",
                    StandardBrand = "Audio", StandardModel = "Bus Plus 7 Inç Dokunmatik", TechnicalServiceName = "Audio Elektronik Yetkili Servis 0216 528 88 88",
                    WarrantyExpiryDate = DateTime.Now.AddYears(3), MaintenanceNotes = "Bina girişi, güvenlik kamerası ve yangın santrali entegre.", IsCustomizable = false
                });

                var balkon = new GMK360.Core.Entities.UnitSpace { BuildingUnitId = unitId, Name = "Balkon", Type = "Balkon", SquareMeters = 8, Description = "Kaymaz seramik zemin, alüminyum korkuluk" };
                balkon.Measurements.Add(new GMK360.Core.Entities.SpaceMeasurement { Category = "Zemin", Description = "Kaymaz Dış Cephe Seramiği", Quantity = 8, Unit = "m2" });

                spacesToAdd.AddRange(new[] { salon, mutfak, ebYatak, ebBanyo, cocuk, misafir, genelBanyo, antre, balkon });
            }

            _context.UnitSpaces.AddRange(spacesToAdd);
            await _context.SaveChangesAsync();
        }



        public async Task<IActionResult> DeleteSpaceFixture(int id)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            var fixture = await _context.SpaceFixtures

                .Include(f => f.Options)

                .FirstOrDefaultAsync(f => f.Id == id);



            if (fixture == null) return NotFound();



            if (fixture.Options != null && fixture.Options.Any())

            {

                _context.RemoveRange(fixture.Options);

            }



            _context.SpaceFixtures.Remove(fixture);

            await _context.SaveChangesAsync();



            return Ok(new { success = true });

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

                .Include(f => f.Options)

                .Where(f => f.UnitSpaceId == spaceId)

                .Select(f => new {

                    id = f.Id,

                    itemName = f.ItemName,

                    category = f.Category,

                    quantity = f.Quantity,

                    unit = f.Unit,

                    standardBrand = f.StandardBrand,

                    standardModel = f.StandardModel,

                    supplierName = f.SupplierId != null ? f.Supplier.BusinessName : f.SupplierName,

                    technicalServiceName = f.TechnicalServiceId != null ? f.TechnicalService.BusinessName : f.TechnicalServiceName,

                    supplierId = f.SupplierId,

                    technicalServiceId = f.TechnicalServiceId,

                    warrantyExpiryDate = f.WarrantyExpiryDate.HasValue ? f.WarrantyExpiryDate.Value.ToString("dd.MM.yyyy") : null,

                    nextMaintenanceDate = f.NextMaintenanceDate.HasValue ? f.NextMaintenanceDate.Value.ToString("dd.MM.yyyy") : null,

                    maintenanceNotes = f.MaintenanceNotes,

                    estimatedPrice = f.EstimatedPrice,

                    totalPrice = f.TotalPrice,

                    isCustomizable = f.IsCustomizable,

                    options = f.Options.Select(o => new {

                        id = o.Id,

                        optionName = o.OptionName,

                        priceDifference = o.PriceDifference,

                        imageUrl = o.ImageUrl,

                        isStandard = o.IsStandard

                    }).ToList()

                })

                .ToListAsync();



            return Json(fixtures);

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> AddSpaceFixture(

            int spaceId, 

            string itemName, 

            string category, 

            double quantity, 

            string unit, 

            string supplierName, 

            string technicalServiceName, 

            string maintenanceNotes, 

            bool isCustomizable = false, 

            string? standardBrand = null, 

            string? standardModel = null, 

            decimal? estimatedPrice = null, 

            decimal? totalPrice = null,

            DateTime? warrantyExpiryDate = null,

            DateTime? nextMaintenanceDate = null)

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



            var fixture = new GMK360.Core.Entities.SpaceFixture

            {

                UnitSpaceId = spaceId,

                ItemName = itemName,

                Category = category,

                Quantity = quantity,

                Unit = unit,

                StandardBrand = standardBrand,

                StandardModel = standardModel,

                EstimatedPrice = estimatedPrice,

                TotalPrice = totalPrice,

                SupplierName = supplierName,

                TechnicalServiceName = technicalServiceName,

                MaintenanceNotes = maintenanceNotes,

                WarrantyExpiryDate = warrantyExpiryDate,

                NextMaintenanceDate = nextMaintenanceDate,

                IsCustomizable = isCustomizable

            };



            _context.SpaceFixtures.Add(fixture);

            await _context.SaveChangesAsync();



            return RedirectToAction(nameof(ManageUnitSpaces), new { id = space.BuildingUnitId });

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> AddCommonFixturesTemplate(int spaceId, string templateType)

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



            var fixturesToAdd = new List<GMK360.Core.Entities.SpaceFixture>();



            if (templateType == "Su Deposu" || templateType.Contains("Su Deposu"))

            {

                fixturesToAdd.Add(new GMK360.Core.Entities.SpaceFixture

                {

                    UnitSpaceId = spaceId,

                    ItemName = "Paslanmaz Çelik Modüler Su Deposu",

                    Category = "Tesisat",

                    Quantity = 1,

                    Unit = "Adet",

                    StandardBrand = "Standart",

                    MaintenanceNotes = "Su Deposu Odası - Yıllık klorlama ve temizlik",

                    IsCustomizable = false

                });

                fixturesToAdd.Add(new GMK360.Core.Entities.SpaceFixture

                {

                    UnitSpaceId = spaceId,

                    ItemName = "Frekans Kontrollü Çift Pompalı Hidrofor Grubu",

                    Category = "Tesisat",

                    Quantity = 1,

                    Unit = "Set",

                    StandardBrand = "Wilo / Grundfos",

                    MaintenanceNotes = "Hidrofor Kaidesi - 6 Aylık membran ve basınç kontrolü",

                    IsCustomizable = false

                });

                fixturesToAdd.Add(new GMK360.Core.Entities.SpaceFixture

                {

                    UnitSpaceId = spaceId,

                    ItemName = "Otomatik Klorlama ve Dozaj Ünitesi",

                    Category = "Tesisat",

                    Quantity = 1,

                    Unit = "Adet",

                    MaintenanceNotes = "Dozaj Hattı - Aylık klor seviye kontrolü",

                    IsCustomizable = false

                });

                fixturesToAdd.Add(new GMK360.Core.Entities.SpaceFixture

                {

                    UnitSpaceId = spaceId,

                    ItemName = "Otomatik Şamandıralı Dalgıç Drenaj Pompası",

                    Category = "Tesisat",

                    Quantity = 1,

                    Unit = "Adet",

                    MaintenanceNotes = "Drenaj Çukuru - 6 Aylık flatör kontrolü",

                    IsCustomizable = false

                });

            }

            else if (templateType == "Sığınak" || templateType.Contains("Sığınak"))

            {

                fixturesToAdd.Add(new GMK360.Core.Entities.SpaceFixture

                {

                    UnitSpaceId = spaceId,

                    ItemName = "Mekanik Sığınak Havalandırma Cihazı & Radyoaktif Filtre",

                    Category = "Tesisat",

                    Quantity = 1,

                    Unit = "Adet",

                    MaintenanceNotes = "Sığınak Havalandırma Şaftı - Yıllık filtre testi",

                    IsCustomizable = false

                });

                fixturesToAdd.Add(new GMK360.Core.Entities.SpaceFixture

                {

                    UnitSpaceId = spaceId,

                    ItemName = "Kuru Kimyevi Tozlu Yangın Söndürme Tüpü (6 kg ABC)",

                    Category = "Diğer",

                    Quantity = 2,

                    Unit = "Adet",

                    MaintenanceNotes = "Sığınak Duvar Askısı - Yıllık dolum kontrolü",

                    IsCustomizable = false

                });

                fixturesToAdd.Add(new GMK360.Core.Entities.SpaceFixture

                {

                    UnitSpaceId = spaceId,

                    ItemName = "Acil Kaçış Yönlendirme ve Aydınlatma Kiti (Akü Destekli)",

                    Category = "Elektrik",

                    Quantity = 2,

                    Unit = "Adet",

                    MaintenanceNotes = "Çıkış Kapısı Üzeri - 6 Aylık akü testi",

                    IsCustomizable = false

                });

                fixturesToAdd.Add(new GMK360.Core.Entities.SpaceFixture

                {

                    UnitSpaceId = spaceId,

                    ItemName = "Acil Durum İlk Yardım Dolabı ve Katlanır Sedye",

                    Category = "Diğer",

                    Quantity = 1,

                    Unit = "Set",

                    MaintenanceNotes = "Sığınak Duvarı - 6 Aylık malzeme miat kontrolü",

                    IsCustomizable = false

                });

            }

            else if (templateType == "Jeneratör" || templateType.Contains("Jeneratör"))

            {

                fixturesToAdd.Add(new GMK360.Core.Entities.SpaceFixture

                {

                    UnitSpaceId = spaceId,

                    ItemName = "Otomatik Kabinli Dizel Jeneratör Grubu",

                    Category = "Elektrik",

                    Quantity = 1,

                    Unit = "Adet",

                    StandardBrand = "Aksa / Teksan / Çukurova",

                    MaintenanceNotes = "Jeneratör Kaidesi - 6 Aylık yağ ve filtre bakımı",

                    IsCustomizable = false

                });

                fixturesToAdd.Add(new GMK360.Core.Entities.SpaceFixture

                {

                    UnitSpaceId = spaceId,

                    ItemName = "Otomatik Transfer Panosu (ATS / Şebeke-Jeneratör)",

                    Category = "Elektrik",

                    Quantity = 1,

                    Unit = "Adet",

                    MaintenanceNotes = "Ana Dağıtım Panosu Yanı - Yıllık kontaktör kontrolü",

                    IsCustomizable = false

                });

                fixturesToAdd.Add(new GMK360.Core.Entities.SpaceFixture

                {

                    UnitSpaceId = spaceId,

                    ItemName = "Kritik Susturuculu Egzoz Hattı ve Titreşim Kompansatörü",

                    Category = "Tesisat",

                    Quantity = 1,

                    Unit = "Hat",

                    MaintenanceNotes = "Egzoz Şaftı",

                    IsCustomizable = false

                });

            }

            else if (templateType == "Kazan Dairesi" || templateType.Contains("Kazan"))

            {

                fixturesToAdd.Add(new GMK360.Core.Entities.SpaceFixture

                {

                    UnitSpaceId = spaceId,

                    ItemName = "Kaskad Doğalgazlı Yoğuşmalı Kazan",

                    Category = "Tesisat",

                    Quantity = 2,

                    Unit = "Adet",

                    StandardBrand = "Viessmann / Buderus / Vaillant",

                    MaintenanceNotes = "Kazan Kaidesi - Yıllık brülör ve baca temizliği",

                    IsCustomizable = false

                });

                fixturesToAdd.Add(new GMK360.Core.Entities.SpaceFixture

                {

                    UnitSpaceId = spaceId,

                    ItemName = "Frekans Kontrollü Islak Rotorlu Sirkülasyon Pompaları",

                    Category = "Tesisat",

                    Quantity = 2,

                    Unit = "Adet",

                    MaintenanceNotes = "Isıtma Kollektörü Hattı - 6 Aylık sızdırmazlık kontrolü",

                    IsCustomizable = false

                });

                fixturesToAdd.Add(new GMK360.Core.Entities.SpaceFixture

                {

                    UnitSpaceId = spaceId,

                    ItemName = "Membranlı Kapalı Genleşme Deposu",

                    Category = "Tesisat",

                    Quantity = 1,

                    Unit = "Adet",

                    MaintenanceNotes = "Kazan Dairesi - 6 Aylık ön gaz basınç kontrolü",

                    IsCustomizable = false

                });

                fixturesToAdd.Add(new GMK360.Core.Entities.SpaceFixture

                {

                    UnitSpaceId = spaceId,

                    ItemName = "Gaz Kaçak Alarm Dedektörü ve Selenoid Emniyet Vanası",

                    Category = "Tesisat",

                    Quantity = 1,

                    Unit = "Set",

                    MaintenanceNotes = "Doğalgaz Giriş Hattı - 6 Aylık alarm testi",

                    IsCustomizable = false

                });

            }



            if (fixturesToAdd.Any())

            {

                _context.SpaceFixtures.AddRange(fixturesToAdd);

                await _context.SaveChangesAsync();

            }



            return Json(new { success = true, count = fixturesToAdd.Count });

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> AddFixtureOption(int fixtureId, string optionName, decimal? priceDifference, bool isStandard, IFormFile? optionImage)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            var fixture = await _context.SpaceFixtures

                .Include(f => f.UnitSpace)

                    .ThenInclude(s => s.BuildingUnit)

                        .ThenInclude(u => u.Building)

                            .ThenInclude(b => b.ConstructionProject)

                .FirstOrDefaultAsync(f => f.Id == fixtureId);



            if (fixture == null || (!User.IsInRole("Admin") && fixture.UnitSpace.BuildingUnit.Building.ConstructionProject.AgencyId != agencyId))

                return Json(new { success = false, message = "Yetkisiz işlem." });



            string? imageUrl = null;

            if (optionImage != null && optionImage.Length > 0)

            {

                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"Agency_{agencyId}", "options");

                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                var ext = Path.GetExtension(optionImage.FileName).ToLowerInvariant();

                var fileName = $"opt_{fixtureId}_{Guid.NewGuid().ToString("N").Substring(0, 8)}{ext}";

                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))

                {

                    await optionImage.CopyToAsync(stream);

                }

                imageUrl = $"/uploads/Agency_{agencyId}/options/{fileName}";

            }



            var option = new GMK360.Core.Entities.MaterialOption

            {

                SpaceFixtureId = fixtureId,

                OptionName = optionName,

                PriceDifference = priceDifference ?? 0,

                IsStandard = isStandard,

                ImageUrl = imageUrl

            };



            _context.MaterialOptions.Add(option);

            await _context.SaveChangesAsync();



            return Json(new { success = true, message = "Seçenek başarıyla eklendi!", optionId = option.Id });

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteFixtureOption(int optionId)

        {

            var agencyId = await GetUserAgencyIdAsync();

            if (agencyId == null) return Unauthorized();



            var option = await _context.MaterialOptions

                .Include(o => o.SpaceFixture)

                    .ThenInclude(f => f.UnitSpace)

                        .ThenInclude(s => s.BuildingUnit)

                            .ThenInclude(u => u.Building)

                                .ThenInclude(b => b.ConstructionProject)

                .FirstOrDefaultAsync(o => o.Id == optionId);



            if (option == null || (!User.IsInRole("Admin") && option.SpaceFixture.UnitSpace.BuildingUnit.Building.ConstructionProject.AgencyId != agencyId))

                return Json(new { success = false, message = "Yetkisiz işlem." });



            _context.MaterialOptions.Remove(option);

            await _context.SaveChangesAsync();



            return Json(new { success = true });

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



                string cleanPhone = (phone ?? "").Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Trim();

                if (!cleanPhone.StartsWith("+90") && !cleanPhone.StartsWith("0")) cleanPhone = "0" + cleanPhone;



                var existingUser = await _userManager.Users.FirstOrDefaultAsync(u => 

                    u.PhoneNumber == cleanPhone || u.PhoneNumber == phone || (!string.IsNullOrEmpty(email) && u.Email == email));



                if (existingUser != null)

                {

                    string existingFullName = $"{existingUser.FirstName} {existingUser.LastName}".Trim();

                    string inputFullName = $"{firstName} {lastName}".Trim();



                    if (!string.Equals(existingFullName, inputFullName, StringComparison.OrdinalIgnoreCase))

                    {

                        TempData["ErrorMessage"] = $"Girilen telefon numarası ({phone}), sistemde kayıtlı olan '{existingFullName}' kullanıcısına aittir! Yeni bir paydaş oluşturmak için lütfen paydaşa ait farklı bir telefon numarası giriniz.";

                        return RedirectToAction("Details", new { id = projectId });

                    }

                }

                else

                {

                    

                    string generatedUserName = string.IsNullOrEmpty(email) ? cleanPhone : email;

                    string generatedEmail = string.IsNullOrEmpty(email) ? $"{cleanPhone}@gmk360.local" : email;

                    

                    existingUser = new ApplicationUser

                    {

                        UserName = generatedUserName,

                        Email = generatedEmail,

                        FirstName = firstName,

                        LastName = lastName,

                        PhoneNumber = cleanPhone,

                        EmailConfirmed = false,

                        PhoneNumberConfirmed = false,

                        UserType = (role == 1) ? GMK360.Core.Entities.Identity.UserType.PropertyOwner : GMK360.Core.Entities.Identity.UserType.Individual

                    };



                    var result = await _userManager.CreateAsync(existingUser, "Shadow.User2026!");

                    if (!result.Succeeded)

                    {

                        string errs = string.Join(", ", result.Errors.Select(e => e.Description));

                        TempData["ErrorMessage"] = $"Gölge kullanıcı oluşturulurken hata oluştu: {errs}";

                        return RedirectToAction("Details", new { id = projectId });

                    }



                    // Gölge kullanıcı rol ataması

                    try

                    {

                        var roleManager = HttpContext.RequestServices.GetService<RoleManager<GMK360.Core.Entities.Identity.ApplicationRole>>();

                        if (roleManager != null)

                        {

                            if (!await roleManager.RoleExistsAsync("Customer"))

                            {

                                await roleManager.CreateAsync(new GMK360.Core.Entities.Identity.ApplicationRole { Name = "Customer" });

                            }

                        }



                        if (!await _userManager.IsInRoleAsync(existingUser, "Customer"))

                        {

                            await _userManager.AddToRoleAsync(existingUser, "Customer");

                        }

                    }

                    catch { /* Rol ataması ana süreci engellemez */ }

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

                        Notes = "Firma tarafından davet edildi (Gölge Onay Bekliyor)",

                        CreatedAt = DateTime.UtcNow,

                        IsDeleted = false

                    };

                    

                    _context.ProjectStakeholders.Add(stakeholder);

                    await _context.SaveChangesAsync();



                    try

                    {

                        var smsService = HttpContext.RequestServices.GetService<GMK360.Web.Services.Sms.ISmsService>();

                        if (smsService != null)

                        {

                            string smsBody = $"Sayın {firstName} {lastName}, GMK360 inşaat projesine paydaş olarak eklendiniz. Sisteme giriş bilgileriniz: Kullanıcı: {existingUser.UserName} Şifre: Shadow.User2026!";

                            await smsService.SmsGonderAsync(existingUser.PhoneNumber ?? phone, smsBody);

                        }

                    }

                    catch { /* SMS hatası ana süreci engellemez */ }

                    

                    TempData["SuccessMessage"] = $"{firstName} {lastName} projeye başarıyla eklendi! (Gölge Kullanıcı: {existingUser.UserName} / Şifre: Shadow.User2026!)";

                }

                else

                {

                    if (existingStakeholder.IsDeleted)

                    {

                        existingStakeholder.IsDeleted = false;

                        existingStakeholder.Role = (GMK360.Core.Entities.Construction.StakeholderRole)role;

                        existingStakeholder.SharePercentage = (role == 1) ? sharePercentage : null;

                        existingStakeholder.UpdatedAt = DateTime.UtcNow;

                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = $"{firstName} {lastName} projeye başarıyla yeniden dahil edildi!";

                    }

                    else

                    {

                        TempData["ErrorMessage"] = "Bu kişi zaten bu projenin paydaşı!";

                    }

                }



                return RedirectToAction("Details", new { id = projectId });

            }

            catch (Exception ex)

            {

                TempData["ErrorMessage"] = "Sistem Hatası: " + ex.Message;

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

                

                TempData["SuccessMessage"] = "Paydaş bilgileri başarıyla güncellendi.";

                return RedirectToAction("Details", new { id = projectId });

            }

            catch (System.Exception ex)

            {

                TempData["ErrorMessage"] = "Paydaş düzenlenirken hata oluştu: " + ex.Message;

                return projectId > 0 ? RedirectToAction("Details", new { id = projectId }) : RedirectToAction("Index");

            }

        }



        [HttpPost]

        public async Task<IActionResult> DeleteStakeholder(int stakeholderId)

        {

            int projectId = 0;

            try

            {

                var st = await _context.ProjectStakeholders.FirstOrDefaultAsync(s => s.Id == stakeholderId);

                if (st == null)

                {

                    TempData["ErrorMessage"] = "Paydaş bulunamadı.";

                    return RedirectToAction("Index");

                }

                

                projectId = st.ProjectId;

                st.IsDeleted = true;

                await _context.SaveChangesAsync();

                

                TempData["SuccessMessage"] = "Paydaş projeden başarıyla kaldırıldı.";

                return RedirectToAction("Details", new { id = projectId });

            }

            catch (System.Exception ex)

            {

                TempData["ErrorMessage"] = "Paydaş silinirken hata oluştu: " + ex.Message;

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



    

        [HttpGet]

        public async Task<IActionResult> Map(int id)

        {

            var project = await _context.ConstructionProjects.FindAsync(id);

            if (project == null) return NotFound();



            if (!string.IsNullOrEmpty(project.GoogleMapsUrl))

            {

                return Redirect(project.GoogleMapsUrl);

            }

            else if (project.Latitude.HasValue && project.Longitude.HasValue)

            {

                return Redirect($"https://www.google.com/maps/search/?api=1&query={project.Latitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)},{project.Longitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

            }



            // Fallback: If no location is set, just redirect to the details page with an alert

            TempData["ErrorMessage"] = "Bu proje için henüz bir konum/harita bilgisi girilmemiş.";

            return RedirectToAction("Details", new { id = id });

        }



        [HttpGet]

        public async Task<IActionResult> GetLocationJson(int id)

        {

            var project = await _context.ConstructionProjects.FindAsync(id);

            if (project == null) return NotFound();



            string mapsUrl = !string.IsNullOrEmpty(project.GoogleMapsUrl)

                ? project.GoogleMapsUrl

                : (project.Latitude.HasValue && project.Longitude.HasValue

                    ? $"https://www.google.com/maps/search/?api=1&query={project.Latitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)},{project.Longitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}"

                    : "");



            return Json(new

            {

                id = project.Id,

                name = project.Name,

                address = project.Address ?? "Adres belirtilmemiş",

                latitude = project.Latitude,

                longitude = project.Longitude,

                mapsUrl = mapsUrl

            });

        }



        [HttpGet("ConstructionProject/Equipment/{id}")]

        public async Task<IActionResult> Equipment(int id)

        {

            var project = await _context.ConstructionProjects.FindAsync(id);

            if (project == null) return NotFound();

            

            ViewData["ProjectId"] = id;

            ViewData["ProjectName"] = project.Name;

            

            var equipments = await _context.CompanyEquipments.ToListAsync();

            var logs = await _context.EquipmentUsageLogs

                                     .Include(x => x.CompanyEquipment)

                                     .Where(x => x.ConstructionProjectId == id)

                                     .OrderByDescending(x => x.CreatedAt)

                                     .ToListAsync();

                                     

            ViewData["Logs"] = logs;

            return View(equipments);

        }



        [HttpPost("ConstructionProject/AddEquipmentLog")]

        public async Task<IActionResult> AddEquipmentLog(int projectId, int equipmentId, string usageType, DateTime startDate, DateTime? endDate, TimeSpan? startTime, TimeSpan? endTime, int duration)

        {

            var project = await _context.ConstructionProjects.FindAsync(projectId);

            var equipment = await _context.CompanyEquipments.FindAsync(equipmentId);

            if (project == null || equipment == null) return NotFound();

            

            decimal totalCost = 0;

            if (usageType == "Saatlik")

                totalCost = equipment.UnitCost * duration;

            else if (usageType == "Günlük")

                totalCost = equipment.UnitCost * 8 * duration; // Assuming 8 hour workday

            else

                totalCost = equipment.UnitCost * duration;

                

            var log = new GMK360.Core.Entities.Construction.Workflows.EquipmentUsageLog

            {

                ConstructionProjectId = projectId,

                CompanyEquipmentId = equipmentId,

                StartDate = startDate,

                EndDate = endDate,

                StartTime = startTime,

                EndTime = endTime,

                UsageType = usageType,

                Duration = duration,

                TotalCost = totalCost,

                CreatedAt = DateTime.Now

            };

            

            _context.EquipmentUsageLogs.Add(log);

            await _context.SaveChangesAsync();

            

            TempData["SuccessMessage"] = "Kiralık araç talebi ve kullanım kaydı başarıyla eklendi!";

            return RedirectToAction("Equipment", new { id = projectId });

        }

    }



    public class CreateSubProjectDto

    {

        public int ParentProjectId { get; set; }

        public int? AmenityId { get; set; }

        public string AmenityName { get; set; } = string.Empty;

        public string? CustomProjectName { get; set; }

        public string? Category { get; set; } // "SosyalDonati", "DisSaha_Peyzaj", "OzelImalat", "Altyapi", "Asansor", "Mekanik_Elektrik"

        public string? ExecutionMode { get; set; } // "DxfEngineering" or "DocumentProcurement"

        public bool HasDxf { get; set; }

        public string? Description { get; set; }

        public decimal? EstimatedBudget { get; set; }

    }

}

