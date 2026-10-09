import pyodbc

conn_str = (
    r'DRIVER={ODBC Driver 17 for SQL Server};'
    r'SERVER=(localdb)\mssqllocaldb;'
    r'DATABASE=GMK360Db;'
    r'Trusted_Connection=yes;'
)

conn = pyodbc.connect(conn_str)
cursor = conn.cursor()

cursor.execute("IF COL_LENGTH('ConstructionProjects', 'BypassedPhases') IS NULL BEGIN ALTER TABLE ConstructionProjects ADD BypassedPhases NVARCHAR(MAX) NULL; END")

conn.commit()
print("DB updated successfully!")