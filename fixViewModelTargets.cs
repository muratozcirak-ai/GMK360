using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Models\CreateProjectWizardViewModel.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        if (!code.Contains("TargetTotalApartments"))
        {
            code = code.Replace("public double? TotalLandArea { get; set; }", 
                "public double? TotalLandArea { get; set; }\r\n        public int? TargetTotalApartments { get; set; }\r\n        public int? TargetTotalShops { get; set; }");
        }

        // Remove from WizardBlockItem
        code = Regex.Replace(code, @"\s*public int TotalApartments \{ get; set; \}", "");
        code = Regex.Replace(code, @"\s*public int TotalShops \{ get; set; \}", "");

        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Updated CreateProjectWizardViewModel.cs");
    }
}
