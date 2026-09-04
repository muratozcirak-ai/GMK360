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

            // Let's replace the whole accordion-button content with a simpler span.
            var regex = new Regex(@"<button class=""accordion-button.*?<span class=""text-truncate.*?<\/span>.*?<\/button>", RegexOptions.Singleline);
            
            // We will just read the file line by line and find the accordion button to manually replace it if regex is risky,
            // but regex is fine if we are careful. Actually, it's safer to just replace the div flex classes:
            
            text = text.Replace(@"<div class=""d-flex w-100 align-items-center pe-5"">", @"<span class=""w-100"">");
            text = text.Replace(@"<div class=""text-truncate me-auto fw-bold"">", @"<span class=""fw-bold me-2"">");
            text = text.Replace(@"<div class=""flex-shrink-0 text-end"">", @"<span class=""text-muted small""> (");
            text = text.Replace(@"<span class=""badge bg-white text-dark border me-2"">", @"");
            text = text.Replace(@"</span>
                                                      @if(space.SquareMeters.HasValue)", @" - @if(space.SquareMeters.HasValue)");
            text = text.Replace(@"<span class=""badge bg-info bg-opacity-10 text-info border border-info"">", @" ");
            text = text.Replace(@"m²</span>", @"m²");
            text = text.Replace(@"</div>
                                         </div>", @")</span></span>");
                                         
            // The previous fixUI.cs added these tags. Let's make sure it matches.
            // If it doesn't match perfectly, it might break. Let's just write a C# script to do it very carefully.
        }
    }
}
