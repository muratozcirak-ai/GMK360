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
            if (string.IsNullOrEmpty(userId)) return false;

            int rootId = building.ParentBuildingId ?? building.Id;
            bool hasRoster = _context.BuildingManagers.Any(m => m.BuildingId == rootId);
            if (hasRoster)
            {
                return _context.BuildingManagers.Any(m => m.BuildingId == rootId && m.UserId == userId && m.IsActive);
            }

            return building.ManagerUserId == userId || building.CreatedByUserId == userId;
        }

        private async Task<Building?> GetManagedBuildingAsync(int buildingId)
        {
            var userId = GetUserId();
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == buildingId);
            return CanAccessBuilding(building, userId) ? building : null;
        }

        private async Task<bool> HasDecisionAuthorityAsync(int buildingId)
        {
            if (IsAdminOrSuperAdmin()) return true;
            var building = await _context.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == buildingId);
            if (building == null) return false;
            int rootId = building.ParentBuildingId ?? building.Id;
            if ((building.Name ?? "").Contains("Örnek", StringComparison.OrdinalIgnoreCase)) return true;

            return await _context.DocumentArchives.AnyAsync(d =>
                d.BuildingId == rootId
                && d.SourceModule == "BuildingManager"
                && d.Category == "KararDefteriYetki");
        }

        // 1. Yönettiğim Binalar & Genel Kumanda Merkezi
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var query = _context.Buildings
                .Include(b => b.HousingComplex)
                .Include(b => b.City)
                .Include(b => b.District)
                .Include(b => b.ChildBuildings)
                    .ThenInclude(cb => cb.Units)
                        .ThenInclude(u => u.Debts)
                .Include(b => b.Units)
                    .ThenInclude(u => u.Debts)
                        .ThenInclude(d => d.BuildingExpense)
                .Include(b => b.Expenses)
                .Where(b => b.ParentBuildingId == null)
                .AsQueryable();

            if (!User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                query = query.Where(b =>
                    b.BuildingManagers.Any(m => m.IsActive)
                        ? b.BuildingManagers.Any(m => m.UserId == userId && m.IsActive)
                        : (b.ManagerUserId == userId || b.CreatedByUserId == userId));
            }

            var buildings = await query
                .OrderByDescending(b => b.Id)
                .ToListAsync();

            // Yalnizca tohum/ornek verilerden kalan birebir ayni isimli coklu ornek kayit varsa koru (gercek bloklara dokunma)
            buildings = buildings
                .GroupBy(b => b.Id)
                .Select(g => g.First())
                .ToList();

            // Portföy Çapında Genel İstatistikler (Kök yapı + kulelerin tüm birimleri)
            var allUnits = buildings
                .SelectMany(b => (b.Units ?? new System.Collections.Generic.List<BuildingUnit>())
                    .Concat((b.ChildBuildings ?? new System.Collections.Generic.List<Building>()).SelectMany(cb => cb.Units ?? new System.Collections.Generic.List<BuildingUnit>())))
                .Where(u => !(u.Category ?? "").StartsWith("OrtakAlan"))
                .ToList();
            var allDebts = allUnits.SelectMany(u => u.Debts ?? new System.Collections.Generic.List<UnitDebt>()).ToList();

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

        // 2. Yeni Bina / Site Açılış & 3 Adımlı Sihirbaz
        public async Task<IActionResult> Onboarding(int? id, string? mode = null, string? name = null)
        {
            var userId = GetUserId();
            var vm = new GMK360.Web.Models.FacilityWizardPageViewModel
            {
                ManagementMode = string.Equals(mode, "Site", StringComparison.OrdinalIgnoreCase) ? "Site" : "Apartman",
                Name = name?.Trim() ?? string.Empty
            };

            if (id.HasValue)
            {
                var building = await _context.Buildings
                    .Include(b => b.HousingComplex)
                    .Include(b => b.ChildBuildings)
                    .FirstOrDefaultAsync(b => b.Id == id.Value);

                if (building == null) return NotFound();
                if (!CanAccessBuilding(building, userId)) return Forbid();
                if (building.ParentBuildingId.HasValue)
                {
                    return RedirectToAction(nameof(Onboarding), new { id = building.ParentBuildingId.Value });
                }

                vm.BuildingId = building.Id;
                vm.HousingComplexId = building.HousingComplexId;
                vm.ManagementMode = building.HousingComplexId.HasValue ? "Site" : ParseTechnicalTag(building.TechnicalFeatures, "YapiTuru") ?? (building.ChildBuildings.Any() ? "Site" : "Apartman");
                vm.Name = building.HousingComplex?.Name ?? building.Name;
                vm.Description = building.Description;
                vm.BuildingAge = building.BuildingAge;
                vm.TaxNumber = building.TaxNumber;
                vm.CityId = building.CityId;
                vm.DistrictId = building.DistrictId;
                vm.NeighborhoodId = building.NeighborhoodId;
                vm.StreetId = building.StreetId;
                vm.StreetName = building.StreetName;
                vm.BuildingNumber = building.BuildingNumber;
                vm.ExtraAddressLabel = ParseTechnicalTag(building.TechnicalFeatures, "EkNo");
                vm.Ada = ParseTechnicalTag(building.TechnicalFeatures, "Ada");
                vm.Parsel = ParseTechnicalTag(building.TechnicalFeatures, "Parsel");
                vm.Latitude = building.Latitude?.ToString(System.Globalization.CultureInfo.InvariantCulture);
                vm.Longitude = building.Longitude?.ToString(System.Globalization.CultureInfo.InvariantCulture);
                vm.RootBuilding = building;

                var docs = await _context.DocumentArchives
                    .Where(d => d.BuildingId == building.Id && d.SourceModule == "BuildingManager")
                    .OrderByDescending(d => d.Id)
                    .ToListAsync();
                vm.Photos = docs.Where(d => d.Category == "BinaGorseli").ToList();
                vm.DecisionDocument = docs.FirstOrDefault(d => d.Category == "KararDefteriYetki");

                // Mevcut yapıları (Site ise aynı complex altındaki tüm kök binalar, değilse sadece bu bina) yükle
                var rootStructures = building.HousingComplexId.HasValue
                    ? await _context.Buildings.Include(b => b.ChildBuildings).Where(b => b.HousingComplexId == building.HousingComplexId && b.ParentBuildingId == null).OrderBy(b => b.Id).ToListAsync()
                    : new System.Collections.Generic.List<Building> { building };

                vm.InitialStructures = rootStructures.Select(rb => new GMK360.Web.Models.FacilityStep2StructureDto
                {
                    LayoutPattern = rb.LayoutPattern ?? "Tekil Yapı",
                    BlockName = string.IsNullOrWhiteSpace(rb.BlockName) ? "Ana Bina" : rb.BlockName,
                    BaseArea = rb.BaseArea,
                    TotalFloors = rb.TotalFloors ?? 5,
                    TotalApartments = rb.TotalApartments,
                    TotalShops = rb.TotalShops,
                    BasementFloors = rb.BasementFloors,
                    HasGroundFloor = rb.HasGroundFloor,
                    HasRoof = rb.HasRoof,
                    RoofType = rb.RoofType ?? "Düz Teras Çatı (Tabliye)",
                    Children = (rb.ChildBuildings ?? new System.Collections.Generic.List<Building>())
                        .OrderBy(c => c.Id)
                        .Select(c => new GMK360.Web.Models.FacilityStep2ChildBlockDto
                        {
                            BlockName = c.BlockName,
                            TotalFloors = c.TotalFloors ?? 8,
                            TotalApartments = c.TotalApartments,
                            TotalShops = c.TotalShops,
                            BasementFloors = c.BasementFloors,
                            HasGroundFloor = c.HasGroundFloor,
                            HasRoof = c.HasRoof,
                            RoofType = c.RoofType ?? "Düz Teras Çatı (Tabliye)"
                        }).ToList()
                }).ToList();
            }

            ViewBag.Cities = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                await _context.Cities.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", vm.CityId);
            ViewBag.Districts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                vm.CityId > 0 ? await _context.Districts.Where(d => d.CityId == vm.CityId).OrderBy(d => d.Name).ToListAsync() : new System.Collections.Generic.List<District>(),
                "Id", "Name", vm.DistrictId);
            ViewBag.Neighborhoods = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                vm.DistrictId > 0 ? await _context.Neighborhoods.Where(n => n.DistrictId == vm.DistrictId).OrderBy(n => n.Name).ToListAsync() : new System.Collections.Generic.List<Neighborhood>(),
                "Id", "Name", vm.NeighborhoodId);
            ViewBag.Streets = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                vm.NeighborhoodId > 0 ? await _context.Streets.Where(s => s.NeighborhoodId == vm.NeighborhoodId).OrderBy(s => s.Name).ToListAsync() : new System.Collections.Generic.List<Street>(),
                "Id", "Name", vm.StreetId);

            return View(vm);
        }

        // Tekil Merkez Adres ID: İl + İlçe + Mahalle + Sokak + Bina/Kapı No mükerrer kontrolü
        [HttpGet]
        public async Task<IActionResult> CheckDuplicateAddress(
            int cityId,
            int districtId,
            int neighborhoodId,
            int? streetId,
            string? streetName,
            string? buildingNumber,
            int? excludeBuildingId = null)
        {
            if (cityId <= 0 || districtId <= 0 || neighborhoodId <= 0 || string.IsNullOrWhiteSpace(buildingNumber))
            {
                return Json(new { exists = false });
            }

            var resolvedStreetName = streetName;
            if (streetId.HasValue && streetId.Value > 0 && string.IsNullOrWhiteSpace(resolvedStreetName))
            {
                resolvedStreetName = await _context.Streets.Where(s => s.Id == streetId.Value).Select(s => s.Name).FirstOrDefaultAsync();
            }

            var existing = await FindDuplicateAddressBuildingAsync(cityId, districtId, neighborhoodId, streetId, resolvedStreetName, buildingNumber, excludeBuildingId);
            if (existing == null)
            {
                return Json(new { exists = false });
            }

            return Json(new
            {
                exists = true,
                existingBuildingId = existing.Id,
                existingBuildingName = existing.Name,
                addressLabel = $"{existing.StreetName} No: {existing.BuildingNumber}",
                message = $"Bu adreste (Merkez ID #{existing.Id} — {existing.Name}) sistemde zaten kayıtlı bir yapı bulunmaktadır. Mükerrer bina açılamaz."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFacilityStep1([FromForm] GMK360.Web.Models.FacilityStep1FormModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return Json(new { success = false, message = "Apartman veya Site adı zorunludur." });
            if (model.CityId <= 0 || model.DistrictId <= 0 || model.NeighborhoodId <= 0)
                return Json(new { success = false, message = "Lütfen İl, İlçe ve Mahalle seçimini tamamlayınız." });
            if (string.IsNullOrWhiteSpace(model.BuildingNumber))
                return Json(new { success = false, message = "Tekil Merkez Adres ID için Bina / Kapı No zorunludur." });

            string resolvedStreetName = (model.StreetName ?? string.Empty).Trim();
            if (model.StreetId.HasValue && model.StreetId.Value > 0)
            {
                var dbStreetName = await _context.Streets.Where(s => s.Id == model.StreetId.Value).Select(s => s.Name).FirstOrDefaultAsync();
                if (!string.IsNullOrWhiteSpace(dbStreetName))
                {
                    resolvedStreetName = dbStreetName;
                }
            }

            if (string.IsNullOrWhiteSpace(resolvedStreetName))
                return Json(new { success = false, message = "Lütfen Sokak / Cadde seçiniz veya adını yazınız." });

            int? excludeId = model.BuildingId > 0 ? model.BuildingId : null;
            var duplicate = await FindDuplicateAddressBuildingAsync(model.CityId, model.DistrictId, model.NeighborhoodId, model.StreetId, resolvedStreetName, model.BuildingNumber, excludeId);
            if (duplicate != null)
            {
                return StatusCode(409, new
                {
                    success = false,
                    isDuplicate = true,
                    existingBuildingId = duplicate.Id,
                    message = $"Tekil Merkez ID uyarısı: Bu adreste zaten '{duplicate.Name}' (#{duplicate.Id}) kayıtlı. Mükerrer bina oluşturulamaz."
                });
            }

            var userId = GetUserId() ?? string.Empty;
            var userEmail = User.Identity?.Name;
            bool isSite = string.Equals(model.ManagementMode, "Site", StringComparison.OrdinalIgnoreCase);

            double? lat = double.TryParse((model.Latitude ?? "").Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedLat) ? parsedLat : null;
            double? lng = double.TryParse((model.Longitude ?? "").Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedLng) ? parsedLng : null;

            Building? building = null;
            if (model.BuildingId > 0)
            {
                building = await _context.Buildings.Include(b => b.HousingComplex).FirstOrDefaultAsync(b => b.Id == model.BuildingId);
                if (building == null) return NotFound(new { success = false, message = "Bina kaydı bulunamadı." });
                if (!CanAccessBuilding(building, userId)) return Forbid();
            }

            // Site seçildiyse HousingComplex oluştur veya güncelle
            HousingComplex? complex = building?.HousingComplex;
            if (isSite)
            {
                var mgmtId = !string.IsNullOrEmpty(userEmail)
                    ? await _context.ManagementCompanies.Where(m => m.ContactEmail == userEmail).Select(m => (int?)m.Id).FirstOrDefaultAsync()
                    : null;

                if (complex == null)
                {
                    complex = new HousingComplex
                    {
                        Name = model.Name.Trim(),
                        ManagementCompanyId = mgmtId,
                        CityId = model.CityId,
                        DistrictId = model.DistrictId,
                        NeighborhoodId = model.NeighborhoodId,
                        StreetId = model.StreetId > 0 ? model.StreetId : null,
                        Address = $"{resolvedStreetName} No: {model.BuildingNumber.Trim()}",
                        TotalBlocks = 1
                    };
                    _context.HousingComplexes.Add(complex);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    complex.Name = model.Name.Trim();
                    complex.CityId = model.CityId;
                    complex.DistrictId = model.DistrictId;
                    complex.NeighborhoodId = model.NeighborhoodId;
                    complex.StreetId = model.StreetId > 0 ? model.StreetId : null;
                    complex.Address = $"{resolvedStreetName} No: {model.BuildingNumber.Trim()}";
                }
            }

            if (building == null)
            {
                building = new Building
                {
                    ManagerUserId = userId,
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.Now,
                    HasBlock = true,
                    BlockName = isSite ? "Ana Blok" : model.Name.Trim(),
                    TotalFloors = 5,
                    BasementFloors = 1,
                    HasGroundFloor = true,
                    HasRoof = true,
                    RoofType = "Düz Teras Çatı (Tabliye)",
                    LayoutPattern = "Tekil Yapı"
                };
                _context.Buildings.Add(building);
            }

            building.Name = model.Name.Trim();
            building.HousingComplexId = isSite ? complex?.Id : null;
            building.Description = model.Description?.Trim();
            building.BuildingAge = model.BuildingAge;
            building.TaxNumber = model.TaxNumber?.Trim();
            building.CityId = model.CityId;
            building.DistrictId = model.DistrictId;
            building.NeighborhoodId = model.NeighborhoodId;
            building.StreetId = model.StreetId > 0 ? model.StreetId : null;
            building.StreetName = resolvedStreetName;
            building.BuildingNumber = model.BuildingNumber.Trim();
            building.Latitude = lat;
            building.Longitude = lng;
            building.TechnicalFeatures = BuildTechnicalFeaturesString(
                isSite ? "Site" : "Apartman",
                model.Ada,
                model.Parsel,
                model.ExtraAddressLabel);
            if (building.OnboardingStep < 1) building.OnboardingStep = 1;

            await _context.SaveChangesAsync();

            return Json(new { success = true, buildingId = building.Id, complexId = building.HousingComplexId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFacilityDocument(int buildingId, string category, System.Collections.Generic.List<Microsoft.AspNetCore.Http.IFormFile> files)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();
            if (files == null || !files.Any()) return Json(new { success = false, message = "Lütfen dosya seçiniz." });

            bool isDecisionDoc = string.Equals(category, "KararDefteriYetki", StringComparison.OrdinalIgnoreCase);
            string normalizedCategory = isDecisionDoc ? "KararDefteriYetki" : "BinaGorseli";
            var allowedExts = isDecisionDoc
                ? new[] { ".pdf", ".jpg", ".jpeg", ".png", ".webp" }
                : new[] { ".jpg", ".jpeg", ".png", ".webp" };

            var userId = GetUserId();
            var agencyId = await _context.AgencyConsultants
                .Where(c => c.UserId == userId && c.IsActive)
                .Select(c => (int?)c.AgencyId)
                .FirstOrDefaultAsync();

            var subFolder = isDecisionDoc ? "decisions" : "photos";
            var createdDocs = new System.Collections.Generic.List<object>();

            foreach (var file in files)
            {
                if (file == null || file.Length == 0) continue;
                var ext = System.IO.Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? "";
                if (!allowedExts.Contains(ext))
                {
                    return Json(new { success = false, message = $"Geçersiz dosya formatı ({ext})." });
                }

                var docUrl = await SaveUploadedBuildingFileAsync(building.Id, subFolder, file, ext);
                var doc = new DocumentArchive
                {
                    AgencyId = agencyId,
                    BuildingId = building.Id,
                    Title = isDecisionDoc ? $"Karar Defteri Yetki Yazısı - {building.Name}" : $"{building.Name} Görseli",
                    FileName = file.FileName,
                    FileExtension = ext,
                    Category = normalizedCategory,
                    SourceModule = "BuildingManager",
                    SourceRecordId = building.Id,
                    DocumentUrl = docUrl,
                    Status = "Onaylandı",
                    UploadDate = DateTime.Now,
                    Year = DateTime.Now.Year,
                    CreatedAt = DateTime.UtcNow
                };
                _context.DocumentArchives.Add(doc);
                await _context.SaveChangesAsync();

                createdDocs.Add(new { id = doc.Id, fileName = doc.FileName, url = doc.DocumentUrl, category = doc.Category });
            }

            return Json(new { success = true, documents = createdDocs });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFacilityDocument(int docId)
        {
            var doc = await _context.DocumentArchives.FirstOrDefaultAsync(d => d.Id == docId && d.SourceModule == "BuildingManager");
            if (doc == null) return NotFound(new { success = false, message = "Dosya bulunamadı." });

            var building = doc.BuildingId.HasValue ? await GetManagedBuildingAsync(doc.BuildingId.Value) : null;
            if (building == null) return Forbid();

            _context.DocumentArchives.Remove(doc);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFacilityStep2([FromBody] GMK360.Web.Models.FacilityStep2Request model)
        {
            if (model == null || model.BuildingId <= 0 || model.Structures == null || !model.Structures.Any())
                return Json(new { success = false, message = "En az bir yapı / blok bilgisi gereklidir." });
            if (!model.KvkkAuthorityAccepted)
                return Json(new { success = false, message = "Devam etmek için KMK & KVKK Yönetici Yetki Taahhütnamesini onaylamalısınız." });

            var userId = GetUserId();
            var rootBuilding = await _context.Buildings
                .Include(b => b.ChildBuildings)
                .FirstOrDefaultAsync(b => b.Id == model.BuildingId);

            if (rootBuilding == null || !CanAccessBuilding(rootBuilding, userId))
                return Forbid();

            bool isSite = string.Equals(model.ManagementMode, "Site", StringComparison.OrdinalIgnoreCase) || rootBuilding.HousingComplexId.HasValue;
            var structures = isSite ? model.Structures : model.Structures.Take(1).ToList();

            // 1. Ana kök yapıyı ve varsa alt kule/bitişik bloklarını senkronize et
            var firstStruct = structures[0];
            await SyncRootStructureAsync(rootBuilding, firstStruct, isPrimary: true);

            // 2. Site modunda 2+ bağımsız temel/yapı varsa ek kök binaları oluştur (aynı HousingComplexId ve adres altında)
            if (isSite && rootBuilding.HousingComplexId.HasValue && structures.Count > 1)
            {
                var existingSiblings = await _context.Buildings
                    .Include(b => b.ChildBuildings)
                    .Where(b => b.HousingComplexId == rootBuilding.HousingComplexId && b.ParentBuildingId == null && b.Id != rootBuilding.Id)
                    .OrderBy(b => b.Id)
                    .ToListAsync();

                for (int i = 1; i < structures.Count; i++)
                {
                    var s = structures[i];
                    Building sibling = i - 1 < existingSiblings.Count
                        ? existingSiblings[i - 1]
                        : CloneRootTemplate(rootBuilding, s.BlockName);

                    if (sibling.Id == 0)
                    {
                        _context.Buildings.Add(sibling);
                        await _context.SaveChangesAsync();
                    }
                    await SyncRootStructureAsync(sibling, s, isPrimary: false);
                }

                var complex = await _context.HousingComplexes.FirstOrDefaultAsync(c => c.Id == rootBuilding.HousingComplexId.Value);
                if (complex != null)
                {
                    complex.TotalBlocks = await _context.Buildings.CountAsync(b => b.HousingComplexId == complex.Id);
                }
            }

            // 3. Bina Yönetim Kadrosu (HeadManager) ve KVKK/KMK Taahhüt Logu
            if (!string.IsNullOrEmpty(userId))
            {
                bool hasHead = await _context.BuildingManagers.AnyAsync(m =>
                    m.BuildingId == rootBuilding.Id && m.UserId == userId && m.IsActive);
                if (!hasHead)
                {
                    _context.BuildingManagers.Add(new BuildingManager
                    {
                        BuildingId = rootBuilding.Id,
                        UserId = userId,
                        Role = BuildingManagerRole.HeadManager,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            int totalBlocks = structures.Sum(s => s.LayoutPattern == "Tekil Yapı" ? 1 : Math.Max(1, s.Children?.Count ?? 1));
            int totalUnits = structures.Sum(s => s.TotalApartments + s.TotalShops + (s.Children?.Sum(c => c.TotalApartments + c.TotalShops) ?? 0));
            var pricing = GMK360.Web.Helpers.FacilityPricingHelper.Calculate(isSite, totalBlocks, totalUnits);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            _context.AuditLogs.Add(new GMK360.Core.Entities.Auditing.AuditLog
            {
                UserId = userId ?? "system",
                ActionType = "FACILITY_ONBOARDING_CONSENT",
                EntityName = "Building",
                EntityId = rootBuilding.Id.ToString(),
                OldValues = "{}",
                NewValues = System.Text.Json.JsonSerializer.Serialize(new
                {
                    IpAddress = ip,
                    KvkkAccepted = true,
                    Mode = isSite ? "Site" : "Apartman",
                    pricing.TierName,
                    pricing.AnnualPrice,
                    pricing.BlockCount,
                    pricing.UnitCount,
                    AcceptedAtUtc = DateTime.UtcNow
                }),
                AffectedColumns = "OnboardingStep,BuildingManagers",
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

            rootBuilding.OnboardingStep = 3;
            await _context.SaveChangesAsync();

            return Json(new { success = true, redirectUrl = Url.Action(nameof(Detail), new { id = rootBuilding.Id }) });
        }

        private static string NormalizeAddressToken(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            return System.Text.RegularExpressions.Regex.Replace(input.Trim().ToLowerInvariant(), @"\s+", " ");
        }

        private async Task<Building?> FindDuplicateAddressBuildingAsync(
            int cityId,
            int districtId,
            int neighborhoodId,
            int? streetId,
            string? streetName,
            string? buildingNumber,
            int? excludeBuildingId)
        {
            var normNo = NormalizeAddressToken(buildingNumber);
            var normStreet = NormalizeAddressToken(streetName);

            var candidates = await _context.Buildings
                .Where(b => b.ParentBuildingId == null
                    && b.CityId == cityId
                    && b.DistrictId == districtId
                    && b.NeighborhoodId == neighborhoodId
                    && (!excludeBuildingId.HasValue || b.Id != excludeBuildingId.Value))
                .ToListAsync();

            return candidates.FirstOrDefault(b =>
                NormalizeAddressToken(b.BuildingNumber) == normNo
                && ((streetId.HasValue && streetId.Value > 0 && b.StreetId == streetId.Value)
                    || (!string.IsNullOrEmpty(normStreet) && NormalizeAddressToken(b.StreetName) == normStreet)));
        }

        private static string BuildTechnicalFeaturesString(string managementMode, string? ada, string? parsel, string? extraLabel)
        {
            var parts = new System.Collections.Generic.List<string> { $"YapiTuru:{managementMode}" };
            if (!string.IsNullOrWhiteSpace(ada)) parts.Add($"Ada:{ada.Trim()}");
            if (!string.IsNullOrWhiteSpace(parsel)) parts.Add($"Parsel:{parsel.Trim()}");
            if (!string.IsNullOrWhiteSpace(extraLabel)) parts.Add($"EkNo:{extraLabel.Trim()}");
            return string.Join(" | ", parts);
        }

        private static string? ParseTechnicalTag(string? features, string key)
        {
            if (string.IsNullOrWhiteSpace(features)) return null;
            var match = System.Text.RegularExpressions.Regex.Match(features, $@"(?:^|\|\s*){System.Text.RegularExpressions.Regex.Escape(key)}:([^|]+)");
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }

        private async Task<string> SaveUploadedBuildingFileAsync(int buildingId, string subFolder, Microsoft.AspNetCore.Http.IFormFile file, string ext)
        {
            var env = HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.Hosting.IWebHostEnvironment)) as Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
            var webRoot = env?.WebRootPath ?? System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
            var folder = System.IO.Path.Combine(webRoot, "uploads", "buildings", buildingId.ToString(), subFolder);
            System.IO.Directory.CreateDirectory(folder);

            var uniqueName = $"{subFolder}_{buildingId}_{Guid.NewGuid():N}{ext}";
            var fullPath = System.IO.Path.Combine(folder, uniqueName);
            using (var stream = new System.IO.FileStream(fullPath, System.IO.FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return $"/uploads/buildings/{buildingId}/{subFolder}/{uniqueName}";
        }

        private static Building CloneRootTemplate(Building source, string blockName)
        {
            return new Building
            {
                Name = $"{source.Name} - {blockName}",
                BlockName = blockName,
                HousingComplexId = source.HousingComplexId,
                ManagerUserId = source.ManagerUserId,
                CreatedByUserId = source.CreatedByUserId,
                TaxNumber = source.TaxNumber,
                CityId = source.CityId,
                DistrictId = source.DistrictId,
                NeighborhoodId = source.NeighborhoodId,
                StreetId = source.StreetId,
                StreetName = source.StreetName,
                BuildingNumber = source.BuildingNumber,
                Latitude = source.Latitude,
                Longitude = source.Longitude,
                TechnicalFeatures = source.TechnicalFeatures,
                BuildingAge = source.BuildingAge,
                CreatedAt = DateTime.Now
            };
        }

        private async Task SyncRootStructureAsync(Building target, GMK360.Web.Models.FacilityStep2StructureDto dto, bool isPrimary)
        {
            bool hasMultiChildren = dto.LayoutPattern != "Tekil Yapı" && dto.Children != null && dto.Children.Any();
            target.BlockName = string.IsNullOrWhiteSpace(dto.BlockName) ? "Ana Bina" : dto.BlockName.Trim();
            if (!isPrimary) target.Name = target.BlockName;
            target.LayoutPattern = dto.LayoutPattern;
            target.BaseArea = dto.BaseArea;
            target.TotalFloors = dto.TotalFloors;
            target.BasementFloors = dto.BasementFloors;
            target.HasGroundFloor = dto.HasGroundFloor;
            target.HasRoof = dto.HasRoof;
            target.RoofType = dto.RoofType;
            target.TotalApartments = dto.TotalApartments;
            target.TotalShops = dto.TotalShops;
            target.TotalUnits = dto.TotalApartments + dto.TotalShops;
            target.HasBlock = true;
            target.HasBlocks = hasMultiChildren;
            target.OnboardingStep = 3;
            await _context.SaveChangesAsync();

            // Ortak baza / podyum veya tekil yapı birimlerini ve ortak alanları üret
            await GenerateFloorUnitsIfEmptyAsync(target, dto.TotalApartments, dto.TotalShops, dto.TotalFloors, dto.HasGroundFloor);
            await EnsureBaselineCommonSpacesAsync(target);

            if (!hasMultiChildren) return;

            var existingChildren = await _context.Buildings
                .Where(b => b.ParentBuildingId == target.Id)
                .OrderBy(b => b.Id)
                .ToListAsync();

            for (int cIdx = 0; cIdx < dto.Children.Count; cIdx++)
            {
                var cDto = dto.Children[cIdx];
                Building child = cIdx < existingChildren.Count
                    ? existingChildren[cIdx]
                    : CloneRootTemplate(target, cDto.BlockName);

                child.ParentBuildingId = target.Id;
                child.BlockName = string.IsNullOrWhiteSpace(cDto.BlockName) ? $"{(char)('A' + cIdx)} Blok" : cDto.BlockName.Trim();
                child.Name = $"{target.Name} - {child.BlockName}";
                child.LayoutPattern = dto.LayoutPattern;
                child.TotalFloors = cDto.TotalFloors;
                child.TotalApartments = cDto.TotalApartments;
                child.TotalShops = cDto.TotalShops;
                child.TotalUnits = cDto.TotalApartments + cDto.TotalShops;
                child.BasementFloors = cDto.BasementFloors;
                child.HasGroundFloor = cDto.HasGroundFloor;
                child.HasRoof = cDto.HasRoof;
                child.RoofType = cDto.RoofType;
                child.HasBlock = true;
                child.OnboardingStep = 3;

                if (child.Id == 0) _context.Buildings.Add(child);
                await _context.SaveChangesAsync();

                await GenerateFloorUnitsIfEmptyAsync(child, cDto.TotalApartments, cDto.TotalShops, cDto.TotalFloors, cDto.HasGroundFloor);
                await EnsureBaselineCommonSpacesAsync(child);
            }
        }

        private async Task GenerateFloorUnitsIfEmptyAsync(Building building, int apartmentCount, int shopCount, int totalFloors, bool hasGroundFloor)
        {
            bool alreadyHasLivingUnits = await _context.BuildingUnits
                .AnyAsync(u => u.BuildingId == building.Id && !(u.Category ?? "").StartsWith("OrtakAlan"));
            if (alreadyHasLivingUnits) return;

            var newUnits = new System.Collections.Generic.List<BuildingUnit>();
            int normalFloors = Math.Max(1, totalFloors);

            if (apartmentCount > 0)
            {
                int perFloor = (int)Math.Ceiling((double)apartmentCount / normalFloors);
                int created = 0;
                for (int fl = 1; fl <= normalFloors && created < apartmentCount; fl++)
                {
                    for (int slot = 0; slot < perFloor && created < apartmentCount; slot++)
                    {
                        created++;
                        newUnits.Add(new BuildingUnit
                        {
                            BuildingId = building.Id,
                            FloorLevel = fl,
                            FloorName = $"{fl}. Kat",
                            DoorNumber = created.ToString(),
                            UnitNumber = created.ToString(),
                            Category = "Daire",
                            RoomLayout = "3+1",
                            IsEmpty = true
                        });
                    }
                }
            }

            if (shopCount > 0)
            {
                int shopFloor = hasGroundFloor ? 0 : 1;
                string shopFloorName = hasGroundFloor ? "Zemin Kat" : "1. Kat";
                for (int s = 1; s <= shopCount; s++)
                {
                    newUnits.Add(new BuildingUnit
                    {
                        BuildingId = building.Id,
                        FloorLevel = shopFloor,
                        FloorName = shopFloorName,
                        DoorNumber = $"D{s}",
                        UnitNumber = $"D{s}",
                        Category = "Dükkan",
                        RoomLayout = "Dükkan",
                        IsEmpty = true
                    });
                }
            }

            if (newUnits.Any())
            {
                _context.BuildingUnits.AddRange(newUnits);
                await _context.SaveChangesAsync();
            }
        }

        private async Task EnsureBaselineCommonSpacesAsync(Building building)
        {
            bool exists = await _context.BuildingUnits
                .AnyAsync(u => u.BuildingId == building.Id && (u.Category == "OrtakAlan_Blok" || (u.Category ?? "").StartsWith("OrtakAlan")));
            if (exists) return;

            var commonUnit = new BuildingUnit
            {
                BuildingId = building.Id,
                FloorLevel = 0,
                FloorName = "Ortak Alanlar",
                DoorNumber = "ORTAK",
                UnitNumber = "ORTAK",
                Category = "OrtakAlan_Blok",
                IsEmpty = false
            };
            _context.BuildingUnits.Add(commonUnit);
            await _context.SaveChangesAsync();

            var spaces = new System.Collections.Generic.List<UnitSpace>
            {
                new UnitSpace { BuildingUnitId = commonUnit.Id, Name = "Merdiven & Sahanlık", Type = "OrtakAlan" }
            };
            if (building.HasRoof)
            {
                spaces.Add(new UnitSpace { BuildingUnitId = commonUnit.Id, Name = $"Çatı ({building.RoofType ?? "Ana Çatı"})", Type = "OrtakAlan" });
            }
            if ((building.TotalFloors ?? 0) >= 4)
            {
                spaces.Add(new UnitSpace { BuildingUnitId = commonUnit.Id, Name = "Asansör & Makine Dairesi", Type = "OrtakAlan" });
            }
            if (building.BasementFloors > 0)
            {
                spaces.Add(new UnitSpace { BuildingUnitId = commonUnit.Id, Name = "Bodrum, Hidrofor & Otopark", Type = "OrtakAlan" });
            }

            _context.UnitSpaces.AddRange(spaces);
            await _context.SaveChangesAsync();
        }


        // 3. Bina Detay (Yönetim Paneli)
        public async Task<IActionResult> Detail(int id)
        {
            var userId = GetUserId();
            var building = await _context.Buildings
                .Include(b => b.HousingComplex)
                .Include(b => b.City)
                .Include(b => b.District)
                .Include(b => b.ChildBuildings)
                    .ThenInclude(cb => cb.Units)
                        .ThenInclude(u => u.Debts)
                .Include(b => b.BuildingManagers)
                    .ThenInclude(m => m.User)
                .Include(b => b.Units)
                    .ThenInclude(u => u.Debts)
                        .ThenInclude(d => d.BuildingExpense)
                .Include(b => b.Expenses)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (building == null) return NotFound();
            if (!CanAccessBuilding(building, userId)) return Forbid();

            int rootId = building.ParentBuildingId ?? building.Id;

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
                .Where(u => u.BuildingId == id && (u.Category == "OrtakAlan_Blok" || (u.Category ?? "").StartsWith("OrtakAlan")))
                .ToListAsync();

            ViewBag.CommonSpaces = commonUnits.SelectMany(u => u.Spaces ?? new List<UnitSpace>()).ToList();

            bool hasAuthorityDoc = await HasDecisionAuthorityAsync(rootId);
            var governanceMembers = await _context.BuildingManagers
                .Include(m => m.User)
                .Where(m => m.BuildingId == rootId && m.IsActive)
                .OrderBy(m => m.Role)
                .ToListAsync();

            bool isSite = building.HousingComplexId.HasValue || (building.ChildBuildings?.Any() ?? false);
            int blockCount = building.ChildBuildings != null && building.ChildBuildings.Any() ? building.ChildBuildings.Count : 1;
            if (building.HousingComplexId.HasValue)
            {
                blockCount = Math.Max(blockCount, await _context.Buildings.CountAsync(b => b.HousingComplexId == building.HousingComplexId && b.ParentBuildingId == null));
            }
            int livingUnitCount = (building.Units?.Count(u => !(u.Category ?? "").StartsWith("OrtakAlan")) ?? 0)
                + (building.ChildBuildings?.Sum(cb => cb.Units?.Count(u => !(u.Category ?? "").StartsWith("OrtakAlan")) ?? 0) ?? 0);

            ViewBag.HasDecisionAuthority = hasAuthorityDoc;
            ViewBag.GovernanceMembers = governanceMembers;
            ViewBag.PricingTier = GMK360.Web.Helpers.FacilityPricingHelper.Calculate(isSite, blockCount, livingUnitCount);
            ViewBag.RootBuildingId = rootId;

            return View(building);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignGovernanceMember(int buildingId, string email, BuildingManagerRole role)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["ErrorMessage"] = "Lütfen kurul üyesi / aza e-posta adresini giriniz.";
                return RedirectToAction(nameof(Detail), new { id = buildingId });
            }

            int rootId = building.ParentBuildingId ?? building.Id;
            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email.Trim() || u.UserName == email.Trim());
            if (targetUser == null)
            {
                TempData["ErrorMessage"] = $"'{email}' adresine sahip kayıtlı bir GMK360 kullanıcısı bulunamadı. Önce üye olması gerekir.";
                return RedirectToAction(nameof(Detail), new { id = buildingId });
            }

            var existing = await _context.BuildingManagers
                .FirstOrDefaultAsync(m => m.BuildingId == rootId && m.UserId == targetUser.Id && m.IsActive);
            if (existing != null)
            {
                existing.Role = role;
            }
            else
            {
                _context.BuildingManagers.Add(new BuildingManager
                {
                    BuildingId = rootId,
                    UserId = targetUser.Id,
                    Role = role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.AuditLogs.Add(new GMK360.Core.Entities.Auditing.AuditLog
            {
                UserId = GetUserId() ?? "system",
                ActionType = "GOVERNANCE_ASSIGN",
                EntityName = "BuildingManager",
                EntityId = rootId.ToString(),
                OldValues = "{}",
                NewValues = System.Text.Json.JsonSerializer.Serialize(new { TargetUserId = targetUser.Id, targetUser.Email, Role = role.ToString() }),
                AffectedColumns = "BuildingManagers",
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{targetUser.FirstName} {targetUser.LastName} ({role}) yönetim kadrosuna eklendi.";
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferHeadManager(int buildingId, string newManagerEmail)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            int rootId = building.ParentBuildingId ?? building.Id;
            var newUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == newManagerEmail.Trim() || u.UserName == newManagerEmail.Trim());
            if (newUser == null)
            {
                TempData["ErrorMessage"] = $"'{newManagerEmail}' ile kayıtlı kullanıcı bulunamadı. Yeni yöneticinin önce GMK360'a kayıtlı olması gerekir.";
                return RedirectToAction(nameof(Detail), new { id = buildingId });
            }

            var currentManagers = await _context.BuildingManagers
                .Where(m => m.BuildingId == rootId && m.IsActive && m.Role == BuildingManagerRole.HeadManager)
                .ToListAsync();
            foreach (var mgr in currentManagers)
            {
                mgr.IsActive = false; // Geçmişi korumak için silinmez, arşivlenir
            }

            _context.BuildingManagers.Add(new BuildingManager
            {
                BuildingId = rootId,
                UserId = newUser.Id,
                Role = BuildingManagerRole.HeadManager,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            var relatedBuildings = await _context.Buildings
                .Where(b => b.Id == rootId || b.ParentBuildingId == rootId || (building.HousingComplexId.HasValue && b.HousingComplexId == building.HousingComplexId))
                .ToListAsync();
            foreach (var b in relatedBuildings)
            {
                b.ManagerUserId = newUser.Id;
            }

            _context.AuditLogs.Add(new GMK360.Core.Entities.Auditing.AuditLog
            {
                UserId = GetUserId() ?? "system",
                ActionType = "HEAD_MANAGER_TRANSFER",
                EntityName = "Building",
                EntityId = rootId.ToString(),
                OldValues = System.Text.Json.JsonSerializer.Serialize(new { PreviousManagers = currentManagers.Select(m => m.UserId) }),
                NewValues = System.Text.Json.JsonSerializer.Serialize(new { NewHeadManagerUserId = newUser.Id, newUser.Email }),
                AffectedColumns = "ManagerUserId,BuildingManagers",
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Yönetim yetkisi tüm bina geçmişi korunarak {newUser.Email} hesabına devredildi.";
            return RedirectToAction(nameof(Index));
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

            bool hasResidentContact = !string.IsNullOrWhiteSpace(ownerPhone)
                || !string.IsNullOrWhiteSpace(tenantPhone)
                || !string.IsNullOrWhiteSpace(ownerEmail)
                || !string.IsNullOrWhiteSpace(tenantEmail);
            if (hasResidentContact && !await HasDecisionAuthorityAsync(buildingId))
            {
                TempData["ErrorMessage"] = "KVKK ve KMK gereği sakinlerin telefon/e-posta bilgilerini eklemeden önce Karar Defteri Yetki Yazısını yüklemelisiniz.";
                return RedirectToAction(nameof(Detail), new { id = buildingId });
            }

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
