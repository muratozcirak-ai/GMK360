using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        // Remove old SaveStep1 entirely
        int step1Start = code.IndexOf("[HttpPost]\r\n        public async Task<IActionResult> SaveStep1");
        if(step1Start == -1) step1Start = code.IndexOf("        [HttpPost]\n        public async Task<IActionResult> SaveStep1");
        
        if (step1Start != -1)
        {
            int step1End = code.IndexOf("return Json(new { success = true, projectId = project.Id });\r\n        }");
            if (step1End == -1) step1End = code.IndexOf("return Json(new { success = true, projectId = project.Id });\n        }");
            
            if (step1End != -1) {
                step1End += "return Json(new { success = true, projectId = project.Id });\r\n        }".Length;
                code = code.Remove(step1Start, step1End - step1Start);
                
                string newSaveStep1 = @"
        [HttpPost]
        public async Task<IActionResult> SaveStep1([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)
        {
            try 
            {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null) return Json(new { success = false, message = ""Yetkisiz erişim. Oturumunuz kapanmış olabilir."" });

                bool addressExists = await _context.ConstructionProjects.AnyAsync(p => 
                    p.Address == model.Address && 
                    p.Name != model.Name 
                );
                
                if (addressExists)
                {
                    return Json(new { success = false, message = ""Bu adreste halihazırda kayıtlı bir yapı/proje bulunmaktadır. Aynı adrese ikinci bir bina eklenemez!"" });
                }

                var project = new ConstructionProject
                {
                    Name = model.Name,
                    Description = model.Description,
                    AgencyId = agencyId.Value,
                    Address = model.Address ?? """",
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    TotalLandArea = model.TotalLandArea,
                    Status = 0 // Draft
                };

                _context.ConstructionProjects.Add(project);
                await _context.SaveChangesAsync();

                return Json(new { success = true, projectId = project.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ""Kayıt sırasında sistemsel bir hata oluştu: "" + ex.Message });
            }
        }";
                code = code.Insert(step1Start, newSaveStep1);
                Console.WriteLine("Replaced SaveStep1.");
            }
        }
        
        File.WriteAllText(path, code, new UTF8Encoding(true));
    }
}
