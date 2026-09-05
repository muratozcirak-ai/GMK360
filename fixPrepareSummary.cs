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

        string oldLoop = @"blocks\.forEach\(b => \{\s*\}\);";
        
        string newLoop = @"blocks.forEach(b => {
                let aptsInput = b.querySelector('input[name*=""TotalApartments""]');
                let shopsInput = b.querySelector('input[name*=""TotalShops""]');
                if (aptsInput) totalApts += (parseInt(aptsInput.value) || 0);
                if (shopsInput) totalShops += (parseInt(shopsInput.value) || 0);
            });";

        code = Regex.Replace(code, oldLoop, newLoop);
        
        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed prepareSummary loop");
    }
}
