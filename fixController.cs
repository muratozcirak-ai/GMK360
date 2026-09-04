using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

class Program {
    static void Main() {
        string controllerPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string ctrlText = File.ReadAllText(controllerPath);
        
        string oldGenerate = @"              int doorCounter = 1;
            int totalUnitsCreated = 0;

            // 1. Bodrum Katları (Örn: -2, -1)
            for (int f = basementFloors; f >= 1; f--)
            {
                for (int u = 0; u < unitsPerFloor; u++)
                {
                    var unit = new GMK360.Core.Entities.BuildingUnit
                    {
                        BuildingId = buildingId,
                        DoorNumber = doorCounter.ToString(),
                        FloorLevel = -f,
                        FloorName = $""-{f}. Kat (Bodrum)"",
                        RoomLayout = string.IsNullOrEmpty(roomLayout) ? ""3+1"" : roomLayout,
                        IsEmpty = true
                    };
                    _context.BuildingUnits.Add(unit);
                    doorCounter++;
                    totalUnitsCreated++;
                }
            }

            // 2. Zemin Kat (0)
            if (hasGroundFloor)
            {
                for (int u = 0; u < unitsPerFloor; u++)
                {
                    var unit = new GMK360.Core.Entities.BuildingUnit
                    {
                        BuildingId = buildingId,
                        DoorNumber = doorCounter.ToString(),
                        FloorLevel = 0,
                        FloorName = ""Zemin Kat"",
                        RoomLayout = string.IsNullOrEmpty(roomLayout) ? ""3+1"" : roomLayout,
                        IsEmpty = true
                    };
                    _context.BuildingUnits.Add(unit);
                    doorCounter++;
                    totalUnitsCreated++;
                }
            }

            // 3. Normal Katlar (1, 2, 3...)
            for (int f = 1; f <= normalFloors; f++)
            {
                for (int u = 0; u < unitsPerFloor; u++)
                {
                    var unit = new GMK360.Core.Entities.BuildingUnit
                    {
                        BuildingId = buildingId,
                        DoorNumber = doorCounter.ToString(),
                        FloorLevel = f,
                        FloorName = $""{f}. Kat"",
                        RoomLayout = string.IsNullOrEmpty(roomLayout) ? ""3+1"" : roomLayout,
                        IsEmpty = true
                    };
                    _context.BuildingUnits.Add(unit);
                    doorCounter++;
                    totalUnitsCreated++;
                }
            }";
            
        string newGenerate = @"              int doorCounter = 1;
            int totalUnitsCreated = 0;
            
            // Önce mevcut birimleri silelim (Temiz üretim)
            var existingUnits = _context.BuildingUnits.Where(u => u.BuildingId == buildingId).ToList();
            _context.BuildingUnits.RemoveRange(existingUnits);
            await _context.SaveChangesAsync();

            // 1. Bodrum Katları (-1, -2 vb. tek parça otopark/sığınak)
            for (int f = basementFloors; f >= 1; f--)
            {
                var unit = new GMK360.Core.Entities.BuildingUnit
                {
                    BuildingId = buildingId,
                    DoorNumber = f == 1 ? ""Sığınak"" : ""Otopark"",
                    FloorLevel = -f,
                    FloorName = $""-{f}. Bodrum Kat"",
                    RoomLayout = ""Ortak Alan"",
                    IsEmpty = true
                };
                _context.BuildingUnits.Add(unit);
                totalUnitsCreated++;
            }

            // 2. Zemin Kat (Dükkanlar)
            if (hasGroundFloor)
            {
                for (int u = 1; u <= unitsPerFloor; u++)
                {
                    var unit = new GMK360.Core.Entities.BuildingUnit
                    {
                        BuildingId = buildingId,
                        DoorNumber = ""Dükkan "" + u,
                        FloorLevel = 0,
                        FloorName = ""Zemin Kat"",
                        RoomLayout = ""Ticari Alan"",
                        IsEmpty = true
                    };
                    _context.BuildingUnits.Add(unit);
                    totalUnitsCreated++;
                }
            }

            // 3. Normal Katlar (Daireler)
            for (int f = 1; f <= normalFloors; f++)
            {
                for (int u = 1; u <= unitsPerFloor; u++)
                {
                    string layout = string.IsNullOrEmpty(roomLayout) ? ""3+1"" : roomLayout;
                    if (f == normalFloors && normalFloors > 1) layout = ""Dubleks"";
                    
                    var unit = new GMK360.Core.Entities.BuildingUnit
                    {
                        BuildingId = buildingId,
                        DoorNumber = ""Daire "" + doorCounter.ToString(),
                        FloorLevel = f,
                        FloorName = $""{f}. Kat"",
                        RoomLayout = layout,
                        IsEmpty = true
                    };
                    _context.BuildingUnits.Add(unit);
                    doorCounter++;
                    totalUnitsCreated++;
                }
            }";
            
        ctrlText = ctrlText.Replace(oldGenerate, newGenerate);
        
        ctrlText = Regex.Replace(ctrlText, @"public async Task<IActionResult> UpdateUnitProperties\(int Id, int BuildingId, string DoorNumber, string RoomLayout\).*?await _context\.SaveChangesAsync\(\);", @"public async Task<IActionResult> UpdateUnitProperties(int Id, int BuildingId, string DoorNumber, string RoomLayout, string OwnerName, string OwnerPhone)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var unit = await _context.BuildingUnits
                .Include(u => u.Building)
                .ThenInclude(b => b.ConstructionProject)
                .FirstOrDefaultAsync(u => u.Id == Id && u.Building.ConstructionProject.AgencyId == agencyId);
                
            if (unit != null)
            {
                unit.DoorNumber = DoorNumber;
                unit.RoomLayout = RoomLayout;
                unit.OwnerName = OwnerName;
                unit.OwnerPhone = OwnerPhone;
                unit.IsEmpty = string.IsNullOrEmpty(OwnerName);
                await _context.SaveChangesAsync();", RegexOptions.Singleline);
                
        File.WriteAllText(controllerPath, ctrlText, System.Text.Encoding.UTF8);
    }
}
