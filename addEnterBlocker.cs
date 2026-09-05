using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string enterBlocker = @"
        // Enter tuşunun formu otomatik göndermesini engelle
        $(window).keydown(function(event){
            if(event.keyCode == 13) {
                event.preventDefault();
                return false;
            }
        });
";

        if (!code.Contains("event.keyCode == 13"))
        {
            code = code.Replace("$(document).ready(function() {", "$(document).ready(function() {\n" + enterBlocker);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Added Enter key blocker.");
        }
    }
}
