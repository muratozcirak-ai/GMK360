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
        string pattern1 = @"<input asp-for=""CurrentStateImageFile"".*?<\/div>\s*@if \(!string\.IsNullOrEmpty\(\(string\)ViewBag\.CurrentStateImageUrl\)\)\s*\{\s*<a href=""@ViewBag\.CurrentStateImageUrl"".*?<\/a>\s*\}";
        string replace1 = @"<div class=""input-group"">
                                              <input asp-for=""CurrentStateImageFile"" type=""file"" class=""form-control bg-light"" accept=""image/jpeg,image/png,application/pdf"" />
                                              @if (!string.IsNullOrEmpty((string)ViewBag.CurrentStateImageUrl))
                                              {
                                                  <a href=""@ViewBag.CurrentStateImageUrl"" target=""_blank"" class=""btn btn-outline-primary""><i class=""ph ph-image""></i> Kayıtlı Görsel</a>
                                              }
                                          </div>
                                          <div class=""form-text text-muted"">Şantiyenin/arazinin şu anki hali (JPG/PNG).</div>";
        code = Regex.Replace(code, pattern1, replace1, RegexOptions.Singleline);

        // Fix CoverImage block
        string pattern2 = @"<input asp-for=""CoverImageFile"".*?<\/div>\s*@if \(!string\.IsNullOrEmpty\(\(string\)ViewBag\.CoverImageUrl\)\)\s*\{\s*<a href=""@ViewBag\.CoverImageUrl"".*?<\/a>\s*\}";
        string replace2 = @"<div class=""input-group"">
                                              <input asp-for=""CoverImageFile"" type=""file"" class=""form-control bg-light"" accept=""image/jpeg,image/png,application/pdf"" />
                                              @if (!string.IsNullOrEmpty((string)ViewBag.CoverImageUrl))
                                              {
                                                  <a href=""@ViewBag.CoverImageUrl"" target=""_blank"" class=""btn btn-outline-primary""><i class=""ph ph-image""></i> Kayıtlı Görsel</a>
                                              }
                                          </div>
                                          <div class=""form-text text-muted"">Mimari 3D render görselini seçin (JPG/PNG).</div>";
        code = Regex.Replace(code, pattern2, replace2, RegexOptions.Singleline);

        // Fix Validation issue on Lat/Lng by removing asp-for and using name and value manually so jQuery val doesn't block it
        string pattern3 = @"<input type=""text"" asp-for=""Latitude"".*?\/>";
        string replace3 = @"<input type=""text"" name=""Latitude"" id=""latInput"" class=""form-control bg-light"" placeholder=""Enlem"" value=""@Model.Latitude"" readonly />";
        code = Regex.Replace(code, pattern3, replace3, RegexOptions.Singleline);

        string pattern4 = @"<input type=""text"" asp-for=""Longitude"".*?\/>";
        string replace4 = @"<input type=""text"" name=""Longitude"" id=""lngInput"" class=""form-control bg-light"" placeholder=""Boylam"" value=""@Model.Longitude"" readonly />";
        code = Regex.Replace(code, pattern4, replace4, RegexOptions.Singleline);

        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("UI Fixes Applied.");
    }
}
