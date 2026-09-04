using System;
using System.IO;

class Program
{
    static void Main()
    {
        string viewPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(viewPath);
        
        int start = view.IndexOf("<div class=\"row g-3\">");
        if(start > -1)
        {
             string sub = view.Substring(start, 2500);
             Console.WriteLine(sub);
        }
    }
}
