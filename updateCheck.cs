using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string oldCheck = @"                bool addressExists = await _context.ConstructionProjects.AnyAsync(p => 
                    p.Address == model.Address && 
                    p.Name != model.Name 
                );";
                
        string newCheck = @"                bool addressExists = await _context.ConstructionProjects.AnyAsync(p => 
                    p.CityId == model.CityId &&
                    p.DistrictId == model.DistrictId &&
                    p.NeighborhoodId == model.NeighborhoodId &&
                    p.StreetId == model.StreetId &&
                    p.Address == model.Address &&
                    p.Id != model.DraftProjectId
                );";

        if (code.Contains(oldCheck))
        {
            code = code.Replace(oldCheck, newCheck);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Updated uniqueness check.");
        }
    }
}
