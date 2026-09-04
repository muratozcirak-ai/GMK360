using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string ctrl = File.ReadAllText(path, Encoding.UTF8);

        string oldCreate = @"        public async Task<IActionResult> Create()
        {
            ViewBag.Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            return View();
        }";

        string newCreate = @"        public async Task<IActionResult> Create()
        {
            var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Cities = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(cities, ""Id"", ""Name"");
            ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new List<SelectListItem>());
            ViewBag.Neighborhoods = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new List<SelectListItem>());
            ViewBag.Streets = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new List<SelectListItem>());
            return View();
        }";

        if (ctrl.Contains(oldCreate))
        {
            ctrl = ctrl.Replace(oldCreate, newCreate);
            File.WriteAllText(path, ctrl, new UTF8Encoding(true));
            Console.WriteLine("Controller Create GET action updated with SelectList.");
        }
        else
        {
            Console.WriteLine("Could not find the Create method to replace.");
        }
    }
}
