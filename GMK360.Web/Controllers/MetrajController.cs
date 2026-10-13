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

        public MetrajController(ApplicationDbContext context)
        {
            _context = context;
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
                .Where(p => p.ProjectId == id)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            return View();
        }

        public class DnaProfileSaveDto
        {
            public int ProjectId { get; set; }
            public string Name { get; set; }
            public List<DnaProfileDetailDto> Details { get; set; }
        }

        public class DnaProfileDetailDto
        {
            public string Phase { get; set; }
            public string SelectedItem { get; set; }
            public int ValueId { get; set; } // OptionId
        }

        [HttpPost]
        public IActionResult SaveDnaProfile([FromBody] DnaProfileSaveDto data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.Name)) return BadRequest();

            var profile = new ProjectDnaProfile
            {
                ProjectId = data.ProjectId,
                Name = data.Name,
                Status = "DXF Bekliyor",
                CreatedBy = User.Identity?.Name ?? "Admin",
                CreatedDate = DateTime.UtcNow
            };

            foreach (var d in data.Details ?? new List<DnaProfileDetailDto>())
            {
                var option = _context.DnaTemplateOptions.FirstOrDefault(o => o.Id == d.ValueId);
                
                profile.Details.Add(new ProjectDnaDetail
                {
                    Phase = d.Phase,
                    SelectedItem = d.SelectedItem,
                    DnaProfessionId = option?.DnaProfessionId,
                    Quantity = 0,
                    Unit = "Belirsiz"
                });
            }

            _context.ProjectDnaProfiles.Add(profile);
            _context.SaveChanges();

            return Ok();
        }

        [HttpPost]
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
    }
}


