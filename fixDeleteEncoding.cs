using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string pattern = @"Silmek istedi.*?inize emin misiniz\?";
        string replace = @"Silmek istediğinize emin misiniz?";
        code = Regex.Replace(code, pattern, replace);
        
        string pattern2 = @"title=""Plan.*? Sil""";
        string replace2 = @"title=""Planı Sil""";
        code = Regex.Replace(code, pattern2, replace2);

        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed delete button encoding.");
    }
}
