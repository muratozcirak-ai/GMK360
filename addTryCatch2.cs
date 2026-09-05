using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string newSaveStep2 = @"
        [HttpPost]
        public async Task<IActionResult> SaveStep2([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)
        {
            try 
            {
                if (model.DraftProjectId == 0) return Json(new { success = false, message = ""Proje ID bulunamadı."" });
                
                var project = await _context.ConstructionProjects
                    .Include(p => p.Blocks)
                    .FirstOrDefaultAsync(p => p.Id == model.DraftProjectId);
                    
                if (project == null) return Json(new { success = false, message = ""Proje bulunamadı."" });

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
                            BlockName = b.BlockName,
                            BaseArea = b.BaseArea,
                            BasementFloors = b.BasementFloors,
                            TotalFloors = b.TotalFloors,
                            HasRoof = b.HasRoof,
                            CityId = 1, 
                            DistrictId = 1,
                            NeighborhoodId = 1
                        };
                        blocks.Add(building);
                    }
                    project.Blocks = blocks;
                }
                
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ""Blokları kaydederken hata oluştu: "" + ex.Message + (ex.InnerException != null ? "" - "" + ex.InnerException.Message : """") });
            }
        }";

        if (code.Contains("public async Task<IActionResult> SaveStep2"))
        {
            int startIdx = code.IndexOf("[HttpPost]\r\n        public async Task<IActionResult> SaveStep2");
            if (startIdx == -1) startIdx = code.IndexOf("[HttpPost]\n        public async Task<IActionResult> SaveStep2");

            if (startIdx != -1) {
                int endIdx = code.IndexOf("return Json(new { success = true });", startIdx);
                if (endIdx != -1) {
                    endIdx = code.IndexOf("}", endIdx) + 1;
                    code = code.Remove(startIdx, endIdx - startIdx);
                    code = code.Insert(startIdx, newSaveStep2);
                    Console.WriteLine("Wrapped SaveStep2 in try/catch.");
                }
            }
        }

        File.WriteAllText(path, code, new UTF8Encoding(true));
    }
}
