import re

with open(r'GMK360.Web\Controllers\SeedController.cs', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# We need to change how semtToIlce is populated
old_logic = '''            string currentTable = "";
            
            // Regex patterns based on standard mysqldump
            // (1, 'Adana')
            var cityRegex = new Regex(@"^\((\d+),\s*'([^']+)'\)");
            // (1, 1, 'Alada')
            var districtRegex = new Regex(@"^\((\d+),\s*(\d+),\s*'([^']+)'\)");
            // (1, 1, 'Karatas Semti')
            var semtRegex = new Regex(@"^\((\d+),\s*(\d+),\s*'([^']+)'\)");
            // (1, 1, 'Akpnar Mah', '01720')
            var neighborhoodRegex = new Regex(@"^\((\d+),\s*(\d+),\s*'([^']*)',\s*'([^']*)'\)");

            foreach (var line in lines)
            {
                var cleanLine = line.Trim();
                if (string.IsNullOrEmpty(cleanLine)) continue;

                if (cleanLine.StartsWith("INSERT INTO olt_iller")) { currentTable = "iller"; continue; }
                if (cleanLine.StartsWith("INSERT INTO olt_ilceler")) { currentTable = "ilceler"; continue; }
                if (cleanLine.StartsWith("INSERT INTO olt_semtler")) { currentTable = "semtler"; continue; }
                if (cleanLine.StartsWith("INSERT INTO olt_mahalleler")) { currentTable = "mahalleler"; continue; }

                if (cleanLine.StartsWith("("))
                {
                    if (currentTable == "iller")
                    {
                        var match = cityRegex.Match(cleanLine);
                        if (match.Success)
                        {
                            int id = int.Parse(match.Groups[1].Value);
                            string name = match.Groups[2].Value;
                            cities.Add(new City { Id = id, CountryId = 1, Name = name, PlateCode = id.ToString("D2") });
                        }
                    }
                    else if (currentTable == "ilceler")
                    {
                        var match = districtRegex.Match(cleanLine);
                        if (match.Success)
                        {
                            int id = int.Parse(match.Groups[1].Value);
                            int ilId = int.Parse(match.Groups[2].Value);
                            string name = match.Groups[3].Value;
                            districts.Add(new District { Id = id, CityId = ilId, Name = name });
                        }
                    }
                    else if (currentTable == "semtler")
                    {
                        var match = semtRegex.Match(cleanLine);
                        if (match.Success)
                        {
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
                            int id = int.Parse(match.Groups[1].Value);
                            int semtId = int.Parse(match.Groups[2].Value);
                            string name = match.Groups[3].Value;
                            string zipCode = match.Groups[4].Value;

                            if (semtToIlce.TryGetValue(semtId, out int ilceId))
                            {
                                neighborhoods.Add(new Neighborhood { Id = id, DistrictId = ilceId, Name = name, ZipCode = zipCode });
                            }
                        }
                    }
                }
            }'''

new_logic = '''            // Regex patterns based on standard mysqldump
            var cityRegex = new Regex(@"^\\((\\d+),\\s*'([^']+)'\\)");
            var districtRegex = new Regex(@"^\\((\\d+),\\s*(\\d+),\\s*'([^']+)'\\)");
            var semtRegex = new Regex(@"^\\((\\d+),\\s*(\\d+),\\s*'([^']+)'\\)");
            var neighborhoodRegex = new Regex(@"^\\((\\d+),\\s*(\\d+),\\s*'([^']*)',\\s*'([^']*)'\\)");

            // 1. Pass: Parse ONLY semtler because mahalleler depend on them and they appear later in the SQL file!
            string currentTable = "";
            foreach (var line in lines)
            {
                var cleanLine = line.Trim();
                if (string.IsNullOrEmpty(cleanLine)) continue;

                if (cleanLine.StartsWith("INSERT INTO olt_iller")) { currentTable = "iller"; continue; }
                if (cleanLine.StartsWith("INSERT INTO olt_ilceler")) { currentTable = "ilceler"; continue; }
                if (cleanLine.StartsWith("INSERT INTO olt_semtler")) { currentTable = "semtler"; continue; }
                if (cleanLine.StartsWith("INSERT INTO olt_mahalleler")) { currentTable = "mahalleler"; continue; }

                if (cleanLine.StartsWith("(") && currentTable == "semtler")
                {
                    var match = semtRegex.Match(cleanLine);
                    if (match.Success)
                    {
                        int id = int.Parse(match.Groups[1].Value);
                        int ilceId = int.Parse(match.Groups[2].Value);
                        semtToIlce[id] = ilceId;
                    }
                }
            }

            // 2. Pass: Parse Cities, Districts, Neighborhoods
            currentTable = "";
            foreach (var line in lines)
            {
                var cleanLine = line.Trim();
                if (string.IsNullOrEmpty(cleanLine)) continue;

                if (cleanLine.StartsWith("INSERT INTO olt_iller")) { currentTable = "iller"; continue; }
                if (cleanLine.StartsWith("INSERT INTO olt_ilceler")) { currentTable = "ilceler"; continue; }
                if (cleanLine.StartsWith("INSERT INTO olt_semtler")) { currentTable = "semtler"; continue; }
                if (cleanLine.StartsWith("INSERT INTO olt_mahalleler")) { currentTable = "mahalleler"; continue; }

                if (cleanLine.StartsWith("("))
                {
                    if (currentTable == "iller")
                    {
                        var match = cityRegex.Match(cleanLine);
                        if (match.Success)
                        {
                            int id = int.Parse(match.Groups[1].Value);
                            string name = match.Groups[2].Value;
                            cities.Add(new City { Id = id, CountryId = 1, Name = name, PlateCode = id.ToString("D2") });
                        }
                    }
                    else if (currentTable == "ilceler")
                    {
                        var match = districtRegex.Match(cleanLine);
                        if (match.Success)
                        {
                            int id = int.Parse(match.Groups[1].Value);
                            int ilId = int.Parse(match.Groups[2].Value);
                            string name = match.Groups[3].Value;
                            districts.Add(new District { Id = id, CityId = ilId, Name = name });
                        }
                    }
                    else if (currentTable == "mahalleler")
                    {
                        var match = neighborhoodRegex.Match(cleanLine);
                        if (match.Success)
                        {
                            int id = int.Parse(match.Groups[1].Value);
                            int semtId = int.Parse(match.Groups[2].Value);
                            string name = match.Groups[3].Value;
                            string zipCode = match.Groups[4].Value;

                            if (semtToIlce.TryGetValue(semtId, out int ilceId))
                            {
                                neighborhoods.Add(new Neighborhood { Id = id, DistrictId = ilceId, Name = name, ZipCode = zipCode });
                            }
                        }
                    }
                }
            }'''

# Note: Python string replace for large blocks can be tricky due to invisible chars. 
# We'll use regex for replacement.
pattern = r'string currentTable = "";.*?if \(semtToIlce\.TryGetValue\(semtId, out int ilceId\)\).*?neighborhoods\.Add\(.*?\}\s*\}\s*\}\s*\}\s*\}'

import re
content = re.sub(pattern, new_logic, content, flags=re.DOTALL)

with open(r'GMK360.Web\Controllers\SeedController.cs', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("SeedController logic fixed for table order.")
