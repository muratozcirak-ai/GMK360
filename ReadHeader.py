import pyodbc

conn_str = r'Driver={ODBC Driver 17 for SQL Server};Server=(localdb)\mssqllocaldb;Database=master;Trusted_Connection=yes;'
conn = pyodbc.connect(conn_str, autocommit=True)
cursor = conn.cursor()
cursor.execute("RESTORE HEADERONLY FROM DISK = 'C:\\Users\\murat\\Desktop\\GMK360Db_Backup.bak'")
columns = [column[0] for column in cursor.description]
for row in cursor.fetchall():
    print(dict(zip(columns, row)))
