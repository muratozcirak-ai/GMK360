using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string controllerPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string controllerCode = File.ReadAllText(controllerPath, Encoding.UTF8);

        string saveStep1Find = @"public async Task<IActionResult> SaveStep1\(\[FromForm\] GMK360\.Web\.Models\.CreateProjectWizardViewModel model\)\s*\{\s*try \{\s*var agencyId = await GetUserAgencyIdAsync\(\);\s*if \(agencyId == null\) return Json\(new \{ success = false, message = ""Yetkisiz erişim\."" \}\);\s*GMK360\.Core\.Entities\.Construction\.ConstructionProject project;[\s\S]*?await _context\.SaveChangesAsync\(\);\s*return Json\(new \{ success = true, draftId = project\.Id, projectId = project\.Id \}\);\s*\} catch \(Exception ex\) \{";

        string saveStep1Replace = @"public async Task<IActionResult> SaveStep1([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)
        {
            try {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null) return Json(new { success = false, message = ""Yetkisiz erişim."" });

                GMK360.Core.Entities.Construction.ConstructionProject project;
                if (model.DraftProjectId > 0)
                {
                    project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == model.DraftProjectId);
                    if (project == null) return Json(new { success = false, message = ""Proje bulunamadı."" });
                }
                else
                {
                    project = new GMK360.Core.Entities.Construction.ConstructionProject { AgencyId = agencyId.Value, Status = 0, CreatedAt = DateTime.UtcNow };
                    _context.ConstructionProjects.Add(project);
                }

                project.Name = model.Name;
                project.Description = model.Description;
                project.Address = model.Address ?? ""Adres belirtilmedi"";
                
                if (model.StartDate.Year > 1) project.StartDate = model.StartDate;
                if (model.EndDate.Year > 1) project.EndDate = model.EndDate;
                
                project.TotalLandArea = model.TotalLandArea;
                project.CityId = model.CityId;
                project.DistrictId = model.DistrictId;
                project.NeighborhoodId = model.NeighborhoodId;
                project.StreetId = model.StreetId;
                project.TargetTotalApartments = model.TargetTotalApartments;
                project.TargetTotalShops = model.TargetTotalShops;
                project.Latitude = model.Latitude;
                project.Longitude = model.Longitude;

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
                    project.CoverImageUrl = ""/uploads/projects/covers/"" + uniqueFileName;
                }

                await _context.SaveChangesAsync();

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
                    var defaultFolder = await _context.DmsFolders.FirstOrDefaultAsync();
                    if (defaultFolder != null) {
                        var dmsDoc = new GMK360.Core.Entities.DmsDocument
                        {
                            Title = ""Mevcut Durum Görseli (İlk Hali)"",
                            DocumentUrl = ""/uploads/projects/current/"" + uniqueFileName,
                            FileExtension = Path.GetExtension(model.CurrentStateImageFile.FileName),
                            FileSizeBytes = model.CurrentStateImageFile.Length,
                            EntityType = ""ConstructionProject"",
                            EntityId = project.Id, 
                            UploadDate = DateTime.UtcNow,
                            UploadedByUserId = _userManager.GetUserId(User) ?? """",
                            FolderId = defaultFolder.Id,
                            PhysicalLocationNote = """"
                        };
                        _context.DmsDocuments.Add(dmsDoc);
                        await _context.SaveChangesAsync();
                    }
                }

                return Json(new { success = true, draftId = project.Id, projectId = project.Id });
            } catch (Exception ex) {";

        if (Regex.IsMatch(controllerCode, saveStep1Find, RegexOptions.Singleline))
        {
            controllerCode = Regex.Replace(controllerCode, saveStep1Find, saveStep1Replace, RegexOptions.Singleline);
            File.WriteAllText(controllerPath, controllerCode, new UTF8Encoding(true));
            Console.WriteLine("SaveStep1 updated.");
        }
        else
        {
            Console.WriteLine("SaveStep1 NOT FOUND.");
        }
    }
}
