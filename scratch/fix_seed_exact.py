with open(r'GMK360.Web\Controllers\SeedController.cs', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

target = '''            string currentTable = "";
            
            // Regex patterns based on standard mysqldump
            // (1, 'Adana')
            var cityRegex = new Regex(@"^\((\d+),\s*'([^']+)'\)");
            // (1, 1, 'Aladağ')
            var districtRegex = new Regex(@"^\((\d+),\s*(\d+),\s*'([^']+)'\)");
            // (1, 1, 'Karatas Semti')
            var semtRegex = new Regex(@"^\((\d+),\s*(\d+),\s*'([^']+)'\)");
            // (1, 1, 'Akpınar Mah', '01720')
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

new_logic = '''            // Regex patterns
            var cityRegex = new Regex(@"^\\((\\d+),\\s*'([^']+)'\\)");
            var districtRegex = new Regex(@"^\\((\\d+),\\s*(\\d+),\\s*'([^']+)'\\)");
            var semtRegex = new Regex(@"^\\((\\d+),\\s*(\\d+),\\s*'([^']+)'\\)");
            var neighborhoodRegex = new Regex(@"^\\((\\d+),\\s*(\\d+),\\s*'([^']*)',\\s*'([^']*)'\\)");

            // Pass 1: Parse Semtler
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

            // Pass 2: Parse Others
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

# Replace by finding the index
start_idx = content.find('string currentTable = "";')
end_idx = content.find('// In EF Core, inserting with explicit identity can be tricky.')
if start_idx != -1 and end_idx != -1:
    content = content[:start_idx] + new_logic + '\n\n            ' + content[end_idx:]
    with open(r'GMK360.Web\Controllers\SeedController.cs', 'w', encoding='utf-8-sig') as f:
        f.write(content)
    print("Fixed.")
else:
    print("Could not find start or end index.")
