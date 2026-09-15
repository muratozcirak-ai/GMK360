import pyodbc

conn_str = r'Driver={ODBC Driver 17 for SQL Server};Server=(localdb)\mssqllocaldb;Database=master;Trusted_Connection=yes;'
try:
    conn = pyodbc.connect(conn_str, autocommit=True)
    cursor = conn.cursor()
    
    print("Restoring database with MOVE...")
    cursor.execute('''
        RESTORE DATABASE GMK360Db 
        FROM DISK = 'C:\\Users\\murat\\Desktop\\GMK360Db_Backup.bak' 
        WITH REPLACE, RECOVERY,
        MOVE 'GMK360Db' TO 'C:\\Users\\murat\\GMK360Db.mdf',
        MOVE 'GMK360Db_log' TO 'C:\\Users\\murat\\GMK360Db_log.ldf';
    ''')
    
    cursor.execute('ALTER DATABASE GMK360Db SET MULTI_USER;')
    print("Database restored successfully!")
    
except Exception as e:
    print(f"Error: {e}")
