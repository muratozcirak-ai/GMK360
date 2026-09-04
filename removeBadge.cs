using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string viewPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string view = File.ReadAllText(viewPath, Encoding.UTF8);

        // Remove the badge
        string pattern = @"<span class=""badge bg-light text-dark border rounded-pill px-3 py-2"">Toplam @\(Model\.Units\?\.Count \?\? 0\) Adet</span>";
        view = Regex.Replace(view, pattern, "");

        File.WriteAllText(viewPath, view, new UTF8Encoding(true));
        Console.WriteLine("Badge removed.");
    }
}
