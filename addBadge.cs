using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string viewPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string view = File.ReadAllText(viewPath, Encoding.UTF8);

        string oldStr = @"<h4 class=""mb-0 text-navy fw-bold"">@Model.BlockName</h4>";
        string newStr = @"<h4 class=""mb-0 text-navy fw-bold"">@Model.BlockName</h4>
                        @if (Model.ParentBuilding != null) { <span class=""badge bg-warning text-dark border ms-3 mt-1""><i class=""bi bi-link-45deg me-1""></i>@Model.ParentBuilding.BlockName Üzerinde (Kule)</span> }";
                        
        view = view.Replace(oldStr, newStr);

        File.WriteAllText(viewPath, view, new UTF8Encoding(true));
        Console.WriteLine("ManageBlock updated.");
    }
}
