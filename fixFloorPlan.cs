using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        string oldHeader = @"                                <h2 class=""accordion-header"" id=""@headingId"">
                                    <button class=""accordion-button bg-light fw-bold collapsed"" type=""button"" data-bs-toggle=""collapse"" data-bs-target=""#@collapseId"" aria-expanded=""false"" aria-controls=""@collapseId"">
                                        <i class=""bi bi-building-up me-2 text-primary""></i> @floorName
                                        <span class=""badge bg-secondary ms-auto me-3"">@(floorGroup.Count()) Birim</span>
                                        @if(planDoc != null)
                                        {
                                            <span class=""badge bg-success me-2""><i class=""bi bi-file-earmark-image""></i> Plan Yüklü</span>
                                        }
                                    </button>
                                </h2>";
                                
        string newHeader = @"                                <h2 class=""accordion-header d-flex align-items-center bg-light border-bottom"" id=""@headingId"">
                                    <button class=""accordion-button bg-transparent fw-bold collapsed flex-grow-1 shadow-none border-0"" type=""button"" data-bs-toggle=""collapse"" data-bs-target=""#@collapseId"" aria-expanded=""false"" aria-controls=""@collapseId"">
                                        <i class=""bi bi-building-up me-2 text-primary""></i> @floorName
                                        <span class=""badge bg-secondary ms-3"">@(floorGroup.Count()) Birim</span>
                                    </button>
                                    
                                    <div class=""pe-3 d-flex align-items-center"" style=""min-width: 280px; z-index: 2;"">
                                        @if(planDoc != null)
                                        {
                                            <a href=""@planDoc.DocumentUrl"" target=""_blank"" class=""btn btn-sm btn-success rounded-pill me-2""><i class=""bi bi-file-earmark-image me-1""></i> Planı Gör</a>
                                            <form asp-action=""DeleteArchitectureMedia"" method=""post"" class=""m-0 p-0"">
                                                <input type=""hidden"" name=""documentId"" value=""@planDoc.Id"" />
                                                <input type=""hidden"" name=""buildingId"" value=""@Model.Id"" />
                                                <button type=""submit"" class=""btn btn-sm btn-outline-danger rounded-circle p-1"" onclick=""return confirm('Silmek istediğinize emin misiniz?');"" title=""Planı Sil""><i class=""bi bi-trash""></i></button>
                                            </form>
                                        }
                                        else
                                        {
                                            <form asp-action=""UploadArchitectureMedia"" method=""post"" enctype=""multipart/form-data"" class=""d-flex m-0 p-0 w-100"">
                                                <input type=""hidden"" name=""buildingId"" value=""@Model.Id"" />
                                                <input type=""hidden"" name=""entityType"" value=""BuildingFloor_@Model.Id"" />
                                                <input type=""hidden"" name=""entityId"" value=""@floorGroup.Key"" />
                                                <input type=""hidden"" name=""title"" value=""@floorName Planı"" />
                                                <div class=""input-group input-group-sm"">
                                                    <input type=""file"" name=""file"" class=""form-control form-control-sm"" accept=""image/*,.pdf"" required />
                                                    <button type=""submit"" class=""btn btn-outline-primary""><i class=""bi bi-upload""></i></button>
                                                </div>
                                            </form>
                                        }
                                    </div>
                                </h2>";

        string oldFooter = @"                                        <!-- KAT PLANI BÖLÜMÜ -->
                                        <div class=""bg-white p-3 text-end border-top d-flex justify-content-between align-items-center"">
                                            @if(planDoc != null)
                                            {
                                                <div>
                                                    <a href=""@planDoc.DocumentUrl"" target=""_blank"" class=""btn btn-sm btn-success rounded-pill""><i class=""bi bi-file-earmark-image me-1""></i> @floorName Planını Görüntüle</a>
                                                </div>
                                                <form asp-action=""DeleteArchitectureMedia"" method=""post"" class=""d-inline"">
                                                    <input type=""hidden"" name=""documentId"" value=""@planDoc.Id"" />
                                                    <input type=""hidden"" name=""buildingId"" value=""@Model.Id"" />
                                                    <button type=""submit"" class=""btn btn-sm btn-outline-danger rounded-pill"" onclick=""return confirm('Kat planını silmek istediğinize emin misiniz?');"">Planı Sil</button>
                                                </form>
                                            }
                                            else
                                            {
                                                <div class=""w-100"">
                                                    <form asp-action=""UploadArchitectureMedia"" method=""post"" enctype=""multipart/form-data"" class=""d-flex align-items-center justify-content-end"">
                                                        <input type=""hidden"" name=""buildingId"" value=""@Model.Id"" />
                                                        <input type=""hidden"" name=""entityType"" value=""BuildingFloor_@Model.Id"" />
                                                        <input type=""hidden"" name=""entityId"" value=""@floorGroup.Key"" />
                                                        <input type=""hidden"" name=""title"" value=""@floorName Planı"" />
                                                        
                                                        <label class=""small text-muted me-2"">Bu Kata Ait Planı Yükle:</label>
                                                        <div class=""input-group input-group-sm"" style=""max-width: 300px;"">
                                                            <input type=""file"" name=""file"" class=""form-control"" accept=""image/*,.pdf"" required />
                                                            <button type=""submit"" class=""btn btn-primary""><i class=""bi bi-upload""></i></button>
                                                        </div>
                                                    </form>
                                                </div>
                                            }
                                        </div>";

        if (text.Contains(oldHeader))
        {
            text = text.Replace(oldHeader, newHeader);
            text = text.Replace(oldFooter, "");
            File.WriteAllText(path, text, new UTF8Encoding(true));
            Console.WriteLine("Floor Plan UI Moved to Header");
        }
        else
        {
            Console.WriteLine("Old header not found");
        }
    }
}
