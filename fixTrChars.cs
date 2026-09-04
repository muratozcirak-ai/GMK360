using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string content = File.ReadAllText(path, Encoding.UTF8);

        content = content.Replace("Bamsz", "Bağımsız")
                         .Replace("Blmler", "Bölümler")
                         .Replace("Blm", "Bölüm")
                         .Replace("Dzenleme", "Düzenleme")
                         .Replace("Dzenle", "Düzenle")
                         .Replace("Ltfen", "Lütfen")
                         .Replace("iin", "için")
                         .Replace("sein", "seçin")
                         .Replace("Seili", "Seçili")
                         .Replace("zellikleri", "Özellikleri")
                         .Replace("Grnm", "Görünüm")
                         .Replace("Oluturulmu", "Oluşturulmuş")
                         .Replace("oluturulur", "oluşturulur")
                         .Replace("Plan", "Planı")
                         .Replace("Gr", "Gör")
                         .Replace("ablon", "Şablon")
                         .Replace("detayl", "detaylı")
                         .Replace("yaknda", "yakında")
                         .Replace("ynetim arayz", "yönetim arayüzü")
                         .Replace("ekleyebileceiniz", "ekleyebileceğiniz")
                         .Replace("hibir", "hiçbir")
                         .Replace("henz", "henüz")
                         .Replace("rn:", "Örn:")
                         .Replace("Ynetimi", "Yönetimi")
                         .Replace("Detayna Dn", "Detayına Dön")
                         .Replace("baaryla", "başarıyla")
                         .Replace("Snak", "Sığınak")
                         .Replace("Ak", "Açık")
                         .Replace("Dkkan", "Dükkan")
                         .Replace("Jeneratr", "Jeneratör")
                         .Replace("nternet", "İnternet")
                         .Replace("Grntl", "Görüntülü")
                         .Replace("ifreli Giri", "Şifreli Giriş")
                         .Replace("Akll Ev Altyaps", "Akıllı Ev Altyapısı")
                         .Replace("Engelli Rampas", "Engelli Rampası")
                         .Replace("Seiniz...", "Seçiniz...")
                         .Replace("lemler", "İşlemler")
                         .Replace("Silmek istediinize emin misiniz?", "Silmek istediğinize emin misiniz?")
                         .Replace("Ykle", "Yükle")
                         .Replace("Klasr", "Klasör")
                         .Replace("Kat Says", "Kat Sayısı")
                         .Replace("Zemin st", "Zemin Üstü")
                         .Replace("Dier Donanmlar", "Diğer Donanımlar")
                         .Replace("Saysal", "Sayısal")
                         .Replace("aada", "aşağıda");

        File.WriteAllText(path, content, new UTF8Encoding(true));
        Console.WriteLine("Turkish characters fixed in ManageBlock.");
    }
}
