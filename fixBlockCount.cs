using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string oldCount = @"<input type=""number"" id=""blockCount"" class=""form-control form-control-lg rounded-3"" min=""1"" max=""20"" value=""1"" />";
        string newCount = @"<input type=""number"" id=""blockCount"" class=""form-control form-control-lg rounded-3"" min=""1"" max=""20"" value=""@(Model.Blocks != null && Model.Blocks.Any() ? Model.Blocks.Count : 1)"" />";
        
        code = code.Replace(oldCount, newCount);
        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed blockCount.");
    }
}
