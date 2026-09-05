using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        // Put back Details
        string detailsMethod = @"        // GET: ConstructionProject/Details/5
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
            if (!User.IsInRole(""Admin"") && project.AgencyId != agencyId)
            {
                return Unauthorized();
            }

            return View(project);
        }
";
        
        int insertIdx = code.IndexOf("[HttpGet]\r\n        public async Task<IActionResult> Create(int? id)");
        if (insertIdx == -1) insertIdx = code.IndexOf("[HttpGet]\n        public async Task<IActionResult> Create(int? id)");
        
        if (insertIdx != -1) {
            code = code.Insert(insertIdx, detailsMethod);
        }

        // Fix CS0266 (DateTime conversion)
        code = code.Replace("StartDate = project.StartDate,", "StartDate = project.StartDate ?? DateTime.Now,");
        code = code.Replace("EndDate = project.EndDate,", "EndDate = project.EndDate ?? DateTime.Now.AddYears(1),");

        // Fix CS0234 (WizardBlockViewModel namespace)
        code = code.Replace("new GMK360.Web.Models.WizardBlockViewModel", "new GMK360.Web.Models.BlockWizardViewModel");

        // Remove the extra public IActionResult Create() and cities if needed (git diff showed it was deleted, which is fine, we replaced it).

        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Restored Details and fixed types.");
    }
}
