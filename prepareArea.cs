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
            vm = vm.Replace("public double? Longitude;", "public double? Longitude;\n\n        public double? TotalLandArea { get; set; }");
            File.WriteAllText(vmPath, vm, new UTF8Encoding(true));
        }

        string ctrlPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string ctrl = File.ReadAllText(ctrlPath, Encoding.UTF8);
        if (!ctrl.Contains("TotalLandArea = model.TotalLandArea"))
        {
            ctrl = ctrl.Replace("Status = 1,", "Status = 1,\n                TotalLandArea = model.TotalLandArea,");
            File.WriteAllText(ctrlPath, ctrl, new UTF8Encoding(true));
        }

        string viewPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(viewPath, Encoding.UTF8);

        // In Create.cshtml, find the address fields row (Row 5) and add the TotalLandArea field.
        // It's probably after Kapı No / Bina No.
        string htmlToFind = @"<label class=""form-label small fw-bold"">Kapı No / Bina No</label>";
        
        // Actually, let's just insert it after the whole Row 5 or inside it.
        // Let's check what Row 5 looks like.
        Console.WriteLine("Done preparing.");
    }
}
