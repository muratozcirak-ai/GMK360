using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string vmPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Models\CreateProjectWizardViewModel.cs";
        string vm = File.ReadAllText(vmPath, Encoding.UTF8);

        if (!vm.Contains("public bool HasGroundFloor { get; set; }"))
        {
            vm = vm.Replace("public bool HasRoof { get; set; }", "public bool HasRoof { get; set; }\n        public bool HasGroundFloor { get; set; } = true;");
            File.WriteAllText(vmPath, vm, new UTF8Encoding(true));
            Console.WriteLine("ViewModel updated.");
        }
    }
}
