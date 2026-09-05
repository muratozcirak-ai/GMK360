using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string oldAlert = @"alert('Kayıt sırasında bir hata oluştu.');";
        string newAlert = @"alert('Kayıt sırasında bir hata oluştu: ' + (data.message || 'Bilinmeyen hata'));";

        if (code.Contains(oldAlert))
        {
            code = code.Replace(oldAlert, newAlert);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Updated JS Alert in Create.cshtml.");
        }
    }
}
