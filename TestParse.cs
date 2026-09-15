using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

var lines = File.ReadAllLines(@"C:\Users\murat\source\repos\GMK360\TempAddressDb\tr-address-db-main\data.sql");

var semtRegex = new Regex(@"^\((\d+),\s*(\d+),\s*'([^']+)'\)");
var neighborhoodRegex = new Regex(@"^\((\d+),\s*(\d+),\s*'([^']*)',\s*'([^']*)'\)");

int semtCount = 0;
int mahCount = 0;
string currentTable = """";
var semtToIlce = new Dictionary<int, int>();

foreach (var line in lines)
{
    var trimmed = line.Trim();
    if (trimmed.StartsWith("INSERT INTO `volt_semtler`")) { currentTable = "semtler"; continue; }
    else if (trimmed.StartsWith("INSERT INTO `volt_mahalleler`")) { currentTable = "mahalleler"; continue; }
    else if (trimmed.StartsWith("INSERT INTO ")) { currentTable = ""; continue; }

    if (string.IsNullOrEmpty(currentTable) || !trimmed.StartsWith("(")) continue;

    var cleanLine = trimmed;
    if (cleanLine.EndsWith(",") || cleanLine.EndsWith(";"))
        cleanLine = cleanLine.Substring(0, cleanLine.Length - 1);

    if (currentTable == "semtler")
    {
        var match = semtRegex.Match(cleanLine);
        if (match.Success)
        {
            semtCount++;
            int id = int.Parse(match.Groups[1].Value);
            int ilceId = int.Parse(match.Groups[2].Value);
            semtToIlce[id] = ilceId;
        }
    }
    else if (currentTable == "mahalleler")
    {
        var match = neighborhoodRegex.Match(cleanLine);
        if (match.Success)
        {
            mahCount++;
            int semtId = int.Parse(match.Groups[2].Value);
            if (semtToIlce.ContainsKey(semtId))
            {
                // Ok
            }
        }
    }
}
Console.WriteLine($"Semtler: {semtCount}, Mahalleler: {mahCount}");
