import pyodbc

conn_str = r'Driver={ODBC Driver 17 for SQL Server};Server=(localdb)\mssqllocaldb;Database=master;Trusted_Connection=yes;'
try:
    conn = pyodbc.connect(conn_str, autocommit=True)
    cursor = conn.cursor()
    
    cursor.execute('''
        RESTORE FILELISTONLY 
        FROM DISK = 'C:\\Users\\murat\\Desktop\\GMK360Db_Backup.bak'
    ''')
    
    rows = cursor.fetchall()
    for row in rows:
        print(row[0], row[1], row[2]) # LogicalName, PhysicalName, Type
        
except Exception as e:
    print(f"Error: {e}")
