using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string ctrlPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string ctrl = File.ReadAllText(ctrlPath, Encoding.UTF8);

        // 1. Add ViewModel to Controller file namespace (or just use generic parameters)
        // Actually, easiest is just adding a quick class at the bottom of the controller namespace
        string classToAdd = @"
    public class BulkUnitEntry
    {
        public string DoorNumber { get; set; }
        public string UnitStructure { get; set; }
        public double? GrossSquareMeters { get; set; }
        public string FacadeDirection { get; set; }
    }
";
        if (!ctrl.Contains("BulkUnitEntry"))
        {
            ctrl = ctrl.Replace("namespace GMK360.Web.Controllers\r\n{", "namespace GMK360.Web.Controllers\r\n{" + classToAdd);
        }
        
        string newAction = @"
        [HttpPost]
        public async Task<IActionResult> AddMultipleBuildingUnits(int buildingId, int FloorLevel, string FloorName, [FromForm] List<BulkUnitEntry> Units)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var building = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (building == null || building.ConstructionProject?.AgencyId != agencyId)
                return NotFound();

            int added = 0;
            if(Units != null)
            {
                foreach(var u in Units)
                {
                    if(string.IsNullOrWhiteSpace(u.DoorNumber)) continue;
                    
                    var newUnit = new GMK360.Core.Entities.BuildingUnit
                    {
                        BuildingId = buildingId,
                        FloorLevel = FloorLevel,
                        FloorName = string.IsNullOrEmpty(FloorName) ? $""{FloorLevel}. Kat"" : FloorName,
                        DoorNumber = u.DoorNumber,
                        RoomLayout = u.UnitStructure ?? ""Belirtilmedi"",
                        GrossSquareMeters = u.GrossSquareMeters,
                        FacadeDirection = u.FacadeDirection
                    };
                    _context.BuildingUnits.Add(newUnit);
                    added++;
                }
            }

            if(added > 0)
            {
                await _context.SaveChangesAsync();
                TempData[""SuccessMessage""] = $""{added} adet yeni birim başarıyla eklendi."";
            }
            
            return RedirectToAction(nameof(ManageBlock), new { id = buildingId });
        }
