import pyodbc
import time

conn_str = r'Driver={ODBC Driver 17 for SQL Server};Server=(localdb)\mssqllocaldb;Database=master;Trusted_Connection=yes;'
conn = pyodbc.connect(conn_str, autocommit=True)
cursor = conn.cursor()

try:
    cursor.execute('RESTORE DATABASE GMK360Db WITH RECOVERY')
    print("Recovery applied.")
except Exception as e:
    print(f"Error: {e}")

time.sleep(2)
try:
    cursor.execute('USE GMK360Db; SELECT COUNT(*) FROM Districts')
    print("Districts count:", cursor.fetchone()[0])
except Exception as e:
    print(f"Error checking count: {e}")
