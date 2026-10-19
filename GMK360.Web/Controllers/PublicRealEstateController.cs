using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using GMK360.Web.Models;

namespace GMK360.Web.Controllers
{
    [Route("Emlak")]
    public class PublicRealEstateController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PublicRealEstateController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ana Arama Portalı Girişi (Örn: gmk360.com/Emlak)
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var showcaseProperties = await _context.Properties
                .Include(p => p.Building)
                    .ThenInclude(b => b.District)
                .Include(p => p.Building)
                    .ThenInclude(b => b.Neighborhood)
                .Where(p => p.State == GMK360.Core.Entities.ListingState.Active)
                .OrderByDescending(p => p.CreatedAt)
                .Take(12)
                .ToListAsync();

            return View(showcaseProperties);
        }

        // Dinamik Kıyaslama Tuvali (Split-Compare Canvas) Arama Rotası
        // Örn: /Emlak/Ara?status=satilik&locationSlug=Kadikoy
        [HttpGet("Ara")]
        [HttpGet("{status}/{locationSlug?}")]
        public async Task<IActionResult> Search(string status = "satilik", string locationSlug = "")
        {
            ViewBag.SearchStatus = status;
            ViewBag.Location = locationSlug;
            
            var query = _context.Properties
                .Include(p => p.Building)
                    .ThenInclude(b => b.District)
                .Include(p => p.Building)
                    .ThenInclude(b => b.Neighborhood)
                .Where(p => p.State == GMK360.Core.Entities.ListingState.Active);
            
            if (!string.IsNullOrEmpty(status) && status.ToLower().Contains("kiralik"))
            {
                query = query.Where(p => p.StatusId == 2);
            }

            var results = await query.ToListAsync();
            return View("Search", results);
        }

        // İlan Detay Sayfası: Örn: /Emlak/Detay/1234/deniz-manzarali-3-1
        [HttpGet("Detay/{id}/{slug?}")]
        public async Task<IActionResult> Detail(int id, string slug = null)
        {
            var property = await _context.Properties
                .Include(p => p.Building)
                    .ThenInclude(b => b.District)
                .Include(p => p.Building)
                    .ThenInclude(b => b.Neighborhood)
                .FirstOrDefaultAsync(p => p.Id == id && p.State == GMK360.Core.Entities.ListingState.Active);

            if (property == null) return NotFound("İlan bulunamadı veya yayından kaldırılmış.");

            return View(property);
        }

        // DİNAMİK KIYASLAMA API'Sİ (Split-Compare Deck İçin AJAX Verisi)
        // 1, 2 veya 3 ilanı sayfa yenilenmeden yan yana getiren motor
        [HttpGet("Api/Karsilastir")]
        public async Task<IActionResult> GetCompareData([FromQuery] string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return Json(new { success = false, message = "Hiçbir ilan seçilmedi." });
            }

            var idList = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out int id) ? id : 0)
                .Where(id => id > 0)
                .Take(3)
                .ToList();

            var properties = await _context.Properties
                .Include(p => p.Building)
                    .ThenInclude(b => b.District)
                .Include(p => p.Building)
                    .ThenInclude(b => b.Neighborhood)
                .Where(p => idList.Contains(p.Id))
                .ToListAsync();

            var result = properties.Select(p => new
            {
                id = p.Id,
                title = p.Title ?? "İsimsiz İlan",
                price = (p.Price > 0 ? p.Price : 5500000).ToString("N0") + " ₺",
                location = $"{p.Building?.District?.Name ?? "Kadıköy"} / {p.Building?.Neighborhood?.Name ?? "Merkez"}",
                roomCount = string.IsNullOrEmpty(p.RoomCount) ? "3+1" : p.RoomCount,
                area = (p.NetArea > 0 ? p.NetArea : 115) + " m² Net",
                buildingAge = string.IsNullOrEmpty(p.BuildingAge) ? "0-5 Yıl" : p.BuildingAge,
                floor = (p.FloorNumber > 0 ? p.FloorNumber.ToString() : "3") + ". Kat",
                heating = "Doğalgaz Kombi",
                dues = "450 ₺ / Ay Aidat",
                imageUrl = "/images/default-property.jpg"
            });

            return Json(new { success = true, items = result });
        }

        // BÖLGENİZDEKİ PROJELER (Müteahhit Lansman & Ön Talep Kataloğu)
        // Hukuki zırh: 'Satış' ve 'Topraktan Satış' KELİMELERİ KESİNLİKLE GEÇMEZ!
        [HttpGet("Projeler")]
        [HttpGet("/Projeler")]
        public IActionResult Projects()
        {
            var vm = new ProjectShowcaseIndexViewModel
            {
                Projects = new List<ProjectShowcaseItem>
                {
                    new ProjectShowcaseItem
                    {
                        Id = 501,
                        ProjectName = "Mavi Çamlıca Yaşam Konakları",
                        ConstructionCompany = "Korkmaz İnşaat Taahhüt A.Ş.",
                        Location = "İstanbul / Üsküdar (Çamlıca Etekleri)",
                        ArchitecturalConcept = "Yatay Mimari, Boğaz Manzaralı 4 Katlı Akıllı Bloklar",
                        TotalBlocks = 4,
                        TotalUnits = 48,
                        UnitTypes = "2+1 Bahçe Dubleksi, 3+1 Ara Kat, 4+1 Çatı Dubleksi",
                        EstimatedCompletionDate = "2027 1. Çeyrek",
                        SocialFacilities = "Kapalı Otopark, Açık & Kapalı Yüzme Havuzu, Fitness & SPA, 7/24 Güvenlik, Çocuk Oyun Parkı",
                        StructuralSafety = "C35/45 Hazır Beton, Radye Jeneral Temel Sistemi, Deprem Yönetmeliği Tam Uyumlu",
                        RealtorAuthorityModel = "Bölgedeki Tüm Yetkili GMK360 Emlakçıları Bilgilendirmeye Yetkilidir (%2.5 Satış Ortaklığı)",
                        PreDemandCount = 38
                    },
                    new ProjectShowcaseItem
                    {
                        Id = 502,
                        ProjectName = "Moda Sahil Prestij Evleri",
                        ConstructionCompany = "Ege Yapı Mimarlık Grubu",
                        Location = "İstanbul / Kadıköy (Moda Sahil Yolu)",
                        ArchitecturalConcept = "Modern Sahil Mimarisi, Akıllı Ev Otomasyonu, Geniş Teraslar",
                        TotalBlocks = 2,
                        TotalUnits = 24,
                        UnitTypes = "3+1 Teraslı, 4+2 Dubleks",
                        EstimatedCompletionDate = "2026 4. Çeyrek",
                        SocialFacilities = "Açık Havuz, Kapalı Otopark, Sosyal Lounge, Elektrikli Araç Şarj İstasyonları",
                        StructuralSafety = "Sismik İzolatörlü Altyapı, C40 Beton Sınıfı, Jet-Grouting Zemin İyileştirme",
                        RealtorAuthorityModel = "VIP Tek Yetkili Acente Danışmanlığı",
                        PreDemandCount = 42
                    },
                    new ProjectShowcaseItem
                    {
                        Id = 503,
                        ProjectName = "Yeşil Vadi Park Villaları",
                        ConstructionCompany = "Başkent İnşaat Grubu",
                        Location = "Ankara / Çankaya (Beytepe Mevkii)",
                        ArchitecturalConcept = "Müstakil Bahçeli Müstakil Girişli Akıllı Eko-Villalar",
                        TotalBlocks = 16,
                        TotalUnits = 16,
                        UnitTypes = "5+1 Müstakil Bahçeli Villa",
                        EstimatedCompletionDate = "2027 2. Çeyrek",
                        SocialFacilities = "Güneş Enerjisi (Güneş Paneli Çatılar), Yağmur Suyu Geri Dönüşümü, Özel Bahçe, Güvenlik",
                        StructuralSafety = "Çelik Konstrüksiyon Destekli Betonarme, C35 Beton, Akıllı İzolasyon",
                        RealtorAuthorityModel = "Tüm GMK360 Gayrimenkul Danışmanlarına Açık Tanıtım",
                        PreDemandCount = 19
                    }
                }
            };

            return View(vm);
        }

        // POST: /Emlak/Projeler/TalepKaydet
        // Ön Bilgilendirme ve Görüşme Talebi (Ön Talep Formu)
        [HttpPost("Projeler/TalepKaydet")]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitProjectPreDemand(ProjectPreDemandRequest request)
        {
            TempData["ProjectSuccessMessage"] = $"Sayın {request.FullName}, {request.InterestedUnitType} tipi bağımsız bölüm için lansman öncesi ön bilgilendirme ve görüşme talebiniz kaydedilmiştir. İnşaat firması yetkilisi en kısa sürede özel randevu için sizinle iletişime geçecektir.";
            return RedirectToAction(nameof(Projects));
        }
    }
}
