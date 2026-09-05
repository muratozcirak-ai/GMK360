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

        // Step 1 fix
        string find1 = @"var form = document\.getElementById\('wizardForm'\);\s*if\(!form\.checkValidity\(\)\) \{\s*var invalidElements = form\.querySelectorAll\(':invalid'\);";
        string replace1 = @"var form = document.getElementById('wizardForm');
            var step = document.getElementById('step1');
            var invalidElements = step.querySelectorAll(':invalid');
            if(invalidElements.length > 0) {";
            
        code = Regex.Replace(code, find1, replace1);

        // Step 2 fix
        string find2 = @"function saveStep2AndContinue\(\) \{\s*var form = document\.getElementById\('wizardForm'\);\s*if\(!form\.checkValidity\(\)\) \{\s*var invalidElements = form\.querySelectorAll\(':invalid'\);";
        string replace2 = @"function saveStep2AndContinue() {
            var form = document.getElementById('wizardForm');
            var step = document.getElementById('step2');
            var invalidElements = step.querySelectorAll(':invalid');
            if(invalidElements.length > 0) {";

        code = Regex.Replace(code, find2, replace2);

        // Replace form.reportValidity() with invalidElements[0].reportValidity()
        string find3 = @"alert\(msg\);\s*form\.reportValidity\(\);";
        string replace3 = @"alert(msg);
                  invalidElements[0].reportValidity();";
        code = Regex.Replace(code, find3, replace3);

        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed Wizard Validation logic.");
    }
}
