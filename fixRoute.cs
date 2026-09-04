using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\Api\LocationApiController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        if (code.Contains("[Route(\"api/[controller]\")]"))
        {
            code = code.Replace("[Route(\"api/[controller]\")]", "[Route(\"api/Location\")]");
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Replaced api/[controller] with api/Location.");
        }
        else
        {
            Console.WriteLine("Did not find api/[controller].");
        }
    }
}
