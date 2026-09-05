using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string oldLine = @"var agencyId = HttpContext.Session.GetInt32(""CurrentAgencyId"") ?? 1;";
        string newLine = @"var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Json(new { success = false, message = ""Yetkisiz erişim. Oturumunuz kapanmış olabilir."" });";

        if (code.Contains(oldLine))
        {
            code = code.Replace(oldLine, newLine);
            
            // Also need to use agencyId.Value when assigning to project
            code = code.Replace("AgencyId = agencyId,", "AgencyId = agencyId.Value,");
            
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Fixed AgencyId crash.");
        }
    }
}
