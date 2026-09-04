using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\Api\LocationApiController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        if (code.Contains("[Route(\"api/Location\")]"))
        {
            code = code.Replace("[Route(\"api/Location\")]", "[Route(\"api/[controller]\")]");
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Reverted LocationApiController route.");
        }
    }
}
