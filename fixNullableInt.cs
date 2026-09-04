using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string viewPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string view = File.ReadAllText(viewPath, Encoding.UTF8);

        string oldLogic = @"if (Model.HasRoof) floorLevels.Add(Model.TotalFloors + 1);
    for (int i = Model.TotalFloors; i >= 1; i--) floorLevels.Add(i);
    if (Model.HasGroundFloor) floorLevels.Add(0);
    for (int i = 1; i <= Model.BasementFloors; i++) floorLevels.Add(-i);";

        string newLogic = @"int totalF = Model.TotalFloors ?? 0;
    int baseF = Model.BasementFloors ?? 0;
    
    if (Model.HasRoof) floorLevels.Add(totalF + 1);
    for (int i = totalF; i >= 1; i--) floorLevels.Add(i);
    if (Model.HasGroundFloor) floorLevels.Add(0);
    for (int i = 1; i <= baseF; i++) floorLevels.Add(-i);";

        view = view.Replace(oldLogic, newLogic);
        
        // Also fix the defaultName logic where it uses Model.TotalFloors
        // (floorLevel == Model.TotalFloors + 1 && Model.HasRoof ? ""Çatı Katı"" : $""{floorLevel}. Kat""));
        string oldNameLogic = @"(floorLevel == Model.TotalFloors + 1 && Model.HasRoof ? ""Çatı Katı"" : $""{floorLevel}. Kat"")";
        string newNameLogic = @"(floorLevel == (Model.TotalFloors ?? 0) + 1 && Model.HasRoof ? ""Çatı Katı"" : $""{floorLevel}. Kat"")";
        
        view = view.Replace(oldNameLogic, newNameLogic);

        File.WriteAllText(viewPath, view, new UTF8Encoding(true));
        Console.WriteLine("Fixed nullable int error in ManageBlock.cshtml");
    }
}
