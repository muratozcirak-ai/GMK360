using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string text = File.ReadAllText(path);

        var regex = new Regex(@"<button class=""accordion-button(.*?)>(.*?)<\/button>", RegexOptions.Singleline);
        
        text = regex.Replace(text, match => {
            string classNames = match.Groups[1].Value;
            string newContent = @"<span class=""w-100 text-truncate fw-bold""><i class=""bi bi-building-up me-2 text-primary""></i> @floorName</span>";
            return string.Format(@"<button class=""accordion-button{0}"">{1}</button>", classNames, newContent);
        });

        File.WriteAllText(path, text, System.Text.Encoding.UTF8);
    }
}
