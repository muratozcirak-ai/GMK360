using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Building.cs";
        string text = File.ReadAllText(path, Encoding.UTF8);

        string insert = @"
        public string? InsulationType { get; set; } // Mantolama/Yalıtım
        public int? ElevatorCount { get; set; } // Asansör Sayısı
        public string? ParkingType { get; set; } // Otopark (Açık, Kapalı, Yok)
";

        string target = "public string? TechnicalFeatures { get; set; }";
        
        if(text.Contains(target)) {
            text = text.Replace(target, target + insert);
            File.WriteAllText(path, text, new UTF8Encoding(true));
            Console.WriteLine("Added");
        } else {
            Console.WriteLine("Target not found");
        }
    }
}
