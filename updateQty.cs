using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string htmlPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string ctrlPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        
        // 1. UPDATE HTML
        string html = File.ReadAllText(htmlPath, Encoding.UTF8);
        
        string oldDoor = @"                        <div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Kapı No</label>
                            <input type=""text"" name=""DoorNumber"" class=""form-control"" required />
                        </div>";
                        
        string newDoor = @"                        <div class=""col-md-3 mb-3"">
                            <label class=""form-label fw-bold small"">Adet (Çoklu)</label>
                            <input type=""number"" name=""Quantity"" class=""form-control"" value=""1"" min=""1"" required />
                        </div>
                        <div class=""col-md-9 mb-3"">
                            <label class=""form-label fw-bold small"">Kapı No / İsim</label>
                            <input type=""text"" name=""DoorNumber"" class=""form-control"" placeholder=""Örn: 15 veya Su Deposu"" required />
                            <div class=""form-text"" style=""font-size:10px;"">Adet 1'den büyükse sonuna sayı eklenir (Örn: Depo 1, Depo 2)</div>
                        </div>";
                        
        string oldStruct = @"                            <select name=""UnitStructure"" class=""form-select"">
                                <option value=""Ara Kat"" selected>Ara Kat</option>
                                <option value=""Çatı Dubleksi"">Çatı Dubleksi</option>
                                <option value=""Ters Dubleks"">Ters Dubleks</option>
                                <option value=""Bahçe Katı"">Bahçe Katı</option>
                                <option value=""Dükkan / Ticari"">Dükkan / Ticari</option>
                                <option value=""Ortak Alan"">Ortak Alan (Sığınak, Kömürlük vb.)</option>
                                <option value=""Depo"">Depo</option>
                            </select>";
                            
        string newStruct = @"                            <select name=""UnitStructure"" class=""form-select"" id=""unitStructureSelect"">
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
                            </select>";
                            
        string jsStart = @"// Populate Add Unit Modal when clicked from a specific floor";
        string newJsStart = @"// Hide Room Count for non-residential
            $('#unitStructureSelect').change(function() {
                var val = $(this).val();
                if (val === 'Su Deposu' || val === 'Sığınak' || val === 'Otopark' || val === 'Depo' || val === 'Ortak Alan' || val === 'Dükkan / Ticari') {
                    $('#roomCountContainer').hide();
                    $('#addUnitModal input[name=""RoomCount""]').val('-');
                } else {
                    $('#roomCountContainer').show();
                    if($('#addUnitModal input[name=""RoomCount""]').val() === '-') {
                        $('#addUnitModal input[name=""RoomCount""]').val('');
                    }
                }
            });
            
            // Populate Add Unit Modal when clicked from a specific floor";

        if (html.Contains(oldDoor)) html = html.Replace(oldDoor, newDoor);
        if (html.Contains(oldStruct)) html = html.Replace(oldStruct, newStruct);
        if (html.Contains(jsStart)) html = html.Replace(jsStart, newJsStart);
        
        // Also wrap RoomCount in id="roomCountContainer"
        string rcStart = @"<div class=""col-md-3 mb-3"">
                            <label class=""form-label fw-bold small"">Oda Sayısı / Tipi</label>";
        string rcNew = @"<div class=""col-md-3 mb-3"" id=""roomCountContainer"">
                            <label class=""form-label fw-bold small"">Oda Sayısı / Tipi</label>";
        if(html.Contains(rcStart)) html = html.Replace(rcStart, rcNew);
                            
        File.WriteAllText(htmlPath, html, new UTF8Encoding(true));

        // 2. UPDATE CONTROLLER
        var ctrl = File.ReadAllText(ctrlPath, Encoding.UTF8);
        string oldActionStart = @"        public async Task<IActionResult> AddBuildingUnit(int buildingId, int FloorLevel, string FloorName, string DoorNumber, string RoomCount, string UnitStructure, double? GrossSquareMeters, string FacadeDirection)
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
                if (string.IsNullOrWhiteSpace(RoomCount)) 
                {
                    combinedRoomLayout = UnitStructure;
                } 
                else 
                {
                    combinedRoomLayout = $""{RoomCount.Trim()} ({UnitStructure})"";
                }
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
        }";
        
        string newActionStart = @"        public async Task<IActionResult> AddBuildingUnit(int buildingId, int FloorLevel, string FloorName, string DoorNumber, string RoomCount, string UnitStructure, double? GrossSquareMeters, string FacadeDirection, int Quantity = 1)
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
            if (UnitStructure != ""Dükkan / Ticari"" && UnitStructure != ""Depo"" && UnitStructure != ""Su Deposu"" && UnitStructure != ""Sığınak"" && UnitStructure != ""Otopark"" && UnitStructure != ""Ortak Alan"" && RoomCount != ""-"")
            {
                if (string.IsNullOrWhiteSpace(RoomCount)) 
                {
                    combinedRoomLayout = UnitStructure;
                } 
                else 
                {
                    combinedRoomLayout = $""{RoomCount.Trim()} ({UnitStructure})"";
                }
            }

            for(int i = 0; i < Quantity; i++) 
            {
                string finalDoorName = DoorNumber;
                if(Quantity > 1) {
                    finalDoorName = $""{DoorNumber} {i + 1}"";
                }
                
                var newUnit = new GMK360.Core.Entities.BuildingUnit
                {
                    BuildingId = buildingId,
                    FloorLevel = FloorLevel,
                    FloorName = string.IsNullOrEmpty(FloorName) ? $""{FloorLevel}. Kat"" : FloorName,
                    DoorNumber = finalDoorName,
                    RoomLayout = combinedRoomLayout,
                    GrossSquareMeters = GrossSquareMeters,
                    FacadeDirection = FacadeDirection
                };
                _context.BuildingUnits.Add(newUnit);
            }
            
            await _context.SaveChangesAsync();

            TempData[""SuccessMessage""] = Quantity > 1 ? $""{Quantity} adet birim başarıyla eklendi."" : $""{DoorNumber} başarıyla eklendi."";
            return RedirectToAction(nameof(ManageBlock), new { id = buildingId });
        }";

        if (ctrl.Contains(oldActionStart)) {
            ctrl = ctrl.Replace(oldActionStart, newActionStart);
            File.WriteAllText(ctrlPath, ctrl, new UTF8Encoding(true));
            Console.WriteLine("Controller updated for Quantity");
        } else {
            Console.WriteLine("Controller old action not found!");
        }
    }
}
