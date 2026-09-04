using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string viewPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(viewPath, Encoding.UTF8);

        // Find the address input and change it to type="number" strictly.
        string pattern = @"<input type=""text"" asp-for=""Address"" id=""fullAddress"" class=""form-control rounded-start-3"" placeholder=""Örn: 16/A"" pattern="".*?"" title="".*?"" required />";
        string replacement = @"<input type=""number"" asp-for=""Address"" id=""fullAddress"" class=""form-control rounded-start-3"" placeholder=""Örn: 16"" min=""1"" required />";
        
        view = Regex.Replace(view, pattern, replacement);

        // Also check if the old one was still there by any chance
        string oldPattern = @"<input type=""text"" asp-for=""Address"" id=""fullAddress"" class=""form-control rounded-start-3"" placeholder=""Örn: 16"" required />";
        view = Regex.Replace(view, oldPattern, replacement);

        File.WriteAllText(viewPath, view, new UTF8Encoding(true));
        Console.WriteLine("Address input changed to strictly number.");
    }
}
