import pyodbc

conn_str = r'Driver={ODBC Driver 17 for SQL Server};Server=(localdb)\mssqllocaldb;Database=master;Trusted_Connection=yes;'
conn = pyodbc.connect(conn_str)
cursor = conn.cursor()
cursor.execute("SELECT state_desc FROM sys.databases WHERE name='GMK360Db'")
print(cursor.fetchone()[0])
