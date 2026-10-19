using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Helpers;
using GMK360.Core.Entities;
using GMK360.Web.Models;
using GMK360.Web.Services;

namespace GMK360.Web.Controllers
{
    public class UstaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUstaWorkspaceService _ustaService;

        public UstaController(ApplicationDbContext context, IUstaWorkspaceService ustaService)
        {
            _context = context;
            _ustaService = ustaService;
        }

        // --- USTA PWA KOKPİTİ & İŞ MASASI ---
        public async Task<IActionResult> Index(int? id, string? viewMode)
        {
            var model = await _ustaService.GetUstaDashboardAsync(id);
            ViewBag.ViewMode = viewMode ?? "PWA"; // "PWA" (Usta Masası) veya "Kartvizit" (Müşteri Önizleme)
            return View(model);
        }

        // Tek Tıkla Örnek Usta Yükle (Ahmet Usta - Kadıköy)
        public async Task<IActionResult> AddSampleUsta()
        {
            await _ustaService.ResetToSampleDataAsync();
            TempData["SuccessMessage"] = "Ahmet Usta (MYK Onaylı, Kadıköy Fenerbahçe Mah.) örnek işleri, çırağı ve nalbur açık hesabı başarıyla yüklendi!";
            return RedirectToAction(nameof(Index));
        }

        // Yeni İş & Fiyat Teklifi Verme (Fatura & Stopaj Kalkanı Dahil)
        [HttpPost]
        public async Task<IActionResult> CreateJobQuote(
            string jobTitle, 
            string customerName, 
            string customerPhone, 
            string locationAddress, 
            decimal agreedAmount, 
            TaxShieldOption taxShield = TaxShieldOption.NetAgreed_TaxOnClient, 
            bool isMaterialIncluded = false, 
            string paymentMethod = "İş Bitimi Peşin / Havale", 
            string notes = "")
        {
            if (string.IsNullOrWhiteSpace(jobTitle) || agreedAmount <= 0)
            {
                TempData["ErrorMessage"] = "Lütfen iş başlığı ve geçerli bir hakediş tutarı giriniz.";
                return RedirectToAction(nameof(Index));
            }

            var job = await _ustaService.CreateJobQuoteAsync(jobTitle, customerName, customerPhone, locationAddress, agreedAmount, taxShield, isMaterialIncluded, paymentMethod, notes);

            TempData["SuccessMessage"] = $"'{job.JobTitle}' işi oluşturuldu! WhatsApp Mutabakat linkiniz hazır.";
            TempData["OpenWhatsAppJobId"] = job.Id;
            return RedirectToAction(nameof(Index));
        }

        // İş Durumu Güncelle (Teklif Verildi -> Devam Ediyor -> Tamamlandı)
        [HttpPost]
        public async Task<IActionResult> UpdateJobStatus(int jobId, string status)
        {
            await _ustaService.UpdateJobStatusAsync(jobId, status);
            TempData["SuccessMessage"] = "İş durumu güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // İşe Masraf Ekle (Malzeme, Çırak, Yol & Yemek)
        [HttpPost]
        public async Task<IActionResult> AddJobExpense(int jobId, string expenseType, decimal amount, string description)
        {
            if (amount <= 0)
            {
                TempData["ErrorMessage"] = "Lütfen geçerli bir masraf tutarı giriniz.";
                return RedirectToAction(nameof(Index));
            }

            await _ustaService.AddJobExpenseAsync(jobId, expenseType, amount, description);
            TempData["SuccessMessage"] = "Masraf işlendi, net kârınız güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // Nalbur Açık Hesap (Cari Defteri) Hareketi: Malzeme Çek / Ödeme Yap
        [HttpPost]
        public async Task<IActionResult> AddNalburTransaction(string transactionType, decimal amount, string description, int? relatedJobId)
        {
            if (amount <= 0 || string.IsNullOrWhiteSpace(description))
            {
                TempData["ErrorMessage"] = "Lütfen tutar ve açıklama giriniz.";
                return RedirectToAction(nameof(Index));
            }

            await _ustaService.AddNalburTransactionAsync(transactionType, amount, description, relatedJobId);
            TempData["SuccessMessage"] = "Nalbur açık hesap hareketi işlendi!";
            return RedirectToAction(nameof(Index));
        }

        // Çırak / Kalfa Ekle (Organik Geleceğin Ustası Kaydı)
        [HttpPost]
        public async Task<IActionResult> AddAssistant(string fullName, string phoneNumber, string role, decimal dailyWage)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phoneNumber))
            {
                TempData["ErrorMessage"] = "Çırak / Kalfa adı ve telefon numarası zorunludur.";
                return RedirectToAction(nameof(Index));
            }

