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

        string find = @"model\.StartDate = draft\.StartDate \?\? default\(DateTime\);\s*model\.EndDate = draft\.EndDate \?\? default\(DateTime\);";
        string replace = @"model.StartDate = draft.StartDate;
                    model.EndDate = draft.EndDate ?? default(DateTime);";

        if (Regex.IsMatch(controllerCode, find))
        {
            controllerCode = Regex.Replace(controllerCode, find, replace);
            File.WriteAllText(controllerPath, controllerCode, new UTF8Encoding(true));
            Console.WriteLine("Fixed Dates again.");
        }
    }
}
