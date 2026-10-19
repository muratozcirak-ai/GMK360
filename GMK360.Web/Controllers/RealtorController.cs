using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GMK360.Data.Contexts;
using GMK360.Web.Models;
using GMK360.Web.Services;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class RealtorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRealtorWorkspaceService _realtorService;

        public RealtorController(ApplicationDbContext context, IRealtorWorkspaceService realtorService)
        {
            _context = context;
            _realtorService = realtorService;
        }

        // GET: /Realtor
        // Mahalle Emlakçısı Masası & Mega Broker Yönetim Kokpiti
        public async Task<IActionResult> Index()
        {
            var model = await _realtorService.GetDashboardAsync();
            return View(model);
        }

        // POST: /Realtor/SaveNegotiationNote
        // "Alıcıya Ne Dedim? / Satıcıya Ne Dedim?" Gizli Pazarlık Defteri Kaydı
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveNegotiationNote(
            string propertyTitle, string buyerName, string buyerPhone,
            decimal buyerOfferedPrice, string whatIToldBuyer,
            decimal sellerBottomPrice, string whatIToldSeller, string secretNotes)
        {
            await _realtorService.AddNegotiationNoteAsync(
                propertyTitle, buyerName, buyerPhone, buyerOfferedPrice,
                whatIToldBuyer, sellerBottomPrice, whatIToldSeller, secretNotes);

            TempData["SuccessMessage"] = "Gizli pazarlık ve görüşme notunuz şifreli hafızaya kaydedildi.";
            return RedirectToAction(nameof(Index), new { tab = "negotiation" });
        }

        // POST: /Realtor/SaveShowing
        // Yer Gösterme ve Saha Randevusu Kaydı
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveShowing(
            DateTime appointmentDate, string clientName, string clientPhone,
            string propertyTitle, string location, string feedback, bool interestedInRenovation)
        {
            await _realtorService.AddShowingAppointmentAsync(
                appointmentDate, clientName, clientPhone,
                propertyTitle, location, feedback, interestedInRenovation);

            TempData["SuccessMessage"] = "Yer gösterme randevusu ve müşteri notları işlendi.";
            return RedirectToAction(nameof(Index), new { tab = "showing" });
        }

        // POST: /Realtor/CreateListing
        // Sınırsız & Kotasız İlan Ekleme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateListing(
            string title, string listingType, string location,
            decimal price, string roomCount, int areaM2, string assignedConsultant)
        {
            await _realtorService.AddListingAsync(
                title, listingType, location, price, roomCount, areaM2, assignedConsultant);

            TempData["SuccessMessage"] = "Yeni portföy sınırsız vitrine başarıyla eklendi.";
            return RedirectToAction(nameof(Index), new { tab = "listings" });
        }

        // POST: /Realtor/AssignConsultant
        // Mega Broker: Portföyü Danışmana Atama
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignConsultant(int listingId, string consultantName)
        {
            await _realtorService.AssignConsultantToListingAsync(listingId, consultantName);
            TempData["SuccessMessage"] = $"Portföy başarıyla {consultantName} isimli danışmana devredildi.";
            return RedirectToAction(nameof(Index), new { tab = "broker" });
        }

        // POST: /Realtor/AddReferralLandlord
        // Kazan-Kazan: Daire Sahibini Sisteme Katma (4-5 Müşteride Bedava + Pasif Gelir)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReferralLandlord(string landlordName, string propertyAddress)
        {
            await _realtorService.AddReferredLandlordAsync(landlordName, propertyAddress);
            TempData["SuccessMessage"] = "Mülk sahibi referans havuzunuza eklendi! Pasif gelir kotanız güncellendi.";
            return RedirectToAction(nameof(Index), new { tab = "passive" });
        }

        // GET: /Realtor/GetFestivalGreeting
        [HttpGet]
        public IActionResult GetFestivalGreeting(string clientName, string brokerName, string agencyName)
        {
            var text = _realtorService.GenerateFestivalGreetingMessage(clientName, brokerName, agencyName);
            return Json(new { success = true, text = text });
        }

        // GET: /Realtor/GetTadilatQuote
        // Emlakçı Danışmanlık: Daireye 3 Alternatifli Tadilat Maliyeti
        [HttpGet]
        public IActionResult GetTadilatQuote(string propertyTitle, decimal m2)
        {
            var text = _realtorService.GenerateTadilatAdvisoryText(propertyTitle, m2);
            return Json(new { success = true, text = text });
        }

        // GET: /Realtor/BrokerHub
        // Mega Broker & Çok Danışmanlı Ofis Kokpiti
        public async Task<IActionResult> BrokerHub()
        {
            var model = await _realtorService.GetDashboardAsync();
            return View(model);
        }

        // GET: /Realtor/MultiPost
        public IActionResult MultiPost()
        {
            return View();
        }
    }
}
