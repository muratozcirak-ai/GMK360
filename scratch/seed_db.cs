using System;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connStr = ""Server=(localdb)\\mssqllocaldb;Database=GMK360Db;Trusted_Connection=True;"";
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            conn.Open();
            
            // Delete old ones
            SqlCommand cmdDel = new SqlCommand(""DELETE FROM SystemPhaseTemplates WHERE PhaseCategory IN (4,5,6,7,8,9)"", conn);
            cmdDel.ExecuteNonQuery();

            // Insert new ones
            string insertSql = @""
INSERT INTO SystemPhaseTemplates (PhaseCategory, SubCategory, ItemName, IsQuoteRequired, CreatedAt, IsDeleted) VALUES 
(4, '3.1 Kolon, Kiriş ve Döşeme', '3.1.A: Kolon, Kiriş ve Döşeme İmalatları (Kalıp, Demir, Beton)', 0, GETDATE(), 0),
(4, '3.2 Duvar ve Lento İşleri', '3.2.A: Tuğla, Gazbeton veya Bims Duvar Örümleri ve Lento İşleri', 0, GETDATE(), 0),
(4, '3.3 Özel İmalatlar', '3.3.A: Asansör Kuyusu, Merdiven ve Çelik Konstrüksiyon İşleri (Varsa)', 0, GETDATE(), 0),

(5, '4.1 Çatı', '4.1.A: Çatı Konstrüksiyonu (Ahşap/Çelik) ve Kaplaması (Kiremit, Şıngıl, Membran vb.)', 0, GETDATE(), 0),
(5, '4.2 Dış Cephe', '4.2.A: Dış Cephe Isı Yalıtımı (Mantolama) ve Dış Cephe Boyası/Kaplaması', 0, GETDATE(), 0),
(5, '4.3 İzolasyon', '4.3.A: Teras, Çatı Deresi ve Tüm Islak Hacimlerin (Banyo/Balkon) Su Yalıtımları', 0, GETDATE(), 0),

(6, '5.1 Sıva ve Boya', '5.1.A: İç Cephe Kaba Sıva, Alçı Sıva, Kartonpiyer ve Boya İşleri', 0, GETDATE(), 0),
(6, '5.2 Zemin Kaplamaları', '5.2.A: Zemin Kaplamaları (Şap dökümü, Seramik, Parke, Mermer işleri)', 0, GETDATE(), 0),
(6, '5.3 Doğrama ve Ahşap', '5.3.A: Doğrama ve Ahşap İşleri (Dış Pencereler, İç Kapılar, Çelik Kapı, Mutfak ve Banyo Dolapları)', 0, GETDATE(), 0),

(8, '6.1 Sıhhi Tesisat', '6.1.A: Temiz ve Pis Su Tesisatı Altyapısı ile Vitrifiye Montajı', 0, GETDATE(), 0),
(8, '6.2 İklimlendirme', '6.2.A: Isıtma, Soğutma ve Havalandırma (Yerden Isıtma, Petek, Kombi veya VRF)', 0, GETDATE(), 0),
(8, '6.3 Doğalgaz ve Yangın', '6.3.A: Doğalgaz Tesisatı ve Yangın Tesisatı (Şaft İçi Borulama ve Kolektörler)', 0, GETDATE(), 0),

(7, '7.1 Kuvvetli Akım', '7.1.A: Kuvvetli Akım Tesisatı (Ana panolar, kat panoları, kablolama, priz ve aydınlatma)', 0, GETDATE(), 0),
(7, '7.2 Zayıf Akım', '7.2.A: Zayıf Akım Tesisatı (İnternet, Kamera, Diafon, Uydu, Yangın ihbar)', 0, GETDATE(), 0),
(7, '7.3 Topraklama ve Paratoner', '7.3.A: Paratoner ve Temel Dışı Topraklama Sonlandırma İşleri', 0, GETDATE(), 0),

(9, '8.1 Altyapı Bağlantıları', '8.1.A: Altyapı Son Bağlantıları (Belediye Rögar, Şebeke Suyu ve TEDAŞ nihai)', 0, GETDATE(), 0),
(9, '8.2 Çevre Düzenleme', '8.2.A: Peyzaj, Yürüyüş Yolları, Açık/Kapalı Otopark Zeminleri ve Bahçe Duvarı', 0, GETDATE(), 0),
(9, '8.3 Temizlik', '8.3.A: Şantiye İnce Temizliği (Teslimat öncesi profesyonel temizlik)', 0, GETDATE(), 0),
(9, '8.4 Teslimat ve İskan', '8.4.A: İskan (Yapı Kullanım İzin Belgesi) Harçları ve Resmi Teslim İşlemleri', 0, GETDATE(), 0);
"";
            SqlCommand cmdIns = new SqlCommand(insertSql, conn);
            int count = cmdIns.ExecuteNonQuery();
            Console.WriteLine($""Seeded {count} rows successfully!"");
        }
    }
}