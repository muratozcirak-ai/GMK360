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

        // Fix modal header
        string pattern1 = @"<h5 class=""modal-title fw-bold""><i class=""bi bi-building-up text-success me-2""><\/i>Bina .*?skeletini G.*?ncelle<\/h5>";
        string replace1 = @"<h5 class=""modal-title fw-bold""><i class=""bi bi-building-up text-success me-2""></i>Bina İskeletini Güncelle</h5>";
        code = Regex.Replace(code, pattern1, replace1);

        // Fix alert warning text
        string pattern2 = @"<div class=""alert alert-warning py-2 small mb-4"">.*?<\/div>";
        string replace2 = @"<div class=""alert alert-warning py-2 small mb-4"">
                          <i class=""bi bi-exclamation-triangle me-1""></i> Kat sayılarını değiştirdiğinizde binanın kat yapısı yeniden çizilir. Mevcut daireler silinmez ancak tanımlanan kat aralığı dışına çıkan katlar (örn: 9'dan 8'e düşürülürse 9. kat) listede görünmez.
                      </div>";
        code = Regex.Replace(code, pattern2, replace2, RegexOptions.Singleline);

        // Fix Normal Kat Sayısı label
        string pattern3 = @"<label class=""form-label fw-bold small"">Normal Kat Say.*?s.*?<\/label>";
        string replace3 = @"<label class=""form-label fw-bold small"">Normal Kat Sayısı</label>";
        code = Regex.Replace(code, pattern3, replace3);

        // Fix Bodrum Kat Sayısı label
        string pattern4 = @"<label class=""form-label fw-bold small"">Bodrum Kat Say.*?s.*?<\/label>";
        string replace4 = @"<label class=""form-label fw-bold small"">Bodrum Kat Sayısı</label>";
        code = Regex.Replace(code, pattern4, replace4);

        // Fix Çatı Katı Var label
        string pattern5 = @"<label class=""form-check-label fw-bold small"" for=""hasRoofSwitch"">.*?at.*? Kat.*? Var<\/label>";
        string replace5 = @"<label class=""form-check-label fw-bold small"" for=""hasRoofSwitch"">Çatı Katı Var</label>";
        code = Regex.Replace(code, pattern5, replace5);

        // Fix İptal button
        string pattern6 = @"<button type=""button"" class=""btn btn-light rounded-pill px-4"" data-bs-dismiss=""modal"">.*?ptal<\/button>";
        string replace6 = @"<button type=""button"" class=""btn btn-light rounded-pill px-4"" data-bs-dismiss=""modal"">İptal</button>";
        code = Regex.Replace(code, pattern6, replace6);

        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed Turkish characters in modal.");
    }
}
