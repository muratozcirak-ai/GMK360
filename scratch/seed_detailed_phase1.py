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

templates = [
    (2, '1.1 Şantiye Elektriği İşleri', '1.1.A: Elektrik Projesi Çizimi ve Kurum Onayı', 1),
    (2, '1.1 Şantiye Elektriği İşleri', '1.1.B: Geçici Şantiye Panosu, Şalter ve Malzeme Tedariği', 1),
    (2, '1.1 Şantiye Elektriği İşleri', '1.1.C: Pano Kurulumu, Kablolama ve Topraklama İşçiliği', 1),

    (2, '1.2 Şantiye Suyu İşleri', '1.2.A: İSKİ/ASKİ Geçici Su Aboneliği ve Harçlar', 0),
    (2, '1.2 Şantiye Suyu İşleri', '1.2.B: Şebeke Bağlantı Hattı Kazısı ve Boru Tedariği', 1),
    (2, '1.2 Şantiye Suyu İşleri', '1.2.C: Şantiye Su Deposu ve Hidrofor Sistemi Tedariği', 1),

    (2, '1.3 Arazi Ölçümü ve Çevre Güvenliği', '1.3.A: Sınır Tespiti ve Aplikasyon Krokisi Çıkarılması', 1),
    (2, '1.3 Arazi Ölçümü ve Çevre Güvenliği', '1.3.B: Çevre Kapatma Direkleri, Ankraj ve Beton İşçiliği', 1),
    (2, '1.3 Arazi Ölçümü ve Çevre Güvenliği', '1.3.C: Trapez Sac, OSB veya Tel Çit Malzeme Tedariği', 1),

    (2, '1.4 Geçici Şantiye Yapıları', '1.4.A: Konteyner Zemin Tesviyesi (İş Makinesi Kiralama)', 1),
    (2, '1.4 Geçici Şantiye Yapıları', '1.4.B: Şantiye Şefi ve Yönetim Ofisi Konteyneri Tedariği', 1),
    (2, '1.4 Geçici Şantiye Yapıları', '1.4.C: İşçi Yatakhane, Yemekhane ve WC/Duş Konteynerleri Tedariği', 1),
    (2, '1.4 Geçici Şantiye Yapıları', '1.4.D: Konteynerleri İndirmek İçin Mobil Vinç Hizmeti', 1),

    (2, '1.5 Yıkım ve Hafriyat Hazırlığı', '1.5.A: Mevcut Yapı Yıkım Ruhsatı ve Asbest Raporu Alınması', 1),
    (2, '1.5 Yıkım ve Hafriyat Hazırlığı', '1.5.B: Hafriyat Öncesi Döküm Sahası İzinleri ve Harç Ödemeleri', 0)
]

for t in templates:
    cursor.execute("INSERT INTO SystemPhaseTemplates (PhaseCategory, SubCategory, ItemName, IsQuoteRequired) VALUES (?, ?, ?, ?)", t)

print("DB seeded with detailed Phase 1 WBS")