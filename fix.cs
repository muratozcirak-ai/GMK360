using System;
using System.IO;

class Program {
    static void Main() {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string text = File.ReadAllText(path);
        
        text = text.Replace("@unit.Title<br/>\r\n                                                            <small class=\"text-muted\">No: @unit.UnitNumber</small>", "@unit.DoorNumber");
        text = text.Replace("<td><span class=\"badge bg-info bg-opacity-10 text-info border border-info\">@unit.PropertyType</span></td>", "");
        text = text.Replace("openEditUnitModal(@unit.Id, '@unit.Title', '@unit.PropertyType', '@unit.RoomLayout', '@unit.UnitNumber')", "openEditUnitModal(@unit.Id, '@unit.DoorNumber', '@unit.RoomLayout')");
        
        File.WriteAllText(path, text, System.Text.Encoding.UTF8);
    }
}
