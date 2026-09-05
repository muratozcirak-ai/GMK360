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

        string find = @"public async Task<IActionResult> Create\(\[Bind\(""Name,Description,Address,StartDate,EndDate,CoverImageUrl""\)\] ConstructionProject project\)";
        string replace = @"[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(""Name,Description,Address,StartDate,EndDate,CoverImageUrl"")] ConstructionProject project)";

        code = Regex.Replace(code, find, replace);
        
        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed ambiguous Create method routing.");
    }
}
