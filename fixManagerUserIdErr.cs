using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string oldBlock = @"ManagerUserId = _userManager\.GetUserId\(User\) \?\? project\.ManagerUserId";
        string newBlock = @"ManagerUserId = _userManager.GetUserId(User) ?? """"";

        if(Regex.IsMatch(code, oldBlock))
        {
            code = Regex.Replace(code, oldBlock, newBlock);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Fixed ManagerUserId reference.");
        }
    }
}
