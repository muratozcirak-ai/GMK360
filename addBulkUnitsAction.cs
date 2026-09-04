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

        int insertIndex = -1;
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            if (lines[i].Contains("}"))
            {
                // Find the second to last closing brace to insert a new method
                int openBraces = 0, closeBraces = 0;
                for(int j=0; j<lines.Count; j++) {
                    if(lines[j].Contains("{")) openBraces++;
                    if(lines[j].Contains("}")) closeBraces++;
                }
                insertIndex = i - 1; 
                break;
            }
        }
        
        // safer way: Just find the end of UpdateBlockDetails and insert after it.
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Contains("TempData[\"SuccessMessage\"] = \"Blok özellikleri başarıyla güncellendi.\";"))
            {
                insertIndex = i + 3; // return line + closing brace + 1
                break;
            }
        }

        if (insertIndex != -1)
        {
            string bulkAction = @"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdateUnits(int buildingId, string unitIds, string FacadeDirection, double? GrossSquareMeters, double? NetSquareMeters, string RoomLayout)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            if (string.IsNullOrEmpty(unitIds))
            {
                TempData[""ErrorMessage""] = ""Hiçbir daire seçilmedi."";
                return RedirectToAction(nameof(ManageBlock), new { id = buildingId });
            }

            var ids = unitIds.Split(',').Select(id => int.TryParse(id, out int parsed) ? parsed : 0).Where(id => id > 0).ToList();
            if (!ids.Any())
            {
                TempData[""ErrorMessage""] = ""Geçersiz daire seçimi."";
                return RedirectToAction(nameof(ManageBlock), new { id = buildingId });
            }

            var unitsToUpdate = await _context.BuildingUnits
                .Include(u => u.Building)
                .ThenInclude(b => b.ConstructionProject)
                .Where(u => ids.Contains(u.Id) && u.Building.ConstructionProject.AgencyId == agencyId)
                .ToListAsync();

            if (!unitsToUpdate.Any())
            {
                TempData[""ErrorMessage""] = ""Güncellenecek daire bulunamadı veya yetkiniz yok."";
                return RedirectToAction(nameof(ManageBlock), new { id = buildingId });
            }

            foreach (var unit in unitsToUpdate)
            {
                if (!string.IsNullOrEmpty(FacadeDirection))
                    unit.FacadeDirection = FacadeDirection;
                    
                if (GrossSquareMeters.HasValue)
                    unit.GrossSquareMeters = GrossSquareMeters;
                    
                if (NetSquareMeters.HasValue)
                    unit.NetSquareMeters = NetSquareMeters;
                    
                if (!string.IsNullOrEmpty(RoomLayout))
                    unit.RoomLayout = RoomLayout;
            }

            _context.UpdateRange(unitsToUpdate);
            await _context.SaveChangesAsync();

            TempData[""SuccessMessage""] = $""{unitsToUpdate.Count} adet bağımsız bölüm başarıyla güncellendi."";
            return RedirectToAction(nameof(ManageBlock), new { id = buildingId });
        }
";
            lines.Insert(insertIndex, bulkAction);
            File.WriteAllLines(path, lines, new UTF8Encoding(true));
            Console.WriteLine("BulkUpdateUnits added.");
        }
    }
}
