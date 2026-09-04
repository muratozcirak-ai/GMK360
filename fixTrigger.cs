using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(path, Encoding.UTF8);

        string target = "});\n\n            // Ilce degisince mahalleyi doldur";
        string replacement = "});\n\n            if ($('#CityId').val()) { $('#CityId').trigger('change'); }\n\n            // Ilce degisince mahalleyi doldur";

        if (view.Contains(target) && !view.Contains(".trigger('change'); }"))
        {
            view = view.Replace(target, replacement);
            File.WriteAllText(path, view, new UTF8Encoding(true));
            Console.WriteLine("Added trigger change.");
        }
    }
}
