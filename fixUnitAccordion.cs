using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string[] files = { 
            @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageUnit.cshtml",
            @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\TemplateSpaces.cshtml"
        };

        foreach(var path in files)
        {
            if(!File.Exists(path)) continue;
            string text = File.ReadAllText(path);

            var regex = new Regex(@"<button class=""accordion-button(.*?)>(.*?)<\/button>", RegexOptions.Singleline);
            
            text = regex.Replace(text, match => {
                string classNames = match.Groups[1].Value;
                
                string newContent = @"<span class=""w-100 text-truncate fw-bold""><i class=""bi bi-bounding-box-circles me-2 text-secondary""></i> @space.Name <small class=""text-muted fw-normal ms-2"">(@space.Type @if(space.SquareMeters.HasValue){<text>- @space.SquareMeters m²</text>})</small></span>";
                
                return string.Format(@"<button class=""accordion-button{0}"">{1}</button>", classNames, newContent);
            });

            File.WriteAllText(path, text, System.Text.Encoding.UTF8);
        }
    }
}
