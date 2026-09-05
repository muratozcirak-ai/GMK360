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

        string find = @"BlockName = b\.BlockName,\s*BuildingNumber = blockCounter\.ToString\(\),";
        string replace = @"BlockName = b.BlockName,
                            BuildingNumber = blockCounter.ToString(),
                            StreetName = ""Belirtilmedi"",";

        if (Regex.IsMatch(code, find, RegexOptions.Singleline))
        {
            code = Regex.Replace(code, find, replace, RegexOptions.Singleline);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Fixed StreetName in SaveStep2.");
        }
        else
        {
            Console.WriteLine("Could not find Building creation in SaveStep2.");
        }
    }
}
