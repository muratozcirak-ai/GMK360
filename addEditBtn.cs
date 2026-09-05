using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Details.cshtml";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string oldButton = @"<a href=""/ConstructionProject"" class=""btn btn-light""><i class=""bi bi-arrow-left""></i> Projelere Dön</a>";
        string newButtons = @"<a href=""/ConstructionProject"" class=""btn btn-light""><i class=""bi bi-arrow-left""></i> Projelere Dön</a>
            <a href=""/ConstructionProject/Create/@Model.Id"" class=""btn btn-warning text-dark fw-bold ms-2""><i class=""bi bi-magic""></i> Sihirbaza Dön / Düzenle</a>";

        if (code.Contains(oldButton))
        {
            code = code.Replace(oldButton, newButtons);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Added button to Details.");
        }
        else
        {
            Console.WriteLine("Button not found. Trying another format.");
            if (code.Contains("Projelere Dön")) {
                int idx = code.IndexOf("Projelere Dön");
                int aEnd = code.IndexOf("</a>", idx);
                code = code.Insert(aEnd + 4, @" <a href=""/ConstructionProject/Create/@Model.Id"" class=""btn btn-warning text-dark fw-bold ms-2""><i class=""bi bi-magic""></i> Sihirbaza Dön / Düzenle</a>");
                File.WriteAllText(path, code, new UTF8Encoding(true));
                Console.WriteLine("Added button fallback.");
            }
        }
    }
}
