import pyodbc

conn_str = (
    r"Driver={ODBC Driver 17 for SQL Server};"
    r"Server=(localdb)\MSSQLLocalDB;"
    r"Database=GMK360Db;"
    r"Trusted_Connection=yes;"
)

conn = pyodbc.connect(conn_str, autocommit=True)
cursor = conn.cursor()
cursor.execute("DELETE FROM ConstructionBudgetItems WHERE SourceType = 1 AND ItemName IN ('Şantiye Suyu Aboneliği', 'Şantiye Elektriği Aboneliği', 'Asbest Temizleme ve Raporu', 'Çevre Kapatması (Sac/OSB)', 'Şantiye Ofisi (Konteyner) Kurulumu', 'Kamera ve Güvenlik Sistemleri', 'Söküm İşlemleri (Hurda Ayırma)', 'Yıkım ve Hafriyat', 'Eksi Kota İnme (Kazı)', 'Grobeton Dökümü')")

# Also delete the ones with broken encoding
cursor.execute("DELETE FROM ConstructionBudgetItems WHERE SourceType = 1 AND SubCategory LIKE '%Idari%'")
cursor.execute("DELETE FROM ConstructionBudgetItems WHERE SourceType = 1 AND SubCategory LIKE '%Yikim%'")
cursor.execute("DELETE FROM ConstructionBudgetItems WHERE SourceType = 1 AND SubCategory LIKE '%Hazirligi%'")
cursor.execute("DELETE FROM ConstructionBudgetItems WHERE SourceType = 1 AND SubCategory LIKE '%Gvenlik%'")

print("Budget items cleaned")