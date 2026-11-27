using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Interfaces;
using GMK360.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GMK360.Web.Services
{
    public class KmkAdvisorResult
    {
        public string Answer { get; set; } = "";
        public string DraftText { get; set; } = "";
        public string RecommendedSlug { get; set; } = "";
        public string RecommendedTitle { get; set; } = "";
        public string RecommendedUrl { get; set; } = "";
        public string LegalDisclaimer { get; set; } = "Bu yanıt 634 sayılı KMK ve GMK360 Merkezi Mevzuat Kütüphanesi rehberliğinde sunulan ön bilgilendirmedir; kesin icra veya dava işlemi öncesi hukuk danışmanınıza teyit ettiriniz.";
        public string SourceEngine { get; set; } = "GMK360 KMK Bilgi Motoru";
        public bool IsMemberLockedRecommendation { get; set; }
    }

    /// <summary>
    /// Merkezi Articles KMK kütüphanesine dayalı (grounded) Yapay Zeka Danışma Motoru.
    /// GeminiApiKey tanımlıysa canlı Gemini AI'yı merkezi mevzuat bağlamıyla çağırır; anahtar yoksa veya API erişilemezse
    /// doğrudan merkezi KMK veri tabanından deterministik akıllı yanıt ve taslak metin üretir.
    /// </summary>
    public class KmkAiAdvisorService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGeminiAiService _geminiService;
        private readonly IConfiguration _configuration;

        public KmkAiAdvisorService(
            ApplicationDbContext context,
            IGeminiAiService geminiService,
            IConfiguration configuration)
        {
            _context = context;
            _geminiService = geminiService;
            _configuration = configuration;
        }

        public async Task<KmkAdvisorResult> AskAsync(string question, bool isMember, string? activeSlug = null)
        {
            await KmkGuideSeederService.EnsureKmkGuidesSeededAsync(_context);

            var allPublished = await _context.Articles
                .AsNoTracking()
                .Where(a => a.IsPublished)
                .ToListAsync();

            var accessible = isMember
                ? allPublished
                : allPublished.Where(a => !a.IsMembersOnly).ToList();

            var activeChapter = !string.IsNullOrWhiteSpace(activeSlug)
                ? accessible.FirstOrDefault(a => string.Equals(a.Slug, activeSlug, StringComparison.OrdinalIgnoreCase))
                : null;

            var scored = RankArticlesByQuestion(question, accessible, activeChapter);
            var topArticles = scored.Take(3).Select(x => x.Article).ToList();
            var bestMatch = topArticles.FirstOrDefault() ?? accessible.FirstOrDefault();

            string knowledgeContext = BuildKnowledgeContext(topArticles);

            // Gemini API anahtarı varsa canlı modeli merkezi mevzuat bağlamıyla çalıştır
            var apiKey = _configuration["GeminiApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                try
                {
                    var rawJson = await _geminiService.AskKmkAdvisorAsync(
                        question,
                        knowledgeContext,
                        isMember,
                        activeChapter?.Title);

                    var parsed = TryParseGeminiAdvisorJson(rawJson, bestMatch, isMember);
                    if (parsed != null)
                    {
                        return parsed;
                    }
                }
                catch
                {
                    // API kesintisinde kesintisiz yerel KMK bilgi motoruna düş
                }
            }

            return BuildLocalGroundedResponse(question, topArticles, allPublished, isMember);
        }

        private static List<(Article Article, int Score)> RankArticlesByQuestion(
            string question,
            List<Article> articles,
            Article? activeChapter)
        {
            var tokens = TokenizeTurkish(question);
            var results = new List<(Article Article, int Score)>();

            foreach (var art in articles)
            {
                int score = activeChapter != null && art.Id == activeChapter.Id ? 4 : 0;
                string titleNorm = NormalizeTurkish(art.Title);
                string tagsNorm = NormalizeTurkish(art.SeoTags ?? "");
                string bodyNorm = NormalizeTurkish(StripHtml(art.HtmlContent));

                foreach (var tk in tokens)
                {
                    if (titleNorm.Contains(tk)) score += 6;
                    if (tagsNorm.Contains(tk)) score += 4;
                    if (bodyNorm.Contains(tk)) score += 2;
                }

                results.Add((art, score));
            }

            return results
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Article.IsMembersOnly)
                .ToList();
        }

        private static string BuildKnowledgeContext(List<Article> articles)
        {
            var sb = new StringBuilder();
            foreach (var art in articles)
            {
                string plain = StripHtml(art.HtmlContent);
                if (plain.Length > 1800) plain = plain.Substring(0, 1800) + "...";
                sb.AppendLine($"[SLUG: {art.Slug}] [BAŞLIK: {art.Title}] [KATEGORİ: {art.Category}]");
                sb.AppendLine(plain);
                sb.AppendLine("---");
            }
            return sb.ToString();
        }

        private static KmkAdvisorResult? TryParseGeminiAdvisorJson(string rawJson, Article? fallbackBest, bool isMember)
        {
            if (string.IsNullOrWhiteSpace(rawJson)) return null;
            string cleaned = rawJson.Trim();
            if (cleaned.StartsWith("```"))
            {
                cleaned = Regex.Replace(cleaned, @"^```(?:json)?\s*|\s*```$", "", RegexOptions.Singleline).Trim();
            }

            using var doc = JsonDocument.Parse(cleaned);
            var root = doc.RootElement;
            if (root.TryGetProperty("error", out _)) return null;

            string answer = root.TryGetProperty("answer", out var ansEl) ? (ansEl.GetString() ?? "") : "";
            if (string.IsNullOrWhiteSpace(answer)) return null;

            string draft = root.TryGetProperty("draftText", out var drEl) ? (drEl.GetString() ?? "") : "";
            string slug = root.TryGetProperty("recommendedSlug", out var slEl) ? (slEl.GetString() ?? "") : (fallbackBest?.Slug ?? "");
            string title = root.TryGetProperty("recommendedTitle", out var ttEl) ? (ttEl.GetString() ?? "") : (fallbackBest?.Title ?? "");

            return new KmkAdvisorResult
            {
                Answer = answer,
                DraftText = draft,
                RecommendedSlug = slug,
                RecommendedTitle = title,
                RecommendedUrl = BuildTargetUrl(slug, isMember),
                SourceEngine = "GMK360 Gemini AI (KMK Bağlamlı)"
            };
        }

        private static KmkAdvisorResult BuildLocalGroundedResponse(
            string question,
            List<Article> topAccessible,
            List<Article> allArticles,
            bool isMember)
        {
            string q = NormalizeTurkish(question);
            var best = topAccessible.FirstOrDefault() ?? allArticles.FirstOrDefault();

            string answer;
            string draft = "";

            if (q.Contains("cati") || q.Contains("blok") || q.Contains("asansor") || q.Contains("69"))
            {
                best = FindBestBySlug(topAccessible, allArticles, "kat-mulkiyeti-blok-giderleri-kmk-69", best);
                answer = "634 sayılı KMK Madde 69 (Toplu Yapı Hükümleri) uyarınca; çok bloklu sitelerde sadece tek bir bloğu ilgilendiren ortak yerlerin (o bloğun çatısı, dış cephesi, asansörü, merdiven boşluğu) bakım ve onarım giderleri kural olarak yalnızca o bloktaki kat malikleri tarafından arsa payı/yönetim planı esasına göre karşılanır. Diğer bloklara borç çıkarılmaz. İstisna olarak tapudaki Site Yönetim Planı'nda 'tüm blokların çatı giderleri site genelinden karşılanır' yazıyorsa tüm site katılır.";
                if (q.Contains("taslak") || q.Contains("karar") || q.Contains("yaz") || q.Contains("ornek"))
                {
                    draft = "Site Yönetim Planı'nda aksi hüküm bulunmadığından KMK Md. 69 uyarınca ilgili blok ortak alan onarım işinin alınan en uygun teklif bedeliyle yaptırılmasına ve bu demirbaş giderinin yalnızca ilgili blok kat maliklerinden arsa payları / eşit pay esas alınarak tahsil edilmesine karar verilmiştir.";
                }
            }
            else if (q.Contains("defter") || q.Contains("yapistir") || q.Contains("noter") || q.Contains("mutaharrik") || q.Contains("muteharrik") || q.Contains("cikti") || q.Contains("imza") || q.Contains("daksil"))
            {
                best = FindBestBySlug(topAccessible, allArticles, "apartman-karar-defteri-nasil-tutulur", best);
                answer = "KMK Md. 32'ye göre kararların her sayfası noter tasdikli karar defterine işlenmesi ve toplantıda bulunan maliklerce ıslak imzalanması şarttır; tek başına dijital e-defter henüz yeterli değildir. Bilgisayar çıktısını ciltli deftere yapıştırmak uygulamada geçerlidir ancak atılan imzaların yarısı yapıştırılan kağıda, yarısı doğrudan defter yaprağına taşacak (bütünleşik) şekilde atılmalı ve köşedeki noter damgası kapanmamalıdır. En sorunsuz yol ise yıl başında boş A4 sayfalarını notere 'Müteharrik Yaprak Karar Defteri' olarak damgalatıp doğrudan üzerine yazdırmaktır. Ayrıca her yıl Ocak ayı sonuna kadar notere kapanış tasdiki zorunludur.";
            }
            else if (q.Contains("aidat") || q.Contains("icra") || q.Contains("ihtar") || q.Contains("gecikme") || q.Contains("kvkk") || q.Contains("borclu") || q.Contains("isletme projesi"))
            {
                best = FindBestBySlug(topAccessible, allArticles, "aidat-belirleme-isletme-projesi-ve-icra", best);
                answer = "KMK Md. 37 uyarınca İşletme Projesi maliklere tebliğ edildikten sonra 7 gün içinde itiraz edilmezse kesinleşir ve İcra İflas Kanunu Md. 68/1 anlamında resmi belge sayılır. Ödenmeyen aidatlar için KMK Md. 20 gereği aylık %5 gecikme tazminatı uygulanır. Kiracı aidattan kira bedeliyle sınırlı müteselsil sorumludur; ödemezse tapu malikine başvurulabilir. KVKK gereği borçluların isim/daire bilgisi asla apartman panosuna veya toplu WhatsApp grubuna yazılamaz; sadece Yönetici, Azalar ve Denetçi görebilir.";
                if (q.Contains("taslak") || q.Contains("ihtar") || q.Contains("ornek") || q.Contains("yaz"))
                {
                    draft = "KONU: Kesinleşmiş İşletme Projesi Aidat Borcu İhtarı (KMK Md. 20 & 37)\nSayın [Adı Soyadı - ... Blok Daire ...],\nKesinleşmiş işletme projesi uyarınca tarafınıza tahakkuk eden toplam ... TL aidat borcunun işbu ihtarın tebliğinden itibaren 7 gün içinde site banka hesabına ödenmesini; aksi halde KMK Md. 20 gereği aylık %5 gecikme tazminatı ile ilamsız icra takibi başlatılacağını ihtaren bildiririz.";
                }
            }
            else if (q.Contains("toplanti") || q.Contains("cagri") || q.Contains("vekalet") || q.Contains("yetersayi") || q.Contains("cogunluk") || q.Contains("4/5") || q.Contains("oy orani"))
            {
                best = FindBestBySlug(topAccessible, allArticles, isMember ? "kmk-konulara-gore-oy-oranlari-ve-ornek-kararlar" : "kmk-yeni-yonetici-ilk-adimlar", best);
                answer = "KMK Md. 29-31'e göre olağanüstü toplantılarda çağrı toplantıdan en az 15 gün önce imza karşılığı veya taahhütlü yapılmalıdır. 1. toplantıda tüm maliklerin sayı ve arsa payı çoğunluğu (%50+1) aranır; sağlanamazsa en az 7 gün (en fazla 15 gün) sonra yapılan 2. toplantıda katılanların salt çoğunluğu ile karar alınır. Bir kişi oy sayısının %5'inden fazlası için vekil olamaz (40 ve altı dairede en fazla 2 vekalet). Yönetici seçimi ve aidat kararları %50+1; dış cephe renk/mimari değişiklik ve Yönetim Planı değişikliği ise 4/5 (%80) çoğunluk gerektirir.";
                if (q.Contains("taslak") || q.Contains("karar") || q.Contains("yonetici"))
                {
                    draft = "Kat Malikleri Kurulumuz .../.../2026 tarihinde KMK Md. 34 gereği sayı ve arsa payı çoğunluğuyla toplanarak Daire ... maliki [Adı Soyadı]'nı 1 yıl süreyle yönetici seçmiş; banka hesabı açma, vergi/SGK işlemleri ve aidat icra takibi başlatma yetkisi vermiştir.";
                }
            }
            else if (q.Contains("kiraci") || q.Contains("tahliye") || q.Contains("tufe") || q.Contains("taahhut") || q.Contains("kira"))
            {
                best = FindBestBySlug(topAccessible, allArticles, isMember ? "kiraci-malik-haklari-tufe-ve-tahliye" : "aidat-belirleme-isletme-projesi-ve-icra", best);
                answer = "TBK ve KMK ayrımına göre temizlik, ortak elektrik/su, görevli maaşı gibi günlük işletme giderleri oturan kiracıya; çatı yenileme, mantolama, asansör motor değişimi ve kıdem tazminatı fonu gibi demirbaş giderleri mülk sahibine aittir. Konut kiralarında artış sınırı 12 aylık TÜFE ortalamasıdır. Tahliye taahhütnamesi ancak mülkün teslim tarihinden sonraki bir günde imzalanmışsa geçerlidir ve kira yılı içinde 2 haklı ihtar tahliye hakkı doğurur (dava öncesi arabuluculuk zorunludur).";
            }
            else if (q.Contains("denetim") || q.Contains("kidem") || q.Contains("fatura") || q.Contains("kapici") || q.Contains("yeni yonetici"))
            {
                best = FindBestBySlug(topAccessible, allArticles, isMember ? "denetim-kurulu-gider-belgeleme-ve-personel" : "kmk-yeni-yonetici-ilk-adimlar", best);
                answer = "Yeni yönetici göreve başlarken mutlaka eski yönetimden Karar Defteri, kasa/banka mevcudu ve demirbaşları tutanakla devralmalı, potansiyel vergi numarası ile site adına banka hesabı açtırmalıdır. Tüm giderler fatura/fişle belgelenmeli ve önemli alımlar Karar Defteri sayfa/karar numarasıyla ilişkilendirilmelidir. Denetim Kurulu KMK Md. 41 gereği en az 3 ayda bir defter ve hesapları kontrol edip genel kurula rapor sunar.";
            }
            else
            {
                var summary = best != null ? StripHtml(best.HtmlContent) : "";
                if (summary.Length > 420) summary = summary.Substring(0, 420).TrimEnd() + "...";
                answer = best != null
                    ? $"GMK360 Merkezi KMK Mevzuat Kütüphanesi'nde bu sorunuzla en ilişkili rehber '{best.Title}' başlığıdır: {summary}"
                    : "634 sayılı Kat Mülkiyeti Kanunu çerçevesinde karar defteri, aidat/icra, toplantı yeter sayısı, blok giderleri (Md. 69) veya kiracı hakları hakkında spesifik sorunuzu yazabilirsiniz.";
            }

            bool lockedRec = best != null && best.IsMembersOnly && !isMember;
            string slugOut = best?.Slug ?? "";
            return new KmkAdvisorResult
            {
                Answer = answer,
                DraftText = draft,
                RecommendedSlug = slugOut,
                RecommendedTitle = best?.Title ?? "KMK Mevzuat Rehberi",
                RecommendedUrl = string.IsNullOrWhiteSpace(slugOut) ? "/Mevzuat" : BuildTargetUrl(slugOut, isMember),
                IsMemberLockedRecommendation = lockedRec,
                SourceEngine = "GMK360 KMK Bilgi Motoru"
            };
        }

        private static Article? FindBestBySlug(List<Article> topAccessible, List<Article> allArticles, string targetSlug, Article? fallback)
        {
            return topAccessible.FirstOrDefault(a => a.Slug == targetSlug)
                ?? allArticles.FirstOrDefault(a => a.Slug == targetSlug)
                ?? fallback;
        }

        private static string BuildTargetUrl(string slug, bool isMember)
        {
            return isMember
                ? $"/BuildingManager/MevzuatRehberi?slug={slug}"
                : $"/Mevzuat/{slug}";
        }

        private static string StripHtml(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return "";
            var plain = Regex.Replace(html, "<.*?>", " ");
            plain = WebUtility.HtmlDecode(plain);
            return Regex.Replace(plain, @"\s+", " ").Trim();
        }

        private static List<string> TokenizeTurkish(string text)
        {
            var norm = NormalizeTurkish(text);
            return Regex.Split(norm, @"[^a-z0-9]+")
                .Where(t => t.Length >= 2)
                .Distinct()
                .ToList();
        }

        private static string NormalizeTurkish(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            return input.Trim().ToLowerInvariant()
                .Replace("ç", "c").Replace("ğ", "g").Replace("ı", "i")
                .Replace("İ", "i").Replace("ö", "o").Replace("ş", "s").Replace("ü", "u");
        }
    }
}
