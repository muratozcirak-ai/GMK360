using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string viewPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(viewPath, Encoding.UTF8);

        string oldNameHtml = @"<div class=""col-md-12 col-lg-4"">
                                      <label class=""form-label small fw-bold"">Blok / Bina Adı</label>
                                      <input type=""text"" name=""Blocks[${i}].BlockName"" class=""form-control b-name"" value=""${defaultName}"" required ${safeCount === 1 ? 'readonly' : ''} />
                                  </div>";
                                  
        string newNameHtml = @"<div class=""col-md-12 col-lg-3"">
                                      <label class=""form-label small fw-bold"">Blok Adı</label>
                                      <input type=""text"" name=""Blocks[${i}].BlockName"" class=""form-control b-name"" value=""${defaultName}"" required ${safeCount === 1 ? 'readonly' : ''} />
                                  </div>
                                  <div class=""col-12 col-lg-2"">
                                      <label class=""form-label small fw-bold"">Taban (m²)</label>
                                      <input type=""number"" name=""Blocks[${i}].BaseArea"" class=""form-control"" placeholder=""Örn: 200"" min=""1"" />
                                  </div>";

        // Since it's Turkish characters in powershell, let's just do an index based replace
        int idx = view.IndexOf("name=\"Blocks[${i}].BlockName\"");
        if (idx != -1)
        {
            // Just regex replace the entire col-md-12 col-lg-4 div
            string pattern = @"<div class=""col-md-12 col-lg-4"">\s*<label class=""form-label small fw-bold"">Blok / Bina Ad.*?</label>\s*<input type=""text"" name=""Blocks\[\$\{i\}\]\.BlockName"" class=""form-control b-name"" value=""\$\{defaultName\}"" required \$\{safeCount === 1 \? 'readonly' : ''\} />\s*</div>";
            view = System.Text.RegularExpressions.Regex.Replace(view, pattern, newNameHtml, System.Text.RegularExpressions.RegexOptions.Singleline);
            File.WriteAllText(viewPath, view, new UTF8Encoding(true));
            Console.WriteLine("Replaced view.");
        }
    }
}
