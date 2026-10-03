import pyodbc

conn_str = (
    r'DRIVER={ODBC Driver 17 for SQL Server};'
    r'SERVER=(localdb)\mssqllocaldb;'
    r'DATABASE=GMK360Db;'
    r'Trusted_Connection=yes;'
)

conn = pyodbc.connect(conn_str)
cursor = conn.cursor()

# Check and Add ItemCode to SystemPhaseTemplates
cursor.execute("IF COL_LENGTH('SystemPhaseTemplates', 'ItemCode') IS NULL BEGIN ALTER TABLE SystemPhaseTemplates ADD ItemCode NVARCHAR(50) NULL; END")

# Check and Add ItemCode to ConstructionBudgetItems
cursor.execute("IF COL_LENGTH('ConstructionBudgetItems', 'ItemCode') IS NULL BEGIN ALTER TABLE ConstructionBudgetItems ADD ItemCode NVARCHAR(50) NULL; END")

conn.commit()
print("ItemCode added to tables successfully!")