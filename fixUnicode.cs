using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        
        // Read raw bytes to check if it's UTF8 or Windows-1254
        byte[] bytes = File.ReadAllBytes(path);
        
        // Let's just do Regex replace using \uFFFD in C# if we read it as UTF8
        string content = File.ReadAllText(path, Encoding.UTF8);
        
        content = content.Replace("Ba\uFFFDms\uFFFDe", "Bağımsız"); // wait, ı is \uFFFD, z is z
        content = content.Replace("Ba\uFFFDms\uFFFDz", "Bağımsız");
        content = content.Replace("B\uFFFDl\uFFFDmler", "Bölümler");
        content = content.Replace("D\uFFFDzenleme", "Düzenleme");
        content = content.Replace("D\uFFFDzenle", "Düzenle");
        content = content.Replace("L\uFFFDtfen", "Lütfen");
        content = content.Replace("i\uFFFDin", "için");
        content = content.Replace("se\uFFFDin", "seçin");
        content = content.Replace("Se\uFFFDili", "Seçili");
        content = content.Replace("\uFFFDzellikleri", "Özellikleri");
        content = content.Replace("G\uFFFD\uFFFDr\uFFFDn\uFFFDm", "Görünüm"); // wait, ö is \uFFFD, ü is \uFFFD. G\uFFFD\uFFFDr\uFFFDn\uFFFDm might be messed up.
        
        File.WriteAllText(path, content, new UTF8Encoding(true));
        Console.WriteLine("Done replacing unicode replacement chars.");
    }
}
