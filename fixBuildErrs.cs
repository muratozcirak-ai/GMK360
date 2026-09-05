using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        // Fix CS0019
        code = code.Replace("StartDate = project.StartDate ?? DateTime.Now,", "StartDate = project.StartDate,");
        code = code.Replace("EndDate = project.EndDate ?? DateTime.Now.AddYears(1),", "EndDate = project.EndDate,");

        // Fix CS0234
        code = code.Replace("new GMK360.Web.Models.BlockWizardViewModel", "new GMK360.Web.Models.WizardBlockItem");

        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed errors.");
    }
}
