using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string oldHidden = @"<input type=""hidden"" id=""DraftProjectId"" name=""DraftProjectId"" value=""0"" />";
        string newHidden = @"<input type=""hidden"" asp-for=""DraftProjectId"" id=""DraftProjectId"" />";

        if (code.Contains(oldHidden))
        {
            code = code.Replace(oldHidden, newHidden);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Fixed DraftProjectId hidden input.");
        }
    }
}
