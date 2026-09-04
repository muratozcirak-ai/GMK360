using System;
using System.IO;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Details.cshtml";
        string text = File.ReadAllText(path);

        string target = @"<i class=""bi bi-palette me-2""></i> Müşteri Malzeme Kataloğu
        </a>";
        
        string injection = @"<i class=""bi bi-palette me-2""></i> Müşteri Malzeme Kataloğu
        </a>
        <a asp-action=""Templates"" asp-route-projectId=""@Model.Id"" class=""btn btn-info rounded-pill px-4 shadow-sm text-white me-2 mt-2 mt-md-0"">
            <i class=""bi bi-collection me-2""></i> Daire Şablonları
        </a>
        <a asp-action=""Amenities"" asp-route-projectId=""@Model.Id"" class=""btn btn-success rounded-pill px-4 shadow-sm text-white mt-2 mt-md-0"">
            <i class=""bi bi-tree me-2""></i> Dış Alanlar
        </a>";

        if(text.Contains(@"<i class=""bi bi-palette me-2""></i> Müşteri Malzeme Kataloğu"))
        {
            text = text.Replace(target, injection);
            File.WriteAllText(path, text, System.Text.Encoding.UTF8);
            Console.WriteLine("Buttons injected successfully.");
        }
        else
        {
            Console.WriteLine("Target not found!");
        }
    }
}
