using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string vmPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Models\CreateProjectWizardViewModel.cs";
        string vm = File.ReadAllText(vmPath, Encoding.UTF8);

        if (!vm.Contains("public double? TotalLandArea"))
        {
            vm = vm.Replace("public double? Longitude { get; set; }", "public double? Longitude { get; set; }\n\n        public double? TotalLandArea { get; set; }");
            File.WriteAllText(vmPath, vm, new UTF8Encoding(true));
        }
        
        string ctrlPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string ctrl = File.ReadAllText(ctrlPath, Encoding.UTF8);
        if (!ctrl.Contains("TotalLandArea = model.TotalLandArea"))
        {
            ctrl = ctrl.Replace("Status = 1,", "Status = 1,\n                            TotalLandArea = model.TotalLandArea,");
            File.WriteAllText(ctrlPath, ctrl, new UTF8Encoding(true));
        }

        Console.WriteLine("TotalLandArea added to ViewModel and Controller.");
    }
}
