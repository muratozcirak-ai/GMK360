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

        string find = @"public async Task<IActionResult> Create\(int\? projectId = null\) \{";
        string replace = @"public async Task<IActionResult> Create(int? id = null) { 
            int? projectId = id;";

        code = Regex.Replace(code, find, replace, RegexOptions.Singleline);
        
        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed routing parameter name.");
    }
}
