using Microsoft.AspNetCore.Mvc;
using GMK360.Web.Services.Construction;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Construction;
using GMK360.Core.Entities.Construction.Workflows;
using System.Linq;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectManagerApiController : ControllerBase
    {
        private readonly ProjectManagerService _projectManagerService;
        private readonly ApplicationDbContext _context;

        public ProjectManagerApiController(ProjectManagerService projectManagerService, ApplicationDbContext context)
        {
            _projectManagerService = projectManagerService;
            _context = context;
        }

        
        
        [HttpPost("test-initialize-project")]
        public IActionResult TestInitializeProject()
        {
            var mockProject = new ConstructionProject
            {
                Name = "Big Bang Projesi (Baza + 2 Kule + Matrix)",
                Description = "Tüm sistemin çalıştığı test",
                AgencyId = 1,
                OuterWallType = "Ytong",
                PlumbingType = "Gomme",
                ElevatorType = "Klasik",
                Address = "Big Bang Sokak",
                StartDate = System.DateTime.UtcNow,
                LifecycleStatus = GMK360.Core.Entities.Enums.ProjectLifecycleStatus.UnderConstruction,
                Status = (GMK360.Core.Entities.Construction.ProjectStatus)1
            };
            
            var agency = _context.Agencies.FirstOrDefault();
            if (agency != null) {
                mockProject.AgencyId = agency.Id;
            }

            _context.ConstructionProjects.Add(mockProject);
            _context.SaveChanges();

            // Tetikle! 2 Bodrum, 1 Baza, 3 Kat A Kulesi, 3 Kat B Kulesi
            _projectManagerService.InitializeProject(mockProject.Id, 2, 1, 3, 3);

            var totalTasks = _context.ProjectTasks.Count(t => t.ProjectId == mockProject.Id);
            var totalElevations = _context.ProjectElevations.Count(e => e.ProjectId == mockProject.Id);

            return Ok(new {
                Message = "BÜYÜK PATLAMA (BIG BANG) BAŞARILI!",
                ProjectDNA = mockProject.Name,
                TotalElevationsCreated = totalElevations,
                TotalTasksGenerated = totalTasks
            });
        }

        [HttpPost("test-generate-tree")]
        public IActionResult TestGenerateTree()
        {
            // 1. Create a mock project
            var mockProject = new ConstructionProject
            {
                Name = "Karma Proje (Baza + 2 Kule)",
                Description = "Ağaç Hiyerarşisi Test Projesi",
                AgencyId = 1,
                Address = "Test Mah.",
                StartDate = System.DateTime.UtcNow,
                LifecycleStatus = GMK360.Core.Entities.Enums.ProjectLifecycleStatus.UnderConstruction,
                Status = (GMK360.Core.Entities.Construction.ProjectStatus)1
            };
            
            var agency = _context.Agencies.FirstOrDefault();
            if (agency != null) {
                mockProject.AgencyId = agency.Id;
            }

            _context.ConstructionProjects.Add(mockProject);
            _context.SaveChanges();

            // 2. 2 Bodrum, 2 Baza Katı, 5 Kat A Kulesi, 8 Kat B Kulesi
            _projectManagerService.GenerateElevationTree(mockProject.Id, 2, 2, 5, 8);

            // 3. Ağacı (Tree) Veritabanından Hiyerarşik Olarak Çekelim
            var elevations = _context.ProjectElevations
                .Where(e => e.ProjectId == mockProject.Id)
                .OrderBy(e => e.ElevationLevel)
                .Select(e => new {
                    e.Id,
                    e.BlockName,
                    e.Name,
                    e.ElevationLevel,
                    ParentId = e.ParentElevationId,
                    ParentName = e.ParentElevation != null ? e.ParentElevation.Name : "YOK (Temel)"
                }).ToList();

            return Ok(new {
                Message = "Ağaç (Tree) Hiyerarşisi Mükemmel Kuruldu!",
                TotalElevations = elevations.Count,
                Tree = elevations
            });
        }

        [HttpPost("test-generate-matrix")]
        public IActionResult TestGenerateMatrix()
        {
            // 1. Create a mock DNA project
                        var mockProject = new ConstructionProject
            {
                Name = "Test DNA Projesi",
                Description = "Ytong ve Gömme Tesisat test projesi",
                AgencyId = 1, 
                OuterWallType = "Ytong",
                PlumbingType = "Gomme",
                ElevatorType = "MRL",
                Address = "Test Mah. Test Sok.",
                StartDate = System.DateTime.UtcNow,
                LifecycleStatus = GMK360.Core.Entities.Enums.ProjectLifecycleStatus.UnderConstruction,
                Status = (GMK360.Core.Entities.Construction.ProjectStatus)1
            };
            
            // Check if Agency 1 exists
            var agency = _context.Agencies.FirstOrDefault();
            if (agency != null) {
                mockProject.AgencyId = agency.Id;
            }

            _context.ConstructionProjects.Add(mockProject);
            _context.SaveChanges();

            // 2. Create a mock Elevation
            var mockElevation = new ProjectElevation
            {
                ProjectId = mockProject.Id,
                BlockName = "A Kulesi",
                Name = "1. Kat",
                ElevationLevel = 1
            };
            _context.ProjectElevations.Add(mockElevation);
            _context.SaveChanges();

            // 3. Trigger Karar Motoru
            _projectManagerService.GenerateMatrixForElevation(mockProject.Id, mockElevation.Id);

            // 4. Return the generated tasks
            var generatedTasks = _context.ProjectTasks
                .Where(t => t.ProjectId == mockProject.Id && t.ElevationId == mockElevation.Id)
                .Select(t => t.TaskName)
                .ToList();

            return Ok(new {
                Message = "Matris Başarıyla Oluşturuldu!",
                ProjectDNA = new {
                    mockProject.OuterWallType,
                    mockProject.PlumbingType,
                    mockProject.ElevatorType
                },
                GeneratedTaskCount = generatedTasks.Count,
                Tasks = generatedTasks
            });
        }
    }
}
