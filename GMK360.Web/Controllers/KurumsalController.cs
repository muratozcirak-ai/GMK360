using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GMK360.Web.Models;
using GMK360.Web.Services;

namespace GMK360.Web.Controllers
{
    public class KurumsalController : Controller
    {
        private readonly IKurumsalB2BWorkspaceService _kurumsalService;

        public KurumsalController(IKurumsalB2BWorkspaceService kurumsalService)
        {
            _kurumsalService = kurumsalService;
        }

        // --- KURUMSAL B2B ÇÖZÜM ORTAKLARI & SANAYİ MASASI ---
        public async Task<IActionResult> Index()
        {
            var model = await _kurumsalService.GetDashboardAsync();
            return View(model);
        }

        // Demo Verileri Yenile
        public async Task<IActionResult> AddSampleData()
        {
            await _kurumsalService.ResetToSampleDataAsync();
            TempData["SuccessMessage"] = "Kurumsal B2B Endüstriyel Tedarikçiler, Yapı Denetim ve Hukuk Masası verileri yüklendi!";
            return RedirectToAction(nameof(Index));
        }

        // Şantiye DNA İhalesine Fiyat Teklifi Ver
        [HttpPost]
        public async Task<IActionResult> SubmitTenderBid(int tenderId, string bidderCompany, decimal bidAmount, string notes = "")
        {
            if (tenderId <= 0 || string.IsNullOrWhiteSpace(bidderCompany) || bidAmount <= 0)
            {
                TempData["ErrorMessage"] = "Lütfen firma adı ve geçerli teklif tutarını giriniz.";
                return RedirectToAction(nameof(Index));
            }

            await _kurumsalService.SubmitDnaTenderBidAsync(tenderId, bidderCompany, bidAmount, notes);
            TempData["SuccessMessage"] = "İhale teklifiniz müteahhit firmanın DNA paneline iletildi!";
            return RedirectToAction(nameof(Index));
        }
    }
}
