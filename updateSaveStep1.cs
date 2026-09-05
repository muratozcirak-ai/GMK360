using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        // Update Create(int? id)
        string oldModelMap = @"                    DraftProjectId = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    Address = project.Address,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    TotalLandArea = project.TotalLandArea,";
                    
        string newModelMap = @"                    DraftProjectId = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    Address = project.Address,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    TotalLandArea = project.TotalLandArea,
                    CityId = project.CityId ?? 0,
                    DistrictId = project.DistrictId ?? 0,
                    NeighborhoodId = project.NeighborhoodId ?? 0,
                    StreetId = project.StreetId,";

        if (code.Contains(oldModelMap))
        {
            code = code.Replace(oldModelMap, newModelMap);
        }

        // Update SaveStep1
        string oldSaveStep1Start = @"        public async Task<IActionResult> SaveStep1([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)
        {
            try 
            {
                var agencyId = await GetUserAgencyIdAsync();";
                
        string oldSaveStep1Body = @"                var project = new ConstructionProject
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
                await _context.SaveChangesAsync();";
                
        string newSaveStep1Body = @"
                string uploadedImageUrl = """";
                if (model.CoverImageFile != null && model.CoverImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, ""uploads"", ""projects"", ""covers"");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + ""_"" + Path.GetFileName(model.CoverImageFile.FileName);
                    using (var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create)) { await model.CoverImageFile.CopyToAsync(fileStream); }
                    uploadedImageUrl = ""/uploads/projects/covers/"" + uniqueFileName;
                }
                
                string currentStateImageUrl = """";
                if (model.CurrentStateImageFile != null && model.CurrentStateImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, ""uploads"", ""projects"", ""current"");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + ""_"" + Path.GetFileName(model.CurrentStateImageFile.FileName);
                    using (var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create)) { await model.CurrentStateImageFile.CopyToAsync(fileStream); }
                    currentStateImageUrl = ""/uploads/projects/current/"" + uniqueFileName;
                }

                ConstructionProject project;
                if (model.DraftProjectId > 0) 
                {
                    project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == model.DraftProjectId);
                    if (project != null) {
                        project.Name = model.Name;
                        project.Description = model.Description;
                        project.Address = model.Address ?? """";
                        project.StartDate = model.StartDate;
                        project.EndDate = model.EndDate;
                        project.TotalLandArea = model.TotalLandArea;
                        project.CityId = model.CityId;
                        project.DistrictId = model.DistrictId;
                        project.NeighborhoodId = model.NeighborhoodId;
                        project.StreetId = model.StreetId;
                        if (!string.IsNullOrEmpty(uploadedImageUrl)) project.CoverImageUrl = uploadedImageUrl;
                    }
                }
                else 
                {
                    project = new ConstructionProject
                    {
                        Name = model.Name,
                        Description = model.Description,
                        AgencyId = agencyId.Value,
                        Address = model.Address ?? """",
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        TotalLandArea = model.TotalLandArea,
                        CityId = model.CityId,
                        DistrictId = model.DistrictId,
                        NeighborhoodId = model.NeighborhoodId,
                        StreetId = model.StreetId,
                        CoverImageUrl = uploadedImageUrl,
                        Status = 0 // Draft
                    };
                    _context.ConstructionProjects.Add(project);
                }

                await _context.SaveChangesAsync();";

        if (code.Contains(oldSaveStep1Body))
        {
            code = code.Replace(oldSaveStep1Body, newSaveStep1Body);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Updated SaveStep1 and Create map.");
        }
    }
}
