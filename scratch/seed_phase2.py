import pyodbc

conn_str = (
    r'DRIVER={ODBC Driver 17 for SQL Server};'
    r'SERVER=(localdb)\mssqllocaldb;'
    r'DATABASE=GMK360Db;'
    r'Trusted_Connection=yes;'
)

conn = pyodbc.connect(conn_str)
cursor = conn.cursor()

# First delete any existing phase 2 to avoid duplicates
cursor.execute("DELETE FROM SystemPhaseTemplates WHERE PhaseCategory = 3")

insert_sql = '''
INSERT INTO SystemPhaseTemplates (PhaseCategory, SubCategory, ItemName, IsQuoteRequired, CreatedAt, IsDeleted) VALUES 
(3, N'2.1 Hafriyat ve Zemin İksası', N'2.1.A: Derin Hafriyat Kazısı ve Hafriyatın Döküm Sahasına Nakliyesi', 0, GETDATE(), 0),
(3, N'2.1 Hafriyat ve Zemin İksası', N'2.1.B: Fore Kazık / Mini Kazık veya İksa İşleri', 0, GETDATE(), 0),

(3, N'2.2 Temel Altı Hazırlık ve Yalıtım', N'2.2.A: Zemin Tesviyesi ve Grobeton Dökümü', 0, GETDATE(), 0),
(3, N'2.2 Temel Altı Hazırlık ve Yalıtım', N'2.2.B: Temel Altı Su ve Isı Yalıtımı İşleri (Membran / Sürme İzolasyon)', 0, GETDATE(), 0),
(3, N'2.2 Temel Altı Hazırlık ve Yalıtım', N'2.2.C: Yalıtım Koruma Betonu veya Keçe Serimi', 0, GETDATE(), 0),

(3, N'2.3 Temel Betonarme İmalatı', N'2.3.A: Temel Altı Topraklama Ağı (Elektrik) ve Gider Boruları (Mekanik) Rezervasyonları', 0, GETDATE(), 0),
(3, N'2.3 Temel Betonarme İmalatı', N'2.3.B: Temel Kalıp Çakılması İşçiliği ve Malzemesi', 0, GETDATE(), 0),
(3, N'2.3 Temel Betonarme İmalatı', N'2.3.C: Temel Demir Donatı (Nervürlü Çelik) İşçiliği ve Malzeme Tedariği', 0, GETDATE(), 0),
(3, N'2.3 Temel Betonarme İmalatı', N'2.3.D: Temel Betonu (Hazır Beton) Tedariği ve Pompa Hizmeti', 0, GETDATE(), 0);
'''
cursor.execute(insert_sql)
conn.commit()
print("Seeded Phase 2 successfully via pyodbc!")