using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Models\CreateProjectWizardViewModel.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string badProps = @"        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
        public int? NeighborhoodId { get; set; }
        public int? StreetId { get; set; }";

        if (code.Contains(badProps))
        {
            code = code.Replace(badProps, "");
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Fixed ViewModel duplicates.");
        }
    }
}
