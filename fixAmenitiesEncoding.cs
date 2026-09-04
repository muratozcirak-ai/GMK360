using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Amenities.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        // Replace bad encoding
        text = text.Replace("ÅŸ", "ş").Replace("Åž", "Ş");
        text = text.Replace("Ä±", "ı").Replace("Ä°", "İ");
        text = text.Replace("Ã§", "ç").Replace("Ã‡", "Ç");
        text = text.Replace("Ã¶", "ö").Replace("Ã–", "Ö");
        text = text.Replace("Ã¼", "ü").Replace("Ãœ", "Ü");
        text = text.Replace("ÄŸ", "ğ").Replace("Äž", "Ğ");
        text = text.Replace("mÂ²", "m²");
        
        File.WriteAllText(path, text, new UTF8Encoding(true));
    }
}
