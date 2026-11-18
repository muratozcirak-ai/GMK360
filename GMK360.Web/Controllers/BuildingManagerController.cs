using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using System.Security.Claims;
using GMK360.Core.Interfaces;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class BuildingManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBuildingManagementService _buildingManagementService;

        public BuildingManagerController(ApplicationDbContext context, IBuildingManagementService buildingManagementService)
        {
            _context = context;
            _buildingManagementService = buildingManagementService;
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private bool IsAdminOrSuperAdmin()
        {
            return User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
        }

        private bool CanAccessBuilding(Building? building, string? userId)
        {
            if (building == null) return false;
            if (IsAdminOrSuperAdmin()) return true;
            return !string.IsNullOrEmpty(userId) &&
                   (building.ManagerUserId == userId || building.CreatedByUserId == userId);
        }

        private async Task<Building?> GetManagedBuildingAsync(int buildingId)
        {
            var userId = GetUserId();
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == buildingId);
            return CanAccessBuilding(building, userId) ? building : null;
        }

        // 1. Yönettiğim Binalar & Genel Kumanda Merkezi
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var query = _context.Buildings
                .Include(b => b.Units)
                    .ThenInclude(u => u.Debts)
                        .ThenInclude(d => d.BuildingExpense)
                .Include(b => b.Expenses)
                .AsQueryable();

            if (!User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                query = query.Where(b => b.ManagerUserId == userId || b.CreatedByUserId == userId);
            }

            var buildings = await query.ToListAsync();

            // Mükerrer tohum/test binalarını tekilleştir (Aynı isimli olanlardan en zengin birimli olanı tek satır tut)
            buildings = buildings
                .GroupBy(b => (b.Name ?? string.Empty).Trim().ToLowerInvariant())
                .Select(g => g.OrderByDescending(x => x.Units?.Count ?? 0).ThenByDescending(x => x.Id).First())
                .ToList();

            // Portföy Çapında Genel İstatistikler
            var allUnits = buildings.SelectMany(b => b.Units ?? new List<BuildingUnit>()).ToList();
            var allDebts = allUnits.SelectMany(u => u.Debts ?? new List<UnitDebt>()).ToList();

            ViewBag.TotalBuildings = buildings.Count;
            ViewBag.TotalUnits = allUnits.Count;
            ViewBag.ResidentialUnits = allUnits.Count(u => u.Category != "Dukkan");
            ViewBag.CommercialUnits = allUnits.Count(u => u.Category == "Dukkan");
            ViewBag.OccupiedUnits = allUnits.Count(u => !u.IsEmpty);
            ViewBag.EmptyUnits = allUnits.Count(u => u.IsEmpty);
            ViewBag.TotalPaid = allDebts.Where(d => d.IsPaid).Sum(d => d.Amount);
            ViewBag.TotalUnpaid = allDebts.Where(d => !d.IsPaid).Sum(d => d.Amount);
            ViewBag.TotalExpenses = buildings.SelectMany(b => b.Expenses ?? new List<BuildingExpense>()).Sum(e => e.TotalAmount);
            ViewBag.PendingTicketsCount = await _context.SupportTickets
                .CountAsync(t => t.ContextType == "Building" && t.Status != "Closed" && t.Status != "Tamamlandı");

            return View(buildings);
        }

        // Tek Tıkla Örnek Site Yükleme (Demo için 12 Bağımsız Bölüm + Demirbaş & Aidat Çetelesi)
        [HttpPost]
        public async Task<IActionResult> AddSampleBuilding()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Zaten demo bina eklenmiş mi kontrol et
            var existingDemo = await _context.Buildings.FirstOrDefaultAsync(b => (b.ManagerUserId == userId || b.CreatedByUserId == userId) && b.Name.Contains("Kardelen Konutları"));
            if (existingDemo != null)
            {
                TempData["SuccessMessage"] = "Örnek 'Kardelen Konutları' sitesi zaten listenizde mevcut. Yönetim Masası'ndan inceleyebilirsiniz.";
                return RedirectToAction(nameof(Index));
            }

            // 1. Bina Oluştur
            var building = new Building
            {
                Name = "Kardelen Konutları - A Blok",
                BlockName = "A Blok",
                HasBlock = true,
                CityId = 34,
                DistrictId = 1,
                NeighborhoodId = 1,
                StreetName = "Bağdat Caddesi",
                BuildingNumber = "44/A",
                TotalUnits = 12,
                TotalFloors = 6,
                ManagerUserId = userId,
                CreatedByUserId = userId,
                CreatedAt = DateTime.Now,
                OnboardingStep = 3,
                IsApproved = true
            };

            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

            // 2. 12 Adet Bağımsız Bölüm (10 Daire + 2 Zemin Dükkan)
            var sampleUnits = new List<BuildingUnit>
            {
                // Zemin Dükkanlar (Ticari / Muafiyetli)
                new BuildingUnit { BuildingId = building.Id, DoorNumber = "Dükkan 1", FloorLevel = 0, FloorName = "Zemin Kat", Category = "Dukkan", RoomLayout = "Ticari / Mağaza", OwnerName = "Ahmet Emlak Yatırım A.Ş.", OwnerPhone = "0532 100 2001", TenantName = "Eczane Kardelen", TenantPhone = "0532 555 1001", IsEmpty = false, GrossSquareMeters = 180, NetSquareMeters = 160 },
                new BuildingUnit { BuildingId = building.Id, DoorNumber = "Dükkan 2", FloorLevel = 0, FloorName = "Zemin Kat", Category = "Dukkan", RoomLayout = "Ticari / Kafe", OwnerName = "Murat Akın (Malik)", OwnerPhone = "0533 100 2002", OwnerUserId = userId, TenantName = "Kardelen Fırın & Cafe", TenantPhone = "0533 555 1002", IsEmpty = false, GrossSquareMeters = 140, NetSquareMeters = 120 },

                // Daireler (1 - 10)
                new BuildingUnit { BuildingId = building.Id, DoorNumber = "Daire 1", FloorLevel = 1, FloorName = "1. Kat", Category = "Daire", RoomLayout = "2+1", OwnerName = "Mehmet Demir (Ev Sahibi)", OwnerPhone = "0542 111 0001", TenantName = "Caner Yılmaz (Kiracı)", TenantPhone = "0542 999 0001", IsEmpty = false, GrossSquareMeters = 95, NetSquareMeters = 82 },
                new BuildingUnit { BuildingId = building.Id, DoorNumber = "Daire 2", FloorLevel = 1, FloorName = "1. Kat", Category = "Daire", RoomLayout = "2+1", OwnerName = "Fatma Kaya (Ev Sahibi)", OwnerPhone = "0542 111 0002", TenantName = "Selin Erdem (Kiracı)", TenantPhone = "0542 999 0002", IsEmpty = false, GrossSquareMeters = 95, NetSquareMeters = 82 },
                new BuildingUnit { BuildingId = building.Id, DoorNumber = "Daire 3", FloorLevel = 2, FloorName = "2. Kat", Category = "Daire", RoomLayout = "3+1", OwnerName = "Hasan Öztürk (Kendisi Oturuyor)", OwnerPhone = "0542 111 0003", IsEmpty = false, GrossSquareMeters = 125, NetSquareMeters = 108 },
                new BuildingUnit { BuildingId = building.Id, DoorNumber = "Daire 4", FloorLevel = 2, FloorName = "2. Kat", Category = "Daire", RoomLayout = "3+1", OwnerName = "Ayşe Yıldız", OwnerPhone = "0542 111 0004", TenantName = "Burak Koç (Kiracı)", TenantPhone = "0542 999 0004", IsEmpty = false, GrossSquareMeters = 125, NetSquareMeters = 108 },
                new BuildingUnit { BuildingId = building.Id, DoorNumber = "Daire 5", FloorLevel = 3, FloorName = "3. Kat", Category = "Daire", RoomLayout = "3+1", OwnerName = "Murat Akın (Yatırımcı Malik)", OwnerPhone = "0533 100 2002", OwnerUserId = userId, TenantName = "Oğuz Şen (Kiracı)", TenantPhone = "0542 999 0005", IsEmpty = false, GrossSquareMeters = 125, NetSquareMeters = 108 },
                new BuildingUnit { BuildingId = building.Id, DoorNumber = "Daire 6", FloorLevel = 3, FloorName = "3. Kat", Category = "Daire", RoomLayout = "3+1", OwnerName = "Murat Akın (Yatırımcı Malik)", OwnerPhone = "0533 100 2002", OwnerUserId = userId, TenantName = "Emre Kurt (Kiracı)", TenantPhone = "0542 999 0006", IsEmpty = false, GrossSquareMeters = 125, NetSquareMeters = 108 },
                new BuildingUnit { BuildingId = building.Id, DoorNumber = "Daire 7", FloorLevel = 4, FloorName = "4. Kat", Category = "Daire", RoomLayout = "3+1", OwnerName = "Zeynep Çelik", OwnerPhone = "0542 111 0007", TenantName = "Derya Aydın (Kiracı)", TenantPhone = "0542 999 0007", IsEmpty = false, GrossSquareMeters = 125, NetSquareMeters = 108 },
                new BuildingUnit { BuildingId = building.Id, DoorNumber = "Daire 8", FloorLevel = 4, FloorName = "4. Kat", Category = "Daire", RoomLayout = "3+1", OwnerName = "Ali Rıza Bey (Malik)", OwnerPhone = "0542 111 0008", IsEmpty = true, GrossSquareMeters = 125, NetSquareMeters = 108 },
                new BuildingUnit { BuildingId = building.Id, DoorNumber = "Daire 9", FloorLevel = 5, FloorName = "5. Kat", Category = "Daire", RoomLayout = "4+1 Dubleks", OwnerName = "Kemal Sunal", OwnerPhone = "0542 111 0009", TenantName = "Deniz Aras (Kiracı)", TenantPhone = "0542 999 0009", IsEmpty = false, GrossSquareMeters = 185, NetSquareMeters = 160 },
                new BuildingUnit { BuildingId = building.Id, DoorNumber = "Daire 10", FloorLevel = 5, FloorName = "5. Kat", Category = "Daire", RoomLayout = "4+1 Dubleks", OwnerName = "Hülya Avşar", OwnerPhone = "0542 111 0010", TenantName = "Kerem Alptekin (Kiracı)", TenantPhone = "0542 999 0010", IsEmpty = false, GrossSquareMeters = 185, NetSquareMeters = 160 }
            };

            _context.BuildingUnits.AddRange(sampleUnits);
            await _context.SaveChangesAsync();

            // 3. Örnek Giderler (Demirbaş vs İşletme)
            var expRoof = new BuildingExpense
            {
                BuildingId = building.Id,
                Description = "Çatı İzolasyon ve Kiremit Aktarma (Demirbaş Onarım)",
                TotalAmount = 36000m,
                PayerType = "Owner",
                ScopeType = ExpenseScopeType.Building,
                ExpenseType = BuildingExpenseType.CapitalExpenditure,
                Date = DateTime.Today.AddDays(-10)
            };

            var expElec = new BuildingExpense
            {
                BuildingId = building.Id,
                Description = "Ekim Ayı Merdiven Elektriği & Ortak Alan Temizliği",
                TotalAmount = 6000m,
                PayerType = "Tenant",
                ScopeType = ExpenseScopeType.Building,
                ExpenseType = BuildingExpenseType.Operational,
                Date = DateTime.Today.AddDays(-2)
            };

            var expElev = new BuildingExpense
            {
                BuildingId = building.Id,
                Description = "Asansör Periyodik Yeşil Etiket Bakım & Yağlama",
                TotalAmount = 3600m,
                PayerType = "Tenant",
                ScopeType = ExpenseScopeType.Building,
                ExpenseType = BuildingExpenseType.Operational,
                Date = DateTime.Today.AddDays(-1)
            };

            _context.BuildingExpenses.AddRange(expRoof, expElec, expElev);
            await _context.SaveChangesAsync();

            // 4. Borç Dağıtımı (Çatı maliklere, elektrik herkese, asansör sadece dairelere - dükkanlar muaf!)
            decimal roofShare = 3000m; // 36.000 / 12
            decimal elecShare = 500m;  // 6.000 / 12
            decimal elevShare = 360m;  // 3.600 / 10

            int idx = 0;
            foreach (var unit in sampleUnits)
            {
                idx++;
                bool isShop = unit.Category == "Dukkan";

                // Çatı (Demirbaş - Ev Sahipleri)
                _context.UnitDebts.Add(new UnitDebt
                {
                    BuildingUnitId = unit.Id,
                    BuildingExpenseId = expRoof.Id,
                    Amount = roofShare,
                    IsPaid = idx <= 8,
                    PaymentDate = idx <= 8 ? DateTime.Today.AddDays(-idx) : null,
                    PaidBy = idx <= 8 ? unit.OwnerName : null
                });

                // Ortak Elektrik (İşletme - Sakinler)
                _context.UnitDebts.Add(new UnitDebt
                {
                    BuildingUnitId = unit.Id,
                    BuildingExpenseId = expElec.Id,
                    Amount = elecShare,
                    IsPaid = idx % 2 == 0,
                    PaymentDate = idx % 2 == 0 ? DateTime.Today.AddDays(-1) : null,
                    PaidBy = idx % 2 == 0 ? (unit.TenantName ?? unit.OwnerName) : null
                });

                // Asansör Bakımı (Dükkanlar Muaftır!)
                if (!isShop)
                {
                    _context.UnitDebts.Add(new UnitDebt
                    {
                        BuildingUnitId = unit.Id,
                        BuildingExpenseId = expElev.Id,
                        Amount = elevShare,
                        IsPaid = idx <= 6,
                        PaymentDate = idx <= 6 ? DateTime.Today : null,
                        PaidBy = idx <= 6 ? (unit.TenantName ?? unit.OwnerName) : null
                    });
                }
            }

            // 5. Ortak Tesisler & Zimmetli Demirbaşlar
            var commonUnit = new BuildingUnit
            {
                BuildingId = building.Id,
                DoorNumber = "Ortak Tesisler",
                Category = "OrtakAlan_Blok",
                FloorLevel = -1,
                FloorName = "Tesis & Ortak Alanlar",
                RoomLayout = "Ortak Alan / Tesisat"
            };
            _context.BuildingUnits.Add(commonUnit);
            await _context.SaveChangesAsync();

            var spaceShelter = new UnitSpace { BuildingUnitId = commonUnit.Id, Name = "Sığınak & Acil Depo", Type = "Sığınak", Description = "50 Kişilik acil sığınak ve su deposu" };
            var spaceGen = new UnitSpace { BuildingUnitId = commonUnit.Id, Name = "Jeneratör Odası", Type = "Teknik Oda", Description = "Otomatik dizel jeneratör ve transfer panosu" };
            var spaceHydro = new UnitSpace { BuildingUnitId = commonUnit.Id, Name = "Hidrofor & Kazan Dairesi", Type = "Mekanik Tesisat", Description = "Su depoları, hidrofor ve arıtma sistemi" };
            var spaceFire = new UnitSpace { BuildingUnitId = commonUnit.Id, Name = "Yangın Güvenlik İstasyonu", Type = "Güvenlik", Description = "Kat sahanlıkları ve teknik hacim yangın tüpleri" };
            var spaceGarden = new UnitSpace { BuildingUnitId = commonUnit.Id, Name = "Açık Alan & Bahçe", Type = "Sosyal Donatı", Description = "Çocuk oyun parkı, kamelyalar ve bahçıvan odası" };
            var spaceRoof = new UnitSpace { BuildingUnitId = commonUnit.Id, Name = "Çatı & Asansör Kulesi", Type = "Çatı & Kule", Description = "Asansör makine dairesi, paratoner ve su izolasyonu" };

            _context.UnitSpaces.AddRange(spaceShelter, spaceGen, spaceHydro, spaceFire, spaceGarden, spaceRoof);
            await _context.SaveChangesAsync();

            var fixtures = new List<SpaceFixture>
            {
                // Sığınak
                new SpaceFixture { UnitSpaceId = spaceShelter.Id, Category = "Tesisat", ItemName = "5 Ton Paslanmaz Acil Su Deposu", StandardBrand = "Ecomak", Quantity = 1, Unit = "Adet", MaintenanceNotes = "Depo dolu ve klorlama yapıldı.", NextMaintenanceDate = DateTime.Today.AddMonths(6) },
                new SpaceFixture { UnitSpaceId = spaceShelter.Id, Category = "Havalandırma", ItemName = "Sığınak Havalandırma & HEPA Filtre", StandardBrand = "Aironn", Quantity = 1, Unit = "Set", SupplierName = "Aironn Havalandırma A.Ş.", TechnicalServiceName = "Aironn Yetkili Servis (0216 300 40 50)", MaintenanceNotes = "Filtreler temiz, hava debisi test edildi.", NextMaintenanceDate = DateTime.Today.AddMonths(12) },

                // Jeneratör
                new SpaceFixture { UnitSpaceId = spaceGen.Id, Category = "Enerji", ItemName = "110 kVA Otomatik Dizel Jeneratör", StandardBrand = "Aksa", StandardModel = "APD-110A", Quantity = 1, Unit = "Adet", SupplierName = "Aksa Jeneratör A.Ş.", TechnicalServiceName = "Aksa Kadıköy Yetkili Servis (0216 555 12 12)", MaintenanceNotes = "Mazot seviyesi %85 (140 Lt). Periyodik yağ ve filtre bakımı yapıldı.", NextMaintenanceDate = DateTime.Today.AddMonths(5) },

                // Hidrofor
                new SpaceFixture { UnitSpaceId = spaceHydro.Id, Category = "Tesisat", ItemName = "Çift Pompalı Frekans Kontrollü Hidrofor", StandardBrand = "Alarko Carrier", StandardModel = "ALH-2/50", Quantity = 1, Unit = "Set", SupplierName = "Alarko Yetkili Bayi", TechnicalServiceName = "Alarko Kadıköy Servis (0216 414 00 20)", MaintenanceNotes = "Genleşme tankı hava basıncı 4.5 Bar olarak ayarlandı.", NextMaintenanceDate = DateTime.Today.AddMonths(4) },

                // Yangın İstasyonu
                new SpaceFixture { UnitSpaceId = spaceFire.Id, Category = "Yangın Güvenlik", ItemName = "6 Kg KKT Yangın Söndürme Tüpleri (6 Adet)", StandardBrand = "YangınSan", StandardModel = "KKT-6", Quantity = 6, Unit = "Adet", SupplierName = "Yangın Güvenlik Sistemleri Ltd.", TechnicalServiceName = "YangınSan Periyodik Dolum Merkezi", MaintenanceNotes = "Her kat sahanlığı ve bodrum katta montajlı. Mühürlü ve basınç ibreleri yeşil alanda.", NextMaintenanceDate = DateTime.Today.AddMonths(7) },

                // Bahçe
                new SpaceFixture { UnitSpaceId = spaceGarden.Id, Category = "Bahçe Ekipmanı", ItemName = "Benzinli Çim Biçme Makinesi", StandardBrand = "Honda", StandardModel = "HRG 466", Quantity = 1, Unit = "Adet", SupplierName = "Honda Bahçe Dünyası", MaintenanceNotes = "Zimmetli Personel: Apartman Görevlisi Ali Usta. Bıçakları bilendi.", NextMaintenanceDate = DateTime.Today.AddMonths(3) },
                new SpaceFixture { UnitSpaceId = spaceGarden.Id, Category = "Bahçe Ekipmanı", ItemName = "50m Makaralı Bahçe Hortumu & Kar Küreği Seti", StandardBrand = "Gardena", Quantity = 1, Unit = "Set", MaintenanceNotes = "Zimmetli: Ali Usta. Kilitli bahçe deposunda muhafaza edilmektedir." },

                // Çatı & Asansör
                new SpaceFixture { UnitSpaceId = spaceRoof.Id, Category = "Asansör", ItemName = "630 Kg 8 Kişilik Halatlı Asansör Makine Grubu", StandardBrand = "Kone", StandardModel = "MonoSpace", Quantity = 1, Unit = "Adet", SupplierName = "Kone Asansör San. A.Ş.", TechnicalServiceName = "Kone Türkiye 7/24 Servis (0216 444 0 566)", MaintenanceNotes = "Yeşil Etiket onaylı. Aylık periyodik bakım sözleşmesi kapsamında.", NextMaintenanceDate = DateTime.Today.AddDays(25) },
                new SpaceFixture { UnitSpaceId = spaceRoof.Id, Category = "İzolasyon", ItemName = "Çift Kat Membran Çatı Su İzolasyonu & Oluklar", StandardBrand = "Onduline", Quantity = 400, Unit = "m2", SupplierName = "Kardelen Çatı & İzolasyon", MaintenanceNotes = "10 Yıl Garantili Su İzolasyonu Uygulaması (Ekim 2026)." }
            };

            _context.SpaceFixtures.AddRange(fixtures);
            await _context.SaveChangesAsync();

            // 6. Noter Karar Defteri (KMK Md. 32)
            var sampleMeeting = new SystemMeeting
            {
                ContextType = "Building",
                ContextId = building.Id,
                AgendaTitle = "2026 Yılı Kat Malikleri Olağan Genel Kurul Toplantısı ve Çatı Onarım Kararı",
                MeetingDate = DateTime.Today.AddDays(-10),
                IsConcluded = true,
                CreatedAt = DateTime.Now.AddDays(-10)
            };
            _context.SystemMeetings.Add(sampleMeeting);
            await _context.SaveChangesAsync();

            var decisions = new List<SystemMeetingDecision>
            {
                new SystemMeetingDecision { SystemMeetingId = sampleMeeting.Id, DecisionText = "Karar #2026/01: Çatı izolasyonunun yenilenmesine, toplanan teklifler arasından 36.000 TL bedelli teklifin seçilmesine ve bağımsız bölüm başına 3.000 TL demirbaş payı çıkarılmasına oy çokluğuyla karar verilmiştir." },
                new SystemMeetingDecision { SystemMeetingId = sampleMeeting.Id, DecisionText = "Karar #2026/02: Zemin dükkanların yönetim planı uyarınca asansör periyodik bakım ve merdiven aydınlatma aidatından muaf tutulmasına; ancak çatı ve dış cephe ana demirbaş giderlerine arsa payları oranında iştirak etmelerine oy birliğiyle karar verilmiştir." },
                new SystemMeetingDecision { SystemMeetingId = sampleMeeting.Id, DecisionText = "Karar #2026/03: Bodrum kat sığınak acil su deposunun ve hidrofor filtrelerinin Alarko yetkili servisi tarafından yenilenmesine ve yangın tüplerinin yıllık muayenesinin yaptırılmasına karar verilmiştir." },
                new SystemMeetingDecision { SystemMeetingId = sampleMeeting.Id, DecisionText = "Karar #2026/04: Apartman görevlisine bahçe bakım ekipmanlarının (çim biçme makinesi, hortum ve kar küreği) zimmet tutanağı ile teslim edilmesine karar verilmiştir." }
            };
            _context.SystemMeetingDecisions.AddRange(decisions);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "🎉 Harika! 12 Bağımsız Bölümlü (10 Daire + 2 Zemin Dükkan) Örnek 'Kardelen Konutları' sitesi, ortak alan zimmetli demirbaşları, noter karar defteri ve aidat çetelesiyle başarıyla yüklendi!";
            return RedirectToAction(nameof(Index));
        }

        // 2. Yeni Bina Ekle (Artık SPA Onboarding'e yönlendirebiliriz)
        [HttpPost]
        public async Task<IActionResult> CreateBuilding(string name, string address)
        {
            var userId = GetUserId();
            var building = new Building
            {
                Name = name,
                Address = address,
                ManagerUserId = userId,
                CreatedByUserId = userId,
                CreatedAt = DateTime.Now,
                OnboardingStep = 1 // Adım 1: Bina oluşturuldu
            };

            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Onboarding), new { id = building.Id });
        }

        // Onboarding (SPA - Tek Sayfa)
        public async Task<IActionResult> Onboarding(int? id)
        {
            var userId = GetUserId();
            Building? building = null;

            if (id.HasValue)
            {
                building = await _context.Buildings
                    .Include(b => b.Units)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (building != null && !CanAccessBuilding(building, userId))
                    return Forbid();
            }

            // Eğer id yoksa veya bulunamadıysa yeni bina onboarding başlatılır (Step 0)
            return View(building);
        }

        // API Endpoint for Step 1: Bina ve Adres Kaydı
        [HttpPost]
        public async Task<IActionResult> SaveOnboardingStep1([FromBody] Building buildingData)
        {
            var userId = GetUserId();
            var building = new Building
            {
                Name = buildingData.Name,
                Address = buildingData.Address,
                CityId = buildingData.CityId,
                DistrictId = buildingData.DistrictId,
                NeighborhoodId = buildingData.NeighborhoodId,
                ManagerUserId = userId,
                CreatedByUserId = userId,
                CreatedAt = DateTime.Now,
                OnboardingStep = 1
            };

            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

            return Json(new { success = true, buildingId = building.Id, step = 1 });
        }

        // API Endpoint for Step 2: Daireler ve Kişiler
        [HttpPost]
        public async Task<IActionResult> SaveOnboardingStep2(int buildingId, [FromBody] List<BuildingUnit> units)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Json(new { success = false, message = "Bina bulunamadı veya yetkiniz yok." });

            foreach (var unit in units)
            {
                unit.BuildingId = buildingId;
                _context.BuildingUnits.Add(unit);
            }
            
            building.OnboardingStep = 2;
            await _context.SaveChangesAsync();

            // TODO: Call NetGSM Notification service to invite users here

            return Json(new { success = true, step = 2 });
        }

        // API Endpoint for Step 3: Sabit Giderler
        [HttpPost]
        public async Task<IActionResult> SaveOnboardingStep3(int buildingId, [FromBody] List<BuildingExpense> expenses)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Json(new { success = false, message = "Bina bulunamadı veya yetkiniz yok." });

            foreach (var expense in expenses)
            {
                expense.BuildingId = buildingId;
                expense.Date = DateTime.Now;
                _context.BuildingExpenses.Add(expense);
            }
            
            building.OnboardingStep = 3;
            await _context.SaveChangesAsync();

            return Json(new { success = true, step = 3 });
        }


        // 3. Bina Detay (Yönetim Paneli)
        public async Task<IActionResult> Detail(int id)
        {
            var userId = GetUserId();
            var building = await _context.Buildings
                .Include(b => b.Units)
                    .ThenInclude(u => u.Debts)
                        .ThenInclude(d => d.BuildingExpense)
                .Include(b => b.Expenses)
                .FirstOrDefaultAsync(b => b.Id == id && (b.ManagerUserId == userId || b.CreatedByUserId == userId || User.IsInRole("Admin") || User.IsInRole("SuperAdmin")));

            if (building == null) return NotFound();

            // Noter Karar Defteri (KMK Md. 32)
            var meetings = await _context.SystemMeetings
                .Include(m => m.Decisions)
                .Where(m => m.ContextType == "Building" && m.ContextId == id)
                .OrderByDescending(m => m.MeetingDate)
                .ToListAsync();
            ViewBag.Meetings = meetings;

            // Ortak Alan Tesis Mekanları ve Zimmetli Demirbaşlar
            var commonUnits = await _context.BuildingUnits
                .Include(u => u.Spaces)
                    .ThenInclude(s => s.Fixtures)
                .Where(u => u.BuildingId == id && (u.Category == "OrtakAlan_Blok" || u.Category.StartsWith("OrtakAlan")))
                .ToListAsync();

            ViewBag.CommonSpaces = commonUnits.SelectMany(u => u.Spaces ?? new List<UnitSpace>()).ToList();

            return View(building);
        }

        // 3.1 Noter Karar Defteri - Yeni Toplantı ve Karar Ekle
        [HttpPost]
        public async Task<IActionResult> CreateMeeting(int buildingId, string agendaTitle, DateTime meetingDate, string decisionText)
        {
            var userId = GetUserId();
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == buildingId && (b.ManagerUserId == userId || b.CreatedByUserId == userId || User.IsInRole("Admin") || User.IsInRole("SuperAdmin")));
            if (building == null) return Unauthorized();

            var meeting = new SystemMeeting
            {
                ContextType = "Building",
                ContextId = buildingId,
                AgendaTitle = agendaTitle,
                MeetingDate = meetingDate,
                IsConcluded = true,
                CreatedAt = DateTime.Now
            };

            _context.SystemMeetings.Add(meeting);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(decisionText))
            {
                var decision = new SystemMeetingDecision
                {
                    SystemMeetingId = meeting.Id,
                    DecisionText = decisionText
                };
                _context.SystemMeetingDecisions.Add(decision);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "🎉 Genel Kurul Kararı başarıyla kaydedildi. Noter Defteri Baskı Şablonunu indirebilirsiniz.";
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        // 3.2 Noter Karar Defteri Baskı Şablonu (KMK Md. 32 A4 Islak İmza Tutanağı)
        [HttpGet]
        public async Task<IActionResult> PrintDecisionSheet(int meetingId)
        {
            var meeting = await _context.SystemMeetings
                .Include(m => m.Decisions)
                .FirstOrDefaultAsync(m => m.Id == meetingId && m.ContextType == "Building");

            if (meeting == null) return NotFound("Toplantı kaydı bulunamadı.");

            var building = await _context.Buildings
                .Include(b => b.Units)
                .FirstOrDefaultAsync(b => b.Id == meeting.ContextId);

            if (!CanAccessBuilding(building, GetUserId()))
                return Forbid();

            ViewBag.Building = building;
            return View(meeting);
        }

        // 3.3 Ortak Alana Zimmetli Demirbaş / Ekipman Ekle (Sığınak, Jeneratör, Yangın, Çatı, Bahçe)
        [HttpPost]
        public async Task<IActionResult> AddCommonFixture(int buildingId, string spaceName, string category, string itemName, string brand, string supplierName, string serviceName, string notes, DateTime? nextMaintenanceDate, double quantity = 1, string unit = "Adet")
        {
            var userId = GetUserId();
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == buildingId && (b.ManagerUserId == userId || b.CreatedByUserId == userId || User.IsInRole("Admin") || User.IsInRole("SuperAdmin")));
            if (building == null) return Unauthorized();

            var commonUnit = await _context.BuildingUnits.FirstOrDefaultAsync(u => u.BuildingId == buildingId && u.Category == "OrtakAlan_Blok");
            if (commonUnit == null)
            {
                commonUnit = new BuildingUnit
                {
                    BuildingId = buildingId,
                    DoorNumber = "Ortak Tesisler",
                    Category = "OrtakAlan_Blok",
                    FloorLevel = -1,
                    FloorName = "Tesis & Ortak Alanlar"
                };
                _context.BuildingUnits.Add(commonUnit);
                await _context.SaveChangesAsync();
            }

            var space = await _context.UnitSpaces.FirstOrDefaultAsync(s => s.BuildingUnitId == commonUnit.Id && s.Name == spaceName);
            if (space == null)
            {
                space = new UnitSpace
                {
                    BuildingUnitId = commonUnit.Id,
                    Name = spaceName,
                    Type = "Ortak Alan",
                    Description = spaceName
                };
                _context.UnitSpaces.Add(space);
                await _context.SaveChangesAsync();
            }

            var fixture = new SpaceFixture
            {
                UnitSpaceId = space.Id,
                Category = category ?? "Demirbaş",
                ItemName = itemName,
                StandardBrand = brand,
                SupplierName = supplierName,
                TechnicalServiceName = serviceName,
                MaintenanceNotes = notes,
                NextMaintenanceDate = nextMaintenanceDate,
                Quantity = quantity,
                Unit = unit ?? "Adet",
                CreatedAt = DateTime.Now
            };

            _context.SpaceFixtures.Add(fixture);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{itemName}' demirbaşı başarıyla '{spaceName}' alanına zimmetlendi.";
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        // 3.4 GVK Madde 74 GMSİ Kira Gelir Vergisi Beyannamesi Demirbaş İndirim Tutanağı
        [HttpGet]
        public async Task<IActionResult> PrintGmsiTaxReport(int buildingId, int? year)
        {
            int reportYear = year ?? DateTime.Now.Year;
            var building = await _context.Buildings
                .Include(b => b.Units)
                    .ThenInclude(u => u.Debts)
                        .ThenInclude(d => d.BuildingExpense)
                .Include(b => b.Expenses)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (building == null) return NotFound();
            if (!CanAccessBuilding(building, GetUserId())) return Forbid();

            ViewBag.ReportYear = reportYear;
            return View(building);
        }

        // 3.5 Islak İmzalı Defter Sayfası Arşivleme
        [HttpPost]
        public async Task<IActionResult> UploadMeetingScan(int meetingId, Microsoft.AspNetCore.Http.IFormFile? scanFile)
        {
            var meeting = await _context.SystemMeetings.FirstOrDefaultAsync(m => m.Id == meetingId && m.ContextType == "Building");
            if (meeting == null) return NotFound();

            var building = await GetManagedBuildingAsync(meeting.ContextId);
            if (building == null) return Forbid();

            if (scanFile == null || scanFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen yüklenecek PDF veya görsel dosyasını seçiniz.";
                return RedirectToAction(nameof(Detail), new { id = building.Id });
            }

            var ext = System.IO.Path.GetExtension(scanFile.FileName)?.ToLowerInvariant();
            var allowedExts = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };
            if (string.IsNullOrEmpty(ext) || !allowedExts.Contains(ext))
            {
                TempData["ErrorMessage"] = "Yalnızca PDF veya görsel (JPG, PNG, WEBP) dosyaları yüklenebilir.";
                return RedirectToAction(nameof(Detail), new { id = building.Id });
            }

            var env = HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.Hosting.IWebHostEnvironment)) as Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
            var webRoot = env?.WebRootPath ?? System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
            var folder = System.IO.Path.Combine(webRoot, "uploads", "buildings", building.Id.ToString(), "decisions");
            System.IO.Directory.CreateDirectory(folder);

            var uniqueName = $"karar_{meeting.Id}_{Guid.NewGuid():N}{ext}";
            var fullPath = System.IO.Path.Combine(folder, uniqueName);
            using (var stream = new System.IO.FileStream(fullPath, System.IO.FileMode.Create))
            {
                await scanFile.CopyToAsync(stream);
            }

            _context.DocumentArchives.Add(new DocumentArchive
            {
                BuildingId = building.Id,
                Title = $"Islak İmzalı Karar - {meeting.AgendaTitle}",
                FileName = scanFile.FileName,
                FileExtension = ext,
                Category = "KararDefteri",
                SourceModule = "BuildingManager",
                SourceRecordId = meeting.Id,
                DocumentUrl = $"/uploads/buildings/{building.Id}/decisions/{uniqueName}",
                Status = "Tamamlandı",
                UploadDate = DateTime.Now,
                Year = meeting.MeetingDate.Year,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Noter onaylı ve ıslak imzalı karar sayfası dijital arşive kaydedildi.";
            return RedirectToAction(nameof(Detail), new { id = building.Id });
        }

        // 4. Daire / Bölüm Ekle
        [HttpPost]
        public async Task<IActionResult> AddUnit(int buildingId, string unitNumber, string? ownerName, string? ownerPhone, string? tenantName, string? tenantPhone, bool isEmpty, string? category = "Daire", string? ownerEmail = null, string? tenantEmail = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var unit = new BuildingUnit
            {
                BuildingId = buildingId,
                DoorNumber = unitNumber,
                UnitNumber = unitNumber,
                Category = string.IsNullOrWhiteSpace(category) ? "Daire" : category.Trim(),
                OwnerName = ownerName,
                OwnerPhone = ownerPhone,
                OwnerEmail = ownerEmail,
                TenantName = tenantName,
                TenantPhone = tenantPhone,
                TenantEmail = tenantEmail,
                IsEmpty = isEmpty
            };

            _context.BuildingUnits.Add(unit);
            building.TotalUnits += 1;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{unitNumber}' bağımsız bölümü eklendi.";
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        // 5. Gider Ekle ve Dağıt
        [HttpPost]
        public async Task<IActionResult> AddExpense(int buildingId, string description, decimal totalAmount, string payerType)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            if (totalAmount <= 0)
            {
                TempData["ErrorMessage"] = "Gider tutarı sıfırdan büyük olmalıdır.";
                return RedirectToAction(nameof(Detail), new { id = buildingId });
            }

            var expense = new BuildingExpense
            {
                BuildingId = buildingId,
                Description = description,
                TotalAmount = totalAmount,
                PayerType = payerType,
                Date = DateTime.Now
            };
            
            _context.BuildingExpenses.Add(expense);
            await _context.SaveChangesAsync(); // ID almak için

            // OrtakAlan kayitlari haric tum bagimsiz bolumlere paylastir
            var units = await _context.BuildingUnits
                .Where(u => u.BuildingId == buildingId && u.Category != "OrtakAlan_Blok" && (u.Category == null || !u.Category.StartsWith("OrtakAlan")))
                .ToListAsync();
            
            if (units.Any())
            {
                decimal amountPerUnit = Math.Round(totalAmount / units.Count, 2);

                foreach (var unit in units)
                {
                    _context.UnitDebts.Add(new UnitDebt
                    {
                        BuildingUnitId = unit.Id,
                        BuildingExpenseId = expense.Id,
                        Amount = amountPerUnit,
                        IsPaid = false
                    });
                }
                
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Gider işlendi ve bağımsız bölümlere dağıtıldı.";
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        // 6. Ödeme Alındı İşaretle
        [HttpPost]
        public async Task<IActionResult> MarkDebtAsPaid(int debtId, string paidBy)
        {
            var existingDebt = await _context.UnitDebts
                .Include(d => d.BuildingUnit)
                    .ThenInclude(u => u.Building)
                .FirstOrDefaultAsync(d => d.Id == debtId);

            if (existingDebt == null) return NotFound();
            if (!CanAccessBuilding(existingDebt.BuildingUnit?.Building, GetUserId()))
                return Forbid();

            var debt = await _buildingManagementService.MarkDebtAsPaidAsync(debtId, paidBy);
            
            if (debt != null)
            {
                TempData["SuccessMessage"] = "Ödeme başarıyla kaydedildi ve dijital makbuz gönderildi.";
                return RedirectToAction(nameof(Detail), new { id = debt.BuildingUnit.BuildingId });
            }
            
            TempData["ErrorMessage"] = "Ödeme kaydedilirken bir hata oluştu.";
            return RedirectToAction(nameof(Index));
        }

        // 7. Sakinler & Daireler Listesi
        public async Task<IActionResult> Sakinler(int? buildingId)
        {
            var userId = GetUserId();
            bool isAdmin = IsAdminOrSuperAdmin();
            var buildingsQuery = _context.Buildings.AsQueryable();
            if (!isAdmin) buildingsQuery = buildingsQuery.Where(b => b.ManagerUserId == userId || b.CreatedByUserId == userId);

            var query = _context.BuildingUnits
                .Include(u => u.Building)
                .Where(u => u.Category != "OrtakAlan_Blok" && (u.Category == null || !u.Category.StartsWith("OrtakAlan")));
            if (!isAdmin) query = query.Where(u => u.Building.ManagerUserId == userId || u.Building.CreatedByUserId == userId);
            if (buildingId.HasValue) query = query.Where(u => u.BuildingId == buildingId.Value);
            
            var units = await query.ToListAsync();
            ViewBag.Buildings = await buildingsQuery.ToListAsync();
            ViewBag.SelectedBuildingId = buildingId;
            return View(units);
        }

        // 8. Aidat ve Borç Takibi
        public async Task<IActionResult> Aidatlar(int? buildingId)
        {
            var userId = GetUserId();
            bool isAdmin = IsAdminOrSuperAdmin();
            var buildingsQuery = _context.Buildings.AsQueryable();
            if (!isAdmin) buildingsQuery = buildingsQuery.Where(b => b.ManagerUserId == userId || b.CreatedByUserId == userId);

            var query = _context.UnitDebts
                .Include(d => d.BuildingUnit).ThenInclude(u => u.Building)
                .Include(d => d.BuildingExpense)
                .AsQueryable();
            if (!isAdmin) query = query.Where(d => d.BuildingUnit.Building.ManagerUserId == userId || d.BuildingUnit.Building.CreatedByUserId == userId);
                
            if (buildingId.HasValue) query = query.Where(d => d.BuildingUnit.BuildingId == buildingId.Value);
            
            var debts = await query.OrderByDescending(d => d.Id).ToListAsync();
            ViewBag.Buildings = await buildingsQuery.ToListAsync();
            ViewBag.SelectedBuildingId = buildingId;
            return View(debts);
        }

        // 9. Giderler
        public async Task<IActionResult> Giderler(int? buildingId)
        {
            var userId = GetUserId();
            bool isAdmin = IsAdminOrSuperAdmin();
            var buildingsQuery = _context.Buildings.AsQueryable();
            if (!isAdmin) buildingsQuery = buildingsQuery.Where(b => b.ManagerUserId == userId || b.CreatedByUserId == userId);

            var query = _context.BuildingExpenses.Include(e => e.Building).AsQueryable();
            if (!isAdmin) query = query.Where(e => e.Building.ManagerUserId == userId || e.Building.CreatedByUserId == userId);
            
            if (buildingId.HasValue) query = query.Where(e => e.BuildingId == buildingId.Value);
            
            var expenses = await query.OrderByDescending(e => e.Date).ToListAsync();
            ViewBag.Buildings = await buildingsQuery.ToListAsync();
            ViewBag.SelectedBuildingId = buildingId;
            return View(expenses);
        }

        // 7. İç Talepler (Ticket) Yönetimi
        public async Task<IActionResult> TicketManagement(int buildingId)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var tickets = await _context.SupportTickets
                .Include(t => t.CreatorUser)
                .Where(t => t.ContextType == "Building" && t.ContextId == buildingId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.Building = building;
            return View(tickets);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTicketStatus(int ticketId, string status)
        {
            var userId = GetUserId();
            var ticket = await _context.SupportTickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null) return NotFound();

            var building = ticket.ContextId.HasValue
                ? await _context.Buildings.FirstOrDefaultAsync(b => b.Id == ticket.ContextId.Value)
                : null;

            if (!IsAdminOrSuperAdmin() && ticket.AssignedToUserId != userId && !CanAccessBuilding(building, userId))
                return Forbid();

            ticket.Status = status;
            ticket.UpdatedAt = DateTime.Now;
            
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(TicketManagement), new { buildingId = ticket.ContextId });
        }
    }
}
