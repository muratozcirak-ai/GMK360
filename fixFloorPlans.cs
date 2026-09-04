using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string ctrl = File.ReadAllText(path, Encoding.UTF8);

        string oldViewBag = @"ViewBag.FloorPlans = architectureDocs.Where(d => d.EntityType == ""BuildingFloor"").ToList();";
        string newViewBag = @"ViewBag.FloorPlans = architectureDocs.Where(d => d.EntityType == floorEntityType).ToList();";

        if(ctrl.Contains(oldViewBag))
        {
            ctrl = ctrl.Replace(oldViewBag, newViewBag);
            File.WriteAllText(path, ctrl, new UTF8Encoding(true));
            Console.WriteLine("Fixed FloorPlans query.");
        }
    }
}
