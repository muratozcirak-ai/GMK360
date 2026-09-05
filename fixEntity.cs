using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Construction\ConstructionProject.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string oldBlock = @"public string\? CoverImageUrl \{ get; set; \}";
        string newBlock = @"public string? CoverImageUrl { get; set; }
        public string? CurrentStateImageUrl { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }";

        if(Regex.IsMatch(code, oldBlock))
        {
            code = Regex.Replace(code, oldBlock, newBlock);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Added missing fields to ConstructionProject entity.");
        }
        else
        {
            Console.WriteLine("Could not find the target string.");
        }
    }
}
