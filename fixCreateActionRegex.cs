using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string ctrl = File.ReadAllText(path, Encoding.UTF8);

        string pattern = @"public async Task<IActionResult> Create\(\)\s*\{\s*ViewBag\.Cities = await _context\.Cities\.OrderBy\(c => c\.Name\)\.ToListAsync\(\);\s*return View\(\);\s*\}";
        string replacement = @"public async Task<IActionResult> Create() { var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync(); ViewBag.Cities = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(cities, ""Id"", ""Name""); ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>()); ViewBag.Neighborhoods = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>()); ViewBag.Streets = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>()); return View(); }";

        if (Regex.IsMatch(ctrl, pattern))
        {
            ctrl = Regex.Replace(ctrl, pattern, replacement);
            File.WriteAllText(path, ctrl, new UTF8Encoding(true));
            Console.WriteLine("Fixed successfully.");
        }
        else
        {
            Console.WriteLine("Not found!");
        }
    }
}
