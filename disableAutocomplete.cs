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

        // Turn off autocomplete on the form
        code = code.Replace(@"<form id=""wizardForm"" enctype=""multipart/form-data"">", 
                            @"<form id=""wizardForm"" enctype=""multipart/form-data"" autocomplete=""off"">");

        // Explicitly turn off autocomplete on Name, Description, Address, etc.
        code = code.Replace(@"asp-for=""Name"" class=""form-control rounded-3""", 
                            @"asp-for=""Name"" class=""form-control rounded-3"" autocomplete=""off""");
                            
        code = code.Replace(@"asp-for=""Address"" class=""form-control rounded-3""", 
                            @"asp-for=""Address"" class=""form-control rounded-3"" autocomplete=""off""");

        code = code.Replace(@"name=""Address"" class=""form-control rounded-3""", 
                            @"name=""Address"" class=""form-control rounded-3"" autocomplete=""off""");
        
        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Autocomplete disabled in Create.cshtml");
    }
}
