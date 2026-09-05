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

        string pattern = @"TotalUnits = 0, \/\/ Removed from block step";
        string replace = @"TotalUnits = b.TotalApartments + b.TotalShops,";
        
        code = Regex.Replace(code, pattern, replace);
        
        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Restored TotalUnits logic in Controller");
    }
}
