import pyodbc
conn_str = (
    r"Driver={ODBC Driver 17 for SQL Server};"
    r"Server=(localdb)\MSSQLLocalDB;"
    r"Database=GMK360Db;"
    r"Trusted_Connection=yes;"
)
conn = pyodbc.connect(conn_str, autocommit=True)
cursor = conn.cursor()
cursor.execute("DELETE FROM ConstructionBudgetItems WHERE PhaseCategory = 2")
print("Deleted all Phase 1 budget items from the user's project")