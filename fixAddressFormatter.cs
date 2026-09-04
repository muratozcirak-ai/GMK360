using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string viewPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(viewPath, Encoding.UTF8);

        // 1. Change type="number" back to type="text"
        string pattern = @"<input type=""number"" asp-for=""Address"" id=""fullAddress"" class=""form-control rounded-start-3"" placeholder=""Örn: 16"" min=""1"" required />";
        string replacement = @"<input type=""text"" asp-for=""Address"" id=""fullAddress"" class=""form-control rounded-start-3"" placeholder=""Örn: 16 veya 16/A"" required />";
        view = Regex.Replace(view, pattern, replacement);

        // 2. Add the JS formatting script
        string jsCode = @"
              // KAPI NO AUTO-FORMATTER
              $('#fullAddress').on('input', function() {
                  let val = $(this).val().toUpperCase();
                  
                  // Sadece rakam, harf, tire ve taksim kalacak şekilde temizle
                  val = val.replace(/[^0-9A-Z\/\-]/g, '');
                  
                  // Eğer rakamdan hemen sonra harf geliyorsa araya taksim koy (Örn: 16A -> 16/A)
                  val = val.replace(/^(\d+)([A-Z])$/, '$1/$2');
                  
                  // Sıkı format kontrolü: Rakamlar + Opsiyonel (Taksim/Tire) + Opsiyonel (Tek Harf)
                  let match = val.match(/^(\d+)(?:([\/\-])([A-Z])?)?.*/);
                  if (match) {
                      let num = match[1];
                      let sep = match[2] || '';
                      let letter = match[3] || '';
                      val = num + sep + letter;
                  } else {
                      val = val.replace(/[^0-9]/g, ''); // Kurallara uymazsa sadece rakam bırak
                  }
                  
                  $(this).val(val);
              });
";

        // Insert JS inside the existing script tag
        int scriptIndex = view.IndexOf("$(document).ready(function () {");
        if (scriptIndex != -1)
        {
            int insertIndex = scriptIndex + "$(document).ready(function () {".Length;
            view = view.Insert(insertIndex, jsCode);
            File.WriteAllText(viewPath, view, new UTF8Encoding(true));
            Console.WriteLine("JS Formatter added.");
        }
        else
        {
            Console.WriteLine("Could not find script tag.");
        }
    }
}
