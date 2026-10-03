import codecs
import re

path = 'GMK360.Web/Controllers/PhaseOneController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'public async Task<IActionResult> SyncFromPool\(int id\)'
replacement = '''[HttpGet("PhaseOne/ForceSeed")]
        public IActionResult ForceSeed()
        {
            _context.SystemPhaseTemplates.RemoveRange(_context.SystemPhaseTemplates.Where(t => (int)t.PhaseCategory == 2));
            _context.SaveChanges();

            var templates = new SystemPhaseTemplate[]
            {
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.YikimVeZeminHazirligi, SubCategory = "1.1 Yıkım ve Hafriyat Öncesi Hazırlık", ItemName = "1.1.A: Yıkım Ruhsatı, Asbest Raporu ve İzinlerin Alınması", IsQuoteRequired = true },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.YikimVeZeminHazirligi, SubCategory = "1.1 Yıkım ve Hafriyat Öncesi Hazırlık", ItemName = "1.1.B: Mevcut Yapının Yıkılması ve Molozun Döküm Sahasına Nakliyesi", IsQuoteRequired = true },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.YikimVeZeminHazirligi, SubCategory = "1.2 Arazi Ölçümü ve Çevre Güvenliği", ItemName = "1.2.A: Harita Mühendisi Sınır Tespiti (Aplikasyon)", IsQuoteRequired = true },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.YikimVeZeminHazirligi, SubCategory = "1.2 Arazi Ölçümü ve Çevre Güvenliği", ItemName = "1.2.B: Şantiye Etrafının Kapatılması (Sac/Tel Çit Çekilmesi)", IsQuoteRequired = true },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.YikimVeZeminHazirligi, SubCategory = "1.3 Geçici Şantiye Yapıları (Yaşam Alanı Kurulumu)", ItemName = "1.3.A: Konteyner Zemin Tesviyesi (JCB/Beko Loder ile düzeltme)", IsQuoteRequired = true },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.YikimVeZeminHazirligi, SubCategory = "1.3 Geçici Şantiye Yapıları (Yaşam Alanı Kurulumu)", ItemName = "1.3.B: Şantiye Şefi ve Yönetim Ofisi Konteyneri Kurulumu", IsQuoteRequired = true },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.YikimVeZeminHazirligi, SubCategory = "1.3 Geçici Şantiye Yapıları (Yaşam Alanı Kurulumu)", ItemName = "1.3.C: İşçi Yatakhane, WC ve Yemekhane Kurulumu", IsQuoteRequired = true },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.YikimVeZeminHazirligi, SubCategory = "1.4 Şantiye Elektriği ve Suyu", ItemName = "1.4.A: Geçici Şantiye Elektrik Aboneliği ve Pano Kurulumu", IsQuoteRequired = false },
                new SystemPhaseTemplate { PhaseCategory = BudgetPhaseCategory.YikimVeZeminHazirligi, SubCategory = "1.4 Şantiye Elektriği ve Suyu", ItemName = "1.4.B: Geçici Şantiye Su Aboneliği ve Şebeke Bağlantısı", IsQuoteRequired = false }
            };
            _context.SystemPhaseTemplates.AddRange(templates);
            _context.SaveChanges();
            return Content("Seeded C#");
        }

        [HttpPost("PhaseOne/SyncFromPool/{id}")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SyncFromPool(int id)'''
content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)