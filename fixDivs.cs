using System;
using System.IO;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(path);
        
        // Let's find exactly this piece and replace it
        string badStr = @"</div>
                                </div>
                            </div></div>
                        </div>
                        <div class=""text-end mt-5"">";
        string badStr2 = "</div></div>\r\n                        </div>\r\n                        <div class=\"text-end mt-5\">";
        string badStr3 = "</div></div>\n                        </div>\n                        <div class=\"text-end mt-5\">";
        
        string goodStr = @"</div>
                                </div>
                            </div>
                        </div>
                        <div class=""text-end mt-5"">";
                        
        view = view.Replace(badStr, goodStr);
        view = view.Replace(badStr2, goodStr);
        view = view.Replace(badStr3, goodStr);

        File.WriteAllText(path, view, new System.Text.UTF8Encoding(true));
        Console.WriteLine("Divs fixed.");
    }
}
