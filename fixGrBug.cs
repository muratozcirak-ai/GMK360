using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string content = File.ReadAllText(path, Encoding.UTF8);

        content = content.Replace("HasGöroundFloor", "HasGroundFloor");
        content = content.Replace("GörossSquareMeters", "GrossSquareMeters");
        content = content.Replace("Göroup", "Group"); // Just in case Group was also replaced

        File.WriteAllText(path, content, new UTF8Encoding(true));
        Console.WriteLine("Fixed the Gr replacement bug.");
    }
}
