using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string viewPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string view = File.ReadAllText(viewPath, Encoding.UTF8);

        // Replace the unitsByFloor logic with a comprehensive list of floor levels
        string topLogicPattern = @"var unitsByFloor = Model\.Units\?.*?var blockImages =";
        string newTopLogic = @"var blockImages =";
        
        string newCsharpLogic = @"
    var allUnits = Model.Units ?? new List<GMK360.Core.Entities.BuildingUnit>();
    
    // Determine all possible floor levels (Expected + Actual)
    var floorLevels = new HashSet<int>();
    
    // 1. Add expected floors from Building properties
    if (Model.HasRoof) floorLevels.Add(Model.TotalFloors + 1);
    for (int i = Model.TotalFloors; i >= 1; i--) floorLevels.Add(i);
    if (Model.HasGroundFloor) floorLevels.Add(0);
    for (int i = 1; i <= Model.BasementFloors; i++) floorLevels.Add(-i);
    
    // 2. Add any floors that actually have units (in case they added one outside expected bounds)
    foreach(var u in allUnits) {
        floorLevels.Add(u.FloorLevel);
    }
    
    var sortedFloors = floorLevels.OrderByDescending(f => f).ToList();
    
    var blockImages =";

        if(Regex.IsMatch(view, topLogicPattern, RegexOptions.Singleline))
        {
            view = Regex.Replace(view, topLogicPattern, newCsharpLogic, RegexOptions.Singleline);
        }

        // Replace the loop condition
        // Old: @if (unitsByFloor != null && unitsByFloor.Any())
        // New: @if (sortedFloors.Any())
        string ifPattern = @"@if \(unitsByFloor != null && unitsByFloor\.Any\(\)\)";
        view = view.Replace(ifPattern, @"@if (sortedFloors.Any())");
        
        // Old: @foreach (var floorGroup in unitsByFloor)
        // New: @foreach (var floorLevel in sortedFloors) { var floorUnits = allUnits.Where(u => u.FloorLevel == floorLevel).OrderBy(u => u.Id).ToList(); ... }
        string foreachPattern = @"@foreach \(var floorGroup in unitsByFloor\)
                        \{
                            var floorName = floorGroup\.First\(\)\.FloorName \?\? \$\""\{floorGroup\.Key\}\. Kat\"";
                            var headingId = \$\""heading\{floorGroup\.Key\}\"".Replace\(""\-"", ""minus""\);
                            var collapseId = \$\""collapse\{floorGroup\.Key\}\"".Replace\(""\-"", ""minus""\);
                            
                            // Kat planını bul
                            var planDoc = floorPlans\.FirstOrDefault\(d => d\.EntityId == floorGroup\.Key\);";

        string newForeach = @"@foreach (var floorLevel in sortedFloors)
                        {
                            var floorUnits = allUnits.Where(u => u.FloorLevel == floorLevel).OrderBy(u => u.Id).ToList();
                            string defaultName = floorLevel == 0 ? ""Zemin Kat"" : (floorLevel < 0 ? $""{-floorLevel}. Bodrum"" : (floorLevel == Model.TotalFloors + 1 && Model.HasRoof ? ""Çatı Katı"" : $""{floorLevel}. Kat""));
                            var floorName = floorUnits.FirstOrDefault()?.FloorName ?? defaultName;
                            
                            var headingId = $""heading{floorLevel}"".Replace(""-"", ""minus"");
                            var collapseId = $""collapse{floorLevel}"".Replace(""-"", ""minus"");
                            
                            var planDoc = floorPlans.FirstOrDefault(d => d.EntityId == floorLevel);";
                            
        view = view.Replace(foreachPattern, newForeach);
        
        // Also need to fix any floorGroup.Key or floorGroup references
        // floorGroup.Key -> floorLevel
        view = view.Replace("floorGroup.Key", "floorLevel");
        
        // floorGroup.Count() -> floorUnits.Count()
        view = view.Replace("floorGroup.Count()", "floorUnits.Count()");
        
        // foreach(var unit in floorGroup.OrderBy...) -> foreach(var unit in floorUnits)
        string unitForeachPattern = @"@foreach \(var unit in floorGroup\.OrderBy\(u => u\.Id\)\)";
        view = view.Replace(unitForeachPattern, @"@foreach (var unit in floorUnits)");

        File.WriteAllText(viewPath, view, new UTF8Encoding(true));
        Console.WriteLine("ManageBlock view updated for structural floors.");
    }
}
