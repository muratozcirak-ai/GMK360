import pyodbc

conn_str = (
    r'DRIVER={ODBC Driver 17 for SQL Server};'
    r'SERVER=(localdb)\mssqllocaldb;'
    r'DATABASE=GMK360Db;'
    r'Trusted_Connection=yes;'
)

conn = pyodbc.connect(conn_str)
cursor = conn.cursor()

# Delete old records for phases 4, 5, 6, 7, 8, 9
cursor.execute("DELETE FROM SystemPhaseTemplates WHERE PhaseCategory IN (4,5,6,7,8,9)")

# Insert new ones
insert_sql = '''
INSERT INTO SystemPhaseTemplates (PhaseCategory, SubCategory, ItemName, IsQuoteRequired, CreatedAt, IsDeleted) VALUES 
(4, N'3.1 Kolon, Kiriş ve Döşeme', N'3.1.A: Kolon, Kiriş ve Döşeme İmalatları (Kalıp, Demir, Beton)', 0, GETDATE(), 0),
(4, N'3.2 Duvar ve Lento İşleri', N'3.2.A: Tuğla, Gazbeton veya Bims Duvar Örümleri ve Lento İşleri', 0, GETDATE(), 0),
(4, N'3.3 Özel İmalatlar', N'3.3.A: Asansör Kuyusu, Merdiven ve Çelik Konstrüksiyon İşleri (Varsa)', 0, GETDATE(), 0),

(5, N'4.1 Çatı', N'4.1.A: Çatı Konstrüksiyonu (Ahşap/Çelik) ve Kaplaması (Kiremit, Şıngıl, Membran vb.)', 0, GETDATE(), 0),
(5, N'4.2 Dış Cephe', N'4.2.A: Dış Cephe Isı Yalıtımı (Mantolama) ve Dış Cephe Boyası/Kaplaması', 0, GETDATE(), 0),
(5, N'4.3 İzolasyon', N'4.3.A: Teras, Çatı Deresi ve Tüm Islak Hacimlerin (Banyo/Balkon) Su Yalıtımları', 0, GETDATE(), 0),

(6, N'5.1 Sıva ve Boya', N'5.1.A: İç Cephe Kaba Sıva, Alçı Sıva, Kartonpiyer ve Boya İşleri', 0, GETDATE(), 0),
(6, N'5.2 Zemin Kaplamaları', N'5.2.A: Zemin Kaplamaları (Şap dökümü, Seramik, Parke, Mermer işleri)', 0, GETDATE(), 0),
(6, N'5.3 Doğrama ve Ahşap', N'5.3.A: Doğrama ve Ahşap İşleri (Dış Pencereler, İç Kapılar, Çelik Kapı, Mutfak ve Banyo Dolapları)', 0, GETDATE(), 0),

(8, N'6.1 Sıhhi Tesisat', N'6.1.A: Temiz ve Pis Su Tesisatı Altyapısı ile Vitrifiye Montajı', 0, GETDATE(), 0),
(8, N'6.2 İklimlendirme', N'6.2.A: Isıtma, Soğutma ve Havalandırma (Yerden Isıtma, Petek, Kombi veya VRF)', 0, GETDATE(), 0),
(8, N'6.3 Doğalgaz ve Yangın', N'6.3.A: Doğalgaz Tesisatı ve Yangın Tesisatı (Şaft İçi Borulama ve Kolektörler)', 0, GETDATE(), 0),

(7, N'7.1 Kuvvetli Akım', N'7.1.A: Kuvvetli Akım Tesisatı (Ana panolar, kat panoları, kablolama, priz ve aydınlatma)', 0, GETDATE(), 0),
(7, N'7.2 Zayıf Akım', N'7.2.A: Zayıf Akım Tesisatı (İnternet, Kamera, Diafon, Uydu, Yangın ihbar)', 0, GETDATE(), 0),
(7, N'7.3 Topraklama ve Paratoner', N'7.3.A: Paratoner ve Temel Dışı Topraklama Sonlandırma İşleri', 0, GETDATE(), 0),

(9, N'8.1 Altyapı Bağlantıları', N'8.1.A: Altyapı Son Bağlantıları (Belediye Rögar, Şebeke Suyu ve TEDAŞ nihai)', 0, GETDATE(), 0),
(9, N'8.2 Çevre Düzenleme', N'8.2.A: Peyzaj, Yürüyüş Yolları, Açık/Kapalı Otopark Zeminleri ve Bahçe Duvarı', 0, GETDATE(), 0),
(9, N'8.3 Temizlik', N'8.3.A: Şantiye İnce Temizliği (Teslimat öncesi profesyonel temizlik)', 0, GETDATE(), 0),
(9, N'8.4 Teslimat ve İskan', N'8.4.A: İskan (Yapı Kullanım İzin Belgesi) Harçları ve Resmi Teslim İşlemleri', 0, GETDATE(), 0);
'''
cursor.execute(insert_sql)
conn.commit()
print("Seeded database successfully via pyodbc!")