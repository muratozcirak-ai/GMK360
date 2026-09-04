using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        var text = File.ReadAllText(path, Encoding.UTF8);

        string oldLogic = @"            // Combine RoomCount and UnitStructure logically
            string combinedRoomLayout = UnitStructure;
            if (UnitStructure != ""Dükkan / Ticari"" && UnitStructure != ""Depo"" && RoomCount != ""-"")
            {
                combinedRoomLayout = $""{RoomCount} ({UnitStructure})"";
            }";
            
        string newLogic = @"            // Combine RoomCount and UnitStructure logically
            string combinedRoomLayout = UnitStructure;
            if (UnitStructure != ""Dükkan / Ticari"" && UnitStructure != ""Depo"" && RoomCount != ""-"")
            {
                if (string.IsNullOrWhiteSpace(RoomCount)) 
                {
                    combinedRoomLayout = UnitStructure;
                } 
                else 
                {
                    combinedRoomLayout = $""{RoomCount.Trim()} ({UnitStructure})"";
                }
            }";

        if (text.Contains(oldLogic))
        {
            text = text.Replace(oldLogic, newLogic);
            File.WriteAllText(path, text, new UTF8Encoding(true));
            Console.WriteLine("Controller Logic Fixed");
        }
        else
        {
            Console.WriteLine("Old logic not found in controller.");
        }
    }
}
