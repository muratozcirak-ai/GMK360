import pyodbc

conn_str = (
    r'DRIVER={ODBC Driver 17 for SQL Server};'
    r'SERVER=(localdb)\mssqllocaldb;'
    r'DATABASE=GMK360Db;'
    r'Trusted_Connection=yes;'
)

conn = pyodbc.connect(conn_str)
cursor = conn.cursor()

# Check and Add LinkedPhaseCategory to ProjectLegalDocuments
cursor.execute("IF COL_LENGTH('ProjectLegalDocuments', 'LinkedPhaseCategory') IS NULL BEGIN ALTER TABLE ProjectLegalDocuments ADD LinkedPhaseCategory INT NULL; END")

conn.commit()
print("LinkedPhaseCategory added to ProjectLegalDocuments successfully!")