import re

with open(r'GMK360.Web\Controllers\SeedController.cs', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

content = content.replace('INSERT INTO  olt_iller', 'INSERT INTO olt_iller')
content = content.replace('INSERT INTO \x0bolt_iller', 'INSERT INTO olt_iller') # vertical tab

content = content.replace('INSERT INTO  olt_ilceler', 'INSERT INTO olt_ilceler')
content = content.replace('INSERT INTO \x0bolt_ilceler', 'INSERT INTO olt_ilceler')

content = content.replace('INSERT INTO  olt_semtler', 'INSERT INTO olt_semtler')
content = content.replace('INSERT INTO \x0bolt_semtler', 'INSERT INTO olt_semtler')

content = content.replace('INSERT INTO  olt_mahalleler', 'INSERT INTO olt_mahalleler')
content = content.replace('INSERT INTO \x0bolt_mahalleler', 'INSERT INTO olt_mahalleler')

with open(r'GMK360.Web\Controllers\SeedController.cs', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("Backticks restored in SeedController.cs")
