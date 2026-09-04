using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string content = File.ReadAllText(path, Encoding.UTF8);

        content = content.Replace("ÖÖzellikleri", "Özellikleri");
        content = content.Replace("ÖÖ", "Ö"); // Just in case there are others
        content = content.Replace("Görünüm", "Görünüm"); // double check

        File.WriteAllText(path, content, new UTF8Encoding(true));
        Console.WriteLine("Fixed typo ÖÖzellikleri");
    }
}
