using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string code = File.ReadAllText(path, Encoding.UTF8);

        // Fix CurrentStateImage block
        string oldCurrentState = @"<label class=""form-label fw-bold"">Mevcut Durum Görseli \(İlk Hali\)<\/label>.*?<i class=""ph ph-image me-1""><\/i> Mevcut Görseli Göster<\/a>\s*\}";
        string newCurrentState = @"<label class=""form-label fw-bold"">Mevcut Durum Görseli (İlk Hali)</label>
                                          <div class=""input-group"">
                                              <input asp-for=""CurrentStateImageFile"" type=""file"" class=""form-control bg-light"" accept=""image/jpeg,image/png,application/pdf"" />
                                              @if (!string.IsNullOrEmpty((string)ViewBag.CurrentStateImageUrl))
                                              {
                                                  <a href=""@ViewBag.CurrentStateImageUrl"" target=""_blank"" class=""btn btn-outline-primary""><i class=""ph ph-image""></i> Görseli Aç</a>
                                              }
                                          </div>
                                          <div class=""form-text text-muted"">Şantiyenin/arazinin şu anki hali (JPG/PNG).</div>";
        
        // Use simpler replacement instead of regex over multiple lines with unknown whitespace
        string block1 = @"<label class=""form-label fw-bold"">Mevcut Durum Grseli (lk Hali)</label>
                                          <input asp-for=""CurrentStateImageFile"" type=""file"" class=""form-control rounded-3"" accept=""image/jpeg,image/png,application/pdf"" />
                                          <div class=""form-text text-muted"">antiyenin/arazinin u anki hali (JPG/PNG).</div>
                                            @if (!string.IsNullOrEmpty((string)ViewBag.CurrentStateImageUrl))
                                            {
                                                <a href=""@ViewBag.CurrentStateImageUrl"" target=""_blank"" class=""d-inline-block mt-2 badge bg-primary text-decoration-none p-2""><i class=""ph ph-image me-1""></i> Mevcut Grseli Gster</a>
                                            }";

        // Since encoding causes issues, let's just find the indexes
        
        Console.WriteLine("Done in script, but writing actual logic via exact string logic in C#.");
    }
}
