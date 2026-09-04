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
        
        string oldUploadForm = @"                                            <form asp-action=""UploadArchitectureMedia"" method=""post"" enctype=""multipart/form-data"" class=""d-flex m-0 p-0 w-100"">
                                                <input type=""hidden"" name=""buildingId"" value=""@Model.Id"" />
                                                <input type=""hidden"" name=""entityType"" value=""BuildingFloor_@Model.Id"" />
                                                <input type=""hidden"" name=""entityId"" value=""@floorGroup.Key"" />
                                                <input type=""hidden"" name=""title"" value=""@floorName Planı"" />
                                                <div class=""input-group input-group-sm"">
                                                    <input type=""file"" name=""file"" class=""form-control form-control-sm"" accept=""image/*,.pdf"" required />
                                                    <button type=""submit"" class=""btn btn-primary""><i class=""bi bi-upload me-1""></i>Yükle</button>
                                                </div>
                                            </form>";
                                            
        string newUploadForm = @"                                            <form asp-action=""UploadArchitectureMedia"" method=""post"" enctype=""multipart/form-data"" class=""d-flex m-0 p-0 w-100"">
                                                <input type=""hidden"" name=""buildingId"" value=""@Model.Id"" />
                                                <input type=""hidden"" name=""entityType"" value=""BuildingFloor_@Model.Id"" />
                                                <input type=""hidden"" name=""entityId"" value=""@floorGroup.Key"" />
                                                <input type=""hidden"" name=""title"" value=""@floorName Planı"" />
                                                <div class=""input-group input-group-sm"">
                                                    <input type=""text"" name=""PhysicalLocationNote"" class=""form-control form-control-sm"" placeholder=""Fiziksel Konum (Örn: Mavi Klasör)"" style=""max-width:200px;"" />
                                                    <input type=""file"" name=""file"" class=""form-control form-control-sm"" accept=""image/*,.pdf"" required style=""max-width:220px;"" />
                                                    <button type=""submit"" class=""btn btn-primary""><i class=""bi bi-upload""></i> Yükle</button>
                                                </div>
                                            </form>";
                                            
        // Need to make the div holding it slightly wider to accommodate the new input
        string oldDiv = @"<div class=""pe-3 d-flex align-items-center"" style=""min-width: 280px; z-index: 2;"">";
        string newDiv = @"<div class=""pe-3 d-flex align-items-center"" style=""min-width: 500px; z-index: 2;"">";

        if (html.Contains(oldUploadForm)) html = html.Replace(oldUploadForm, newUploadForm);
        if (html.Contains(oldDiv)) html = html.Replace(oldDiv, newDiv);
        
        File.WriteAllText(htmlPath, html, new UTF8Encoding(true));

        // 2. UPDATE CONTROLLER
        var ctrl = File.ReadAllText(ctrlPath, Encoding.UTF8);
        
        string oldActionStart = @"        public async Task<IActionResult> UploadArchitectureMedia(int buildingId, string entityType, int entityId, string title, IFormFile file)
        {";
        
        string newActionStart = @"        public async Task<IActionResult> UploadArchitectureMedia(int buildingId, string entityType, int entityId, string title, string PhysicalLocationNote, IFormFile file)
        {";
        
        string oldDocStart = @"                var doc = new GMK360.Core.Entities.DmsDocument
                {
                    Title = title ?? file.FileName,
                    DocumentUrl = ""/uploads/architecture/"" + uniqueFileName,
                    FileExtension = Path.GetExtension(file.FileName),
                    FileSizeBytes = file.Length,
                    EntityType = entityType, 
                    EntityId = entityId,
                    UploadedByUserId = _userManager.GetUserId(User) ?? """",
                    UploadDate = DateTime.UtcNow,
                    FolderId = defaultFolder.Id,
                    PhysicalLocationNote = ""Dijital Kopya""
                };";
                
        string newDocStart = @"                var doc = new GMK360.Core.Entities.DmsDocument
                {
                    Title = title ?? file.FileName,
                    DocumentUrl = ""/uploads/architecture/"" + uniqueFileName,
                    FileExtension = Path.GetExtension(file.FileName),
                    FileSizeBytes = file.Length,
                    EntityType = entityType, 
                    EntityId = entityId,
                    UploadedByUserId = _userManager.GetUserId(User) ?? """",
                    UploadDate = DateTime.UtcNow,
                    FolderId = defaultFolder.Id,
                    PhysicalLocationNote = string.IsNullOrWhiteSpace(PhysicalLocationNote) ? ""Belirtilmedi"" : PhysicalLocationNote
                };";

        if (ctrl.Contains(oldActionStart)) ctrl = ctrl.Replace(oldActionStart, newActionStart);
        if (ctrl.Contains(oldDocStart)) ctrl = ctrl.Replace(oldDocStart, newDocStart);
        
        File.WriteAllText(ctrlPath, ctrl, new UTF8Encoding(true));
        
        Console.WriteLine("PhysicalLocationNote manual input added.");
    }
}
