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

        string find = @"draft\.Blocks\.Add\(new Building\s*\{\s*AgencyId = agencyId\.Value,\s*BlockName = b\.BlockName,\s*BaseArea = b\.BaseArea,\s*TotalFloors = b\.TotalFloors,\s*BasementFloors = b\.BasementFloors,\s*TotalUnits = b\.TotalApartments \+ b\.TotalShops,\s*HasGroundFloor = b\.HasGroundFloor,\s*HasRoof = b\.HasRoof,\s*BuildingType = 0\s*\}\);";
        
        string replace = @"draft.Blocks.Add(new Building
                    {
                        AgencyId = agencyId.Value,
                        Name = draft.Name + "" - "" + b.BlockName,
                        StreetName = ""Belirtilmedi"",
                        BuildingNumber = draft.Address ?? ""Belirtilmedi"",
                        BlockName = b.BlockName,
                        BaseArea = b.BaseArea,
                        TotalFloors = b.TotalFloors,
                        BasementFloors = b.BasementFloors,
                        TotalUnits = b.TotalApartments + b.TotalShops,
                        HasGroundFloor = b.HasGroundFloor,
                        HasRoof = b.HasRoof,
                        BuildingType = 0
                    });";

        if (Regex.IsMatch(code, find, RegexOptions.Singleline))
        {
            code = Regex.Replace(code, find, replace, RegexOptions.Singleline);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Fixed Building required fields in SaveStep2.");
        }
        else
        {
            Console.WriteLine("Could not find Building creation in SaveStep2.");
        }
    }
}
