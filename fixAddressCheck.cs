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

        string oldCheck = @"bool addressExists = await _context\.ConstructionProjects\.AnyAsync\(p =>\s*p\.CityId == model\.CityId &&\s*p\.DistrictId == model\.DistrictId &&\s*p\.NeighborhoodId == model\.NeighborhoodId &&\s*p\.StreetId == model\.StreetId &&\s*p\.Address == model\.Address &&\s*p\.LifecycleStatus != GMK360\.Core\.Entities\.Enums\.ProjectLifecycleStatus\.Demolished &&\s*p\.Id != model\.DraftProjectId\s*\);";
        
        string newCheck = @"
                string addr = (model.Address ?? """").Trim().ToLower();
                bool addressExists = await _context.ConstructionProjects.AnyAsync(p => 
                    p.CityId == model.CityId &&
                    p.DistrictId == model.DistrictId &&
                    p.NeighborhoodId == model.NeighborhoodId &&
                    (p.StreetId == model.StreetId || (p.StreetId == null && model.StreetId == null)) &&
                    p.Address.Trim().ToLower() == addr &&
                    p.LifecycleStatus != GMK360.Core.Entities.Enums.ProjectLifecycleStatus.Demolished &&
                    p.Id != model.DraftProjectId
                );";

        code = Regex.Replace(code, oldCheck, newCheck);
        
        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed address exists check.");
    }
}
