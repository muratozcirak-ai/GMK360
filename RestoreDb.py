import pyodbc

conn_str = r'Driver={ODBC Driver 17 for SQL Server};Server=(localdb)\mssqllocaldb;Database=master;Trusted_Connection=yes;'
try:
    conn = pyodbc.connect(conn_str, autocommit=True)
    cursor = conn.cursor()
    
    # Kick off active connections
    cursor.execute('''
        ALTER DATABASE GMK360Db 
        SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    ''')
    
    print("Restoring database...")
    cursor.execute('''
        RESTORE DATABASE GMK360Db 
        FROM DISK = 'C:\\Users\\murat\\Desktop\\GMK360Db_Backup.bak' 
        WITH REPLACE, RECOVERY;
    ''')
    
    cursor.execute('''
        ALTER DATABASE GMK360Db 
        SET MULTI_USER;
    ''')
    print("Database restored successfully!")
    
except Exception as e:
    print(f"Error: {e}")
    try:
        cursor.execute('''
            ALTER DATABASE GMK360Db 
            SET MULTI_USER;
        ''')
    except:
        pass
