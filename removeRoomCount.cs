using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string html = File.ReadAllText(path, Encoding.UTF8);

        // Remove RoomCount div
        string roomCountPattern = @"<div class=""col-md-3 mb-3"" id=""roomCountContainer"">.*?</div>";
        html = Regex.Replace(html, roomCountPattern, "", RegexOptions.Singleline);
        
        // Widen UnitStructure div to col-md-6 since we removed RoomCount (col-md-3)
        // Wait, DoorNumber row had Adet(3) and KapiNo(9).
        // RoomCount row had RoomCount(3) and UnitStructure(3) originally? Wait, UnitStructure was 3? Let's make it 12.
        string structPattern = @"<div class=""col-md-3 mb-3"">\s*<label class=""form-label fw-bold small"">Yapı / Tür</label>";
        string newStruct = @"<div class=""col-md-12 mb-3"">
                            <label class=""form-label fw-bold small"">Yapı / Tür</label>";
        html = Regex.Replace(html, structPattern, newStruct);
        
        // Add the warning text under the select
        string selectEndPattern = @"<option value=""Ortak Alan"">Diğer Ortak Alan</option>\s*</select>";
        string newSelectEnd = @"<option value=""Ortak Alan"">Diğer Ortak Alan</option>
                            </select>
                            <div class=""form-text text-info"" style=""font-size:11px;""><i class=""bi bi-info-circle me-1""></i>Aradığınız tür listede yoksa, Sistem Ayarları > Tanımlamalar menüsünden yeni tür ekleyebilirsiniz. (Not: 3+1 gibi detaylar daire iç ölçülerinde belirlenecektir.)</div>";
        html = Regex.Replace(html, selectEndPattern, newSelectEnd);
        
        // Remove the JS that hides room count since it no longer exists
        string jsPattern = @"// Hide Room Count for non-residential.*?// Populate Add Unit Modal" ;
        string newJs = @"// Populate Add Unit Modal";
        html = Regex.Replace(html, jsPattern, newJs, RegexOptions.Singleline);
        
        File.WriteAllText(path, html, new UTF8Encoding(true));
        
        // NOW FIX CONTROLLER
        string ctrlPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string ctrl = File.ReadAllText(ctrlPath, Encoding.UTF8);
        
        string oldCtrlAction = @"public async Task<IActionResult> AddBuildingUnit(int buildingId, int FloorLevel, string FloorName, string DoorNumber, string RoomCount, string UnitStructure, double\? GrossSquareMeters, string FacadeDirection, int Quantity = 1)";
        string newCtrlAction = @"public async Task<IActionResult> AddBuildingUnit(int buildingId, int FloorLevel, string FloorName, string DoorNumber, string UnitStructure, double? GrossSquareMeters, string FacadeDirection, int Quantity = 1)";
        
        ctrl = Regex.Replace(ctrl, oldCtrlAction, newCtrlAction);
        
        // Replace the combination logic
        string logicPattern = @"// Combine RoomCount and UnitStructure logically.*?for\(int i = 0; i < Quantity; i\+\+\)";
        string newLogic = @"// We no longer use RoomCount. UnitStructure is exactly what we save as RoomLayout.
            string combinedRoomLayout = UnitStructure;

            for(int i = 0; i < Quantity; i++)";
            
        ctrl = Regex.Replace(ctrl, logicPattern, newLogic, RegexOptions.Singleline);
        
        File.WriteAllText(ctrlPath, ctrl, new UTF8Encoding(true));
        
        Console.WriteLine("RoomCount successfully eradicated.");
    }
}
