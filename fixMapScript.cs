using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(path, Encoding.UTF8);

        string leafletLinks = @"<link rel=""stylesheet"" href=""https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"" integrity=""sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY="" crossorigin=""""/>
<script src=""https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"" integrity=""sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo="" crossorigin=""""></script>
";

        if (!view.Contains("leaflet.css"))
        {
            // Insert after Layout declaration
            string searchStr = "Layout = \"~/Views/Shared/_ConstructionLayout.cshtml\";\r\n}";
            if (view.Contains(searchStr))
            {
                view = view.Replace(searchStr, searchStr + "\n\n" + leafletLinks);
                File.WriteAllText(path, view, new UTF8Encoding(true));
                Console.WriteLine("Added Leaflet.");
            }
            else
            {
                // Fallback for linux line endings
                string searchStr2 = "Layout = \"~/Views/Shared/_ConstructionLayout.cshtml\";\n}";
                if (view.Contains(searchStr2))
                {
                    view = view.Replace(searchStr2, searchStr2 + "\n\n" + leafletLinks);
                    File.WriteAllText(path, view, new UTF8Encoding(true));
                    Console.WriteLine("Added Leaflet.");
                }
                else
                {
                    Console.WriteLine("Could not find Layout declaration.");
                }
            }
        }
        else
        {
            Console.WriteLine("Leaflet already included.");
        }
    }
}
