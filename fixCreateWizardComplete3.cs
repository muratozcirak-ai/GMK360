using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        // Bulacağımız yer: public async Task<IActionResult> CreateWizard([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)
        // Ve bitişi: return RedirectToAction(nameof(Details), new { id = project.Id });
        // Sadece basit string işlemleriyle yapalım.

        int startIndex = code.IndexOf("public async Task<IActionResult> CreateWizard([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)");
        if (startIndex == -1)
        {
            Console.WriteLine("Could not find CreateWizard");
            return;
        }

        int endIndex = code.IndexOf("public async Task<IActionResult> Edit", startIndex);
        if (endIndex == -1)
        {
            Console.WriteLine("Could not find Edit method to end the replacement.");
            return;
        }

        string newMethod = @"public async Task<IActionResult> CreateWizard([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)
        {
            try
            {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null) return Unauthorized();

                if (model.DraftProjectId <= 0)
                {
                    TempData[""ErrorMessage""] = ""Proje ID bulunamadÄ±. LÃ¼tfen iÅŸleminizi baÅŸtan yapÄ±n."";
                    return RedirectToAction(nameof(Index));
                }

                var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == model.DraftProjectId);
                if (project == null)
                {
                    TempData[""ErrorMessage""] = ""Proje bulunamadÄ±."";
                    return RedirectToAction(nameof(Index));
                }

                project.Status = 1; // 1 = Devam Ediyor
                await _context.SaveChangesAsync();

                TempData[""SuccessMessage""] = ""Åžantiye baÅŸarÄ±yla baÅŸlatÄ±ldÄ± ve bloklar oluÅŸturuldu."";
                return RedirectToAction(nameof(Details), new { id = project.Id });
            }
            catch (Exception ex)
            {
                TempData[""ErrorMessage""] = ""Bir hata oluÅŸtu: "" + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        ";

        string newCode = code.Substring(0, startIndex) + newMethod + code.Substring(endIndex);
        File.WriteAllText(path, newCode, new UTF8Encoding(true));
        Console.WriteLine("REPLACED CREATEWIZARD SUCCESSFULLY!");
    }
}
