# -*- coding: utf-8 -*-
import pyodbc

conn_str = (
    r"Driver={ODBC Driver 17 for SQL Server};"
    r"Server=(localdb)\MSSQLLocalDB;"
    r"Database=GMK360Db;"
    r"Trusted_Connection=yes;"
)

conn = pyodbc.connect(conn_str, autocommit=True)
cursor = conn.cursor()

cursor.execute("DELETE FROM SystemPhaseTemplates")

cursor.execute('''
INSERT INTO SystemPhaseTemplates (PhaseCategory, SubCategory, ItemName, IsQuoteRequired) VALUES 
(2, N'1.1 İdari ve Altyapı Hazırlıkları', N'Şantiye Suyu Aboneliği', 0),
(2, N'1.1 İdari ve Altyapı Hazırlıkları', N'Şantiye Elektriği Aboneliği', 0),
(2, N'1.1 İdari ve Altyapı Hazırlıkları', N'Asbest Temizleme ve Raporu', 1),
(2, N'1.2 Mobilizasyon ve Güvenlik', N'Çevre Kapatması (Sac/OSB)', 1),
(2, N'1.2 Mobilizasyon ve Güvenlik', N'Şantiye Ofisi (Konteyner) Kurulumu', 1),
(2, N'1.2 Mobilizasyon ve Güvenlik', N'Kamera ve Güvenlik Sistemleri', 1),
(2, N'1.3 Yıkım Operasyonu', N'Söküm İşlemleri (Hurda Ayırma)', 0),
(2, N'1.3 Yıkım Operasyonu', N'Yıkım ve Hafriyat', 1),
(2, N'1.4 Zemin Hazırlığı', N'Eksi Kota İnme (Kazı)', 1),
(2, N'1.4 Zemin Hazırlığı', N'Grobeton Dökümü', 1);
''')
print("DB fixed")