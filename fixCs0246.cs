using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        code = code.Replace("public async Task<IActionResult> SaveStep1([FromForm] CreateProjectWizardViewModel model)", "public async Task<IActionResult> SaveStep1([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)");
        code = code.Replace("public async Task<IActionResult> SaveStep2([FromForm] CreateProjectWizardViewModel model)", "public async Task<IActionResult> SaveStep2([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)");

        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed namespace issue.");
    }
}
