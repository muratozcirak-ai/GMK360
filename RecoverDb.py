import pyodbc

conn_str = r'Driver={ODBC Driver 17 for SQL Server};Server=(localdb)\mssqllocaldb;Database=master;Trusted_Connection=yes;'
try:
    conn = pyodbc.connect(conn_str, autocommit=True)
    cursor = conn.cursor()
    
    cursor.execute('RESTORE DATABASE GMK360Db WITH RECOVERY;')
    cursor.execute('ALTER DATABASE GMK360Db SET MULTI_USER;')
    
    print("Database recovered.")
except Exception as e:
    print(f"Error: {e}")
