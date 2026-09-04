using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        text = text.Replace("BrÃ¼t", "Brüt");
        text = text.Replace("mÂ²", "m²");
        text = text.Replace("Ã–rn:", "Örn:");
        text = text.Replace("GÃ¼ney", "Güney");
        text = text.Replace("Ã–zellikl", "Özellikl");
        text = text.Replace("DÃ¼zenle", "Düzenle");
        text = text.Replace("BatÄ±", "Batı");
        text = text.Replace("Ã–zellikler", "Özellikler");
        text = text.Replace("AsansÃ¶r", "Asansör");
        text = text.Replace("YÃ¼k", "Yük");
        text = text.Replace("AsansÃ¶rÃ¼", "Asansörü");
        text = text.Replace("Ã‡ift", "Çift");
        text = text.Replace("AÃ§Ä±klama", "Açıklama");
        text = text.Replace("AdÄ±", "Adı");
        text = text.Replace("BaÄŸÄ±msÄ±z", "Bağımsız");

        File.WriteAllText(path, text, new UTF8Encoding(true)); // Save with BOM
    }
}
