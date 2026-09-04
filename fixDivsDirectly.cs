using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(path, Encoding.UTF8);

        // Replace EXACTLY the problematic string
        string bad = @"                            </div></div>
                        </div>
                        <div class=""text-end mt-5"">";
                        
        string good = @"                            </div>
                        </div>
                        <div class=""text-end mt-5"">";
                        
        if (view.Contains(bad))
        {
            view = view.Replace(bad, good);
            File.WriteAllText(path, view, new UTF8Encoding(true));
            Console.WriteLine("Fixed exact match 1.");
            return;
        }

        string bad2 = "</div></div>\r\n                        </div>\r\n                        <div class=\"text-end mt-5\">";
        string good2 = "</div>\r\n                        </div>\r\n                        <div class=\"text-end mt-5\">";
        if (view.Contains(bad2))
        {
            view = view.Replace(bad2, good2);
            File.WriteAllText(path, view, new UTF8Encoding(true));
            Console.WriteLine("Fixed exact match 2.");
            return;
        }
        
        string bad3 = "</div></div>\n                        </div>\n                        <div class=\"text-end mt-5\">";
        string good3 = "</div>\n                        </div>\n                        <div class=\"text-end mt-5\">";
        if (view.Contains(bad3))
        {
            view = view.Replace(bad3, good3);
            File.WriteAllText(path, view, new UTF8Encoding(true));
            Console.WriteLine("Fixed exact match 3.");
            return;
        }

        Console.WriteLine("Could not find the match.");
    }
}
