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

        string find = @"StreetId = model\.StreetId,\s*StreetName = ""\-"",";
        string replace = @"StreetId = model.StreetId,";

        code = Regex.Replace(code, find, replace, RegexOptions.Singleline);
        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Removed duplicate StreetName in CreateWizard.");
    }
}
