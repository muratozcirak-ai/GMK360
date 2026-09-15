import pyodbc

conn_str = r'Driver={ODBC Driver 17 for SQL Server};Server=(localdb)\mssqllocaldb;Database=GMK360Db;Trusted_Connection=yes;'
sql_file = r'C:\Users\murat\source\repos\GMK360\TempAddressDb\tr-address-db-main\data.sql'

conn = pyodbc.connect(conn_str)
cursor = conn.cursor()

# cursor.execute('DELETE FROM Neighborhoods')
# cursor.execute('DELETE FROM Districts')
# cursor.execute('SET IDENTITY_INSERT Districts ON')
# cursor.execute('SET IDENTITY_INSERT Neighborhoods ON')

def parse_and_insert():
    in_ilce = False
    in_semt = False
    in_mahalle = False
    
    ilceler = []
    semtler = {}
    mahalleler = []
    
    with open(sql_file, 'r', encoding='utf-8', errors='ignore') as f:
        for line in f:
            if line.startswith('INSERT INTO olt_ilceler'):
                in_ilce = True
                continue
            if line.startswith('INSERT INTO olt_semtler'):
                in_semt = True
                continue
            if line.startswith('INSERT INTO olt_mahalleler'):
                in_mahalle = True
                continue
                
            if in_ilce:
                if line.strip().startswith('('):
                    # (1, 1, 'Aladağ'),
                    parts = line.strip().strip(',;').strip('()').split(',')
                    if len(parts) >= 3:
                        id_val = int(parts[0].strip())
                        city_val = int(parts[1].strip())
                        name_val = parts[2].strip().strip("'")
                        ilceler.append((id_val, city_val, name_val))
                if line.strip().endswith(';'):
                    in_ilce = False
                    
            if in_semt:
                if line.strip().startswith('('):
                    parts = line.strip().strip(',;').strip('()').split(',')
                    if len(parts) >= 3:
                        id_val = int(parts[0].strip())
                        ilce_val = int(parts[1].strip())
                        semtler[id_val] = ilce_val
                if line.strip().endswith(';'):
                    in_semt = False
                    
            if in_mahalle:
                if line.strip().startswith('('):
                    parts = line.strip().strip(',;').strip('()').split(',')
                    if len(parts) >= 4:
                        id_val = int(parts[0].strip())
                        semt_val = int(parts[1].strip())
                        name_val = parts[2].strip().strip("'")
                        zip_val = parts[3].strip().strip("'")
                        ilce_val = semtler.get(semt_val, 1)
                        mahalleler.append((id_val, ilce_val, name_val, zip_val))
                if line.strip().endswith(';'):
                    in_mahalle = False

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
        cursor.execute('''INSERT INTO Neighborhoods (Id, DistrictId, Name, ZipCode, IsDeleted, CreatedAt) 
                          VALUES (?, ?, ?, ?, 0, GETDATE())''', m)
    cursor.execute('SET IDENTITY_INSERT Neighborhoods OFF')
    conn.commit()
    print(f"Inserted {len(mahalleler)} neighborhoods.")

parse_and_insert()

