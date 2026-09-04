using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string text = File.ReadAllText(path, Encoding.UTF8);

        string target = @".Include(p => p.Amenities)";
        string replacement = @".Include(p => p.Amenities)
                .Include(p => p.Blocks)";
        
        if (text.Contains(target))
        {
            text = text.Replace(target, replacement);
            File.WriteAllText(path, text, new UTF8Encoding(true));
            Console.WriteLine("Added Include(p => p.Blocks)");
        }
    }
}
