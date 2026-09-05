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

        string oldBlock = @"if\(!form\.checkValidity\(\)\) \{\s*form\.reportValidity\(\);\s*return;\s*\}";
        string newBlock = @"if(!form.checkValidity()) {
                  var invalidElements = form.querySelectorAll(':invalid');
                  var msg = ""Eksik veya hatalı alanlar var:\\n"";
                  invalidElements.forEach(el => {
                      var name = el.name || el.id || 'Bilinmeyen Alan';
                      msg += ""- "" + name + ""\\n"";
                  });
                  alert(msg);
                  form.reportValidity();
                  return;
              }";

        if(Regex.IsMatch(code, oldBlock))
        {
            code = Regex.Replace(code, oldBlock, newBlock);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Added validation debug alert.");
        }
        else
        {
            Console.WriteLine("Could not find validation block.");
        }
    }
}
