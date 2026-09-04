using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Details.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        // Find the "Çevre Özellikleri" div content
        string targetStart = @"<div class=""row g-3"">
                        <div class=""col-sm-6"">
                            <div class=""d-flex align-items-center p-2 rounded bg-light"">
                                <i class=""bi bi-hospital text-danger fs-3 me-3""></i>";
                                
        string targetEnd = @"<div class=""text-end mt-2"">
                        <span class=""text-muted fs-8 fst-italic"">* Konumlar Google Haritalar / Yapay Zeka servisi ile otomatik çekilmektedir.</span>
                    </div>";

        int startIdx = text.IndexOf(@"<div class=""row g-3"">", text.IndexOf("Çevre Özellikleri ve Yakın Konumlar"));
        int endIdx = text.IndexOf("</div>", text.IndexOf(targetEnd)) + 6;

        if (startIdx != -1 && endIdx != -1 && endIdx > startIdx)
        {
            string newContent = @"
                    <div class=""alert alert-secondary d-flex align-items-center p-4 mb-0"">
                        <i class=""bi bi-geo-alt fs-2x text-muted me-4""></i>
                        <div class=""d-flex flex-column"">
                            <h4 class=""mb-1 text-dark"">Konum Verileri Bekleniyor</h4>
                            <span class=""text-muted"">Sistem canlıya alındığında Google Haritalar (Places API) üzerinden hastane, okul, park gibi çevre özellikleri buraya otomatik çekilecektir.</span>
                        </div>
                    </div>";
            
            text = text.Remove(startIdx, endIdx - startIdx);
            text = text.Insert(startIdx, newContent);
            
            File.WriteAllText(path, text, new UTF8Encoding(true));
            Console.WriteLine("Replaced with empty state");
        }
        else
        {
            Console.WriteLine("Could not find the target block.");
        }
    }
}
