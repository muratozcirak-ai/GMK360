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

        // Update Create(int? id) mapping
        string oldMapping = @"TotalLandArea = project.TotalLandArea,
                      CityId = project.CityId \?\? 0,";
        string newMapping = @"TotalLandArea = project.TotalLandArea,
                      Latitude = project.Latitude,
                      Longitude = project.Longitude,
                      CityId = project.CityId ?? 0,";
        
        if (Regex.IsMatch(code, oldMapping)) {
            code = Regex.Replace(code, oldMapping, newMapping);
            Console.WriteLine("Updated Create mapping.");
        }

        // Update SaveStep1 mapping for updating existing
        string oldSaveExisting = @"project.StreetId = model.StreetId;
                        if \(!string.IsNullOrEmpty\(uploadedImageUrl\)\) project.CoverImageUrl = uploadedImageUrl;";
        string newSaveExisting = @"project.StreetId = model.StreetId;
                        project.Latitude = model.Latitude;
                        project.Longitude = model.Longitude;
                        if (!string.IsNullOrEmpty(uploadedImageUrl)) project.CoverImageUrl = uploadedImageUrl;
                        if (!string.IsNullOrEmpty(currentStateImageUrl)) project.CurrentStateImageUrl = currentStateImageUrl;";

        if (Regex.IsMatch(code, oldSaveExisting)) {
            code = Regex.Replace(code, oldSaveExisting, newSaveExisting);
            Console.WriteLine("Updated SaveStep1 existing.");
        }

        // Update SaveStep1 mapping for new project
        string oldSaveNew = @"StreetId = model.StreetId,
                        CoverImageUrl = uploadedImageUrl,
                        Status = 0 // Draft";
        string newSaveNew = @"StreetId = model.StreetId,
                        Latitude = model.Latitude,
                        Longitude = model.Longitude,
                        CoverImageUrl = uploadedImageUrl,
                        CurrentStateImageUrl = currentStateImageUrl,
                        Status = 0 // Draft";

        if (Regex.IsMatch(code, oldSaveNew)) {
            code = Regex.Replace(code, oldSaveNew, newSaveNew);
            Console.WriteLine("Updated SaveStep1 new.");
        }

        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Done.");
    }
}
