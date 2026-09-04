using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string viewPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string view = File.ReadAllText(viewPath, Encoding.UTF8);

        view = view.Replace("@if (unitsByFloor != null && unitsByFloor.Any())", "@if (sortedFloors.Any())");
        
        string oldForeach = @"@foreach (var floorGroup in unitsByFloor)
                        {
                            var floorName = floorGroup.First().FloorName ?? $""{floorLevel}. Kat"";
                            var headingId = $""heading{floorLevel}"".Replace(""-"", ""minus"");
                            var collapseId = $""collapse{floorLevel}"".Replace(""-"", ""minus"");
                            
                            // Kat planını bul
                            var planDoc = floorPlans.FirstOrDefault(d => d.EntityId == floorLevel);";
                            
        string newForeach = @"@foreach (var floorLevel in sortedFloors)
                        {
                            var floorUnits = allUnits.Where(u => u.FloorLevel == floorLevel).OrderBy(u => u.Id).ToList();
                            string defaultName = floorLevel == 0 ? ""Zemin Kat"" : (floorLevel < 0 ? $""{-floorLevel}. Bodrum"" : (floorLevel == Model.TotalFloors + 1 && Model.HasRoof ? ""Çatı Katı"" : $""{floorLevel}. Kat""));
                            var floorName = floorUnits.FirstOrDefault()?.FloorName ?? defaultName;
                            
                            var headingId = $""heading{floorLevel}"".Replace(""-"", ""minus"");
                            var collapseId = $""collapse{floorLevel}"".Replace(""-"", ""minus"");
                            
                            var planDoc = floorPlans.FirstOrDefault(d => d.EntityId == floorLevel);";
                            
        view = view.Replace(oldForeach, newForeach);
        
        view = view.Replace("@foreach (var unit in floorGroup.OrderBy(u => u.Id))", "@foreach (var unit in floorUnits)");
        
        File.WriteAllText(viewPath, view, new UTF8Encoding(true));
        Console.WriteLine("Fixed syntax errors in ManageBlock.cshtml");
    }
}
