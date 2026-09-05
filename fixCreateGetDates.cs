using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string controllerPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string controllerCode = File.ReadAllText(controllerPath, Encoding.UTF8);

        string createGetFind = @"model\.DraftProjectId = draft\.Id;\s*model\.Name = draft\.Name;\s*model\.Description = draft\.Description;\s*model\.Address = draft\.Address;\s*model\.TotalLandArea = draft\.TotalLandArea;\s*model\.TargetTotalApartments = draft\.TargetTotalApartments;\s*model\.TargetTotalShops = draft\.TargetTotalShops;\s*model\.Latitude = draft\.Latitude;\s*model\.Longitude = draft\.Longitude;\s*model\.CityId = draft\.CityId \?\? 0;";

        string createGetReplace = @"model.DraftProjectId = draft.Id;
                    model.Name = draft.Name;
                    model.Description = draft.Description;
                    model.Address = draft.Address;
                    model.StartDate = draft.StartDate;
                    model.EndDate = draft.EndDate;
                    ViewBag.CoverImageUrl = draft.CoverImageUrl;
                    
                    var currentStateDoc = _context.DmsDocuments.FirstOrDefault(d => d.EntityType == ""ConstructionProject"" && d.EntityId == draft.Id && d.Title == ""Mevcut Durum Görseli (İlk Hali)"");
                    if (currentStateDoc != null) {
                        ViewBag.CurrentStateImageUrl = currentStateDoc.DocumentUrl;
                    }

                    model.TotalLandArea = draft.TotalLandArea;
                    model.TargetTotalApartments = draft.TargetTotalApartments;
                    model.TargetTotalShops = draft.TargetTotalShops;
                    model.Latitude = draft.Latitude;
                    model.Longitude = draft.Longitude;
                    model.CityId = draft.CityId ?? 0;";

        if (Regex.IsMatch(controllerCode, createGetFind, RegexOptions.Singleline))
        {
            controllerCode = Regex.Replace(controllerCode, createGetFind, createGetReplace, RegexOptions.Singleline);
            File.WriteAllText(controllerPath, controllerCode, new UTF8Encoding(true));
            Console.WriteLine("Create GET updated with Dates and Images.");
        }
        else
        {
            Console.WriteLine("Create GET NOT FOUND.");
        }
    }
}
