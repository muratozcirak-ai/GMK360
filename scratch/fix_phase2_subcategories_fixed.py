import pyodbc

conn_str = (
    r'DRIVER={ODBC Driver 17 for SQL Server};'
    r'SERVER=(localdb)\mssqllocaldb;'
    r'DATABASE=GMK360Db;'
    r'Trusted_Connection=yes;'
)

conn = pyodbc.connect(conn_str)
cursor = conn.cursor()

# Update 2.1
cursor.execute("UPDATE SystemPhaseTemplates SET SubCategory = N'2.1 Hafriyat ve Zemin İksası (Destekleme)' WHERE PhaseCategory = 3 AND SubCategory LIKE N'%2.1 Hafriyat%'")

# Update 2.2
cursor.execute("UPDATE SystemPhaseTemplates SET SubCategory = N'2.2 Temel Altı Hazırlık ve Yalıtım (Bohçalama)' WHERE PhaseCategory = 3 AND SubCategory LIKE N'%2.2 Temel Altı%'")

# Update ConstructionBudgetItems
cursor.execute("UPDATE ConstructionBudgetItems SET SubCategory = N'2.1 Hafriyat ve Zemin İksası (Destekleme)' WHERE PhaseCategory = 3 AND SubCategory LIKE N'%2.1 Hafriyat%'")
cursor.execute("UPDATE ConstructionBudgetItems SET SubCategory = N'2.2 Temel Altı Hazırlık ve Yalıtım (Bohçalama)' WHERE PhaseCategory = 3 AND SubCategory LIKE N'%2.2 Temel Altı%'")

conn.commit()
print("Updated SubCategories successfully!")