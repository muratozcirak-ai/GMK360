using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GMK360.Web.Models;
using GMK360.Web.Services;

namespace GMK360.Web.Controllers
{
    public class TedarikciController : Controller
    {
        private readonly ITedarikciWorkspaceService _tedarikciService;

        public TedarikciController(ITedarikciWorkspaceService tedarikciService)
        {
            _tedarikciService = tedarikciService;
        }

        // --- NALBUR / TEDARİKÇİ MASASI & AÇIK HESAP ---
        public async Task<IActionResult> Index()
        {
            var model = await _tedarikciService.GetTedarikciDashboardAsync();
            return View(model);
        }

        // Tek Tıkla Örnek Nalbur Yükle (Özkan Yapı Market)
        public async Task<IActionResult> AddSampleData()
        {
            await _tedarikciService.ResetToSampleDataAsync();
            TempData["SuccessMessage"] = "Özkan Yapı Market (Açık Hesap Defteri, Organik Envanter ve Güvenilir Ustalar) yüklendi!";
            return RedirectToAction(nameof(Index));
        }

        // Hızlı Açık Hesap Hareketi & Organik Envanter Öğrenme
        [HttpPost]
        public async Task<IActionResult> AddFastTransaction(int customerId, string rawInput, decimal amount, string transactionType = "Malzeme Verildi (+Borç)")
        {
            if (customerId <= 0 || string.IsNullOrWhiteSpace(rawInput) || amount <= 0)
            {
                TempData["ErrorMessage"] = "Lütfen müşteri, serbest ürün açıklaması ve tutar giriniz.";
                return RedirectToAction(nameof(Index));
            }

            await _tedarikciService.AddFastTransactionAsync(customerId, rawInput, amount, transactionType);
            TempData["SuccessMessage"] = "İşlem kaydedildi ve sistem yazılan ürünü otomatik envanter hafızasına aldı!";
            return RedirectToAction(nameof(Index));
        }

        // Yeni Müşteri / Usta Açık Hesabı Aç (Merkezi Tekil Kimlik ile)
        [HttpPost]
        public async Task<IActionResult> AddCustomer(string fullName, string phoneNumber, string customerType = "Usta", decimal initialDebt = 0)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phoneNumber))
            {
                TempData["ErrorMessage"] = "İsim ve telefon numarası zorunludur.";
                return RedirectToAction(nameof(Index));
            }

            await _tedarikciService.AddCustomerAccountAsync(fullName, phoneNumber, customerType, initialDebt);
            TempData["SuccessMessage"] = $"{fullName} açık hesap defterinize ve GMK360 merkezi tekil üye ağına kaydedildi!";
            return RedirectToAction(nameof(Index));
        }

        // Hibrit Yetenek Ayarı (Satıcı / Taşeronluk Yapıyorum)
        [HttpPost]
        public async Task<IActionResult> ToggleHybrid(bool isSeller, bool isSubcontractor)
        {
            await _tedarikciService.ToggleHybridCapabilityAsync(isSeller, isSubcontractor);
            TempData["SuccessMessage"] = "Ticari yetenek ayarlarınız güncellendi!";
            return RedirectToAction(nameof(Index));
        }
    }
}
