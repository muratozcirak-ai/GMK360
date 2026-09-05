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

        string oldBlock = @"container\.insertAdjacentHTML\('beforeend', html\);\s*\}\s*\}\s*";
        string newBlock = @"container.insertAdjacentHTML('beforeend', html);
              }
            }
          }
";
        if(Regex.IsMatch(code, oldBlock))
        {
            code = Regex.Replace(code, oldBlock, newBlock);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Fixed missing bracket in generateBlocks.");
        }
        else
        {
            Console.WriteLine("Could not find the insertion point.");
        }
    }
}
