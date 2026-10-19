using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Construction;
using GMK360.Core.Entities.Construction.Workflows;
using GMK360.Core.Entities.B2B;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

using System.IO;
using Microsoft.AspNetCore.Http;
using GMK360.Web.Models;
using GMK360.Core.Entities;

namespace GMK360.Web.Controllers
{
    public class MetrajController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GMK360.Web.Services.Construction.DnaTimeOffsetService _dnaTimeOffsetService;

        public MetrajController(ApplicationDbContext context, GMK360.Web.Services.Construction.DnaTimeOffsetService dnaTimeOffsetService)
        {
            _context = context;
            _dnaTimeOffsetService = dnaTimeOffsetService;
        }

        public IActionResult Index(int projectId, int? recipeId)
        {
            return RedirectToAction(nameof(DrawingArchive), new { projectId = projectId });
        }

        // =================================================================================
        // KANAT 1: DİJİTAL ÇİZİM ARŞİVİ & İLK KABUL MASASI
        // =================================================================================
        public async Task<IActionResult> DrawingArchive(int? projectId, int? id)
        {
            int pId = projectId ?? id ?? 0;
            if (pId <= 0) return RedirectToAction("Index", "ConstructionProject");

            ViewData["ProjectId"] = pId;

            var project = await _context.ConstructionProjects
                .Include(p => p.Blocks).ThenInclude(b => b.ChildBuildings).ThenInclude(c => c.Units)
                .Include(p => p.Blocks).ThenInclude(b => b.Units)
                .FirstOrDefaultAsync(p => p.Id == pId);

            if (project == null) return NotFound();

            ViewData["ProjectName"] = project.Name;
            ViewBag.Project = project;

            // Tüm binaları/blokları topla (Ana baza/blok ve alt kuleler)
            var allBuildings = new List<Building>();
            var parentBlocks = project.Blocks.Where(b => b.ParentBuildingId == null).ToList();
            
            foreach (var b in parentBlocks)
            {
                allBuildings.Add(b);
                if (b.ChildBuildings != null && b.ChildBuildings.Any())
                {
                    allBuildings.AddRange(b.ChildBuildings);
                }
            }
            foreach (var b in project.Blocks)
            {
                if (!allBuildings.Any(x => x.Id == b.Id))
                {
                    allBuildings.Add(b);
                }
            }

            var allBuildingIds = allBuildings.Select(b => b.Id).Distinct().ToList();
            var floorEntityTypes = allBuildingIds.Select(bId => "BuildingFloor_" + bId).ToList();

            // Kat Planı Belgelerini çek (DmsDocument)
            var floorPlanDocs = await _context.DmsDocuments
                .Where(d => floorEntityTypes.Contains(d.EntityType))
                .ToListAsync();

            // Çizim Arşivi (DocumentArchive) belgelerini çek
            var allDrawings = await _context.DocumentArchives
                .Where(d => d.ProjectId == pId)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();

            // Kat Matrisini Oluştur
            var matrix = new List<FloorPlanMatrixItem>();

            foreach (var building in allBuildings)
            {
                int totalSlabs = GetTotalSlabs(building);
                var slabIndices = new HashSet<int>();

                if (totalSlabs > 0)
                {
                    for (int s = 1; s <= totalSlabs; s++)
                    {
                        slabIndices.Add(s);
                    }
                }
                else
                {
                    slabIndices.Add(1);
                }

                if (building.Units != null && building.Units.Any())
                {
                    foreach (var u in building.Units)
                    {
                        slabIndices.Add(u.FloorLevel);
                    }
                }

                foreach (var slabIdx in slabIndices.OrderBy(s => s))
                {
                    string entityType = "BuildingFloor_" + building.Id;
                    var doc = floorPlanDocs.FirstOrDefault(d => d.EntityType == entityType && d.EntityId == slabIdx);
                    var unitsOnFloor = building.Units?.Where(u => u.FloorLevel == slabIdx).ToList() ?? new List<BuildingUnit>();

                    string floorName = GetFloorNameBySlabIndex(building, slabIdx);

                    matrix.Add(new FloorPlanMatrixItem
                    {
                        BuildingId = building.Id,
                        BlockName = building.BlockName ?? building.Name,
                        IsChildBuilding = building.ParentBuildingId.HasValue,
                        ParentBuildingId = building.ParentBuildingId,
                        FloorLevel = slabIdx,
                        FloorName = floorName,
                        UnitCount = unitsOnFloor.Count,
                        TotalGrossM2 = unitsOnFloor.Sum(u => u.GrossSquareMeters ?? 0),
                        TotalNetM2 = unitsOnFloor.Sum(u => u.NetSquareMeters ?? 0),
                        HasPlan = doc != null,
                        PlanUrl = doc?.DocumentUrl,
                        PlanFileName = doc?.Title,
                        PlanExtension = doc?.FileExtension,
                        PlanUploadDate = doc?.UploadDate,
                        PlanSizeBytes = doc?.FileSizeBytes
                    });
                }
            }

            var model = new DrawingArchiveViewModel
            {
                Project = project,
                Blocks = allBuildings,
                FloorMatrix = matrix,
                AllDrawings = allDrawings,
                ArchitecturalDrawings = allDrawings.Where(d => d.Category == "Mimari" || (d.Category != null && d.Category.Contains("Mimari"))).ToList(),
                StructuralDrawings = allDrawings.Where(d => d.Category == "Statik" || (d.Category != null && d.Category.Contains("Statik"))).ToList(),
                MechanicalDrawings = allDrawings.Where(d => d.Category == "Mekanik" || (d.Category != null && d.Category.Contains("Mekanik"))).ToList(),
                ElectricalDrawings = allDrawings.Where(d => d.Category == "Elektrik" || (d.Category != null && d.Category.Contains("Elektrik"))).ToList(),
                TotalFloorsCount = matrix.Count,
                FloorsWithPlanCount = matrix.Count(m => m.HasPlan)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDrawing(int projectId, string category, string title, int? buildingId, int? floorLevel, string? revision, IFormFile? file)
        {
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project == null) return NotFound();

            int agencyId = project.AgencyId;
            var agencyClaim = User.FindFirst("AgencyId")?.Value;
            if (int.TryParse(agencyClaim, out int aid)) agencyId = aid;

            if (file != null && file.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"Agency_{agencyId}", $"Project_{projectId}", "drawings");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var safeCategory = string.IsNullOrWhiteSpace(category) ? "Mimari" : category.Trim();
                var uniqueName = $"{safeCategory}_{Guid.NewGuid().ToString("N").Substring(0, 8)}{ext}";
                var filePath = Path.Combine(uploadsFolder, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var docUrl = $"/uploads/Agency_{agencyId}/Project_{projectId}/drawings/{uniqueName}";
                string cleanTitle = string.IsNullOrWhiteSpace(title) ? Path.GetFileNameWithoutExtension(file.FileName) : title.Trim();
                string fullTitle = string.IsNullOrWhiteSpace(revision) ? cleanTitle : $"{cleanTitle} ({revision})";

                var archive = new GMK360.Core.Entities.DocumentArchive
                {
                    AgencyId = agencyId,
                    ProjectId = projectId,
                    BuildingId = buildingId,
                    Category = safeCategory,
                    Title = fullTitle,
                    FileName = file.FileName,
                    FileExtension = ext,
                    DocumentUrl = docUrl,
                    UploadDate = DateTime.UtcNow,
                    Year = DateTime.Now.Year,
                    SourceModule = "Construction",
                    Status = "İlk Kabul Onaylandı"
                };
                _context.DocumentArchives.Add(archive);

                // Eğer bir kat planı seçildiyse DmsDocument ile Yapı Yönetimi Katına da bağla
                if (buildingId.HasValue && floorLevel.HasValue)
                {
                    string floorEntityType = "BuildingFloor_" + buildingId.Value;
                    var existingDoc = await _context.DmsDocuments
                        .FirstOrDefaultAsync(d => d.EntityType == floorEntityType && d.EntityId == floorLevel.Value);

                    if (existingDoc != null)
                    {
                        existingDoc.DocumentUrl = docUrl;
                        existingDoc.FileExtension = ext;
                        existingDoc.FileSizeBytes = file.Length;
                        existingDoc.UploadDate = DateTime.UtcNow;
                        existingDoc.Title = fullTitle;
                        _context.Update(existingDoc);
                    }
                    else
                    {
                        var dmsDoc = new GMK360.Core.Entities.DmsDocument
                        {
                            EntityType = floorEntityType,
                            EntityId = floorLevel.Value,
                            Title = fullTitle,
                            DocumentUrl = docUrl,
                            FileExtension = ext,
                            FileSizeBytes = file.Length,
                            UploadDate = DateTime.UtcNow,
                            UploadedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ""
                        };
                        _context.DmsDocuments.Add(dmsDoc);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"'{fullTitle}' çizimi başarıyla ilk kabul arşivine eklendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Lütfen geçerli bir dosya seçiniz.";
            }

            return RedirectToAction(nameof(DrawingArchive), new { projectId = projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDrawing(int id, int projectId)
        {
            var archive = await _context.DocumentArchives.FirstOrDefaultAsync(d => d.Id == id && d.ProjectId == projectId);
            if (archive != null)
            {
                _context.DocumentArchives.Remove(archive);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Çizim arşivden başarıyla kaldırıldı.";
            }
            return RedirectToAction(nameof(DrawingArchive), new { projectId = projectId });
        }

        private int GetTotalSlabs(Building b)
        {
            return b.BasementFloors + (b.HasGroundFloor ? 1 : 0) + (b.TotalFloors ?? 0) + (b.HasRoof ? 1 : 0);
        }

        private string GetFloorNameBySlabIndex(Building b, int slabIndex)
        {
            int current = 1;
            int basements = b.BasementFloors;
            if (slabIndex <= basements)
            {
                int bodrumNo = (basements - slabIndex) + 1;
                return $"-{bodrumNo}. Bodrum";
            }
            current += basements;

            if (b.HasGroundFloor)
            {
                if (slabIndex == current) return "Zemin Kat";
                current++;
            }

            int totalF = b.TotalFloors ?? 0;
            if (slabIndex < current + totalF)
            {
                int katNo = slabIndex - current + 1;
                return $"{katNo}. Normal Kat";
            }
            current += totalF;

            if (b.HasRoof && slabIndex == current)
            {
                return "Çatı Katı";
            }

            return $"{slabIndex}. Seviye";
        }

        
        
        public IActionResult ProjectDNA(int id)
        {
            ViewData["ProjectId"] = id;
            
            var project = _context.ConstructionProjects
                .Include(p => p.Blocks)
                .Include(p => p.Amenities)
                .FirstOrDefault(p => p.Id == id);
            if (project == null) return NotFound();

            ViewBag.Project = project;
            ViewData["ProjectName"] = project.Name;
            
            ViewBag.WallRecipes = _context.MaterialRecipes.ToList();
            ViewBag.MasterMaterials = _context.MasterMaterials.ToList();
            ViewBag.WorkGroups = _context.WorkGroupDefinitions.ToList();

            // DINAMIK ADIM 1 VERISI
            var step1 = _context.DnaTemplateSteps
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                        .ThenInclude(o => o.Profession)
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Profession)
                .FirstOrDefault(s => s.StepOrder == 1 || s.StepName.Contains("Adım 1") || s.Id == 3);

            if (step1 != null)
            {
                step1.Questions = step1.Questions.OrderBy(q => q.QuestionOrder).ToList();
            }

            ViewBag.Step1 = step1;

            // URETILEN PROFILLERI (KAYITLARI) CEK
            var savedProfiles = _context.ProjectDnaProfiles
                .Include(p => p.Details)
                    .ThenInclude(d => d.Profession)
                .Where(p => p.ProjectId == id)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            ViewBag.SavedProfiles = savedProfiles;

            // YAPI / BLOK HİYERARŞİSİ (A Blok, Bitişik Bloklar, Ortak Baza)
            var structures = project.Blocks.Where(b => b.ParentBuildingId == null && !b.IsExistingBuilding).ToList();
            ViewBag.Structures = structures;
            var defaultStructure = structures.FirstOrDefault();
            ViewBag.ActiveStructure = defaultStructure;

            // DXF'SİZ PARAMETRİK KEŞİF & İHALE LİSTESİ
            var activeProfile = savedProfiles.FirstOrDefault();
            ViewBag.ParametricTakeoff = GenerateParametricTakeoff(project, activeProfile);
            ViewBag.WorkflowSteps = GenerateParametricWorkflow(project, activeProfile, DateTime.Today, defaultStructure?.Id);

            return View();
        }

        // =================================================================================
        // ŞANTİYE İŞ AKIŞI, MALZEME & KADRO MASASI (YAPI ODAKLI ÇALIŞMA MASASI)
        // =================================================================================
        public async Task<IActionResult> BuildingWorkflow(int id, int? profileId, int? buildingId)
        {
            var project = await _context.ConstructionProjects
                .Include(p => p.Blocks).ThenInclude(b => b.ChildBuildings).ThenInclude(c => c.Units)
                .Include(p => p.Blocks).ThenInclude(b => b.Units)
                .Include(p => p.Amenities)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return NotFound();

            ViewData["ProjectId"] = id;
            ViewData["ProjectName"] = project.Name;
            ViewBag.Project = project;

            // Yapılar (A Blok, Bitişik Bloklar, Ortak Baza)
            var structures = project.Blocks.Where(b => b.ParentBuildingId == null && !b.IsExistingBuilding).ToList();
            ViewBag.Structures = structures;

            // Kayıtlı Profiller
            var savedProfiles = await _context.ProjectDnaProfiles
                .Include(p => p.Details).ThenInclude(d => d.Profession)
                .Where(p => p.ProjectId == id)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            ViewBag.SavedProfiles = savedProfiles;
            var activeProfile = profileId.HasValue 
                ? savedProfiles.FirstOrDefault(p => p.Id == profileId.Value) 
                : savedProfiles.FirstOrDefault(p => p.Status == "Tamamlandı") ?? savedProfiles.FirstOrDefault();
            ViewBag.ActiveProfile = activeProfile;

            // Seçili Yapı (varsayılan: parametre veya ilk yapı)
            var activeStructure = buildingId.HasValue 
                ? structures.FirstOrDefault(s => s.Id == buildingId.Value) 
                : structures.FirstOrDefault();
            ViewBag.ActiveStructure = activeStructure;

            return View();
        }

        // YENİ TAM SAYFA DNA SİHİRBAZI (YENİ SEKME)
        public IActionResult DnaWizard(int id, int? profileId, int? startStep = null)
        {
            var project = _context.ConstructionProjects
                .Include(p => p.Blocks)
                .Include(p => p.Amenities)
                .FirstOrDefault(p => p.Id == id);
            if (project == null) return NotFound();

            ViewBag.Project = project;
            ViewData["ProjectId"] = id;
            ViewData["ProjectName"] = project.Name;

            // ADIM 0 (Mecburi Statik ve Taşıyıcılar - 20 Soru)
            var step0 = _context.DnaTemplateSteps
                .AsNoTracking()
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Profession)
                .FirstOrDefault(s => s.StepOrder == 0 || s.Id == 2);

            if (step0 != null && step0.Questions != null)
            {
                step0.Questions = step0.Questions.OrderBy(q => q.QuestionOrder).ToList();
            }
            ViewBag.Step0 = step0;

            // ADIM 1 (Saha Analizi ve Şantiye Kurulumu)
            var step1 = _context.DnaTemplateSteps
                .AsNoTracking()
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                        .ThenInclude(o => o.Profession)
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Profession)
                .FirstOrDefault(s => s.StepOrder == 1 || s.Id == 3);

            if (step1 != null && step1.Questions != null)
            {
                step1.Questions = step1.Questions.OrderBy(q => q.QuestionOrder).ToList();
            }
            ViewBag.Step1 = step1;

            // TÜM ADIMLAR (Navigasyon ve Tüm Fazların Soruları/Seçenekleri İçin)
            ViewBag.AllSteps = _context.DnaTemplateSteps
                .AsNoTracking()
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                        .ThenInclude(o => o.Profession)
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Profession)
                .OrderBy(s => s.StepOrder)
                .ToList();

            // İŞ BRANŞLARI (Pazar Yeri Eşleştirmesi)
            ViewBag.Professions = _context.DnaProfessions.OrderBy(p => p.Name).ToList();

            // MEVCUT BİR TASLAK VEYA PROFİL VAR MI?
            // NOT: profileId parametresi verilmişse (Düzenle tıklandıysa) o profili açar.
            // Verilmediyse (Yeni Profil tıklandıysa) tertemiz YENİ bir profil açar; eski taslağa zorlamaz!
            if (profileId.HasValue && profileId.Value > 0)
            {
                var existing = _context.ProjectDnaProfiles
                    .Include(p => p.Details)
                        .ThenInclude(d => d.Profession)
                    .FirstOrDefault(p => p.Id == profileId.Value && p.ProjectId == id);
                ViewBag.ExistingProfile = existing;
            }
            else
            {
                ViewBag.ExistingProfile = null;
            }

            int? effectiveStartStep = startStep;
            if (!effectiveStartStep.HasValue && ViewBag.ExistingProfile != null)
            {
                var prof = ViewBag.ExistingProfile as ProjectDnaProfile;
                if (prof != null && int.TryParse(prof.ExtraField1, out int sStep))
                {
                    effectiveStartStep = sStep;
                }
            }
            ViewBag.StartStep = effectiveStartStep;

            // PARAMETRİK KEŞİF LİSTESİ
            var currProfile = ViewBag.ExistingProfile as ProjectDnaProfile;
            ViewBag.ParametricTakeoff = GenerateParametricTakeoff(project, currProfile);
            ViewBag.WorkflowSteps = GenerateParametricWorkflow(project, currProfile, DateTime.Today);

            return View();
        }

        public class DnaProfileSaveDto
        {
            public int? ProfileId { get; set; }
            public int ProjectId { get; set; }
            public string Name { get; set; }
            public int? StartStep { get; set; }
            public List<DnaProfileDetailDto> Details { get; set; }
        }

        public class DnaProfileDetailDto
        {
            public string Phase { get; set; }
            public string SelectedItem { get; set; }
            public int ValueId { get; set; } // OptionId
        }

        [HttpGet]
        public IActionResult CheckProfileName(int projectId, string name, int? profileId)
        {
            if (string.IsNullOrWhiteSpace(name)) return Json(new { exists = false });
            var trimmed = name.Trim().ToLower();
            bool exists = _context.ProjectDnaProfiles.Any(p =>
                p.ProjectId == projectId &&
                p.Name.Trim().ToLower() == trimmed &&
                (!profileId.HasValue || p.Id != profileId.Value));
            return Json(new { exists });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult AutoSaveProfile([FromBody] DnaProfileSaveDto data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.Name)) return BadRequest();

            var trimmedName = data.Name.Trim();

            ProjectDnaProfile profile = null;
            if (data.ProfileId.HasValue && data.ProfileId.Value > 0)
            {
                profile = _context.ProjectDnaProfiles
                    .Include(p => p.Details)
                    .FirstOrDefault(p => p.Id == data.ProfileId.Value && p.ProjectId == data.ProjectId);
            }

            if (profile == null)
            {
                // Aynı isimde aktif bir taslak varsa onu devam ettir
                profile = _context.ProjectDnaProfiles
                    .Include(p => p.Details)
                    .FirstOrDefault(p => p.ProjectId == data.ProjectId && p.Name.Trim().ToLower() == trimmedName.ToLower());
            }

            // Başka bir profille isim çakışması kontrolü
            bool isDuplicate = _context.ProjectDnaProfiles.Any(p =>
                p.ProjectId == data.ProjectId &&
                p.Name.Trim().ToLower() == trimmedName.ToLower() &&
                (profile == null || p.Id != profile.Id));

            if (isDuplicate)
            {
                return Json(new { success = false, duplicate = true, message = "Bu isimde bir DNA profili bu projede zaten kayıtlı! Lütfen farklı bir isim belirleyiniz." });
            }

            if (profile == null)
            {
                profile = new ProjectDnaProfile
                {
                    ProjectId = data.ProjectId,
                    Name = trimmedName,
                    Status = "Taslak",
                    CreatedBy = User.Identity?.Name ?? "Admin",
                    CreatedDate = DateTime.UtcNow,
                    ExtraField1 = data.StartStep?.ToString()
                };
                _context.ProjectDnaProfiles.Add(profile);
            }
            else
            {
                profile.Name = trimmedName;
                profile.Status = "Taslak";
                if (data.StartStep.HasValue)
                {
                    profile.ExtraField1 = data.StartStep.Value.ToString();
                }
                if (profile.Details != null && profile.Details.Any())
                {
                    _context.ProjectDnaDetails.RemoveRange(profile.Details);
                }
            }

            _context.SaveChanges();

            if (data.Details != null && data.Details.Any())
            {
                var optIds = data.Details.Select(d => d.ValueId).ToList();
                var options = _context.DnaTemplateOptions.Where(o => optIds.Contains(o.Id)).ToList();
                var questions = _context.DnaTemplateQuestions.Where(q => optIds.Contains(q.Id)).ToList();

                foreach (var d in data.Details)
                {
                    var opt = options.FirstOrDefault(o => o.Id == d.ValueId);
                    int? profId = opt?.DnaProfessionId;
                    if (!profId.HasValue && opt != null)
                    {
                        var optQ = _context.DnaTemplateQuestions.Find(opt.DnaTemplateQuestionId);
                        profId = optQ?.DnaProfessionId;
                    }
                    if (!profId.HasValue)
                    {
                        var qItem = questions.FirstOrDefault(q => q.Id == d.ValueId);
                        profId = qItem?.DnaProfessionId;
                    }

                    decimal qty = 0;
                    string unit = "Belirsiz";
                    if (!string.IsNullOrEmpty(d.SelectedItem) && d.SelectedItem.Contains(":"))
                    {
                        var parts = d.SelectedItem.Split(':');
                        if (parts.Length > 1)
                        {
                            var valStr = parts[1].Replace("m", "").Trim();
                            decimal.TryParse(valStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out qty);
                            unit = "mt";
                        }
                    }

                    _context.ProjectDnaDetails.Add(new ProjectDnaDetail
                    {
                        ProfileId = profile.Id,
                        Phase = d.Phase,
                        SelectedItem = d.SelectedItem,
                        DnaProfessionId = profId,
                        Quantity = qty,
                        Unit = unit
                    });
                }
                _context.SaveChanges();
            }

            return Json(new { success = true, profileId = profile.Id });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult SaveDnaProfile([FromBody] DnaProfileSaveDto data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.Name)) return BadRequest();

            var trimmedName = data.Name.Trim();

            ProjectDnaProfile profile = null;
            if (data.ProfileId.HasValue && data.ProfileId.Value > 0)
            {
                profile = _context.ProjectDnaProfiles
                    .Include(p => p.Details)
                    .FirstOrDefault(p => p.Id == data.ProfileId.Value && p.ProjectId == data.ProjectId);
            }

            if (profile == null)
            {
                profile = _context.ProjectDnaProfiles
                    .Include(p => p.Details)
                    .FirstOrDefault(p => p.ProjectId == data.ProjectId && p.Name.Trim().ToLower() == trimmedName.ToLower());
            }

            // Aynı isimde başka bir profil var mı kontrolü
            bool isDuplicate = _context.ProjectDnaProfiles.Any(p =>
                p.ProjectId == data.ProjectId &&
                p.Name.Trim().ToLower() == trimmedName.ToLower() &&
                (profile == null || p.Id != profile.Id));

            if (isDuplicate)
            {
                return Json(new { success = false, duplicate = true, message = "Bu isimde bir DNA profili bu projede zaten kayıtlı! Lütfen farklı bir isim belirleyiniz." });
            }

            if (profile == null)
            {
                profile = new ProjectDnaProfile
                {
                    ProjectId = data.ProjectId,
                    Name = trimmedName,
                    Status = "DNA Tamamlandı",
                    CreatedBy = User.Identity?.Name ?? "Admin",
                    CreatedDate = DateTime.UtcNow,
                    ExtraField1 = data.StartStep?.ToString()
                };
                _context.ProjectDnaProfiles.Add(profile);
            }
            else
            {
                profile.Name = trimmedName;
                profile.Status = "DNA Tamamlandı";
                if (data.StartStep.HasValue)
                {
                    profile.ExtraField1 = data.StartStep.Value.ToString();
                }
                if (profile.Details != null && profile.Details.Any())
                {
                    _context.ProjectDnaDetails.RemoveRange(profile.Details);
                }
            }

            _context.SaveChanges();

            if (data.Details != null && data.Details.Any())
            {
                var optIds = data.Details.Select(d => d.ValueId).ToList();
                var options = _context.DnaTemplateOptions.Where(o => optIds.Contains(o.Id)).ToList();
                var questions = _context.DnaTemplateQuestions.Where(q => optIds.Contains(q.Id)).ToList();

                foreach (var d in data.Details)
                {
                    var opt = options.FirstOrDefault(o => o.Id == d.ValueId);
                    int? profId = opt?.DnaProfessionId;
                    if (!profId.HasValue && opt != null)
                    {
                        var optQ = _context.DnaTemplateQuestions.Find(opt.DnaTemplateQuestionId);
                        profId = optQ?.DnaProfessionId;
                    }
                    if (!profId.HasValue)
                    {
                        var qItem = questions.FirstOrDefault(q => q.Id == d.ValueId);
                        profId = qItem?.DnaProfessionId;
                    }

                    decimal qty = 0;
                    string unit = "Belirsiz";
                    if (!string.IsNullOrEmpty(d.SelectedItem) && d.SelectedItem.Contains(":"))
                    {
                        var parts = d.SelectedItem.Split(':');
                        if (parts.Length > 1)
                        {
                            var valStr = parts[1].Replace("m", "").Trim();
                            decimal.TryParse(valStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out qty);
                            unit = "mt";
                        }
                    }

                    _context.ProjectDnaDetails.Add(new ProjectDnaDetail
                    {
                        ProfileId = profile.Id,
                        Phase = d.Phase,
                        SelectedItem = d.SelectedItem,
                        DnaProfessionId = profId,
                        Quantity = qty,
                        Unit = unit
                    });
                }
                _context.SaveChanges();
            }

            return Json(new { success = true, profileId = profile.Id });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult DeleteDnaProfile(int id)
        {
            var profile = _context.ProjectDnaProfiles.Include(p => p.Details).FirstOrDefault(p => p.Id == id);
            if (profile != null)
            {
                _context.ProjectDnaDetails.RemoveRange(profile.Details);
                _context.ProjectDnaProfiles.Remove(profile);
                _context.SaveChanges();
            }
            return Ok();
        }

        public IActionResult ParseDxf(int projectId, int recipeId)
        {
            // Simulate DXF parsing
            var recipe = _context.MaterialRecipes
                .Include(r => r.Ingredients)
                .ThenInclude(i => i.MasterMaterial)
                .FirstOrDefault(r => r.Id == recipeId);

            ViewBag.Recipe = recipe;
            ViewData["ProjectId"] = projectId;
            return View("Result");
        }

        [HttpPost]
        public IActionResult SimulateDxfUnitTypeRead(int projectId)
        {
            // DXF'ten Daire Tiplerini (Tip Planları) Okuma Simülasyonu
            var existing = _context.DxfUnitTypeProfiles.Where(x => x.ProjectId == projectId).ToList();
            if(existing.Any()) {
                _context.DxfUnitSpaces.RemoveRange(_context.DxfUnitSpaces.Where(x => existing.Select(e => e.Id).Contains(x.DxfUnitTypeProfileId)));
                _context.DxfUnitTypeProfiles.RemoveRange(existing);
                _context.SaveChanges();
            }

            var typeA = new DxfUnitTypeProfile { ProjectId = projectId, ModelName = "A Tipi (3+1 Kuzey)", Category = "Daire", TotalGrossArea = 145, TotalNetArea = 120, TotalSpaceCount = 8 };
            var typeB = new DxfUnitTypeProfile { ProjectId = projectId, ModelName = "Ticari Mağaza 1", Category = "Ticari", TotalGrossArea = 250, TotalNetArea = 230, TotalSpaceCount = 4 };
            var typeC = new DxfUnitTypeProfile { ProjectId = projectId, ModelName = "Ortak Alan: Sığınak & Otopark", Category = "OrtakAlan", TotalGrossArea = 600, TotalNetArea = 580, TotalSpaceCount = 3 };

            _context.DxfUnitTypeProfiles.AddRange(typeA, typeB, typeC);
            _context.SaveChanges();

            // Odaları (Mahalleri) Oluştur
            _context.DxfUnitSpaces.AddRange(
                new DxfUnitSpace { DxfUnitTypeProfileId = typeA.Id, SpaceName = "Salon", SpaceGroup = "Oda / Yaşam Alanı", FloorArea = 30, CeilingArea = 30, GrossWallArea = 50, NetWallArea = 42, SkirtingLength = 22, WindowSillLength = 3, InteriorDoorCount = 1, WindowCount = 2 },
                new DxfUnitSpace { DxfUnitTypeProfileId = typeA.Id, SpaceName = "Ebeveyn Banyo", SpaceGroup = "Islak Hacim", FloorArea = 5, CeilingArea = 5, GrossWallArea = 15, NetWallArea = 14, SkirtingLength = 0, WindowSillLength = 0, InteriorDoorCount = 1, WindowCount = 1 },
                new DxfUnitSpace { DxfUnitTypeProfileId = typeB.Id, SpaceName = "Ana Satış Alanı", SpaceGroup = "Ticari Alan", FloorArea = 200, CeilingArea = 200, GrossWallArea = 120, NetWallArea = 90, SkirtingLength = 60, WindowSillLength = 10, InteriorDoorCount = 0, WindowCount = 4 },
                new DxfUnitSpace { DxfUnitTypeProfileId = typeC.Id, SpaceName = "Kapalı Otopark", SpaceGroup = "Ortak Alan", FloorArea = 500, CeilingArea = 500, GrossWallArea = 300, NetWallArea = 280, SkirtingLength = 120, WindowSillLength = 0, InteriorDoorCount = 2, WindowCount = 0 }
            );
            _context.SaveChanges();

            return Ok();
        }

        [HttpGet]
        public IActionResult GetDxfUnitTypes(int projectId)
        {
            var types = _context.DxfUnitTypeProfiles
                .Include(x => x.Spaces)
                .Where(x => x.ProjectId == projectId)
                .Select(x => new {
                    x.Id,
                    x.ModelName,
                    x.Category,
                    x.TotalGrossArea,
                    x.TotalSpaceCount,
                    SpacesSummary = string.Join(", ", x.Spaces.Take(2).Select(s => $"{s.SpaceName} ({s.FloorArea}m²)")),
                    SpacesCount = x.Spaces.Count
                }).ToList();

            return Json(types);
        }

        // =================================================================================
        // OTONOM ŞANTİYE ŞEFİ AJANDASI (T-, T, T+ ZAMAN TÜNELİ)
        // =================================================================================
        public async Task<IActionResult> SiteAgenda(int id, int? profileId, DateTime? baseDate)
        {
            var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null) return NotFound();

            ViewData["ProjectId"] = id;
            ViewData["ProjectName"] = project.Name;

            ProjectDnaProfile profile = null;
            if (profileId.HasValue && profileId.Value > 0)
            {
                profile = await _context.ProjectDnaProfiles
                    .Include(p => p.Details)
                        .ThenInclude(d => d.Profession)
                    .FirstOrDefaultAsync(p => p.Id == profileId.Value && p.ProjectId == id);
            }

            if (profile == null)
            {
                profile = await _context.ProjectDnaProfiles
                    .Include(p => p.Details)
                        .ThenInclude(d => d.Profession)
                    .Where(p => p.ProjectId == id)
                    .OrderByDescending(p => p.CreatedDate)
                    .FirstOrDefaultAsync();
            }

            ViewBag.Profile = profile;
            var effectiveBaseDate = baseDate ?? DateTime.Today.AddDays(7);
            ViewBag.BaseDate = effectiveBaseDate;

            // Diğer profilleri de dropdown için gönder
            ViewBag.AllProfiles = await _context.ProjectDnaProfiles
                .Where(p => p.ProjectId == id)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            var tasks = _dnaTimeOffsetService.GenerateTasksForProfile(profile, effectiveBaseDate);
            ViewBag.Tasks = tasks;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SyncSiteAgenda(int projectId, int profileId, DateTime baseDate)
        {
            int agencyId = 1;
            var agencyIdClaim = User.FindFirst("AgencyId")?.Value;
            if (int.TryParse(agencyIdClaim, out int aid)) agencyId = aid;

            var profile = await _context.ProjectDnaProfiles
                .Include(p => p.Details)
                .FirstOrDefaultAsync(p => p.Id == profileId && p.ProjectId == projectId);

            if (profile != null)
            {
                var tasks = _dnaTimeOffsetService.GenerateTasksForProfile(profile, baseDate);
                int count = await _dnaTimeOffsetService.SyncTasksToAgendaAsync(agencyId, projectId, tasks);
                TempData["SuccessMessage"] = $"{count} adet otonom şantiye görevi sistem ajandanıza (takvime) başarıyla işlendi!";
            }

            return RedirectToAction(nameof(SiteAgenda), new { id = projectId, profileId = profileId, baseDate = baseDate.ToString("yyyy-MM-dd") });
        }

        // =================================================================================
        // DXF'SİZ PARAMETRİK KEŞİF, İHALE & MALZEME MOTORU
        // =================================================================================
        [HttpGet]
        public async Task<IActionResult> GetParametricTakeoff(int projectId, int? profileId)
        {
            var project = await _context.ConstructionProjects
                .Include(p => p.Blocks)
                .Include(p => p.Amenities)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return NotFound(new { success = false, message = "Proje bulunamadı." });

            ProjectDnaProfile profile = null;
            if (profileId.HasValue && profileId.Value > 0)
            {
                profile = await _context.ProjectDnaProfiles
                    .Include(p => p.Details)
                        .ThenInclude(d => d.Profession)
                    .FirstOrDefaultAsync(p => p.Id == profileId.Value && p.ProjectId == projectId);
            }
            if (profile == null)
            {
                profile = await _context.ProjectDnaProfiles
                    .Include(p => p.Details)
                        .ThenInclude(d => d.Profession)
                    .Where(p => p.ProjectId == projectId)
                    .OrderByDescending(p => p.CreatedDate)
                    .FirstOrDefaultAsync();
            }

            var items = GenerateParametricTakeoff(project, profile);
            return Json(new { success = true, items = items });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SendParametricToB2B([FromBody] SendParametricB2BDto dto)
        {
            if (dto == null || dto.ProjectId <= 0) return BadRequest(new { success = false, message = "Geçersiz parametre." });

            var project = await _context.ConstructionProjects
                .Include(p => p.Blocks)
                .Include(p => p.Amenities)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);

            if (project == null) return NotFound(new { success = false, message = "Proje bulunamadı." });

            ProjectDnaProfile profile = null;
            if (dto.ProfileId.HasValue && dto.ProfileId.Value > 0)
            {
                profile = await _context.ProjectDnaProfiles
                    .Include(p => p.Details)
                        .ThenInclude(d => d.Profession)
                    .FirstOrDefaultAsync(p => p.Id == dto.ProfileId.Value && p.ProjectId == dto.ProjectId);
            }
            if (profile == null)
            {
                profile = await _context.ProjectDnaProfiles
                    .Include(p => p.Details)
                        .ThenInclude(d => d.Profession)
                    .Where(p => p.ProjectId == dto.ProjectId)
                    .OrderByDescending(p => p.CreatedDate)
                    .FirstOrDefaultAsync();
            }

            var allItems = GenerateParametricTakeoff(project, profile);
            var selectedItems = (dto.SelectedItemIds != null && dto.SelectedItemIds.Any())
                ? allItems.Where(i => dto.SelectedItemIds.Contains(i.Id)).ToList()
                : allItems;

            if (!selectedItems.Any()) return BadRequest(new { success = false, message = "Hiç kalem seçilmedi." });

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var consultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == userId);
            int agencyId = consultant?.AgencyId ?? (project.AgencyId > 0 ? project.AgencyId : 1);

            var sb = new StringBuilder();
            sb.AppendLine($"### 🏗️ {project.Name} - Parametrik İhale & Malzeme Alım Paketi");
            sb.AppendLine($"*DXF Çizimi Olmaksızın Proje Yapı Özeti ve DNA Çevresel Faktörlerinden Otomatik Türetilmiştir.*");
            sb.AppendLine();
            sb.AppendLine("| # | İmalat / Malzeme Kalemi | Miktar | Birim | İlgili İş Branşı (Taşeron) | Hesaplama Formülü |");
            sb.AppendLine("|---|---|---|---|---|---|");

            foreach (var item in selectedItems)
            {
                sb.AppendLine($"| {item.Id} | **{item.Title}** | {item.Quantity:N0} | {item.Unit} | `{item.ProfessionName}` | {item.Formula} |");
            }

            var quoteRequest = new B2BQuoteRequest
            {
                RequesterAgencyId = agencyId,
                RequesterUserId = userId,
                SourceModule = "ConstructionProject",
                SourceReferenceId = project.Id,
                Title = $"{project.Name} - Parametrik İhale Paketi ({selectedItems.Count} Kalem)",
                Description = sb.ToString(),
                Deadline = DateTime.UtcNow.AddDays(14),
                Status = "Draft",
                CreatedAt = DateTime.UtcNow
            };

            _context.B2BQuoteRequests.Add(quoteRequest);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                quoteRequestId = quoteRequest.Id,
                itemCount = selectedItems.Count,
                message = $"{selectedItems.Count} adet parametrik keşif kalemi B2B Satın Alma & İhale havuzuna başarıyla aktarıldı!"
            });
        }

        // =================================================================================
        // ÇİFT LİSTE & TEK SIRA KODLU (X01-X09) İŞ GÜCÜ, TEDARİK VE TAKVİM MOTORU
        // =================================================================================
        [HttpGet]
        public async Task<IActionResult> GetParametricWorkflow(int projectId, int? profileId, DateTime? startDate, int? buildingId = null, string? mode = null)
        {
            var project = await _context.ConstructionProjects
                .Include(p => p.Blocks)
                .Include(p => p.Amenities)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return NotFound(new { success = false, message = "Proje bulunamadı." });

            ProjectDnaProfile profile = null;
            if (profileId.HasValue && profileId.Value > 0)
            {
                profile = await _context.ProjectDnaProfiles
                    .Include(p => p.Details)
                        .ThenInclude(d => d.Profession)
                    .FirstOrDefaultAsync(p => p.Id == profileId.Value && p.ProjectId == projectId);
            }
            if (profile == null)
            {
                profile = await _context.ProjectDnaProfiles
                    .Include(p => p.Details)
                        .ThenInclude(d => d.Profession)
                    .Where(p => p.ProjectId == projectId)
                    .OrderByDescending(p => p.CreatedDate)
                    .FirstOrDefaultAsync();
            }

            var baseDate = startDate ?? DateTime.Today;
            var steps = GenerateParametricWorkflow(project, profile, baseDate, buildingId);

            int totalManDays = steps.Sum(s => s.TotalManDays);
            int maxCrew = steps.Any() ? steps.Max(s => s.CrewSize) : 0;
            int totalDurationDays = steps.Any() ? (int)(steps.Max(s => s.PlannedEndDate.Value) - steps.Min(s => s.PlannedStartDate.Value)).TotalDays + 1 : 0;
            var estimatedCompletion = steps.Any() ? steps.Max(s => s.PlannedEndDate.Value).ToString("dd.MM.yyyy") : "";

            return Json(new
            {
                success = true,
                steps = steps,
                summary = new
                {
                    totalManDays = totalManDays,
                    maxCrew = maxCrew,
                    totalDurationDays = totalDurationDays,
                    estimatedCompletion = estimatedCompletion,
                    startDateStr = baseDate.ToString("dd.MM.yyyy")
                }
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SendWorkflowToB2B([FromBody] SendWorkflowB2BDto dto)
        {
            if (dto == null || dto.ProjectId <= 0) return BadRequest(new { success = false, message = "Geçersiz parametre." });

            var project = await _context.ConstructionProjects
                .Include(p => p.Blocks)
                .Include(p => p.Amenities)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);

            if (project == null) return NotFound(new { success = false, message = "Proje bulunamadı." });

            ProjectDnaProfile profile = null;
            if (dto.ProfileId.HasValue && dto.ProfileId.Value > 0)
            {
                profile = await _context.ProjectDnaProfiles
                    .Include(p => p.Details)
                        .ThenInclude(d => d.Profession)
                    .FirstOrDefaultAsync(p => p.Id == dto.ProfileId.Value && p.ProjectId == dto.ProjectId);
            }
            if (profile == null)
            {
                profile = await _context.ProjectDnaProfiles
                    .Include(p => p.Details)
                        .ThenInclude(d => d.Profession)
                    .Where(p => p.ProjectId == dto.ProjectId)
                    .OrderByDescending(p => p.CreatedDate)
                    .FirstOrDefaultAsync();
            }

            var allSteps = GenerateParametricWorkflow(project, profile, DateTime.Today, dto.BuildingId);
            var selectedSteps = (dto.SelectedStepCodes != null && dto.SelectedStepCodes.Any())
                ? allSteps.Where(s => dto.SelectedStepCodes.Contains(s.StepCode)).ToList()
                : allSteps;

            if (!selectedSteps.Any()) return BadRequest(new { success = false, message = "Hiç aşama seçilmedi." });

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var consultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == userId);
            int agencyId = consultant?.AgencyId ?? (project.AgencyId > 0 ? project.AgencyId : 1);

            var sb = new StringBuilder();
            string targetTrack = dto.TargetTrack ?? "Material"; // "Material" or "Labor"
            string structPrefix = !string.IsNullOrEmpty(selectedSteps.FirstOrDefault()?.TargetStructureName)
                ? $"[{selectedSteps.First().TargetStructureName}] "
                : "";
            string rfpTitle = "";

            if (targetTrack == "Labor")
            {
                rfpTitle = $"{project.Name} - {structPrefix}Taşeron & İş Gücü Çağrısı ({selectedSteps.Count} Aşama)";
                sb.AppendLine($"### 👷 {project.Name} - {structPrefix}Taşeron & İş Gücü Hizmet Çağrısı");
                sb.AppendLine($"*İnşaat İş Akış Sıralamasına Göre Ekip, Kadro ve Süre İhtiyaçları*");
                sb.AppendLine();
                sb.AppendLine("| Kod | Parça | Aşama Adı | İlgili Branş / Meslek | Önerilen Ekip Kadrosu | Süre (İş Günü) | Hacim (Adam-Gün) | Kritik Gereksinim / Ekipman |");
                sb.AppendLine("|:---:|---|---|---|---|:---:|:---:|---|");
                foreach (var s in selectedSteps)
                {
                    sb.AppendLine($"| **{s.StepCode}** | {s.PartName} | {s.PhaseName} | `{s.ProfessionName}` | {s.CrewComposition} ({s.CrewSize} Kişi) | {s.DurationDays} Gün | {s.TotalManDays} Adam-Gün | {s.EquipmentNeeded} |");
                }
            }
            else
            {
                rfpTitle = $"{project.Name} - {structPrefix}Sıralı Malzeme Tedarik İhalesi ({selectedSteps.Count} Aşama)";
                sb.AppendLine($"### 📦 {project.Name} - {structPrefix}Sıralı Malzeme Tedarik & Satın Alma Paketi");
                sb.AppendLine($"*İnşaat İş Akış Sıralamasına Göre Tedarik Edilecek Malzemeler*");
                sb.AppendLine();
                sb.AppendLine("| Kod | Parça | Aşama | Tedarik Edilecek Malzeme | Miktar | Birim | Tedarikçi Grubu | Mühendislik Hesabı |");
                sb.AppendLine("|:---:|---|---|---|---|---|---|---|");
                foreach (var s in selectedSteps)
                {
                    sb.AppendLine($"| **{s.StepCode}** | {s.PartName} | {s.PhaseName} | **{s.MaterialTitle}** | {s.MaterialQuantity:N0} | {s.MaterialUnit} | `{s.SupplierCategory}` | {s.MaterialFormula} |");
                }
            }

            var quoteRequest = new B2BQuoteRequest
            {
                RequesterAgencyId = agencyId,
                RequesterUserId = userId,
                SourceModule = "ConstructionProject",
                SourceReferenceId = project.Id,
                Title = rfpTitle,
                Description = sb.ToString(),
                Deadline = DateTime.UtcNow.AddDays(14),
                Status = "Draft",
                CreatedAt = DateTime.UtcNow
            };

            _context.B2BQuoteRequests.Add(quoteRequest);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                quoteRequestId = quoteRequest.Id,
                itemCount = selectedSteps.Count,
                message = $"{selectedSteps.Count} adet iş akış aşaması B2B {(targetTrack == "Labor" ? "Taşeron & İş Gücü" : "Malzeme Satın Alma")} havuzuna başarıyla aktarıldı!"
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> TriggerWorkflowAgenda([FromBody] TriggerWorkflowAgendaDto dto)
        {
            if (dto == null || dto.ProjectId <= 0) return BadRequest(new { success = false, message = "Geçersiz parametre." });

            var project = await _context.ConstructionProjects
                .Include(p => p.Blocks)
                .Include(p => p.Amenities)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);

            if (project == null) return NotFound(new { success = false, message = "Proje bulunamadı." });

            ProjectDnaProfile profile = null;
            if (dto.ProfileId.HasValue && dto.ProfileId.Value > 0)
            {
                profile = await _context.ProjectDnaProfiles
                    .Include(p => p.Details)
                        .ThenInclude(d => d.Profession)
                    .FirstOrDefaultAsync(p => p.Id == dto.ProfileId.Value && p.ProjectId == dto.ProjectId);
            }
            if (profile == null)
            {
                profile = await _context.ProjectDnaProfiles
                    .Include(p => p.Details)
                        .ThenInclude(d => d.Profession)
                    .Where(p => p.ProjectId == dto.ProjectId)
                    .OrderByDescending(p => p.CreatedDate)
                    .FirstOrDefaultAsync();
            }

            var baseDate = dto.StartDate > DateTime.MinValue ? dto.StartDate : DateTime.Today;
            var allSteps = GenerateParametricWorkflow(project, profile, baseDate, dto.BuildingId);
            var selectedSteps = (dto.SelectedStepCodes != null && dto.SelectedStepCodes.Any())
                ? allSteps.Where(s => dto.SelectedStepCodes.Contains(s.StepCode)).ToList()
                : allSteps;

            if (!selectedSteps.Any()) return BadRequest(new { success = false, message = "Hiç aşama seçilmedi." });

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var consultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == userId);
            int agencyId = consultant?.AgencyId ?? (project.AgencyId > 0 ? project.AgencyId : 1);

            int createdCount = 0;

            foreach (var step in selectedSteps)
            {
                if (!step.PlannedStartDate.HasValue) continue;

                // 1. Ana İmalat Görevi (T 0)
                var mainTitle = $"🏗️ [{step.StepCode}] {step.PhaseName} İmalat Başlangıcı";
                bool existsMain = await _context.AgendaRecords.AnyAsync(a =>
                    a.AgencyId == agencyId &&
                    a.ProjectId == project.Id &&
                    a.EventDate.Date == step.PlannedStartDate.Value.Date &&
                    a.Title == mainTitle &&
                    !a.IsDeleted);

                if (!existsMain)
                {
                    _context.AgendaRecords.Add(new AgendaRecord
                    {
                        AgencyId = agencyId,
                        ProjectId = project.Id,
                        Title = mainTitle,
                        EventDate = step.PlannedStartDate.Value,
                        RecordTypeId = AgendaConstants.TypeSantiyeDenetimi,
                        StatusId = AgendaConstants.StatusPlanlandi,
                        Notes = $"Kadro: {step.CrewComposition} ({step.CrewSize} Kişi)\nSüre: {step.DurationDays} İş Günü ({step.TotalManDays} Adam-Gün)\nÖnkoşul: {step.Predecessors}\nKritik Kural: {step.CriticalDependency}\nMalzeme: {step.MaterialTitle} ({step.MaterialQuantity:N0} {step.MaterialUnit})"
                    });
                    createdCount++;
                }

                // 2. T- Ön Hazırlık Hatırlatması (2 veya 3 gün önce)
                int reminderDays = step.DurationDays >= 5 ? 3 : 2;
                var reminderDate = step.PlannedStartDate.Value.AddDays(-reminderDays);
                var reminderTitle = $"⚠️ [T-{reminderDays} Hazırlık - {step.StepCode}] {step.PhaseName} Ön Kontrolleri";

                bool existsReminder = await _context.AgendaRecords.AnyAsync(a =>
                    a.AgencyId == agencyId &&
                    a.ProjectId == project.Id &&
                    a.EventDate.Date == reminderDate.Date &&
                    a.Title == reminderTitle &&
                    !a.IsDeleted);

                if (!existsReminder)
                {
                    _context.AgendaRecords.Add(new AgendaRecord
                    {
                        AgencyId = agencyId,
                        ProjectId = project.Id,
                        Title = reminderTitle,
                        EventDate = reminderDate,
                        RecordTypeId = AgendaConstants.TypeTaseronToplantisi,
                        StatusId = AgendaConstants.StatusPlanlandi,
                        Notes = $"⚠️ DİKKAT: {step.PlannedStartDate.Value:dd.MM.yyyy} tarihinde başlayacak {step.PhaseName} için;\n1) {step.MaterialTitle} depoya sevk edildi mi?\n2) {step.ProfessionName} ekibi teyit edildi mi?\n3) {step.CriticalDependency}"
                    });
                    createdCount++;
                }
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                createdCount = createdCount,
                message = $"{selectedSteps.Count} adet aşamaya ait {createdCount} otonom ajanda görevi ve T- hatırlatması takviminize başarıyla işlendi!"
            });
        }

        private List<ParametricTakeoffItemDto> GenerateParametricTakeoff(ConstructionProject project, ProjectDnaProfile profile)
        {
            var list = new List<ParametricTakeoffItemDto>();
            if (project == null) return list;

            var blocks = project.Blocks?.Where(b => !b.IsExistingBuilding).ToList() ?? new List<GMK360.Core.Entities.Building>();
            var rootBlocks = blocks.Where(b => b.ParentBuildingId == null).ToList();

            double landArea = project.TotalLandArea ?? 0;
            double baseArea = rootBlocks.Sum(b => b.BaseArea ?? 0);
            if (baseArea <= 0 && blocks.Any()) baseArea = blocks.Sum(b => b.BaseArea ?? 0);
            if (baseArea <= 0) baseArea = project.EskiBinaOturumAlani ?? (landArea > 0 ? landArea * 0.40 : 500);

            int maxFloors = blocks.Any() ? blocks.Max(b => (b.TotalFloors ?? 0) + b.BasementFloors) : (project.TotalFloors ?? 5);
            int maxBasement = blocks.Any() ? blocks.Max(b => b.BasementFloors) : (project.EskiBodrumKatSayisi ?? 2);
            if (maxBasement <= 0) maxBasement = 2;
            if (maxFloors <= 0) maxFloors = 5;

            int totalUnits = blocks.Sum(b => b.TotalApartments);
            if (totalUnits <= 0) totalUnits = project.TargetTotalApartments ?? (project.EskiDaireSayisi ?? 10);

            int totalShops = blocks.Sum(b => b.TotalShops);
            if (totalShops <= 0) totalShops = project.TargetTotalShops ?? (project.EskiDukkanSayisi ?? 0);

            // Toplam inşaat alanı
            double totalFloorArea = 0;
            if (blocks.Any(b => b.BaseArea.HasValue && b.BaseArea > 0))
            {
                totalFloorArea = blocks.Sum(b => (b.BaseArea ?? (baseArea / Math.Max(1, blocks.Count))) * ((b.TotalFloors ?? 5) + b.BasementFloors));
            }
            else
            {
                totalFloorArea = baseArea * maxFloors;
            }
            if (totalFloorArea <= 0) totalFloorArea = baseArea * 5;

            // Çevre uzunlukları
            double landPerimeter = landArea > 0 ? Math.Round(4 * Math.Sqrt(landArea), 1) : 240;
            double buildingPerimeter = baseArea > 0 ? Math.Round(4 * Math.Sqrt(baseArea), 1) : 120;

            // DNA Detayları
            var details = profile?.Details?.ToList() ?? new List<ProjectDnaDetail>();

            var zeminDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Zemin") || d.SelectedItem.Contains("Kaya") || d.SelectedItem.Contains("Kum") || d.SelectedItem.Contains("Kil"));
            string zeminTipi = zeminDetail != null ? zeminDetail.SelectedItem : "Yumuşak / Kumlu Zemin";
            double kabarma = zeminTipi.Contains("Kaya") ? 1.35 : 1.25;

            decimal ihataUzunlugu = (decimal)landPerimeter;
            var ihataLenDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Çevre Sınırı Toplam") || (d.SelectedItem.Contains("İhata") && d.SelectedItem.Contains(":")));
            if (ihataLenDetail != null && ihataLenDetail.Quantity > 0)
            {
                ihataUzunlugu = ihataLenDetail.Quantity;
            }
            else if (ihataLenDetail != null && ihataLenDetail.SelectedItem.Contains(":"))
            {
                var p = ihataLenDetail.SelectedItem.Split(':');
                if (p.Length > 1 && decimal.TryParse(p[1].Replace("m", "").Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal qVal))
                {
                    ihataUzunlugu = qVal;
                }
            }

            var ihataMatDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Sac") || d.SelectedItem.Contains("Tel Örgü") || d.SelectedItem.Contains("Pano"));
            string ihataMalzemesi = ihataMatDetail != null ? ihataMatDetail.SelectedItem : "Trapez Sac Kaplama";

            var kapiDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Kapı"));
            string kapiTipi = kapiDetail != null ? kapiDetail.SelectedItem : "Sürgülü Sac Kapı (Raylı)";

            var molozDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Yıkım") || d.SelectedItem.Contains("Moloz") || d.SelectedItem.Contains("Enkaz") || d.SelectedItem.Contains("Boş Arsa"));
            bool isCleanPlot = molozDetail != null && (molozDetail.SelectedItem.Contains("Boş") || molozDetail.SelectedItem.Contains("Gerekmez") || molozDetail.SelectedItem.Contains("Yok"));
            bool molozVar = molozDetail != null && !isCleanPlot && (molozDetail.SelectedItem.Contains("Yıkılacak") || molozDetail.SelectedItem.Contains("Moloz ve Çöp") || molozDetail.SelectedItem.Contains("Enkaz"));
            bool hasDemolition = !isCleanPlot && (molozVar || (project.EskiToplamMetrekare.HasValue && project.EskiToplamMetrekare.Value > 0) || (project.EskiKatSayisi.HasValue && project.EskiKatSayisi.Value > 0));

            int itemId = 1;

            // 1. Kazı ve Hafriyat
            decimal kaziM3 = Math.Round((decimal)(baseArea * (maxBasement * 3.2 + 1.0) * kabarma), 0);
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "01. Hafriyat & Saha Hazırlığı",
                Title = $"Derin Kazı ve Hafriyat Nakliyesi ({zeminTipi})",
                Description = $"Proje taban oturumu ({baseArea:N0} m²) ve {maxBasement} bodrum kat derinliğine göre hafriyat kazısı, döküm sahasına nakliyesi ve tesviyesi.",
                Formula = $"{baseArea:N0} m² Taban × {(maxBasement * 3.2 + 1.0):N1}m Derinlik × {kabarma} Kabarma",
                Quantity = kaziM3,
                Unit = "m³",
                ProfessionName = "Hafriyat ve Nakliye Ekibi",
                DnaSource = "Yapı Özeti + Adım 1 Zemin Seçimi"
            });

            // 2. Yıkım / Moloz
            if (hasDemolition)
            {
                double eskiAlan = project.EskiToplamMetrekare ?? (project.EskiBinaOturumAlani ?? (baseArea * 0.5));
                decimal molozM3 = Math.Round((decimal)(eskiAlan * 0.45), 0);
                if (molozM3 < 300) molozM3 = 450;
                list.Add(new ParametricTakeoffItemDto
                {
                    Id = itemId++,
                    Category = "01. Hafriyat & Saha Hazırlığı",
                    Title = "Eski Yapı Yıkımı, Enkaz Sökümü & Moloz Bertarafı",
                    Description = "Parselde bulunan mevcut yapının kontrollü yıkımı, demir ayrıştırma ve molozun belediye döküm sahasına nakliyesi.",
                    Formula = $"Mevcut Yapı Hacmi ({eskiAlan:N0} m²) × 0.45 m³/m² Moloz Katsayısı",
                    Quantity = molozM3,
                    Unit = "m³",
                    ProfessionName = "Yıkım ve Enkaz Taşeronu",
                    DnaSource = "Proje Künyesi + Adım 1 Yıkım/Moloz Seçimi"
                });
            }

            // 3. Çevre İhata Çiti
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "02. Şantiye Mobilizasyon & Güvenlik",
                Title = $"{ihataMalzemesi} ile Şantiye Çevre İhatası Kapatılması",
                Description = "Şantiye parsel sınırlarının iş güvenliği standartlarına uygun çit ve destek dikmeleri ile çevrilmesi.",
                Formula = $"Arsa Parsel Çevre Uzunluğu ({ihataUzunlugu:N0} mt)",
                Quantity = ihataUzunlugu,
                Unit = "mt",
                ProfessionName = "İhata / Çit Ustası",
                DnaSource = "Adım 1 Çevresel Faktör (İhata Metrajı)"
            });

            // 4. Şantiye Kapısı
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "02. Şantiye Mobilizasyon & Güvenlik",
                Title = $"{kapiTipi} (6.00m Ağır Vasıta Girişi)",
                Description = "Şantiye ana araç ve transmikser giriş kapısı, sürgülü ray mekanizması ve kilit sistemi montajı.",
                Formula = "1 Takım Çift Raylı Sürgülü Güvenlik Giriş Kapısı",
                Quantity = 1,
                Unit = "Takım",
                ProfessionName = "Demir Doğramacı",
                DnaSource = "Adım 1 Giriş Kapısı Seçimi"
            });

            // 5. Mobilizasyon Binaları
            int konteynerSayisi = Math.Max(2, Math.Min(6, (int)Math.Ceiling(totalFloorArea / 5000.0)));
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "02. Şantiye Mobilizasyon & Güvenlik",
                Title = "Şantiye Konteyner / Prefabrik Kampı Kurulumu",
                Description = "Şantiye yönetim ofisi, yemekhane, soyunma odası ve WC/duş konteynerlerinin sahaya nakliyesi ve teraziye alınması.",
                Formula = $"Toplam {totalFloorArea:N0} m² inşaat büyüklüğüne göre mobilizasyon ünitesi",
                Quantity = konteynerSayisi,
                Unit = "Adet",
                ProfessionName = "Prefabrik / Konteyner Tedarikçisi",
                DnaSource = "Yapı Özeti + Adım 1 Konaklama Seçimi"
            });

            // 6. İksa / Şoring
            decimal iksaM2 = Math.Round((decimal)(buildingPerimeter * (maxBasement * 3.2)), 0);
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "03. Zemin Güvenliği & İksa",
                Title = "Derin Kazı İksası & Güvenlik Perdesi (Püskürtme Beton / Mini Kazık)",
                Description = "Yol ve komşu parsellerin göçmesini önlemek için derin kazı şoring, zemin çivisi ve hasır çelikli püskürtme beton iksa imalatı.",
                Formula = $"{buildingPerimeter:N0} mt Bina Çevresi × {(maxBasement * 3.2):N1} mt Kazı Derinliği",
                Quantity = iksaM2,
                Unit = "m²",
                ProfessionName = "İksa ve Ankraj Ekibi",
                DnaSource = "Yapı Özeti + Adım 1 İksa Seçimi"
            });

            // 7. C20/25 Temel Grobetonu
            decimal grobetonM3 = Math.Round((decimal)(baseArea * 0.15), 0);
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "04. Temel & Yalıtım",
                Title = "C20/25 Pompalı Temel Altı Grobetonu (15 cm)",
                Description = "Radye temel altı membran bohçalama öncesi tesviye ve zemin koruma grobetonu dökümü.",
                Formula = $"{baseArea:N0} m² Taban Oturumu × 0.15m Kalınlık",
                Quantity = grobetonM3,
                Unit = "m³",
                ProfessionName = "Betoncu",
                DnaSource = "Yapı Özeti (Taban TAKS)"
            });

            // 8. Membran Bohçalama & Perde Su Yalıtımı
            decimal yalitimM2 = Math.Round((decimal)(baseArea + (buildingPerimeter * (maxBasement * 3.2))), 0);
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "04. Temel & Yalıtım",
                Title = "Çift Kat Bitümlü Membran Temel Bohçalama & Perde Yalıtımı",
                Description = "Radye temel altı ve bodrum dış perde duvarlarının yeraltı suyu ve neme karşı çift kat polyester keçe takviyeli membranla bohçalanması.",
                Formula = $"{baseArea:N0} m² Taban + {Math.Round(buildingPerimeter * maxBasement * 3.2, 0):N0} m² Perde Yüzeyi",
                Quantity = yalitimM2,
                Unit = "m²",
                ProfessionName = "İzolasyoncu / Bohçalama Ustası",
                DnaSource = "Yapı Özeti + Adım 2 Temel İzolasyonu"
            });

            // 9. C30/37 Taşıyıcı Hazır Beton
            decimal betonM3 = Math.Round((decimal)(totalFloorArea * 0.40), 0);
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "05. Taşıyıcı Kaba Yapı (Statik)",
                Title = "C30/37 Taşıyıcı Statik Hazır Beton (Radye, Kolon, Perde, Döşeme)",
                Description = "Bina taşıyıcı karkas sistemi için transmikser ve beton pompası ile dökülecek toplam hazır beton metrajı.",
                Formula = $"{totalFloorArea:N0} m² Toplam İnşaat Alanı × 0.40 m³/m² Katsayısı",
                Quantity = betonM3,
                Unit = "m³",
                ProfessionName = "Betoncu",
                DnaSource = "Yapı Özeti + Adım 0 Taşıyıcı Sabitleri"
            });

            // 10. Nervürlü Donatı Çeliği
            decimal demirTon = Math.Round((decimal)(totalFloorArea * 0.034), 1);
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "05. Taşıyıcı Kaba Yapı (Statik)",
                Title = "B420C Nervürlü Donatı Çeliği (Q8-Q26 Kalın/Orta/İnce Demir)",
                Description = "Temel, perde, kolon, kiriş ve döşemelerde kullanılacak nervürlü inşaat demiri (etriye, boy donatı, pilye).",
                Formula = $"{totalFloorArea:N0} m² Toplam İnşaat Alanı × 34 kg/m² Donatı Oranı",
                Quantity = demirTon,
                Unit = "Ton",
                ProfessionName = "Demirci",
                DnaSource = "Yapı Özeti + Adım 0 Donatı Sabitleri"
            });

            // 11. Kalıp ve İskele İmalatı
            decimal kalipM2 = Math.Round((decimal)(totalFloorArea * 2.5), 0);
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "05. Taşıyıcı Kaba Yapı (Statik)",
                Title = "Endüstriyel Kalıp ve İskele Sistemi Kurulum & Söküm İşçiliği",
                Description = "Perde, kolon, kiriş ve asmolen/kaset döşeme kalıplarının projeye uygun kurulması, yağlanması ve sökümü.",
                Formula = $"{totalFloorArea:N0} m² Toplam İnşaat Alanı × 2.5 m²/m² Kalıp Yüzeyi",
                Quantity = kalipM2,
                Unit = "m²",
                ProfessionName = "Kalıpçı",
                DnaSource = "Yapı Özeti + Adım 0 Kalıp Sabitleri"
            });

            // 12. Dış Cephe Güvenlikli İskele
            decimal iskeleM2 = Math.Round((decimal)(buildingPerimeter * (maxFloors * 3.0)), 0);
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "06. Dış Cephe & İnce İmalatlar",
                Title = "TSE Belgeli Flanşlı Güvenlikli Dış Cephe İskelesi (Kurulum & Kiralama)",
                Description = "Bina dış cephesine kurulacak tam güvenlikli, merdivenli ve fileli çalışma iskelesi.",
                Formula = $"{buildingPerimeter:N0} mt Çevre × {(maxFloors * 3.0):N0} mt Bina Yüksekliği",
                Quantity = iskeleM2,
                Unit = "m²",
                ProfessionName = "İskeleci",
                DnaSource = "Yapı Özeti (Kat & Yükseklik)"
            });

            // 13. Dış Cephe Isı Mantolama
            decimal mantoM2 = Math.Round((decimal)(iskeleM2 * 0.85m), 0);
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "06. Dış Cephe & İnce İmalatlar",
                Title = "Taşyünü / EPS Dış Cephe Isı Mantolama & Mineral Sıva",
                Description = "Dış cephe ısı yalıtım levhası montajı, dubelleme, fileli sıva ve dekoratif mineral sıva kaplaması.",
                Formula = $"{iskeleM2:N0} m² Brüt Cephe × 0.85 (Pencere/Boşluk Düşülmüş Net)",
                Quantity = mantoM2,
                Unit = "m²",
                ProfessionName = "Dış Cephe Ustası",
                DnaSource = "Yapı Özeti + Adım 3 Gövde Seçimi"
            });

            // 14. Tesviye Şapı
            decimal sapM2 = Math.Round((decimal)(totalFloorArea * 0.82), 0);
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "06. Dış Cephe & İnce İmalatlar",
                Title = "Helikopterli ve Mastarlı Zemin Tesviye Şapı",
                Description = "Daire içi kuru ve ıslak hacimlerde izolasyon ve döşeme kaplaması öncesi helikopterli tesviye şapı dökümü.",
                Formula = $"{totalFloorArea:N0} m² İnşaat Alanı × 0.82 (Net Döşeme Alanı)",
                Quantity = sapM2,
                Unit = "m²",
                ProfessionName = "Şapçı",
                DnaSource = "Yapı Özeti + Adım 0 Şap Sabitleri"
            });

            // 15. Şantiye Elektriği
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "07. Geçici MEP & Şantiye Altyapısı",
                Title = "Şantiye Geçici Enerji, Dağıtım Panosu & Çevre Aydınlatma",
                Description = "Ana şantiye elektrik panosu, kaçak akım röleleri, kat tali panoları ve gece güvenlik aydınlatması kurulumu.",
                Formula = "Şantiye Geçici Elektrik & Pano Paketi",
                Quantity = 1,
                Unit = "Set",
                ProfessionName = "Şantiye Elektrikçisi",
                DnaSource = "Adım 1 Şantiye Elektriği Seçimi"
            });

            // 16. Şantiye Suyu
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "07. Geçici MEP & Şantiye Altyapısı",
                Title = "15 Tonluk Polietilen Şantiye Su Deposu & 3 HP Otomatik Hidrofor",
                Description = "Beton kürü, temizlik ve kullanım suyu için şantiye su deposu, otomatik hidrofor ve dağıtım borulama tesisatı.",
                Formula = "Şantiye Su Depolama & Basınçlandırma Paketi",
                Quantity = 1,
                Unit = "Set",
                ProfessionName = "Sıhhi Tesisatçı",
                DnaSource = "Adım 1 Şantiye Suyu Seçimi"
            });

            // 17. Ankastre 3'lü Set (Adım 4)
            var ankastreDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Ankastre") || d.SelectedItem.Contains("Fırın") || d.SelectedItem.Contains("Ocak"));
            string ankastreTitle = ankastreDetail != null ? ankastreDetail.SelectedItem : "Ankastre 3'lü Set (Fırın + Ocak + Davlumbaz)";
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "08. İç Mekan, Mutfak & Ahşap İmalatları",
                Title = $"{ankastreTitle} Temini ve Montajı",
                Description = "Daireler için bağımsız ankastre fırın, cam ocak ve davlumbaz temini, elektrik/gaz bağlantısı ve montajı.",
                Formula = $"{totalUnits} Bağımsız Bölüm (Daire) × 1 Takım",
                Quantity = totalUnits,
                Unit = "Set",
                ProfessionName = "Mutfak & Mobilya Ekibi",
                DnaSource = "Adım 4 Soru 8: Ankastre Set Standardı"
            });

            // 18. Giriş Portmanto / Vestiyer Dolabı (Adım 4)
            var vestiyerDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Portmanto") || d.SelectedItem.Contains("Vestiyer"));
            string vestiyerTitle = vestiyerDetail != null ? vestiyerDetail.SelectedItem : "Giriş Holü Portmanto / Vestiyer Dolabı";
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "08. İç Mekan, Mutfak & Ahşap İmalatları",
                Title = $"{vestiyerTitle} İmalat ve Montajı",
                Description = "Daire giriş hollerine projesine uygun gömme veya modüler vestiyer, ayakkabılık ve ayna montajı.",
                Formula = $"{totalUnits} Daire Giriş Holü × 1 Adet",
                Quantity = totalUnits,
                Unit = "Adet",
                ProfessionName = "Mutfak & Mobilya Ekibi",
                DnaSource = "Adım 4 Soru 10: Portmanto / Vestiyer Standardı"
            });

            // 19. Parapet Harpuştası & Çatı Çıkış Kapağı (Adım 5)
            var harpushtaDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Harpuşta") || d.SelectedItem.Contains("Parapet"));
            string harpushtaTitle = harpushtaDetail != null ? harpushtaDetail.SelectedItem : "Mermer / Granit Parapet Harpuştası";
            decimal harpushtaMt = Math.Round((decimal)(buildingPerimeter * 1.10), 0);
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "09. Çatı, Teras & Harpuşta",
                Title = $"{harpushtaTitle} & Emniyetli Çatı Çıkış Kapağı",
                Description = "Teras ve çatı parapet duvarlarının su sızdırmaz damlalıklı mermer/granit ile kaplanması ve çatı çıkış kapağı.",
                Formula = $"{buildingPerimeter:N0} mt Bina Çevresi × 1.10 Parapet Gelişimi",
                Quantity = harpushtaMt,
                Unit = "mt",
                ProfessionName = "Mermer & Doğaltaş Ustası",
                DnaSource = "Adım 5 Soru 6: Parapet Harpuştası ve Çatı Çıkış Güvenliği"
            });

            // 20. Pencere Denizlikleri (Adım 6)
            var denizlikDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Denizlik"));
            string denizlikTitle = denizlikDetail != null ? denizlikDetail.SelectedItem : "3cm Kalınlığında Doğaltaş / Mermer Pencere Denizlikleri";
            decimal denizlikMt = totalUnits * 7;
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "10. Doğrama, Denizlik & Balkon Korkulukları",
                Title = $"{denizlikTitle} (İç ve Dış Denizlikler)",
                Description = "Pencere altlarına pahlı, damlalıklı iç ve dış mermer/granit denizlik montajı.",
                Formula = $"{totalUnits} Daire × 5 Pencere × 1.4 mt Ortalama Açıklık",
                Quantity = denizlikMt,
                Unit = "mt",
                ProfessionName = "Mermer & Doğaltaş Ustası",
                DnaSource = "Adım 6 Soru 6: Pencere İç ve Dış Denizlik Malzemesi"
            });

            // 21. Balkon ve Fransız Balkon Korkulukları (Adım 6)
            var korkulukDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Korkuluk"));
            string korkulukTitle = korkulukDetail != null ? korkulukDetail.SelectedItem : "Alüminyum & Lamine Cam Balkon Korkuluk Sistemi";
            decimal korkulukMt = totalUnits * 6;
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "10. Doğrama, Denizlik & Balkon Korkulukları",
                Title = $"{korkulukTitle} İmalat ve Montajı",
                Description = "Balkon ve Fransız pencereler için statik hesaplı küpeşteli güvenlik korkulukları.",
                Formula = $"{totalUnits} Daire × 6.0 mt Ortalama Balkon/Fransız Çevresi",
                Quantity = korkulukMt,
                Unit = "mt",
                ProfessionName = "Korkuluk & Metal Ustası",
                DnaSource = "Adım 6 Soru 7: Balkon ve Fransız Balkon Korkuluk Sistemi"
            });

            // 22. Bina İçi Sessiz Atık Su / Pis Su Boruları (Adım 7)
            var pisuDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Pis Su") || d.SelectedItem.Contains("Atık Su") || d.SelectedItem.Contains("Sessiz"));
            string pisuTitle = pisuDetail != null ? pisuDetail.SelectedItem : "Mineral Takviyeli Sessiz Atık Su Boru Sistemi";
            decimal pisuMt = totalUnits * 25;
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "11. Mekanik, Sıhhi Tesisat & Yangın (MEP)",
                Title = $"{pisuTitle} Düşey Kolon ve Daire Tesisatı",
                Description = "Katlar arası dikey ana iniş şaftları ve daire içi ıslak hacimlerin sızdırmaz, ses yalıtımlı pis su borulaması.",
                Formula = $"{totalUnits} Daire × 25 mt (Şaft ve Daire İçi Dağıtım)",
                Quantity = pisuMt,
                Unit = "mt",
                ProfessionName = "Sıhhi Tesisatçı",
                DnaSource = "Adım 7 Soru 6: Atık Su / Pis Su Borusu Standardı"
            });

            // 23. Su Deposu & Hidrofor (Adım 7)
            var suDeposuDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Su Deposu") || d.SelectedItem.Contains("Hidrofor"));
            string suDeposuTitle = suDeposuDetail != null ? suDeposuDetail.SelectedItem : "AISI 304 Paslanmaz Çelik Modüler Su Deposu & Çift Pompalı Hidrofor";
            int depoTakim = Math.Max(1, blocks.Count);
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "11. Mekanik, Sıhhi Tesisat & Yangın (MEP)",
                Title = $"{suDeposuTitle} Sistemi",
                Description = "Yangın ve kullanım suyu rezervi için paslanmaz modüler depo, frekans invertörlü hidrofor ve vana grubu.",
                Formula = $"{depoTakim} Blok / Yapı × 1 Takım Merkezi Basınçlandırma Paketi",
                Quantity = depoTakim,
                Unit = "Takım",
                ProfessionName = "Sıhhi Tesisatçı",
                DnaSource = "Adım 7 Soru 7: Temiz Su Deposu ve Hidrofor Sistemi"
            });

            // 24. Yangın Merdiveni Basamak Kaplaması (Adım 8)
            var yanginBasamakDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Yangın Merdiveni Basamak") || d.SelectedItem.Contains("Yangın Merdiveni Kaplama") || d.SelectedItem.Contains("Traverten"));
            string yanginBasamakTitle = yanginBasamakDetail != null ? yanginBasamakDetail.SelectedItem : "Doğaltaş / Traverten Yangın Merdiveni Basamak Kaplaması";
            decimal yanginBasamakMt = maxFloors * 22;
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "12. Ortak Alanlar, Yangın Güvenliği & Sirkülasyon",
                Title = yanginBasamakTitle,
                Description = "Acil kaçış ve yangın merdiveni basamaklarının kaymaz profil traverten/mermer veya epoksi kaplaması.",
                Formula = $"{maxFloors} Kat × 18 Basamak × 1.2 mt Rıht/Basamak Ölçüsü",
                Quantity = yanginBasamakMt,
                Unit = "mt",
                ProfessionName = "Mermer & Doğaltaş Ustası",
                DnaSource = "Adım 8 Soru 6: Yangın Merdiveni Basamak Kaplaması"
            });

            // 24B. Yangın Merdiveni Korkuluk & Küpeştesi (Adım 8)
            var yanginKorkulukDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Yangın Merdiveni Korkuluk") || d.SelectedItem.Contains("Kaçış Küpeşte"));
            string yanginKorkulukTitle = yanginKorkulukDetail != null ? yanginKorkulukDetail.SelectedItem : "Elektrostatik Boyalı Demir Boru Yangın Korkuluğu";
            decimal yanginKorkulukMt = maxFloors * 22;
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "12. Ortak Alanlar, Yangın Güvenliği & Sirkülasyon",
                Title = yanginKorkulukTitle,
                Description = "Yangın merdiveni kovası emniyet korkuluğu ve yangına dayanıklı kaçış tutamağı montajı.",
                Formula = $"{maxFloors} Kat × 22 mt Merdiven İniş Hattı",
                Quantity = yanginKorkulukMt,
                Unit = "mt",
                ProfessionName = "Korkuluk & Metal Ustası",
                DnaSource = "Adım 8 Soru 7: Yangın Merdiveni Korkuluk Standardı"
            });

            // 24C. Kat Holü Asansör Söve ve Çerçeve Kaplaması (Adım 8)
            var asansorSoveDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Asansör Söve") || d.SelectedItem.Contains("Kat Holü Asansör"));
            string asansorSoveTitle = asansorSoveDetail != null ? asansorSoveDetail.SelectedItem : "Mermer / Granit Asansör Kapı Sövesi";
            int asansorKapiAdet = maxFloors;
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "12. Ortak Alanlar, Yangın Güvenliği & Sirkülasyon",
                Title = asansorSoveTitle,
                Description = "Her kat koridorunda asansör kapı çevrelerinin mermer, granit veya kompozit söve ile giydirilmesi.",
                Formula = $"{asansorKapiAdet} Kat × 1 Kapı Açıklığı",
                Quantity = asansorKapiAdet,
                Unit = "Adet",
                ProfessionName = "Mermer & Doğaltaş Ustası",
                DnaSource = "Adım 8 Soru 5: Kat Holü Asansör Sövesi"
            });

            // 25. Yangın Kaçış Kapıları (Adım 8)
            var yanginKapiDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Yangın Kaçış Kapı") || d.SelectedItem.Contains("Yangın Merdiveni ve Acil Kaçış") || d.SelectedItem.Contains("EI90") || d.SelectedItem.Contains("EI60"));
            string yanginKapiTitle = yanginKapiDetail != null ? yanginKapiDetail.SelectedItem : "EI90 Panik Barlı Çift Cidarlı Yangın Kaçış Kapısı";
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "12. Ortak Alanlar, Yangın Güvenliği & Sirkülasyon",
                Title = $"{yanginKapiTitle} (Duman Sızdırmaz Fitilli)",
                Description = "Kat koridorlarından yangın merdivenine açılan panik barlı, yaylı menteşeli ve duman fitilli sertifikalı yangın kapıları.",
                Formula = $"{maxFloors} Kat × 1 Adet Kat Yangın Çıkış Kapısı",
                Quantity = maxFloors,
                Unit = "Adet",
                ProfessionName = "Kapı ve Ahşap Montaj Ekibi",
                DnaSource = "Adım 8 Soru 7: Yangın Merdiveni Acil Kaçış Kapıları"
            });

            // 26. Kat Yangın Dolapları & Ekipmanları (Adım 8)
            var yanginDolapDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Yangın Dolabı") || d.SelectedItem.Contains("Yangın Hortumu"));
            string yanginDolapTitle = yanginDolapDetail != null ? yanginDolapDetail.SelectedItem : "Tüp Bölmeli Gömme Tip Kat Yangın Dolabı (TSE EN 671-1)";
            list.Add(new ParametricTakeoffItemDto
            {
                Id = itemId++,
                Category = "12. Ortak Alanlar, Yangın Güvenliği & Sirkülasyon",
                Title = $"{yanginDolapTitle} ve Lans Takımı",
                Description = "Her kat koridorunda 30m yarı sert yangın hortumlu, vanalı ve 6 kg KKT yangın söndürme tüplü dolap montajı.",
                Formula = $"{maxFloors} Kat × 1 Adet Kat Yangın Dolabı",
                Quantity = maxFloors,
                Unit = "Adet",
                ProfessionName = "Yangın Güvenlik & Mekanik Ekibi",
                DnaSource = "Adım 8 Soru 8: Kat Yangın Dolabı ve Ekipman Modeli"
            });

            // 27+. Dış Saha ve Sosyal Donatılar (Adım 9)
            if (project.Amenities != null && project.Amenities.Any())
            {
                foreach (var am in project.Amenities)
                {
                    decimal amQty = (decimal)(am.SquareMeters ?? 1);
                    if (amQty <= 0) amQty = 1;
                    string unit = (am.SquareMeters.HasValue && am.SquareMeters.Value > 0) ? "m²" : "Adet";

                    list.Add(new ParametricTakeoffItemDto
                    {
                        Id = itemId++,
                        Category = "13. Dış Saha, Sosyal Donatılar & Çevre Düzenleme",
                        Title = $"{am.Name} İmalat ve Düzenlemesi",
                        Description = $"{am.Type ?? "Sosyal Donatı"} kapsamında zemin kaplaması, ekipman montajı ve çevre düzenlemesi.",
                        Formula = $"Proje Donatı Envanteri: {am.Name} ({amQty:N0} {unit})",
                        Quantity = amQty,
                        Unit = unit,
                        ProfessionName = "Peyzaj ve Dış Saha Düzenleme Ustası",
                        DnaSource = $"Adım 9: Donatı Envanteri ({am.Type ?? "Sosyal Donatı"})"
                    });
                }
            }

            return list;
        }

        private List<WorkflowStepItemDto> GenerateParametricWorkflow(ConstructionProject project, ProjectDnaProfile profile, DateTime? baseDate = null, int? buildingId = null)
        {
            var list = new List<WorkflowStepItemDto>();
            if (project == null) return list;

            var allBlocks = project.Blocks?.Where(b => !b.IsExistingBuilding).ToList() ?? new List<GMK360.Core.Entities.Building>();
            var rootBlocks = allBlocks.Where(b => b.ParentBuildingId == null).ToList();

            GMK360.Core.Entities.Building targetStructure = null;
            if (buildingId.HasValue && buildingId.Value > 0)
            {
                targetStructure = allBlocks.FirstOrDefault(b => b.Id == buildingId.Value);
            }
            if (targetStructure == null)
            {
                targetStructure = rootBlocks.FirstOrDefault() ?? allBlocks.FirstOrDefault();
            }

            var targetBlocks = new List<GMK360.Core.Entities.Building>();
            if (targetStructure != null)
            {
                var children = allBlocks.Where(b => b.ParentBuildingId == targetStructure.Id).ToList();
                if (children.Any())
                {
                    targetBlocks.AddRange(children);
                }
                else
                {
                    targetBlocks.Add(targetStructure);
                }
            }
            else
            {
                targetBlocks = allBlocks;
            }

            string targetStructureName = targetStructure?.BlockName ?? targetStructure?.Name ?? (targetBlocks.Count == 1 ? (targetBlocks[0].BlockName ?? targetBlocks[0].Name) : project.Name);
            int? targetBuildingId = targetStructure?.Id ?? targetBlocks.FirstOrDefault()?.Id;

            double landArea = project.TotalLandArea ?? 0;
            double baseArea = targetStructure?.BaseArea ?? targetBlocks.Sum(b => b.BaseArea ?? 0);
            if (baseArea <= 0 && targetBlocks.Any()) baseArea = targetBlocks.Sum(b => b.BaseArea ?? 0);
            if (baseArea <= 0) baseArea = project.EskiBinaOturumAlani ?? (landArea > 0 ? landArea * 0.40 : 500);

            int maxFloors = targetBlocks.Any() ? targetBlocks.Max(b => (b.TotalFloors ?? 0) + b.BasementFloors) : (project.TotalFloors ?? 5);
            if (targetStructure != null && targetStructure.TotalFloors.HasValue && targetStructure.TotalFloors.Value > 0)
            {
                int structFloors = (targetStructure.TotalFloors.Value) + targetStructure.BasementFloors;
                if (structFloors > maxFloors) maxFloors = structFloors;
            }

            int maxBasement = targetBlocks.Any() ? targetBlocks.Max(b => b.BasementFloors) : (project.EskiBodrumKatSayisi ?? 2);
            if (targetStructure != null && targetStructure.BasementFloors > maxBasement) maxBasement = targetStructure.BasementFloors;
            if (maxBasement <= 0) maxBasement = 2;
            if (maxFloors <= 0) maxFloors = 5;

            double totalFloorArea = 0;
            if (targetBlocks.Any(b => b.BaseArea.HasValue && b.BaseArea > 0))
            {
                totalFloorArea = targetBlocks.Sum(b => (b.BaseArea ?? (baseArea / Math.Max(1, targetBlocks.Count))) * ((b.TotalFloors ?? 5) + b.BasementFloors));
            }
            else
            {
                totalFloorArea = baseArea * maxFloors;
            }
            if (totalFloorArea <= 0) totalFloorArea = baseArea * maxFloors;

            double landPerimeter = landArea > 0 ? Math.Round(4 * Math.Sqrt(landArea), 1) : 240;
            double buildingPerimeter = baseArea > 0 ? Math.Round(4 * Math.Sqrt(baseArea), 1) : 120;

            int totalUnits = targetBlocks.Sum(b => b.TotalApartments);
            if (totalUnits <= 0) totalUnits = targetStructure?.TotalApartments ?? (project.TargetTotalApartments ?? (project.EskiDaireSayisi ?? 10));

            var details = profile?.Details?.ToList() ?? new List<ProjectDnaDetail>();

            var zeminDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Zemin") || d.SelectedItem.Contains("Kaya") || d.SelectedItem.Contains("Kum") || d.SelectedItem.Contains("Kil"));
            string zeminTipi = zeminDetail != null ? zeminDetail.SelectedItem : "Yumuşak / Kumlu Zemin";
            double kabarma = zeminTipi.Contains("Kaya") ? 1.35 : 1.25;

            decimal ihataUzunlugu = (decimal)landPerimeter;
            var ihataLenDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Çevre Sınırı Toplam") || (d.SelectedItem.Contains("İhata") && d.SelectedItem.Contains(":")));
            if (ihataLenDetail != null && ihataLenDetail.Quantity > 0)
            {
                ihataUzunlugu = ihataLenDetail.Quantity;
            }
            else if (ihataLenDetail != null && ihataLenDetail.SelectedItem.Contains(":"))
            {
                var p = ihataLenDetail.SelectedItem.Split(':');
                if (p.Length > 1 && decimal.TryParse(p[1].Replace("m", "").Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal qVal))
                {
                    ihataUzunlugu = qVal;
                }
            }

            var ihataMatDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Sac") || d.SelectedItem.Contains("Tel Örgü") || d.SelectedItem.Contains("Pano"));
            string ihataMalzemesi = ihataMatDetail != null ? ihataMatDetail.SelectedItem : "Trapez Sac Kaplama";

            var kapiDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Kapı"));
            string kapiTipi = kapiDetail != null ? kapiDetail.SelectedItem : "Sürgülü Raylı Sac Kapı (6m)";

            var molozDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Yıkım") || d.SelectedItem.Contains("Moloz") || d.SelectedItem.Contains("Enkaz") || d.SelectedItem.Contains("Boş Arsa"));
            bool isCleanPlot = molozDetail != null && (molozDetail.SelectedItem.Contains("Boş") || molozDetail.SelectedItem.Contains("Gerekmez") || molozDetail.SelectedItem.Contains("Yok"));
            bool molozVar = molozDetail != null && !isCleanPlot && (molozDetail.SelectedItem.Contains("Yıkılacak") || molozDetail.SelectedItem.Contains("Moloz ve Çöp") || molozDetail.SelectedItem.Contains("Enkaz"));
            bool hasDemolition = !isCleanPlot && (molozVar || (project.EskiToplamMetrekare.HasValue && project.EskiToplamMetrekare.Value > 0) || (project.EskiKatSayisi.HasValue && project.EskiKatSayisi.Value > 0));

            var elektrikDetail = details.FirstOrDefault(d => (d.Phase.Contains("1") || d.Phase.Contains("Faz 1") || d.Phase.Contains("Adım 1")) && (d.SelectedItem.Contains("Elektrik") || d.SelectedItem.Contains("Abonelik") || d.SelectedItem.Contains("Trafo") || d.SelectedItem.Contains("Jeneratör")));
            string elektrikTipi = elektrikDetail != null ? elektrikDetail.SelectedItem : "Şantiye Şebeke Aboneliği ve TMŞ Dağıtım Panosu";

            var suDetail = details.FirstOrDefault(d => (d.Phase.Contains("1") || d.Phase.Contains("Faz 1") || d.Phase.Contains("Adım 1")) && (d.SelectedItem.Contains("Su") || d.SelectedItem.Contains("Depo") || d.SelectedItem.Contains("Hidrofor") || d.SelectedItem.Contains("Kuyu")));
            string suTipi = suDetail != null ? suDetail.SelectedItem : "15 Tonluk Su Deposu ve 3 HP Hidrofor";

            int konteynerSayisi = Math.Max(2, Math.Min(6, (int)Math.Ceiling(totalFloorArea / 5000.0)));
            decimal kaziM3 = Math.Round((decimal)(baseArea * (maxBasement * 3.2 + 1.0) * kabarma), 0);
            decimal iksaM2 = Math.Round((decimal)(buildingPerimeter * (maxBasement * 3.2)), 0);
            decimal grobetonM3 = Math.Round((decimal)(baseArea * 0.15), 0);
            decimal yalitimM2 = Math.Round((decimal)(baseArea + (buildingPerimeter * (maxBasement * 3.2))), 0);
            decimal temelBetonM3 = Math.Round((decimal)(baseArea * 0.70), 0);
            if (temelBetonM3 < 50) temelBetonM3 = 100;
            decimal temelDemirTon = Math.Round((decimal)(temelBetonM3 * 0.085m), 1);

            decimal karkasBetonM3 = Math.Round((decimal)(totalFloorArea * 0.38), 0);
            decimal karkasDemirTon = Math.Round((decimal)(totalFloorArea * 0.032), 1);

            decimal sapM2 = Math.Round((decimal)(totalFloorArea * 0.82), 0);
            decimal iskeleM2 = Math.Round((decimal)(buildingPerimeter * (maxFloors * 3.0)), 0);
            decimal mantoM2 = Math.Round((decimal)(iskeleM2 * 0.85m), 0);
            decimal harpushtaMt = Math.Round((decimal)(buildingPerimeter * 1.10), 0);
            decimal denizlikMt = totalUnits * 7;
            decimal korkulukMt = totalUnits * 6;
            decimal pisuMt = totalUnits * 25;
            var pisuDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Pis Su") || d.SelectedItem.Contains("Atık Su") || d.SelectedItem.Contains("Sessiz"));
            string pisuTitle = pisuDetail != null ? pisuDetail.SelectedItem : "Sessiz Atık Su Boru Sistemi";

            decimal landscapeArea = (decimal)(project.LandscapeArea ?? (landArea * 0.4));
            DateTime curDate = (baseDate ?? DateTime.Today).Date;

            // 4 Parça Hiyerarşi Sabitleri
            int p1Idx = 1;
            string p1Name = "1. Parça: Kaba İnşaat (Temel & Taşıyıcı İskelet)";
            string p1Badge = "primary";
            string p1Trigger = "İşbaşı / Şantiye Kuruluşu ile Başlar";

            int p2Idx = 2;
            string p2Name = "2. Parça: Statik İnce (Kaba İnce / Kovalayan İşler)";
            string p2Badge = "info";
            string p2Trigger = "⚡ Kovalama Kuralı: Kaba 2 kat önden gidince alttan kat kat başlar!";

            int p3Idx = 3;
            string p3Name = "3. Parça: Tercihe Göre İnce İşler (Daire İçi İmalatlar)";
            string p3Badge = "warning";
            string p3Trigger = "⚡ Tetikleyici: Kat kaba duvarları ve tesisat sızdırmazlık testi bitince başlar!";

            int p4Idx = 4;
            string p4Name = "4. Parça: Demirbaş, Montaj & Dış Çevre (Teslim Aşaması)";
            string p4Badge = "success";
            string p4Trigger = "⚡ Tetikleyici: Daire içi boya ve zemin kaplamaları bitince temiz montaja geçilir!";

            // Helper metot: Alt adımları temiz (mokapsız) oluşturur
            List<WorkflowSubStepDto> CreateSubSteps(string prePrep, string exec, string quality, string isg)
            {
                return new List<WorkflowSubStepDto>
                {
                    new WorkflowSubStepDto { Title = prePrep, Category = "PrePreparation", IsCompleted = false },
                    new WorkflowSubStepDto { Title = exec, Category = "Execution", IsCompleted = false },
                    new WorkflowSubStepDto { Title = quality, Category = "QualityAndCuring", IsCompleted = false },
                    new WorkflowSubStepDto { Title = isg, Category = "SafetyIsg", IsCompleted = false }
                };
            }

            int stepOrderSeq = 1;
            DateTime runDate = curDate;

            // =========================================================================
            // 1. PARÇA: KABA İNŞAAT (TEMEL & TAŞIYICI İSKELET)
            // =========================================================================

            // K-01: Harita, Aplikasyon & Röper Alma (Adım 1)
            var k01Start = runDate;
            var k01End = k01Start.AddDays(2);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "K-01",
                Order = stepOrderSeq++,
                PhaseName = "Harita, Aplikasyon & Röper Alma",
                StageCategory = "Saha Hazırlığı & Ölçüm",
                PartIndex = p1Idx,
                PartName = p1Name,
                PartBadgeClass = p1Badge,
                TriggerRule = "İşbaşı ile başlar",
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "Ahşap Kazıklar, Kireç Tozu ve Çelik Röper Çubukları",
                MaterialDescription = "Arsa köşe poligonu, köşe kotları ve hafriyat sınırlarının kireçle işaretlenmesi.",
                MaterialQuantity = 0,
                MaterialUnit = "Takım",
                MaterialFormula = "DXF Bekliyor (Mimari vaziyet ve köşe koordinatlarından belirlenecek)",
                SupplierCategory = "Harita Mühendislik Ofisi / Topoğraf Ekipmanı",
                LaborTitle = "Harita Mühendisi & Topoğraf Ekibi",
                ProfessionName = "Harita Mühendisi / Topoğraf",
                CrewComposition = "1 Harita Müh. + 1 Topoğraf + 2 Yardımcı/Amele",
                CrewSize = 4,
                DurationDays = 2,
                EquipmentNeeded = "Total Station, Nivo, Lazer Metre, GPS Cihazı",
                Predecessors = "İmar çapı ve mimari vaziyet planı kabulü",
                CriticalDependency = "Köşe kazıkları çakılmadan ve röper alınmadan çevre ihata yapılamaz, hafriyat başlayamaz!",
                DnaSource = "Adım 1: Saha Analizi & Aplikasyon",
                PlannedStartDate = k01Start,
                PlannedEndDate = k01End,
                SubSteps = CreateSubSteps(
                    "T-2 Belediye imar çapı, aplikasyon krokisi ve köşe koordinat listesi temini",
                    "Total Station ve GPS ile parsel köşe kazıklarının çakılması ve bina taban sınır kireçlemesi",
                    "İmar çekme mesafelerinin (ön/yan/arka bahçe) ve sabit röper kotunun doğrulanması",
                    "Saha çevre güvenlik hattı ve komşu parsel sınır reflektörlü şeritlerinin çekilmesi"
                )
            });

            // K-02: Şantiye Çevre İhatası & Raylı Güvenlik Giriş Kapısı (Adım 1)
            var k02Start = k01End;
            var k02End = k02Start.AddDays(4);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "K-02",
                Order = stepOrderSeq++,
                PhaseName = "Şantiye Çevre İhatası & Raylı Güvenlik Giriş Kapısı",
                StageCategory = "Saha Güvenliği & Çevreleme",
                PartIndex = p1Idx,
                PartName = p1Name,
                PartBadgeClass = p1Badge,
                TriggerRule = "Aplikasyon sınırlarına göre başlar",
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = $"{ihataMalzemesi} ve {kapiTipi}",
                MaterialDescription = "Şantiye parsel çevresinin kapatılması, dikme direkleri, payandalar ve ana araç giriş kapısı.",
                MaterialQuantity = 0,
                MaterialUnit = "mt",
                MaterialFormula = "DXF Bekliyor (Parsel çevre sınır ölçüsünden hesaplanacak)",
                SupplierCategory = "Demir Çelik / Çit & Sac Bayisi",
                LaborTitle = "Demir Doğrama ve Çit Montaj Ekibi",
                ProfessionName = "Demir / Çit Ustası",
                CrewComposition = "1 Demir Doğrama Formeni + 2 Kaynakçı + 2 Montaj İşçisi",
                CrewSize = 5,
                DurationDays = 4,
                EquipmentNeeded = "Kaynak Makinesi, Profil Kesme, Matkap Takımları",
                Predecessors = "Harita aplikasyon sınır tespiti",
                CriticalDependency = "İşyeri ve belediye denetimi gereği çevre ihatası ve kapı olmadan şantiyeye makine sokulamaz!",
                DnaSource = "Adım 1: Şantiye Çevre İhatası",
                PlannedStartDate = k02Start,
                PlannedEndDate = k02End,
                SubSteps = CreateSubSteps(
                    "T-2 İhata panoları, ankraj profilleri ve raylı kapı mekanizmasının şantiyeye sevki",
                    "Direk ankrajlarının sabitlenmesi, çevre panolarının montajı ve raylı giriş kapısı montajı",
                    "Rüzgar fırtına yüküne karşı payanda rijitliği ve kilit sistemi kontrolü",
                    "Şantiye tanıtım tabelası, 'Yetkisiz Girilmez' levhaları ve acil toplanma noktası tabelası"
                )
            });

            // K-03: Şantiye Geçici Elektrik, Su Altyapısı & Mobilizasyon (Adım 1)
            var k03Start = k02End;
            var k03End = k03Start.AddDays(3);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "K-03",
                Order = stepOrderSeq++,
                PhaseName = "Şantiye Geçici Elektrik, Su Altyapısı & Mobilizasyon",
                StageCategory = "Mobilizasyon & Altyapı",
                PartIndex = p1Idx,
                PartName = p1Name,
                PartBadgeClass = p1Badge,
                TriggerRule = "Çevreleme tamamlanınca başlar",
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = $"{elektrikTipi}, {suTipi} ve Şantiye Konteyner Kampı",
                MaterialDescription = "Şantiye ana dağıtım panosu, sayaç/kofre, su deposu ve hidrofor sistemi, şantiye ofis ve yemekhanesi.",
                MaterialQuantity = 0,
                MaterialUnit = "Set",
                MaterialFormula = "DXF Bekliyor (Şantiye yerleşim planından netleşecek)",
                SupplierCategory = "Şantiye Altyapı & Konteyner Tedarikçisi",
                LaborTitle = "Elektrik & Tesisat Altyapı Ekibi",
                ProfessionName = "Şantiye Elektrik & Tesisatçısı",
                CrewComposition = "1 Elektrik Ustası + 1 Tesisat Ustası + 1 Mobil Vinç Operatörü + 2 Yardımcı",
                CrewSize = 5,
                DurationDays = 3,
                EquipmentNeeded = "Mobil Vinç, Topraklama Ölçüm Cihazı, Boru Kaynak Makinesi",
                Predecessors = "Çevre ihatası ve emniyetli saha alanı",
                CriticalDependency = "ŞANTİYE ALTIN KURALI: Elektrik ve su hazır olmadan yıkım yapılamaz, şantiye çalışamaz!",
                DnaSource = "Adım 1: Elektrik, Su ve Mobilizasyon",
                PlannedStartDate = k03Start,
                PlannedEndDate = k03End,
                SubSteps = CreateSubSteps(
                    "T-3 Dağıtım şirketi geçici sayaç onayı, su bağlantı izni ve konteyner nakliye organizasyonu",
                    "Konteynerlerin vinç ile yerleşimi, kaçak akım korumalı ana pano ve temiz su deposu montajı",
                    "Pano topraklama meger ölçümü ve hidrofor tesisatı basınç testi",
                    "Pano kilit altına alma, 6 kg ABC yangın söndürme tüpleri ve su taşkın drenajı"
                )
            });

            // K-04 (Opsiyonel): Eski Yapı Yıkımı, Enkaz Sökümü & Moloz Bertarafı (Adım 1)
            var prevEnd = k03End;
            if (hasDemolition)
            {
                var k04Start = prevEnd;
                var k04End = k04Start.AddDays(5);
                prevEnd = k04End;
                list.Add(new WorkflowStepItemDto
                {
                    StepCode = "K-04",
                    Order = stepOrderSeq++,
                    PhaseName = "Eski Yapı Yıkımı, Enkaz Sökümü & Moloz Bertarafı",
                    StageCategory = "Yıkım & Moloz Temizliği",
                    PartIndex = p1Idx,
                    PartName = p1Name,
                    PartBadgeClass = p1Badge,
                    TriggerRule = "Elektrik/su kesme tutanağı onaylanınca başlar",
                    TargetStructureName = targetStructureName,
                    TargetBuildingId = targetBuildingId,
                    MaterialTitle = "Yıkım Kırıcı Uçları, Toz Bastırma Tesisatı ve Moloz Nakliye Kamyonları",
                    MaterialDescription = "Mevcut yapının kontrollü yıkımı, demirlerin hurdaya ayrıştırılması ve hafriyat döküm sahasına nakliyesi.",
                    MaterialQuantity = 0,
                    MaterialUnit = "m²",
                    MaterialFormula = "DXF Bekliyor (Mevcut yapı rölevesinden netleşecek)",
                    SupplierCategory = "Hafriyat & Yıkım Firması",
                    LaborTitle = "Ağır Yıkım Ekibi",
                    ProfessionName = "Yıkım Taşeronu",
                    CrewComposition = "1 Yıkım Formeni + 1 Kırıcı Ekskavatör Op. + 1 Loder Op. + 6 Kamyon Şoförü + 2 Sulama/Hurda İşçisi",
                    CrewSize = 11,
                    DurationDays = 5,
                    EquipmentNeeded = "30 Ton Paletli Kırıcı Ekskavatör, Loder, 6 Damperli Kamyon, Su Tankeri",
                    Predecessors = "Bedaş/İski/İgdaş ana kesme tutanakları ve Belediye yıkım ruhsatı",
                    CriticalDependency = "Çevre ve can güvenliği için elektrik/su/gaz kesilip onaylanmadan yıkıma başlanamaz!",
                    DnaSource = "Adım 1: Yıkım & Enkaz",
                    PlannedStartDate = k04Start,
                    PlannedEndDate = k04End,
                    SubSteps = CreateSubSteps(
                        "T-3 Belediye yıkım ruhsatı, çevre koruma ağı ve asbest analizi teyidi",
                        "Üstten alta kontrollü yıkım, hurda demir ayrıştırma ve molozun kamyonlara yüklenmesi",
                        "Zemin seviyesi temizliği ve komşu bina bitişik nizam duvar tahribat kontrolü",
                        "Toz bastırma su tankeri kullanımı, çevre yaya koruma koridoru ve güvenlik şeridi"
                    )
                });
            }

            // K-05: Derin Kazı (Hafriyat) & Güvenlik İksası (Shoring) (Adım 1)
            var k05Start = prevEnd;
            var k05End = k05Start.AddDays(14);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "K-05",
                Order = stepOrderSeq++,
                PhaseName = "Derin Kazı (Hafriyat) & Güvenlik İksası (Shoring)",
                StageCategory = "Hafriyat & Zemin İksası",
                PartIndex = p1Idx,
                PartName = p1Name,
                PartBadgeClass = p1Badge,
                TriggerRule = "Saha açılınca temel kazısı başlar",
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = $"{zeminTipi} Kazısı ve Güvenlik İksa Perdesi",
                MaterialDescription = "Kademeli hafriyat kazısı, döküm sahasına nakliye, hasır çelik ve püskürtme beton (shotcrete) iksa perdesi.",
                MaterialQuantity = 0,
                MaterialUnit = "m³",
                MaterialFormula = "DXF Bekliyor (Bodrum kat kesit yüksekliklerinden ve kazı kotundan hesaplanacak)",
                SupplierCategory = "Hafriyat & Zemin Firması",
                LaborTitle = "Ağır Hafriyat & İksa Operasyon Ekibi",
                ProfessionName = "Hafriyatçı & İksacı",
                CrewComposition = "2 Ağır Ekskavatör Op. + 1 Loder Op. + 8 Kamyon Şoförü + 2 Delgi Op. + 2 Shotcrete Ustası + 2 Saha Güvenlik",
                CrewSize = 17,
                DurationDays = 14,
                EquipmentNeeded = "2 Ekskavatör (Kovalı & Kırıcılı), 1 Loder, 8 Damperli Kamyon, Shotcrete Pompası, Ankraj Delgi Makinesi",
                Predecessors = "Mobilizasyonun tamamlanması ve kazı sınır kazıklarının çakılması",
                CriticalDependency = "Yol ve komşu parsel şevlerinin göçmesini önlemek için kademeli iksa uygulanmadan derin kazıya girilemez!",
                DnaSource = "Adım 1: Zemin & Hafriyat",
                PlannedStartDate = k05Start,
                PlannedEndDate = k05End,
                SubSteps = CreateSubSteps(
                    "T-3 Zemin etüdü raporu kontrolü, hafriyat döküm fişleri ve kazı güzergah izinleri teyidi",
                    "Kademeli kazı, kamyonlara yükleme, iksa perdesi hasır çelik ve püskürtme beton uygulaması",
                    "Optik nivo ile temel taban kotu kontrolü ve iksa deplasman lazer okuması",
                    "Derin kazı çukuru çevresi korkulukları, araç teker stoperi ve şev güvenlik takibi"
                )
            });

            // K-06: Taban Tesviyesi, Mıcır Drenajı & C20/25 Grobeton (Adım 2 & Adım 0)
            var k06Start = k05End;
            var k06End = k06Start.AddDays(2);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "K-06",
                Order = stepOrderSeq++,
                PhaseName = "Taban Tesviyesi, Mıcır Drenajı & C20/25 Grobeton",
                StageCategory = "Temel Tabanı",
                PartIndex = p1Idx,
                PartName = p1Name,
                PartBadgeClass = p1Badge,
                TriggerRule = "Kazı tamamlanıp taban kotu alınınca başlar",
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "C20/25 Hazır Grobeton ve Taban Filon Mıcırı",
                MaterialDescription = "Temel tabanına mıcır serilmesi, silindirlenmesi ve üzerine mastarlı grobeton dökümü.",
                MaterialQuantity = 0,
                MaterialUnit = "m³",
                MaterialFormula = "DXF Bekliyor (Temel taban oturum alanından hesaplanacak)",
                SupplierCategory = "Hazır Beton Santrali & Taş Ocağı",
                LaborTitle = "Grobeton ve Taban Tesviye Ekibi",
                ProfessionName = "Grobeton & Zemin Ustası",
                CrewComposition = "1 Beton Formeni + 1 Pompa Operatörü + 4 Vibratör Usta/Amelesi + 2 Mastarcı",
                CrewSize = 8,
                DurationDays = 2,
                EquipmentNeeded = "Beton Pompası, Silindir, Dalgıç Vibratör, Alüminyum Mastarlar",
                Predecessors = "Temel kazısının tamamlanması ve taban kotunun onaylanması",
                CriticalDependency = "Grobeton mastarı düzgün atılmadan üzerine su yalıtım membranı serilemez!",
                DnaSource = "Adım 2: Taban İmalatları (Grobeton)",
                PlannedStartDate = k06Start,
                PlannedEndDate = k06End,
                SubSteps = CreateSubSteps(
                    "T-2 Beton santrali grobeton siparişi ve silindir ile zemin sıkıştırması",
                    "Filon mıcır serimi, grobeton pompa dökümü ve mastarlı tesviye",
                    "Grobeton üzeri kot/mastar düzgünlüğü ve priz alma sulaması",
                    "Pompa boru hatlarının emniyet kilitleri ve döküm alanı KKD denetimi"
                )
            });

            // K-07: Çift Kat Membran Temel Bohçalama & Koruma Şapı (Adım 2)
            var k07Start = k06End;
            var k07End = k07Start.AddDays(5);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "K-07",
                Order = stepOrderSeq++,
                PhaseName = "Çift Kat Membran Temel Bohçalama & Koruma Şapı",
                StageCategory = "Temel Su Yalıtımı",
                PartIndex = p1Idx,
                PartName = p1Name,
                PartBadgeClass = p1Badge,
                TriggerRule = "Grobeton kuruyunca başlar",
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "SBS Katkılı Çift Kat Polyester Keçeli Membran, Bitüm Astar ve Koruma Betonu",
                MaterialDescription = "Grobeton üzerine bitüm astar, çift kat membran şaloma yapıştırma, pah teşliki ve üzerine koruma betonu.",
                MaterialQuantity = 0,
                MaterialUnit = "m²",
                MaterialFormula = "DXF Bekliyor (Temel tabanı ve perde çevre yüzey alanından hesaplanacak)",
                SupplierCategory = "Su İzolasyon Malzemeleri Bayisi",
                LaborTitle = "Su Yalıtımı ve Bohçalama Ekibi",
                ProfessionName = "Su Yalıtım Ustası",
                CrewComposition = "1 İzolasyon Formeni + 4 Şaloma/Yalıtım Ustası + 2 Taşıma İşçisi",
                CrewSize = 7,
                DurationDays = 5,
                EquipmentNeeded = "Şaloma Takımları, Gaz Tüpleri, Astar Püskürtme Pompası",
                Predecessors = "Grobetonun kuruması ve yüzey tozunun temizlenmesi",
                CriticalDependency = "Su yalıtımı delinmeden üzerine donatı demiri konamaz; mutlaka koruma şapı/betonu atılmalıdır!",
                DnaSource = "Adım 2: Temel Bohçalama",
                PlannedStartDate = k07Start,
                PlannedEndDate = k07End,
                SubSteps = CreateSubSteps(
                    "T-2 Bitüm astar, membran ruloları ve şaloma tüplerinin sevki",
                    "Astar uygulaması, çift kat şaşırtmalı şaloma yapıştırma ve pah çıtaları teşliki",
                    "Ek yerleri bindirme kaynak kontrolü ve koruma betonu uygulaması",
                    "Şaloma alevli çalışma yangın güvenlik tüpleri ve havalandırma önlemleri"
                )
            });

            // K-08: Temel Kalıp, Donatı Çeliği, Paspayı & Radye Temel Betonu (Adım 0)
            var k08Start = k07End;
            var k08End = k08Start.AddDays(8);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "K-08",
                Order = stepOrderSeq++,
                PhaseName = "Temel Kalıp, Donatı Çeliği, Paspayı & Radye Temel Betonu",
                StageCategory = "Temel & Kaba İnşaat",
                PartIndex = p1Idx,
                PartName = p1Name,
                PartBadgeClass = p1Badge,
                TriggerRule = "Bohçalama koruma şapı kuruyunca başlar",
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "C30/37 Taşıyıcı Hazır Beton ve B420C Nervürlü Donatı Çeliği",
                MaterialDescription = "Radye temel ahşap/çelik dış kalıbı, çift sıra nervürlü donatı, sehpalar, paspayları ve monolitik beton dökümü.",
                MaterialQuantity = 0,
                MaterialUnit = "m³",
                MaterialFormula = "DXF Bekliyor (Statik radye temel aplikasyon çiziminden hesaplanacak)",
                SupplierCategory = "Demir Çelik Bayisi & Hazır Beton Santrali",
                LaborTitle = "Demirci, Kalıpçı & Temel Beton Ekibi",
                ProfessionName = "Demirci, Kalıpçı & Betoncu",
                CrewComposition = "1 Şef + 6 Demirci + 4 Kalıpçı + 4 Betoncu + 2 Pompa Operatörü",
                CrewSize = 17,
                DurationDays = 8,
                EquipmentNeeded = "2 Mobil Pompa, 4 Vibratör, Slump Konisi, Numune Küp Kalıpları",
                Predecessors = "Bohçalama koruma şapı, topraklama şeridi ve paspayı takozlarının serimi",
                CriticalDependency = "Yapı Denetim demir vizesi imzalanmadan ve santral sevkiyatı planlanmadan beton dökülemez!",
                DnaSource = "Adım 0: Statik Taşıyıcılar (Radye Temel)",
                PlannedStartDate = k08Start,
                PlannedEndDate = k08End,
                SubSteps = CreateSubSteps(
                    "T-3 Santral döküm rezervasyonu, demir çap ve boy donatı metraj kontrolü",
                    "Temel dış kalıbı, donatı bağlama, paspayı takozu ve pompa ile radye dökümü",
                    "İrsaliye kontrolü, slump testi, numune küp alımı ve 7 gün ıslak kür sulaması",
                    "Pompa bom elektrik hattı mesafesi, vibratör kablo yalıtımları ve zorunlu KKD"
                )
            });

            // K-09: Kat Karkas İskelet: Kolon/Perde Kalıp & Donatı, Kat Tabliyesi (Adım 0)
            var k09Start = k08End;
            var k09End = k09Start.AddDays(14);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "K-09",
                Order = stepOrderSeq++,
                PhaseName = $"Kat Karkas İskelet: Kolon/Perde Kalıp & Donatı, Kat Tabliyesi ({maxFloors} Kat)",
                StageCategory = "Taşıyıcı İskelet",
                PartIndex = p1Idx,
                PartName = p1Name,
                PartBadgeClass = p1Badge,
                TriggerRule = "Radye temel betonu priz alınca kat kat kovalama ile başlar",
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "Kat Taşıyıcı C30/37 Hazır Beton, B420C Donatı Çeliği ve Endüstriyel Kalıp",
                MaterialDescription = "Kolon, perde, kiriş ve kat döşeme kalıp, donatı ve beton döküm döngüsü.",
                MaterialQuantity = 0,
                MaterialUnit = "m³",
                MaterialFormula = "DXF Bekliyor (Statik kat kalıp ve donatı planlarından hesaplanacak)",
                SupplierCategory = "Hazır Beton Santrali & Kalıp/İskele Tedarikçisi",
                LaborTitle = "Kalıpçı, Demirci & Betonarme Ekibi",
                ProfessionName = "Kalıpçı, Demirci & Betonarme Ustası",
                CrewComposition = "1 Kalıp Formeni + 6 Kalıpçı + 6 Demirci + 3 Betoncu",
                CrewSize = 16,
                DurationDays = 14,
                EquipmentNeeded = "Kule Vinç, Kolon/Perde Çelik Kalıpları, Dalgıç Vibratörler",
                Predecessors = "Temel dökümü ve kolon/perde filizlerinin kontrolü",
                CriticalDependency = "Kat betonu dökülmeden önce kat kenar güvenlik korkulukları ve asansör şaft kapakları takılmalıdır!",
                DnaSource = "Adım 0: Statik Taşıyıcılar (Kat Karkas)",
                PlannedStartDate = k09Start,
                PlannedEndDate = k09End,
                SubSteps = CreateSubSteps(
                    "T-3 Kat kalıp planı, donatı filizleri ve kat santral döküm sırası teyidi",
                    "Kolon/perde kalıp montajı, demir bağlama, tabliye iskelesi ve kat betonu dökümü",
                    "Döşeme mastar kotları, şakül kontrolü, taze beton slump & küp numunesi ve kür sulaması",
                    "Kat kenarları geçici güvenlik korkulukları, merdiven boşluğu kapakları ve paraşüt tipi emniyet kemeri"
                )
            });

            // K-10: Çatı Taşıyıcı İskeleti, Su Yalıtımı, Parapet Harpuştası & Yağmur İnişleri (Adım 5)
            var k10Start = k09End;
            var k10End = k10Start.AddDays(8);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "K-10",
                Order = stepOrderSeq++,
                PhaseName = "Çatı Taşıyıcı İskeleti, Su Yalıtımı, Parapet Harpuştası & Yağmur İnişleri",
                StageCategory = "Çatı & Kapanış",
                PartIndex = p1Idx,
                PartName = p1Name,
                PartBadgeClass = p1Badge,
                TriggerRule = "Son kat tavan tabliyesi dökülünce başlar",
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "Çatı Ahşap/Çelik Taşıyıcı Konstrüksiyonu, Su/Isı İzolasyonu ve Parapet Harpuştası",
                MaterialDescription = "Çatı ahşap/çelik taşıyıcı dikmeleri, OSB kaplama, su/ısı yalıtımı, dere inişleri ve mermer/alüminyum parapet harpuştaları.",
                MaterialQuantity = 0,
                MaterialUnit = "mt",
                MaterialFormula = "DXF Bekliyor (Çatı planı ve parapet çevre uzunluğundan hesaplanacak)",
                SupplierCategory = "Keresteci & Çatı İzolasyon Bayisi",
                LaborTitle = "Çatı ve İzolasyon Ekibi",
                ProfessionName = "Çatı & İzolasyon Ustası",
                CrewComposition = "1 Çatı Formeni + 4 Çatı Ustası + 2 İzolasyoncu",
                CrewSize = 7,
                DurationDays = 8,
                EquipmentNeeded = "Mobil Vinç / Halatlı Çatı Vinci, Kaynak Takımları, Şaloma",
                Predecessors = "Son kat tabliye betonunun priz alması ve parapet kalıplarının sökümü",
                CriticalDependency = "Çatı su yalıtımı ve dereleri tamamlanmadan alt katlara kaba ince işlere geçilemez!",
                DnaSource = "Adım 5: Çatı & Su İzolasyonu",
                PlannedStartDate = k10Start,
                PlannedEndDate = k10End,
                SubSteps = CreateSubSteps(
                    "T-3 Çatı karkas malzemeleri, izolasyon membranları ve vinç planı",
                    "Taşıyıcı iskelet montajı, OSB/tahta kaplama, su/ısı yalıtımı ve parapet harpuştaları",
                    "Dere eğimleri, su tahliye testi ve harpuşta ek yeri sızdırmazlık kontrolü",
                    "Çatı hattı yaşam hattı (life line), tam korumalı emniyet kemeri ve saçak güvenlik filesi"
                )
            });

            // =========================================================================
            // 2. PARÇA: STATİK İNCE (KABA İNCE / KOVALAYAN İŞLER)
            // =========================================================================

            // S-01: Dış ve İç Bölme Duvar Örümü & Hatıllar / Lentolar (Adım 3)
            var s01Start = k08End.AddDays(7); // Kaba 2 kat önden gidince başlar
            var s01End = s01Start.AddDays(10);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "S-01",
                Order = stepOrderSeq++,
                PhaseName = "Dış ve İç Bölme Duvar Örümü & Hatıllar / Lentolar",
                StageCategory = "Duvar İmalatları",
                PartIndex = p2Idx,
                PartName = p2Name,
                PartBadgeClass = p2Badge,
                TriggerRule = p2Trigger,
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "Gazbeton / Bims Bloklar, Örgü Harcı ve Hatıl / Lentolar",
                MaterialDescription = "Dış cephe ve daire içi oda bölme duvarlarının örülmesi, kapı üstü lentoları ve hatıl bağlantıları.",
                MaterialQuantity = 0,
                MaterialUnit = "m²",
                MaterialFormula = "DXF Bekliyor (Mimari kat planı duvar metrajlarından hesaplanacak)",
                SupplierCategory = "Gazbeton / Tuğla & Harç Fabrika Bayisi",
                LaborTitle = "Duvar Örme Ekibi",
                ProfessionName = "Duvar Ustası",
                CrewComposition = "1 Formen + 6 Duvar Ustası + 4 Taşıma/Harç İşçisi",
                CrewSize = 11,
                DurationDays = 10,
                EquipmentNeeded = "Harç Mikseri, Blok Testeresi, İç Mekan Seyyar İskele",
                Predecessors = "Üst kat tabliyesinin dökülmesi ve dikme kulelerinin güvenli sökümü",
                CriticalDependency = "Kaba 2 kat önden gitmeden alttan duvar örümüne başlanamaz!",
                DnaSource = "Adım 3: Bölme Duvarlar",
                PlannedStartDate = s01Start,
                PlannedEndDate = s01End,
                SubSteps = CreateSubSteps(
                    "T-2 Mimari projeye göre kat kapı ve pencere boşluklarının sahaya aplikasyonu",
                    "İlk sıra harç mastarı, duvar örümü, esnek tavan birleşimi (poliüretan köpük) ve kapı lentoları",
                    "Duvar şakül, gönye ve hatıl donatı bağlantılarının kontrolü",
                    "İç iskele korkulukları, malzeme istif güvenliği ve toz maskesi kullanımı"
                )
            });

            // S-02: Daire İçi ve Şaft Sıhhi Tesisat, Pis Su & Temiz Su Borulama (Adım 7)
            var s02Start = s01Start.AddDays(3);
            var s02End = s02Start.AddDays(8);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "S-02",
                Order = stepOrderSeq++,
                PhaseName = "Daire İçi ve Şaft Sıhhi Tesisat, Pis Su & Temiz Su Borulama",
                StageCategory = "Mekanik Tesisat",
                PartIndex = p2Idx,
                PartName = p2Name,
                PartBadgeClass = p2Badge,
                TriggerRule = p2Trigger,
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = $"{pisuTitle}, PPRC Temiz Su Boruları ve Kollektörler",
                MaterialDescription = "Bina dikey ana şaft kolonları, daire içi ıslak hacim pis su giderleri ve PPRC temiz su dağıtım tesisatı.",
                MaterialQuantity = 0,
                MaterialUnit = "mt",
                MaterialFormula = "DXF Bekliyor (Mekanik tesisat şaft ve daire içi kolon projelerinden hesaplanacak)",
                SupplierCategory = "Sıhhi Tesisat & Boru Toptancısı",
                LaborTitle = "Sıhhi Tesisat Ekibi",
                ProfessionName = "Sıhhi Tesisat Ustası",
                CrewComposition = "1 Mekanik Müh. + 4 Tesisat Ustası + 2 Yardımcı",
                CrewSize = 7,
                DurationDays = 8,
                EquipmentNeeded = "PPRC Kaynak Makinesi, Boru Kesme Aparatları, Basınç Test Pompası",
                Predecessors = "Kat duvarlarının örülmesi ve şaft boşluklarının açılması",
                CriticalDependency = "Sıva ve şap atılmadan önce tesisat boruları test edilip basınca tabi tutulmalıdır!",
                DnaSource = "Adım 7: Sıhhi Tesisat & Borulama",
                PlannedStartDate = s02Start,
                PlannedEndDate = s02End,
                SubSteps = CreateSubSteps(
                    "T-3 Tesisat güzergahlarının ve rezervuar/kollektör yerlerinin işaretlenmesi",
                    "Pis su eğim hattı, sessiz boru kelepçelemesi, temiz su PPRC borulama ve kollektör montajı",
                    "Test pompası ile 10 bar basınç sızdırmazlık testi ve 24 saat basınç manometre takibi",
                    "Şaft çalışma platformları, kaynak güvenliği ve boru kesme çapağı gözlüğü"
                )
            });

            // S-03: Daire İçi Elektrik Altyapısı, Borulama, Kablolama ve Sortiler (Adım 7)
            var s03Start = s01Start.AddDays(4);
            var s03End = s03Start.AddDays(8);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "S-03",
                Order = stepOrderSeq++,
                PhaseName = "Daire İçi Elektrik Altyapısı, Borulama, Kablolama ve Sortiler",
                StageCategory = "Elektrik Altyapı",
                PartIndex = p2Idx,
                PartName = p2Name,
                PartBadgeClass = p2Badge,
                TriggerRule = p2Trigger,
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "Halogen Free Borular, NYM Kablolar, Buat ve Kasalar",
                MaterialDescription = "Duvar içi elektrik kanallarının açılması, kasalama, linye borulaması, buat montajı ve kablo çekimi.",
                MaterialQuantity = 0,
                MaterialUnit = "Sorti",
                MaterialFormula = "DXF Bekliyor (Elektrik kuvvetli/zayıf akım projelerinden sorti sayısı netleşecek)",
                SupplierCategory = "Elektrik Malzemeleri Toptancısı",
                LaborTitle = "Elektrik Tesisat Ekibi",
                ProfessionName = "Elektrik Ustası",
                CrewComposition = "1 Elektrik Formeni + 4 Elektrik Ustası + 3 Yardımcı",
                CrewSize = 8,
                DurationDays = 8,
                EquipmentNeeded = "Kanal Açma Makinesi, Sustalı Kablo Çekici, Meger Test Cihazı",
                Predecessors = "Kat duvarlarının örülmesi ve tavan karkasının netleşmesi",
                CriticalDependency = "Kablolar çekilmeden ve buatlar sabitlenmeden sıva yapılamaz!",
                DnaSource = "Adım 7: Elektrik Tesisatı & Kablolama",
                PlannedStartDate = s03Start,
                PlannedEndDate = s03End,
                SubSteps = CreateSubSteps(
                    "T-2 Elektrik projesine göre priz, anahtar ve aydınlatma linye kotlarının çizilmesi",
                    "Kanal açma, borulama, kasalama, buat montajı ve kablo çekimi",
                    "Faz, nötr, toprak meger izolasyon testi ve kablo süreklilik kontrolü",
                    "Kanal açma tozu için filtreli maske, spiral koruyucu kapağı ve eldiven denetimi"
                )
            });

            // S-04: İç Cephe Kaba Sıva, Alçı Sıva & Tavan Alçıpan Asma Tavan (Adım 3)
            var s04Start = s02End;
            var s04End = s04Start.AddDays(12);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "S-04",
                Order = stepOrderSeq++,
                PhaseName = "İç Cephe Kaba Sıva, Alçı Sıva & Tavan Alçıpan Asma Tavan",
                StageCategory = "Sıva & Alçıpan",
                PartIndex = p2Idx,
                PartName = p2Name,
                PartBadgeClass = p2Badge,
                TriggerRule = p2Trigger,
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "Perlitli Alçı Sıva, Ano / Köşe Profilleri ve Tavan Alçıpan Levhaları",
                MaterialDescription = "Duvarlarda makineli alçı sıva püskürtme, ano çekimi, mastarlama ve tavan alçıpan asma tavan imalatı.",
                MaterialQuantity = 0,
                MaterialUnit = "m²",
                MaterialFormula = "DXF Bekliyor (Kat yükseklikleri ve net duvar yüzey alanlarından hesaplanacak)",
                SupplierCategory = "Yapı Kimyasalları & Alçı Toptancısı",
                LaborTitle = "Alçı Sıva & Asma Tavan Ekibi",
                ProfessionName = "Alçı Sıva & Asma Tavan Ustası",
                CrewComposition = "1 Sıva Formeni + 6 Alçı/Sıva Ustası + 4 Yardımcı",
                CrewSize = 11,
                DurationDays = 12,
                EquipmentNeeded = "Alçı Sıva Püskürtme Makinesi, Alüminyum H Mastarlar, Lazer Hizalama",
                Predecessors = "Elektrik kasalama ve tesisat borulama hatlarının kapatılması",
                CriticalDependency = "Elektrik tesisat testleri tamamlanıp kapak takılmadan sıva kapatılamaz!",
                DnaSource = "Adım 3: Sıva & Alçıpan",
                PlannedStartDate = s04Start,
                PlannedEndDate = s04End,
                SubSteps = CreateSubSteps(
                    "T-2 Ano çıtalarının çekilmesi ve kapı/pencere kenarları fileli köşe profili montajı",
                    "Makineli alçı sıva püskürtme, mastarlama ve tavan alçıpan karkas montajı",
                    "2 metrelik mastar ile yüzey kaçıklığı ve şakül kontrolü",
                    "Hareketli platform iskele emniyet kilitleri ve göz yıkama solüsyonu kontrolü"
                )
            });

            // S-05: Helikopterli ve Mastarlı Zemin Tesviye Şapı (Adım 3)
            var s05Start = s04End;
            var s05End = s05Start.AddDays(6);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "S-05",
                Order = stepOrderSeq++,
                PhaseName = "Helikopterli ve Mastarlı Zemin Tesviye Şapı",
                StageCategory = "Zemin Şapı",
                PartIndex = p2Idx,
                PartName = p2Name,
                PartBadgeClass = p2Badge,
                TriggerRule = p2Trigger,
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "Zemin Şap Kumu, Çimento ve Yüzey Sertleştirici Katkı",
                MaterialDescription = "Zemindeki tesisat borularının üzerine mastarlı çimento esaslı tesviye şapı atılması ve helikopter perdahı.",
                MaterialQuantity = 0,
                MaterialUnit = "m²",
                MaterialFormula = "DXF Bekliyor (Net döşeme alanı mimari plandan hesaplanacak)",
                SupplierCategory = "Şap Kumu & Çimento Bayisi",
                LaborTitle = "Helikopterli Şap Ekibi",
                ProfessionName = "Şap Ustası",
                CrewComposition = "1 Şap Formeni + 4 Şap Ustası + 2 Pompa/Mikser Operatörü",
                CrewSize = 7,
                DurationDays = 6,
                EquipmentNeeded = "Şap Pompası (Kara Şap), Helikopter Perdah Makinesi, Lazer Nivo",
                Predecessors = "Zeminden geçen tesisat borularının yalıtımı ve kot referans çizgisi",
                CriticalDependency = "Zemin şapı atılmadan önce temiz/pis su boruları basınç testinden geçmiş ve sabitlenmiş olmalıdır!",
                DnaSource = "Adım 3: Şap İmalatları",
                PlannedStartDate = s05Start,
                PlannedEndDate = s05End,
                SubSteps = CreateSubSteps(
                    "T-2 Lazer nivo ile kat kotlarının işaretlenmesi ve şap anolarının oluşturulması",
                    "Şap pompası ile harç basımı, mastarlama ve helikopterli yüzey perdahı",
                    "Lazer nivo ile eğim kontrolü, dilatasyon derz kesimi ve çatlama önleme sulaması",
                    "Helikopterli perdah makinesi acil durdurma mandalı ve çizme kullanımı"
                )
            });

            // S-06: Dış Cephe Güvenlikli İskele Kurulumu & Isı Yalıtım / Mantolama (Adım 6)
            var s06Start = k10End;
            var s06End = s06Start.AddDays(14);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "S-06",
                Order = stepOrderSeq++,
                PhaseName = "Dış Cephe Güvenlikli İskele Kurulumu & Isı Yalıtım / Mantolama",
                StageCategory = "Dış Cephe Mantolama",
                PartIndex = p2Idx,
                PartName = p2Name,
                PartBadgeClass = p2Badge,
                TriggerRule = p2Trigger,
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "Taşyünü / EPS Mantolama Levhaları, Fileli Sıva ve Dekoratif Dış Cephe Kaplaması",
                MaterialDescription = "TS EN 12810 onaylı güvenlikli flanşlı iskele kurulumu, ısı yalıtım levhaları, dübelleme, fileli sıva ve dekoratif cephe kaplaması.",
                MaterialQuantity = 0,
                MaterialUnit = "m²",
                MaterialFormula = "DXF Bekliyor (Bina cephe açılımları ve kat yüksekliklerinden hesaplanacak)",
                SupplierCategory = "Dış Cephe Yalıtım Bayisi & İskele Kiralama",
                LaborTitle = "Dış Cephe ve Mantolama Ekibi",
                ProfessionName = "Mantolama & Cephe Ustası",
                CrewComposition = "1 İskele Süpervizörü + 6 Cephe Ustası + 4 Yardımcı",
                CrewSize = 11,
                DurationDays = 14,
                EquipmentNeeded = "Güvenlikli Cephe İskelesi, Dübel Matkapları, Harç Karıştırma Mikseri",
                Predecessors = "Kaba inşaat cephe temizliği ve denizlik montajları",
                CriticalDependency = "Dış cephe güvenlikli iskelesi yeşil etiket (TS EN 12810 onaylı) almadan cephe çalışması başlatılamaz!",
                DnaSource = "Adım 6: Dış Cephe & Mantolama",
                PlannedStartDate = s06Start,
                PlannedEndDate = s06End,
                SubSteps = CreateSubSteps(
                    "T-3 Güvenlikli flanşlı iskele kurulumu, statik ankraj tespiti ve güvenlik ağları",
                    "Levha yapıştırma, mekanik dübelleme, çift kat fileli sıva ve dekoratif kaplama",
                    "Dübel çekme testi, köşe diklik kontrolü ve su iticilik incelemesi",
                    "İskele yeşil etiket denetimi, rüzgar hız takibi ve paraşüt tipi çift kollu emniyet kemeri"
                )
            });

            // =========================================================================
            // 3. PARÇA: TERCİHE GÖRE İNCE İŞLER (DAİRE İÇİ İMALATLAR)
            // =========================================================================

            // D-01: Islak Hacimler (Banyo/WC/Balkon) Çift Kat Sürme Su Yalıtımı (Adım 4)
            var d01Start = s05End.AddDays(2);
            var d01End = d01Start.AddDays(5);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "D-01",
                Order = stepOrderSeq++,
                PhaseName = "Islak Hacimler (Banyo/WC/Balkon) Çift Kat Sürme Su Yalıtımı",
                StageCategory = "Islak Hacim Yalıtımı",
                PartIndex = p3Idx,
                PartName = p3Name,
                PartBadgeClass = p3Badge,
                TriggerRule = p3Trigger,
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "Çift Bileşenli Tam Elastik Su Yalıtımı ve Pah Bantları",
                MaterialDescription = "Banyo, tuvalet ve balkon zeminlerine çift kat elastik sürme izolasyon ve süzgeç manşetleri uygulaması.",
                MaterialQuantity = 0,
                MaterialUnit = "m²",
                MaterialFormula = "DXF Bekliyor (Islak hacim zemin ve duvar yüzey alanlarından hesaplanacak)",
                SupplierCategory = "Su İzolasyonu Kimyasalları Bayisi",
                LaborTitle = "Islak Hacim İzolasyon Ekibi",
                ProfessionName = "İzolasyon Ustası",
                CrewComposition = "1 İzolasyon Formeni + 3 İzolasyon Ustası",
                CrewSize = 4,
                DurationDays = 5,
                EquipmentNeeded = "İzolasyon Karıştırıcı Mikser, Rulo ve Pah Fırçaları",
                Predecessors = "Islak hacim şapının kuruması ve süzgeç kasalarının montajı",
                CriticalDependency = "Su yalıtımı 48 saat su göllendirme sızdırmazlık testinden geçmeden üzerine seramik döşenemez!",
                DnaSource = "Adım 4: Islak Hacim Su Yalıtımı",
                PlannedStartDate = d01Start,
                PlannedEndDate = d01End,
                SubSteps = CreateSubSteps(
                    "T-2 Zemin tozunun temizlenmesi, pah bandı ve elastik astar hazırlığı",
                    "Duvar-taban birleşim pah bantları, ilk kat ve ikinci kat şaşırtmalı sürme yalıtım",
                    "48 saat su göllendirme testi ve alt kat tavanında ıslaklık/nem kontrolü",
                    "Havalandırmasız banyolarda fanlı hava tahliyesi ve organik buhar maskesi"
                )
            });

            // D-02: Islak Hacim & Mutfak Seramik / Porselen Kaplama (Adım 4)
            var d02Start = d01End;
            var d02End = d02Start.AddDays(12);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "D-02",
                Order = stepOrderSeq++,
                PhaseName = "Islak Hacim & Mutfak Seramik / Porselen Kaplama",
                StageCategory = "Seramik & Fayans",
                PartIndex = p3Idx,
                PartName = p3Name,
                PartBadgeClass = p3Badge,
                TriggerRule = p3Trigger,
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "Porselen Seramik / Fayans, Esnek Yapıştırıcı ve Derz Dolgusu",
                MaterialDescription = "Banyo duvar/zemin seramik kaplamaları, mutfak tezgah arası ve balkon seramiklerinin döşenmesi.",
                MaterialQuantity = 0,
                MaterialUnit = "m²",
                MaterialFormula = "DXF Bekliyor (Islak hacim ve mutfak döşeme/duvar net alanlarından hesaplanacak)",
                SupplierCategory = "Seramik Mağazası & Yapıştırıcı Bayisi",
                LaborTitle = "Seramik ve Fayans Ekibi",
                ProfessionName = "Seramik Ustası",
                CrewComposition = "1 Formen + 6 Seramik Ustası + 3 Yardımcı",
                CrewSize = 10,
                DurationDays = 12,
                EquipmentNeeded = "Lazer Hizalama Nivosu, Sulu Seramik Kesme Makinesi, Tesviye Klipsleri",
                Predecessors = "Islak hacim su yalıtım sızdırmazlık onay tutanağı",
                CriticalDependency = "Seramik yapıştırıcısı priz almadan ve derz dolgusu çekilmeden vitrifiye montajına başlanamaz!",
                DnaSource = "Adım 4: Seramik & Zemin",
                PlannedStartDate = d02Start,
                PlannedEndDate = d02End,
                SubSteps = CreateSubSteps(
                    "T-2 Seramik açılım şeması, derz artı/takozları ve lazer hizalama hazırlığı",
                    "Taraklı yapıştırma harcı çekimi, tesviye klipsleri ile seramik montajı ve derz dolgusu",
                    "Mastar ile yüzey sıfırlığı, eğim kontrolü ve derz dolgusu pürüzsüzlük kontrolü",
                    "Islak seramik kesme makinesi koruması, toz maskesi ve dizlik kullanımı"
                )
            });

            // D-03: Daire İçi Duvar & Tavan Su Bazlı Silikonlu Boya (Adım 4)
            var d03Start = s05End.AddDays(7);
            var d03End = d03Start.AddDays(10);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "D-03",
                Order = stepOrderSeq++,
                PhaseName = "Daire İçi Duvar & Tavan Su Bazlı Silikonlu Boya",
                StageCategory = "Boya & Badana",
                PartIndex = p3Idx,
                PartName = p3Name,
                PartBadgeClass = p3Badge,
                TriggerRule = p3Trigger,
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "İpeksi Mat Silikonlu İç Cephe Boyası, Astar ve Macun",
                MaterialDescription = "Duvar ve tavan yüzeylerinde zımpara, tamirat macunu, astar ve çift kat silikonlu boya uygulaması.",
                MaterialQuantity = 0,
                MaterialUnit = "m²",
                MaterialFormula = "DXF Bekliyor (Net duvar/tavan boya alanlarından hesaplanacak)",
                SupplierCategory = "Boya Bayisi & Yapı Kimyası",
                LaborTitle = "Boya & Yüzey İşlemleri Ekibi",
                ProfessionName = "Boya Ustası",
                CrewComposition = "1 Boya Formeni + 5 Boyacı Ustası",
                CrewSize = 6,
                DurationDays = 10,
                EquipmentNeeded = "Zımpara Makinesi, Boya Ruloları, Kestirme Fırçaları, Spot Projektör",
                Predecessors = "Alçı sıva zımparası ve nem oranının %10 altına düşmesi",
                CriticalDependency = "Ahşap kapılar ve parkeler döşenmeden önce duvarların 1. ve 2. kat astar/boyası tamamlanmalıdır!",
                DnaSource = "Adım 4: İç Mekan Boya",
                PlannedStartDate = d03Start,
                PlannedEndDate = d03End,
                SubSteps = CreateSubSteps(
                    "T-2 Alçı sıva yüzey zımparası, spot lamba ile yüzey kontrolü ve şeffaf astar çekimi",
                    "Rulo ve kestirme fırçalar ile 2 kat boya uygulaması ve süpürgelik altı izolasyonu",
                    "Işık altında fırça izi, dalgalanma ve ton farkı kontrolü",
                    "İç mekan havalandırması ve göz koruyucu gözlük"
                )
            });

            // D-04: Daire İçi Parke, Süpürgelik & Zemin Kaplamaları (Adım 4)
            var d04Start = d03End;
            var d04End = d04Start.AddDays(7);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "D-04",
                Order = stepOrderSeq++,
                PhaseName = "Daire İçi Parke, Süpürgelik & Zemin Kaplamaları",
                StageCategory = "Parke & Zemin",
                PartIndex = p3Idx,
                PartName = p3Name,
                PartBadgeClass = p3Badge,
                TriggerRule = p3Trigger,
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "32. Sınıf Derzli Laminat Parke, Kapron Şilte ve Süpürgelik",
                MaterialDescription = "Salon ve odalarda kapron şilte serimi, kilitli laminat parke montajı ve 8cm lake süpürgelikler.",
                MaterialQuantity = 0,
                MaterialUnit = "m²",
                MaterialFormula = "DXF Bekliyor (Kuru hacim net döşeme alanlarından hesaplanacak)",
                SupplierCategory = "Parke & Orman Ürünleri Mağazası",
                LaborTitle = "Laminat Parke Montaj Ekibi",
                ProfessionName = "Parke Ustası",
                CrewComposition = "1 Formen + 4 Parke Ustası",
                CrewSize = 5,
                DurationDays = 7,
                EquipmentNeeded = "Giyotin Parke Kesici, Gönye Kesme Testeresi, Parke Çektirme Demiri",
                Predecessors = "Zemin şapının tamamen kuruması (nem <%2) ve tavan boyalarının bitmesi",
                CriticalDependency = "Zemin nemi %2'nin altına düşmeden parke serilemez; aksi takdirde kabarma ve deformasyon oluşur!",
                DnaSource = "Adım 4: Parke & Zemin",
                PlannedStartDate = d04Start,
                PlannedEndDate = d04End,
                SubSteps = CreateSubSteps(
                    "T-2 Dijital nem ölçer ile şap nem kontrolü ve kapron şilte serimi",
                    "Duvar dibi 1.5 cm genleşme boşluğu bırakarak kilitli parke ve süpürgelik montajı",
                    "Zemin esneme, kilit oturumu ve süpürgelik köşe gönye kontrolü",
                    "Giyotin kesici güvenliği ve diz koruyucu pedler"
                )
            });

            // =========================================================================
            // 4. PARÇA: DEMİRBAŞ, MONTAJ & DIŞ ÇEVRE (TESLİM AŞAMASI)
            // =========================================================================

            // M-01: Daire Giriş Çelik Kapıları ve İç Ahşap Oda Kapıları (Adım 4)
            var m01Start = d04End;
            var m01End = m01Start.AddDays(6);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "M-01",
                Order = stepOrderSeq++,
                PhaseName = "Daire Giriş Çelik Kapıları ve İç Ahşap Oda Kapıları",
                StageCategory = "Kapı Montajı",
                PartIndex = p4Idx,
                PartName = p4Name,
                PartBadgeClass = p4Badge,
                TriggerRule = p4Trigger,
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "Monoblok Çelik Daire Giriş Kapısı ve İç Ahşap Oda Kapıları",
                MaterialDescription = "Monoblok kilitli çelik kapılar, ahşap kasa/kanat oda kapıları, pervazlar, kollar ve kilitler.",
                MaterialQuantity = 0,
                MaterialUnit = "Adet",
                MaterialFormula = "DXF Bekliyor (Mimari kapı pencere şemasından adetler netleşecek)",
                SupplierCategory = "Çelik Kapı & Ahşap Kapı İmalatçısı",
                LaborTitle = "Kapı Montaj Ekibi",
                ProfessionName = "Kapı Montaj Ustası",
                CrewComposition = "1 Formen + 3 Kapı Montaj Ustası + 2 Yardımcı",
                CrewSize = 6,
                DurationDays = 6,
                EquipmentNeeded = "Lazer Terazi, Akülü Vidalama, Köpük Tabancası",
                Predecessors = "Zemin parke ve ıslak hacim seramik döşemelerinin tamamlanması",
                CriticalDependency = "Kapı kasaları montajından önce zemin kaplama kalınlıkları netleşmiş olmalıdır!",
                DnaSource = "Adım 4: Çelik ve Ahşap Kapılar",
                PlannedStartDate = m01Start,
                PlannedEndDate = m01End,
                SubSteps = CreateSubSteps(
                    "T-3 Daire bazında kapı açılış yönleri ve net kasa açıklıklarının kontrolü",
                    "Kasa terazisi, poliüretan köpük dolgusu, kanat ayarı ve pervaz montajı",
                    "Kilit, kol ve menteşe sürtünme testleri ve kanat ayar kontrolü",
                    "Ağır çelik kapı taşımalarında ergonomik kaldırma ve eldiven kullanımı"
                )
            });

            // M-02: Mutfak Dolapları, Tezgah & Banyo Dolapları Montajı (Adım 4)
            var m02Start = m01Start.AddDays(2);
            var m02End = m02Start.AddDays(8);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "M-02",
                Order = stepOrderSeq++,
                PhaseName = "Mutfak Dolapları, Tezgah & Banyo Dolapları Montajı",
                StageCategory = "Sabit Mobilya Montajı",
                PartIndex = p4Idx,
                PartName = p4Name,
                PartBadgeClass = p4Badge,
                TriggerRule = p4Trigger,
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "Modüler Mutfak Dolapları, Granit / Kuvars Tezgah ve Banyo Dolapları",
                MaterialDescription = "MDF gövde modüler mutfak dolapları, kuvars tezgah, banyo aynalı lavabo dolapları montajı.",
                MaterialQuantity = 0,
                MaterialUnit = "Daire Takımı",
                MaterialFormula = "DXF Bekliyor (Mutfak ve banyo mimari yerleşim detaylarından hesaplanacak)",
                SupplierCategory = "Mutfak / Banyo Mobilya Üreticisi & Mermer Atölyesi",
                LaborTitle = "Mobilya & Tezgah Montaj Ekibi",
                ProfessionName = "Mobilya Montaj Ustası",
                CrewComposition = "1 Formen + 4 Mobilya Ustası + 2 Mermer/Tezgah Ustası",
                CrewSize = 7,
                DurationDays = 8,
                EquipmentNeeded = "Lazer Hizalama, Akülü Montaj Setleri, Vakumlu Vantuzlar",
                Predecessors = "İç boya, zemin kaplamaları ve elektrik/sıhhi tesisat sortilerinin hazır olması",
                CriticalDependency = "Dolap arkası tesisat vanaları ve priz yerleri milimetrik hizalanmalıdır!",
                DnaSource = "Adım 4: Mutfak & Banyo Mobilyaları",
                PlannedStartDate = m02Start,
                PlannedEndDate = m02End,
                SubSteps = CreateSubSteps(
                    "T-3 İmalat çizimlerine göre mutfak ve banyo duvarlarının gönye ve tesisat kontrolü",
                    "Modüler gövde asımı, raylı çekmece/kapak montajı ve tezgah montajı",
                    "Kapak boşlukları, ray amortisör testleri ve eviye sızdırmazlık silikonu kontrolü",
                    "Duvar asma raylarının taşıma kapasitesi ve güvenli montaj vidaları denetimi"
                )
            });

            // M-03: Vitrifiye, Armatür ve Bataryalar Montajı (Adım 4)
            var m03Start = m02End;
            var m03End = m03Start.AddDays(5);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "M-03",
                Order = stepOrderSeq++,
                PhaseName = "Vitrifiye, Armatür ve Bataryalar Montajı",
                StageCategory = "Vitrifiye & Armatür",
                PartIndex = p4Idx,
                PartName = p4Name,
                PartBadgeClass = p4Badge,
                TriggerRule = p4Trigger,
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "Asma Klozet, Gömme Rezervuar, Lavabo, Batarya ve Duş Setleri",
                MaterialDescription = "Gömme rezervuar kumanda panelleri, klozet kapakları, lavabo bataryaları, duş başlıkları ve aksesuarlar.",
                MaterialQuantity = 0,
                MaterialUnit = "Daire Seti",
                MaterialFormula = "DXF Bekliyor (Islak hacim vitrifiye yerleşim planından netleşecek)",
                SupplierCategory = "Vitrifiye & Banyo Donanım Bayisi",
                LaborTitle = "İnce Sıhhi Tesisat Montaj Ekibi",
                ProfessionName = "Vitrifiye Montaj Ustası",
                CrewComposition = "1 Formen + 4 Montaj Ustası",
                CrewSize = 5,
                DurationDays = 5,
                EquipmentNeeded = "Tork Anahtarları, Silikon Tabancaları, Boru Anahtarları",
                Predecessors = "Banyo mobilyaları ve seramik imalatlarının tamamlanması",
                CriticalDependency = "Batarya ve klozet montajları tamamlandıktan sonra basınçlı su testi yapılmadan daire teslim edilemez!",
                DnaSource = "Adım 4: Vitrifiye & Armatür",
                PlannedStartDate = m03Start,
                PlannedEndDate = m03End,
                SubSteps = CreateSubSteps(
                    "T-2 Daire bazlı vitrifiye kutularının dağıtımı ve ara muslukların temizliği",
                    "Asma klozet, lavabo montajı, bataryalar ve duş başlığı montajı",
                    "Tüm bataryaların su akış ve rezervuar su basma testi, kaçak kontrolü",
                    "Porselen montajlarında aşırı sıkma çatlamalarını önleme ve koruyucu eldiven"
                )
            });

            // M-04: Asansör Montajı, Güvenlik Testleri ve Tescil Ruhsatı (Adım 8)
            var m04Start = s05End;
            var m04End = m04Start.AddDays(12);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "M-04",
                Order = stepOrderSeq++,
                PhaseName = "Asansör Montajı, Güvenlik Testleri ve Tescil Ruhsatı",
                StageCategory = "Asansör & Sirkülasyon",
                PartIndex = p4Idx,
                PartName = p4Name,
                PartBadgeClass = p4Badge,
                TriggerRule = p4Trigger,
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "Makine Dairesiz (MRL) Asansör Paketi, Raylar ve Kabin Donanımı",
                MaterialDescription = "Kılavuz raylar, tahrik motoru, kabin, halatlar, karşı ağırlık, otomatik kat kapıları ve kumanda panosu.",
                MaterialQuantity = 0,
                MaterialUnit = "Adet Asansör",
                MaterialFormula = "DXF Bekliyor (Asansör kuyu rölevesi ve durak sayısından hesaplanacak)",
                SupplierCategory = "Asansör İmalatçı & Montaj Firması",
                LaborTitle = "Asansör İmalat ve Montaj Ekibi",
                ProfessionName = "Asansör Montajcısı",
                CrewComposition = "1 Asansör Mühendisi + 3 Montaj Ustası",
                CrewSize = 4,
                DurationDays = 12,
                EquipmentNeeded = "Kuyu İçi Tirfor / Halatlı Çektirme, Lazer Şakül, Tork Anahtarları",
                Predecessors = "Kuyu dibi su yalıtımı, şaft duvarları ve asansör makine dairesi / kule betonu",
                CriticalDependency = "A Tipi Muayene Kuruluşu yeşil etiket vermeden ve tescil ruhsatı çıkmadan asansör çalıştırılamaz!",
                DnaSource = "Adım 8: Asansör Sistemleri",
                PlannedStartDate = m04Start,
                PlannedEndDate = m04End,
                SubSteps = CreateSubSteps(
                    "T-5 Kuyu şakülü atılması, kuyu dibi temizliği ve ray konsollarının kontrolü",
                    "Kılavuz ray montajı, makine/motor montajı, kabin ve paraşüt fren sistemi kurulumu",
                    "A Tipi Muayene Kuruluşu yeşil etiket kabulü ve belediye tescil belgesi",
                    "Kuyu içi yaşam hattı, kuyu kapıları koruyucu paravanları ve revizyon kumanda denetimi"
                )
            });

            // M-05: Kat Holleri / Yangın Merdiveni Mermer Kaplama & Yangın Dolapları (Adım 8)
            var m05Start = m04Start.AddDays(2);
            var m05End = m05Start.AddDays(8);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "M-05",
                Order = stepOrderSeq++,
                PhaseName = "Kat Holleri / Yangın Merdiveni Mermer Kaplama & Yangın Dolapları",
                StageCategory = "Ortak Alanlar & Yangın Güvenliği",
                PartIndex = p4Idx,
                PartName = p4Name,
                PartBadgeClass = p4Badge,
                TriggerRule = p4Trigger,
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "Kat Holü / Merdiven Mermerleri, EI90 Yangın Kaçış Kapıları ve Yangın Dolapları",
                MaterialDescription = "Basamak ve kat koridorları mermer kaplama, panik barlı yangın merdiven kapıları ve yangın dolabı lans setleri.",
                MaterialQuantity = 0,
                MaterialUnit = "Kat Seti",
                MaterialFormula = "DXF Bekliyor (Merdiven basamak detayları ve kat holü alanlarından hesaplanacak)",
                SupplierCategory = "Mermer Ocağı & Yangın Güvenlik Firması",
                LaborTitle = "Mermer & Yangın Güvenlik Ekibi",
                ProfessionName = "Mermerci & Yangın Ekibi",
                CrewComposition = "1 Formen + 4 Mermerci + 2 Yangın Tesisatçısı",
                CrewSize = 7,
                DurationDays = 8,
                EquipmentNeeded = "Sulu Mermer Kesici, Lazer Hizalama, Boru Pres Makinesi",
                Predecessors = "Kat koridorları ve merdiven kova kaba sıvalarının tamamlanması",
                CriticalDependency = "İtfaiye yangın uygunluk raporu için yangın kapıları duman fitilleri ve yaylı menteşeleri eksiksiz olmalıdır!",
                DnaSource = "Adım 8: Yangın Güvenliği & Ortak Alanlar",
                PlannedStartDate = m05Start,
                PlannedEndDate = m05End,
                SubSteps = CreateSubSteps(
                    "T-3 Yangın kaçış kapıları sertifikaları, basamak mermerleri ve hortum dolapları sevki",
                    "Merdiven mermer kaplama, yangın kaçış kapıları panik bar montajı ve kat dolapları montajı",
                    "Kapı yaylı otomatik kapanma testi, duman sızdırmazlık fitili ve yangın hattı su testi",
                    "Yangın merdiveni geçici aydınlatması ve basamak kaydırmaz bant uygulaması"
                )
            });

            // M-06: Dış Çevre Düzenleme, Peyzaj, Bahçe Duvarı & Dış Aydınlatma (Adım 9)
            var m06Start = s06End.AddDays(2);
            var m06End = m06Start.AddDays(10);
            list.Add(new WorkflowStepItemDto
            {
                StepCode = "M-06",
                Order = stepOrderSeq++,
                PhaseName = "Dış Çevre Düzenleme, Peyzaj, Bahçe Duvarı & Dış Aydınlatma",
                StageCategory = "Peyzaj & Çevre Düzenleme",
                PartIndex = p4Idx,
                PartName = p4Name,
                PartBadgeClass = p4Badge,
                TriggerRule = p4Trigger,
                TargetStructureName = targetStructureName,
                TargetBuildingId = targetBuildingId,
                MaterialTitle = "Bahçe Duvarı Taşları, Kilit Parke Taşı, Bitkisel Toprak ve LED Dış Aydınlatma Projektörleri",
                MaterialDescription = "Site bahçe çevre istinat duvarları, açık otopark kilit taşları, otomatik bahçe sulama, rulo çim ve çevre projektörleri.",
                MaterialQuantity = 0,
                MaterialUnit = "m²",
                MaterialFormula = "DXF Bekliyor (Vaziyet planı peyzaj ve sert zemin metrajlarından hesaplanacak)",
                SupplierCategory = "Peyzaj Fidanlığı & Kilit Taşı Üreticisi",
                LaborTitle = "Peyzaj ve Dış Saha Düzenleme Ekibi",
                ProfessionName = "Peyzaj & Taş Ustası",
                CrewComposition = "1 Peyzaj Mimarı / Formen + 4 Taş Ustası + 4 Bahçıvan/İşçi",
                CrewSize = 9,
                DurationDays = 10,
                EquipmentNeeded = "Mini Beko Loder, Kompaktör, Taş Kırma Giyotini, Çim Havalandırma",
                Predecessors = "Ağır kule vinç sökümü, şantiye konteynerlerinin kaldırılması ve dış saha temizliği",
                CriticalDependency = "Ağır şantiye makineleri sahadan çıkmadan peyzaj ve dış kaplamalar yapılamaz!",
                DnaSource = "Adım 9: Peyzaj & Dış Çevre",
                PlannedStartDate = m06Start,
                PlannedEndDate = m06End,
                SubSteps = CreateSubSteps(
                    "T-3 Çevre düzenleme kot planı, kilit taşı ve bitkisel toprak sevki",
                    "Bahçe duvarları, otopark/yürüyüş yolu parke taşları, otomatik sulama ve bitki dikimi",
                    "Yağmur suyu çevre drenaj eğimleri ve çevre aydınlatma linye testi",
                    "Saha çevre aydınlatması ve hendek emniyet şeritleri"
                )
            });

            return list;
        }

        public class ParametricTakeoffItemDto
        {
            public int Id { get; set; }
            public string Category { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Formula { get; set; }
            public decimal Quantity { get; set; }
            public string Unit { get; set; }
            public string ProfessionName { get; set; }
            public string DnaSource { get; set; }
            public string Status { get; set; } = "Parametrik Hesaplandı";
        }

        public class SendParametricB2BDto
        {
            public int ProjectId { get; set; }
            public int? ProfileId { get; set; }
            public List<int> SelectedItemIds { get; set; }
        }

        public class WorkflowStepItemDto
        {
            public string StepCode { get; set; }
            public int Order { get; set; }
            public string PhaseName { get; set; }
            public string StageCategory { get; set; }

            // 4 Parçalı Hiyerarşi & Yapı Bağı
            public int PartIndex { get; set; } = 1; // 1: Kaba İnşaat, 2: Statik İnce, 3: Tercihe Göre İnce, 4: Demirbaş & Montaj
            public string PartName { get; set; } = "1. Parça: Kaba İnşaat (Temel & Taşıyıcı İskelet)";
            public string PartBadgeClass { get; set; } = "primary";
            public string TriggerRule { get; set; } = "İşbaşı Tarihine Göre Kat Kat Başlar";
            public string TargetStructureName { get; set; } = "";
            public int? TargetBuildingId { get; set; }

            // Malzeme Bacağı
            public string MaterialTitle { get; set; }
            public string MaterialDescription { get; set; }
            public decimal MaterialQuantity { get; set; }
            public string MaterialUnit { get; set; }
            public string MaterialFormula { get; set; }
            public string SupplierCategory { get; set; }

            // İş Gücü & Taşeron Bacağı
            public string LaborTitle { get; set; }
            public string ProfessionName { get; set; }
            public string CrewComposition { get; set; }
            public int CrewSize { get; set; }
            public int DurationDays { get; set; }
            public int TotalManDays => CrewSize * DurationDays;
            public string EquipmentNeeded { get; set; }

            // İş Akışı & Bağımlılık
            public string Predecessors { get; set; }
            public string CriticalDependency { get; set; }
            public string DnaSource { get; set; }

            // Takvim
            public DateTime? PlannedStartDate { get; set; }
            public DateTime? PlannedEndDate { get; set; }
            public string PlannedDateRange => PlannedStartDate.HasValue && PlannedEndDate.HasValue
                ? $"{PlannedStartDate.Value:dd.MM.yyyy} - {PlannedEndDate.Value:dd.MM.yyyy}" : "";

            // 4 Hayati Alt Aşama & Belgelendirme (Ön Hazırlık, İmalat, Kalite/Kür, İSG)
            public List<WorkflowSubStepDto> SubSteps { get; set; } = new List<WorkflowSubStepDto>();
        }

        public class WorkflowSubStepDto
        {
            public string Title { get; set; } = "";
            public string Category { get; set; } = "PrePreparation"; // "PrePreparation", "Execution", "QualityAndCuring", "SafetyIsg"
            public bool IsCompleted { get; set; }
            public string? TestResultValue { get; set; } // Örn: "Slump: 18 cm", "28G Kırım: 34.6 MPa"
            public string? PhotoUrl { get; set; }
            public string? DocumentUrl { get; set; }
            public string? Notes { get; set; }
        }

        public class SendWorkflowB2BDto
        {
            public int ProjectId { get; set; }
            public int? ProfileId { get; set; }
            public int? BuildingId { get; set; }
            public string TargetTrack { get; set; } // "Material" or "Labor"
            public List<string> SelectedStepCodes { get; set; }
        }

        public class TriggerWorkflowAgendaDto
        {
            public int ProjectId { get; set; }
            public int? ProfileId { get; set; }
            public int? BuildingId { get; set; }
            public DateTime StartDate { get; set; }
            public List<string> SelectedStepCodes { get; set; }
        }
    }
}


