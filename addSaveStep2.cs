using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string saveStep2Action = @"
        [HttpPost]
        public async Task<IActionResult> SaveStep2([FromForm] CreateProjectWizardViewModel model)
        {
            if (model.DraftProjectId == 0) return Json(new { success = false, message = ""Proje ID bulunamadı."" });
            
            var project = await _context.ConstructionProjects
                .Include(p => p.Blocks)
                .FirstOrDefaultAsync(p => p.Id == model.DraftProjectId);
                
            if (project == null) return Json(new { success = false, message = ""Proje bulunamadı."" });

            // Mevcut blokları temizle (tekrar gelmişse)
            if (project.Blocks != null && project.Blocks.Any())
            {
                _context.Buildings.RemoveRange(project.Blocks);
            }

            var blocks = new List<Building>();
            if (model.Blocks != null)
            {
                foreach (var b in model.Blocks)
                {
                    var building = new Building
                    {
                        Name = b.BlockName,
                        BaseArea = b.BaseArea,
                        BasementFloors = b.BasementFloors,
                        TotalFloors = b.TotalFloors,
                        HasRoof = b.HasRoof,
                        // HasGroundFloor mapping vs if needed
                        AgencyId = project.AgencyId
                    };
                    blocks.Add(building);
                }
                project.Blocks = blocks;
            }
            
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
";
        if (!code.Contains("SaveStep2"))
        {
            int insertIndex = code.IndexOf("public async Task<IActionResult> SaveStep1");
            if (insertIndex != -1)
            {
                code = code.Insert(insertIndex, saveStep2Action);
                File.WriteAllText(path, code, new UTF8Encoding(true));
                Console.WriteLine("Added SaveStep2 to controller.");
            }
        }
    }
}
