import re

semt = re.compile(r'^\((\d+),\s*(\d+),\s*\'([^\']+)\'\)')

count = 0
with open(r'C:\Users\murat\source\repos\GMK360\TempAddressDb\tr-address-db-main\data.sql', 'r', encoding='utf-8', errors='ignore') as f:
    for line in f:
        cleanLine = line.strip()
        if cleanLine.endswith(',') or cleanLine.endswith(';'):
            cleanLine = cleanLine[:-1]
        
        if semt.match(cleanLine):
            count += 1

print(f"Semt matches: {count}")
