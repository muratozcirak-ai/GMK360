using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string oldCode = @"            var project = new ConstructionProject
            {
                Name = model.Name,
                Description = model.Description,
                AgencyId = agencyId.Value,
                CoverImageUrl = uploadedImageUrl,
                Address = model.Address ?? """",
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Status = 1,
                SelectionDeadline = DateTime.UtcNow.AddDays(30),
                TotalLandArea = model.TotalLandArea,
                LandscapeArea = 0,
                Blocks = blocks
            };

            _context.ConstructionProjects.Add(project);
            await _context.SaveChangesAsync();";

        string newCode = @"            var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == model.DraftProjectId);
            if (project == null) {
                // Draft bulunamazsa yeni oluştur (fallback)
                project = new ConstructionProject
                {
                    Name = model.Name,
                    Description = model.Description,
                    AgencyId = agencyId.Value,
                    Address = model.Address ?? """",
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    SelectionDeadline = DateTime.UtcNow.AddDays(30),
                    TotalLandArea = model.TotalLandArea,
                    LandscapeArea = 0,
                    Blocks = blocks
                };
                _context.ConstructionProjects.Add(project);
            }
            else {
                // Mevcut Draft'ı aktifleştir
                project.Status = 1;
                project.SelectionDeadline = DateTime.UtcNow.AddDays(30);
            }

            if (!string.IsNullOrEmpty(uploadedImageUrl)) project.CoverImageUrl = uploadedImageUrl;

            await _context.SaveChangesAsync();";

        if (code.Contains(oldCode))
        {
            code = code.Replace(oldCode, newCode);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Replaced Final Save Logic");
        }
        else
        {
            Console.WriteLine("Still could not find it. Here is the context:");
            int idx = code.IndexOf("var project = new ConstructionProject");
            if (idx != -1) {
                Console.WriteLine(code.Substring(idx, Math.Min(400, code.Length - idx)));
            }
        }
    }
}
