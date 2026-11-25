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
using GMK360.Web.Services;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class BuildingManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBuildingManagementService _buildingManagementService;
        private readonly FacilityGovernanceService _governanceService;
        private readonly FacilityPayrollAndBudgetService _payrollService;

        public BuildingManagerController(
            ApplicationDbContext context,
            IBuildingManagementService buildingManagementService,
            FacilityGovernanceService governanceService,
            FacilityPayrollAndBudgetService payrollService)
        {
            _context = context;
            _buildingManagementService = buildingManagementService;
            _governanceService = governanceService;
            _payrollService = payrollService;
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

        private async Task<System.Collections.Generic.List<Building>> GetUserManagedRootBuildingsAsync()
        {
            var userId = GetUserId();
            var query = _context.Buildings
                .Where(b => b.ParentBuildingId == null)
                .AsQueryable();

            if (!IsAdminOrSuperAdmin())
            {
                query = query.Where(b =>
                    b.BuildingManagers.Any(m => m.IsActive)
                        ? b.BuildingManagers.Any(m => m.UserId == userId && m.IsActive)
                        : (b.ManagerUserId == userId || b.CreatedByUserId == userId));
            }

            return await query.OrderByDescending(b => b.Id).ToListAsync();
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
        public async Task<IActionResult> Onboarding(int? id, string? mode = null, string? name = null, int count = 1)
        {
            var userId = GetUserId();
            ViewBag.RequestedStructureCount = Math.Clamp(count, 1, 30);
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
                vm.LandAreaSqm = ParseTechnicalTag(building.TechnicalFeatures, "AraziM2");
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
                    ? await _context.Buildings
                        .Include(b => b.ChildBuildings).ThenInclude(c => c.Units)
                        .Include(b => b.Units).ThenInclude(u => u.Spaces)
                        .Where(b => b.HousingComplexId == building.HousingComplexId && b.ParentBuildingId == null)
                        .OrderBy(b => b.Id)
                        .ToListAsync()
                    : await _context.Buildings
                        .Include(b => b.ChildBuildings).ThenInclude(c => c.Units)
                        .Include(b => b.Units).ThenInclude(u => u.Spaces)
                        .Where(b => b.Id == building.Id)
                        .ToListAsync();

                vm.InitialStructures = rootStructures.Select(rb =>
                {
                    var allUnits = rb.Units ?? new System.Collections.Generic.List<BuildingUnit>();
                    bool IsBuildingWideAbstractTag(string name) =>
                        name.StartsWith("Çatı", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(name, "Merdiven & Sahanlık", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(name, "Asansör & Makine Dairesi", StringComparison.OrdinalIgnoreCase);

                    var physicalTechUnits = allUnits
                        .Where(u => (u.Category == "OrtakAlan_Tesis" || u.Category == "OrtakAlan_Depo")
                                 && !IsBuildingWideAbstractTag(u.DoorNumber))
                        .OrderBy(u => u.FloorLevel).ThenBy(u => u.Id)
                        .ToList();

                    var mappedCommonSpaces = physicalTechUnits.Any()
                        ? physicalTechUnits.Select(u =>
                        {
                            var firstSpace = u.Spaces?.FirstOrDefault();
                            bool isOutdoor = u.FloorLevel == -99 || firstSpace?.Type == "CevreDuzenleme";
                            return new GMK360.Web.Models.FacilityCommonSpaceDto
                            {
                                Name = u.DoorNumber,
                                SpaceGroup = isOutdoor ? "CevreDuzenleme" : "BinaIci",
                                FloorLevel = u.FloorLevel,
                                SquareMeters = u.GrossSquareMeters ?? firstSpace?.SquareMeters,
                                Description = !string.IsNullOrWhiteSpace(firstSpace?.Description) ? firstSpace.Description : u.RoomLayout
                            };
                        }).ToList()
                        : allUnits
                            .Where(u => u.Category == "OrtakAlan_Blok" || (u.Category ?? "").StartsWith("OrtakAlan"))
                            .SelectMany(u => u.Spaces ?? new System.Collections.Generic.List<UnitSpace>())
                            .Where(s => s.Name != "Sabit Gider & Muhasebe" && !IsBuildingWideAbstractTag(s.Name))
                            .Select(s =>
                            {
                                int guessedFloor = s.Type == "CevreDuzenleme" ? -99
                                    : (s.Name.Contains("Bodrum", StringComparison.OrdinalIgnoreCase)
                                       || s.Name.Contains("Sığınak", StringComparison.OrdinalIgnoreCase)
                                       || s.Name.Contains("Pompa", StringComparison.OrdinalIgnoreCase)
                                       || s.Name.Contains("Hidrofor", StringComparison.OrdinalIgnoreCase)
                                       || s.Name.Contains("Depo", StringComparison.OrdinalIgnoreCase)
                                       || s.Name.Contains("Ardiye", StringComparison.OrdinalIgnoreCase)
                                       || s.Name.Contains("Kömür", StringComparison.OrdinalIgnoreCase)
                                       || s.Name.Contains("Kazan", StringComparison.OrdinalIgnoreCase)
                                       || s.Name.Contains("Jeneratör", StringComparison.OrdinalIgnoreCase)
                                       || s.Name.Contains("Garaj", StringComparison.OrdinalIgnoreCase) ? -1 : 0);
                                return new GMK360.Web.Models.FacilityCommonSpaceDto
                                {
                                    Name = s.Name,
                                    SpaceGroup = s.Type == "CevreDuzenleme" ? "CevreDuzenleme" : "BinaIci",
                                    FloorLevel = guessedFloor,
                                    SquareMeters = s.SquareMeters,
                                    Description = s.Description
                                };
                            }).ToList();

                    return new GMK360.Web.Models.FacilityStep2StructureDto
                    {
                        LayoutPattern = rb.LayoutPattern ?? "Tekil Yapı",
                        BlockName = string.IsNullOrWhiteSpace(rb.BlockName) ? "Ana Bina" : rb.BlockName,
                        BaseArea = rb.BaseArea,
                        TotalFloors = rb.TotalFloors ?? 5,
                        GroundApartments = allUnits
                            .Count(u => u.FloorLevel == 0 && u.Category != "Dukkan" && u.Category != "Dükkan" && !(u.Category ?? "").StartsWith("OrtakAlan")),
                        TotalApartments = rb.TotalApartments,
                        TotalShops = rb.TotalShops,
                        BasementFloors = rb.BasementFloors,
                        HasGroundFloor = rb.HasGroundFloor,
                        HasRoof = rb.HasRoof,
                        RoofType = rb.RoofType ?? "Düz Teras Çatı (Tabliye)",
                        IndoorCommonAreas = mappedCommonSpaces.Where(s => s.SpaceGroup != "CevreDuzenleme").Select(s => s.Name).ToList(),
                        OutdoorCommonAreas = mappedCommonSpaces.Where(s => s.SpaceGroup == "CevreDuzenleme").Select(s => s.Name).ToList(),
                        CommonSpaces = mappedCommonSpaces,
                        Children = (rb.ChildBuildings ?? new System.Collections.Generic.List<Building>())
                            .OrderBy(c => c.Id)
                            .Select(c => new GMK360.Web.Models.FacilityStep2ChildBlockDto
                            {
                                BlockName = c.BlockName,
                                TotalFloors = c.TotalFloors ?? 8,
                                GroundApartments = (c.Units ?? new System.Collections.Generic.List<BuildingUnit>())
                                    .Count(u => u.FloorLevel == 0 && u.Category != "Dukkan" && u.Category != "Dükkan" && !(u.Category ?? "").StartsWith("OrtakAlan")),
                                TotalApartments = c.TotalApartments,
                                TotalShops = c.TotalShops,
                                BasementFloors = c.BasementFloors,
                                HasGroundFloor = c.HasGroundFloor,
                                HasRoof = c.HasRoof,
                                RoofType = c.RoofType ?? "Düz Teras Çatı (Tabliye)"
                            }).ToList()
                    };
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
                model.ExtraAddressLabel,
                model.LandAreaSqm);
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

            try
            {
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
                    // Site modunda dış çevre yalnızca ana site kök yapısında tek kez tutulur; ikincil bloklarda yalnızca bina içi kalır
                    s.OutdoorCommonAreas = new System.Collections.Generic.List<string>();
                    if (s.CommonSpaces != null)
                    {
                        s.CommonSpaces = s.CommonSpaces.Where(cs => cs.SpaceGroup != "CevreDuzenleme" && cs.FloorLevel != -99).ToList();
                    }
                    Building sibling = i - 1 < existingSiblings.Count
                        ? existingSiblings[i - 1]
                        : CloneRootTemplate(rootBuilding, s.BlockName);

                    if (sibling.Id == 0)
                    {
                        _context.Buildings.Add(sibling);
                        await _context.SaveChangesAsync();
                    }
                    if (!string.IsNullOrEmpty(userId))
                    {
                        bool siblingHasHead = await _context.BuildingManagers.AnyAsync(m =>
                            m.BuildingId == sibling.Id && m.UserId == userId && m.IsActive);
                        if (!siblingHasHead)
                        {
                            _context.BuildingManagers.Add(new BuildingManager
                            {
                                BuildingId = sibling.Id,
                                UserId = userId,
                                Role = BuildingManagerRole.HeadManager,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
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
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Yapı oluşturulurken hata: {ex.GetBaseException().Message}" });
            }
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

        private static string BuildTechnicalFeaturesString(string managementMode, string? ada, string? parsel, string? extraLabel, string? landAreaSqm = null)
        {
            var parts = new System.Collections.Generic.List<string> { $"YapiTuru:{managementMode}" };
            if (!string.IsNullOrWhiteSpace(ada)) parts.Add($"Ada:{ada.Trim()}");
            if (!string.IsNullOrWhiteSpace(parsel)) parts.Add($"Parsel:{parsel.Trim()}");
            if (!string.IsNullOrWhiteSpace(landAreaSqm)) parts.Add($"AraziM2:{landAreaSqm.Trim()}");
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

            // Ortak baza / podyum veya tekil yapı birimlerini ve bina içi / çevre düzenleme ortak alanlarını üret
            await GenerateFloorUnitsIfEmptyAsync(target, dto.TotalApartments, dto.TotalShops, dto.TotalFloors, dto.HasGroundFloor, dto.GroundApartments, allowResyncIfClean: true);
            await EnsureBaselineCommonSpacesAsync(target, dto.IndoorCommonAreas, dto.OutdoorCommonAreas, dto.CommonSpaces);

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

                await GenerateFloorUnitsIfEmptyAsync(child, cDto.TotalApartments, cDto.TotalShops, cDto.TotalFloors, cDto.HasGroundFloor, cDto.GroundApartments, allowResyncIfClean: true);
                await EnsureBaselineCommonSpacesAsync(child, null, null);
            }
        }

        private async Task GenerateFloorUnitsIfEmptyAsync(
            Building building,
            int apartmentCount,
            int shopCount,
            int totalFloors,
            bool hasGroundFloor,
            int groundApartments = 0,
            bool allowResyncIfClean = false)
        {
            var existingLivingUnits = await _context.BuildingUnits
                .Include(u => u.Debts)
                .Where(u => u.BuildingId == building.Id && !(u.Category ?? "").StartsWith("OrtakAlan"))
                .ToListAsync();

            if (existingLivingUnits.Any())
            {
                // Güvenlik: Kayıtlı malik, kiracı veya borcu olmayan boş taslak daireler Sihirbaz'dan kat/zemin düzeltmesiyle güncellenebilir
                bool isCleanTemplate = allowResyncIfClean && existingLivingUnits.All(u =>
                    string.IsNullOrWhiteSpace(u.OwnerName)
                    && string.IsNullOrWhiteSpace(u.TenantName)
                    && string.IsNullOrWhiteSpace(u.OwnerUserId)
                    && string.IsNullOrWhiteSpace(u.TenantUserId)
                    && !(u.Debts?.Any() ?? false));

                if (!isCleanTemplate) return;

                _context.BuildingUnits.RemoveRange(existingLivingUnits);
                await _context.SaveChangesAsync();
            }

            var newUnits = new System.Collections.Generic.List<BuildingUnit>();
            int normalFloors = Math.Max(1, totalFloors);
            int createdApt = 0;

            // 1. Zemin Kattaki Daireler (örn: Zemin katta 1 daire + 2 dükkan)
            int actualGroundApts = (hasGroundFloor || groundApartments > 0)
                ? Math.Clamp(groundApartments, 0, apartmentCount)
                : 0;
            for (int g = 0; g < actualGroundApts; g++)
            {
                createdApt++;
                newUnits.Add(new BuildingUnit
                {
                    BuildingId = building.Id,
                    FloorLevel = 0,
                    FloorName = "Zemin Kat",
                    DoorNumber = createdApt.ToString(),
                    UnitNumber = createdApt.ToString(),
                    Category = "Daire",
                    RoomLayout = "2+1",
                    IsEmpty = true
                });
            }

            // 2. Normal Katlara Kalan Dairelerin Dağılımı (1..normalFloors)
            int remainingApts = Math.Max(0, apartmentCount - createdApt);
            if (remainingApts > 0)
            {
                int basePerFloor = remainingApts / normalFloors;
                int extra = remainingApts % normalFloors;
                for (int fl = 1; fl <= normalFloors && createdApt < apartmentCount; fl++)
                {
                    int onThisFloor = basePerFloor + (fl <= extra ? 1 : 0);
                    for (int slot = 0; slot < onThisFloor && createdApt < apartmentCount; slot++)
                    {
                        createdApt++;
                        newUnits.Add(new BuildingUnit
                        {
                            BuildingId = building.Id,
                            FloorLevel = fl,
                            FloorName = $"{fl}. Kat",
                            DoorNumber = createdApt.ToString(),
                            UnitNumber = createdApt.ToString(),
                            Category = "Daire",
                            RoomLayout = "3+1",
                            IsEmpty = true
                        });
                    }
                }
            }

            // 3. Zemin / Cadde Dükkanları
            if (shopCount > 0)
            {
                int shopFloor = (hasGroundFloor || groundApartments > 0) ? 0 : 1;
                string shopFloorName = shopFloor == 0 ? "Zemin Kat" : "1. Kat";
                for (int s = 1; s <= shopCount; s++)
                {
                    newUnits.Add(new BuildingUnit
                    {
                        BuildingId = building.Id,
                        FloorLevel = shopFloor,
                        FloorName = shopFloorName,
                        DoorNumber = $"D{s}",
                        UnitNumber = $"D{s}",
                        Category = "Dukkan",
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

        private static string FormatFacilityFloorName(int floorLevel)
        {
            if (floorLevel == -99) return "Bahçe & Çevre Alanı";
            if (floorLevel == 99) return "Çatı / Teras";
            if (floorLevel < 0) return $"{Math.Abs(floorLevel)}. Bodrum";
            if (floorLevel == 0) return "Zemin Kat";
            return $"{floorLevel}. Kat";
        }

        private static bool IsStorageSpaceName(string name)
        {
            return name.Contains("Depo", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Ardiye", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Kömür", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Kiler", StringComparison.OrdinalIgnoreCase);
        }

        private async Task EnsureBaselineCommonSpacesAsync(
            Building building,
            System.Collections.Generic.List<string>? indoorAreas = null,
            System.Collections.Generic.List<string>? outdoorAreas = null,
            System.Collections.Generic.List<GMK360.Web.Models.FacilityCommonSpaceDto>? customCommonSpaces = null)
        {
            var commonUnit = await _context.BuildingUnits
                .Include(u => u.Spaces)
                .FirstOrDefaultAsync(u => u.BuildingId == building.Id && u.Category == "OrtakAlan_Blok");

            if (commonUnit == null)
            {
                commonUnit = new BuildingUnit
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
            }

            if (customCommonSpaces != null && customCommonSpaces.Any())
            {
                var existingPhysicalTech = await _context.BuildingUnits
                    .Include(u => u.Spaces)
                        .ThenInclude(s => s.Fixtures)
                    .Where(u => u.BuildingId == building.Id && (u.Category == "OrtakAlan_Tesis" || u.Category == "OrtakAlan_Depo"))
                    .ToListAsync();

                var desiredNames = customCommonSpaces
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                    .Select(x => x.Name.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                bool IsWholeBuildingTag(string n) =>
                    n.StartsWith("Çatı", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(n, "Merdiven & Sahanlık", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(n, "Asansör & Makine Dairesi", StringComparison.OrdinalIgnoreCase);

                // Demirbaşı olmayan kaldırılmış fiziksel teknik birimleri ve Zemin Katı kirleten eski Çatı/Merdiven kartlarını temizle
                var toRemoveUnits = existingPhysicalTech
                    .Where(u => (!desiredNames.Contains(u.DoorNumber) || (u.FloorLevel == 0 && IsWholeBuildingTag(u.DoorNumber)))
                             && !(u.Spaces?.Any(s => s.Fixtures != null && s.Fixtures.Any()) ?? false))
                    .ToList();
                if (toRemoveUnits.Any())
                {
                    var remSpaces = toRemoveUnits.SelectMany(u => u.Spaces ?? new System.Collections.Generic.List<UnitSpace>()).ToList();
                    if (remSpaces.Any()) _context.UnitSpaces.RemoveRange(remSpaces);
                    _context.BuildingUnits.RemoveRange(toRemoveUnits);
                }

                // Eski ORTAK container üzerindeki boş eski mekanları temizle (Sabit Gider hariç), çift gösterimi önle
                var legacyEmptySpaces = (commonUnit.Spaces ?? new System.Collections.Generic.List<UnitSpace>())
                    .Where(s => s.Name != "Sabit Gider & Muhasebe" && !_context.SpaceFixtures.Any(f => f.UnitSpaceId == s.Id))
                    .ToList();
                if (legacyEmptySpaces.Any()) _context.UnitSpaces.RemoveRange(legacyEmptySpaces);

                foreach (var cs in customCommonSpaces.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
                {
                    string cleanName = cs.Name.Trim();
                    string spaceType = cs.SpaceGroup == "CevreDuzenleme" ? "CevreDuzenleme" : "BinaIci";
                    if (IsWholeBuildingTag(cleanName))
                    {
                        _context.UnitSpaces.Add(new UnitSpace
                        {
                            BuildingUnitId = commonUnit.Id,
                            Name = cleanName,
                            Type = spaceType,
                            SquareMeters = cs.SquareMeters,
                            Description = cs.Description?.Trim()
                        });
                        continue;
                    }

                    int targetFloor = cs.FloorLevel;
                    if (spaceType == "CevreDuzenleme") targetFloor = -99;
                    string floorName = FormatFacilityFloorName(targetFloor);
                    string techCat = IsStorageSpaceName(cleanName) ? "OrtakAlan_Depo" : "OrtakAlan_Tesis";

                    var existingUnit = existingPhysicalTech.FirstOrDefault(u => string.Equals(u.DoorNumber, cleanName, StringComparison.OrdinalIgnoreCase) && !toRemoveUnits.Contains(u));
                    if (existingUnit != null)
                    {
                        existingUnit.FloorLevel = targetFloor;
                        existingUnit.FloorName = floorName;
                        existingUnit.Category = techCat;
                        existingUnit.GrossSquareMeters = cs.SquareMeters;
                        existingUnit.RoomLayout = string.IsNullOrWhiteSpace(cs.Description) ? floorName : cs.Description.Trim();

                        var innerSpace = existingUnit.Spaces?.FirstOrDefault();
                        if (innerSpace == null)
                        {
                            _context.UnitSpaces.Add(new UnitSpace
                            {
                                BuildingUnitId = existingUnit.Id,
                                Name = cleanName,
                                Type = spaceType,
                                SquareMeters = cs.SquareMeters,
                                Description = cs.Description?.Trim()
                            });
                        }
                        else
                        {
                            innerSpace.Name = cleanName;
                            innerSpace.Type = spaceType;
                            innerSpace.SquareMeters = cs.SquareMeters;
                            innerSpace.Description = cs.Description?.Trim();
                        }
                    }
                    else
                    {
                        var newTechUnit = new BuildingUnit
                        {
                            BuildingId = building.Id,
                            FloorLevel = targetFloor,
                            FloorName = floorName,
                            DoorNumber = cleanName,
                            UnitNumber = cleanName,
                            Category = techCat,
                            GrossSquareMeters = cs.SquareMeters,
                            RoomLayout = string.IsNullOrWhiteSpace(cs.Description) ? floorName : cs.Description.Trim(),
                            IsEmpty = false,
                            Spaces = new System.Collections.Generic.List<UnitSpace>
                            {
                                new UnitSpace
                                {
                                    Name = cleanName,
                                    Type = spaceType,
                                    SquareMeters = cs.SquareMeters,
                                    Description = cs.Description?.Trim()
                                }
                            }
                        };
                        _context.BuildingUnits.Add(newTechUnit);
                    }
                }

                await _context.SaveChangesAsync();
                return;
            }

            // Geriye dönük / otomatik onarım modu: Bodrum katı varsa en azından Sığınak / Depo başlangıç birimi oluştur
            if (building.BasementFloors > 0)
            {
                bool hasAnyBasementUnit = await _context.BuildingUnits.AnyAsync(u => u.BuildingId == building.Id && u.FloorLevel < 0 && u.FloorLevel != -99);
                if (!hasAnyBasementUnit)
                {
                    for (int b = 1; b <= building.BasementFloors; b++)
                    {
                        int fl = -b;
                        string bTitle = b == 1 ? "Sığınak & Depolama Alanı" : $"{b}. Bodrum Garaj / Tesis";
                        _context.BuildingUnits.Add(new BuildingUnit
                        {
                            BuildingId = building.Id,
                            FloorLevel = fl,
                            FloorName = FormatFacilityFloorName(fl),
                            DoorNumber = bTitle,
                            UnitNumber = bTitle,
                            Category = "OrtakAlan_Tesis",
                            RoomLayout = "Bodrum Ortak & Teknik Alan",
                            GrossSquareMeters = building.BaseArea,
                            IsEmpty = false,
                            Spaces = new System.Collections.Generic.List<UnitSpace>
                            {
                                new UnitSpace { Name = bTitle, Type = "BinaIci", SquareMeters = building.BaseArea, Description = "Bodrum kat ortak teknik hacmi" }
                            }
                        });
                    }
                }
            }

            var existingList = (commonUnit.Spaces ?? new System.Collections.Generic.List<UnitSpace>()).ToList();
            var existingNames = existingList.Select(s => s.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var spacesToAdd = new System.Collections.Generic.List<UnitSpace>();
            if (indoorAreas != null && indoorAreas.Any())
            {
                foreach (var name in indoorAreas.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
                {
                    if (!existingNames.Contains(name.Trim()))
                    {
                        spacesToAdd.Add(new UnitSpace { BuildingUnitId = commonUnit.Id, Name = name.Trim(), Type = "BinaIci" });
                    }
                }
            }

            if (outdoorAreas != null && outdoorAreas.Any())
            {
                foreach (var name in outdoorAreas.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
                {
                    if (!existingNames.Contains(name.Trim()))
                    {
                        spacesToAdd.Add(new UnitSpace { BuildingUnitId = commonUnit.Id, Name = name.Trim(), Type = "CevreDuzenleme" });
                    }
                }
            }

            if (spacesToAdd.Any())
            {
                _context.UnitSpaces.AddRange(spacesToAdd);
            }
            await _context.SaveChangesAsync();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBuildingLandArea(int buildingId, double? landAreaSqm)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            string mode = building.HousingComplexId.HasValue ? "Site" : (ParseTechnicalTag(building.TechnicalFeatures, "YapiTuru") ?? "Apartman");
            string? ada = ParseTechnicalTag(building.TechnicalFeatures, "Ada");
            string? parsel = ParseTechnicalTag(building.TechnicalFeatures, "Parsel");
            string? ekNo = ParseTechnicalTag(building.TechnicalFeatures, "EkNo");
            string? landStr = (landAreaSqm.HasValue && landAreaSqm.Value > 0)
                ? landAreaSqm.Value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)
                : null;

            building.TechnicalFeatures = BuildTechnicalFeaturesString(mode, ada, parsel, ekNo, landStr);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = landAreaSqm.HasValue && landAreaSqm > 0
                ? $"Toplam arazi ölçüsü {landAreaSqm.Value:0.##} m² olarak güncellendi."
                : "Toplam arazi ölçüsü temizlendi.";
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkPopulateFloorUnits(
            int buildingId,
            string category,
            string unitName,
            string targetFloorsCsv,
            double? squareMeters = null,
            int countPerFloor = 1)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();
            if (string.IsNullOrWhiteSpace(unitName) || string.IsNullOrWhiteSpace(targetFloorsCsv))
            {
                TempData["ErrorMessage"] = "Birim adı ve en az bir hedef kat seçimi zorunludur.";
                return RedirectToAction(nameof(Detail), new { id = buildingId });
            }

            string cleanCat = string.IsNullOrWhiteSpace(category) ? "OrtakAlan_Tesis" : category.Trim();
            if (cleanCat == "Dükkan") cleanCat = "Dukkan";
            bool isTechOrStorage = cleanCat.StartsWith("OrtakAlan");
            int count = Math.Clamp(countPerFloor, 1, 20);
            var floors = targetFloorsCsv
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => int.TryParse(s, out int f) ? (int?)f : null)
                .Where(f => f.HasValue)
                .Select(f => f!.Value)
                .Distinct()
                .ToList();

            if (!floors.Any())
            {
                TempData["ErrorMessage"] = "Geçerli bir kat seçilmedi.";
                return RedirectToAction(nameof(Detail), new { id = buildingId });
            }

            int addedCount = 0;
            foreach (var fl in floors)
            {
                string floorName = FormatFacilityFloorName(fl);
                for (int i = 1; i <= count; i++)
                {
                    string door = count > 1 ? $"{unitName.Trim()} {i}" : unitName.Trim();
                    var unit = new BuildingUnit
                    {
                        BuildingId = buildingId,
                        FloorLevel = fl,
                        FloorName = floorName,
                        DoorNumber = door,
                        UnitNumber = door,
                        Category = cleanCat,
                        GrossSquareMeters = squareMeters,
                        RoomLayout = cleanCat == "OrtakAlan_Depo" ? "Depolama / Ardiye" : (isTechOrStorage ? "Teknik / Ortak Birim" : (cleanCat == "Dukkan" ? "Dükkan" : "3+1")),
                        IsEmpty = !isTechOrStorage
                    };
                    if (isTechOrStorage)
                    {
                        unit.Spaces = new System.Collections.Generic.List<UnitSpace>
                        {
                            new UnitSpace
                            {
                                Name = door,
                                Type = fl == -99 ? "CevreDuzenleme" : "BinaIci",
                                SquareMeters = squareMeters
                            }
                        };
                    }
                    else
                    {
                        building.TotalUnits += 1;
                    }
                    _context.BuildingUnits.Add(unit);
                    addedCount++;
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{floors.Count} katta toplam {addedCount} adet '{unitName.Trim()}' birimi üretildi.";
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCommonSpaceToBuilding(int buildingId, string name, string spaceGroup, int floorLevel = -1, double? squareMeters = null, string? description = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Alan adı zorunludur.";
                return RedirectToAction(nameof(Detail), new { id = buildingId });
            }

            string cleanName = name.Trim();
            int targetFloor = spaceGroup == "CevreDuzenleme" ? -99 : floorLevel;
            string spaceType = spaceGroup == "CevreDuzenleme" ? "CevreDuzenleme" : "BinaIci";
            string category = IsStorageSpaceName(cleanName) ? "OrtakAlan_Depo" : "OrtakAlan_Tesis";

            var techUnit = new BuildingUnit
            {
                BuildingId = buildingId,
                FloorLevel = targetFloor,
                FloorName = FormatFacilityFloorName(targetFloor),
                DoorNumber = cleanName,
                UnitNumber = cleanName,
                Category = category,
                GrossSquareMeters = squareMeters,
                RoomLayout = string.IsNullOrWhiteSpace(description) ? FormatFacilityFloorName(targetFloor) : description.Trim(),
                IsEmpty = false,
                Spaces = new System.Collections.Generic.List<UnitSpace>
                {
                    new UnitSpace
                    {
                        Name = cleanName,
                        Type = spaceType,
                        SquareMeters = squareMeters,
                        Description = description?.Trim()
                    }
                }
            };
            _context.BuildingUnits.Add(techUnit);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{cleanName}' ({FormatFacilityFloorName(targetFloor)}) birimi eklendi.";
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        // İnşaat ERP Yapısı ile Uyumlu Kat / Bodrum / Bahçe Birim Ekleme (Pompa Odası, Hidrofor, Depo, Daire, Dükkan)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFloorUnit(
            int buildingId,
            int floorLevel,
            string category,
            string unitName,
            double? squareMeters,
            string? roomLayout,
            string? firstFixtureName = null,
            string? firstFixtureBrand = null,
            string? technicalServiceName = null,
            DateTime? nextMaintenanceDate = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();
            if (string.IsNullOrWhiteSpace(unitName))
            {
                TempData["ErrorMessage"] = "Birim adı / kapı numarası zorunludur.";
                return RedirectToAction(nameof(Detail), new { id = buildingId });
            }

            string cleanName = unitName.Trim();
            string cleanCat = string.IsNullOrWhiteSpace(category) ? "OrtakAlan_Tesis" : category.Trim();
            if (cleanCat == "Dükkan") cleanCat = "Dukkan";
            string floorName = FormatFacilityFloorName(floorLevel);
            bool isTechOrStorage = cleanCat.StartsWith("OrtakAlan");

            var newUnit = new BuildingUnit
            {
                BuildingId = buildingId,
                FloorLevel = floorLevel,
                FloorName = floorName,
                DoorNumber = cleanName,
                UnitNumber = cleanName,
                Category = cleanCat,
                GrossSquareMeters = squareMeters,
                RoomLayout = string.IsNullOrWhiteSpace(roomLayout)
                    ? (cleanCat == "OrtakAlan_Depo" ? "Depolama / Ardiye" : (isTechOrStorage ? "Teknik / Ortak Birim" : (cleanCat == "Dukkan" ? "Dükkan" : "3+1")))
                    : roomLayout.Trim(),
                IsEmpty = !isTechOrStorage
            };

            if (isTechOrStorage)
            {
                var space = new UnitSpace
                {
                    Name = cleanName,
                    Type = floorLevel == -99 ? "CevreDuzenleme" : "BinaIci",
                    SquareMeters = squareMeters,
                    Description = roomLayout?.Trim()
                };
                if (!string.IsNullOrWhiteSpace(firstFixtureName))
                {
                    space.Fixtures = new System.Collections.Generic.List<SpaceFixture>
                    {
                        new SpaceFixture
                        {
                            Category = "Teknik Ekipman",
                            ItemName = firstFixtureName.Trim(),
                            StandardBrand = firstFixtureBrand?.Trim(),
                            TechnicalServiceName = technicalServiceName?.Trim(),
                            NextMaintenanceDate = nextMaintenanceDate,
                            Quantity = 1,
                            Unit = "Adet",
                            CreatedAt = DateTime.Now
                        }
                    };
                }
                newUnit.Spaces = new System.Collections.Generic.List<UnitSpace> { space };
            }
            else
            {
                building.TotalUnits += 1;
            }

            _context.BuildingUnits.Add(newUnit);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{floorName} katına '{cleanName}' birimi eklendi.";
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        private async Task SaveFixtureDocumentIfProvidedAsync(int buildingId, SpaceFixture fixture, Microsoft.AspNetCore.Http.IFormFile? documentFile)
        {
            if (documentFile == null || documentFile.Length == 0) return;
            var ext = System.IO.Path.GetExtension(documentFile.FileName)?.ToLowerInvariant() ?? "";
            var allowed = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext)) return;

            var docUrl = await SaveUploadedBuildingFileAsync(buildingId, "fixtures", documentFile, ext);
            _context.DocumentArchives.Add(new DocumentArchive
            {
                BuildingId = buildingId,
                Title = $"{fixture.ItemName} - Garanti / Bakım Evrağı",
                FileName = documentFile.FileName,
                FileExtension = ext,
                Category = "DemirbasEvrak",
                SourceModule = "SpaceFixture",
                SourceRecordId = fixture.Id,
                DocumentUrl = docUrl,
                Status = "Onaylandı",
                UploadDate = DateTime.Now,
                Year = DateTime.Now.Year,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFixtureToFloorUnit(
            int buildingId,
            int unitId,
            string itemName,
            string? category = "Demirbaş",
            string? brand = null,
            string? model = null,
            double quantity = 1,
            string? unitMeasure = "Adet",
            string? supplierName = null,
            string? serviceName = null,
            DateTime? warrantyExpiryDate = null,
            DateTime? nextMaintenanceDate = null,
            string? notes = null,
            Microsoft.AspNetCore.Http.IFormFile? documentFile = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();
            if (string.IsNullOrWhiteSpace(itemName))
            {
                TempData["ErrorMessage"] = "Demirbaş / ekipman adı zorunludur.";
                return RedirectToAction(nameof(Detail), new { id = buildingId });
            }

            var unit = await _context.BuildingUnits
                .Include(u => u.Spaces)
                .FirstOrDefaultAsync(u => u.Id == unitId && u.BuildingId == buildingId);
            if (unit == null) return NotFound();

            var space = unit.Spaces?.FirstOrDefault();
            if (space == null)
            {
                space = new UnitSpace
                {
                    BuildingUnitId = unit.Id,
                    Name = unit.DoorNumber,
                    Type = unit.FloorLevel == -99 ? "CevreDuzenleme" : "BinaIci",
                    SquareMeters = unit.GrossSquareMeters,
                    Description = unit.RoomLayout
                };
                _context.UnitSpaces.Add(space);
                await _context.SaveChangesAsync();
            }

            var fixture = new SpaceFixture
            {
                UnitSpaceId = space.Id,
                Category = string.IsNullOrWhiteSpace(category) ? "Demirbaş" : category.Trim(),
                ItemName = itemName.Trim(),
                StandardBrand = brand?.Trim(),
                StandardModel = model?.Trim(),
                SupplierName = supplierName?.Trim(),
                TechnicalServiceName = serviceName?.Trim(),
                WarrantyExpiryDate = warrantyExpiryDate,
                NextMaintenanceDate = nextMaintenanceDate,
                MaintenanceNotes = notes?.Trim(),
                Quantity = Math.Max(0.01, quantity),
                Unit = string.IsNullOrWhiteSpace(unitMeasure) ? "Adet" : unitMeasure.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.SpaceFixtures.Add(fixture);
            await _context.SaveChangesAsync();
            await SaveFixtureDocumentIfProvidedAsync(buildingId, fixture, documentFile);

            TempData["SuccessMessage"] = $"'{fixture.ItemName}' ({fixture.Category}) '{unit.DoorNumber}' birimine kaydedildi.";
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFloorFixture(
            int buildingId,
            int fixtureId,
            string itemName,
            string? category = "Demirbaş",
            string? brand = null,
            string? model = null,
            double quantity = 1,
            string? unitMeasure = "Adet",
            string? supplierName = null,
            string? serviceName = null,
            DateTime? warrantyExpiryDate = null,
            DateTime? nextMaintenanceDate = null,
            string? notes = null,
            Microsoft.AspNetCore.Http.IFormFile? documentFile = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var fixture = await _context.SpaceFixtures
                .Include(f => f.UnitSpace)
                    .ThenInclude(s => s.BuildingUnit)
                .FirstOrDefaultAsync(f => f.Id == fixtureId && f.UnitSpace.BuildingUnit.BuildingId == buildingId);
            if (fixture == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(itemName)) fixture.ItemName = itemName.Trim();
            if (!string.IsNullOrWhiteSpace(category)) fixture.Category = category.Trim();
            fixture.StandardBrand = brand?.Trim();
            fixture.StandardModel = model?.Trim();
            fixture.Quantity = Math.Max(0.01, quantity);
            fixture.Unit = string.IsNullOrWhiteSpace(unitMeasure) ? "Adet" : unitMeasure.Trim();
            fixture.SupplierName = supplierName?.Trim();
            fixture.TechnicalServiceName = serviceName?.Trim();
            fixture.WarrantyExpiryDate = warrantyExpiryDate;
            fixture.NextMaintenanceDate = nextMaintenanceDate;
            fixture.MaintenanceNotes = notes?.Trim();

            await _context.SaveChangesAsync();
            await SaveFixtureDocumentIfProvidedAsync(buildingId, fixture, documentFile);

            TempData["SuccessMessage"] = $"'{fixture.ItemName}' teknik kartı / servis bilgisi güncellendi.";
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFloorFixture(int buildingId, int fixtureId)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var fixture = await _context.SpaceFixtures
                .Include(f => f.UnitSpace)
                    .ThenInclude(s => s.BuildingUnit)
                .FirstOrDefaultAsync(f => f.Id == fixtureId && f.UnitSpace.BuildingUnit.BuildingId == buildingId);
            if (fixture == null) return NotFound();

            var docs = await _context.DocumentArchives
                .Where(d => d.BuildingId == buildingId && d.SourceModule == "SpaceFixture" && d.SourceRecordId == fixtureId)
                .ToListAsync();
            if (docs.Any()) _context.DocumentArchives.RemoveRange(docs);

            _context.SpaceFixtures.Remove(fixture);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{fixture.ItemName}' envanterden kaldırıldı.";
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkSwitchTechnicalService(int buildingId, string? categoryFilter, string newServiceName)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();
            if (string.IsNullOrWhiteSpace(newServiceName))
            {
                TempData["ErrorMessage"] = "Yeni teknik servis firma adı ve telefonu zorunludur.";
                return RedirectToAction(nameof(Detail), new { id = buildingId });
            }

            var query = _context.SpaceFixtures
                .Include(f => f.UnitSpace)
                    .ThenInclude(s => s.BuildingUnit)
                .Where(f => f.UnitSpace.BuildingUnit.BuildingId == buildingId);

            if (!string.IsNullOrWhiteSpace(categoryFilter) && categoryFilter != "ALL")
            {
                query = query.Where(f => f.Category == categoryFilter);
            }

            var list = await query.ToListAsync();
            foreach (var fx in list)
            {
                fx.TechnicalServiceName = newServiceName.Trim();
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{list.Count} kalemin yetkili teknik servisi '{newServiceName.Trim()}' olarak güncellendi.";
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        [HttpGet]
        public async Task<IActionResult> PrintEquipmentInventory(int id)
        {
            var building = await GetManagedBuildingAsync(id);
            if (building == null) return Forbid();

            var unitsWithSpaces = await _context.BuildingUnits
                .Include(u => u.Spaces)
                    .ThenInclude(s => s.Fixtures)
                .Where(u => u.BuildingId == id)
                .OrderByDescending(u => u.FloorLevel)
                .ThenBy(u => u.DoorNumber)
                .ToListAsync();

            var fixtureDocs = await _context.DocumentArchives
                .Where(d => d.BuildingId == id && d.SourceModule == "SpaceFixture" && d.SourceRecordId.HasValue)
                .ToListAsync();

            ViewBag.Building = building;
            ViewBag.FixtureDocuments = fixtureDocs
                .GroupBy(d => d.SourceRecordId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            return View(unitsWithSpaces);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFloorTechnicalUnit(int buildingId, int unitId)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var unit = await _context.BuildingUnits
                .Include(u => u.Spaces)
                    .ThenInclude(s => s.Fixtures)
                .FirstOrDefaultAsync(u => u.Id == unitId && u.BuildingId == buildingId);

            if (unit == null || !(unit.Category ?? "").StartsWith("OrtakAlan") || unit.Category == "OrtakAlan_Blok")
            {
                TempData["ErrorMessage"] = "Yalnızca kat içi teknik veya depolama birimleri buradan kaldırılabilir.";
                return RedirectToAction(nameof(Detail), new { id = buildingId });
            }

            var allFixtures = (unit.Spaces ?? new System.Collections.Generic.List<UnitSpace>())
                .SelectMany(s => s.Fixtures ?? new System.Collections.Generic.List<SpaceFixture>())
                .ToList();
            if (allFixtures.Any()) _context.SpaceFixtures.RemoveRange(allFixtures);
            if (unit.Spaces != null && unit.Spaces.Any()) _context.UnitSpaces.RemoveRange(unit.Spaces);
            _context.BuildingUnits.Remove(unit);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{unit.DoorNumber}' birimi kaldırıldı.";
            return RedirectToAction(nameof(Detail), new { id = buildingId });
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
                .Include(b => b.Units)
                    .ThenInclude(u => u.Spaces)
                        .ThenInclude(s => s.Fixtures)
                .Include(b => b.Expenses)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (building == null) return NotFound();
            if (!CanAccessBuilding(building, userId)) return Forbid();

            // Daha once Step 2/3 sirasinda hata sebebiyle birimleri olusamamis kayitlari otomatik onar veya eksik bodrumları tamamla
            bool hasLivingUnits = building.Units?.Any(u => !(u.Category ?? "").StartsWith("OrtakAlan")) ?? false;
            bool missingBasement = building.BasementFloors > 0 && !(building.Units?.Any(u => u.FloorLevel < 0 && u.FloorLevel != -99) ?? false);
            if ((!hasLivingUnits && building.OnboardingStep >= 3 && (building.TotalApartments + building.TotalShops) > 0) || missingBasement)
            {
                if (!hasLivingUnits && building.OnboardingStep >= 3 && (building.TotalApartments + building.TotalShops) > 0)
                {
                    await GenerateFloorUnitsIfEmptyAsync(building, building.TotalApartments, building.TotalShops, building.TotalFloors ?? 5, building.HasGroundFloor);
                }
                await EnsureBaselineCommonSpacesAsync(building);
                building = await _context.Buildings
                    .Include(b => b.HousingComplex)
                    .Include(b => b.City)
                    .Include(b => b.District)
                    .Include(b => b.ChildBuildings).ThenInclude(cb => cb.Units).ThenInclude(u => u.Debts)
                    .Include(b => b.BuildingManagers).ThenInclude(m => m.User)
                    .Include(b => b.Units).ThenInclude(u => u.Debts).ThenInclude(d => d.BuildingExpense)
                    .Include(b => b.Units).ThenInclude(u => u.Spaces).ThenInclude(s => s.Fixtures)
                    .Include(b => b.Expenses)
                    .FirstOrDefaultAsync(b => b.Id == id) ?? building;
            }

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

            ViewBag.CommonSpaces = commonUnits
                .SelectMany(u => u.Spaces ?? new List<UnitSpace>())
                .Where(s => s.Name != "Sabit Gider & Muhasebe")
                .ToList();

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

            var unitIds = building.Units?.Select(u => u.Id).ToList() ?? new System.Collections.Generic.List<int>();
            var residencyPeriods = unitIds.Any()
                ? await _context.ResidencyPeriods
                    .Where(rp => unitIds.Contains(rp.BuildingUnitId))
                    .OrderByDescending(rp => rp.Status == "ACTIVE")
                    .ThenByDescending(rp => rp.StartDate)
                    .ToListAsync()
                : new System.Collections.Generic.List<ResidencyPeriod>();

            var pendingApprovals = await _context.ApprovalRequests
                .Where(a => a.BuildingId == rootId && a.Status == "PENDING")
                .OrderByDescending(a => a.Id)
                .ToListAsync();

            var fixtureDocs = await _context.DocumentArchives
                .Where(d => d.BuildingId == id && d.SourceModule == "SpaceFixture" && d.SourceRecordId.HasValue)
                .OrderByDescending(d => d.Id)
                .ToListAsync();

            bool isSiteSecondaryBlock = building.ParentBuildingId != null;
            int? sitePrimaryBuildingId = building.ParentBuildingId;
            string? sitePrimaryBuildingName = building.ParentBuilding?.Name;
            if (!isSiteSecondaryBlock && building.HousingComplexId.HasValue)
            {
                var sitePrimary = await _context.Buildings
                    .Where(b => b.HousingComplexId == building.HousingComplexId && b.ParentBuildingId == null)
                    .OrderBy(b => b.Id)
                    .Select(b => new { b.Id, b.Name })
                    .FirstOrDefaultAsync();
                if (sitePrimary != null && sitePrimary.Id != building.Id)
                {
                    isSiteSecondaryBlock = true;
                    sitePrimaryBuildingId = sitePrimary.Id;
                    sitePrimaryBuildingName = sitePrimary.Name;
                }
            }

            double? landAreaSqm = null;
            if (double.TryParse(ParseTechnicalTag(building.TechnicalFeatures, "AraziM2"), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedLand))
            {
                landAreaSqm = parsedLand;
            }
            double totalFootprintSqm = building.HousingComplexId.HasValue
                ? (await _context.Buildings.Where(b => b.HousingComplexId == building.HousingComplexId && b.ParentBuildingId == null).SumAsync(b => b.BaseArea ?? 0))
                : (building.BaseArea ?? 0);

            ViewBag.LandAreaSqm = landAreaSqm;
            ViewBag.TotalFootprintSqm = totalFootprintSqm;
            ViewBag.IsSiteSecondaryBlock = isSiteSecondaryBlock;
            ViewBag.SitePrimaryBuildingId = sitePrimaryBuildingId;
            ViewBag.SitePrimaryBuildingName = sitePrimaryBuildingName;
            ViewBag.IsMultiBlockSite = isSite || blockCount > 1;
            ViewBag.HasDecisionAuthority = hasAuthorityDoc;
            ViewBag.GovernanceMembers = governanceMembers;
            ViewBag.ActiveBoardMemberCount = await _governanceService.GetActiveBoardMemberCountAsync(rootId);
            ViewBag.ResidencyPeriods = residencyPeriods;
            ViewBag.PendingApprovals = pendingApprovals;
            ViewBag.FixtureDocuments = fixtureDocs
                .GroupBy(d => d.SourceRecordId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());
            ViewBag.PricingTier = GMK360.Web.Helpers.FacilityPricingHelper.Calculate(isSite, blockCount, livingUnitCount);
            ViewBag.RootBuildingId = rootId;

            var siteBlocks = new System.Collections.Generic.List<Building>();
            if (building.HousingComplexId.HasValue)
            {
                var complexRoots = await _context.Buildings
                    .Include(b => b.Units)
                    .Include(b => b.ChildBuildings).ThenInclude(cb => cb.Units)
                    .Where(b => b.HousingComplexId == building.HousingComplexId && b.ParentBuildingId == null)
                    .OrderBy(b => b.Id)
                    .ToListAsync();
                foreach (var cr in complexRoots)
                {
                    siteBlocks.Add(cr);
                    if (cr.ChildBuildings != null && cr.ChildBuildings.Any())
                    {
                        siteBlocks.AddRange(cr.ChildBuildings.OrderBy(cb => cb.Id));
                    }
                }
            }
            else
            {
                var rootWithChildren = await _context.Buildings
                    .Include(b => b.Units)
                    .Include(b => b.ChildBuildings).ThenInclude(cb => cb.Units)
                    .FirstOrDefaultAsync(b => b.Id == rootId);
                if (rootWithChildren != null)
                {
                    siteBlocks.Add(rootWithChildren);
                    if (rootWithChildren.ChildBuildings != null && rootWithChildren.ChildBuildings.Any())
                    {
                        siteBlocks.AddRange(rootWithChildren.ChildBuildings.OrderBy(cb => cb.Id));
                    }
                }
            }

            if (siteBlocks.Count <= 1)
            {
                var allManagedRoots = await GetUserManagedRootBuildingsAsync();
                if (allManagedRoots.Count > 1)
                {
                    var rootIds = allManagedRoots.Select(r => r.Id).ToList();
                    var withUnits = await _context.Buildings
                        .Include(b => b.Units)
                        .Where(b => rootIds.Contains(b.Id) || (b.ParentBuildingId.HasValue && rootIds.Contains(b.ParentBuildingId.Value)))
                        .OrderBy(b => b.Id)
                        .ToListAsync();
                    siteBlocks = withUnits;
                }
            }
            ViewBag.SiteBlocks = siteBlocks.GroupBy(b => b.Id).Select(g => g.First()).ToList();

            return View(building);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignGovernanceMember(
            int buildingId,
            string? email,
            BuildingManagerRole role,
            [FromServices] FacilityTaskDispatchService taskDispatch,
            string? fullName = null,
            string? phoneNumber = null,
            string? tcIdentityNo = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var cleanEmail = email?.Trim();
            var cleanPhone = phoneNumber?.Trim();
            var cleanName = fullName?.Trim();
            var cleanTc = GMK360.Web.Helpers.TcknHelper.IsValidTcIdentity(tcIdentityNo)
                ? GMK360.Web.Helpers.TcknHelper.Normalize(tcIdentityNo)
                : null;
            if (string.IsNullOrWhiteSpace(cleanEmail) && FacilityTaskDispatchService.NormalizePhoneLast10(cleanPhone).Length < 10 && string.IsNullOrWhiteSpace(cleanTc))
            {
                TempData["ErrorMessage"] = "Lütfen kurul üyesi / aza için geçerli bir e-posta, 10 haneli telefon veya TCKN giriniz.";
                return RedirectToAction(nameof(Detail), new { id = buildingId });
            }

            int rootId = building.ParentBuildingId ?? building.Id;
            GMK360.Core.Entities.Identity.ApplicationUser? targetUser = null;

            if (!string.IsNullOrWhiteSpace(cleanTc))
            {
                targetUser = await _context.Users.FirstOrDefaultAsync(u => u.TcIdentityNo == cleanTc);
            }
            if (targetUser == null && !string.IsNullOrWhiteSpace(cleanEmail))
            {
                targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == cleanEmail || u.UserName == cleanEmail);
            }
            if (targetUser == null && !string.IsNullOrWhiteSpace(cleanPhone))
            {
                var normPhone = FacilityTaskDispatchService.NormalizePhoneLast10(cleanPhone);
                if (normPhone.Length >= 10)
                {
                    var phoneUsers = await _context.Users.Where(u => u.PhoneNumber != null).ToListAsync();
                    targetUser = phoneUsers.FirstOrDefault(u => FacilityTaskDispatchService.NormalizePhoneLast10(u.PhoneNumber) == normPhone);
                }
            }

            if (targetUser == null)
            {
                targetUser = await taskDispatch.ProvisionResidentProfileAsync(
                    null,
                    string.IsNullOrWhiteSpace(cleanName) ? "Kurul Üyesi" : cleanName,
                    cleanEmail,
                    cleanPhone,
                    "Owner",
                    GetUserId() ?? "system",
                    cleanTc);

                if (targetUser == null)
                {
                    TempData["ErrorMessage"] = "Kurul üyesi / aza profili oluşturulamadı. E-posta veya telefon bilgisini kontrol ediniz.";
                    return RedirectToAction(nameof(Detail), new { id = buildingId });
                }
            }

            var existing = await _context.BuildingManagers
                .FirstOrDefaultAsync(m => m.BuildingId == rootId && m.UserId == targetUser.Id && m.IsActive);
            if (existing != null)
            {
                existing.Role = role;
                existing.UpdatedAt = DateTime.UtcNow;
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

            var roleText = role == BuildingManagerRole.BoardMember ? "Aza (Yönetim Kurulu)" : (role == BuildingManagerRole.Auditor ? "Denetçi" : "Yönetici");
            var displayName = !string.IsNullOrWhiteSpace(targetUser.FirstName)
                ? $"{targetUser.FirstName} {targetUser.LastName}".Trim()
                : (cleanName ?? targetUser.Email ?? "Üye");

            if (!string.IsNullOrWhiteSpace(targetUser.ActivationToken))
            {
                var inviteUrl = $"{Request.Scheme}://{Request.Host}/Account/ActivateResident?token={targetUser.ActivationToken}";
                bool mailSent = await TrySendActivationEmailAsync(targetUser, roleText, building.Name);
                TempData["LastActivationUrl"] = inviteUrl;
                TempData["LastActivationLabel"] = $"{displayName} ({roleText})";
                TempData["SuccessMessage"] = mailSent
                    ? $"{displayName} ({roleText}) kadroya eklendi ve {targetUser.Email} adresine aktivasyon e-postası gönderildi."
                    : $"{displayName} ({roleText}) kadroya eklendi ve kişisel profil aktivasyon bağlantısı hazırlandı. Bağlantıyı kopyalayıp iletebilirsiniz.";
            }
            else
            {
                TempData["SuccessMessage"] = $"{displayName} ({roleText}) yönetim kadrosuna eklendi.";
            }

            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveGovernanceMember(int buildingId, int managerId)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            int rootId = building.ParentBuildingId ?? building.Id;
            var member = await _context.BuildingManagers
                .FirstOrDefaultAsync(m => m.Id == managerId && m.BuildingId == rootId && m.IsActive);

            if (member == null)
            {
                TempData["ErrorMessage"] = "Kurul üyesi kaydı bulunamadı.";
                return RedirectToAction(nameof(Detail), new { id = buildingId });
            }

            if (member.Role == BuildingManagerRole.HeadManager)
            {
                int headCount = await _context.BuildingManagers.CountAsync(m => m.BuildingId == rootId && m.IsActive && m.Role == BuildingManagerRole.HeadManager);
                if (headCount <= 1)
                {
                    TempData["ErrorMessage"] = "Son aktif Ana Yönetici çıkarılamaz; bunun için Yetki Devri kullanın.";
                    return RedirectToAction(nameof(Detail), new { id = buildingId });
                }
            }

            member.IsActive = false;
            member.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kurul / aza üyesi pasife alındı (denetim geçmişi korundu).";
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetSiteIdentity(int buildingId, string? subdomain, bool isIndependentSite = true)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var cleanSub = System.Text.RegularExpressions.Regex
                .Replace((subdomain ?? "").Trim().ToLowerInvariant(), @"[^a-z0-9\-]", "")
                .Trim('-');

            if (!string.IsNullOrEmpty(cleanSub))
            {
                if (cleanSub.Length < 3 || cleanSub == "www" || cleanSub == "api" || cleanSub == "admin")
                {
                    TempData["ErrorMessage"] = "Alt domain en az 3 karakterli olmalı (örn: yesilvadi).";
                    return RedirectToAction(nameof(Detail), new { id = buildingId });
                }

                bool taken = await _context.HousingComplexes.AnyAsync(c => c.Subdomain == cleanSub && (!building.HousingComplexId.HasValue || c.Id != building.HousingComplexId.Value))
                    || await _context.Buildings.AnyAsync(b => b.Subdomain == cleanSub && b.Id != building.Id && (!building.HousingComplexId.HasValue || b.HousingComplexId != building.HousingComplexId.Value))
                    || await _context.Agencies.AnyAsync(a => a.Subdomain == cleanSub)
                    || await _context.ManagementCompanies.AnyAsync(m => m.Subdomain == cleanSub);

                if (taken)
                {
                    TempData["ErrorMessage"] = $"'{cleanSub}.gmk360.com' adresi başka bir kurum/site tarafından kullanılıyor.";
                    return RedirectToAction(nameof(Detail), new { id = buildingId });
                }
            }

            if (building.HousingComplexId.HasValue)
            {
                var complex = await _context.HousingComplexes.FirstOrDefaultAsync(c => c.Id == building.HousingComplexId.Value);
                if (complex != null)
                {
                    complex.Subdomain = string.IsNullOrEmpty(cleanSub) ? null : cleanSub;
                    complex.IsIndependentSite = isIndependentSite;
                }
            }
            building.Subdomain = string.IsNullOrEmpty(cleanSub) ? null : cleanSub;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = string.IsNullOrEmpty(cleanSub)
                ? "Site / bina yönetim kimliği güncellendi."
                : $"Kurumsal site adresi güncellendi: https://{cleanSub}.gmk360.com";
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
        public async Task<IActionResult> CreateMeeting(
            int buildingId,
            string agendaTitle,
            DateTime meetingDate,
            string? decisionText = null,
            string? returnTo = null,
            bool isPlannedOnly = false,
            string? meetingTime = null)
        {
            var userId = GetUserId();
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == buildingId && (b.ManagerUserId == userId || b.CreatedByUserId == userId || User.IsInRole("Admin") || User.IsInRole("SuperAdmin") || _context.BuildingManagers.Any(m => m.BuildingId == (b.ParentBuildingId ?? b.Id) && m.UserId == userId && m.IsActive)));
            if (building == null) return Unauthorized();

            DateTime effectiveDate = meetingDate;
            if (!string.IsNullOrWhiteSpace(meetingTime) && TimeSpan.TryParse(meetingTime, out var parsedTime))
            {
                effectiveDate = meetingDate.Date.Add(parsedTime);
            }

            var meeting = new SystemMeeting
            {
                ContextType = "Building",
                ContextId = buildingId,
                AgendaTitle = agendaTitle,
                MeetingDate = effectiveDate,
                IsConcluded = !isPlannedOnly,
                CreatedAt = DateTime.Now
            };

            _context.SystemMeetings.Add(meeting);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(decisionText))
            {
                var decision = new SystemMeetingDecision
                {
                    SystemMeetingId = meeting.Id,
                    DecisionText = decisionText.Trim()
                };
                _context.SystemMeetingDecisions.Add(decision);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = isPlannedOnly
                ? $"📅 '{agendaTitle}' toplantısı planlandı ve Kiracı / Ev Sahibi / Aza ekranlarına yansıtıldı."
                : "🎉 Genel Kurul Kararı başarıyla kaydedildi. Noter Defteri Baskı Şablonunu indirebilir, Pazar Yeri / B2B üzerinden teklif toplayabilirsiniz.";
            if (string.Equals(returnTo, "Oylamalar", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(Oylamalar), new { buildingId });
            }
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConcludeMeetingWithDecision(int meetingId, int buildingId, string decisionText, string? returnTo = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var meeting = await _context.SystemMeetings.FirstOrDefaultAsync(m => m.Id == meetingId && m.ContextType == "Building" && m.ContextId == buildingId);
            if (meeting == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(decisionText))
            {
                _context.SystemMeetingDecisions.Add(new SystemMeetingDecision
                {
                    SystemMeetingId = meeting.Id,
                    DecisionText = decisionText.Trim(),
                    CreatedAt = DateTime.Now
                });
            }

            meeting.IsConcluded = true;
            meeting.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Toplantı #{meeting.Id} karara bağlandı ve Noter Karar Defterine işlendi.";
            if (string.Equals(returnTo, "Oylamalar", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(Oylamalar), new { buildingId });
            }
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

        private async Task<bool> TrySendActivationEmailAsync(
            GMK360.Core.Entities.Identity.ApplicationUser? targetUser,
            string roleTitle,
            string buildingName)
        {
            if (targetUser == null
                || string.IsNullOrWhiteSpace(targetUser.ActivationToken)
                || string.IsNullOrWhiteSpace(targetUser.Email)
                || targetUser.Email.EndsWith("@gmk360.local", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var emailSvc = HttpContext.RequestServices.GetService<IEmailService>();
            if (emailSvc == null) return false;

            var activateUrl = $"{Request.Scheme}://{Request.Host}/Account/ActivateResident?token={Uri.EscapeDataString(targetUser.ActivationToken)}";
            var fullName = $"{targetUser.FirstName} {targetUser.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(fullName)) fullName = targetUser.Email;

            var subject = $"GMK360 | {buildingName} ({roleTitle}) Profil Aktivasyonu";
            var body = $@"
<div style=""font-family:Arial,sans-serif;max-width:560px;margin:0 auto;padding:20px;border:1px solid #e2e8f0;border-radius:12px;"">
  <h2 style=""color:#15803d;margin-top:0;"">GMK360 Bina &amp; Site Portalı Daveti</h2>
  <p>Merhaba <strong>{System.Net.WebUtility.HtmlEncode(fullName)}</strong>,</p>
  <p><strong>{System.Net.WebUtility.HtmlEncode(buildingName)}</strong> yönetiminde <strong>{System.Net.WebUtility.HtmlEncode(roleTitle)}</strong> olarak profiliniz oluşturuldu.</p>
  <p>E-postanızı doğrulamak, kendi şifrenizi ve kişisel 4–6 haneli onay/PWA PIN kodunuzu belirlemek için aşağıdaki butona tıklayın:</p>
  <p style=""margin:24px 0;"">
    <a href=""{activateUrl}"" style=""background:#15803d;color:#fff;text-decoration:none;padding:12px 22px;border-radius:999px;font-weight:bold;display:inline-block;"">Profilimi Doğrula &amp; Şifre/PIN Belirle</a>
  </p>
  <p style=""font-size:12px;color:#64748b;"">Buton çalışmazsa bağlantıyı kopyalayıp tarayıcınıza yapıştırın:<br/><a href=""{activateUrl}"">{activateUrl}</a></p>
</div>";

            try
            {
                var res = await emailSvc.SendEmailWithResultAsync(targetUser.Email, subject, body);
                return res.Success;
            }
            catch
            {
                return false;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendResidentActivationEmail(int unitId, int buildingId, string roleType, int? selectedBuildingFilter = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var unit = await _context.BuildingUnits.FirstOrDefaultAsync(u => u.Id == unitId && u.BuildingId == buildingId);
            if (unit == null) return NotFound();

            string? targetUserId = string.Equals(roleType, "Tenant", StringComparison.OrdinalIgnoreCase)
                ? unit.TenantUserId
                : unit.OwnerUserId;
            if (string.IsNullOrWhiteSpace(targetUserId))
            {
                TempData["ErrorMessage"] = "Önce ilgili malik/kiracı için iletişim bilgisini kaydedin.";
                return RedirectToAction(nameof(Sakinler), new { buildingId = selectedBuildingFilter });
            }

            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
            if (targetUser == null || string.IsNullOrWhiteSpace(targetUser.ActivationToken))
            {
                TempData["ErrorMessage"] = "Bu kullanıcı profilini zaten aktifleştirmiş veya aktivasyon bağlantısı yok.";
                return RedirectToAction(nameof(Sakinler), new { buildingId = selectedBuildingFilter });
            }

            string roleTitle = string.Equals(roleType, "Tenant", StringComparison.OrdinalIgnoreCase)
                ? $"Daire {unit.DoorNumber} Kiracısı"
                : $"Daire {unit.DoorNumber} Kat Maliki";

            bool sent = await TrySendActivationEmailAsync(targetUser, roleTitle, building.Name);
            TempData[sent ? "SuccessMessage" : "ErrorMessage"] = sent
                ? $"✉️ {targetUser.Email} adresine aktivasyon e-postası gönderildi."
                : "Geçerli bir kişisel e-posta adresi kayıtlı değil veya SMTP ayarları eksik.";

            return RedirectToAction(nameof(Sakinler), new { buildingId = selectedBuildingFilter });
        }

        private async Task SyncResidentUsersAndPeriodsAsync(BuildingUnit unit, string? ownerTcNo = null, string? tenantTcNo = null)
        {
            var dispatchSvc = HttpContext.RequestServices.GetService<FacilityTaskDispatchService>();
            var creatorId = GetUserId() ?? "system";

            if (dispatchSvc != null && (!string.IsNullOrWhiteSpace(unit.OwnerEmail) || !string.IsNullOrWhiteSpace(unit.OwnerPhone) || !string.IsNullOrWhiteSpace(ownerTcNo)))
            {
                var ownerUser = await dispatchSvc.ProvisionResidentProfileAsync(
                    unit.OwnerUserId,
                    unit.OwnerName,
                    unit.OwnerEmail,
                    unit.OwnerPhone,
                    "Owner",
                    creatorId,
                    ownerTcNo);
                if (ownerUser != null) unit.OwnerUserId = ownerUser.Id;
            }
            else if (!string.IsNullOrWhiteSpace(unit.OwnerEmail))
            {
                var ownerUser = await _context.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == unit.OwnerEmail || u.NormalizedEmail == unit.OwnerEmail.ToUpper());
                if (ownerUser != null) unit.OwnerUserId = ownerUser.Id;
            }

            if (!unit.IsEmpty && (!string.IsNullOrWhiteSpace(unit.TenantEmail) || !string.IsNullOrWhiteSpace(unit.TenantPhone) || !string.IsNullOrWhiteSpace(unit.TenantName)))
            {
                if (dispatchSvc != null && (!string.IsNullOrWhiteSpace(unit.TenantEmail) || !string.IsNullOrWhiteSpace(unit.TenantPhone) || !string.IsNullOrWhiteSpace(tenantTcNo)))
                {
                    var tenantUser = await dispatchSvc.ProvisionResidentProfileAsync(
                        unit.TenantUserId,
                        unit.TenantName,
                        unit.TenantEmail,
                        unit.TenantPhone,
                        "Tenant",
                        creatorId,
                        tenantTcNo);
                    if (tenantUser != null) unit.TenantUserId = tenantUser.Id;
                }
                else if (!string.IsNullOrWhiteSpace(unit.TenantEmail))
                {
                    var tenantUser = await _context.Users.AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Email == unit.TenantEmail || u.NormalizedEmail == unit.TenantEmail.ToUpper());
                    if (tenantUser != null) unit.TenantUserId = tenantUser.Id;
                }
            }
            else if (unit.IsEmpty || string.IsNullOrWhiteSpace(unit.TenantName))
            {
                unit.TenantUserId = null;
            }

            await _context.SaveChangesAsync();
            await _governanceService.EnsureActivePeriodFromUnitAsync(unit, "Owner");
            if (!unit.IsEmpty && (!string.IsNullOrWhiteSpace(unit.TenantName) || !string.IsNullOrWhiteSpace(unit.TenantPhone) || !string.IsNullOrWhiteSpace(unit.TenantEmail)))
            {
                await _governanceService.EnsureActivePeriodFromUnitAsync(unit, "Tenant");
            }
        }

        // 4. Daire / Bölüm Ekle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUnit(
            int buildingId,
            string unitNumber,
            string? ownerName,
            string? ownerPhone,
            string? tenantName,
            string? tenantPhone,
            bool isEmpty,
            string? category = "Daire",
            string? ownerEmail = null,
            string? tenantEmail = null,
            string? residentQuickPin = null,
            int floorLevel = 1,
            decimal? landShare = null,
            double? grossSquareMeters = null,
            string? returnAction = null,
            string? ownerQuickPin = null,
            string? tenantQuickPin = null,
            string? ownerTcNo = null,
            string? tenantTcNo = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            bool hasTenantInfo = !string.IsNullOrWhiteSpace(tenantName)
                || !string.IsNullOrWhiteSpace(tenantPhone)
                || !string.IsNullOrWhiteSpace(tenantEmail);
            bool effectiveEmpty = isEmpty && !hasTenantInfo;

            string floorLabel = floorLevel == 0
                ? "Zemin Kat"
                : (floorLevel < 0 ? $"{Math.Abs(floorLevel)}. Bodrum" : $"{floorLevel}. Kat");

            string? effectiveOwnerPin = !string.IsNullOrWhiteSpace(ownerQuickPin) ? ownerQuickPin
                : (!hasTenantInfo ? residentQuickPin : null);
            string? effectiveTenantPin = !effectiveEmpty
                ? (!string.IsNullOrWhiteSpace(tenantQuickPin) ? tenantQuickPin : residentQuickPin)
                : null;

            var unit = new BuildingUnit
            {
                BuildingId = buildingId,
                DoorNumber = unitNumber.Trim(),
                UnitNumber = unitNumber.Trim(),
                FloorLevel = floorLevel,
                FloorName = floorLabel,
                Category = string.IsNullOrWhiteSpace(category) ? "Daire" : category.Trim(),
                OwnerName = ownerName?.Trim(),
                OwnerPhone = ownerPhone?.Trim(),
                OwnerEmail = ownerEmail?.Trim(),
                TenantName = effectiveEmpty ? null : tenantName?.Trim(),
                TenantPhone = effectiveEmpty ? null : tenantPhone?.Trim(),
                TenantEmail = effectiveEmpty ? null : tenantEmail?.Trim(),
                IsEmpty = effectiveEmpty,
                LandShare = landShare.HasValue ? Math.Max(0m, landShare.Value) : 0m,
                GrossSquareMeters = grossSquareMeters.HasValue && grossSquareMeters.Value > 0 ? grossSquareMeters.Value : null,
                ResidentQuickPinHash = (!string.IsNullOrWhiteSpace(residentQuickPin) && residentQuickPin.Trim().Length >= 4)
                    ? FacilityTaskDispatchService.HashQuickPin(residentQuickPin.Trim())
                    : null,
                OwnerQuickPinHash = (!string.IsNullOrWhiteSpace(effectiveOwnerPin) && effectiveOwnerPin.Trim().Length >= 4)
                    ? FacilityTaskDispatchService.HashQuickPin(effectiveOwnerPin.Trim())
                    : null,
                TenantQuickPinHash = (!string.IsNullOrWhiteSpace(effectiveTenantPin) && effectiveTenantPin.Trim().Length >= 4)
                    ? FacilityTaskDispatchService.HashQuickPin(effectiveTenantPin.Trim())
                    : null
            };

            _context.BuildingUnits.Add(unit);
            building.TotalUnits += 1;
            await _context.SaveChangesAsync();
            await SyncResidentUsersAndPeriodsAsync(unit, ownerTcNo, tenantTcNo);

            TempData["SuccessMessage"] = $"'{unitNumber}' bağımsız bölümü sakin/iletişim bilgileriyle eklendi.";
            if (string.Equals(returnAction, "Sakinler", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(Sakinler), new { buildingId });
            }
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        // 5. Gider Ekle ve Dağıt (İkili Cari: İşletme -> Kiracı/Malik, Demirbaş -> Daima Malik)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExpense(
            int buildingId,
            string description,
            decimal totalAmount,
            BuildingExpenseType? expenseType,
            string? payerType,
            Microsoft.AspNetCore.Http.IFormFile? invoiceFile = null,
            string? returnAction = null,
            string distributionRule = "Esit",
            string expenseItemKey = "Genel",
            ExpenseScopeType scopeType = ExpenseScopeType.Building,
            decimal shopParticipationPercent = 100m,
            decimal? ownerSplitPercent = null,
            string? expenseGroup = null,
            bool chargeToUnits = true,
            int? bankAccountId = null,
            string? physicalArchiveLocation = null,
            string? linkedSourceType = null,
            int? linkedTaskTicketId = null,
            int? linkedFixedExpenseId = null,
            string? linkedSourceLabel = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            decimal parsedAmount = ParseFlexibleDecimal(Request.Form["totalAmount"].FirstOrDefault()) ?? totalAmount;
            decimal? parsedOwnerSplit = ParseFlexibleDecimal(Request.Form["ownerSplitPercent"].FirstOrDefault()) ?? ownerSplitPercent;

            string? resolvedSourceLabel = string.IsNullOrWhiteSpace(linkedSourceLabel) ? null : linkedSourceLabel.Trim();
            if (string.IsNullOrWhiteSpace(resolvedSourceLabel))
            {
                if (linkedTaskTicketId.HasValue)
                {
                    var t = await _context.BuildingTaskTickets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == linkedTaskTicketId.Value);
                    if (t != null) resolvedSourceLabel = $"Arıza/İş #{t.Id}: {t.Category} ({(t.Description.Length > 45 ? t.Description[..45] + "..." : t.Description)})";
                }
                else if (linkedFixedExpenseId.HasValue)
                {
                    var f = await _context.BuildingFixedExpenses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == linkedFixedExpenseId.Value);
                    if (f != null) resolvedSourceLabel = $"Abonelik: {f.Title}";
                }
            }

            var resolvedType = expenseType ?? (string.Equals(payerType, "Owner", StringComparison.OrdinalIgnoreCase)
                ? BuildingExpenseType.CapitalExpenditure
                : BuildingExpenseType.Operational);

            var result = await _governanceService.DistributeExpenseAsync(
                buildingId,
                description,
                parsedAmount,
                resolvedType,
                invoiceFile,
                GetUserId() ?? "system",
                distributionRule,
                expenseItemKey,
                scopeType,
                shopParticipationPercent,
                parsedOwnerSplit,
                expenseGroup,
                chargeToUnits,
                bankAccountId,
                physicalArchiveLocation,
                linkedSourceType,
                linkedTaskTicketId,
                linkedFixedExpenseId,
                resolvedSourceLabel);

            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            if (string.Equals(returnAction, "Aidatlar", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(Aidatlar), new { buildingId });
            }
            if (string.Equals(returnAction, "Accounting", StringComparison.OrdinalIgnoreCase)
                || string.Equals(returnAction, "Giderler", StringComparison.OrdinalIgnoreCase))
            {
                var returnScope = Request.Form["returnScope"].FirstOrDefault();
                return RedirectToAction(nameof(Accounting), new { buildingId = returnScope == "site" ? (int?)null : buildingId, scope = returnScope == "site" ? "site" : null, tab = "faturalar" });
            }
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        // 5B. Daire Taşınma / Devir (Dönemsel Cari İzolasyonu + Kıstelyevm + Borçlu Devir Kilidi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferUnitResidency(
            int unitId,
            int buildingId,
            string roleType,
            string? newFullName,
            string? newPhone,
            string? newEmail,
            DateTime transferDate,
            decimal? monthlyDuesForProration)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var result = await _governanceService.TransferResidencyAsync(
                unitId,
                roleType,
                newFullName,
                newPhone,
                newEmail,
                transferDate == default ? DateTime.Today : transferDate,
                monthlyDuesForProration,
                GetUserId() ?? "system");

            TempData[result.Success ? (result.PendingApproval ? "WarningMessage" : "SuccessMessage") : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        // 5C. Fatura / Gider İptali (Fiziksel Silme Yasak -> Ters Kayıt & 2 Aza Onay Kuyruğu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestExpenseReversal(int expenseId, int buildingId, string reason, string? returnAction = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var result = await _governanceService.RequestOrExecuteExpenseReversalAsync(
                expenseId,
                reason,
                GetUserId() ?? "system");

            TempData[result.Success ? (result.PendingApproval ? "WarningMessage" : "SuccessMessage") : "ErrorMessage"] = result.Message;
            if (string.Equals(returnAction, "Accounting", StringComparison.OrdinalIgnoreCase)
                || string.Equals(returnAction, "Giderler", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(Accounting), new { buildingId, tab = "giderler" });
            }
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        // 5D. Yönetim Kurulu Aza Multi-Sig Onay / Ret İşlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DecideApprovalRequest(int requestId, int buildingId, bool approve)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var approverEmail = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "onayci@gmk360.com";
            var result = await _governanceService.SubmitApprovalDecisionAsync(
                requestId,
                GetUserId() ?? string.Empty,
                approverEmail,
                approve,
                IsAdminOrSuperAdmin());

            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        // 6. Ödeme Alındı İşaretle
        [HttpPost]
        public async Task<IActionResult> MarkDebtAsPaid(int debtId, string paidBy, string? returnAction = null)
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
                if (string.Equals(returnAction, "Aidatlar", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction(nameof(Aidatlar), new { buildingId = debt.BuildingUnit.BuildingId });
                }
                return RedirectToAction(nameof(Detail), new { id = debt.BuildingUnit.BuildingId });
            }
            
            TempData["ErrorMessage"] = "Ödeme kaydedilirken bir hata oluştu.";
            return RedirectToAction(nameof(Index));
        }

        // 7. Sakinler & Daireler Listesi
        public async Task<IActionResult> Sakinler(int? buildingId)
        {
            var buildings = await GetUserManagedRootBuildingsAsync();
            var allowedIds = buildings.Select(b => b.Id).ToList();

            if (buildingId.HasValue)
            {
                var target = await GetManagedBuildingAsync(buildingId.Value);
                if (target == null) return Forbid();
                allowedIds = new System.Collections.Generic.List<int> { target.Id };
            }

            // Sihirbazda sayisi belirlenmis ama henüz unit satiri uretilmemis binalarin (orn: 17 Daire + 2 Dukkan) kartlarini ac
            foreach (var b in buildings.Where(x => allowedIds.Contains(x.Id)))
            {
                bool hasLiving = await _context.BuildingUnits
                    .AnyAsync(u => u.BuildingId == b.Id && !(u.Category ?? "").StartsWith("OrtakAlan"));
                if (!hasLiving && (b.TotalApartments + b.TotalShops) > 0)
                {
                    await GenerateFloorUnitsIfEmptyAsync(b, b.TotalApartments, b.TotalShops, b.TotalFloors ?? 5, b.HasGroundFloor);
                    await EnsureBaselineCommonSpacesAsync(b);
                }
            }

            var units = allowedIds.Any()
                ? await _context.BuildingUnits
                    .Include(u => u.Building)
                    .Where(u => allowedIds.Contains(u.BuildingId)
                        && u.Category != "OrtakAlan_Blok"
                        && (u.Category == null || !u.Category.StartsWith("OrtakAlan")))
                    .OrderBy(u => u.BuildingId)
                    .ThenBy(u => u.FloorLevel)
                    .ThenBy(u => u.DoorNumber)
                    .ToListAsync()
                : new System.Collections.Generic.List<BuildingUnit>();

            int checkBuildingId = buildingId ?? buildings.FirstOrDefault()?.Id ?? 0;
            var linkedUserIds = units
                .SelectMany(u => new[] { u.OwnerUserId, u.TenantUserId })
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();
            var residentProfiles = linkedUserIds.Any()
                ? await _context.Users.AsNoTracking()
                    .Where(u => linkedUserIds.Contains(u.Id!))
                    .ToDictionaryAsync(u => u.Id, u => u)
                : new System.Collections.Generic.Dictionary<string, GMK360.Core.Entities.Identity.ApplicationUser>();

            ViewBag.Buildings = buildings;
            ViewBag.SelectedBuildingId = buildingId;
            ViewBag.HasAuthorityDoc = checkBuildingId > 0 && await HasDecisionAuthorityAsync(checkBuildingId);
            ViewBag.ResidentProfiles = residentProfiles;
            return View(units);
        }

        private static decimal? ParseFlexibleDecimal(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var s = raw.Trim().Replace(" ", "");
            int lastComma = s.LastIndexOf(',');
            int lastDot = s.LastIndexOf('.');
            if (lastComma >= 0 && lastDot >= 0)
            {
                s = lastComma > lastDot
                    ? s.Replace(".", "").Replace(',', '.')
                    : s.Replace(",", "");
            }
            else if (lastComma >= 0)
            {
                s = s.Replace(',', '.');
            }
            return decimal.TryParse(s, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;
        }

        // 7. Bölüm Malik / Kiracı & Aidat Kuralı Güncelleme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUnitContacts(
            int unitId,
            int buildingId,
            string? ownerName,
            string? ownerPhone,
            string? ownerEmail,
            string? tenantName,
            string? tenantPhone,
            string? tenantEmail,
            bool isEmpty,
            string? landShare,
            string? grossSquareMeters,
            string? residentQuickPin,
            int? selectedBuildingFilter,
            string? duesMultiplier = null,
            string? fixedMonthlyDues = null,
            bool? isExemptFromElevator = null,
            bool? isExemptFromStairwell = null,
            string? cleaningParticipationPercent = null,
            string? duesRuleNote = null,
            string? ownerQuickPin = null,
            string? tenantQuickPin = null,
            string? ownerTcNo = null,
            string? tenantTcNo = null,
            bool isDuesExempt = false,
            string? duesAdjustmentAmount = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var unit = await _context.BuildingUnits.FirstOrDefaultAsync(u => u.Id == unitId && u.BuildingId == buildingId);
            if (unit == null) return NotFound();

            bool hasTenantInfo = !string.IsNullOrWhiteSpace(tenantName)
                || !string.IsNullOrWhiteSpace(tenantPhone)
                || !string.IsNullOrWhiteSpace(tenantEmail);
            bool effectiveEmpty = isEmpty && !hasTenantInfo;

            var parsedLandShare = ParseFlexibleDecimal(landShare);
            var parsedGrossSqm = ParseFlexibleDecimal(grossSquareMeters);
            var parsedDuesMultiplier = ParseFlexibleDecimal(duesMultiplier);
            var parsedFixedDues = ParseFlexibleDecimal(fixedMonthlyDues);
            var parsedDuesAdj = ParseFlexibleDecimal(duesAdjustmentAmount);
            var parsedCleaningPct = ParseFlexibleDecimal(cleaningParticipationPercent);

            unit.OwnerName = ownerName?.Trim();
            unit.OwnerPhone = ownerPhone?.Trim();
            unit.OwnerEmail = ownerEmail?.Trim();
            unit.TenantName = effectiveEmpty ? null : tenantName?.Trim();
            unit.TenantPhone = effectiveEmpty ? null : tenantPhone?.Trim();
            unit.TenantEmail = effectiveEmpty ? null : tenantEmail?.Trim();
            unit.IsEmpty = effectiveEmpty;
            if (parsedLandShare.HasValue) unit.LandShare = Math.Max(0m, parsedLandShare.Value);
            if (parsedGrossSqm.HasValue) unit.GrossSquareMeters = Math.Max(0, (double)parsedGrossSqm.Value);
            if (parsedDuesMultiplier.HasValue) unit.DuesMultiplier = Math.Clamp(parsedDuesMultiplier.Value, 0m, 10m);
            if (parsedFixedDues.HasValue)
                unit.FixedMonthlyDues = Math.Max(0m, Math.Round(parsedFixedDues.Value, 2));
            else if (Request.Form.ContainsKey("fixedMonthlyDues") && string.IsNullOrWhiteSpace(Request.Form["fixedMonthlyDues"]))
                unit.FixedMonthlyDues = null;
            unit.IsDuesExempt = isDuesExempt;
            if (parsedDuesAdj.HasValue) unit.DuesAdjustmentAmount = Math.Round(parsedDuesAdj.Value, 2);
            if (isExemptFromElevator.HasValue) unit.IsExemptFromElevator = isExemptFromElevator.Value;
            if (isExemptFromStairwell.HasValue) unit.IsExemptFromStairwell = isExemptFromStairwell.Value;
            if (parsedCleaningPct.HasValue) unit.CleaningParticipationPercent = Math.Clamp(parsedCleaningPct.Value, 0m, 500m);
            if (duesRuleNote != null) unit.DuesRuleNote = string.IsNullOrWhiteSpace(duesRuleNote) ? null : duesRuleNote.Trim();
            if (!string.IsNullOrWhiteSpace(residentQuickPin) && residentQuickPin.Trim().Length >= 4)
            {
                unit.ResidentQuickPinHash = FacilityTaskDispatchService.HashQuickPin(residentQuickPin.Trim());
            }
            if (!string.IsNullOrWhiteSpace(ownerQuickPin) && ownerQuickPin.Trim().Length >= 4)
            {
                unit.OwnerQuickPinHash = FacilityTaskDispatchService.HashQuickPin(ownerQuickPin.Trim());
            }
            if (effectiveEmpty)
            {
                unit.TenantQuickPinHash = null;
            }
            else if (!string.IsNullOrWhiteSpace(tenantQuickPin) && tenantQuickPin.Trim().Length >= 4)
            {
                unit.TenantQuickPinHash = FacilityTaskDispatchService.HashQuickPin(tenantQuickPin.Trim());
            }
            await SyncResidentUsersAndPeriodsAsync(unit, ownerTcNo, tenantTcNo);

            bool ownerInvited = false;
            bool tenantInvited = false;
            if (!string.IsNullOrWhiteSpace(unit.OwnerUserId) && !string.IsNullOrWhiteSpace(unit.OwnerEmail))
            {
                var ow = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == unit.OwnerUserId);
                ownerInvited = await TrySendActivationEmailAsync(ow, $"Daire {unit.DoorNumber} Kat Maliki", building.Name);
            }
            if (!unit.IsEmpty && !string.IsNullOrWhiteSpace(unit.TenantUserId) && !string.IsNullOrWhiteSpace(unit.TenantEmail))
            {
                var tn = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == unit.TenantUserId);
                tenantInvited = await TrySendActivationEmailAsync(tn, $"Daire {unit.DoorNumber} Kiracısı", building.Name);
            }

            var mailNote = (ownerInvited || tenantInvited) ? " E-posta adresi olanlara aktivasyon daveti de gönderildi." : "";
            TempData["SuccessMessage"] = $"'{unit.DoorNumber}' bağımsız bölümü malik/kiracı, arsa payı, aidat muafiyet/katsayı ve PWA PIN bilgileri güncellendi.{mailNote}";
            return RedirectToAction(nameof(Sakinler), new { buildingId = selectedBuildingFilter });
        }

        // 7B. Toplu Aidat Katsayısı & Muafiyet Profili Uygula (Örn: Tüm Dükkanlar Asansör Muaf + Çöp %150)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkApplyUnitDuesProfile(
            int buildingId,
            string targetCategory,
            string? duesMultiplier,
            string? fixedMonthlyDues,
            bool isExemptFromElevator,
            bool isExemptFromStairwell,
            string? cleaningParticipationPercent,
            string? duesRuleNote,
            string? returnAction = null,
            bool isDuesExempt = false,
            string? duesAdjustmentAmount = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var units = await _context.BuildingUnits
                .Where(u => u.BuildingId == buildingId
                    && u.Category != "OrtakAlan_Blok"
                    && (u.Category == null || !u.Category.StartsWith("OrtakAlan")))
                .ToListAsync();

            var filtered = (targetCategory ?? "ALL").ToUpperInvariant() switch
            {
                "DUKKAN" => units.Where(u => string.Equals(u.Category, "Dukkan", StringComparison.OrdinalIgnoreCase) || string.Equals(u.Category, "Dükkan", StringComparison.OrdinalIgnoreCase)).ToList(),
                "DAIRE" => units.Where(u => !string.Equals(u.Category, "Dukkan", StringComparison.OrdinalIgnoreCase) && !string.Equals(u.Category, "Dükkan", StringComparison.OrdinalIgnoreCase)).ToList(),
                _ => units
            };

            if (!filtered.Any())
            {
                TempData["ErrorMessage"] = "Seçilen kategoriye uyan bağımsız bölüm bulunamadı.";
                return RedirectToAction(returnAction == "Sakinler" ? nameof(Sakinler) : nameof(Aidatlar), new { buildingId });
            }

            decimal parsedMultiplier = Math.Clamp(ParseFlexibleDecimal(duesMultiplier) ?? 1m, 0m, 10m);
            decimal? parsedFixedDues = ParseFlexibleDecimal(fixedMonthlyDues);
            decimal? parsedDuesAdj = ParseFlexibleDecimal(duesAdjustmentAmount);
            decimal parsedCleanPct = Math.Clamp(ParseFlexibleDecimal(cleaningParticipationPercent) ?? 100m, 0m, 500m);

            foreach (var u in filtered)
            {
                u.DuesMultiplier = parsedMultiplier;
                if (parsedFixedDues.HasValue)
                    u.FixedMonthlyDues = Math.Max(0m, Math.Round(parsedFixedDues.Value, 2));
                u.IsDuesExempt = isDuesExempt;
                if (parsedDuesAdj.HasValue)
                    u.DuesAdjustmentAmount = Math.Round(parsedDuesAdj.Value, 2);
                u.IsExemptFromElevator = isExemptFromElevator;
                u.IsExemptFromStairwell = isExemptFromStairwell;
                u.CleaningParticipationPercent = parsedCleanPct;
                if (!string.IsNullOrWhiteSpace(duesRuleNote)) u.DuesRuleNote = duesRuleNote.Trim();
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{filtered.Count} adet bağımsız bölüme aidat katsayısı ({parsedMultiplier:0.##}x) ve muafiyet/fark profili topluca uygulandı.";
            return RedirectToAction(returnAction == "Sakinler" ? nameof(Sakinler) : nameof(Aidatlar), new { buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBuildingBaseDues(
            int buildingId,
            string? defaultBaseMonthlyDues,
            int? duesPaymentDay,
            bool applyToAllSiteBlocks = true)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            decimal? parsedBaseDues = ParseFlexibleDecimal(defaultBaseMonthlyDues);
            decimal? cleanDues = parsedBaseDues.HasValue && parsedBaseDues.Value >= 0m
                ? Math.Round(parsedBaseDues.Value, 2)
                : null;
            int? cleanDay = duesPaymentDay.HasValue
                ? Math.Clamp(duesPaymentDay.Value, 1, 31)
                : null;

            var targets = (applyToAllSiteBlocks && building.HousingComplexId.HasValue)
                ? await _context.Buildings.Where(b => b.HousingComplexId == building.HousingComplexId && b.ParentBuildingId == null).ToListAsync()
                : new System.Collections.Generic.List<Building> { building };

            foreach (var b in targets)
            {
                b.DefaultBaseMonthlyDues = cleanDues;
                if (cleanDay.HasValue) b.DuesPaymentDay = cleanDay.Value;
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Sabit baz aylık aidat ({(cleanDues ?? 0m):N2} ₺) ve ödeme günü (Her ayın {cleanDay ?? building.DuesPaymentDay ?? 5}. günü) {targets.Count} yapı için kaydedildi.";
            return RedirectToAction(nameof(Aidatlar), new { buildingId });
        }

        // 8. Aidat ve Borç Takibi
        public async Task<IActionResult> Aidatlar(int? buildingId)
        {
            var buildings = await GetUserManagedRootBuildingsAsync();
            var allowedIds = buildings.Select(b => b.Id).ToList();
            if (buildingId.HasValue)
            {
                var target = await GetManagedBuildingAsync(buildingId.Value);
                if (target == null) return Forbid();
                allowedIds = new System.Collections.Generic.List<int> { target.Id };
            }

            var debts = allowedIds.Any()
                ? await _context.UnitDebts
                    .Include(d => d.BuildingUnit).ThenInclude(u => u.Building)
                    .Include(d => d.BuildingExpense)
                    .Where(d => allowedIds.Contains(d.BuildingUnit.BuildingId) && !d.IsArchived && d.Amount > 0)
                    .OrderByDescending(d => d.Id)
                    .ToListAsync()
                : new System.Collections.Generic.List<UnitDebt>();

            var livingUnits = allowedIds.Any()
                ? await _context.BuildingUnits
                    .Include(u => u.Building)
                    .Where(u => allowedIds.Contains(u.BuildingId)
                        && u.Category != "OrtakAlan_Blok"
                        && (u.Category == null || !u.Category.StartsWith("OrtakAlan")))
                    .OrderBy(u => u.BuildingId).ThenBy(u => u.FloorLevel).ThenBy(u => u.DoorNumber)
                    .ToListAsync()
                : new System.Collections.Generic.List<BuildingUnit>();

            var fixedExpenses = allowedIds.Any()
                ? await _context.BuildingFixedExpenses
                    .AsNoTracking()
                    .Where(f => allowedIds.Contains(f.BuildingId) && f.IsActive && !f.IsDeleted)
                    .ToListAsync()
                : new System.Collections.Generic.List<BuildingFixedExpense>();

            var buildingStaffs = allowedIds.Any()
                ? await _context.BuildingStaffs
                    .AsNoTracking()
                    .Where(s => allowedIds.Contains(s.BuildingId) && s.IsActive && !s.IsDeleted)
                    .ToListAsync()
                : new System.Collections.Generic.List<BuildingStaff>();

            ViewBag.Buildings = buildings;
            ViewBag.SelectedBuildingId = buildingId;
            ViewBag.LivingUnits = livingUnits;
            ViewBag.FixedExpenses = fixedExpenses;
            ViewBag.BuildingStaffs = buildingStaffs;
            return View(debts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAnnualBudgetEstimate(
            int buildingId,
            string? energyMonthly,
            string? staffAndCleaningMonthly,
            string? elevatorMonthly,
            string? adminAndSecurityMonthly,
            string? capitalReserveMonthly,
            string? contingencyPercent,
            bool applyToAllSiteBlocks = true,
            bool applyCalculatedAsBaseDues = false,
            string? recommendedBaseDues = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            decimal energy = Math.Max(0m, ParseFlexibleDecimal(energyMonthly) ?? 0m);
            decimal staffClean = Math.Max(0m, ParseFlexibleDecimal(staffAndCleaningMonthly) ?? 0m);
            decimal elevator = Math.Max(0m, ParseFlexibleDecimal(elevatorMonthly) ?? 0m);
            decimal adminSec = Math.Max(0m, ParseFlexibleDecimal(adminAndSecurityMonthly) ?? 0m);
            decimal capital = Math.Max(0m, ParseFlexibleDecimal(capitalReserveMonthly) ?? 0m);
            decimal reservePct = Math.Clamp(ParseFlexibleDecimal(contingencyPercent) ?? 0m, 0m, 50m);

            decimal operatingSubtotal = energy + staffClean + elevator + adminSec;
            decimal reserveAmount = Math.Round(operatingSubtotal * (reservePct / 100m), 2, MidpointRounding.AwayFromZero);

            bool isSiteWide = applyToAllSiteBlocks && building.HousingComplexId.HasValue;
            var scopeType = isSiteWide ? ExpenseScopeType.HousingComplex : ExpenseScopeType.Building;

            var planItems = new (string Code, string Title, string Group, string Scope, string ItemKey, decimal Amount)[]
            {
                ("BUDGET_ENERGY", "Bütçe: Enerji (Elektrik, Su, Doğalgaz)", "FaturaAbonelik", "Isletme", "Genel", energy),
                ("BUDGET_CLEANING", "Bütçe: Bina Görevlisi & Temizlik/Deterjan", "BakimOnarim", "Isletme", "Temizlik", staffClean),
                ("BUDGET_ELEVATOR", "Bütçe: Asansör Bakım & Kontrol", "BakimOnarim", "Isletme", "Asansor", elevator),
                ("BUDGET_ADMIN", "Bütçe: Güvenlik, Bahçe, Sigorta & Yönetim", "YonetimIdari", "Isletme", "Genel", adminSec),
                ("BUDGET_RESERVE", $"Bütçe: Beklenmedik Gider İhtiyat Payı (%{reservePct:0.##})", "YonetimIdari", "Isletme", "Genel", reserveAmount),
                ("BUDGET_CAPITAL", "Bütçe: Demirbaş, Tadilat & Kıdem Fonu", "DemirbasYatirim", "Demirbas", "Demirbas", capital)
            };

            var existingPlans = await _context.BuildingFixedExpenses
                .Where(f => f.BuildingId == buildingId
                    && !f.IsDeleted
                    && f.SubscriberNo != null
                    && f.SubscriberNo.StartsWith("BUDGET_"))
                .ToListAsync();

            foreach (var p in planItems)
            {
                var match = existingPlans.FirstOrDefault(x => x.SubscriberNo == p.Code);
                if (p.Amount <= 0m)
                {
                    if (match != null)
                    {
                        match.MonthlyAmount = 0m;
                        match.IsAutoRecurring = false;
                        match.IsActive = false;
                    }
                    continue;
                }

                if (match == null)
                {
                    _context.BuildingFixedExpenses.Add(new BuildingFixedExpense
                    {
                        BuildingId = buildingId,
                        Title = p.Title,
                        ExpenseScope = p.Scope,
                        ExpenseGroup = p.Group,
                        ExpenseItemKey = p.ItemKey,
                        DistributionRule = p.Scope == "Demirbas" ? "ArsaPayi" : "Esit",
                        ScopeType = scopeType,
                        ShopParticipationPercent = 100m,
                        OwnerSplitPercent = p.Scope == "Demirbas" ? 100m : null,
                        DueDayOfMonth = building.DuesPaymentDay ?? 5,
                        ProviderName = "KMK İşletme Projesi",
                        SubscriberNo = p.Code,
                        MonthlyAmount = Math.Round(p.Amount, 2),
                        BillingCycleMonths = 1,
                        IsAutoRecurring = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    match.Title = p.Title;
                    match.ExpenseScope = p.Scope;
                    match.ExpenseGroup = p.Group;
                    match.ExpenseItemKey = p.ItemKey;
                    match.ScopeType = scopeType;
                    match.OwnerSplitPercent = p.Scope == "Demirbas" ? 100m : null;
                    match.MonthlyAmount = Math.Round(p.Amount, 2);
                    match.IsAutoRecurring = true;
                    match.IsActive = true;
                }
            }

            if (applyCalculatedAsBaseDues)
            {
                decimal? parsedRec = ParseFlexibleDecimal(recommendedBaseDues);
                if (parsedRec.HasValue && parsedRec.Value > 0m)
                {
                    var targets = isSiteWide
                        ? await _context.Buildings.Where(b => b.HousingComplexId == building.HousingComplexId && b.ParentBuildingId == null).ToListAsync()
                        : new System.Collections.Generic.List<Building> { building };
                    foreach (var b in targets)
                    {
                        b.DefaultBaseMonthlyDues = Math.Round(parsedRec.Value, 2);
                    }
                }
            }

            await _context.SaveChangesAsync();
            decimal monthlyTotal = operatingSubtotal + reserveAmount + capital;
            decimal annualTotal = monthlyTotal * 12m;
            TempData["SuccessMessage"] = $"📊 Yıllık İşletme Projesi bütçesi kaydedildi (Aylık {monthlyTotal:N2} ₺ · 12 Aylık Yıllık Bütçe {annualTotal:N2} ₺).";
            return RedirectToAction(nameof(Aidatlar), new { buildingId = isSiteWide ? (int?)null : buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PreviewExpenseDistribution(
            int buildingId,
            decimal totalAmount,
            BuildingExpenseType expenseType = BuildingExpenseType.Operational,
            string distributionRule = "Esit",
            string expenseItemKey = "Genel",
            ExpenseScopeType scopeType = ExpenseScopeType.Building,
            decimal shopParticipationPercent = 100m,
            decimal? ownerSplitPercent = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var result = await _governanceService.PreviewExpenseDistributionAsync(
                buildingId,
                totalAmount,
                expenseType,
                distributionRule,
                expenseItemKey,
                scopeType,
                shopParticipationPercent,
                ownerSplitPercent);

            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PreviewMonthlyDuesBatch(
            int buildingId,
            int year,
            int month,
            decimal defaultBaseDues = 0m,
            ExpenseScopeType scopeType = ExpenseScopeType.Building,
            decimal? ownerSplitPercent = null,
            string mode = "FixedUnitDues",
            bool includeStaffCosts = true)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var result = string.Equals(mode, "RecurringBudget", StringComparison.OrdinalIgnoreCase)
                ? await _payrollService.PreviewAllRecurringBudgetAsync(buildingId, year, month, includeStaffCosts)
                : await _governanceService.PreviewFixedUnitDuesAsync(
                    buildingId,
                    year,
                    month,
                    defaultBaseDues,
                    scopeType,
                    ownerSplitPercent);

            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostMonthlyDuesBatch(
            int buildingId,
            string mode,
            int year,
            int month,
            decimal defaultBaseDues = 0m,
            ExpenseScopeType scopeType = ExpenseScopeType.Building,
            decimal? ownerSplitPercent = null,
            bool includeStaffCosts = true,
            string? customDescription = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            GovernanceActionResult res;
            if (string.Equals(mode, "RecurringBudget", StringComparison.OrdinalIgnoreCase))
            {
                res = await _payrollService.DistributeAllRecurringBudgetAsync(
                    buildingId,
                    year,
                    month,
                    includeStaffCosts,
                    GetUserId() ?? "system");
            }
            else
            {
                res = await _governanceService.DistributeFixedUnitDuesAsync(
                    buildingId,
                    year,
                    month,
                    defaultBaseDues,
                    scopeType,
                    ownerSplitPercent,
                    GetUserId() ?? "system",
                    customDescription);
            }

            TempData[res.Success ? "SuccessMessage" : "ErrorMessage"] = res.Message;
            return RedirectToAction(nameof(Aidatlar), new { buildingId });
        }

        // 9. Sabit Giderler Ajandası + Satın Almalar & Fatura Takibi (Birleşik Muhasebe, Gider & Kasa ekranına yönlendirir)
        public IActionResult Giderler(int? buildingId)
        {
            return RedirectToAction(nameof(Accounting), new { buildingId, tab = "giderler" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBuildingFixedExpense(
            int buildingId,
            string? title = null,
            string expenseScope = "Isletme",
            string distributionRule = "Esit",
            int dueDayOfMonth = 15,
            decimal monthlyAmount = 0m,
            string? providerName = null,
            string? subscriberNo = null,
            bool isAutoRecurring = true,
            string expenseItemKey = "Genel",
            ExpenseScopeType scopeType = ExpenseScopeType.Building,
            decimal shopParticipationPercent = 100m,
            decimal? ownerSplitPercent = null,
            string? expenseGroup = null,
            string? returnAction = null,
            bool isVariableAmount = false,
            int billingCycleMonths = 1,
            int? billingMonth = null,
            DateTime? contractEndDate = null,
            string? physicalArchiveLocation = null,
            Microsoft.AspNetCore.Http.IFormFile? contractFile = null,
            bool postFirstPaymentNow = false,
            int? firstPaymentBankAccountId = null,
            [FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment? env = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            string? contractUrl = null;
            if (contractFile != null && contractFile.Length > 0)
            {
                var ext = System.IO.Path.GetExtension(contractFile.FileName)?.ToLowerInvariant() ?? "";
                var allowed = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowed.Contains(ext))
                {
                    TempData["ErrorMessage"] = "Sözleşme dosyası yalnızca PDF veya görsel (.jpg/.png/.webp) olabilir.";
                    return RedirectToAction(nameof(Accounting), new { buildingId, tab = "giderler" });
                }

                var webRoot = env?.WebRootPath ?? System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
                var folder = System.IO.Path.Combine(webRoot, "uploads", "buildings", buildingId.ToString(), "contracts");
                System.IO.Directory.CreateDirectory(folder);
                var fileName = $"sozlesme_{buildingId}_{Guid.NewGuid():N}{ext}";
                var fullPath = System.IO.Path.Combine(folder, fileName);
                using (var stream = new System.IO.FileStream(fullPath, System.IO.FileMode.Create))
                {
                    await contractFile.CopyToAsync(stream);
                }
                contractUrl = $"/uploads/buildings/{buildingId}/contracts/{fileName}";
            }

            int normalizedCycle = billingCycleMonths is 3 or 6 or 12 ? billingCycleMonths : 1;
            int? normalizedBillingMonth = normalizedCycle > 1 && contractEndDate.HasValue
                ? contractEndDate.Value.Month
                : (normalizedCycle > 1 && billingMonth.HasValue
                    ? Math.Clamp(billingMonth.Value, 1, 12)
                    : (normalizedCycle > 1 ? DateTime.Today.Month : null));
            int normalizedDueDay = normalizedCycle > 1 && contractEndDate.HasValue
                ? contractEndDate.Value.Day
                : Math.Clamp(dueDayOfMonth, 1, 31);

            var resolvedType = string.Equals(expenseScope, "Demirbas", StringComparison.OrdinalIgnoreCase)
                ? BuildingExpenseType.CapitalExpenditure
                : BuildingExpenseType.Operational;
            var resolvedGroup = FacilityGovernanceService.ResolveExpenseGroup(resolvedType, expenseItemKey, expenseGroup);
            decimal finalAmount = isVariableAmount ? 0m : Math.Max(0m, monthlyAmount);
            string resolvedTitle = !string.IsNullOrWhiteSpace(title)
                ? title.Trim()
                : (!string.IsNullOrWhiteSpace(providerName) ? providerName.Trim() : "Düzenli Abonelik");

            _context.BuildingFixedExpenses.Add(new BuildingFixedExpense
            {
                BuildingId = buildingId,
                Title = resolvedTitle,
                ExpenseScope = string.Equals(expenseScope, "Demirbas", StringComparison.OrdinalIgnoreCase) ? "Demirbas" : "Isletme",
                ExpenseGroup = resolvedGroup,
                ExpenseItemKey = string.IsNullOrWhiteSpace(expenseItemKey) ? "Genel" : expenseItemKey.Trim(),
                DistributionRule = string.IsNullOrWhiteSpace(distributionRule) ? "Esit" : distributionRule.Trim(),
                ScopeType = scopeType,
                ShopParticipationPercent = Math.Clamp(shopParticipationPercent, 0m, 500m),
                OwnerSplitPercent = ownerSplitPercent.HasValue ? Math.Clamp(ownerSplitPercent.Value, 0m, 100m) : null,
                DueDayOfMonth = normalizedDueDay,
                MonthlyAmount = finalAmount,
                BillingCycleMonths = normalizedCycle,
                BillingMonth = normalizedBillingMonth,
                ContractDocumentUrl = contractUrl,
                PhysicalArchiveLocation = string.IsNullOrWhiteSpace(physicalArchiveLocation) ? null : physicalArchiveLocation.Trim(),
                ContractEndDate = contractEndDate,
                ProviderName = providerName?.Trim(),
                SubscriberNo = subscriberNo?.Trim(),
                IsAutoRecurring = !isVariableAmount && isAutoRecurring && finalAmount > 0m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            if (!isVariableAmount && finalAmount > 0m && postFirstPaymentNow)
            {
                await _governanceService.DistributeExpenseAsync(
                    buildingId,
                    $"{resolvedTitle} ({DateTime.Today:yyyy-MM})",
                    finalAmount,
                    resolvedType,
                    null,
                    GetUserId() ?? "system",
                    "Esit",
                    "Genel",
                    ExpenseScopeType.Building,
                    100m,
                    null,
                    resolvedGroup,
                    chargeToUnits: false,
                    bankAccountId: firstPaymentBankAccountId);

                TempData["SuccessMessage"] = $"✅ '{resolvedTitle}' sözleşmesi kaydedildi ve ilk ödemesi ({finalAmount:N2} ₺) bina kasasına gider olarak işlendi.";
                var rs1 = Request.Form["returnScope"].FirstOrDefault();
                return RedirectToAction(nameof(Accounting), new { buildingId = rs1 == "site" ? (int?)null : buildingId, scope = rs1 == "site" ? "site" : null, tab = "abonelikler" });
            }

            TempData["SuccessMessage"] = isVariableAmount
                ? $"'{resolvedTitle}' değişken faturalı abonelik olarak kaydedildi. Ay geldiğinde yanındaki '+ Fatura / Ödeme İşle' butonundan fatura işleyebilirsiniz."
                : $"'{resolvedTitle}' düzenli abonelik/sözleşme listesine eklendi.";
            var rs2 = Request.Form["returnScope"].FirstOrDefault();
            return RedirectToAction(nameof(Accounting), new { buildingId = rs2 == "site" ? (int?)null : buildingId, scope = rs2 == "site" ? "site" : null, tab = "abonelikler" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DistributeBuildingFixedExpense(int fixedExpenseId, int buildingId)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var res = await _payrollService.DistributeFixedExpenseAsync(fixedExpenseId, DateTime.Today.Year, DateTime.Today.Month, GetUserId() ?? "system");
            TempData[res.Success ? "SuccessMessage" : "ErrorMessage"] = res.Message;
            return RedirectToAction(nameof(Accounting), new { buildingId, tab = "giderler" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFixedExpenseSchedule(
            int buildingId,
            string category,
            string itemName,
            decimal monthlyAmount,
            DateTime dueDate,
            string? serviceName,
            string? notes)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var commonUnit = await _context.BuildingUnits
                .Include(u => u.Spaces)
                .FirstOrDefaultAsync(u => u.BuildingId == buildingId && (u.Category == "OrtakAlan_Blok" || (u.Category ?? "").StartsWith("OrtakAlan")));
            if (commonUnit == null)
            {
                await EnsureBaselineCommonSpacesAsync(building);
                commonUnit = await _context.BuildingUnits
                    .Include(u => u.Spaces)
                    .FirstOrDefaultAsync(u => u.BuildingId == buildingId && (u.Category == "OrtakAlan_Blok" || (u.Category ?? "").StartsWith("OrtakAlan")));
            }

            var targetSpace = commonUnit!.Spaces.FirstOrDefault(s => s.Name.Contains("Sabit Gider"))
                ?? commonUnit.Spaces.FirstOrDefault();
            if (targetSpace == null)
            {
                targetSpace = new UnitSpace { BuildingUnitId = commonUnit.Id, Name = "Sabit Gider & Muhasebe", Type = "OrtakAlan" };
                _context.UnitSpaces.Add(targetSpace);
                await _context.SaveChangesAsync();
            }

            _context.SpaceFixtures.Add(new SpaceFixture
            {
                UnitSpaceId = targetSpace.Id,
                Category = category == "SGK & Maaş" ? "SGK & Maaş" : "SabitGider",
                ItemName = itemName?.Trim() ?? "Sabit Gider",
                EstimatedPrice = monthlyAmount,
                Quantity = 1,
                Unit = "Ay",
                TechnicalServiceName = serviceName?.Trim(),
                MaintenanceNotes = notes?.Trim(),
                NextMaintenanceDate = dueDate,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{itemName}' sabit gider ödeme takvimine eklendi.";
            return RedirectToAction(nameof(Giderler), new { buildingId });
        }

        // 7. İç Talepler & Saha Arıza Bildirimleri Yönetimi
        public async Task<IActionResult> TicketManagement(int? buildingId)
        {
            var buildings = await GetUserManagedRootBuildingsAsync();
            if (!buildings.Any())
            {
                TempData["ErrorMessage"] = "Önce yönettiğiniz bir bina veya site oluşturmalısınız.";
                return RedirectToAction(nameof(Index));
            }

            var rootIds = buildings.Select(b => b.Id).ToList();
            var childIds = await _context.Buildings
                .Where(b => b.ParentBuildingId.HasValue && rootIds.Contains(b.ParentBuildingId.Value))
                .Select(b => b.Id)
                .ToListAsync();
            var selectedIds = rootIds.Concat(childIds).Distinct().ToList();

            Building? selectedBuilding = null;
            if (buildingId.HasValue)
            {
                selectedBuilding = await GetManagedBuildingAsync(buildingId.Value);
                if (selectedBuilding == null) return Forbid();
                selectedIds = new System.Collections.Generic.List<int> { selectedBuilding.Id };
            }

            var tickets = await _context.SupportTickets
                .Include(t => t.CreatorUser)
                .Where(t => t.ContextType == "Building" && t.ContextId.HasValue && selectedIds.Contains(t.ContextId.Value))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var taskTickets = await _context.BuildingTaskTickets
                .Include(t => t.Building)
                .Include(t => t.BuildingUnit)
                .Include(t => t.AssignedStaff)
                .Where(t => !t.IsDeleted && selectedIds.Contains(t.BuildingId))
                .OrderByDescending(t => t.Id)
                .Take(100)
                .ToListAsync();

            var staffList = await _context.BuildingStaffs
                .Where(s => !s.IsDeleted && s.IsActive && selectedIds.Contains(s.BuildingId))
                .OrderBy(s => s.FullName)
                .ToListAsync();

            ViewBag.Building = selectedBuilding ?? buildings.First();
            ViewBag.Buildings = buildings;
            ViewBag.SelectedBuildingId = buildingId;
            ViewBag.TaskTickets = taskTickets;
            ViewBag.StaffList = staffList;
            return View(tickets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignOrCloseTaskTicket(
            int ticketId,
            int buildingId,
            int? assignedStaffId,
            string? status,
            string? completionNote,
            [FromServices] FacilityTaskDispatchService taskDispatch)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var tk = await _context.BuildingTaskTickets.FirstOrDefaultAsync(t => t.Id == ticketId && t.BuildingId == buildingId && !t.IsDeleted);
            if (tk == null)
            {
                TempData["ErrorMessage"] = "Arıza / talep kaydı bulunamadı.";
                return RedirectToAction(nameof(TicketManagement), new { buildingId });
            }

            if (status == "Tamamlandi")
            {
                await taskDispatch.CompleteTicketAsync(ticketId, string.IsNullOrWhiteSpace(completionNote) ? "Yönetim tarafından kapatıldı" : completionNote, null);
                TempData["SuccessMessage"] = $"Arıza bildirimi #{ticketId} tamamlandı olarak kapatıldı.";
            }
            else if (assignedStaffId.HasValue && assignedStaffId.Value > 0)
            {
                await taskDispatch.AssignTicketAsync(ticketId, assignedStaffId.Value, tk.Priority);
                TempData["SuccessMessage"] = $"Arıza bildirimi #{ticketId} görevliye atandı.";
            }
            else if (!string.IsNullOrWhiteSpace(status))
            {
                tk.Status = status;
                tk.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Arıza bildirimi #{ticketId} güncellendi.";
            }

            return RedirectToAction(nameof(TicketManagement), new { buildingId });
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

        private async Task<System.Collections.Generic.List<DefinitionValue>> GetFacilityExpenseGroupLookupsAsync()
        {
            var dbItems = await _context.DefinitionValues
                .AsNoTracking()
                .Include(v => v.Category)
                .Where(v => v.Category != null && v.Category.SystemCode == "FACILITY_EXPENSE_GROUP" && !v.IsDeleted)
                .OrderBy(v => v.Order)
                .ToListAsync();

            if (dbItems.Any()) return dbItems;

            return new System.Collections.Generic.List<DefinitionValue>
            {
                new() { SystemCode = "Personel", Name = "Personel Giderleri (Maaş, SGK, Kıdem)", Order = 1 },
                new() { SystemCode = "FaturaAbonelik", Name = "Faturalar & Abonelikler (Elektrik, Su, Gaz)", Order = 2 },
                new() { SystemCode = "BakimOnarim", Name = "Bakım, Onarım & Sarf Giderleri", Order = 3 },
                new() { SystemCode = "DemirbasYatirim", Name = "Demirbaş & Büyük Tadilat (Malik)", Order = 4 },
                new() { SystemCode = "YonetimIdari", Name = "Yönetim, Sigorta & İdari Giderler", Order = 5 }
            };
        }

        // 10. Birleşik Muhasebe, Gider Türleri, Kasa/Banka & Sabit Personel
        public async Task<IActionResult> Accounting(int? buildingId, string? tab = null, string? expenseGroup = null, string? scope = null)
        {
            var buildings = await GetUserManagedRootBuildingsAsync();
            if (!buildings.Any())
            {
                TempData["ErrorMessage"] = "Muhasebe ve kasa ekranı için önce bir bina/site oluşturun.";
                return RedirectToAction(nameof(Index));
            }

            bool isSiteScope = buildings.Count > 1 && (
                string.Equals(scope, "site", StringComparison.OrdinalIgnoreCase)
                || (!buildingId.HasValue && string.IsNullOrWhiteSpace(scope)));

            int targetId = buildingId ?? buildings.First().Id;
            var building = await _context.Buildings
                .Include(b => b.HousingComplex)
                .Include(b => b.ManagementMembers)
                .FirstOrDefaultAsync(b => b.Id == targetId);

            if (building == null) return NotFound();
            if (!CanAccessBuilding(building, GetUserId())) return Forbid();

            var scopeBuildings = isSiteScope
                ? (building.HousingComplexId.HasValue
                    ? buildings.Where(b => b.HousingComplexId == building.HousingComplexId).ToList()
                    : buildings)
                : new System.Collections.Generic.List<Building> { building };
            if (!scopeBuildings.Any()) scopeBuildings = new System.Collections.Generic.List<Building> { building };
            var scopeIds = scopeBuildings.Select(b => b.Id).ToList();

            ViewBag.BankAccounts = await _context.BankAccounts
                .Where(a => a.OwnerType == OwnerType.Building && a.OwnerId.HasValue && scopeIds.Contains(a.OwnerId.Value) && a.IsActive)
                .OrderByDescending(a => a.Id)
                .ToListAsync();

            var staffIds = isSiteScope
                ? await _context.ManagementMembers
                    .Where(m => m.BuildingId.HasValue && scopeIds.Contains(m.BuildingId.Value))
                    .Select(m => m.Id)
                    .ToListAsync()
                : (building.ManagementMembers ?? new System.Collections.Generic.List<ManagementMember>()).Select(m => m.Id).ToList();
            ViewBag.Payrolls = staffIds.Any()
                ? await _context.StaffPayrolls
                    .Include(p => p.ManagementMember)
                    .Include(p => p.PaidFromBankAccount)
                    .Where(p => staffIds.Contains(p.ManagementMemberId))
                    .OrderByDescending(p => p.Id)
                    .ToListAsync()
                : new System.Collections.Generic.List<StaffPayroll>();

            var buildingStaffs = await _context.BuildingStaffs
                .Include(s => s.Leaves)
                .Include(s => s.Building)
                .Where(s => scopeIds.Contains(s.BuildingId) && s.IsActive && !s.IsDeleted)
                .OrderBy(s => s.FullName)
                .ToListAsync();
            var bStaffIds = buildingStaffs.Select(s => s.Id).ToList();

            ViewBag.BuildingStaffs = buildingStaffs;
            ViewBag.MonthlyPayrolls = bStaffIds.Any()
                ? await _context.BuildingStaffMonthlyPayrolls
                    .Include(p => p.Staff)
                    .Where(p => bStaffIds.Contains(p.BuildingStaffId) && !p.IsDeleted)
                    .OrderByDescending(p => p.Year)
                    .ThenByDescending(p => p.Month)
                    .ThenByDescending(p => p.Id)
                    .ToListAsync()
                : new System.Collections.Generic.List<BuildingStaffMonthlyPayroll>();

            ViewBag.Expenses = await _context.BuildingExpenses
                .Include(e => e.Building)
                .Where(e => e.BuildingId.HasValue && scopeIds.Contains(e.BuildingId.Value))
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            ViewBag.FixedExpenses = await _context.BuildingFixedExpenses
                .Include(f => f.Building)
                .Where(f => scopeIds.Contains(f.BuildingId) && f.IsActive && !f.IsDeleted)
                .OrderBy(f => f.DueDayOfMonth)
                .ToListAsync();

            ViewBag.TaskTickets = await _context.BuildingTaskTickets
                .Include(t => t.Building)
                .Where(t => scopeIds.Contains(t.BuildingId) && !t.IsDeleted)
                .OrderByDescending(t => t.Id)
                .Take(60)
                .ToListAsync();

            ViewBag.CommonFixtures = await _context.SpaceFixtures
                .Include(fx => fx.UnitSpace).ThenInclude(us => us.BuildingUnit).ThenInclude(u => u.Building)
                .Where(fx => fx.UnitSpace.BuildingUnit != null
                    && scopeIds.Contains(fx.UnitSpace.BuildingUnit.BuildingId)
                    && (fx.UnitSpace.BuildingUnit.Category == "OrtakAlan_Blok" || (fx.UnitSpace.BuildingUnit.Category != null && fx.UnitSpace.BuildingUnit.Category.StartsWith("OrtakAlan"))))
                .OrderBy(fx => fx.ItemName)
                .Take(40)
                .ToListAsync();

            ViewBag.TotalCollectedDues = await _context.UnitDebts
                .Where(d => d.IsPaid && !d.IsArchived && scopeIds.Contains(d.BuildingUnit.BuildingId))
                .SumAsync(d => (decimal?)d.Amount) ?? 0m;

            ViewBag.TotalPendingDues = await _context.UnitDebts
                .Where(d => !d.IsPaid && !d.IsArchived && scopeIds.Contains(d.BuildingUnit.BuildingId))
                .SumAsync(d => (decimal?)d.Amount) ?? 0m;

            ViewBag.ExpenseGroups = await GetFacilityExpenseGroupLookupsAsync();
            var normalizedTab = string.IsNullOrWhiteSpace(tab) ? "faturalar" : tab.Trim().ToLowerInvariant();
            if (normalizedTab == "giderler") normalizedTab = "faturalar";
            ViewBag.ActiveTab = normalizedTab;
            ViewBag.SelectedExpenseGroup = expenseGroup;
            ViewBag.IsSiteScope = isSiteScope;
            ViewBag.ScopeBuildingCount = scopeIds.Count;
            ViewBag.SiteTitleLabel = !string.IsNullOrWhiteSpace(building.HousingComplex?.Name)
                ? building.HousingComplex!.Name
                : (building.Name ?? "Site Geneli");
            ViewBag.BudgetSummary = await _payrollService.CalculateMultiBuildingBudgetSummaryAsync(scopeIds, building.Id);
            ViewBag.Holidays = GMK360.Web.Helpers.TurkishHolidayHelper.GetHolidaysForYear(DateTime.Today.Year);
            ViewBag.Buildings = buildings;

            return View(building);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBankAccount(int buildingId, string bankName, string accountName, string iban)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            _context.BankAccounts.Add(new BankAccount
            {
                OwnerType = OwnerType.Building,
                OwnerId = buildingId,
                BankName = bankName?.Trim() ?? "Banka",
                AccountName = accountName?.Trim() ?? "Aidat Hesabı",
                IBAN = iban?.Trim() ?? "",
                CurrentBalance = 0m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = "Banka hesabı kaydedildi.";
            return RedirectToAction(nameof(Accounting), new { buildingId, tab = "kasa" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStaff(int buildingId, string firstName, string lastName, string title, decimal salary, ManagementStaffRole role, string? sgkNumber = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            _context.ManagementMembers.Add(new ManagementMember
            {
                BuildingId = buildingId,
                FirstName = firstName?.Trim() ?? "",
                LastName = lastName?.Trim() ?? "",
                Title = title?.Trim() ?? "Personel",
                MonthlySalary = salary,
                SgkNumber = sgkNumber?.Trim(),
                Role = role,
                IsActive = true,
                StartDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = "Sabit personel ve SGK sicil bilgisi kadroya eklendi.";
            return RedirectToAction(nameof(Accounting), new { buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayStaffSalaryAndSgk(
            int buildingId,
            int managementMemberId,
            string period,
            decimal netSalary,
            decimal sgkAndTaxAmount,
            int? bankAccountId,
            bool distributeToUnits)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var staff = await _context.ManagementMembers
                .FirstOrDefaultAsync(m => m.Id == managementMemberId && m.BuildingId == buildingId);
            if (staff == null) return NotFound();

            _context.StaffPayrolls.Add(new StaffPayroll
            {
                ManagementMemberId = staff.Id,
                Period = string.IsNullOrWhiteSpace(period) ? DateTime.Today.ToString("yyyy-MM") : period.Trim(),
                NetSalary = netSalary,
                IsPaid = true,
                PaymentDate = DateTime.Now,
                PaidFromBankAccountId = bankAccountId,
                CreatedAt = DateTime.UtcNow
            });

            if (bankAccountId.HasValue)
            {
                var acc = await _context.BankAccounts.FirstOrDefaultAsync(a => a.Id == bankAccountId.Value && a.OwnerId == buildingId);
                if (acc != null)
                {
                    acc.CurrentBalance -= (netSalary + Math.Max(0m, sgkAndTaxAmount));
                }
            }
            await _context.SaveChangesAsync();

            if (distributeToUnits && (netSalary + Math.Max(0m, sgkAndTaxAmount)) > 0)
            {
                await _governanceService.DistributeExpenseAsync(
                    buildingId,
                    $"Personel Maaş + SGK ({period}) - {staff.FirstName} {staff.LastName} ({staff.Title})",
                    netSalary + Math.Max(0m, sgkAndTaxAmount),
                    BuildingExpenseType.Operational,
                    null,
                    GetUserId() ?? "system");
            }

            TempData["Success"] = $"{staff.FirstName} {staff.LastName} ({period}) maaş & SGK ödemesi muhasebeye işlendi.";
            return RedirectToAction(nameof(Accounting), new { buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBuildingStaff(
            int buildingId,
            string fullName,
            string? tcIdentityNo,
            string? sgkNumber,
            string? phoneNumber,
            string? quickPin,
            string roleType,
            decimal netSalary,
            decimal sgkEmployerShare,
            decimal incomeAndStampTax,
            decimal fringeBenefits,
            decimal monthlySeveranceFund,
            DateTime hireDate)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var staff = new BuildingStaff
            {
                BuildingId = buildingId,
                FullName = fullName?.Trim() ?? "Personel",
                TcIdentityNo = tcIdentityNo?.Trim(),
                SgkNumber = sgkNumber?.Trim(),
                PhoneNumber = phoneNumber?.Trim(),
                QuickPinHash = (!string.IsNullOrWhiteSpace(quickPin) && quickPin.Trim().Length >= 4)
                    ? FacilityTaskDispatchService.HashQuickPin(quickPin.Trim())
                    : null,
                RoleType = string.IsNullOrWhiteSpace(roleType) ? "Kapıcı (Vergi İstisnalı)" : roleType.Trim(),
                NetSalary = Math.Max(0m, netSalary),
                SgkEmployerShare = Math.Max(0m, sgkEmployerShare),
                IncomeAndStampTax = Math.Max(0m, incomeAndStampTax),
                FringeBenefits = Math.Max(0m, fringeBenefits),
                MonthlySeveranceFund = Math.Max(0m, monthlySeveranceFund),
                HireDate = hireDate == default ? DateTime.Today : hireDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.BuildingStaffs.Add(staff);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{staff.FullName}' kadroya eklendi. Aylık işletme maliyeti: {staff.TotalMonthlyOperatingCost:N2} ₺, Kıdem fonu: {staff.MonthlySeveranceFund:N2} ₺.";
            return RedirectToAction(nameof(Accounting), new { buildingId, tab = "personel" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlanBuildingStaffLeave(
            int buildingId,
            int buildingStaffId,
            string leaveType,
            DateTime startDate,
            DateTime endDate,
            decimal replacementCost,
            string? notes)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var res = await _payrollService.PlanStaffLeaveAsync(buildingStaffId, leaveType, startDate, endDate, replacementCost, notes);
            if (res.OverlapWarning != null)
            {
                TempData["WarningMessage"] = res.Message;
            }
            else
            {
                TempData[res.Success ? "Success" : "ErrorMessage"] = res.Message;
            }
            return RedirectToAction(nameof(Accounting), new { buildingId, tab = "personel" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateBuildingStaffPayroll(
            int buildingId,
            int buildingStaffId,
            int year,
            int month,
            decimal holidayDaysWorked,
            int unpaidLeaveDays,
            decimal? overrideReplacementCost)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var payroll = await _payrollService.GenerateOrUpdateMonthlyPayrollAsync(
                buildingStaffId,
                year < 2020 ? DateTime.Today.Year : year,
                Math.Clamp(month, 1, 12),
                holidayDaysWorked,
                unpaidLeaveDays,
                overrideReplacementCost);

            if (payroll == null)
            {
                TempData["ErrorMessage"] = "Puantaj oluşturulacak personel bulunamadı.";
            }
            else
            {
                TempData["Success"] = $"{payroll.Year}-{payroll.Month:00} puantajı hesaplandı (Ödenecek Net: {payroll.NetPayableToStaff:N2} ₺, Toplam Site Yükü: {payroll.TotalSiteCostForMonth:N2} ₺).";
            }
            return RedirectToAction(nameof(Accounting), new { buildingId, tab = "personel" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayBuildingStaffPayroll(
            int buildingId,
            int payrollId,
            string paymentKind,
            int? bankAccountId,
            bool distributeOperatingToUnits,
            bool postSeveranceFundToOwners)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var res = await _payrollService.MarkPayrollPaidAndPostLedgersAsync(
                payrollId,
                paymentKind,
                bankAccountId,
                distributeOperatingToUnits,
                postSeveranceFundToOwners,
                GetUserId() ?? "system");

            TempData[res.Success ? "Success" : "ErrorMessage"] = res.Message;
            return RedirectToAction(nameof(Accounting), new { buildingId, tab = "personel" });
        }

        // 11. Toplantı & Karar Masası
        public async Task<IActionResult> Oylamalar(int? buildingId)
        {
            var buildings = await GetUserManagedRootBuildingsAsync();
            var allowedIds = buildings.Select(b => b.Id).ToList();
            if (buildingId.HasValue)
            {
                var b = await GetManagedBuildingAsync(buildingId.Value);
                if (b == null) return Forbid();
                allowedIds = new System.Collections.Generic.List<int> { b.Id };
            }

            var meetings = allowedIds.Any()
                ? await _context.SystemMeetings
                    .Include(m => m.Decisions)
                    .Where(m => m.ContextType == "Building" && allowedIds.Contains(m.ContextId))
                    .OrderByDescending(m => m.MeetingDate)
                    .ToListAsync()
                : new System.Collections.Generic.List<SystemMeeting>();

            var approvals = allowedIds.Any()
                ? await _context.ApprovalRequests
                    .Include(a => a.Building)
                    .Where(a => allowedIds.Contains(a.BuildingId))
                    .OrderByDescending(a => a.Id)
                    .ToListAsync()
                : new System.Collections.Generic.List<ApprovalRequest>();

            var meetingIds = meetings.Select(m => m.Id).ToList();

            var meetingQuotes = meetingIds.Any()
                ? await _context.B2BQuoteRequests
                    .Include(q => q.Invites).ThenInclude(i => i.NetworkContact)
                    .Where(q => q.SourceModule == "BuildingMeeting" && meetingIds.Contains(q.SourceReferenceId))
                    .OrderByDescending(q => q.Id)
                    .ToListAsync()
                : new System.Collections.Generic.List<GMK360.Core.Entities.B2B.B2BQuoteRequest>();

            var candidateJobs = meetingIds.Any()
                ? await _context.MarketplaceJobs
                    .Include(j => j.Bids)
                    .Where(j => j.IsActive && j.Description != null && j.Description.Contains("[Karar #"))
                    .OrderByDescending(j => j.Id)
                    .ToListAsync()
                : new System.Collections.Generic.List<GMK360.Core.Entities.Marketplace.MarketplaceJob>();

            var jobsByMeeting = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<GMK360.Core.Entities.Marketplace.MarketplaceJob>>();
            foreach (var mId in meetingIds)
            {
                var tag = $"[Karar #{mId}]";
                var matched = candidateJobs.Where(j => j.Description != null && j.Description.Contains(tag)).ToList();
                if (matched.Any()) jobsByMeeting[mId] = matched;
            }

            var decisionStaffs = allowedIds.Any()
                ? await _context.BuildingStaffs
                    .Where(s => allowedIds.Contains(s.BuildingId) && !s.IsDeleted && s.IsActive && s.QuickPinHash != null && s.RoleType != null && s.RoleType.Contains("[Karar #"))
                    .OrderByDescending(s => s.Id)
                    .ToListAsync()
                : new System.Collections.Generic.List<BuildingStaff>();

            var staffsByMeeting = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<BuildingStaff>>();
            foreach (var mId in meetingIds)
            {
                var tag = $"[Karar #{mId}]";
                var matched = decisionStaffs.Where(s => s.RoleType != null && s.RoleType.Contains(tag)).ToList();
                if (matched.Any()) staffsByMeeting[mId] = matched;
            }

            ViewBag.Buildings = buildings;
            ViewBag.BuildingMap = buildings.ToDictionary(b => b.Id, b => b.Name);
            ViewBag.SelectedBuildingId = buildingId;
            ViewBag.Approvals = approvals;
            ViewBag.QuotesByMeeting = meetingQuotes.GroupBy(q => q.SourceReferenceId).ToDictionary(g => g.Key, g => g.ToList());
            ViewBag.JobsByMeeting = jobsByMeeting;
            ViewBag.StaffsByMeeting = staffsByMeeting;
            return View(meetings);
        }

        private async Task<int?> ResolvePurchasingAgencyIdForBuildingAsync(Building building)
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                var consultant = await _context.Set<AgencyConsultant>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.AgencyId > 0);
                if (consultant != null) return consultant.AgencyId;
            }

            var agencyClaim = User.FindFirst("AgencyId")?.Value;
            if (int.TryParse(agencyClaim, out int parsedAgencyId) && parsedAgencyId > 0)
                return parsedAgencyId;

            if (building.ConstructionProjectId.HasValue)
            {
                var projAgency = await _context.ConstructionProjects
                    .AsNoTracking()
                    .Where(p => p.Id == building.ConstructionProjectId.Value && p.AgencyId > 0)
                    .Select(p => (int?)p.AgencyId)
                    .FirstOrDefaultAsync();
                if (projAgency.HasValue) return projAgency.Value;
            }

            var activeProjAgency = await _context.ConstructionProjects
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.AgencyId > 0)
                .OrderByDescending(p => p.Id)
                .Select(p => (int?)p.AgencyId)
                .FirstOrDefaultAsync();
            if (activeProjAgency.HasValue) return activeProjAgency.Value;

            return await _context.Agencies
                .AsNoTracking()
                .OrderBy(a => a.Id)
                .Select(a => (int?)a.Id)
                .FirstOrDefaultAsync();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenMarketplaceJobFromMeeting(
            int meetingId,
            int buildingId,
            string title,
            string jobType,
            decimal? estimatedBudget,
            string? extraNote)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var meeting = await _context.SystemMeetings
                .Include(m => m.Decisions)
                .FirstOrDefaultAsync(m => m.Id == meetingId && m.ContextType == "Building" && m.ContextId == buildingId);
            if (meeting == null) return NotFound();

            var buildingFull = await _context.Buildings
                .Include(b => b.City)
                .Include(b => b.District)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            var locParts = new[] { buildingFull?.City?.Name, buildingFull?.District?.Name, building.Name }
                .Where(x => !string.IsNullOrWhiteSpace(x));
            var locationText = string.Join(" / ", locParts);

            var decisionSummary = meeting.Decisions != null && meeting.Decisions.Any()
                ? string.Join("\n", meeting.Decisions.Select(d => "• " + d.DecisionText))
                : meeting.AgendaTitle;

            var desc = $"[Karar #{meeting.Id}] {building.Name} — {meeting.AgendaTitle} ({meeting.MeetingDate:dd.MM.yyyy})\n\nKarar Detayı:\n{decisionSummary}";
            if (!string.IsNullOrWhiteSpace(extraNote))
            {
                desc += $"\n\nKeşif / İş Kapsamı Notu:\n{extraNote.Trim()}";
            }

            var job = new GMK360.Core.Entities.Marketplace.MarketplaceJob
            {
                Title = string.IsNullOrWhiteSpace(title) ? $"{building.Name} — {meeting.AgendaTitle}" : title.Trim(),
                Description = desc,
                Location = string.IsNullOrWhiteSpace(locationText) ? building.Name : locationText,
                JobType = string.IsNullOrWhiteSpace(jobType) ? "Taşeron / Usta" : jobType.Trim(),
                EstimatedBudget = estimatedBudget.HasValue && estimatedBudget.Value > 0 ? estimatedBudget : null,
                CreatedByUserId = GetUserId() ?? "",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.MarketplaceJobs.Add(job);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"📢 Pazar Yeri ilanı (#{job.Id}) yayına alındı! Ustalar ve firmalar artık bu karar için teklif verebilir.";
            return RedirectToAction(nameof(Oylamalar), new { buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenB2BQuoteFromMeeting(
            int meetingId,
            int buildingId,
            string? title,
            DateTime? deadline,
            string? extraNote)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var meeting = await _context.SystemMeetings
                .Include(m => m.Decisions)
                .FirstOrDefaultAsync(m => m.Id == meetingId && m.ContextType == "Building" && m.ContextId == buildingId);
            if (meeting == null) return NotFound();

            var existingQuote = await _context.B2BQuoteRequests
                .OrderByDescending(q => q.Id)
                .FirstOrDefaultAsync(q => q.SourceModule == "BuildingMeeting" && q.SourceReferenceId == meetingId);
            if (existingQuote != null)
            {
                return RedirectToAction("Details", "B2BPurchasing", new { id = existingQuote.Id });
            }

            var agencyId = await ResolvePurchasingAgencyIdForBuildingAsync(building);
            if (!agencyId.HasValue || agencyId.Value <= 0)
            {
                TempData["ErrorMessage"] = "Sistemde teklif talebi açmak için tanımlı bir ajans profili bulunamadı.";
                return RedirectToAction(nameof(Oylamalar), new { buildingId });
            }

            var decisionSummary = meeting.Decisions != null && meeting.Decisions.Any()
                ? string.Join(" | ", meeting.Decisions.Select(d => d.DecisionText))
                : meeting.AgendaTitle;

            var desc = $"[Karar #{meeting.Id} - {building.Name}] {decisionSummary}";
            if (!string.IsNullOrWhiteSpace(extraNote))
            {
                desc += $" | Not: {extraNote.Trim()}";
            }

            var quote = new GMK360.Core.Entities.B2B.B2BQuoteRequest
            {
                RequesterAgencyId = agencyId.Value,
                RequesterUserId = GetUserId() ?? "",
                SourceModule = "BuildingMeeting",
                SourceReferenceId = meeting.Id,
                Title = string.IsNullOrWhiteSpace(title) ? $"{building.Name} — {meeting.AgendaTitle}" : title.Trim(),
                Description = desc,
                Deadline = deadline ?? DateTime.UtcNow.AddDays(10),
                Status = "Draft",
                CreatedAt = DateTime.UtcNow
            };

            _context.B2BQuoteRequests.Add(quote);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "B2BPurchasing", new { id = quote.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantDecisionPinAccess(
            int meetingId,
            int buildingId,
            string fullName,
            string phoneNumber,
            string roleTitle,
            string quickPin)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var meeting = await _context.SystemMeetings
                .FirstOrDefaultAsync(m => m.Id == meetingId && m.ContextType == "Building" && m.ContextId == buildingId);
            if (meeting == null) return NotFound();

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(quickPin) || quickPin.Trim().Length < 4)
            {
                TempData["ErrorMessage"] = "Yetkili hesabı açmak için ad soyad, telefon numarası ve en az 4 haneli PIN zorunludur.";
                return RedirectToAction(nameof(Oylamalar), new { buildingId });
            }

            var cleanRole = string.IsNullOrWhiteSpace(roleTitle) ? "Karar Yetkilisi / Usta" : roleTitle.Trim();
            var staff = new BuildingStaff
            {
                BuildingId = buildingId,
                FullName = fullName.Trim(),
                PhoneNumber = phoneNumber.Trim(),
                QuickPinHash = FacilityTaskDispatchService.HashQuickPin(quickPin.Trim()),
                RoleType = $"{cleanRole} [Karar #{meeting.Id}]",
                NetSalary = 0m,
                SgkEmployerShare = 0m,
                IncomeAndStampTax = 0m,
                FringeBenefits = 0m,
                MonthlySeveranceFund = 0m,
                HireDate = DateTime.Today,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.BuildingStaffs.Add(staff);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"🔑 '{staff.FullName}' için Karar #{meeting.Id} kapsamında Telefon + PIN giriş yetkisi açıldı (/FacilityPortal).";
            return RedirectToAction(nameof(Oylamalar), new { buildingId });
        }

        // 12. Usta & Tedarikçi Rehberi (Tesis Özel + Onaylı Platform Rehberi)
        public async Task<IActionResult> Rehber(int? buildingId)
        {
            var buildings = await GetUserManagedRootBuildingsAsync();
            var allowedIds = buildings.Select(b => b.Id).ToList();
            if (buildingId.HasValue)
            {
                var b = await GetManagedBuildingAsync(buildingId.Value);
                if (b == null) return Forbid();
                allowedIds = new System.Collections.Generic.List<int> { b.Id };
            }

            var fixtures = allowedIds.Any()
                ? await _context.SpaceFixtures
                    .Include(f => f.UnitSpace).ThenInclude(s => s.BuildingUnit).ThenInclude(u => u.Building)
                    .Where(f => allowedIds.Contains(f.UnitSpace.BuildingUnit.BuildingId)
                        && (!string.IsNullOrEmpty(f.TechnicalServiceName) || !string.IsNullOrEmpty(f.SupplierName)))
                    .OrderByDescending(f => f.Id)
                    .ToListAsync()
                : new System.Collections.Generic.List<SpaceFixture>();

            var providers = await _context.ServiceProviders
                .Include(sp => sp.User)
                .OrderByDescending(sp => sp.IsVerified)
                .ThenByDescending(sp => sp.AverageRating)
                .Take(24)
                .ToListAsync();

            ViewBag.Buildings = buildings;
            ViewBag.SelectedBuildingId = buildingId;
            ViewBag.ServiceProviders = providers;
            return View(fixtures);
        }

        // 13. Planlı İşler & Mevsimlik Bakım Takvimi
        public async Task<IActionResult> BakimTakvimi(int? buildingId)
        {
            var buildings = await GetUserManagedRootBuildingsAsync();
            var allowedIds = buildings.Select(b => b.Id).ToList();
            if (buildingId.HasValue)
            {
                var b = await GetManagedBuildingAsync(buildingId.Value);
                if (b == null) return Forbid();
                allowedIds = new System.Collections.Generic.List<int> { b.Id };
            }

            var fixtures = allowedIds.Any()
                ? await _context.SpaceFixtures
                    .Include(f => f.UnitSpace).ThenInclude(s => s.BuildingUnit).ThenInclude(u => u.Building)
                    .Where(f => allowedIds.Contains(f.UnitSpace.BuildingUnit.BuildingId))
                    .OrderBy(f => f.NextMaintenanceDate ?? DateTime.MaxValue)
                    .ToListAsync()
                : new System.Collections.Generic.List<SpaceFixture>();

            var meetings = allowedIds.Any()
                ? await _context.SystemMeetings
                    .Where(m => m.ContextType == "Building" && allowedIds.Contains(m.ContextId))
                    .OrderByDescending(m => m.MeetingDate)
                    .ToListAsync()
                : new System.Collections.Generic.List<SystemMeeting>();

            ViewBag.Buildings = buildings;
            ViewBag.SelectedBuildingId = buildingId;
            ViewBag.Meetings = meetings;
            return View(fixtures);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SchedulePlannedMaintenance(
            int buildingId,
            string spaceName,
            string category,
            string itemName,
            string? serviceName,
            DateTime nextMaintenanceDate,
            string? notes)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var commonUnit = await _context.BuildingUnits
                .Include(u => u.Spaces)
                .FirstOrDefaultAsync(u => u.BuildingId == buildingId && (u.Category == "OrtakAlan_Blok" || (u.Category ?? "").StartsWith("OrtakAlan")));

            if (commonUnit == null)
            {
                await EnsureBaselineCommonSpacesAsync(building);
                commonUnit = await _context.BuildingUnits
                    .Include(u => u.Spaces)
                    .FirstOrDefaultAsync(u => u.BuildingId == buildingId && (u.Category == "OrtakAlan_Blok" || (u.Category ?? "").StartsWith("OrtakAlan")));
            }

            var targetSpace = commonUnit!.Spaces.FirstOrDefault(s => s.Name.Contains(spaceName, StringComparison.OrdinalIgnoreCase))
                ?? commonUnit.Spaces.FirstOrDefault();

            if (targetSpace == null)
            {
                targetSpace = new UnitSpace { BuildingUnitId = commonUnit.Id, Name = spaceName, Type = "OrtakAlan" };
                _context.UnitSpaces.Add(targetSpace);
                await _context.SaveChangesAsync();
            }

            _context.SpaceFixtures.Add(new SpaceFixture
            {
                UnitSpaceId = targetSpace.Id,
                Category = string.IsNullOrWhiteSpace(category) ? "Bakım" : category.Trim(),
                ItemName = itemName?.Trim() ?? "Planlı Bakım İşi",
                Quantity = 1,
                Unit = "İş",
                TechnicalServiceName = serviceName?.Trim(),
                MaintenanceNotes = notes?.Trim(),
                NextMaintenanceDate = nextMaintenanceDate,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{itemName}' planlı işler bakım takvimine eklendi.";
            return RedirectToAction(nameof(BakimTakvimi), new { buildingId });
        }
    }
}
