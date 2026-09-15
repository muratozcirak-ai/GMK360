import pyodbc

conn_str = r'Driver={ODBC Driver 17 for SQL Server};Server=(localdb)\mssqllocaldb;Database=GMK360Db;Trusted_Connection=yes;'
sql_file = r'C:\Users\murat\source\repos\GMK360\TempAddressDb\tr-address-db-main\data.sql'

conn = pyodbc.connect(conn_str)
cursor = conn.cursor()

ilceler = []
semtler = {}
mahalleler = []

with open(sql_file, 'r', encoding='utf-8', errors='ignore') as f:
    lines = f.readlines()

in_ilceler = False
in_semtler = False
in_mahalleler = False

for line in lines:
    line = line.strip()
    if line.startswith("INSERT INTO olt_ilceler"):
        in_ilceler = True
        continue
    elif line.startswith("INSERT INTO olt_semtler"):
        in_semtler = True
        continue
    elif line.startswith("INSERT INTO olt_mahalleler"):
        in_mahalleler = True
        continue
        
    if in_ilceler:
        if line.startswith('('):
            # (1, 1, 'Aladağ'), or (1, 1, 'Aladağ');
            clean = line.strip(',;')
            parts = clean.strip('()').split(',')
            if len(parts) >= 3:
                try:
                    ilceler.append((int(parts[0]), int(parts[1]), parts[2].strip(" '\"")))
                except:
                    pass
        if line.endswith(';'):
            in_ilceler = False

    if in_semtler:
        if line.startswith('('):
            clean = line.strip(',;')
            parts = clean.strip('()').split(',')
            if len(parts) >= 3:
                try:
                    semtler[int(parts[0])] = int(parts[1])
                except:
                    pass
        if line.endswith(';'):
            in_semtler = False
            
    if in_mahalleler:
        if line.startswith('('):
            clean = line.strip(',;')
            parts = clean.strip('()').split(',')
            if len(parts) >= 4:
                try:
                    mahalleler.append((int(parts[0]), int(parts[1]), parts[2].strip(" '\""), parts[3].strip(" '\"")))
                except:
                    pass
        if line.endswith(';'):
            in_mahalleler = False

cursor.execute('DELETE FROM Neighborhoods')
cursor.execute('DELETE FROM Districts')

cursor.execute('SET IDENTITY_INSERT Districts ON')
for d in ilceler:
    cursor.execute('''INSERT INTO Districts (Id, CityId, Name, RegionName, IsDeleted, CreatedAt) 
                      VALUES (?, ?, ?, '', 0, GETDATE())''', d)
cursor.execute('SET IDENTITY_INSERT Districts OFF')
conn.commit()
print(f"Inserted {len(ilceler)} districts.")

cursor.execute('SET IDENTITY_INSERT Neighborhoods ON')
for m in mahalleler:
    semt_id = m[1]
    ilce_id = semtler.get(semt_id, 1)
    cursor.execute('''INSERT INTO Neighborhoods (Id, DistrictId, Name, ZipCode, IsDeleted, CreatedAt) 
                      VALUES (?, ?, ?, ?, 0, GETDATE())''', (m[0], ilce_id, m[2], m[3]))
cursor.execute('SET IDENTITY_INSERT Neighborhoods OFF')
conn.commit()
print(f"Inserted {len(mahalleler)} neighborhoods.")
