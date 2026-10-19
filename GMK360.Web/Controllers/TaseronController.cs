using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GMK360.Web.Models;
using GMK360.Web.Services;

namespace GMK360.Web.Controllers
{
    public class TaseronController : Controller
    {
        private readonly ITaseronWorkspaceService _taseronService;

        public TaseronController(ITaseronWorkspaceService taseronService)
        {
            _taseronService = taseronService;
        }

        // --- TAŞERON KOKPİTİ & PUANTAJ MASASI ---
        public async Task<IActionResult> Index()
        {
            var model = await _taseronService.GetTaseronDashboardAsync();
            return View(model);
        }

        // Tek Tıkla Örnek Taşeron Yükle (Kaya Kaba İnşaat - 6 Kişilik Usta Ekibi)
        public async Task<IActionResult> AddSampleData()
        {
            await _taseronService.ResetToSampleDataAsync();
            TempData["SuccessMessage"] = "Kaya Taşeronluk (6 Usta, 2 Şantiye Sözleşmesi, Puantaj Çetelesi ve Kâr Pusulası) yüklendi!";
            return RedirectToAction(nameof(Index));
        }

        // Hızlı Günlük Puantaj İşle (Gelen / Gelmeyen / Yarım Gün / Fazla Mesai)
        [HttpPost]
        public async Task<IActionResult> RecordPuantaj(int workerId, double workDayFactor, int overtimeHours = 0, string siteLocation = "Şantiye", string notes = "")
        {
            await _taseronService.RecordDailyPuantajAsync(DateTime.UtcNow, workerId, workDayFactor, overtimeHours, siteLocation, notes);
            TempData["SuccessMessage"] = "Puantaj kaydedildi ve ustanın hakedişine işlendi.";
            return RedirectToAction(nameof(Index));
        }

        // Tek Tuşla: "Tüm Ekip Bugün Sahada (Tam Gün)"
        [HttpPost]
        public async Task<IActionResult> BulkApproveToday(string siteLocation = "Beyaz Konak Şantiyesi")
        {
            await _taseronService.BulkApproveAllWorkersTodayAsync(DateTime.UtcNow, siteLocation);
            TempData["SuccessMessage"] = "Tüm ekip için bugünün tam gün puantajı başarıyla onaylandı!";
            return RedirectToAction(nameof(Index));
        }

        // Cuma Harçlığı / Avans Defteri Kaydı
        [HttpPost]
        public async Task<IActionResult> AddAdvance(int workerId, decimal amount, string paymentType = "Elden Nakit Harcırah", string notes = "")
        {
            if (amount <= 0)
            {
                TempData["ErrorMessage"] = "Lütfen geçerli bir avans tutarı giriniz.";
                return RedirectToAction(nameof(Index));
            }

            await _taseronService.AddWorkerAdvanceAsync(workerId, amount, paymentType, notes);
            TempData["SuccessMessage"] = "Avans başarıyla işlendi ve ay sonu net ödenecek hakedişten düşüldü.";
            return RedirectToAction(nameof(Index));
        }

        // Ekibe Yeni Usta / Zanaatkâr Ekle
        [HttpPost]
        public async Task<IActionResult> AddWorker(string fullName, string phoneNumber, string tradeRole, decimal dailyWage)
        {
            if (string.IsNullOrWhiteSpace(fullName) || dailyWage <= 0)
            {
                TempData["ErrorMessage"] = "Usta adı ve geçerli günlük yevmiye tutarı zorunludur.";
                return RedirectToAction(nameof(Index));
            }

            await _taseronService.AddWorkerAsync(fullName, phoneNumber, tradeRole, dailyWage);
            TempData["SuccessMessage"] = $"{fullName} ekibinize ve GMK360 onaylı zanaatkâr havuzuna eklendi!";
            return RedirectToAction(nameof(Index));
        }

        // Şantiye Dönem Hakediş Talebi Oluştur (Müteahhitten Çekilen Para)
        [HttpPost]
        public async Task<IActionResult> CreateHakedisClaim(int contractId, decimal hakedisAmount, string progressNotes = "")
        {
            if (hakedisAmount <= 0)
            {
                TempData["ErrorMessage"] = "Geçerli bir hakediş tutarı giriniz.";
                return RedirectToAction(nameof(Index));
            }

            await _taseronService.CreateContractHakedisClaimAsync(contractId, hakedisAmount, progressNotes);
            TempData["SuccessMessage"] = "Yeni dönem hakediş talebi oluşturuldu! WhatsApp mutabakat linki hazır.";
            return RedirectToAction(nameof(Index));
        }

        // İskele, Direk, Makine Kiralama veya Sarf Malzeme Gideri Ekle
        [HttpPost]
        public async Task<IActionResult> AddEquipmentExpense(string itemName, string supplierName, decimal amount, string category = "İskele & Makine Kiralama")
        {
            if (string.IsNullOrWhiteSpace(itemName) || amount <= 0)
            {
                TempData["ErrorMessage"] = "Gider kalemi ve tutarı zorunludur.";
                return RedirectToAction(nameof(Index));
            }

            await _taseronService.AddEquipmentExpenseAsync(itemName, supplierName, amount, category);
            TempData["SuccessMessage"] = "Ekipman/sarf gideri işlendi, net kâr pusulanız güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // Çoklu Branş / Yetenek Aç-Kapa
        [HttpPost]
        public async Task<IActionResult> ToggleBranch(string branchName)
        {
            await _taseronService.ToggleBranchAsync(branchName);
            return RedirectToAction(nameof(Index));
        }

        // Resmî A4 Şantiye Hakediş & Puantaj Mutabakat Fişi Yazdır
        [HttpGet]
        public async Task<IActionResult> PrintHakedisSheet(int id)
        {
            var dashboard = await _taseronService.GetTaseronDashboardAsync();
            var contract = dashboard.Contracts.FirstOrDefault(c => c.Id == id) ?? dashboard.Contracts.FirstOrDefault();
            if (contract == null) return NotFound();

            ViewBag.Taseron = dashboard;
            return View(contract);
        }
    }
}
