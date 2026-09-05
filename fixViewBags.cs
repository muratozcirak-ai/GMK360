using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string oldCreateStart = @"        [HttpGet]
        public async Task<IActionResult> Create(int? id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();";

        string newCreateStart = @"        [HttpGet]
        public async Task<IActionResult> Create(int? id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Cities = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(cities, ""Id"", ""Name"");
            ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>());
            ViewBag.Neighborhoods = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>());
            ViewBag.Streets = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>());
";

        if (code.Contains(oldCreateStart))
        {
            code = code.Replace(oldCreateStart, newCreateStart);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Fixed ViewBags in Create.");
        }
    }
}
