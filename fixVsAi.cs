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

        // Remove DOMContentLoaded from inside prepareSummary
        string find = @"            // On page load: if we have a draft id.*?}\);";
        Match m = Regex.Match(code, find, RegexOptions.Singleline);
        if (m.Success)
        {
            string domLoadCode = m.Value;
            code = code.Replace(domLoadCode, "");
            
            // Insert it outside prepareSummary (before the closing </script>)
            string insertTarget = @"</script>";
            code = code.Replace(insertTarget, domLoadCode + "\n    " + insertTarget);
            
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Fixed DOMContentLoaded position.");
        }
        else
        {
            Console.WriteLine("Could not find DOMContentLoaded block.");
        }
    }
}
