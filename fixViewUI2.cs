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

        // Remove from Javascript generateBlocks
        string patternJS1 = @"<div class=""col-12 col-lg-2"">\s*<label class=""form-label small fw-bold"">Daire Say.*?s.*?<\/label>\s*<input type=""number"" name=""Blocks\[\$\{i\}\]\.TotalApartments"".*?<\/div>";
        string patternJS2 = @"<div class=""col-12 col-lg-2"">\s*<label class=""form-label small fw-bold"">D.*?kkan Say.*?s.*?<\/label>\s*<input type=""number"" name=""Blocks\[\$\{i\}\]\.TotalShops"".*?<\/div>";
        
        code = Regex.Replace(code, patternJS1, "", RegexOptions.Singleline);
        code = Regex.Replace(code, patternJS2, "", RegexOptions.Singleline);

        // Replace TotalLandArea block with 3 columns (LandArea, Apartments, Shops)
        string patternStep1 = @"<div class=""col-md-6"">\s*<label class=""form-label fw-bold"">Toplam Arazi Alan.*?\s*<\/div>\s*<\/div>\s*<div class=""col-md-6"">\s*<\/div>";
        string replaceStep1 = @"<div class=""col-md-4"">
                                          <label class=""form-label fw-bold"">Toplam Arazi Alanı (m²)</label>
                                          <div class=""input-group"">
                                              <input type=""number"" asp-for=""TotalLandArea"" class=""form-control rounded-start-3"" placeholder=""Örn: 2500"" min=""1"" required />
                                              <span class=""input-group-text rounded-end-3"">m²</span>
                                          </div>
                                      </div>
                                      <div class=""col-md-4"">
                                          <label class=""form-label fw-bold"">Tahmini Toplam Daire</label>
                                          <input type=""number"" asp-for=""TargetTotalApartments"" class=""form-control rounded-3"" placeholder=""Örn: 40"" min=""0"" required />
                                      </div>
                                      <div class=""col-md-4"">
                                          <label class=""form-label fw-bold"">Tahmini Toplam Dükkan</label>
                                          <input type=""number"" asp-for=""TargetTotalShops"" class=""form-control rounded-3"" placeholder=""Örn: 5"" min=""0"" required />
                                      </div>";

        code = Regex.Replace(code, patternStep1, replaceStep1, RegexOptions.Singleline);

        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("UI replaced.");
    }
}
