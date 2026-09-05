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

        string find = @"return Json\(new \{ success = true, draftId = project\.Id \}\);";
        string replace = @"return Json(new { success = true, draftId = project.Id, projectId = project.Id });";

        code = Regex.Replace(code, find, replace, RegexOptions.Singleline);
        
        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed SaveStep1 JSON return property name.");
    }
}
