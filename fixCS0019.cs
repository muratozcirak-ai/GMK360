using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string viewPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string view = File.ReadAllText(viewPath, Encoding.UTF8);

        view = view.Replace("int baseF = Model.BasementFloors ?? 0;", "int baseF = Model.BasementFloors;");
        
        File.WriteAllText(viewPath, view, new UTF8Encoding(true));
        Console.WriteLine("Fixed CS0019 error in ManageBlock.cshtml");
    }
}
