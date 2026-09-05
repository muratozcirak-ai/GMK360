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

        if (!code.Contains("TargetTotalApartments"))
        {
            code = code.Replace("public virtual ICollection<ProjectAssignment> Assignments { get; set; } = new List<ProjectAssignment>();", 
                "public virtual ICollection<ProjectAssignment> Assignments { get; set; } = new List<ProjectAssignment>();\r\n\r\n        public int? TargetTotalApartments { get; set; }\r\n        public int? TargetTotalShops { get; set; }");
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Added TargetTotalApartments");
        }
    }
}
