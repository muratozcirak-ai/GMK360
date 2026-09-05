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

        string find = @"<div class=""col-md-4"">\s*<label class=""form-label fw-bold"">Toplam Arazi Alanı \(m²\)</label>[\s\S]*?<div class=""col-md-4"">\s*<label class=""form-label fw-bold"">Tahmini Toplam Daire</label>[\s\S]*?<div class=""col-md-4"">\s*<label class=""form-label fw-bold"">Tahmini Toplam Dükkan</label>[\s\S]*?</div>";

        string replace = @"<div class=""col-md-6"">
                                            <label class=""form-label fw-bold"">Toplam Arazi Alanı (m²)</label>
                                            <div class=""input-group"">
                                                <input type=""number"" asp-for=""TotalLandArea"" class=""form-control rounded-start-3"" placeholder=""Örn: 2500"" min=""1"" required />
                                                <span class=""input-group-text rounded-end-3"">m²</span>
                                            </div>
                                        </div>";

        if (Regex.IsMatch(code, find, RegexOptions.Singleline))
        {
            code = Regex.Replace(code, find, replace, RegexOptions.Singleline);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Removed Tahmini Toplam Daire/Dukkan.");
        }
        else
        {
            Console.WriteLine("Could not find the HTML snippet.");
        }
    }
}
