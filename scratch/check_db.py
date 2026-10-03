import pyodbc

conn_str = (
    r"Driver={ODBC Driver 17 for SQL Server};"
    r"Server=(localdb)\MSSQLLocalDB;"
    r"Database=GMK360Db;"
    r"Trusted_Connection=yes;"
)

try:
    conn = pyodbc.connect(conn_str, autocommit=True)
    cursor = conn.cursor()
    cursor.execute("SELECT Id, SubCategory, ItemName FROM SystemPhaseTemplates")
    for row in cursor.fetchall():
        print(row)
    conn.close()
except pyodbc.Error as e:
    print(f"Error: {e}")