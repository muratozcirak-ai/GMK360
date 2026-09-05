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

        // We will replace the entire CreateWizard method to just finalize the draft project
        string pattern = @"public async Task<IActionResult> CreateWizard\(\[FromForm\] GMK360\.Web\.Models\.CreateProjectWizardViewModel model\)\s*\{.*?return RedirectToAction\(nameof\(ManageBlock\), new \{ id = buildingId \}\);\s*\}";
        
        string newCode = @"public async Task<IActionResult> CreateWizard([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)
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
            
            return RedirectToAction(nameof(Index));
        }";

        code = Regex.Replace(code, pattern, newCode, RegexOptions.Singleline);
        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed CreateWizard to finalize draft instead of duplicating.");
    }
}
