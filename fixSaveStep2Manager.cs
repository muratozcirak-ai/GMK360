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

        string oldSaveStep2 = @"                              CityId = project.CityId ?? 1,
                                DistrictId = project.DistrictId ?? 1,
                                NeighborhoodId = project.NeighborhoodId ?? 1,
                                StreetId = project.StreetId,
                                BuildingNumber = project.Address ?? """",
                                StreetName = project.Address ?? """"
                          };";

        string newSaveStep2 = @"                              CityId = project.CityId ?? 1,
                                DistrictId = project.DistrictId ?? 1,
                                NeighborhoodId = project.NeighborhoodId ?? 1,
                                StreetId = project.StreetId,
                                BuildingNumber = project.Address ?? """",
                                StreetName = project.Address ?? """",
                                ManagerUserId = _userManager.GetUserId(User) ?? project.ManagerUserId
                          };";

        if(code.Contains(oldSaveStep2))
        {
            code = code.Replace(oldSaveStep2, newSaveStep2);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Fixed SaveStep2 ManagerUserId.");
        }
        else
        {
            Console.WriteLine("Could not find the target string.");
        }
    }
}
