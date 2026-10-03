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

cursor.execute("DELETE FROM SystemPhaseTemplates WHERE PhaseCategory = 2")
cursor.execute("DELETE FROM ConstructionBudgetItems WHERE PhaseCategory = 2 AND SourceType = 1")

templates = [
    (2, '1.1 Yıkım ve Hafriyat Öncesi Hazırlık', '1.1.A: Yıkım Ruhsatı, Asbest Raporu ve İzinlerin Alınması', 1),
    (2, '1.1 Yıkım ve Hafriyat Öncesi Hazırlık', '1.1.B: Mevcut Yapının Yıkılması ve Molozun Döküm Sahasına Nakliyesi', 1),

    (2, '1.2 Arazi Ölçümü ve Çevre Güvenliği', '1.2.A: Harita Mühendisi Sınır Tespiti (Aplikasyon)', 1),
    (2, '1.2 Arazi Ölçümü ve Çevre Güvenliği', '1.2.B: Şantiye Etrafının Kapatılması (Sac/Tel Çit Çekilmesi)', 1),

    (2, '1.3 Geçici Şantiye Yapıları (Yaşam Alanı)', '1.3.A: Konteyner Zemin Tesviyesi (JCB/Beko Loder ile düzeltme)', 1),
    (2, '1.3 Geçici Şantiye Yapıları (Yaşam Alanı)', '1.3.B: Şantiye Şefi ve Yönetim Ofisi Konteyneri Kurulumu', 1),
    (2, '1.3 Geçici Şantiye Yapıları (Yaşam Alanı)', '1.3.C: İşçi Yatakhane, WC ve Yemekhane Kurulumu', 1),

    (2, '1.4 Şantiye Elektriği ve Suyu', '1.4.A: Geçici Şantiye Elektrik Aboneliği ve Pano Kurulumu', 0),
    (2, '1.4 Şantiye Elektriği ve Suyu', '1.4.B: Geçici Şantiye Su Aboneliği ve Şebeke Bağlantısı', 0)
]

for t in templates:
    cursor.execute("INSERT INTO SystemPhaseTemplates (PhaseCategory, SubCategory, ItemName, IsQuoteRequired) VALUES (?, ?, ?, ?)", t)

print("DB seeded with simplified Phase 1 WBS")