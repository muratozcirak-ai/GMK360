using System;
using System.IO;
using System.Text;
using System.Linq;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        // Find the description div in the sidebar to insert TechnicalFeatures right before it
        string target = @"                <div>
                    <span class=""text-muted small d-block"">Açıklama</span>
                    <span class=""text-dark small"">@(string.IsNullOrEmpty(Model.Description) ? ""Belirtilmedi"" : Model.Description)</span>
                </div>";
                
        if (text.Contains(target))
        {
            string featuresUi = @"
                <div class=""mb-3"">
                    <span class=""text-muted small d-block mb-1"">Diğer Donanımlar ve Notlar</span>
                    @if (!string.IsNullOrEmpty(Model.TechnicalFeatures))
                    {
                        var features = Model.TechnicalFeatures.Split(new[] { "","" }, StringSplitOptions.RemoveEmptyEntries);
                        <div class=""d-flex flex-wrap gap-1"">
                            @foreach (var f in features)
                            {
                                <span class=""badge bg-light text-dark border""><i class=""bi bi-check2-circle text-success me-1""></i>@f.Trim()</span>
                            }
                        </div>
                    }
                    else
                    {
                        <span class=""text-dark small"">Belirtilmedi</span>
                    }
                </div>";
                
            text = text.Replace(target, featuresUi + "\n" + target);
            File.WriteAllText(path, text, new UTF8Encoding(true));
            Console.WriteLine("Sidebar Features Added");
        }
        else
        {
            Console.WriteLine("Target not found for sidebar features.");
        }
    }
}
