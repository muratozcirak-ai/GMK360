using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string text = File.ReadAllText(path);

        string oldMethod = @"private List<GMK360.Core.Entities.UnitSpace> GetDefaultSpacesForLayout(string layout)
        {
            var spaces = new List<GMK360.Core.Entities.UnitSpace>();

            if (layout == ""3+1"")
            {
                spaces.Add(new GMK360.Core.Entities.UnitSpace { Name = ""Salon"", Type = ""YaÅŸam AlanÄ±"" });"; // Just an example of what it might look like, better to regex it.

        var regex = new Regex(@"private List<GMK360\.Core\.Entities\.UnitSpace> GetDefaultSpacesForLayout.*?return spaces;\s*}", RegexOptions.Singleline);

        string newMethod = @"private List<GMK360.Core.Entities.UnitSpace> GetDefaultSpacesForLayout(string layout)
        {
            var spaces = new List<GMK360.Core.Entities.UnitSpace>();
            // SADECE TASLAK - DÜKKAN İÇİ BOŞ (Manuel eklenecek)
            // KULLANICININ İSTEĞİ: Daire şablonu/örnek altyapısı kurulana kadar içleri manuel girilsin
            return spaces;
        }";

        text = regex.Replace(text, newMethod);
        
        // Also fix the text "DÃ¼kkan" in GenerateUnits
        text = text.Replace("DÃ¼kkan", "Dükkan");
        text = text.Replace("SÄ±ÄŸÄ±nak", "Sığınak");
        text = text.Replace("SÄ±ÄŸÄ±nak", "Sığınak"); // In case

        File.WriteAllText(path, text, System.Text.Encoding.UTF8);
    }
}
