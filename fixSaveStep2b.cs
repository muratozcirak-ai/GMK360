using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string oldSaveStep2 = @"                              HasRoof = b.HasRoof,
                              CityId = 1, 
                              DistrictId = 1,
                              NeighborhoodId = 1
                          };";

        string newSaveStep2 = @"                              HasRoof = b.HasRoof,
                              CityId = project.CityId ?? 1, 
                              DistrictId = project.DistrictId ?? 1,
                              NeighborhoodId = project.NeighborhoodId ?? 1,
                              StreetId = project.StreetId,
                              BuildingNumber = project.Address ?? """",
                              StreetName = project.Address ?? """"
                          };";

        if (code.Contains(oldSaveStep2))
        {
            code = code.Replace(oldSaveStep2, newSaveStep2);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Fixed SaveStep2 building logic.");
        }
    }
}
