import re
with open(r'GMK360.Web\Controllers\SeedController.cs', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

content = re.sub(r'INSERT INTO +volt_iller+', 'INSERT INTO olt_iller', content)
content = re.sub(r'INSERT INTO +volt_ilceler+', 'INSERT INTO olt_ilceler', content)
content = re.sub(r'INSERT INTO +volt_semtler+', 'INSERT INTO olt_semtler', content)
content = re.sub(r'INSERT INTO +volt_mahalleler+', 'INSERT INTO olt_mahalleler', content)

with open(r'GMK360.Web\Controllers\SeedController.cs', 'w', encoding='utf-8-sig') as f:
    f.write(content)
