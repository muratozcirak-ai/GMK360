using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string saveStep1Action = @"
        [HttpPost]
        public async Task<IActionResult> SaveStep1([FromForm] CreateProjectWizardViewModel model)
        {
            var agencyId = HttpContext.Session.GetInt32(""CurrentAgencyId"") ?? 1;

            var project = new ConstructionProject
            {
                Name = model.Name,
                Description = model.Description,
                AgencyId = agencyId,
                Address = model.Address ?? """",
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                TotalLandArea = model.TotalLandArea,
                Status = 0 // Draft / Upcoming
            };

            _context.ConstructionProjects.Add(project);
            await _context.SaveChangesAsync();

            return Json(new { success = true, projectId = project.Id });
        }
";
        if (!code.Contains("SaveStep1"))
        {
            int insertIndex = code.IndexOf("public async Task<IActionResult> CreateWizard");
            if (insertIndex != -1)
            {
                code = code.Insert(insertIndex, saveStep1Action);
                File.WriteAllText(path, code, new UTF8Encoding(true));
                Console.WriteLine("Added SaveStep1 to controller.");
            }
        }
    }
}
