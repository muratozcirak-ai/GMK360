import pyodbc
conn_str = (
    r"Driver={ODBC Driver 17 for SQL Server};"
    r"Server=(localdb)\MSSQLLocalDB;"
    r"Database=GMK360Db;"
    r"Trusted_Connection=yes;"
)
conn = pyodbc.connect(conn_str)
cursor = conn.cursor()
cursor.execute("SELECT SubCategory, ItemName FROM SystemPhaseTemplates WHERE PhaseCategory = 2 ORDER BY SubCategory, ItemName")
for row in cursor.fetchall():
    print(f"[{row.SubCategory}] - {row.ItemName}")