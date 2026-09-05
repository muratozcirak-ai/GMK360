using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Data\Contexts\ApplicationDbContext.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        code = code.Replace("GMK360.Core.Entities.System;", "GMK360.Core.Entities.Auditing;");
        
        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed namespace in DbContext.");
    }
}
