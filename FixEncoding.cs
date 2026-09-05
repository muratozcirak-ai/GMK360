using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string code = File.ReadAllText(path, Encoding.UTF8);

        // We can't rely on the corrupted characters in our regex because C# might interpret them differently.
        // We'll replace the exact lines containing the corrupted text using contextual keywords.

        // Fix title
        string oldTitle = @"<h5 class=""fw-bold mb-0""><i class=""bi bi-diagram-3 text-navy me-2""><\/i>Ba.*?B.*?l.*?mler ve Toplu D.*?zenleme<\/h5>";
        string newTitle = @"<h5 class=""fw-bold mb-0""><i class=""bi bi-diagram-3 text-navy me-2""></i>Bağımsız Bölümler ve Toplu Düzenleme</h5>";
        code = Regex.Replace(code, oldTitle, newTitle);

        // Fix green button 1
        string oldBtn1 = @"<i class=""bi bi-building-up me-1""><\/i> Bina .*?skeletini G.*?ncelle \(Kat Ekle\/.*?kar\)";
        string newBtn1 = @"<i class=""bi bi-building-up me-1""></i> Bina İskeletini Güncelle (Kat Ekle/Çıkar)";
        code = Regex.Replace(code, oldBtn1, newBtn1);

        // Fix green button 2 (modal)
        string oldBtn2 = @"<button type=""submit"" class=""btn btn-success rounded-pill px-4"">.*?skeleti G.*?ncelle<\/button>";
        string newBtn2 = @"<button type=""submit"" class=""btn btn-success rounded-pill px-4"">İskeleti Güncelle</button>";
        code = Regex.Replace(code, oldBtn2, newBtn2);

        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed encoding issues.");
    }
}
