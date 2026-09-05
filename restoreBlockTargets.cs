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

        if (!code.Contains("public int TotalApartments"))
        {
            code = code.Replace("public int TotalFloors { get; set; }", 
                "public int TotalFloors { get; set; }\r\n        public int TotalApartments { get; set; }\r\n        public int TotalShops { get; set; }");
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Restored TotalApartments to WizardBlockItem");
        }
    }
}
