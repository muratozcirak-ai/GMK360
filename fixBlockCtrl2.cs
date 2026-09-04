using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        var lines = new List<string>(File.ReadAllLines(path, Encoding.UTF8));

        int start = -1, end = -1;
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Contains("public async Task<IActionResult> UpdateBlockDetails"))
            {
                start = i - 2; // Grab the [HttpPost] and [ValidateAntiForgeryToken] added by my previous script
                if(!lines[start].Contains("[HttpPost]")) start = i; // fallback
            }
            if (start != -1 && i > start && lines[i].Contains("TempData[\"SuccessMessage\"]"))
            {
                end = i + 2; 
                break;
            }
        }

        if (start != -1 && end != -1)
        {
            string newAction = @"        public async Task<IActionResult> UpdateBlockDetails(int Id, string BlockName, double? BaseArea, List<string> SelectedFeatures, string TechnicalFeatures, string Description, string InsulationType, int? ElevatorCount, string ParkingType)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var building = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .FirstOrDefaultAsync(b => b.Id == Id);

            if (building == null || building.ConstructionProject?.AgencyId != agencyId)
                return NotFound();

            building.BlockName = BlockName;
            building.BaseArea = BaseArea;
            
            // Combine predefined checkboxes into a comma-separated string
            string combinedFeatures = """";
            if (SelectedFeatures != null && SelectedFeatures.Count > 0)
            {
                combinedFeatures = string.Join("", "", SelectedFeatures);
            }
            
            // If they also typed custom technical features, append them
            if (!string.IsNullOrEmpty(TechnicalFeatures))
            {
                if (!string.IsNullOrEmpty(combinedFeatures)) combinedFeatures += "", "";
                combinedFeatures += TechnicalFeatures;
            }
            
            building.TechnicalFeatures = combinedFeatures;
            building.Description = Description;
            building.InsulationType = InsulationType;
            building.ElevatorCount = ElevatorCount;
            building.ParkingType = ParkingType;

            _context.Update(building);
            await _context.SaveChangesAsync();

            TempData[""SuccessMessage""] = ""Blok özellikleri başarıyla güncellendi."";
            return RedirectToAction(nameof(ManageBlock), new { id = Id });
        }";
            
            lines.RemoveRange(start, end - start + 1);
            lines.Insert(start, newAction);
            Console.WriteLine("Controller Action Fixed");
        }

        File.WriteAllLines(path, lines, new UTF8Encoding(true));
    }
}
