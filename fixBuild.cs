using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string ctrlPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        var ctrl = File.ReadAllText(ctrlPath, Encoding.UTF8);

        string pattern = @"public async Task<IActionResult> UploadArchitectureMedia\(.*?return RedirectToAction\(""ManageBlock"", new \{ id = buildingId \}\);\s*\}";
        
        string newMethod = @"public async Task<IActionResult> UploadArchitectureMedia(int buildingId, string entityType, int entityId, string title, string PhysicalLocationNote, IFormFile file)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var block = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (block == null || (!User.IsInRole(""Admin"") && block.ConstructionProject.AgencyId != agencyId))
            {
                return Unauthorized();
            }

            if (file != null && file.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, ""uploads"", ""architecture"");
                Directory.CreateDirectory(uploadsFolder);
                
                string uniqueFileName = Guid.NewGuid().ToString() + ""_"" + Path.GetFileName(file.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Default FolderId is 0 or needs to be set to 1 if 1 exists. We'll set it to 1 just in case, but if they had it as 0 before, we'll leave it as default.
                // Wait, let's just get the first folder, if any, or default to 1.
                var firstFolder = await _context.DmsFolders.FirstOrDefaultAsync();
                int folderId = firstFolder != null ? firstFolder.Id : 1;

                var doc = new GMK360.Core.Entities.DmsDocument
                {
                    Title = title ?? file.FileName,
                    DocumentUrl = ""/uploads/architecture/"" + uniqueFileName,
                    FileExtension = Path.GetExtension(file.FileName),
                    FileSizeBytes = file.Length,
                    EntityType = entityType, 
                    EntityId = entityId,
                    UploadedByUserId = _userManager.GetUserId(User) ?? """",
                    UploadDate = DateTime.UtcNow,
                    FolderId = folderId,
                    PhysicalLocationNote = string.IsNullOrWhiteSpace(PhysicalLocationNote) ? ""Belirtilmedi"" : PhysicalLocationNote
                };

                _context.DmsDocuments.Add(doc);
                await _context.SaveChangesAsync();
                TempData[""SuccessMessage""] = $""{title} başarıyla yüklendi."";
            }

            return RedirectToAction(""ManageBlock"", new { id = buildingId });
        }";

        if (Regex.IsMatch(ctrl, pattern, RegexOptions.Singleline))
        {
            ctrl = Regex.Replace(ctrl, pattern, newMethod, RegexOptions.Singleline);
            File.WriteAllText(ctrlPath, ctrl, new UTF8Encoding(true));
            Console.WriteLine("Controller UploadArchitectureMedia fixed and simplified.");
        }
    }
}
