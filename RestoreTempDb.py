import pyodbc

conn_str = r'Driver={ODBC Driver 17 for SQL Server};Server=(localdb)\mssqllocaldb;Database=master;Trusted_Connection=yes;'
try:
    conn = pyodbc.connect(conn_str, autocommit=True)
    cursor = conn.cursor()
    
    # Check if Temp DB already exists and drop it
    try:
        cursor.execute('DROP DATABASE GMK360Db_Temp;')
    except:
        pass
        
    print("Restoring to Temp Database...")
    cursor.execute('''
        RESTORE DATABASE GMK360Db_Temp 
        FROM DISK = 'C:\\Users\\murat\\Desktop\\GMK360Db_Backup.bak' 
        WITH FILE = 1, REPLACE, RECOVERY,
        MOVE 'GMK360Db' TO 'C:\\Users\\murat\\GMK360Db_Temp.mdf',
        MOVE 'GMK360Db_log' TO 'C:\\Users\\murat\\GMK360Db_Temp_log.ldf';
    ''')
    
    print("Database restored to Temp successfully!")
    
except Exception as e:
    print(f"Error: {e}")
