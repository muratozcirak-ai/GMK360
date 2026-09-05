using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string oldMapping = @"if \(project \=\= null\) return NotFound\(\);";
        string newMapping = @"if (project == null) return NotFound();
                  ViewBag.CoverImageUrl = project.CoverImageUrl;
                  ViewBag.CurrentStateImageUrl = project.CurrentStateImageUrl;";
        
        if (Regex.IsMatch(code, oldMapping)) {
            code = Regex.Replace(code, oldMapping, newMapping);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Updated ViewBag image mappings.");
        }
    }
}
