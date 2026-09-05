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

        string find = @"public async Task<IActionResult> CreateWizard\(\[FromForm\] GMK360\.Web\.Models\.CreateProjectWizardViewModel model\).*?return RedirectToAction\(nameof\(Details\), new \{ id = project\.Id \} \);\s*\}\s*catch \(Exception ex\)";
        
        string replace = @"public async Task<IActionResult> CreateWizard([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)
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

                string uploadedImageUrl = """";
                string currentStateImageUrl = """";

                if (model.CoverImageFile != null && model.CoverImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, ""uploads"", ""projects"", ""covers"");
                    Directory.CreateDirectory(uploadsFolder);
                    
                    string uniqueFileName = Guid.NewGuid().ToString() + ""_"" + Path.GetFileName(model.CoverImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CoverImageFile.CopyToAsync(fileStream);
                    }
                    
                    uploadedImageUrl = ""/uploads/projects/covers/"" + uniqueFileName;
                    project.CoverImageUrl = uploadedImageUrl;
                }

                if (model.CurrentStateImageFile != null && model.CurrentStateImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, ""uploads"", ""projects"", ""current"");
                    Directory.CreateDirectory(uploadsFolder);
                    
                    string uniqueFileName = Guid.NewGuid().ToString() + ""_"" + Path.GetFileName(model.CurrentStateImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CurrentStateImageFile.CopyToAsync(fileStream);
                    }
                    
                    currentStateImageUrl = ""/uploads/projects/current/"" + uniqueFileName;
                }

                // Update status to active
                project.Status = 1; // 1 = Devam Ediyor
                await _context.SaveChangesAsync();

                // Handle DMS Documents if images uploaded
                if (!string.IsNullOrEmpty(uploadedImageUrl) || !string.IsNullOrEmpty(currentStateImageUrl))
                {
                    var defaultFolder = _context.DmsFolders.FirstOrDefault();
                    if (defaultFolder == null)
                    {
                        var cabinet = _context.DmsCabinets.FirstOrDefault();
                        if (cabinet == null) {
                            cabinet = new GMK360.Core.Entities.DmsCabinet { Name = ""Genel Evraklar"", Description = ""Sistem tarafından oluşturuldu"", UserId = _userManager.GetUserId(User) ?? """", CreatedAt = DateTime.UtcNow };
                            _context.DmsCabinets.Add(cabinet);
                            await _context.SaveChangesAsync();
                        }
        
                        var shelf = _context.DmsShelves.FirstOrDefault();
                        if (shelf == null) {
                            shelf = new GMK360.Core.Entities.DmsShelf { Name = ""Genel Arşiv"", Description = ""Sistem tarafından oluşturuldu"", CabinetId = cabinet.Id, CreatedAt = DateTime.UtcNow };
                            _context.DmsShelves.Add(shelf);
                            await _context.SaveChangesAsync();
                        }
        
                        defaultFolder = new GMK360.Core.Entities.DmsFolder 
                        { 
                            Name = ""Genel Proje Evrakları"", 
                            Description = ""Sistem tarafından oluşturuldu"",
                            ShelfId = shelf.Id,
                            CreatedAt = DateTime.UtcNow 
                        };
                        _context.DmsFolders.Add(defaultFolder);
                        await _context.SaveChangesAsync();
                    }
                    
                    if (!string.IsNullOrEmpty(uploadedImageUrl))
                    {
                        var dmsDoc = new GMK360.Core.Entities.DmsDocument
                        {
                            Title = ""Proje Görseli (Geleceği Hal)"",
                            DocumentUrl = uploadedImageUrl,
                            FileExtension = model.CoverImageFile != null ? Path.GetExtension(model.CoverImageFile.FileName) : """",
                            FileSizeBytes = model.CoverImageFile != null ? model.CoverImageFile.Length : 0,
                            EntityType = ""ConstructionProject"",
                            EntityId = project.Id,
                            UploadDate = DateTime.UtcNow,
                            UploadedByUserId = _userManager.GetUserId(User) ?? """",
                            FolderId = defaultFolder.Id,
                            PhysicalLocationNote = """"
                        };
                        _context.DmsDocuments.Add(dmsDoc);
                    }
        
                    if (!string.IsNullOrEmpty(currentStateImageUrl))
                    {
                        var dmsDoc = new GMK360.Core.Entities.DmsDocument
                        {
                            Title = ""Mevcut Durum Görseli (İlk Hali)"",
                            DocumentUrl = currentStateImageUrl,
                            FileExtension = model.CurrentStateImageFile != null ? Path.GetExtension(model.CurrentStateImageFile.FileName) : """",
                            FileSizeBytes = model.CurrentStateImageFile != null ? model.CurrentStateImageFile.Length : 0,
                            EntityType = ""ConstructionProject"",
                            EntityId = project.Id,
                            UploadDate = DateTime.UtcNow,
                            UploadedByUserId = _userManager.GetUserId(User) ?? """",
                            FolderId = defaultFolder.Id,
                            PhysicalLocationNote = """"
                        };
                        _context.DmsDocuments.Add(dmsDoc);
                    }
                    
                    await _context.SaveChangesAsync();
                }

                TempData[""SuccessMessage""] = ""Şantiye başarıyla başlatıldı ve bloklar oluşturuldu."";
                return RedirectToAction(nameof(Details), new { id = project.Id });
            }
            catch (Exception ex)";

        if (Regex.IsMatch(code, find, RegexOptions.Singleline))
        {
            code = Regex.Replace(code, find, replace, RegexOptions.Singleline);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Fixed CreateWizard to just activate Draft.");
        }
        else
        {
            Console.WriteLine("Could not find CreateWizard.");
        }
    }
}
