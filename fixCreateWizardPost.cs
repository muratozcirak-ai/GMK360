using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string find = @"public async Task<IActionResult> CreateWizard\(\[FromForm\] GMK360\.Web\.Models\.CreateProjectWizardViewModel model\)\s*\{.*?return RedirectToAction\(nameof\(Index\)\);\s*\}";
        string replace = @"[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWizard([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            if (model.DraftProjectId > 0)
            {
                var draft = await _context.ConstructionProjects
                    .Include(p => p.Blocks)
                    .FirstOrDefaultAsync(p => p.Id == model.DraftProjectId && p.AgencyId == agencyId.Value);

                if (draft != null)
                {
                    draft.Status = 1; // Mark as Active / Devam Ediyor
                    await _context.SaveChangesAsync();
                    
                    var firstBlock = draft.Blocks?.FirstOrDefault();
                    if (firstBlock != null)
                    {
                        TempData[""SuccessMessage""] = ""Şantiye başarıyla başlatıldı ve bloklar oluşturuldu."";
                        return RedirectToAction(""ManageBlock"", new { id = firstBlock.Id });
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            
            TempData[""ErrorMessage""] = ""Şantiye başlatılırken bir hata oluştu: Proje kimliği doğrulanamadı."";
            return RedirectToAction(nameof(Index));
        }";

        code = Regex.Replace(code, find, replace, RegexOptions.Singleline);
        
        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Added HttpPost and ValidateAntiForgeryToken to CreateWizard.");
    }
}
