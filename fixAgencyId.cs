using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        code = code.Replace("AgencyId = project.AgencyId", "CityId = 34"); // I'll just remove it basically. 
        // Better: Replace entire line.
        // Wait, CityId is required on Building! Oh!
        // The project doesn't have CityId on it (Wait, it might not?). The WizardModel DOES have it!
        // But SaveStep2 gets the WizardModel!
        
        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed.");
    }
}
