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

        string saveStep2Find = @"if \(project\.Blocks != null && project\.Blocks\.Any\(\)\)\s*\{\s*_context\.Buildings\.RemoveRange\(project\.Blocks\);\s*\}\s*if \(model\.Blocks != null\)\s*\{\s*int blockCounter = 1;\s*foreach \(var b in model\.Blocks\)\s*\{\s*var building = new GMK360\.Core\.Entities\.Building[\s\S]*?_context\.Buildings\.Add\(building\);\s*blockCounter\+\+;\s*\}\s*\}";

        string saveStep2Replace = @"if (model.Blocks != null)
                {
                    var existingBlocks = project.Blocks?.ToList() ?? new System.Collections.Generic.List<GMK360.Core.Entities.Building>();
                    var currentBlockNames = model.Blocks.Select(b => b.BlockName).ToList();

                    // Sadece formda olmayan eski blokları sil
                    var blocksToRemove = existingBlocks.Where(b => !currentBlockNames.Contains(b.BlockName)).ToList();
                    if (blocksToRemove.Any()) {
                        _context.Buildings.RemoveRange(blocksToRemove);
                    }

                    int blockCounter = 1;
                    foreach (var b in model.Blocks)
                    {
                        var existingBlock = existingBlocks.FirstOrDefault(eb => eb.BlockName == b.BlockName);
                        if (existingBlock != null)
                        {
                            // Varsa SADECE GÜNCELLE (Lifecycle kuralı)
                            existingBlock.BaseArea = b.BaseArea;
                            existingBlock.BasementFloors = b.BasementFloors;
                            existingBlock.TotalFloors = b.TotalFloors;
                            existingBlock.TotalUnits = b.TotalApartments + b.TotalShops;
                            existingBlock.HasRoof = b.HasRoof;
                            existingBlock.HasGroundFloor = b.HasGroundFloor;
                            
                            // Eğer Dükkan Sayısı kolonu eklenirse buraya da eklenecek
                        }
                        else
                        {
                            // Yoksa YENİ EKLE
                            var building = new GMK360.Core.Entities.Building
                            {
                                Name = project.Name + "" - "" + b.BlockName,
                                BlockName = b.BlockName,
                                BuildingNumber = blockCounter.ToString(),
                                StreetName = ""Belirtilmedi"",
                                BaseArea = b.BaseArea,
                                BasementFloors = b.BasementFloors,
                                TotalFloors = b.TotalFloors,
                                TotalUnits = b.TotalApartments + b.TotalShops,
                                HasBlock = true,
                                HasRoof = b.HasRoof,
                                HasGroundFloor = b.HasGroundFloor,
                                ConstructionProjectId = project.Id,
                                CityId = project.CityId ?? 34,
                                DistrictId = project.DistrictId ?? 1,
                                NeighborhoodId = project.NeighborhoodId ?? 1,
                                StreetId = project.StreetId,
                                CreatedAt = DateTime.UtcNow,
                                ManagerUserId = _userManager.GetUserId(User) ?? """"
                            };
                            _context.Buildings.Add(building);
                        }
                        blockCounter++;
                    }
                }";

        if (Regex.IsMatch(controllerCode, saveStep2Find, RegexOptions.Singleline))
        {
            controllerCode = Regex.Replace(controllerCode, saveStep2Find, saveStep2Replace, RegexOptions.Singleline);
            File.WriteAllText(controllerPath, controllerCode, new UTF8Encoding(true));
            Console.WriteLine("SaveStep2 updated to use correct lifecycle logic (Update instead of Delete-All).");
        }
        else
        {
            Console.WriteLine("SaveStep2 NOT FOUND.");
        }
    }
}
