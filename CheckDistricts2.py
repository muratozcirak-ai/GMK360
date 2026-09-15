import pyodbc

conn_str = r'Driver={ODBC Driver 17 for SQL Server};Server=(localdb)\mssqllocaldb;Database=GMK360Db;Trusted_Connection=yes;'
try:
    conn = pyodbc.connect(conn_str)
    cursor = conn.cursor()
    cursor.execute('SELECT COUNT(*) FROM Districts')
    print("Districts: ", cursor.fetchone()[0])
    
    cursor.execute('SELECT COUNT(*) FROM Neighborhoods')
    print("Neighborhoods: ", cursor.fetchone()[0])
    
except Exception as e:
    print(f"Error: {e}")
