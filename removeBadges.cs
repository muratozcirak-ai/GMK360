using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        string oldFeatures = @"                        <div class=""d-flex flex-wrap gap-1"">
                            @foreach (var f in features)
                            {
                                <span class=""badge bg-light text-dark border""><i class=""bi bi-check2-circle text-success me-1""></i>@f.Trim()</span>
                            }
                        </div>";
                        
        string newFeatures = @"                        <ul class=""list-unstyled mb-0"">
                            @foreach (var f in features)
                            {
                                <li class=""text-dark small mb-1""><i class=""bi bi-check2 text-success me-2""></i>@f.Trim()</li>
                            }
                        </ul>";

        if (text.Contains(oldFeatures))
        {
            text = text.Replace(oldFeatures, newFeatures);
            File.WriteAllText(path, text, new UTF8Encoding(true));
            Console.WriteLine("Badges removed from features");
        }
        else
        {
            Console.WriteLine("Old features block not found");
        }
    }
}
