using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        int idx = code.IndexOf("Task<IActionResult> CreateWizard");
        if (idx != -1) {
            Console.WriteLine(code.Substring(idx, Math.Min(2000, code.Length - idx)));
        }
    }
}
