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

        // Add pattern to Address
        string oldAddress = @"<input type=""text"" asp-for=""Address"" id=""fullAddress"" class=""form-control rounded-start-3"" placeholder=""Örn: 16"" required />";
        string newAddress = @"<input type=""text"" asp-for=""Address"" id=""fullAddress"" class=""form-control rounded-start-3"" placeholder=""Örn: 16/A"" pattern=""^[0-9]+[A-Za-z0-9/\-]*$"" title=""Kapı numarası bir rakamla başlamalıdır (Örn: 15, 15/A, 12-B)"" required />";
        
        // Since Turkish chars in powershell might fail exact string match for 'Örn: 16', let's use regex
        view = Regex.Replace(view, 
            @"<input type=""text"" asp-for=""Address"" id=""fullAddress"" class=""form-control rounded-start-3"" placeholder="".*?"" required />", 
            newAddress);

        File.WriteAllText(viewPath, view, new UTF8Encoding(true));
        Console.WriteLine("Address pattern added.");
    }
}
