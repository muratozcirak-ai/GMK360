using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string ctrlPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string ctrl = File.ReadAllText(ctrlPath, Encoding.UTF8);

        ctrl = ctrl.Replace("HasGroundFloor = b.TotalShops > 0,", "HasGroundFloor = b.HasGroundFloor,");
        
        File.WriteAllText(ctrlPath, ctrl, new UTF8Encoding(true));
        Console.WriteLine("Controller CreateWizard updated.");
    }
}
