using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string oldMapping = @"                    EndDate = project.EndDate ?? DateTime.Now.AddYears(1),
                    TotalLandArea = project.TotalLandArea,
                    Blocks = project.Blocks.Select(b => new GMK360.Web.Models.WizardBlockItem";

        string newMapping = @"                    EndDate = project.EndDate ?? DateTime.Now.AddYears(1),
                    TotalLandArea = project.TotalLandArea,
                    CityId = project.CityId ?? 0,
                    DistrictId = project.DistrictId ?? 0,
                    NeighborhoodId = project.NeighborhoodId ?? 0,
                    StreetId = project.StreetId,
                    Blocks = project.Blocks.Select(b => new GMK360.Web.Models.WizardBlockItem";

        if (code.Contains(oldMapping))
        {
            code = code.Replace(oldMapping, newMapping);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Fixed mapping in Create.");
        }
    }
}
