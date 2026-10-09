import re
with open(r'GMK360.Web\Controllers\SeedController.cs', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

content = re.sub(r'if \(cleanLine\.StartsWith\("INSERT INTO.*?volt_iller.*?"\)\)', 'if (cleanLine.Contains("INSERT INTO") && cleanLine.Contains("volt_iller"))', content)
content = re.sub(r'if \(cleanLine\.StartsWith\("INSERT INTO.*?volt_ilceler.*?"\)\)', 'if (cleanLine.Contains("INSERT INTO") && cleanLine.Contains("volt_ilceler"))', content)
content = re.sub(r'if \(cleanLine\.StartsWith\("INSERT INTO.*?volt_semtler.*?"\)\)', 'if (cleanLine.Contains("INSERT INTO") && cleanLine.Contains("volt_semtler"))', content)
content = re.sub(r'if \(cleanLine\.StartsWith\("INSERT INTO.*?volt_mahalleler.*?"\)\)', 'if (cleanLine.Contains("INSERT INTO") && cleanLine.Contains("volt_mahalleler"))', content)

with open(r'GMK360.Web\Controllers\SeedController.cs', 'w', encoding='utf-8-sig') as f:
    f.write(content)
