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

        // Find the start of CreateWizard
        int startIdx = code.IndexOf("public async Task<IActionResult> CreateWizard");
        if (startIdx == -1) return;

        // Find the end of CreateWizard by looking for the next method or end of class
        int endIdx = code.IndexOf("public async Task<IActionResult> Create(", startIdx);
        
        if (endIdx == -1) return;

        string oldMethod = code.Substring(startIdx, endIdx - startIdx);

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
        }

        ";

        code = code.Replace(oldMethod, newCode);
        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Safely replaced CreateWizard");
    }
}
