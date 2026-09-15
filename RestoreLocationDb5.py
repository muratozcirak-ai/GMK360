import pyodbc
import re

conn_str = r'Driver={ODBC Driver 17 for SQL Server};Server=(localdb)\mssqllocaldb;Database=GMK360Db;Trusted_Connection=yes;'
sql_file = r'C:\Users\murat\source\repos\GMK360\TempAddressDb\tr-address-db-main\data.sql'

conn = pyodbc.connect(conn_str)
cursor = conn.cursor()

ilceler = []
semtler = {}
mahalleler = []

with open(sql_file, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

# Using regex to find the blocks safely
ilce_blocks = re.findall(r'INSERT INTO olt_ilceler[^\n]*\n(.*?);', content, re.DOTALL)
if ilce_blocks:
    for block in ilce_blocks:
        tuples = re.findall(r'\((\d+),\s*(\d+),\s*\'(.*?)\'\)', block)
        ilceler.extend(tuples)

semt_blocks = re.findall(r'INSERT INTO olt_semtler[^\n]*\n(.*?);', content, re.DOTALL)
if semt_blocks:
    for block in semt_blocks:
        tuples = re.findall(r'\((\d+),\s*(\d+),\s*\'(.*?)\'\)', block)
        for t in tuples:
            semtler[int(t[0])] = int(t[1])

mahalle_blocks = re.findall(r'INSERT INTO olt_mahalleler[^\n]*\n(.*?);', content, re.DOTALL)
if mahalle_blocks:
    for block in mahalle_blocks:
        tuples = re.findall(r'\((\d+),\s*(\d+),\s*\'(.*?)\',\s*\'(.*?)\'\)', block)
        for t in tuples:
            mahalleler.append((int(t[0]), int(t[1]), t[2], t[3]))

print(f"Parsed {len(ilceler)} ilceler, {len(mahalleler)} mahalleler")

if len(ilceler) > 0:
    cursor.execute('DELETE FROM Neighborhoods')
    cursor.execute('DELETE FROM Districts')

    cursor.execute('SET IDENTITY_INSERT Districts ON')
    for d in ilceler:
        cursor.execute('''INSERT INTO Districts (Id, CityId, Name, RegionName, IsDeleted, CreatedAt) 
                          VALUES (?, ?, ?, '', 0, GETDATE())''', (int(d[0]), int(d[1]), d[2]))
    cursor.execute('SET IDENTITY_INSERT Districts OFF')
    conn.commit()
    print(f"Inserted {len(ilceler)} districts.")

    cursor.execute('SET IDENTITY_INSERT Neighborhoods ON')
    for m in mahalleler:
        semt_id = int(m[1])
        ilce_id = semtler.get(semt_id, 1)
        cursor.execute('''INSERT INTO Neighborhoods (Id, DistrictId, Name, ZipCode, IsDeleted, CreatedAt) 
                          VALUES (?, ?, ?, ?, 0, GETDATE())''', (int(m[0]), ilce_id, m[2], m[3]))
    cursor.execute('SET IDENTITY_INSERT Neighborhoods OFF')
    conn.commit()
    print(f"Inserted {len(mahalleler)} neighborhoods.")

