using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string oldLists = @"            ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>());
            ViewBag.Neighborhoods = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>());
            ViewBag.Streets = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>());";

        string newLists = @"
            var emptyList = new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
            ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(emptyList);
            ViewBag.Neighborhoods = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(emptyList);
            ViewBag.Streets = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(emptyList);";

        if (code.Contains(oldLists))
        {
            code = code.Replace(oldLists, newLists);
        }

        string oldReturnView = @"                if (project == null) return NotFound();

                var model = new GMK360.Web.Models.CreateProjectWizardViewModel";

        string newReturnView = @"                if (project == null) return NotFound();

                if (project.CityId.HasValue) {
                    var districts = await _context.Districts.Where(d => d.CityId == project.CityId.Value).OrderBy(d => d.Name).ToListAsync();
                    ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(districts, ""Id"", ""Name"");
                }
                if (project.DistrictId.HasValue) {
                    var neighborhoods = await _context.Neighborhoods.Where(n => n.DistrictId == project.DistrictId.Value).OrderBy(n => n.Name).ToListAsync();
                    ViewBag.Neighborhoods = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(neighborhoods, ""Id"", ""Name"");
                }
                if (project.NeighborhoodId.HasValue) {
                    var streets = await _context.Streets.Where(s => s.NeighborhoodId == project.NeighborhoodId.Value).OrderBy(s => s.Name).ToListAsync();
                    ViewBag.Streets = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(streets, ""Id"", ""Name"");
                }

                var model = new GMK360.Web.Models.CreateProjectWizardViewModel";

        if (code.Contains(oldReturnView))
        {
            code = code.Replace(oldReturnView, newReturnView);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Updated ConstructionProjectController to load locations.");
        }
    }
}
