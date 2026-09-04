using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string ctrlPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string ctrl = File.ReadAllText(ctrlPath, Encoding.UTF8);

        ctrl = ctrl.Replace("building.HasRoof = b.HasRoof,\n                              BaseArea = b.BaseArea;", 
                            "building.HasRoof = b.HasRoof;\n                        building.BaseArea = b.BaseArea;");

        // Also just in case:
        ctrl = ctrl.Replace("building.HasRoof = b.HasRoof,\r\n                              BaseArea = b.BaseArea;", 
                            "building.HasRoof = b.HasRoof;\n                        building.BaseArea = b.BaseArea;");

        File.WriteAllText(ctrlPath, ctrl, new UTF8Encoding(true));
        Console.WriteLine("Syntax error fixed.");
    }
}
