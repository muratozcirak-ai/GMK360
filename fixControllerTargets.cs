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

        // Update Create GET mapping
        string pattern1 = @"Latitude = project\.Latitude,\s*Longitude = project\.Longitude,";
        string replace1 = @"Latitude = project.Latitude,
                      Longitude = project.Longitude,
                      TargetTotalApartments = project.TargetTotalApartments,
                      TargetTotalShops = project.TargetTotalShops,";
        if (!code.Contains("TargetTotalApartments = project.TargetTotalApartments"))
            code = Regex.Replace(code, pattern1, replace1);

        // Update SaveStep1 mapping for existing
        string pattern2 = @"project\.Latitude = model\.Latitude;\s*project\.Longitude = model\.Longitude;";
        string replace2 = @"project.Latitude = model.Latitude;
                        project.Longitude = model.Longitude;
                        project.TargetTotalApartments = model.TargetTotalApartments;
                        project.TargetTotalShops = model.TargetTotalShops;";
        if (!code.Contains("project.TargetTotalApartments = model.TargetTotalApartments"))
            code = Regex.Replace(code, pattern2, replace2);

        // Update SaveStep1 mapping for new
        string pattern3 = @"Latitude = model\.Latitude,\s*Longitude = model\.Longitude,";
        string replace3 = @"Latitude = model.Latitude,
                        Longitude = model.Longitude,
                        TargetTotalApartments = model.TargetTotalApartments,
                        TargetTotalShops = model.TargetTotalShops,";
        if (!code.Contains("TargetTotalApartments = model.TargetTotalApartments"))
            code = Regex.Replace(code, pattern3, replace3);

        // Update SaveStep2 mapping to use project targets if blocks don't have them
        // Actually, if we remove them from block, we should assign total units differently or just leave it for now.
        // We will remove block.TotalUnits assignment from step 2 if they are not in WizardBlockItem anymore.
        string pattern4 = @"TotalUnits = b\.TotalApartments \+ b\.TotalShops,";
        string replace4 = @"TotalUnits = 0, // Removed from block step";
        code = Regex.Replace(code, pattern4, replace4);

        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Updated ConstructionProjectController.cs");
    }
}
