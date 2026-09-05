using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        // Fix CS0266
        code = code.Replace("EndDate = project.EndDate,", "EndDate = project.EndDate ?? DateTime.Now.AddYears(1),");

        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed EndDate.");
    }
}
