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

        string find = @"public async Task<IActionResult> Create\(\) \{.*?return View\(\); \}";
        string replace = @"public async Task<IActionResult> Create() { 
            var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync(); 
            ViewBag.Cities = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(cities, ""Id"", ""Name""); 
            ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>()); 
            ViewBag.Neighborhoods = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>()); 
            ViewBag.Streets = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>()); 
            return View(new GMK360.Web.Models.CreateProjectWizardViewModel()); 
        }";

        code = Regex.Replace(code, find, replace, RegexOptions.Singleline);
        
        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed GET Create method to pass empty model.");
    }
}
