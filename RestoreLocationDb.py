import pyodbc
import re

conn_str = r'Driver={ODBC Driver 17 for SQL Server};Server=(localdb)\mssqllocaldb;Database=GMK360Db;Trusted_Connection=yes;'
sql_file = r'C:\Users\murat\source\repos\GMK360\TempAddressDb\tr-address-db-main\data.sql'

try:
    conn = pyodbc.connect(conn_str)
    cursor = conn.cursor()
    
    # Enable IDENTITY_INSERT if needed, but we can just let it auto increment or force IDs.
    cursor.execute('DELETE FROM Neighborhoods')
    cursor.execute('DELETE FROM Districts')
    
    with open(sql_file, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()

    # Parse ilceler
    # INSERT INTO olt_ilceler (id, il_id, ilce_adi) VALUES (1, 1, 'Aladağ'), ...
    ilceler_match = re.search(r'INSERT INTO olt_ilceler[^V]+VALUES\s*(.*?);', content, re.DOTALL)
    if ilceler_match:
        ilceler_str = ilceler_match.group(1)
        # Parse tuples (id, il_id, 'ilce_adi')
        tuples = re.findall(r'\((\d+),\s*(\d+),\s*\'(.*?)\'\)', ilceler_str)
        cursor.execute('SET IDENTITY_INSERT Districts ON')
        for t in tuples:
            cursor.execute('''INSERT INTO Districts (Id, CityId, Name, RegionName, IsDeleted, CreatedAt) 
                              VALUES (?, ?, ?, '', 0, GETDATE())''', (int(t[0]), int(t[1]), t[2]))
        cursor.execute('SET IDENTITY_INSERT Districts OFF')
        conn.commit()
        print(f"Inserted {len(tuples)} districts.")

    # Parse mahalleler
    # INSERT INTO olt_mahalleler (id, semt_id, mahalle_adi, posta_kodu) VALUES
    # Wait, the neighborhood is linked to semt_id. We need semtler to map to ilce_id!
    
    semtler_match = re.search(r'INSERT INTO olt_semtler[^V]+VALUES\s*(.*?);', content, re.DOTALL)
    semt_to_ilce = {}
    if semtler_match:
        semtler_str = semtler_match.group(1)
        # (id, ilce_id, 'semt_adi')
        stuples = re.findall(r'\((\d+),\s*(\d+),\s*\'(.*?)\'\)', semtler_str)
        for st in stuples:
            semt_to_ilce[int(st[0])] = int(st[1])
            
    mahalle_match = re.search(r'INSERT INTO olt_mahalleler[^V]+VALUES\s*(.*?);', content, re.DOTALL)
    if mahalle_match:
        mahalle_str = mahalle_match.group(1)
        # (id, semt_id, 'mahalle_adi', 'posta_kodu')
        mtuples = re.findall(r'\((\d+),\s*(\d+),\s*\'(.*?)\',\s*\'(.*?)\'\)', mahalle_str)
        cursor.execute('SET IDENTITY_INSERT Neighborhoods ON')
        for m in mtuples:
            semt_id = int(m[1])
            ilce_id = semt_to_ilce.get(semt_id, 1) # fallback
            cursor.execute('''INSERT INTO Neighborhoods (Id, DistrictId, Name, ZipCode, IsDeleted, CreatedAt) 
                              VALUES (?, ?, ?, ?, 0, GETDATE())''', (int(m[0]), ilce_id, m[2], m[3]))
        cursor.execute('SET IDENTITY_INSERT Neighborhoods OFF')
        conn.commit()
        print(f"Inserted {len(mtuples)} neighborhoods.")
        
except Exception as e:
    print(f"Error: {e}")
