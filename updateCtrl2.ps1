$content = Get-Content -Raw "GMK360.Web\Controllers\ConstructionProjectController.cs"

$oldCode = @"
                foreach (var b in wizard.Blocks)
                {
                    var building = new Building
                    {
                        BlockName = b.BlockName,
                        BaseArea = b.BaseArea,
                        TotalFloors = b.TotalFloors,
                        BasementFloors = b.BasementFloors,
                        HasGroundFloor = b.HasGroundFloor,
                        HasRoof = b.HasRoof,
                        ConstructionProjectId = project.Id,
                        CityId = wizard.CityId,
                        DistrictId = wizard.DistrictId,
                        NeighborhoodId = wizard.NeighborhoodId,
                        StreetId = wizard.StreetId,
                        PostalCode = wizard.PostalCode,
                        Latitude = wizard.Latitude,
                        Longitude = wizard.Longitude,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Buildings.Add(building);
                }
                
                await _context.SaveChangesAsync();
"@

$newCode = @"
                // PASS 1: Save Podiums and Independent buildings first
                var savedBuildings = new Dictionary<int, Building>(); // index -> building
                
                for (int i = 0; i < wizard.Blocks.Count; i++)
                {
                    var b = wizard.Blocks[i];
                    if (b.StructureType == ""podium"" || b.StructureType == ""independent"" || string.IsNullOrEmpty(b.StructureType))
                    {
                        var building = new Building
                        {
                            BlockName = b.BlockName,
                            BaseArea = b.BaseArea,
                            TotalFloors = b.TotalFloors,
                            BasementFloors = b.BasementFloors,
                            HasGroundFloor = b.HasGroundFloor,
                            HasRoof = b.HasRoof,
                            ConstructionProjectId = project.Id,
                            CityId = wizard.CityId,
                            DistrictId = wizard.DistrictId,
                            NeighborhoodId = wizard.NeighborhoodId,
                            StreetId = wizard.StreetId,
                            PostalCode = wizard.PostalCode,
                            Latitude = wizard.Latitude,
                            Longitude = wizard.Longitude,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Buildings.Add(building);
                        savedBuildings[i] = building;
                    }
                }
                await _context.SaveChangesAsync();

                // PASS 2: Save Towers and link them to their parent Podiums
                for (int i = 0; i < wizard.Blocks.Count; i++)
                {
                    var b = wizard.Blocks[i];
                    if (b.StructureType == ""tower"")
                    {
                        var building = new Building
                        {
                            BlockName = b.BlockName,
                            BaseArea = b.BaseArea,
                            TotalFloors = b.TotalFloors,
                            BasementFloors = b.BasementFloors,
                            HasGroundFloor = b.HasGroundFloor,
                            HasRoof = b.HasRoof,
                            ConstructionProjectId = project.Id,
                            CityId = wizard.CityId,
                            DistrictId = wizard.DistrictId,
                            NeighborhoodId = wizard.NeighborhoodId,
                            StreetId = wizard.StreetId,
                            PostalCode = wizard.PostalCode,
                            Latitude = wizard.Latitude,
                            Longitude = wizard.Longitude,
                            CreatedAt = DateTime.UtcNow
                        };

                        if (b.ParentIndex.HasValue && savedBuildings.ContainsKey(b.ParentIndex.Value))
                        {
                            building.ParentBuildingId = savedBuildings[b.ParentIndex.Value].Id;
                        }

                        _context.Buildings.Add(building);
                        savedBuildings[i] = building;
                    }
                }
                await _context.SaveChangesAsync();
"@

$content = $content.Replace($oldCode, $newCode)

$content | Set-Content "GMK360.Web\Controllers\ConstructionProjectController.cs" -Encoding UTF8
Write-Output "Controller updated"
