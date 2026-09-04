using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string ctrlPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string ctrl = File.ReadAllText(ctrlPath, Encoding.UTF8);

        string pattern = @"int unitCounter = 1;\s*// 1\. Bodrum Katlar.*?await _context\.SaveChangesAsync\(\);\s*blockCounter\+\+;";
        string replacement = @"
                        building.HasRoof = b.HasRoof;
                        await _context.SaveChangesAsync(); // Save updated building
                        
                        // Biz otomatik daire oluşturmayı bıraktık! 
                        // Katlar ve daireler ManageBlock ekranından manuel tablo + kopyalama ile yapılacak.
                        blockCounter++;";
        
        if (Regex.IsMatch(ctrl, pattern, RegexOptions.Singleline))
        {
            ctrl = Regex.Replace(ctrl, pattern, replacement, RegexOptions.Singleline);
            File.WriteAllText(ctrlPath, ctrl, new UTF8Encoding(true));
            Console.WriteLine("Successfully removed auto-gen.");
        }
        else
        {
            Console.WriteLine("Failed to match pattern.");
        }
    }
}
