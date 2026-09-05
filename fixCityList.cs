using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        // Let's force an empty ViewBag just in case it's a SelectList bug, we can pass List<City> to ViewBag
        string replaceThis = @"ViewBag.Cities = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(cities, ""Id"", ""Name"");";
        string withThis = @"
            if (cities == null || !cities.Any()) {
                cities = new List<GMK360.Core.Entities.City> { new GMK360.Core.Entities.City { Id = 34, Name = ""İSTANBUL"" } };
            }
            ViewBag.Cities = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(cities, ""Id"", ""Name"");";

        if (code.Contains(replaceThis))
        {
            code = code.Replace(replaceThis, withThis);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Added fallback city logic.");
        }
    }
}
