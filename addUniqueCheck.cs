using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string checkLogic = @"
            // AYNI ADRESTE BİNA/PROJE VAR MI KONTROLÜ (DİJİTAL İKİZ KURALI)
            // Not: İleride doğrudan Building veya Address tablolarından kontrol edilecek.
            bool addressExists = await _context.ConstructionProjects.AnyAsync(p => 
                p.Address == model.Address && 
                p.Name != model.Name // Şimdilik basit Address metni üzerinden kontrol
            );
            
            // Eğer daha hassas bir kontrol isteniyorsa İl, İlçe vs. Building üzerinden yapılabilir
            // Şimdilik test için Address stringi veya Name üzerinden sembolik bir kalkan koyuyoruz
            if (addressExists)
            {
                return Json(new { success = false, message = ""Bu adreste halihazırda kayıtlı bir yapı/proje bulunmaktadır. Aynı adrese ikinci bir bina eklenemez!"" });
            }
";
        if (!code.Contains("Bu adreste halihazırda"))
        {
            int insertIndex = code.IndexOf("var project = new ConstructionProject");
            if (insertIndex != -1)
            {
                code = code.Insert(insertIndex, checkLogic);
                File.WriteAllText(path, code, new UTF8Encoding(true));
                Console.WriteLine("Added Unique Address Check.");
            }
        }
    }
}
