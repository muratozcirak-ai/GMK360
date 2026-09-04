using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string text = File.ReadAllText(path);

        var regex = new Regex(@"public async Task<IActionResult> GenerateUnits.*?return RedirectToAction\(nameof\(ManageBlock\), new \{ id = buildingId \}\);\s*}", RegexOptions.Singleline);

        string newMethod = @"public async Task<IActionResult> GenerateUnits(int buildingId, int normalFloors, int basementFloors, bool hasGroundFloor, int unitsPerFloor, int? templateId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var block = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .Include(b => b.Units)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (block == null) return NotFound();

            if (!User.IsInRole(""Admin"") && block.ConstructionProject.AgencyId != agencyId)
            {
                return Unauthorized();
            }
            
            // Seçilen şablonu getir
            GMK360.Core.Entities.UnitTemplate selectedTemplate = null;
            if (templateId.HasValue && templateId.Value > 0)
            {
                selectedTemplate = await _context.UnitTemplates
                    .Include(t => t.Spaces)
                    .FirstOrDefaultAsync(t => t.Id == templateId.Value);
            }
            
            // 0. ESKİ BİRİMLERİ TEMİZLE
            if (block.Units != null && block.Units.Any())
            {
                _context.BuildingUnits.RemoveRange(block.Units);
                await _context.SaveChangesAsync();
            }

            int doorCounter = 1;
            int totalUnitsCreated = 0;

            // 1. Bodrum Katlar (Sığınak / Otopark)
            for (int f = basementFloors; f >= 1; f--)
            {
                var unit = new GMK360.Core.Entities.BuildingUnit
                {
                    BuildingId = buildingId,
                    DoorNumber = (f == 1) ? ""Sığınak"" : $""Otopark (-{f})"",
                    FloorLevel = -f,
                    FloorName = $""-{f}. Kat (Bodrum)"",
                    RoomLayout = ""Ortak Alan"",
                    IsEmpty = true
                };
                _context.BuildingUnits.Add(unit);
                totalUnitsCreated++;
            }

            // 2. Zemin Kat (Dükkan)
            if (hasGroundFloor)
            {
                for (int u = 0; u < unitsPerFloor; u++)
                {
                    var unit = new GMK360.Core.Entities.BuildingUnit
                    {
                        BuildingId = buildingId,
                        DoorNumber = $""Dükkan {doorCounter}"",
                        FloorLevel = 0,
                        FloorName = ""Zemin Kat"",
                        RoomLayout = ""Ticari Alan"",
                        IsEmpty = true
                    };
                    _context.BuildingUnits.Add(unit);
                    doorCounter++;
                    totalUnitsCreated++;
                }
            }

            // 3. Normal Katlar (Şablon)
            for (int f = 1; f <= normalFloors; f++)
            {
                for (int u = 0; u < unitsPerFloor; u++)
                {
                    var layoutName = selectedTemplate?.RoomLayout ?? ""Belirsiz"";
                    
                    var unit = new GMK360.Core.Entities.BuildingUnit
                    {
                        BuildingId = buildingId,
                        DoorNumber = doorCounter.ToString(),
                        FloorLevel = f,
                        FloorName = $""{f}. Kat"",
                        RoomLayout = layoutName,
                        UnitTemplateId = templateId,
                        IsEmpty = true,
                        Spaces = new System.Collections.Generic.List<GMK360.Core.Entities.UnitSpace>()
                    };
                    
                    // Şablon odalarını kopyala
                    if (selectedTemplate != null && selectedTemplate.Spaces != null)
                    {
                        foreach(var ts in selectedTemplate.Spaces)
                        {
                            unit.Spaces.Add(new GMK360.Core.Entities.UnitSpace
                            {
                                Name = ts.Name,
                                Type = ts.Type,
                                SquareMeters = ts.SquareMeters,
                                Description = ts.Description
                            });
                        }
                    }

                    _context.BuildingUnits.Add(unit);
                    doorCounter++;
                    totalUnitsCreated++;
                }
            }
            
            block.TotalFloors = normalFloors;
            block.BasementFloors = basementFloors;
            block.HasGroundFloor = hasGroundFloor;

            _context.Buildings.Update(block);
            await _context.SaveChangesAsync();

            TempData[""SuccessMessage""] = $""Eski kayıtlar temizlendi. Şablon baz alınarak toplam {totalUnitsCreated} bağımsız bölüm üretildi."";
            return RedirectToAction(nameof(ManageBlock), new { id = buildingId });
        }";

        text = regex.Replace(text, newMethod);
        File.WriteAllText(path, text, System.Text.Encoding.UTF8);
    }
}