";
        if (!ctrl.Contains("AddMultipleBuildingUnits"))
        {
            ctrl = ctrl.Insert(ctrl.IndexOf("public async Task<IActionResult> AddBuildingUnit"), newAction);
            File.WriteAllText(ctrlPath, ctrl, new UTF8Encoding(true));
        }

        // 2. Update ManageBlock.cshtml addUnitModal
        string htmlPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string html = File.ReadAllText(htmlPath, Encoding.UTF8);

        string oldModal = @"<form id=""addUnitForm"" asp-action=""AddBuildingUnit"" method=""post"">";
        string newModalStart = @"<form id=""addUnitForm"" asp-action=""AddMultipleBuildingUnits"" method=""post"">";
        
        // Remove the old inputs and replace with a table
        string bodyPattern = @"<div class=""row"">\s*<div class=""col-md-6 mb-3"">.*?<div class=""modal-footer";
        string newBody = @"
                    <div class=""table-responsive mb-2"">
                        <table class=""table table-sm table-bordered align-middle"" id=""dynamicUnitsTable"">
                            <thead class=""table-light"">
                                <tr>
                                    <th>Kapı No / İsim</th>
                                    <th>Yapı / Tür</th>
                                    <th>Cephe</th>
                                    <th>Brüt Alan (m²)</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td><input type=""text"" name=""Units[0].DoorNumber"" class=""form-control form-control-sm"" placeholder=""Örn: 15 veya Kuzey"" required /></td>
                                    <td>
                                        <select name=""Units[0].UnitStructure"" class=""form-select form-select-sm"">
                                            <option value=""Ara Kat"" selected>Ara Kat</option>
                                            <option value=""Çatı Dubleksi"">Çatı Dubleksi</option>
                                            <option value=""Ters Dubleks"">Ters Dubleks</option>
                                            <option value=""Bahçe Katı"">Bahçe Katı</option>
                                            <option value=""Dükkan / Ticari"">Dükkan / Ticari</option>
                                            <option value=""Su Deposu"">Su Deposu</option>
                                            <option value=""Sığınak"">Sığınak</option>
                                            <option value=""Otopark"">Otopark</option>
                                            <option value=""Depo"">Depo</option>
                                            <option value=""Ortak Alan"">Diğer Ortak Alan</option>
                                        </select>
                                    </td>
                                    <td>
                                        <select name=""Units[0].FacadeDirection"" class=""form-select form-select-sm"">
                                            <option value="""">Seçiniz...</option>
                                            <option value=""Kuzey"">Kuzey</option>
                                            <option value=""Güney"">Güney</option>
                                            <option value=""Doğu"">Doğu</option>
                                            <option value=""Batı"">Batı</option>
                                            <option value=""Kuzeydoğu"">Kuzeydoğu</option>
                                            <option value=""Kuzeybatı"">Kuzeybatı</option>
                                            <option value=""Güneydoğu"">Güneydoğu</option>
                                            <option value=""Güneybatı"">Güneybatı</option>
                                        </select>
                                    </td>
                                    <td><input type=""number"" name=""Units[0].GrossSquareMeters"" class=""form-control form-control-sm"" /></td>
                                    <td></td>
                                </tr>
                            </tbody>
                        </table>
                        <button type=""button"" class=""btn btn-sm btn-outline-success"" id=""btnAddNewUnitRow""><i class=""bi bi-plus-circle""></i> Yeni Satır Ekle</button>
                    </div>
                    <div class=""form-text text-info"" style=""font-size:11px;""><i class=""bi bi-info-circle me-1""></i>Aradığınız tür listede yoksa, Sistem Ayarları > Tanımlamalar menüsünden yeni tür ekleyebilirsiniz. (Not: 3+1 gibi detaylar daire iç ölçülerinde belirlenecektir.)</div>
                </div>
                <div class=""modal-footer";

        if (html.Contains(oldModal)) html = html.Replace(oldModal, newModalStart);
        if (Regex.IsMatch(html, bodyPattern, RegexOptions.Singleline))
        {
            html = Regex.Replace(html, bodyPattern, newBody, RegexOptions.Singleline);
        }

        // Add JS for adding rows
        string jsAnchor = @"// Populate Copy Floor Modal";
        string newJs = @"
            var rowIdx = 1;
            $('#btnAddNewUnitRow').click(function() {
                var newRow = `<tr>
                                <td><input type=""text"" name=""Units[${rowIdx}].DoorNumber"" class=""form-control form-control-sm"" required /></td>
                                <td>
                                    <select name=""Units[${rowIdx}].UnitStructure"" class=""form-select form-select-sm"">
                                        <option value=""Ara Kat"" selected>Ara Kat</option>
                                        <option value=""Çatı Dubleksi"">Çatı Dubleksi</option>
                                        <option value=""Ters Dubleks"">Ters Dubleks</option>
                                        <option value=""Bahçe Katı"">Bahçe Katı</option>
                                        <option value=""Dükkan / Ticari"">Dükkan / Ticari</option>
                                        <option value=""Su Deposu"">Su Deposu</option>
                                        <option value=""Sığınak"">Sığınak</option>
                                        <option value=""Otopark"">Otopark</option>
                                        <option value=""Depo"">Depo</option>
                                        <option value=""Ortak Alan"">Diğer Ortak Alan</option>
                                    </select>
                                </td>
                                <td>
                                    <select name=""Units[${rowIdx}].FacadeDirection"" class=""form-select form-select-sm"">
                                        <option value="""">Seçiniz...</option>
                                        <option value=""Kuzey"">Kuzey</option>
                                        <option value=""Güney"">Güney</option>
                                        <option value=""Doğu"">Doğu</option>
                                        <option value=""Batı"">Batı</option>
                                        <option value=""Kuzeydoğu"">Kuzeydoğu</option>
                                        <option value=""Kuzeybatı"">Kuzeybatı</option>
                                        <option value=""Güneydoğu"">Güneydoğu</option>
                                        <option value=""Güneybatı"">Güneybatı</option>
                                    </select>
                                </td>
                                <td><input type=""number"" name=""Units[${rowIdx}].GrossSquareMeters"" class=""form-control form-control-sm"" /></td>
                                <td><button type=""button"" class=""btn btn-sm btn-danger remove-unit-row""><i class=""bi bi-trash""></i></button></td>
                              </tr>`;
                $('#dynamicUnitsTable tbody').append(newRow);
                rowIdx++;
            });

            $(document).on('click', '.remove-unit-row', function() {
                $(this).closest('tr').remove();
            });

            // Populate Copy Floor Modal";

        if(html.Contains(jsAnchor)) html = html.Replace(jsAnchor, newJs);

        File.WriteAllText(htmlPath, html, new UTF8Encoding(true));
        Console.WriteLine("Dynamic unit grid added!");
    }
}
