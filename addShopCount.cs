using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string buildingPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Building.cs";
        string buildingCode = File.ReadAllText(buildingPath, Encoding.UTF8);

        if (!buildingCode.Contains("public int TotalShops { get; set; } = 0;"))
        {
            string replace = @"public int TotalUnits { get; set; }
        public int TotalShops { get; set; } = 0;
        public int TotalApartments { get; set; } = 0;";
            buildingCode = buildingCode.Replace("public int TotalUnits { get; set; }", replace);
            File.WriteAllText(buildingPath, buildingCode, new UTF8Encoding(true));
            Console.WriteLine("Added TotalShops and TotalApartments to Building.cs");
        }

        string controllerPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string controllerCode = File.ReadAllText(controllerPath, Encoding.UTF8);

        // Update Create(int? id) mapping
        string findMap = @"TotalApartments = b\.TotalUnits,\s*TotalShops = 0,";
        string replaceMap = @"TotalApartments = b.TotalApartments > 0 ? b.TotalApartments : b.TotalUnits, // Geriye dönük uyumluluk
                              TotalShops = b.TotalShops,";
        if (Regex.IsMatch(controllerCode, findMap, RegexOptions.Singleline))
        {
            controllerCode = Regex.Replace(controllerCode, findMap, replaceMap, RegexOptions.Singleline);
            Console.WriteLine("Updated Create mapping");
        }

        // Update SaveStep2 mapping
        string findSave = @"existingBlock\.TotalUnits = b\.TotalApartments \+ b\.TotalShops;\s*existingBlock\.HasRoof";
        string replaceSave = @"existingBlock.TotalUnits = b.TotalApartments + b.TotalShops;
                            existingBlock.TotalApartments = b.TotalApartments;
                            existingBlock.TotalShops = b.TotalShops;
                            existingBlock.HasRoof";
        if (Regex.IsMatch(controllerCode, findSave, RegexOptions.Singleline))
        {
            controllerCode = Regex.Replace(controllerCode, findSave, replaceSave, RegexOptions.Singleline);
            Console.WriteLine("Updated SaveStep2 existingBlock mapping");
        }

        string findSaveNew = @"TotalUnits = b\.TotalApartments \+ b\.TotalShops,\s*HasBlock = true,";
        string replaceSaveNew = @"TotalUnits = b.TotalApartments + b.TotalShops,
                                TotalApartments = b.TotalApartments,
                                TotalShops = b.TotalShops,
                                HasBlock = true,";
        if (Regex.IsMatch(controllerCode, findSaveNew, RegexOptions.Singleline))
        {
            controllerCode = Regex.Replace(controllerCode, findSaveNew, replaceSaveNew, RegexOptions.Singleline);
            Console.WriteLine("Updated SaveStep2 new building mapping");
        }
        
        File.WriteAllText(controllerPath, controllerCode, new UTF8Encoding(true));
    }
}
