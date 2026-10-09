import pyodbc

conn_str = (
    r'DRIVER={ODBC Driver 17 for SQL Server};'
    r'SERVER=(localdb)\mssqllocaldb;'
    r'DATABASE=GMK360Db;'
    r'Trusted_Connection=yes;'
)

conn = pyodbc.connect(conn_str)
cursor = conn.cursor()

cursor.execute("IF COL_LENGTH('AgencyPhonebooks', 'DeductMealCost') IS NULL BEGIN ALTER TABLE AgencyPhonebooks ADD DeductMealCost BIT NOT NULL DEFAULT 0; END")
cursor.execute("IF COL_LENGTH('ConstructionBudgetItems', 'EstimatedMaterialCost') IS NULL BEGIN ALTER TABLE ConstructionBudgetItems ADD EstimatedMaterialCost DECIMAL(18,2) NULL; END")
cursor.execute("IF COL_LENGTH('ConstructionBudgetItems', 'EstimatedLaborCost') IS NULL BEGIN ALTER TABLE ConstructionBudgetItems ADD EstimatedLaborCost DECIMAL(18,2) NULL; END")

conn.commit()
print("DB updated successfully!")