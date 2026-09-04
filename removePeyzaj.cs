using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Details.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        string pattern = @"<div class=""col-sm-6"">\s*<div class=""d-flex align-items-center"">\s*<i class=""bi bi-tree text-success fs-4 me-3""></i>\s*<div>\s*<div class=""text-muted small"">Peyzaj / Yeşil Alan</div>\s*<div class=""fw-bold text-dark"">@\(Model\.LandscapeArea\.HasValue \? Model\.LandscapeArea\.Value \+ "" m²"" : ""Belirtilmedi""\)</div>\s*</div>\s*</div>\s*</div>";
        
        text = Regex.Replace(text, pattern, "");
        
        File.WriteAllText(path, text, new UTF8Encoding(true));
        Console.WriteLine("Removed Peyzaj from Details");
    }
}
