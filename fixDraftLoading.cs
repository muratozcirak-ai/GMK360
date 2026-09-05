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

        string find = @"public async Task<IActionResult> Create\(\) \{.*?return View\(new GMK360\.Web\.Models\.CreateProjectWizardViewModel\(\)\); \}";
        
        string replace = @"public async Task<IActionResult> Create(int? projectId = null) { 
            var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync(); 
            ViewBag.Cities = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(cities, ""Id"", ""Name""); 
            ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>()); 
            ViewBag.Neighborhoods = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>()); 
            ViewBag.Streets = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new System.Collections.Generic.List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>()); 
            
            var model = new GMK360.Web.Models.CreateProjectWizardViewModel();

            if (projectId.HasValue) {
                var draft = await _context.ConstructionProjects.Include(p => p.Blocks).FirstOrDefaultAsync(p => p.Id == projectId.Value);
                if (draft != null) {
                    model.DraftProjectId = draft.Id;
                    model.Name = draft.Name;
                    model.Description = draft.Description;
                    model.Address = draft.Address;
                    model.TotalLandArea = draft.TotalLandArea;
                    model.TargetTotalApartments = draft.TargetTotalApartments;
                    model.TargetTotalShops = draft.TargetTotalShops;
                    model.Latitude = draft.Latitude;
                    model.Longitude = draft.Longitude;
                    model.CityId = draft.CityId;
                    model.DistrictId = draft.DistrictId;
                    model.NeighborhoodId = draft.NeighborhoodId;
                    model.StreetId = draft.StreetId;

                    if (draft.Blocks != null && draft.Blocks.Any()) {
                        model.Blocks = draft.Blocks.Select(b => new GMK360.Web.Models.WizardBlockViewModel {
                            BlockName = b.BlockName,
                            BaseArea = b.BaseArea,
                            TotalFloors = b.TotalFloors,
                            BasementFloors = b.BasementFloors,
                            TotalApartments = b.TotalApartments,
                            TotalShops = b.TotalShops,
                            HasGroundFloor = b.HasGroundFloor,
                            HasRoof = b.HasRoof
                        }).ToList();
                    }

                    if (draft.CityId.HasValue) {
                        var districts = await _context.Districts.Where(d => d.CityId == draft.CityId).ToListAsync();
                        ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(districts, ""Id"", ""Name"", draft.DistrictId);
                    }
                    if (draft.DistrictId.HasValue) {
                        var hoods = await _context.Neighborhoods.Where(n => n.DistrictId == draft.DistrictId).ToListAsync();
                        ViewBag.Neighborhoods = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(hoods, ""Id"", ""Name"", draft.NeighborhoodId);
                    }
                    if (draft.NeighborhoodId.HasValue) {
                        var streets = await _context.Streets.Where(s => s.NeighborhoodId == draft.NeighborhoodId).ToListAsync();
                        ViewBag.Streets = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(streets, ""Id"", ""Name"", draft.StreetId);
                    }
                }
            }

            return View(model); 
        }";

        code = Regex.Replace(code, find, replace, RegexOptions.Singleline);
        
        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Added draft loading to Create GET method.");
    }
}
