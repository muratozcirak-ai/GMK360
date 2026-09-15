import re

sql_file = r'C:\Users\murat\source\repos\GMK360\TempAddressDb\tr-address-db-main\data.sql'

semtRegex = re.compile(r'^\((\d+),\s*(\d+),\s*\'([^\']+)\'\)')
neighborhoodRegex = re.compile(r'^\((\d+),\s*(\d+),\s*\'([^\']*)\',\s*\'([^\']*)\'\)')

semtToIlce = {}
mahalle_count = 0

currentTable = ""
with open(sql_file, 'r', encoding='utf-8', errors='ignore') as f:
    for line in f:
        trimmed = line.strip()
        if trimmed.startswith("INSERT INTO olt_semtler"):
            currentTable = "semtler"
            continue
        elif trimmed.startswith("INSERT INTO olt_mahalleler"):
            currentTable = "mahalleler"
            continue
        elif trimmed.startswith("INSERT INTO "):
            currentTable = ""
            continue
            
        if not currentTable or not trimmed.startswith("("):
            continue
            
        cleanLine = trimmed
        if cleanLine.endswith(",") or cleanLine.endswith(";"):
            cleanLine = cleanLine[:-1]
            
        if currentTable == "semtler":
            m = semtRegex.match(cleanLine)
            if m:
                semtToIlce[int(m.group(1))] = int(m.group(2))
        elif currentTable == "mahalleler":
            m = neighborhoodRegex.match(cleanLine)
            if m:
                semtId = int(m.group(2))
                if semtId in semtToIlce:
                    mahalle_count += 1

print(f"Semt in dict: {len(semtToIlce)}")
print(f"Mahalle count: {mahalle_count}")
