using System;
using System.Text.RegularExpressions;

var line = "(27701, 1006, 'Ã‡imenli Mah', '25530')";
var neighborhoodRegex = new Regex("^\(\(\d+\),\s*\(\d+\),\s*'([^']*)',\s*'([^']*)'\)");
var match = neighborhoodRegex.Match(line);
Console.WriteLine(match.Success);
