using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Details.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        string target = @"                        <div class=""fw-bold"">@(Model.Blocks?.Count ?? 0) Adet</div>
                    </div>
                </div>";
                
        string replacement = target + @"

                <div class=""mt-4 pt-3 border-top position-relative"">
                    <div class=""d-flex justify-content-between align-items-center mb-3"">
                        <h6 class=""fw-bold mb-0 text-muted"">Arazi ve Dış Alanlar</h6>
                        <a asp-action=""Amenities"" asp-route-projectId=""@Model.Id"" class=""btn btn-sm btn-outline-primary rounded-pill""><i class=""bi bi-pencil me-1""></i> Yönet</a>
                    </div>
                    <div class=""row g-3"">
                        <div class=""col-sm-6"">
                            <div class=""d-flex align-items-center"">
                                <i class=""bi bi-aspect-ratio text-success fs-4 me-3""></i>
                                <div>
                                    <div class=""text-muted small"">Toplam Arazi Alanı</div>
                                    <div class=""fw-bold text-dark"">@(Model.TotalLandArea.HasValue ? Model.TotalLandArea.Value + "" m²"" : ""Belirtilmedi"")</div>
                                </div>
                            </div>
                        </div>
                        <div class=""col-sm-6"">
                            <div class=""d-flex align-items-center"">
                                <i class=""bi bi-tree text-success fs-4 me-3""></i>
                                <div>
                                    <div class=""text-muted small"">Peyzaj / Yeşil Alan</div>
                                    <div class=""fw-bold text-dark"">@(Model.LandscapeArea.HasValue ? Model.LandscapeArea.Value + "" m²"" : ""Belirtilmedi"")</div>
                                </div>
                            </div>
                        </div>
                        
                        @if (Model.Amenities != null && Model.Amenities.Any())
                        {
                            foreach(var amenity in Model.Amenities)
                            {
                                <div class=""col-sm-6"">
                                    <div class=""d-flex align-items-center"">
                                        <i class=""bi bi-check-circle text-primary fs-4 me-3""></i>
                                        <div>
                                            <div class=""text-muted small"">@amenity.Name</div>
                                            <div class=""fw-bold text-dark"">
                                                @(amenity.SquareMeters.HasValue ? amenity.SquareMeters.Value + "" m²"" : ""Mevcut"")
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            }
                        }
                        else
                        {
                            <div class=""col-12 mt-3"">
                                <div class=""text-muted small fst-italic""><i class=""bi bi-info-circle me-1""></i>Kamelya, açık otopark, çocuk parkı gibi dış alan donatıları henüz eklenmedi.</div>
                            </div>
                        }
                    </div>
                </div>";

        if(text.Contains(target))
        {
            text = text.Replace(target, replacement);
            File.WriteAllText(path, text, new UTF8Encoding(true));
            Console.WriteLine("Added properly");
        }
        else
        {
            Console.WriteLine("Target not found!");
        }
    }
}
