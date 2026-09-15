import pyodbc

conn_str = r'Driver={ODBC Driver 17 for SQL Server};Server=(localdb)\mssqllocaldb;Database=master;Trusted_Connection=yes;'
try:
    conn = pyodbc.connect(conn_str)
    cursor = conn.cursor()
    cursor.execute('USE GMK360Db; SELECT COUNT(*) FROM Districts')
    print("Districts count:", cursor.fetchone()[0])
except Exception as e:
    print(f"Error: {e}")
