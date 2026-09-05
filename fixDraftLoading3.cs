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

        string find = @"model\.Blocks = draft\.Blocks\.Select\(b => new GMK360\.Web\.Models\.WizardBlockItem \{\s*BlockName = b\.BlockName,\s*BaseArea = b\.BaseArea,\s*TotalFloors = b\.TotalFloors,\s*BasementFloors = b\.BasementFloors,\s*TotalApartments = b\.TotalApartments,\s*TotalShops = b\.TotalShops,\s*HasGroundFloor = b\.HasGroundFloor,\s*HasRoof = b\.HasRoof\s*\}\)\.ToList\(\);";
        
        string replace = @"model.Blocks = draft.Blocks.Select(b => new GMK360.Web.Models.WizardBlockItem {
                            BlockName = b.BlockName,
                            BaseArea = b.BaseArea,
                            TotalFloors = b.TotalFloors ?? 0,
                            BasementFloors = b.BasementFloors ?? 0,
                            TotalApartments = b.TotalUnits,
                            TotalShops = 0,
                            HasGroundFloor = b.HasGroundFloor,
                            HasRoof = b.HasRoof
                        }).ToList();";

        code = Regex.Replace(code, find, replace, RegexOptions.Singleline);
        
        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed building block properties.");
    }
}
