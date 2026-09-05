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

        string oldBlock = @"CityId = 1,\s*DistrictId = 1,\s*NeighborhoodId = 1";
        string newBlock = @"CityId = project.CityId ?? 1,
                              DistrictId = project.DistrictId ?? 1,
                              NeighborhoodId = project.NeighborhoodId ?? 1,
                              StreetId = project.StreetId,
                              BuildingNumber = project.Address ?? """",
                              StreetName = project.Address ?? """"";

        code = Regex.Replace(code, oldBlock, newBlock);
        
        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed SaveStep2 with Regex.");
    }
}
