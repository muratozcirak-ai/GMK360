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
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Contains("public async Task<IActionResult> BulkUpdateUnits"))
            {
                insertIndex = i - 2; // Insert right before BulkUpdateUnits
                break;
            }
        }

        if (insertIndex != -1)
        {
            string addAction = @"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBuildingUnit(int buildingId, int FloorLevel, string FloorName, string DoorNumber, string RoomCount, string UnitStructure, double? GrossSquareMeters, string FacadeDirection)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var building = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (building == null || building.ConstructionProject?.AgencyId != agencyId)
                return NotFound();

            // Combine RoomCount and UnitStructure logically
            string combinedRoomLayout = UnitStructure;
            if (UnitStructure != ""Dükkan / Ticari"" && UnitStructure != ""Depo"" && RoomCount != ""-"")
            {
                combinedRoomLayout = $""{RoomCount} ({UnitStructure})"";
            }

            var newUnit = new GMK360.Core.Entities.BuildingUnit
            {
                BuildingId = buildingId,
                FloorLevel = FloorLevel,
                FloorName = string.IsNullOrEmpty(FloorName) ? $""{FloorLevel}. Kat"" : FloorName,
                DoorNumber = DoorNumber,
                RoomLayout = combinedRoomLayout,
                GrossSquareMeters = GrossSquareMeters,
                FacadeDirection = FacadeDirection
            };

            _context.BuildingUnits.Add(newUnit);
            await _context.SaveChangesAsync();

            TempData[""SuccessMessage""] = $""{DoorNumber} numaralı yeni birim başarıyla eklendi."";
            return RedirectToAction(nameof(ManageBlock), new { id = buildingId });
        }
";
            lines.Insert(insertIndex, addAction);
            File.WriteAllLines(path, lines, new UTF8Encoding(true));
            Console.WriteLine("AddBuildingUnit added.");
        }
    }
}
