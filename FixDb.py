import pyodbc

conn_str = r'Driver={ODBC Driver 17 for SQL Server};Server=(localdb)\mssqllocaldb;Database=GMK360Db;Trusted_Connection=yes;'
conn = pyodbc.connect(conn_str, autocommit=True)
cursor = conn.cursor()

cursor.execute('SELECT Id, Latitude, Longitude FROM ConstructionProjects WHERE Latitude > 90 OR Latitude < -90')
rows = cursor.fetchall()

for r in rows:
    pid = r[0]
    lat = r[1]
    lng = r[2]
    
    # Example: 40956760 -> string -> len is 8. Insert dot after 2nd char. 
    # Or just divide by 1000000.
    
    # Safe fallback: parse as string, insert dot.
    slat = str(int(lat))
    slng = str(int(lng))
    
    if len(slat) > 2:
        new_lat = float(slat[:2] + '.' + slat[2:])
    else:
        new_lat = lat
        
    if len(slng) > 2:
        new_lng = float(slng[:2] + '.' + slng[2:])
    else:
        new_lng = lng
        
    cursor.execute('UPDATE ConstructionProjects SET Latitude = ?, Longitude = ? WHERE Id = ?', (new_lat, new_lng, pid))
    print(f'Fixed project {pid}: {lat},{lng} -> {new_lat},{new_lng}')

