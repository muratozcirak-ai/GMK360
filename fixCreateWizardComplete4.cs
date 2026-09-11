using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        int startIndex = code.IndexOf("public async Task<IActionResult> CreateWizard([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)");
        if (startIndex == -1)
        {
            Console.WriteLine("Could not find CreateWizard");
            return;
        }

        int endIndex = code.IndexOf("public async Task<IActionResult> Create([Bind", startIndex);
        if (endIndex == -1)
        {
            Console.WriteLine("Could not find Create method to end the replacement.");
            return;
        }

        // We need to keep the [HttpPost] and [ValidateAntiForgeryToken] for Create, so we should find the previous [HttpPost] or something, 
        // Actually it's easier to find the exact start of Create: `[HttpPost]\s*[ValidateAntiForgeryToken]\s*public async Task<IActionResult> Create([Bind`
        
        int actualEndIndex = code.LastIndexOf("[HttpPost]", endIndex);

        string newMethod = @"public async Task<IActionResult> CreateWizard([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)
        {
            try
            {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null) return Unauthorized();

                if (model.DraftProjectId <= 0)
                {
                    TempData[""ErrorMessage""] = ""Proje ID bulunamadı. Lütfen işleminizi baştan yapın."";
                    return RedirectToAction(nameof(Index));
                }

                var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == model.DraftProjectId);
                if (project == null)
                {
                    TempData[""ErrorMessage""] = ""Proje bulunamadı."";
                    return RedirectToAction(nameof(Index));
                }

                project.Status = 1; // 1 = Devam Ediyor
                await _context.SaveChangesAsync();

                TempData[""SuccessMessage""] = ""Şantiye başarıyla başlatıldı ve bloklar oluşturuldu."";
                return RedirectToAction(nameof(Details), new { id = project.Id });
            }
            catch (Exception ex)
            {
                TempData[""ErrorMessage""] = ""Bir hata oluştu: "" + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        ";

        string newCode = code.Substring(0, startIndex) + newMethod + code.Substring(actualEndIndex);
        File.WriteAllText(path, newCode, new UTF8Encoding(true));
        Console.WriteLine("REPLACED CREATEWIZARD PERFECTLY!");
    }
}
