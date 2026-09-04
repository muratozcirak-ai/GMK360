using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string content = File.ReadAllText(path, Encoding.UTF8);

        content = content.Replace("ŞŞifreli Girişş", "Şifreli Giriş")
                         .Replace("Açıkıllı", "Akıllı")
                         .Replace("İİnternet", "İnternet")
                         .Replace("Engelli Rampasıı", "Engelli Rampası")
                         .Replace("öÖrn:", "Örn:")
                         .Replace("ÖÖrn:", "Örn:")
                         .Replace("ÖÖzellikleri", "Özellikleri")
                         .Replace("İİşlemler", "İşlemler")
                         .Replace("Görünüm", "Görünüm") // just in case
                         .Replace("GGörünüm", "Görünüm")
                         .Replace("ÇÇatı", "Çatı");

        File.WriteAllText(path, content, new UTF8Encoding(true));
        Console.WriteLine("Reverted over-eager regex replacements.");
    }
}
