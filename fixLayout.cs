using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ConstructionLayout.cshtml";
        string view = File.ReadAllText(path, Encoding.UTF8);

        string oldCode = "@RenderSection(\"Scripts\", required: false)";
        string newCode = "@section Scripts {\n    @RenderSection(\"Scripts\", required: false)\n}";

        if (view.Contains(oldCode))
        {
            view = view.Replace(oldCode, newCode);
            File.WriteAllText(path, view, new UTF8Encoding(true));
            Console.WriteLine("Fixed nested Scripts section.");
        }
        else
        {
            Console.WriteLine("Could not find @RenderSection(\"Scripts\"...).");
        }
    }
}
