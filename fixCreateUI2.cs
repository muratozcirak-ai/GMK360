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

        string oldMap = @"<button class=""btn btn-outline-secondary"" type=""button"" onclick=""openMapModal\(\)"">.*?Haritadan Se.*?<\/button>";
        string newMap = @"<button class=""btn btn-outline-secondary"" type=""button"" onclick=""openMapModal()""><i class=""ph ph-map-pin""></i> Haritadan Seç</button>
                                      @if (Model.Latitude.HasValue && Model.Longitude.HasValue)
                                      {
                                          <a href=""https://maps.google.com/?q=@Model.Latitude,@Model.Longitude"" target=""_blank"" class=""btn btn-outline-primary""><i class=""ph ph-map-trifold""></i> Haritada Göster</a>
                                      }";
        if (Regex.IsMatch(code, oldMap)) { code = Regex.Replace(code, oldMap, newMap); Console.WriteLine("Added map button."); }

        string oldCurrentState = @"<div class=""form-text text-muted"">.*?anki hali \(JPG\/PNG\)\.<\/div>";
        string newCurrentState = @"<div class=""form-text text-muted"">Şantiyenin/arazinin şu anki hali (JPG/PNG).</div>
                                          @if (!string.IsNullOrEmpty((string)ViewBag.CurrentStateImageUrl))
                                          {
                                              <a href=""@ViewBag.CurrentStateImageUrl"" target=""_blank"" class=""d-inline-block mt-2 badge bg-primary text-decoration-none p-2""><i class=""ph ph-image me-1""></i> Mevcut Görseli Göster</a>
                                          }";
        if (Regex.IsMatch(code, oldCurrentState)) { code = Regex.Replace(code, oldCurrentState, newCurrentState); Console.WriteLine("Added current state image button."); }

        string oldCover = @"<div class=""form-text text-muted"">Mimari 3D render g.*?rselini se.*?in \(JPG\/PNG\)\.<\/div>";
        string newCover = @"<div class=""form-text text-muted"">Mimari 3D render görselini seçin (JPG/PNG).</div>
                                          @if (!string.IsNullOrEmpty((string)ViewBag.CoverImageUrl))
                                          {
                                              <a href=""@ViewBag.CoverImageUrl"" target=""_blank"" class=""d-inline-block mt-2 badge bg-primary text-decoration-none p-2""><i class=""ph ph-image me-1""></i> Mevcut Görseli Göster</a>
                                          }";
        if (Regex.IsMatch(code, oldCover)) { code = Regex.Replace(code, oldCover, newCover); Console.WriteLine("Added cover image button."); }

        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Done.");
    }
}
