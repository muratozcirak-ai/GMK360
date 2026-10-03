import codecs
import re

with codecs.open('GMK360.Web/Controllers/PhaseTwoController.cs', 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('PhaseTwoController', 'PhaseOneController')
content = content.replace('PhaseTwo', 'PhaseOne')
content = content.replace('BudgetPhaseCategory.TemelVeAltYapi', 'BudgetPhaseCategory.YikimVeZeminHazirligi')
content = content.replace('PhaseCategory == (BudgetPhaseCategory)3', 'PhaseCategory == (BudgetPhaseCategory)2')

# Now inject the ForceSeed method safely at the end of the class
method = '''
        [HttpGet("PhaseOne/ForceSeedRemainingPhases")]
        public async Task<IActionResult> ForceSeedRemainingPhases()
        {
            var targetCategories = new[] { 4, 5, 6, 7, 8, 9 };
            var toRemove = _context.SystemPhaseTemplates.Where(t => targetCategories.Contains((int)t.PhaseCategory)).ToList();
            _context.SystemPhaseTemplates.RemoveRange(toRemove);
            await _context.SaveChangesAsync();

            var templates = new SystemPhaseTemplate[]
            {
                new SystemPhaseTemplate { PhaseCategory = (BudgetPhaseCategory)4, SubCategory = "3.1 Kolon, Kiriş ve Döşeme", ItemName = "3.1.A: Kolon, Kiriş ve Döşeme İmalatları (Kalıp, Demir, Beton)" },
                new SystemPhaseTemplate { PhaseCategory = (BudgetPhaseCategory)4, SubCategory = "3.2 Duvar ve Lento İşleri", ItemName = "3.2.A: Tuğla, Gazbeton veya Bims Duvar Örümleri ve Lento İşleri" },
                new SystemPhaseTemplate { PhaseCategory = (BudgetPhaseCategory)4, SubCategory = "3.3 Özel İmalatlar", ItemName = "3.3.A: Asansör Kuyusu, Merdiven ve Çelik Konstrüksiyon İşleri (Varsa)" },

                new SystemPhaseTemplate { PhaseCategory = (BudgetPhaseCategory)5, SubCategory = "4.1 Çatı", ItemName = "4.1.A: Çatı Konstrüksiyonu (Ahşap/Çelik) ve Kaplaması (Kiremit, Şıngıl, Membran vb.)" },
                new SystemPhaseTemplate { PhaseCategory = (BudgetPhaseCategory)5, SubCategory = "4.2 Dış Cephe", ItemName = "4.2.A: Dış Cephe Isı Yalıtımı (Mantolama) ve Dış Cephe Boyası/Kaplaması" },
                new SystemPhaseTemplate { PhaseCategory = (BudgetPhaseCategory)5, SubCategory = "4.3 İzolasyon", ItemName = "4.3.A: Teras, Çatı Deresi ve Tüm Islak Hacimlerin (Banyo/Balkon) Su Yalıtımları" },

                new SystemPhaseTemplate { PhaseCategory = (BudgetPhaseCategory)6, SubCategory = "5.1 Sıva ve Boya", ItemName = "5.1.A: İç Cephe Kaba Sıva, Alçı Sıva, Kartonpiyer ve Boya İşleri" },
                new SystemPhaseTemplate { PhaseCategory = (BudgetPhaseCategory)6, SubCategory = "5.2 Zemin Kaplamaları", ItemName = "5.2.A: Zemin Kaplamaları (Şap dökümü, Seramik, Parke, Mermer işleri)" },
                new SystemPhaseTemplate { PhaseCategory = (BudgetPhaseCategory)6, SubCategory = "5.3 Doğrama ve Ahşap", ItemName = "5.3.A: Doğrama ve Ahşap İşleri (Dış Pencereler, İç Kapılar, Çelik Kapı, Mutfak ve Banyo Dolapları)" },

                new SystemPhaseTemplate { PhaseCategory = (BudgetPhaseCategory)8, SubCategory = "6.1 Sıhhi Tesisat", ItemName = "6.1.A: Temiz ve Pis Su Tesisatı Altyapısı ile Vitrifiye Montajı" },
                new SystemPhaseTemplate { PhaseCategory = (BudgetPhaseCategory)8, SubCategory = "6.2 İklimlendirme", ItemName = "6.2.A: Isıtma, Soğutma ve Havalandırma (Yerden Isıtma, Petek, Kombi veya VRF)" },
                new SystemPhaseTemplate { PhaseCategory = (BudgetPhaseCategory)8, SubCategory = "6.3 Doğalgaz ve Yangın", ItemName = "6.3.A: Doğalgaz Tesisatı ve Yangın Tesisatı (Şaft İçi Borulama ve Kolektörler)" },

                new SystemPhaseTemplate { PhaseCategory = (BudgetPhaseCategory)7, SubCategory = "7.1 Kuvvetli Akım", ItemName = "7.1.A: Kuvvetli Akım Tesisatı (Ana panolar, kat panoları, kablolama, priz ve aydınlatma)" },
                new SystemPhaseTemplate { PhaseCategory = (BudgetPhaseCategory)7, SubCategory = "7.2 Zayıf Akım", ItemName = "7.2.A: Zayıf Akım Tesisatı (İnternet, Kamera, Diafon, Uydu, Yangın ihbar)" },
                new SystemPhaseTemplate { PhaseCategory = (BudgetPhaseCategory)7, SubCategory = "7.3 Topraklama ve Paratoner", ItemName = "7.3.A: Paratoner ve Temel Dışı Topraklama Sonlandırma İşleri" },

                new SystemPhaseTemplate { PhaseCategory = (BudgetPhaseCategory)9, SubCategory = "8.1 Altyapı Bağlantıları", ItemName = "8.1.A: Altyapı Son Bağlantıları (Belediye Rögar, Şebeke Suyu ve TEDAŞ nihai)" },
                new SystemPhaseTemplate { PhaseCategory = (BudgetPhaseCategory)9, SubCategory = "8.2 Çevre Düzenleme", ItemName = "8.2.A: Peyzaj, Yürüyüş Yolları, Açık/Kapalı Otopark Zeminleri ve Bahçe Duvarı" },
                new SystemPhaseTemplate { PhaseCategory = (BudgetPhaseCategory)9, SubCategory = "8.3 Temizlik", ItemName = "8.3.A: Şantiye İnce Temizliği (Teslimat öncesi profesyonel temizlik)" },
                new SystemPhaseTemplate { PhaseCategory = (BudgetPhaseCategory)9, SubCategory = "8.4 Teslimat ve İskan", ItemName = "8.4.A: İskan (Yapı Kullanım İzin Belgesi) Harçları ve Resmi Teslim İşlemleri" }
            };

            foreach(var item in templates) { item.IsQuoteRequired = false; }
            _context.SystemPhaseTemplates.AddRange(templates);
            await _context.SaveChangesAsync();
            return Content("Phases 3-8 Seeded C#");
        }
'''

# Find the last closing brace of the class
match = re.search(r'}\s*}\s*$', content)
if match:
    # insert before the last two braces
    content = content[:match.start()] + method + '\n    }\n}\n'

with codecs.open('GMK360.Web/Controllers/PhaseOneController.cs', 'w', 'utf-8-sig') as f:
    f.write(content)