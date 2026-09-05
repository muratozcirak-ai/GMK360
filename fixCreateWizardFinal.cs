using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string oldBlock = @"            var project = new GMK360.Core.Entities.Construction.ConstructionProject
            {
                Name = model.Name,
                Description = string.IsNullOrWhiteSpace(model.Description) ? """" : model.Description,
                Address = string.IsNullOrWhiteSpace(model.Address) ? ""Adres belirtilmedi"" : model.Address,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                CoverImageUrl = uploadedImageUrl,
                AgencyId = agencyId.Value,
                Status = 0, // 0 = Upcoming
                TotalLandArea = model.TotalLandArea,
                CreatedAt = DateTime.UtcNow
            };

            try 
            {
                _context.Add(project);
                await _context.SaveChangesAsync();";

        string newBlock = @"            var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == model.DraftProjectId);
            
            if (project == null) {
                project = new GMK360.Core.Entities.Construction.ConstructionProject
                {
                    Name = model.Name,
                    Description = string.IsNullOrWhiteSpace(model.Description) ? """" : model.Description,
                    Address = string.IsNullOrWhiteSpace(model.Address) ? ""Adres belirtilmedi"" : model.Address,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    CoverImageUrl = uploadedImageUrl,
                    AgencyId = agencyId.Value,
                    Status = 0, // 0 = Upcoming
                    TotalLandArea = model.TotalLandArea,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Add(project);
            }
            else {
                if (!string.IsNullOrEmpty(uploadedImageUrl)) project.CoverImageUrl = uploadedImageUrl;
                project.Status = 1; // Mark as ongoing/started
            }

            try 
            {
                await _context.SaveChangesAsync();";

        if (code.Contains(oldBlock))
        {
            code = code.Replace(oldBlock, newBlock);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Replaced successfully.");
        }
    }
}
