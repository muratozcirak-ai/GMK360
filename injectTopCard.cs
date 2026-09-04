using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Amenities.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        string headerSearch = @"<div class=""row g-5"">";
        string topCard = @"
<div class=""card shadow-sm rounded-4 border-0 mb-5"">
    <div class=""card-body p-4"">
        <form asp-action=""UpdateProjectLandArea"" method=""post"" class=""d-flex flex-wrap align-items-end gap-4"">
            <input type=""hidden"" name=""projectId"" value=""@Model.Id"" />
            <div class=""flex-grow-1"">
                <label class=""form-label fw-bold""><i class=""bi bi-aspect-ratio text-success me-1""></i> Toplam Arazi Alanı (m²)</label>
                <input type=""number"" step=""0.1"" name=""totalLandArea"" class=""form-control form-control-solid rounded-3"" value=""@Model.TotalLandArea"" placeholder=""Örn: 2500"" />
            </div>
            <div class=""flex-grow-1"">
                <label class=""form-label fw-bold""><i class=""bi bi-tree text-success me-1""></i> Peyzaj / Yeşil Alan (m²)</label>
                <input type=""number"" step=""0.1"" name=""landscapeArea"" class=""form-control form-control-solid rounded-3"" value=""@Model.LandscapeArea"" placeholder=""Örn: 800"" />
            </div>
            <div>
                <button type=""submit"" class=""btn btn-success rounded-pill px-4 fw-bold""><i class=""bi bi-check2""></i> Arazi Değerlerini Kaydet</button>
            </div>
        </form>
    </div>
</div>
";
        if(text.Contains(headerSearch) && !text.Contains("UpdateProjectLandArea")) {
            text = text.Replace(headerSearch, topCard + headerSearch);
            File.WriteAllText(path, text, new UTF8Encoding(true));
        }
    }
}
