using System;
using System.Collections.Generic;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Construction.Workflows;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
            ViewData["ProjectId"] = projectId;
            ViewData["RecipeId"] = recipeId ?? 1; // Default fallback for demo
            return View();
        }

        
        
        public IActionResult ProjectDNA(int id)
        {
            ViewData["ProjectId"] = id;
            
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
            ViewBag.SavedProfiles = _context.ProjectDnaProfiles
                .Include(p => p.Details)
                    .ThenInclude(d => d.Profession)
                .Where(p => p.ProjectId == id)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            return View();
        }

        // YENİ TAM SAYFA DNA SİHİRBAZI (YENİ SEKME)
        public IActionResult DnaWizard(int id, int? profileId, int? startStep = null)
        {
            var project = _context.ConstructionProjects.FirstOrDefault(p => p.Id == id);
            if (project == null) return NotFound();

            ViewData["ProjectId"] = id;
            ViewData["ProjectName"] = project.Name;

            // ADIM 0 (Mecburi Statik ve Taşıyıcılar - 20 Soru)
            var step0 = _context.DnaTemplateSteps
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Profession)
                .FirstOrDefault(s => s.StepOrder == 0 || s.Id == 2);

            if (step0 != null && step0.Questions != null)
            {
                step0.Questions = step0.Questions.OrderBy(q => q.QuestionOrder).ToList();
            }
            ViewBag.Step0 = step0;

            // ADIM 1 (Kot 0 Zemin, Temel ve İzolasyon)
            var step1 = _context.DnaTemplateSteps
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
                // Parametre verilmediyse projedeki en son aktif taslağı otomatik getir (Kaldığı yerden devam etsin)
                var latestDraft = _context.ProjectDnaProfiles
                    .Include(p => p.Details)
                        .ThenInclude(d => d.Profession)
                    .Where(p => p.ProjectId == id && p.Status == "Taslak")
                    .OrderByDescending(p => p.CreatedDate)
                    .FirstOrDefault();
                if (latestDraft != null)
                {
                    ViewBag.ExistingProfile = latestDraft;
                }
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
    }
}


