using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string htmlPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string html = File.ReadAllText(htmlPath, Encoding.UTF8);

        // 1. Add Copy Button to table footer next to Add Unit
        string tfootPattern = @"<button type=""button"" class=""btn btn-sm btn-link text-success text-decoration-none fw-bold btn-add-unit-to-floor"".*?</button>";
        string newButtons = @"<button type=""button"" class=""btn btn-sm btn-link text-success text-decoration-none fw-bold btn-add-unit-to-floor"" data-bs-toggle=""modal"" data-bs-target=""#addUnitModal"" data-floorlevel=""@floorGroup.Key"" data-floorname=""@floorName"">
                                                                <i class=""bi bi-plus-circle me-1""></i> Bu Kata Yeni Birim Ekle
                                                            </button>
                                                            <button type=""button"" class=""btn btn-sm btn-link text-primary text-decoration-none fw-bold btn-copy-floor ms-3"" data-bs-toggle=""modal"" data-bs-target=""#copyFloorModal"" data-floorlevel=""@floorGroup.Key"" data-floorname=""@floorName"">
                                                                <i class=""bi bi-files me-1""></i> Bu Katı Kopyala (Şablon)
                                                            </button>";
        if(Regex.IsMatch(html, tfootPattern, RegexOptions.Singleline))
        {
            html = Regex.Replace(html, tfootPattern, newButtons, RegexOptions.Singleline);
        }

        // 2. Add copyFloorModal HTML before bulkEditUnitsModal
        string modalAnchor = @"<!-- Bulk Edit Modal -->";
        string copyModalHtml = @"<!-- Copy Floor Modal -->
<div class=""modal fade"" id=""copyFloorModal"" tabindex=""-1"">
    <div class=""modal-dialog"">
        <div class=""modal-content rounded-4 border-0 shadow"">
            <form id=""copyFloorForm"" asp-action=""CopyFloor"" method=""post"">
                <input type=""hidden"" name=""buildingId"" value=""@Model.Id"" />
                <input type=""hidden"" name=""sourceFloorLevel"" id=""sourceFloorLevel"" />
                
                <div class=""modal-header border-0 pb-0"">
                    <h5 class=""modal-title fw-bold""><i class=""bi bi-files text-primary me-2""></i>Kat Kopyala (Şablon Çoğaltıcı)</h5>
                    <button type=""button"" class=""btn-close"" data-bs-dismiss=""modal""></button>
                </div>
                <div class=""modal-body"">
                    <div class=""alert alert-info py-2 small mb-4"">
                        <i class=""bi bi-info-circle me-1""></i> <strong id=""sourceFloorNameDisplay""></strong> içerisindeki tüm birimler (cephe ve brüt alanlarıyla birlikte) seçtiğiniz yeni katlara birebir kopyalanacaktır.
                    </div>
                    
                    <div class=""mb-3"">
                        <label class=""form-label fw-bold small"">Hedef Katları Yazın (Virgülle Ayırarak)</label>
                        <input type=""text"" name=""targetFloorLevels"" class=""form-control"" placeholder=""Örn: 2, 3, 4, 5"" required />
                        <div class=""form-text"" style=""font-size:11px;"">Kopyalamak istediğiniz kat seviyelerini araya virgül koyarak sayıyla yazın. (Örn: Zemin üstü 4 kata kopyalamak için 1, 2, 3, 4 yazın)</div>
                    </div>
                </div>
                <div class=""modal-footer border-0 pt-0"">
                    <button type=""button"" class=""btn btn-light rounded-pill px-4"" data-bs-dismiss=""modal"">İptal</button>
                    <button type=""submit"" class=""btn btn-primary rounded-pill px-4"">Kopyala</button>
                </div>
            </form>
        </div>
    </div>
</div>

<!-- Bulk Edit Modal -->";
        if(html.Contains(modalAnchor))
        {
            html = html.Replace(modalAnchor, copyModalHtml);
        }

        // 3. Add JS
        string jsAnchor = @"// Populate Add Unit Modal";
        string copyJs = @"// Populate Copy Floor Modal
            $('.btn-copy-floor').click(function() {
                var floorLevel = $(this).data('floorlevel');
                var floorName = $(this).data('floorname');
                $('#copyFloorModal #sourceFloorLevel').val(floorLevel);
                $('#copyFloorModal #sourceFloorNameDisplay').text(floorName);
                $('#copyFloorModal input[name=""targetFloorLevels""]').val('');
            });
            
            // Populate Add Unit Modal";
        if(html.Contains(jsAnchor))
        {
            html = html.Replace(jsAnchor, copyJs);
        }

        File.WriteAllText(htmlPath, html, new UTF8Encoding(true));

        // 4. Update Controller
        string ctrlPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string ctrl = File.ReadAllText(ctrlPath, Encoding.UTF8);

        string newAction = @"
        [HttpPost]
        public async Task<IActionResult> CopyFloor(int buildingId, int sourceFloorLevel, string targetFloorLevels)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var building = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (building == null || building.ConstructionProject?.AgencyId != agencyId)
                return NotFound();

            var sourceUnits = await _context.BuildingUnits
                .Where(u => u.BuildingId == buildingId && u.FloorLevel == sourceFloorLevel)
                .ToListAsync();

            if (!sourceUnits.Any())
                return RedirectToAction(nameof(ManageBlock), new { id = buildingId });

            var targetFloorsStr = targetFloorLevels.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            int copyCount = 0;
            foreach (var fStr in targetFloorsStr)
            {
                if (int.TryParse(fStr.Trim(), out int targetLevel))
                {
                    if (targetLevel == sourceFloorLevel) continue; // Don't copy to itself

                    foreach (var u in sourceUnits)
                    {
                        var newUnit = new GMK360.Core.Entities.BuildingUnit
                        {
                            BuildingId = buildingId,
                            FloorLevel = targetLevel,
                            FloorName = targetLevel == 0 ? ""Zemin Kat"" : (targetLevel < 0 ? $""{targetLevel}. Kat (Bodrum)"" : $""{targetLevel}. Kat""),
                            DoorNumber = u.DoorNumber,
                            RoomLayout = u.RoomLayout,
                            GrossSquareMeters = u.GrossSquareMeters,
                            FacadeDirection = u.FacadeDirection,
                            UnitTypeId = u.UnitTypeId
                        };
                        _context.BuildingUnits.Add(newUnit);
                        copyCount++;
                    }
                }
            }

            if (copyCount > 0)
            {
                await _context.SaveChangesAsync();
                TempData[""SuccessMessage""] = $""Seçili kat şablonu, hedef katlara başarıyla uygulandı ve {copyCount} adet yeni birim oluşturuldu."";
            }

            return RedirectToAction(nameof(ManageBlock), new { id = buildingId });
        }
";
        // Insert right before AddBuildingUnit
        string anchorAction = @"public async Task<IActionResult> AddBuildingUnit";
        if (ctrl.Contains(anchorAction))
        {
            ctrl = ctrl.Insert(ctrl.IndexOf(anchorAction), newAction);
            File.WriteAllText(ctrlPath, ctrl, new UTF8Encoding(true));
            Console.WriteLine("CopyFloor Action added to Controller.");
        }
    }
}
