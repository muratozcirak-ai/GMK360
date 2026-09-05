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

        // Fix the placeholder and button text
        string pattern1 = @"<input type=""text"" name=""PhysicalLocationNote"" class=""form-control form-control-sm"" placeholder=""Fiziksel Konum .*?"" style=""max-width:200px;"" \/>";
        string replace1 = @"<input type=""text"" name=""PhysicalLocationNote"" class=""form-control form-control-sm"" placeholder=""Orijinal Evrak Yeri (Örn: Arşiv)"" style=""max-width:200px;"" title=""Orijinal kat planının fiziki olarak nerede saklandığını yazın"" />";
        code = Regex.Replace(code, pattern1, replace1);

        string pattern2 = @"<button type=""submit"" class=""btn btn-primary""><i class=""bi bi-upload""><\/i> Y.*?kle<\/button>";
        string replace2 = @"<button type=""submit"" class=""btn btn-primary""><i class=""bi bi-upload""></i> Kat Planı Yükle</button>";
        code = Regex.Replace(code, pattern2, replace2);

        // Also fix the Planı Gör button
        string pattern3 = @"<a href=""@planDoc\.DocumentUrl"" target=""_blank"" class=""btn btn-sm btn-success rounded-pill me-2""><i class=""bi bi-file-earmark-image me-1""><\/i> Plan.*? G.*?r<\/a>";
        string replace3 = @"<a href=""@planDoc.DocumentUrl"" target=""_blank"" class=""btn btn-sm btn-success rounded-pill me-2""><i class=""bi bi-file-earmark-image me-1""></i> Kat Planını Gör</a>";
        code = Regex.Replace(code, pattern3, replace3);

        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed texts for physical location and plan upload.");
    }
}
