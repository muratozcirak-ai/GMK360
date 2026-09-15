import pyodbc

conn_str = r'Driver={ODBC Driver 17 for SQL Server};Server=(localdb)\mssqllocaldb;Database=GMK360Db;Trusted_Connection=yes;'
try:
    conn = pyodbc.connect(conn_str, autocommit=True)
    cursor = conn.cursor()
    
    print("Emptying existing location tables...")
    cursor.execute('DELETE FROM Streets')
    cursor.execute('DELETE FROM Neighborhoods')
    cursor.execute('DELETE FROM Districts')
    cursor.execute('DELETE FROM Cities')
    
    print("Copying Cities...")
    cursor.execute('''
        SET IDENTITY_INSERT Cities ON;
        INSERT INTO Cities (Id, CountryId, Name, PlateCode, CreatedAt, UpdatedAt, IsDeleted)
        SELECT Id, CountryId, Name, PlateCode, CreatedAt, UpdatedAt, IsDeleted
        FROM GMK360Db_Temp.dbo.Cities;
        SET IDENTITY_INSERT Cities OFF;
    ''')
    
    print("Copying Districts...")
    cursor.execute('''
        SET IDENTITY_INSERT Districts ON;
        INSERT INTO Districts (Id, CityId, Name, RegionName, CreatedAt, UpdatedAt, IsDeleted)
        SELECT Id, CityId, Name, RegionName, CreatedAt, UpdatedAt, IsDeleted
        FROM GMK360Db_Temp.dbo.Districts;
        SET IDENTITY_INSERT Districts OFF;
    ''')
    
    print("Copying Neighborhoods...")
    cursor.execute('''
        SET IDENTITY_INSERT Neighborhoods ON;
        INSERT INTO Neighborhoods (Id, DistrictId, Name, ZipCode, CreatedAt, UpdatedAt, IsDeleted)
        SELECT Id, DistrictId, Name, ZipCode, CreatedAt, UpdatedAt, IsDeleted
        FROM GMK360Db_Temp.dbo.Neighborhoods;
        SET IDENTITY_INSERT Neighborhoods OFF;
    ''')
    
    print("Copying Streets...")
    try:
        cursor.execute('''
            SET IDENTITY_INSERT Streets ON;
            INSERT INTO Streets (Id, NeighborhoodId, Name, Latitude, Longitude, IsActive, CreatedAt, UpdatedAt, IsDeleted)
            SELECT Id, NeighborhoodId, Name, Latitude, Longitude, IsActive, CreatedAt, UpdatedAt, IsDeleted
            FROM GMK360Db_Temp.dbo.Streets;
            SET IDENTITY_INSERT Streets OFF;
        ''')
    except Exception as e:
        print(f"Street copy error (might not exist in temp): {e}")

    cursor.execute('SELECT COUNT(*) FROM Districts')
    print("Restored Districts: ", cursor.fetchone()[0])
    
    cursor.execute('SELECT COUNT(*) FROM Neighborhoods')
    print("Restored Neighborhoods: ", cursor.fetchone()[0])

except Exception as e:
    print(f"Error: {e}")
