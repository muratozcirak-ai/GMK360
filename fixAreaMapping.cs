using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string ctrlPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string ctrl = File.ReadAllText(ctrlPath, Encoding.UTF8);

        if (!ctrl.Contains("TotalLandArea = model.TotalLandArea"))
        {
            ctrl = ctrl.Replace("Status = 0, // 0 = Upcoming", "Status = 0, // 0 = Upcoming\n                TotalLandArea = model.TotalLandArea,");
            File.WriteAllText(ctrlPath, ctrl, new UTF8Encoding(true));
        }

        Console.WriteLine("Controller fixed for TotalLandArea.");
    }
}
