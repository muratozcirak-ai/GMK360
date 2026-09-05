using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Construction\ConstructionProject.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string insertPoint = "public string Address { get; set; }";
        if (code.Contains(insertPoint) && !code.Contains("public int? CityId"))
        {
            string props = @"public string Address { get; set; }
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
        public int? NeighborhoodId { get; set; }
        public int? StreetId { get; set; }";
            code = code.Replace(insertPoint, props);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Added address fields to ConstructionProject.");
        }
    }
}
