using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        // We need to modify [HttpGet] Create
        string oldCreate = @"        public async Task<IActionResult> Create()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            // Sadece Admin veya ilgili agency'nin verilerini getirmek iin
            // imdilik varsaylan bo model dnyoruz.
            return View();
        }";

        // The old create might not be async depending on how I left it. Let's find it safely.
        int createIdx = code.IndexOf("public async Task<IActionResult> Create()");
        if (createIdx == -1) createIdx = code.IndexOf("public IActionResult Create()");

        if (createIdx != -1)
        {
            // Find end of Create method
            int start = code.LastIndexOf("[HttpGet]", createIdx);
            if (start == -1) start = code.LastIndexOf("public ", createIdx);
            
            int end = code.IndexOf("return View", createIdx);
            end = code.IndexOf("}", end) + 1;

            string newCreate = @"        [HttpGet]
        public async Task<IActionResult> Create(int? id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            if (id.HasValue)
            {
                var project = await _context.ConstructionProjects
                    .Include(p => p.Blocks)
                    .FirstOrDefaultAsync(p => p.Id == id.Value && p.AgencyId == agencyId.Value);

                if (project == null) return NotFound();

                var model = new GMK360.Web.Models.CreateProjectWizardViewModel
                {
                    DraftProjectId = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    Address = project.Address,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    TotalLandArea = project.TotalLandArea,
                    Blocks = project.Blocks.Select(b => new GMK360.Web.Models.WizardBlockViewModel 
                    { 
                        BlockName = b.Name,
                        BaseArea = b.BaseArea ?? 0,
                        BasementFloors = b.BasementFloors,
                        TotalFloors = b.TotalFloors ?? 0,
                        HasRoof = b.HasRoof
                    }).ToList()
                };
                return View(model);
            }

            return View(new GMK360.Web.Models.CreateProjectWizardViewModel());
        }";

            code = code.Remove(start, end - start);
            code = code.Insert(start, newCreate);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Replaced Create action.");
        }
    }
}
