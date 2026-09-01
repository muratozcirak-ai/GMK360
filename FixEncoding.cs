using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string[] dirs = { "GMK360.Web/Views/Shared", "GMK360.Web/Views/Account" };
        foreach (var dir in dirs)
        {
            if (!Directory.Exists(dir)) continue;
            foreach (var file in Directory.GetFiles(dir, "*.cshtml"))
            {
                string content = File.ReadAllText(file, Encoding.Default);
                if (content.Contains("ö") || content.Contains("ı") || content.Contains("ş") || content.Contains("ü") || content.Contains("ç") || content.Contains("ğ") || content.Contains("Ç"))
                {
                    // This means the file was saved as ANSI but contains UTF8 byte sequences as ANSI chars.
                    // Let's read the raw ANSI bytes and parse them as UTF8!
                    byte[] bytes = Encoding.Default.GetBytes(content);
                    string fixedContent = Encoding.UTF8.GetString(bytes);
                    
                    // After fixing, save back as UTF8 (without BOM)
                    File.WriteAllText(file, fixedContent, new UTF8Encoding(false));
                    Console.WriteLine("Fixed " + file);
                }
            }
        }
    }
}
