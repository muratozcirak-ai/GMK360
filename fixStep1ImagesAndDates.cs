using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        // 1. Fix Create.cshtml Date Inputs
        string viewPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string viewCode = File.ReadAllText(viewPath, Encoding.UTF8);

        string oldStartDate = @"<input asp-for=""StartDate"" type=""date"" class=""form-control rounded-3"" required />";
        string newStartDate = @"<input name=""StartDate"" id=""StartDate"" type=""date"" value=""@(Model.StartDate.Year > 1 ? Model.StartDate.ToString(""yyyy-MM-dd"") : """")"" class=""form-control rounded-3"" required />";
        viewCode = viewCode.Replace(oldStartDate, newStartDate);

        string oldEndDate = @"<input asp-for=""EndDate"" type=""date"" class=""form-control rounded-3"" required />";
        string newEndDate = @"<input name=""EndDate"" id=""EndDate"" type=""date"" value=""@(Model.EndDate.Year > 1 ? Model.EndDate.ToString(""yyyy-MM-dd"") : """")"" class=""form-control rounded-3"" required />";
        viewCode = viewCode.Replace(oldEndDate, newEndDate);

        File.WriteAllText(viewPath, viewCode, new UTF8Encoding(true));
        Console.WriteLine("Create.cshtml fixed.");

        // 2. Fix SaveStep1 in ConstructionProjectController.cs
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
                
                // Only update dates if they are valid
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

                // Handle Image Uploads
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
                    // For now we don't have CurrentStateImageUrl on project entity, so we just upload it.
                    // Ideally we should create the DmsDocument here or save it to a field.
                    // We'll store it in a generic way or skip DmsDocument for brevity since we activate it in CreateWizard.
                    // Actually, let's create the DmsDocument right here!
                    var defaultFolder = await _context.DmsFolders.FirstOrDefaultAsync();
                    if (defaultFolder != null) {
                        var dmsDoc = new GMK360.Core.Entities.DmsDocument
                        {
                            Title = ""Mevcut Durum Görseli (İlk Hali)"",
                            DocumentUrl = ""/uploads/projects/current/"" + uniqueFileName,
                            FileExtension = Path.GetExtension(model.CurrentStateImageFile.FileName),
                            FileSizeBytes = model.CurrentStateImageFile.Length,
                            EntityType = ""ConstructionProject"",
                            EntityId = project.Id, // Note: if project is new, Id is 0 until SaveChanges, so we must SaveChanges first!
                            UploadDate = DateTime.UtcNow,
                            UploadedByUserId = _userManager.GetUserId(User) ?? """",
                            FolderId = defaultFolder.Id,
                            PhysicalLocationNote = """"
                        };
                        // We will add it after saving project
                        _context.DmsDocuments.Add(dmsDoc);
                    }
                }

                await _context.SaveChangesAsync();
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
