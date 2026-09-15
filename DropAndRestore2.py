import pyodbc

conn_str = r'Driver={ODBC Driver 17 for SQL Server};Server=(localdb)\mssqllocaldb;Database=master;Trusted_Connection=yes;'
try:
    conn = pyodbc.connect(conn_str, autocommit=True)
    cursor = conn.cursor()
    
    cursor.execute('DROP DATABASE GMK360Db;')
    
    cursor.execute('''
        RESTORE DATABASE GMK360Db 
        FROM DISK = 'C:\\Users\\murat\\Desktop\\GMK360Db_Backup.bak' 
        WITH FILE = 1, REPLACE, RECOVERY,
        MOVE 'GMK360Db' TO 'C:\\Users\\murat\\GMK360Db_new.mdf',
        MOVE 'GMK360Db_log' TO 'C:\\Users\\murat\\GMK360Db_log_new.ldf';
    ''')
    
    print("Database restored successfully!")
    
except Exception as e:
    print(f"Error: {e}")
