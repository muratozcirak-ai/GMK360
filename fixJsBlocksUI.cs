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

        // Remove the black button completely by hiding or deleting the col-md-4 div
        string blackButtonDiv = @"<div class=""col-md-4"">\s*<button type=""button"" class=""btn btn-dark btn-lg w-100 rounded-3"" onclick=""generateBlocks\(\)"">.*?<\/button>\s*<\/div>";
        if (Regex.IsMatch(code, blackButtonDiv)) {
            code = Regex.Replace(code, blackButtonDiv, "");
            Console.WriteLine("Removed black button.");
        }

        // Add oninput to blockCount, and change col-md-8 to col-md-12 since button is gone
        string blockCountContainer = @"<div class=""col-md-8"">\s*<label class=""form-label fw-bold"">.*?<\/label>\s*<input type=""number"" id=""blockCount"" class=""form-control form-control-lg rounded-3"" min=""1"" max=""20"" value=""@\(Model\.Blocks != null && Model\.Blocks\.Any\(\) \? Model\.Blocks\.Count : 1\)"" \/>\s*<\/div>";
        string newBlockCountContainer = @"<div class=""col-md-12"">
                                  <label class=""form-label fw-bold"">Projenizde Kaç Blok (Bina) Var?</label>
                                  <input type=""number"" id=""blockCount"" class=""form-control form-control-lg rounded-3"" min=""1"" max=""20"" value=""@(Model.Blocks != null && Model.Blocks.Any() ? Model.Blocks.Count : 1)"" oninput=""generateBlocks()"" />
                              </div>";
        
        if (Regex.IsMatch(code, blockCountContainer)) {
            code = Regex.Replace(code, blockCountContainer, newBlockCountContainer);
            Console.WriteLine("Added oninput and expanded width.");
        }

        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Done.");
    }
}