            await _ustaService.AddAssistantAsync(fullName, phoneNumber, role, dailyWage);
            TempData["SuccessMessage"] = $"{fullName} ekibinize ve GMK360 Aday Usta Havuzuna kaydedildi!";
            return RedirectToAction(nameof(Index));
        }

        // Çırağa Günlük Yevmiye Yaz (İşe Bağlı Olarak)
        [HttpPost]
        public async Task<IActionResult> AddAssistantDailyWage(int assistantId, int jobId, int daysCount = 1)
        {
            if (daysCount <= 0)
            {
                TempData["ErrorMessage"] = "Geçerli bir gün sayısı giriniz.";
                return RedirectToAction(nameof(Index));
            }

            await _ustaService.AddAssistantDailyWageAsync(assistantId, jobId, daysCount);
            TempData["SuccessMessage"] = "Çırak yevmiyesi başarıyla işlendi ve iş maliyetine yansıtıldı.";
            return RedirectToAction(nameof(Index));
        }

        // Tek Düz Sabit Aidat (Tüm Şehir & Yan İlçeler Yayını) Aç / Kapat
        [HttpPost]
        public async Task<IActionResult> ToggleCityWideSubscription()
        {
            bool isActive = await _ustaService.ToggleCityWideSubscriptionAsync();
            TempData["SuccessMessage"] = isActive 
                ? "Tüm Şehir & Yan İlçeler Yayını Aktif Edildi (Aylık Sabit 250 TL Tek Aidat)." 
                : "Yayın bölgeniz sadece kendi mahallenize çekildi (%100 Ücretsiz Mod).";
            return RedirectToAction(nameof(Index));
        }

        // Resmi A4 Mutabakat Fişi & Stopaj Kalkanı Yazdır
        [HttpGet]
        public async Task<IActionResult> PrintJobSheet(int id)
        {
            var dashboard = await _ustaService.GetUstaDashboardAsync();
            var job = dashboard.Jobs.FirstOrDefault(j => j.Id == id);
            if (job == null) return NotFound();

            ViewBag.Usta = dashboard;
            return View(job);
        }

        // --- PUBLIC VE GERİYE DÖNÜK UYUMLU ESKİ METOTLAR ---

        public async Task<IActionResult> DetailBySlug(string seoSlug)
        {
            if (string.IsNullOrEmpty(seoSlug)) return NotFound();

            var parts = seoSlug.Split('-');
            if (parts.Length > 0 && int.TryParse(parts.Last(), out int id))
            {
                return await Detail(id, isFromSlug: true);
            }
            return NotFound();
        }

        public async Task<IActionResult> Detail(int id, bool isFromSlug = false)
        {
            if (!isFromSlug)
            {
                var prov = await _context.ServiceProviders
                    .Include(p => p.Areas).ThenInclude(a => a.District)
                    .FirstOrDefaultAsync(p => p.Id == id);
                
                if (prov == null)
                {
                    // Fallback: Dashboard'tan aç
                    return RedirectToAction(nameof(Index), new { id = id, viewMode = "Kartvizit" });
                }
                
                var distStr = prov.Areas?.FirstOrDefault()?.District?.Name.ToSeoUrl() ?? "bolge";
                var nameStr = prov.BusinessName.ToSeoUrl();
                var slug = $"{distStr}-{nameStr}-{prov.Id}";
                
                return RedirectPermanent($"/usta/{slug}");
            }

            var model = await _context.ServiceProviders
                .Include(p => p.Areas).ThenInclude(a => a.District)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (model == null)
            {
                return RedirectToAction(nameof(Index), new { id = id, viewMode = "Kartvizit" });
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult TalepAc()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TalepAc(JobRequest model)
        {
            return RedirectToAction("Index", "PublicRealEstate");
        }

        [HttpGet]
        public IActionResult Basvuru()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Basvuru(GMK360.Core.Entities.ServiceProvider model)
        {
            return RedirectToAction("Index", "PublicRealEstate");
        }
    }
}
