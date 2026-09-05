using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string oldMapping = @"TotalLandArea = project\.TotalLandArea,\s*CityId = project\.CityId \?\? 0,";
        string newMapping = @"TotalLandArea = project.TotalLandArea,
                      Latitude = project.Latitude,
                      Longitude = project.Longitude,
                      CityId = project.CityId ?? 0,";
        
        if (Regex.IsMatch(code, oldMapping)) {
            code = Regex.Replace(code, oldMapping, newMapping);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Updated Create mapping.");
        }
    }
}
