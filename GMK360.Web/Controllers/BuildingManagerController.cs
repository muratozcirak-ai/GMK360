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
                var today = DateTime.Today;
                return _context.BuildingManagers.Any(m =>
                    m.BuildingId == rootId
                    && m.UserId == userId
                    && m.IsActive
                    && m.Role != BuildingManagerRole.ReserveMember
                    && (!m.TermEndDate.HasValue || m.TermEndDate.Value.Date >= today));
            }

            return building.ManagerUserId == userId || building.CreatedByUserId == userId;
        }

        private async Task<bool> CanEditGovernanceRecordsAsync(Building? building)
        {
            if (building == null) return false;
            if (IsAdminOrSuperAdmin()) return true;
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return false;

            int rootId = building.ParentBuildingId ?? building.Id;
            bool hasRoster = await _context.BuildingManagers.AnyAsync(m => m.BuildingId == rootId);
            if (!hasRoster)
            {
                return building.ManagerUserId == userId || building.CreatedByUserId == userId;
            }

            var today = DateTime.Today;
            return await _context.BuildingManagers.AnyAsync(m =>
                m.BuildingId == rootId
                && m.UserId == userId
                && m.IsActive
                && (m.Role == BuildingManagerRole.HeadManager || m.Role == BuildingManagerRole.ViceChair || m.Role == BuildingManagerRole.BoardMember)
                && (!m.TermEndDate.HasValue || m.TermEndDate.Value.Date >= today));
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
                .Include(b => b.HousingComplex)
                .Where(b => b.ParentBuildingId == null)
                .AsQueryable();

            if (!IsAdminOrSuperAdmin())
            {
                var today = DateTime.Today;
                query = query.Where(b =>
                    b.BuildingManagers.Any(m => m.IsActive)
                        ? b.BuildingManagers.Any(m =>
                            m.UserId == userId
                            && m.IsActive
                            && m.Role != BuildingManagerRole.ReserveMember
                            && (!m.TermEndDate.HasValue || m.TermEndDate.Value.Date >= today))
                        : (b.ManagerUserId == userId || b.CreatedByUserId == userId));
            }

            return await query.OrderBy(b => b.Id).ToListAsync();
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
                var today = DateTime.Today;
                query = query.Where(b =>
                    b.BuildingManagers.Any(m => m.IsActive)
                        ? b.BuildingManagers.Any(m =>
                            m.UserId == userId
                            && m.IsActive
                            && m.Role != BuildingManagerRole.ReserveMember
                            && (!m.TermEndDate.HasValue || m.TermEndDate.Value.Date >= today))
                        : (b.ManagerUserId == userId || b.CreatedByUserId == userId));
            }

            var buildings = await query
                .OrderBy(b => b.Id)
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

            var managedScopeIds = buildings
                .SelectMany(b => new[] { b.Id }.Concat((b.ChildBuildings ?? new System.Collections.Generic.List<Building>()).Select(cb => cb.Id)))
                .Distinct()
                .ToList();

            var openFaults = managedScopeIds.Any()
                ? await _context.BuildingTaskTickets
                    .AsNoTracking()
                    .Include(t => t.Building)
                    .Include(t => t.BuildingUnit)
                    .Include(t => t.AssignedStaff)
                    .Where(t => !t.IsDeleted && managedScopeIds.Contains(t.BuildingId) && t.Status != "Tamamlandi" && t.Status != "Iptal")
                    .OrderByDescending(t => t.Priority == "Acil")
                    .ThenByDescending(t => t.Id)
                    .Take(12)
                    .ToListAsync()
                : new System.Collections.Generic.List<BuildingTaskTicket>();

            var allScopedTickets = managedScopeIds.Any()
                ? await _context.SupportTickets
                    .AsNoTracking()
                    .Include(t => t.CreatorUser)
                    .Where(t => !t.IsDeleted && t.ContextType == "Building" && t.ContextId.HasValue && managedScopeIds.Contains(t.ContextId.Value))
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync()
                : new System.Collections.Generic.List<SupportTicket>();

            var residentRequests = allScopedTickets
                .Where(t => !string.Equals(t.Category, "ManagerTodo", StringComparison.OrdinalIgnoreCase)
                    && t.Status != "Closed" && t.Status != "Resolved" && t.Status != "Tamamlandı")
                .Take(12)
                .ToList();

            var managerWorkPlans = allScopedTickets
                .Where(t => string.Equals(t.Category, "ManagerTodo", StringComparison.OrdinalIgnoreCase) && t.CreatorUserId == userId)
                .OrderBy(t => t.Status == "Resolved" || t.Status == "Closed")
                .ThenByDescending(t => t.Priority == "Urgent" || t.Priority == "High")
                .ThenByDescending(t => t.Id)
                .Take(15)
                .ToList();

            var complexIds = buildings.Where(b => b.HousingComplexId.HasValue).Select(b => b.HousingComplexId!.Value).Distinct().ToList();
            var upcomingMeetings = managedScopeIds.Any()
                ? await _context.SystemMeetings
                    .AsNoTracking()
                    .Where(m => !m.IsDeleted && !m.IsConcluded && m.ContextType == "Building"
                        && (managedScopeIds.Contains(m.ContextId) || (m.IsSiteWide && m.HousingComplexId.HasValue && complexIds.Contains(m.HousingComplexId.Value))))
                    .OrderBy(m => m.MeetingDate)
                    .Take(5)
                    .ToListAsync()
                : new System.Collections.Generic.List<SystemMeeting>();

            var upcomingMaintenances = managedScopeIds.Any()
                ? await _context.SpaceFixtures
                    .AsNoTracking()
                    .Include(f => f.UnitSpace).ThenInclude(s => s.BuildingUnit).ThenInclude(u => u.Building)
                    .Where(f => !f.IsDeleted && f.NextMaintenanceDate.HasValue && managedScopeIds.Contains(f.UnitSpace.BuildingUnit.BuildingId))
                    .OrderBy(f => f.NextMaintenanceDate)
                    .Take(5)
                    .ToListAsync()
                : new System.Collections.Generic.List<SpaceFixture>();

            var rootIdsForBoard = buildings.Select(b => b.Id).Distinct().ToList();
            var expiringBoardMembers = rootIdsForBoard.Any()
                ? await _context.BuildingManagers
                    .AsNoTracking()
                    .Include(m => m.User)
                    .Include(m => m.Building)
                    .Where(m => m.IsActive && rootIdsForBoard.Contains(m.BuildingId)
                        && m.TermEndDate.HasValue && m.TermEndDate.Value.Date <= DateTime.Today.AddDays(30))
                    .OrderBy(m => m.TermEndDate)
                    .ToListAsync()
                : new System.Collections.Generic.List<BuildingManager>();

            ViewBag.TotalBuildings = buildings.Count;
            ViewBag.TotalUnits = allUnits.Count;
            ViewBag.ResidentialUnits = allUnits.Count(u => u.Category != "Dukkan");
            ViewBag.CommercialUnits = allUnits.Count(u => u.Category == "Dukkan");
            ViewBag.OccupiedUnits = allUnits.Count(u => !u.IsEmpty);
            ViewBag.EmptyUnits = allUnits.Count(u => u.IsEmpty);
            ViewBag.TotalPaid = allDebts.Where(d => d.IsPaid).Sum(d => d.Amount);
            ViewBag.TotalUnpaid = allDebts.Where(d => !d.IsPaid).Sum(d => d.Amount);
            ViewBag.TotalExpenses = buildings.SelectMany(b => b.Expenses ?? new List<BuildingExpense>()).Sum(e => e.TotalAmount);
            ViewBag.PendingTicketsCount = residentRequests.Count;
            ViewBag.OpenFaults = openFaults;
            ViewBag.ResidentRequests = residentRequests;
            ViewBag.ManagerWorkPlans = managerWorkPlans;
            ViewBag.UpcomingMeetings = upcomingMeetings;
            ViewBag.UpcomingMaintenances = upcomingMaintenances;
            ViewBag.ExpiringBoardMembers = expiringBoardMembers;

            return View(buildings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddManagerWorkPlan(
            int buildingId,
            string title,
            string? scheduleNote,
            string priority = "Normal")
        {
            var userId = GetUserId();
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null || string.IsNullOrEmpty(userId)) return Forbid();
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["ErrorMessage"] = "Lütfen iş planı başlığını giriniz.";
                return RedirectToAction(nameof(Index));
            }

            _context.SupportTickets.Add(new SupportTicket
            {
                Title = title.Trim(),
                Description = string.IsNullOrWhiteSpace(scheduleNote) ? "Yönetici iş planı notu" : scheduleNote.Trim(),
                Category = "ManagerTodo",
                Status = "Open",
                Priority = string.IsNullOrWhiteSpace(priority) ? "Normal" : priority.Trim(),
                CreatorUserId = userId,
                AssignedToUserId = userId,
                ContextType = "Building",
                ContextId = building.Id,
                ContextInfo = string.IsNullOrWhiteSpace(scheduleNote) ? DateTime.Today.ToString("dd.MM.yyyy") : scheduleNote.Trim(),
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "İş planı / hatırlatıcı eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleManagerWorkPlan(int planId, string? targetStatus = null)
        {
            var userId = GetUserId();
            var item = await _context.SupportTickets.FirstOrDefaultAsync(t =>
                t.Id == planId && t.Category == "ManagerTodo" && t.CreatorUserId == userId && !t.IsDeleted);
            if (item == null) return NotFound();

            if (targetStatus == "Delete")
            {
                item.IsDeleted = true;
            }
            else
            {
                bool isDone = item.Status == "Resolved" || item.Status == "Closed";
                item.Status = isDone ? "Open" : "Resolved";
            }
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickResolveResidentTicket(int ticketId)
        {
            var userId = GetUserId();
            var ticket = await _context.SupportTickets.FirstOrDefaultAsync(t => t.Id == ticketId && !t.IsDeleted);
            if (ticket == null) return NotFound();
            if (ticket.ContextId.HasValue)
            {
                var b = await GetManagedBuildingAsync(ticket.ContextId.Value);
                if (b == null && !IsAdminOrSuperAdmin()) return Forbid();
            }

            ticket.Status = "Resolved";
            ticket.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{ticket.Title}' talebi tamamlandı olarak işaretlendi.";
            return RedirectToAction(nameof(Index));
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
                                TermStartDate = DateTime.Today,
                                TermEndDate = DateTime.Today.AddYears(1),
                                IsVisibleToResidents = true,
                                HierarchyOrder = 1,
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

            int totalBlocks = structures.Sum(s => s.LayoutPattern == "Tekil Yapı" ? 1 : Math.Max(1, s.Children?.Count ?? 1));
            int totalUnits = structures.Sum(s => s.TotalApartments + s.TotalShops + (s.Children?.Sum(c => c.TotalApartments + c.TotalShops) ?? 0));

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
                        SignatureAuthorityType = totalUnits <= 7 ? SignatureAuthorityType.Single : SignatureAuthorityType.Joint,
                        TermStartDate = DateTime.Today,
                        TermEndDate = DateTime.Today.AddYears(1),
                        IsVisibleToResidents = true,
                        HierarchyOrder = 1,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
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
            string? tcIdentityNo = null,
            SignatureAuthorityType signatureAuthorityType = SignatureAuthorityType.None,
            bool isExternalAppointee = false,
            DateTime? termStartDate = null,
            DateTime? termEndDate = null,
            int? appointmentMeetingId = null,
            bool isVisibleToResidents = true,
            int hierarchyOrder = 0,
            string? customRoleNote = null,
            int? replaceManagerId = null,
            string? returnTo = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();
            if (!await CanEditGovernanceRecordsAsync(building))
            {
                TempData["ErrorMessage"] = "Denetçi / izleyici statüsünde kurul ataması veya yetki değişikliği yapılamaz.";
                return RedirectToAction(nameof(Governance), new { buildingId });
            }

            var cleanEmail = email?.Trim();
            var cleanPhone = phoneNumber?.Trim();
            var cleanName = fullName?.Trim();
            var rawNormTc = GMK360.Web.Helpers.TcknHelper.Normalize(tcIdentityNo);
            if (!string.IsNullOrWhiteSpace(tcIdentityNo) && rawNormTc.Length != 11)
            {
                TempData["OpenSeatOrder"] = hierarchyOrder;
                TempData["ErrorMessage"] = $"T.C. Kimlik Numarası tam 11 haneli rakam olmalıdır (girilen: {rawNormTc.Length} hane).";
                return string.Equals(returnTo, "Governance", StringComparison.OrdinalIgnoreCase)
                    ? RedirectToAction(nameof(Governance), new { buildingId })
                    : RedirectToAction(nameof(Detail), new { id = buildingId });
            }
            var cleanTc = rawNormTc.Length == 11 ? rawNormTc : null;
            if (string.IsNullOrWhiteSpace(cleanEmail) && FacilityTaskDispatchService.NormalizePhoneLast10(cleanPhone).Length < 10 && string.IsNullOrWhiteSpace(cleanTc))
            {
                TempData["OpenSeatOrder"] = hierarchyOrder;
                TempData["ErrorMessage"] = "Lütfen kurul üyesi / aza için geçerli bir e-posta, 10 haneli telefon veya 11 haneli TCKN giriniz.";
                return string.Equals(returnTo, "Governance", StringComparison.OrdinalIgnoreCase)
                    ? RedirectToAction(nameof(Governance), new { buildingId })
                    : RedirectToAction(nameof(Detail), new { id = buildingId });
            }

            if (role == BuildingManagerRole.Auditor && isExternalAppointee)
            {
                TempData["OpenSeatOrder"] = hierarchyOrder;
                TempData["ErrorMessage"] = "KMK Madde 41 gereği Denetçi / Denetim Kurulu Üyesi dışarıdan atanamaz; mutlaka tapu sahibi (kat maliki) olmalıdır.";
                return string.Equals(returnTo, "Governance", StringComparison.OrdinalIgnoreCase)
                    ? RedirectToAction(nameof(Governance), new { buildingId })
                    : RedirectToAction(nameof(Detail), new { id = buildingId });
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
                    return string.Equals(returnTo, "Governance", StringComparison.OrdinalIgnoreCase)
                        ? RedirectToAction(nameof(Governance), new { buildingId })
                        : RedirectToAction(nameof(Detail), new { id = buildingId });
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(cleanPhone) && (replaceManagerId.HasValue || string.IsNullOrWhiteSpace(targetUser.PhoneNumber)))
                {
                    targetUser.PhoneNumber = cleanPhone;
                    _context.Entry(targetUser).Property(u => u.PhoneNumber).IsModified = true;
                }
                if (!string.IsNullOrWhiteSpace(cleanTc))
                {
                    bool tcConflict = await _context.Users.AnyAsync(u => u.Id != targetUser.Id && u.TcIdentityNo == cleanTc);
                    if (tcConflict)
                    {
                        TempData["OpenSeatOrder"] = hierarchyOrder;
                        TempData["ErrorMessage"] = "Bu T.C. Kimlik Numarası sistemde başka bir hesaba tanımlıdır.";
                        return RedirectToAction(nameof(Governance), new { buildingId });
                    }
                    if (replaceManagerId.HasValue || string.IsNullOrWhiteSpace(targetUser.TcIdentityNo) || string.Equals(targetUser.TcIdentityNo.Trim(), cleanTc, StringComparison.Ordinal))
                    {
                        targetUser.TcIdentityNo = cleanTc;
                        _context.Entry(targetUser).Property(u => u.TcIdentityNo).IsModified = true;
                    }
                    else
                    {
                        TempData["OpenSeatOrder"] = hierarchyOrder;
                        TempData["ErrorMessage"] = "Girilen T.C. Kimlik Numarası bu kullanıcının kayıtlı TCKN bilgisi ile uyuşmuyor. Lütfen kontrol ediniz.";
                        return RedirectToAction(nameof(Governance), new { buildingId });
                    }
                }
            }

            // KMK Madde 41: Denetçi mutlaka bu bina/sitede tapu sahibi (kat maliki) olmalıdır
            if (role == BuildingManagerRole.Auditor)
            {
                var complexId = building.HousingComplexId;
                var scopedUnitOwners = await _context.BuildingUnits
                    .AsNoTracking()
                    .Where(u => !(u.Category ?? "").StartsWith("OrtakAlan")
                        && (u.BuildingId == rootId || u.Building.ParentBuildingId == rootId || (complexId.HasValue && u.Building.HousingComplexId == complexId)))
                    .Select(u => new { u.OwnerUserId, u.OwnerEmail, u.OwnerPhone })
                    .ToListAsync();
                var normTargetPhone = FacilityTaskDispatchService.NormalizePhoneLast10(targetUser.PhoneNumber ?? cleanPhone);
                bool isOwnerInScope = scopedUnitOwners.Any(u =>
                    (!string.IsNullOrEmpty(u.OwnerUserId) && u.OwnerUserId == targetUser.Id)
                    || (!string.IsNullOrWhiteSpace(u.OwnerEmail) && !string.IsNullOrWhiteSpace(targetUser.Email) && string.Equals(u.OwnerEmail.Trim(), targetUser.Email.Trim(), StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrWhiteSpace(normTargetPhone) && normTargetPhone.Length >= 10 && FacilityTaskDispatchService.NormalizePhoneLast10(u.OwnerPhone) == normTargetPhone));
                if (!isOwnerInScope)
                {
                    TempData["ErrorMessage"] = "KMK Madde 41 gereği Denetçi mutlaka bu bina/sitede tapu sahibi (kat maliki) olmalıdır. Daire maliki kayıtlarında eşleşme bulunamadı.";
                    return string.Equals(returnTo, "Governance", StringComparison.OrdinalIgnoreCase)
                        ? RedirectToAction(nameof(Governance), new { buildingId })
                        : RedirectToAction(nameof(Detail), new { id = buildingId });
                }
            }

            var resolvedStart = termStartDate ?? DateTime.Today;
            var resolvedEnd = termEndDate ?? resolvedStart.AddYears(1);

            if (replaceManagerId.HasValue)
            {
                var prevHolder = await _context.BuildingManagers
                    .FirstOrDefaultAsync(m => m.Id == replaceManagerId.Value && m.BuildingId == rootId && m.IsActive && m.UserId != targetUser.Id);
                if (prevHolder != null)
                {
                    prevHolder.IsActive = false;
                    prevHolder.UpdatedAt = DateTime.UtcNow;
                }
            }

            if (role == BuildingManagerRole.HeadManager)
            {
                var otherHeads = await _context.BuildingManagers
                    .Where(m => m.BuildingId == rootId && m.IsActive && m.Role == BuildingManagerRole.HeadManager && m.UserId != targetUser.Id)
                    .ToListAsync();
                foreach (var oh in otherHeads)
                {
                    oh.IsActive = false;
                    oh.UpdatedAt = DateTime.UtcNow;
                }
                var scopedBlds = await _context.Buildings
                    .Where(b => b.Id == rootId || b.ParentBuildingId == rootId || (building.HousingComplexId.HasValue && b.HousingComplexId == building.HousingComplexId))
                    .ToListAsync();
                foreach (var sb in scopedBlds)
                {
                    sb.ManagerUserId = targetUser.Id;
                }
            }

            var existing = await _context.BuildingManagers
                .FirstOrDefaultAsync(m => m.BuildingId == rootId && m.UserId == targetUser.Id && m.IsActive);
            if (existing != null)
            {
                existing.Role = role;
                existing.SignatureAuthorityType = signatureAuthorityType;
                existing.IsExternalAppointee = isExternalAppointee;
                existing.TermStartDate = resolvedStart;
                existing.TermEndDate = resolvedEnd;
                existing.AppointmentMeetingId = appointmentMeetingId;
                existing.IsVisibleToResidents = isVisibleToResidents;
                existing.HierarchyOrder = hierarchyOrder;
                existing.CustomRoleNote = string.IsNullOrWhiteSpace(customRoleNote) ? null : customRoleNote.Trim();
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
                    SignatureAuthorityType = signatureAuthorityType,
                    IsExternalAppointee = isExternalAppointee,
                    TermStartDate = resolvedStart,
                    TermEndDate = resolvedEnd,
                    AppointmentMeetingId = appointmentMeetingId,
                    IsVisibleToResidents = isVisibleToResidents,
                    HierarchyOrder = hierarchyOrder,
                    CustomRoleNote = string.IsNullOrWhiteSpace(customRoleNote) ? null : customRoleNote.Trim(),
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
                NewValues = System.Text.Json.JsonSerializer.Serialize(new { TargetUserId = targetUser.Id, targetUser.Email, Role = role.ToString(), SignatureAuthorityType = signatureAuthorityType.ToString(), IsExternalAppointee = isExternalAppointee }),
                AffectedColumns = "BuildingManagers",
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var roleText = BuildingManager.GetRoleLabel(role);
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

            if (string.Equals(returnTo, "Governance", StringComparison.OrdinalIgnoreCase))
            {
                if (hierarchyOrder > 0) TempData["OpenSeatOrder"] = hierarchyOrder;
                return RedirectToAction(nameof(Governance), new { buildingId });
            }
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveGovernanceMember(int buildingId, int managerId, string? returnTo = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();
            if (!await CanEditGovernanceRecordsAsync(building))
            {
                TempData["ErrorMessage"] = "Denetçi / izleyici statüsünde kurul üyesi çıkarılamaz.";
                return RedirectToAction(nameof(Governance), new { buildingId });
            }

            int rootId = building.ParentBuildingId ?? building.Id;
            var member = await _context.BuildingManagers
                .FirstOrDefaultAsync(m => m.Id == managerId && m.BuildingId == rootId && m.IsActive);

            if (member == null)
            {
                TempData["ErrorMessage"] = "Kurul üyesi kaydı bulunamadı.";
                return string.Equals(returnTo, "Governance", StringComparison.OrdinalIgnoreCase)
                    ? RedirectToAction(nameof(Governance), new { buildingId })
                    : RedirectToAction(nameof(Detail), new { id = buildingId });
            }

            if (member.Role == BuildingManagerRole.HeadManager)
            {
                int headCount = await _context.BuildingManagers.CountAsync(m => m.BuildingId == rootId && m.IsActive && m.Role == BuildingManagerRole.HeadManager);
                if (headCount <= 1)
                {
                    TempData["ErrorMessage"] = "Son aktif Ana Yönetici çıkarılamaz; bunun için Yetki Devri veya Yönetici Atama Kararı kullanın.";
                    return string.Equals(returnTo, "Governance", StringComparison.OrdinalIgnoreCase)
                        ? RedirectToAction(nameof(Governance), new { buildingId })
                        : RedirectToAction(nameof(Detail), new { id = buildingId });
                }
            }

            member.IsActive = false;
            member.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kurul / aza üyesi pasife alındı (denetim geçmişi korundu).";
            if (string.Equals(returnTo, "Governance", StringComparison.OrdinalIgnoreCase))
            {
                if (member.HierarchyOrder > 0) TempData["OpenSeatOrder"] = member.HierarchyOrder;
                return RedirectToAction(nameof(Governance), new { buildingId });
            }
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetSiteIdentity(int buildingId, string? subdomain, bool isIndependentSite = true, string? returnTo = null)
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
                    return string.Equals(returnTo, "Governance", StringComparison.OrdinalIgnoreCase)
                        ? RedirectToAction(nameof(Governance), new { buildingId })
                        : RedirectToAction(nameof(Detail), new { id = buildingId });
                }

                bool taken = await _context.HousingComplexes.AnyAsync(c => c.Subdomain == cleanSub && (!building.HousingComplexId.HasValue || c.Id != building.HousingComplexId.Value))
                    || await _context.Buildings.AnyAsync(b => b.Subdomain == cleanSub && b.Id != building.Id && (!building.HousingComplexId.HasValue || b.HousingComplexId != building.HousingComplexId.Value))
                    || await _context.Agencies.AnyAsync(a => a.Subdomain == cleanSub)
                    || await _context.ManagementCompanies.AnyAsync(m => m.Subdomain == cleanSub);

                if (taken)
                {
                    TempData["ErrorMessage"] = $"'{cleanSub}.gmk360.com' adresi başka bir kurum/site tarafından kullanılıyor.";
                    return string.Equals(returnTo, "Governance", StringComparison.OrdinalIgnoreCase)
                        ? RedirectToAction(nameof(Governance), new { buildingId })
                        : RedirectToAction(nameof(Detail), new { id = buildingId });
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
            return string.Equals(returnTo, "Governance", StringComparison.OrdinalIgnoreCase)
                ? RedirectToAction(nameof(Governance), new { buildingId })
                : RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferHeadManager(int buildingId, string newManagerEmail, string? returnTo = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();
            if (!await CanEditGovernanceRecordsAsync(building))
            {
                TempData["ErrorMessage"] = "Denetçi statüsünde ana yönetici devri yapılamaz.";
                return RedirectToAction(nameof(Governance), new { buildingId });
            }

            int rootId = building.ParentBuildingId ?? building.Id;
            var newUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == newManagerEmail.Trim() || u.UserName == newManagerEmail.Trim());
            if (newUser == null)
            {
                TempData["ErrorMessage"] = $"'{newManagerEmail}' ile kayıtlı kullanıcı bulunamadı. Yeni yöneticinin önce GMK360'a kayıtlı olması gerekir.";
                return string.Equals(returnTo, "Governance", StringComparison.OrdinalIgnoreCase)
                    ? RedirectToAction(nameof(Governance), new { buildingId })
                    : RedirectToAction(nameof(Detail), new { id = buildingId });
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
            return string.Equals(returnTo, "Governance", StringComparison.OrdinalIgnoreCase)
                ? RedirectToAction(nameof(Governance), new { buildingId })
                : RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBoardMemberMeta(
            int buildingId,
            int managerId,
            BuildingManagerRole role,
            SignatureAuthorityType signatureAuthorityType,
            DateTime? termStartDate = null,
            DateTime? termEndDate = null,
            int? appointmentMeetingId = null,
            bool isExternalAppointee = false,
            bool isVisibleToResidents = true,
            int hierarchyOrder = 0,
            string? tcIdentityNo = null,
            string? phoneNumber = null,
            string? customRoleNote = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();
            if (!await CanEditGovernanceRecordsAsync(building))
            {
                TempData["ErrorMessage"] = "Denetçi statüsünde yönetim kurulu kaydı güncellenemez.";
                return RedirectToAction(nameof(Governance), new { buildingId });
            }
            int rootId = building.ParentBuildingId ?? building.Id;

            var member = await _context.BuildingManagers
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == managerId && m.BuildingId == rootId && m.IsActive);
            if (member == null) return NotFound();

            if (role == BuildingManagerRole.Auditor && isExternalAppointee)
            {
                TempData["OpenSeatOrder"] = hierarchyOrder;
                TempData["ErrorMessage"] = "KMK Madde 41 gereği Denetçi / Denetim Kurulu Üyesi dışarıdan atanamaz; mutlaka kat maliki olmalıdır.";
                return RedirectToAction(nameof(Governance), new { buildingId });
            }

            if (!string.IsNullOrWhiteSpace(phoneNumber) && member.User != null)
            {
                member.User.PhoneNumber = phoneNumber.Trim();
                _context.Entry(member.User).Property(u => u.PhoneNumber).IsModified = true;
            }

            member.Role = role;
            member.SignatureAuthorityType = signatureAuthorityType;
            member.IsExternalAppointee = isExternalAppointee;
            member.TermStartDate = termStartDate;
            member.TermEndDate = termEndDate ?? (termStartDate.HasValue ? termStartDate.Value.AddYears(1) : null);
            member.AppointmentMeetingId = appointmentMeetingId;
            member.IsVisibleToResidents = isVisibleToResidents;
            member.HierarchyOrder = hierarchyOrder;
            member.CustomRoleNote = string.IsNullOrWhiteSpace(customRoleNote) ? null : customRoleNote.Trim();
            member.UpdatedAt = DateTime.UtcNow;

            string? tcNotice = null;
            string? tcError = null;
            if (!string.IsNullOrWhiteSpace(tcIdentityNo))
            {
                var normTc = GMK360.Web.Helpers.TcknHelper.Normalize(tcIdentityNo);
                if (normTc.Length != 11)
                {
                    tcError = $"Diğer bilgiler kaydedildi; ancak '{tcIdentityNo}' {normTc.Length} haneli olduğu için TCKN kaydedilmedi (tam 11 haneli rakam olmalıdır).";
                }
                else
                {
                    var conflictUser = await _context.Users.AsNoTracking()
                        .Where(u => u.Id != member.UserId && u.TcIdentityNo == normTc)
                        .Select(u => new { u.Email, u.FirstName, u.LastName })
                        .FirstOrDefaultAsync();
                    if (conflictUser != null)
                    {
                        tcError = $"Diğer bilgiler kaydedildi; ancak {normTc} TCKN başka bir hesapta ({conflictUser.Email}) zaten kayıtlıdır.";
                    }
                    else if (member.User != null)
                    {
                        member.User.TcIdentityNo = normTc;
                        _context.Entry(member.User).Property(u => u.TcIdentityNo).IsModified = true;
                        if (!GMK360.Web.Helpers.TcknHelper.IsValidTcIdentity(normTc))
                        {
                            tcNotice = "Kurul bilgileri ve 11 haneli TCKN kaydedildi (Not: Girilen numara MERNIS son 2 hane kontrolünden farklıdır; deneme numarası olarak kaydedildi).";
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["OpenSeatOrder"] = hierarchyOrder;
            if (!string.IsNullOrWhiteSpace(tcError))
            {
                TempData["ErrorMessage"] = tcError;
            }
            else if (!string.IsNullOrWhiteSpace(tcNotice))
            {
                TempData["SuccessMessage"] = tcNotice;
            }
            else
            {
                TempData["SuccessMessage"] = "Kurul üyesi TCKN, yetki ve dönem bilgileri güncellendi.";
            }
            return RedirectToAction(nameof(Governance), new { buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetCustomBoardCounts(int buildingId, int? asilCount, int? yedekCount, int? denetciCount, bool resetToAuto = false)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();
            if (!await CanEditGovernanceRecordsAsync(building))
            {
                TempData["ErrorMessage"] = "Denetçi statüsünde kurul sayısı değiştirilemez.";
                return RedirectToAction(nameof(Governance), new { buildingId });
            }
            int rootId = building.ParentBuildingId ?? building.Id;
            var rootBuilding = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == rootId);
            if (rootBuilding == null) return NotFound();

            if (resetToAuto)
            {
                rootBuilding.BoardTargetAsilCount = null;
                rootBuilding.BoardTargetYedekCount = null;
                rootBuilding.BoardTargetDenetciCount = null;
                TempData["SuccessMessage"] = "Yönetim kurulu üye sayıları bağımsız bölüm sayısına göre KMK otomatik önerisine sıfırlandı.";
            }
            else
            {
                rootBuilding.BoardTargetAsilCount = asilCount.HasValue ? Math.Max(1, Math.Min(15, asilCount.Value)) : (int?)null;
                rootBuilding.BoardTargetYedekCount = yedekCount.HasValue ? Math.Max(0, Math.Min(15, yedekCount.Value)) : (int?)null;
                rootBuilding.BoardTargetDenetciCount = denetciCount.HasValue ? Math.Max(0, Math.Min(9, denetciCount.Value)) : (int?)null;
                TempData["SuccessMessage"] = "Yönetim Planı özel kurul sayıları güncellendi.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Governance), new { buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteReserveToActive(int buildingId, int managerId)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();
            if (!await CanEditGovernanceRecordsAsync(building))
            {
                TempData["ErrorMessage"] = "Denetçi statüsünde yedek üye terfisi yapılamaz.";
                return RedirectToAction(nameof(Governance), new { buildingId });
            }
            int rootId = building.ParentBuildingId ?? building.Id;

            var reserve = await _context.BuildingManagers
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == managerId && m.BuildingId == rootId && m.IsActive && m.Role == BuildingManagerRole.ReserveMember);
            if (reserve == null)
            {
                TempData["ErrorMessage"] = "Yükseltilecek aktif yedek üye bulunamadı.";
                return RedirectToAction(nameof(Governance), new { buildingId });
            }

            reserve.Role = BuildingManagerRole.BoardMember;
            reserve.HierarchyOrder = 1;
            reserve.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var displayName = !string.IsNullOrWhiteSpace(reserve.User?.FirstName)
                ? $"{reserve.User.FirstName} {reserve.User.LastName}".Trim()
                : (reserve.User?.Email ?? "Yedek Üye");
            TempData["SuccessMessage"] = $"{displayName} yedek sıradan Asil Yönetim Kurulu Üyesi (Seviye 3) kadrosuna yükseltildi.";
            return RedirectToAction(nameof(Governance), new { buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleManagerResidentVisibility(int buildingId, int managerId)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();
            int rootId = building.ParentBuildingId ?? building.Id;

            var member = await _context.BuildingManagers
                .FirstOrDefaultAsync(m => m.Id == managerId && m.BuildingId == rootId && m.IsActive);
            if (member == null) return NotFound();

            member.IsVisibleToResidents = !member.IsVisibleToResidents;
            member.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = member.IsVisibleToResidents
                ? "Kurul üyesi iletişim bilgisi Daire Sakinleri panelinde görünür yapıldı."
                : "Kurul üyesi iletişim bilgisi Daire Sakinleri panelinden gizlendi.";
            return RedirectToAction(nameof(Governance), new { buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadSignatureCircular(int buildingId, Microsoft.AspNetCore.Http.IFormFile? circularFile, string? docTitle = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();
            if (!await CanEditGovernanceRecordsAsync(building))
            {
                TempData["ErrorMessage"] = "Denetçi statüsünde imza sirküleri yüklenemez.";
                return RedirectToAction(nameof(Governance), new { buildingId });
            }
            int rootId = building.ParentBuildingId ?? building.Id;
            var rootBuilding = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == rootId);
            if (rootBuilding == null) return NotFound();

            if (circularFile == null || circularFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen noter onaylı imza sirküleri PDF veya görsel dosyasını seçiniz.";
                return RedirectToAction(nameof(Governance), new { buildingId });
            }

            var ext = System.IO.Path.GetExtension(circularFile.FileName)?.ToLowerInvariant();
            var allowedExts = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };
            if (string.IsNullOrEmpty(ext) || !allowedExts.Contains(ext))
            {
                TempData["ErrorMessage"] = "Yalnızca PDF veya görsel (JPG, PNG, WEBP) dosyaları yüklenebilir.";
                return RedirectToAction(nameof(Governance), new { buildingId });
            }

            var env = HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.Hosting.IWebHostEnvironment)) as Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
            var webRoot = env?.WebRootPath ?? System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
            var folder = System.IO.Path.Combine(webRoot, "uploads", "buildings", rootId.ToString(), "governance");
            System.IO.Directory.CreateDirectory(folder);

            var uniqueName = $"sirkuler_{Guid.NewGuid():N}{ext}";
            var fullPath = System.IO.Path.Combine(folder, uniqueName);
            using (var stream = new System.IO.FileStream(fullPath, System.IO.FileMode.Create))
            {
                await circularFile.CopyToAsync(stream);
            }

            var docUrl = $"/uploads/buildings/{rootId}/governance/{uniqueName}";
            rootBuilding.SignatureCircularUrl = docUrl;

            _context.DocumentArchives.Add(new DocumentArchive
            {
                BuildingId = rootId,
                Title = string.IsNullOrWhiteSpace(docTitle) ? $"Noter Onaylı İmza Sirküleri ({rootBuilding.Name})" : docTitle.Trim(),
                FileName = circularFile.FileName,
                FileExtension = ext,
                Category = "ImzaSirkuleri",
                SourceModule = "BuildingManager",
                SourceRecordId = rootId,
                DocumentUrl = docUrl,
                Status = "Aktif",
                UploadDate = DateTime.Now,
                Year = DateTime.Today.Year,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Noter onaylı imza sirküleri / banka yetki belgesi arşivlendi.";
            return RedirectToAction(nameof(Governance), new { buildingId });
        }

        [HttpGet]
        public async Task<IActionResult> Governance(int? buildingId)
        {
            var buildings = await GetUserManagedRootBuildingsAsync();
            Building? selected = null;
            if (buildingId.HasValue)
            {
                selected = await GetManagedBuildingAsync(buildingId.Value);
                if (selected == null) return Forbid();
            }
            selected ??= buildings.FirstOrDefault();

            int? rootId = selected != null ? (selected.ParentBuildingId ?? selected.Id) : (int?)null;
            var managers = rootId.HasValue
                ? await _context.BuildingManagers
                    .Include(m => m.User)
                    .Where(m => m.BuildingId == rootId.Value)
                    .OrderByDescending(m => m.IsActive)
                    .ThenBy(m => m.Role)
                    .ThenBy(m => m.HierarchyOrder)
                    .ThenByDescending(m => m.CreatedAt)
                    .ToListAsync()
                : new System.Collections.Generic.List<BuildingManager>();

            HousingComplex? complex = null;
            var siteBlocks = new System.Collections.Generic.List<Building>();
            if (selected?.HousingComplexId != null)
            {
                complex = await _context.HousingComplexes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == selected.HousingComplexId.Value);
                siteBlocks = await _context.Buildings
                    .AsNoTracking()
                    .Include(b => b.Units).ThenInclude(u => u.OwnerUser)
                    .Include(b => b.Units).ThenInclude(u => u.TenantUser)
                    .Where(b => b.HousingComplexId == selected.HousingComplexId)
                    .OrderBy(b => b.Id)
                    .ToListAsync();
            }
            else if (selected != null)
            {
                var loadedSelected = await _context.Buildings
                    .AsNoTracking()
                    .Include(b => b.Units).ThenInclude(u => u.OwnerUser)
                    .Include(b => b.Units).ThenInclude(u => u.TenantUser)
                    .Include(b => b.ChildBuildings).ThenInclude(cb => cb.Units).ThenInclude(u => u.OwnerUser)
                    .Include(b => b.ChildBuildings).ThenInclude(cb => cb.Units).ThenInclude(u => u.TenantUser)
                    .FirstOrDefaultAsync(b => b.Id == (selected.ParentBuildingId ?? selected.Id));
                if (loadedSelected != null)
                {
                    siteBlocks.Add(loadedSelected);
                    if (loadedSelected.ChildBuildings != null && loadedSelected.ChildBuildings.Any())
                    {
                        siteBlocks.AddRange(loadedSelected.ChildBuildings.OrderBy(cb => cb.Id));
                    }
                }
                else
                {
                    siteBlocks.Add(selected);
                }
            }

            var govDecisions = selected != null
                ? await _context.SystemMeetings
                    .AsNoTracking()
                    .Include(m => m.Votes)
                    .Include(m => m.Decisions)
                    .Where(m => m.ContextType == "Building" && !m.IsDeleted
                        && (m.ContextId == selected.Id || (selected.HousingComplexId.HasValue && m.IsSiteWide && m.HousingComplexId == selected.HousingComplexId)))
                    .OrderByDescending(m => m.MeetingDate)
                    .Take(20)
                    .ToListAsync()
                : new System.Collections.Generic.List<SystemMeeting>();

            var govDecisionIds = govDecisions.Select(d => d.Id).ToList();
            var decisionScans = govDecisionIds.Any()
                ? await _context.DocumentArchives
                    .AsNoTracking()
                    .Where(d => !d.IsDeleted && d.Category == "KararDefteri" && d.SourceRecordId.HasValue && govDecisionIds.Contains(d.SourceRecordId.Value))
                    .OrderByDescending(d => d.Id)
                    .ToListAsync()
                : new System.Collections.Generic.List<DocumentArchive>();

            var circularDocs = rootId.HasValue
                ? await _context.DocumentArchives
                    .AsNoTracking()
                    .Where(d => !d.IsDeleted && d.BuildingId == rootId.Value && d.Category == "ImzaSirkuleri")
                    .OrderByDescending(d => d.Id)
                    .Take(5)
                    .ToListAsync()
                : new System.Collections.Generic.List<DocumentArchive>();

            var scopedUnits = siteBlocks
                .SelectMany(b => b.Units ?? new System.Collections.Generic.List<BuildingUnit>())
                .Where(u => !(u.Category ?? "").StartsWith("OrtakAlan"))
                .OrderBy(u => u.Building?.BlockName ?? u.Building?.Name)
                .ThenBy(u => u.FloorLevel)
                .ThenBy(u => u.DoorNumber)
                .ToList();

            var boardPlan = GMK360.Web.Helpers.KmkBoardRecommendationHelper.BuildPlan(scopedUnits.Count, selected, managers);

            ViewBag.Buildings = buildings;
            ViewBag.SelectedBuilding = selected;
            ViewBag.RootBuildingId = rootId;
            ViewBag.Complex = complex;
            ViewBag.SiteBlocks = siteBlocks;
            ViewBag.TotalScopedUnits = scopedUnits.Count;
            ViewBag.SiteOwnerUnits = scopedUnits.Where(u => !string.IsNullOrWhiteSpace(u.OwnerName) || !string.IsNullOrWhiteSpace(u.TenantName)).ToList();
            ViewBag.Managers = managers;
            ViewBag.BoardPlan = boardPlan;
            ViewBag.RecentGovernanceDecisions = govDecisions;
            ViewBag.DecisionScansByMeeting = decisionScans.GroupBy(d => d.SourceRecordId!.Value).ToDictionary(g => g.Key, g => g.First());
            ViewBag.CircularDocs = circularDocs;
            ViewBag.CanEditGovernance = await CanEditGovernanceRecordsAsync(selected);
            return View();
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
            string? meetingTime = null,
            string? decisionNumber = null,
            string? meetingLocation = null,
            string? quorumType = "Standard",
            Microsoft.AspNetCore.Http.IFormFile? attachmentFile = null,
            string? attachmentTitle = null,
            bool isSiteWide = true,
            string? decisionCategory = "General",
            string? proposedDues = null,
            int? paymentDay = null,
            string? targetName = null,
            string? targetEmail = null,
            string? targetPhone = null,
            string? targetRole = null,
            string? additionalCandidates = null,
            int? targetUnitId = null,
            string? exemptionMode = null,
            string? proposedMultiplier = null,
            string? proposedAdjustment = null,
            string? duesNote = null,
            int? relatedTicketId = null,
            string? estimatedBudget = null,
            bool autoExecuteOnQuorum = true,
            int? bookPageNumber = null,
            DateTime? sourceDocDate = null,
            string? sourceDocNumber = null,
            string? sourceDocSender = null,
            string? chairpersonName = null,
            string? boardMembersSummary = null,
            string? meetingEndTime = null)
        {
            var userId = GetUserId();
            var building = await _context.Buildings.Include(b => b.HousingComplex).FirstOrDefaultAsync(b => b.Id == buildingId && (b.ManagerUserId == userId || b.CreatedByUserId == userId || User.IsInRole("Admin") || User.IsInRole("SuperAdmin") || _context.BuildingManagers.Any(m => m.BuildingId == (b.ParentBuildingId ?? b.Id) && m.UserId == userId && m.IsActive)));
            if (building == null) return Unauthorized();

            DateTime effectiveDate = meetingDate;
            if (!string.IsNullOrWhiteSpace(meetingTime) && TimeSpan.TryParse(meetingTime, out var parsedTime))
            {
                effectiveDate = meetingDate.Date.Add(parsedTime);
            }

            string effectiveDecNo = !string.IsNullOrWhiteSpace(decisionNumber)
                ? decisionNumber.Trim()
                : $"{effectiveDate.Year}/{(await _context.SystemMeetings.CountAsync(m => m.ContextType == "Building" && m.ContextId == buildingId && m.MeetingDate.Year == effectiveDate.Year && !m.IsDeleted) + 1):D2}";

            string cleanCategory = string.IsNullOrWhiteSpace(decisionCategory) ? "General" : decisionCategory.Trim();
            string cleanQuorum = string.IsNullOrWhiteSpace(quorumType) ? "Standard" : quorumType.Trim();
            if (string.Equals(cleanCategory, "BoardDecision", StringComparison.OrdinalIgnoreCase))
            {
                cleanQuorum = "BoardMajority";
            }
            else if (string.Equals(cleanCategory, "ManagementPlan", StringComparison.OrdinalIgnoreCase))
            {
                cleanQuorum = "ManagementPlan";
            }
            else if (string.Equals(cleanCategory, "SecondAssembly", StringComparison.OrdinalIgnoreCase))
            {
                cleanQuorum = "SecondAssembly";
            }
            else if (string.Equals(cleanCategory, "UnitExemption", StringComparison.OrdinalIgnoreCase) && string.Equals(cleanQuorum, "Standard", StringComparison.OrdinalIgnoreCase))
            {
                cleanQuorum = "Unanimous";
            }
            else if (string.Equals(cleanCategory, "AppointManager", StringComparison.OrdinalIgnoreCase)
                || string.Equals(cleanCategory, "AppointBoardMember", StringComparison.OrdinalIgnoreCase)
                || string.Equals(cleanCategory, "CapitalProject", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(cleanQuorum, "Standard", StringComparison.OrdinalIgnoreCase))
                {
                    cleanQuorum = "Innovation";
                }
            }

            decimal? parsedDues = ParseFlexibleDecimal(proposedDues);
            decimal? parsedBudget = ParseFlexibleDecimal(estimatedBudget);
            decimal? parsedMult = ParseFlexibleDecimal(proposedMultiplier);
            decimal? parsedAdj = ParseFlexibleDecimal(proposedAdjustment);
            var proposalPayload = new DecisionProposalPayload
            {
                ProposedDues = parsedDues.HasValue && parsedDues.Value >= 0m ? Math.Round(parsedDues.Value, 2) : null,
                PaymentDay = paymentDay.HasValue ? Math.Clamp(paymentDay.Value, 1, 31) : null,
                TargetName = string.IsNullOrWhiteSpace(targetName) ? null : targetName.Trim(),
                TargetEmail = string.IsNullOrWhiteSpace(targetEmail) ? null : targetEmail.Trim(),
                TargetPhone = string.IsNullOrWhiteSpace(targetPhone) ? null : targetPhone.Trim(),
                TargetRole = string.IsNullOrWhiteSpace(targetRole) ? null : targetRole.Trim(),
                AdditionalCandidates = string.IsNullOrWhiteSpace(additionalCandidates) ? null : additionalCandidates.Trim(),
                TargetUnitId = targetUnitId,
                ExemptionMode = string.IsNullOrWhiteSpace(exemptionMode) ? null : exemptionMode.Trim(),
                ProposedMultiplier = parsedMult.HasValue && parsedMult.Value > 0m ? Math.Round(parsedMult.Value, 2) : null,
                ProposedAdjustment = parsedAdj.HasValue ? Math.Round(parsedAdj.Value, 2) : null,
                DuesNote = string.IsNullOrWhiteSpace(duesNote) ? null : duesNote.Trim(),
                RelatedTicketId = relatedTicketId,
                EstimatedBudget = parsedBudget.HasValue && parsedBudget.Value > 0m ? Math.Round(parsedBudget.Value, 2) : null
            };
            bool hasStructuredPayload = proposalPayload.ProposedDues.HasValue
                || !string.IsNullOrWhiteSpace(proposalPayload.TargetName)
                || !string.IsNullOrWhiteSpace(proposalPayload.TargetEmail)
                || !string.IsNullOrWhiteSpace(proposalPayload.TargetPhone)
                || proposalPayload.TargetUnitId.HasValue
                || proposalPayload.RelatedTicketId.HasValue
                || proposalPayload.EstimatedBudget.HasValue;

            int rootBldIdForMgr = building.ParentBuildingId ?? building.Id;
            var activeMgrs = await _context.BuildingManagers
                .AsNoTracking()
                .Include(m => m.User)
                .Where(m => m.BuildingId == rootBldIdForMgr && m.IsActive)
                .OrderBy(m => m.Role)
                .ToListAsync();
            string? defaultChair = activeMgrs
                .Where(m => m.Role == BuildingManagerRole.HeadManager)
                .Select(m => !string.IsNullOrWhiteSpace(m.User?.FirstName) ? $"{m.User.FirstName} {m.User.LastName}".Trim() : (m.User?.DisplayName ?? m.User?.Email))
                .FirstOrDefault();
            string? defaultAzas = string.Join(", ", activeMgrs
                .Where(m => m.Role == BuildingManagerRole.BoardMember || m.Role == BuildingManagerRole.Auditor)
                .Select(m => !string.IsNullOrWhiteSpace(m.User?.FirstName) ? $"{m.User.FirstName} {m.User.LastName}".Trim() : (m.User?.DisplayName ?? m.User?.Email))
                .Where(n => !string.IsNullOrWhiteSpace(n)));
            int computedNextPage = bookPageNumber
                ?? ((await _context.SystemMeetings
                    .Where(m => m.ContextType == "Building" && m.ContextId == buildingId && !m.IsDeleted && m.BookPageNumber.HasValue)
                    .MaxAsync(m => (int?)m.BookPageNumber) ?? 0) + 1);

            var meeting = new SystemMeeting
            {
                ContextType = "Building",
                ContextId = buildingId,
                IsSiteWide = isSiteWide && building.HousingComplexId.HasValue,
                HousingComplexId = building.HousingComplexId,
                AgendaTitle = agendaTitle,
                DecisionNumber = effectiveDecNo,
                BookPageNumber = computedNextPage,
                SourceDocDate = sourceDocDate,
                SourceDocNumber = string.IsNullOrWhiteSpace(sourceDocNumber) ? null : sourceDocNumber.Trim(),
                SourceDocSender = string.IsNullOrWhiteSpace(sourceDocSender) ? null : sourceDocSender.Trim(),
                ChairpersonName = string.IsNullOrWhiteSpace(chairpersonName) ? defaultChair : chairpersonName.Trim(),
                BoardMembersSummary = string.IsNullOrWhiteSpace(boardMembersSummary) ? (string.IsNullOrWhiteSpace(defaultAzas) ? null : defaultAzas) : boardMembersSummary.Trim(),
                MeetingEndTime = string.IsNullOrWhiteSpace(meetingEndTime) ? null : meetingEndTime.Trim(),
                MeetingLocation = string.IsNullOrWhiteSpace(meetingLocation) ? "Site Yönetim Odası / Ortak Alan" : meetingLocation.Trim(),
                QuorumType = cleanQuorum,
                InvitationScope = string.Equals(cleanQuorum, "BoardMajority", StringComparison.OrdinalIgnoreCase) ? "BoardOnly" : "OwnersAndProxyTenants",
                DecisionCategory = cleanCategory,
                ProposedPayloadJson = hasStructuredPayload ? System.Text.Json.JsonSerializer.Serialize(proposalPayload) : null,
                AutoExecuteOnQuorum = autoExecuteOnQuorum,
                VerificationToken = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant(),
                MeetingDate = effectiveDate,
                IsConcluded = !isPlannedOnly,
                CreatedAt = DateTime.Now
            };

            _context.SystemMeetings.Add(meeting);
            await _context.SaveChangesAsync();

            var finalDecisionText = decisionText?.Trim();
            if (string.IsNullOrWhiteSpace(finalDecisionText))
            {
                string candInfo = !string.IsNullOrWhiteSpace(proposalPayload.AdditionalCandidates)
                    ? $" (Diğer adaylar: {proposalPayload.AdditionalCandidates})"
                    : "";
                string exInfo = proposalPayload.ExemptionMode switch
                {
                    "FullExempt" => " ve görev süresince dairesinin aidattan %100 muaf tutulmasına",
                    "HalfExempt" => " ve görev süresince dairesine %50 aidat indirimi uygulanmasına",
                    _ => ""
                };
                finalDecisionText = cleanCategory switch
                {
                    "DuesUpdate" when proposalPayload.ProposedDues.HasValue =>
                        $"Kat Mülkiyeti Kanunu Md. 37 uyarınca aylık baz aidatın {proposalPayload.ProposedDues.Value:N2} ₺ olarak belirlenmesine ve her ayın {(proposalPayload.PaymentDay ?? building.DuesPaymentDay ?? 5)}. günü tahsil edilmesine karar verilmesi oylamaya sunulmuştur.",
                    "AppointManager" when !string.IsNullOrWhiteSpace(proposalPayload.TargetName) =>
                        $"KMK Md. 34/40 gereğince {proposalPayload.TargetName} ({proposalPayload.TargetEmail ?? proposalPayload.TargetPhone ?? "yetkili"}) adlı kişinin Ana Yönetici olarak atanmasına{exInfo} ilişkin karar önerisidir.{candInfo}",
                    "AppointBoardMember" when !string.IsNullOrWhiteSpace(proposalPayload.TargetName) =>
                        $"KMK Md. 34/40/41 uyarınca {proposalPayload.TargetName} adlı kişinin {(string.Equals(proposalPayload.TargetRole, "Auditor", StringComparison.OrdinalIgnoreCase) ? "Denetçi" : "Yönetim Kurulu Azası")} olarak seçilmesine{exInfo} ilişkin karar önerisidir.{candInfo}",
                    "UnitExemption" when proposalPayload.TargetUnitId.HasValue =>
                        $"KMK Md. 20/40 kapsamında Bölüm ID #{proposalPayload.TargetUnitId.Value} için '{proposalPayload.DuesNote ?? proposalPayload.ExemptionMode ?? "Aidat Muafiyeti / Özel Tarife"}' düzenlemesinin oylanmasına ilişkin karar önerisidir.",
                    "ComplaintResolution" =>
                        $"KMK Md. 18/30 uyarınca '{agendaTitle}' konulu malik/kiracı şikayet ve talebinin karara bağlanmasına ilişkin gündem maddesidir.",
                    "CapitalProject" when proposalPayload.EstimatedBudget.HasValue =>
                        $"KMK Md. 42 kapsamında '{agendaTitle}' işinin tahmini {proposalPayload.EstimatedBudget.Value:N0} ₺ bütçe ile onaylanarak yönetici iş planına alınmasına karar önerisidir.",
                    _ => null
                };
            }

            if (!string.IsNullOrWhiteSpace(finalDecisionText))
            {
                var decision = new SystemMeetingDecision
                {
                    SystemMeetingId = meeting.Id,
                    DecisionText = finalDecisionText
                };
                _context.SystemMeetingDecisions.Add(decision);
                await _context.SaveChangesAsync();
            }

            if (attachmentFile != null && attachmentFile.Length > 0)
            {
                var savedUrl = await SaveMeetingAttachmentFileAsync(buildingId, meeting.Id, attachmentFile);
                if (!string.IsNullOrEmpty(savedUrl))
                {
                    _context.BuildingDecisionAttachments.Add(new BuildingDecisionAttachment
                    {
                        SystemMeetingId = meeting.Id,
                        AuthorUserId = userId,
                        AuthorName = User.Identity?.Name ?? "Yönetici",
                        AuthorRoleLabel = "Yönetici",
                        DocumentTitle = string.IsNullOrWhiteSpace(attachmentTitle) ? attachmentFile.FileName : attachmentTitle.Trim(),
                        FileUrl = savedUrl,
                        CreatedAt = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                }
            }

            var inviteCounts = await SendMeetingInvitationNotificationsAsync(building, meeting, sendEmails: true);
            string notifyNote = inviteCounts.NotifiedCount > 0
                ? $" ({inviteCounts.NotifiedCount} kişiye portal bildirimi, {inviteCounts.EmailedCount} kişiye e-posta çağrısı gönderildi.)"
                : "";

            TempData["SuccessMessage"] = (isPlannedOnly
                ? $"📅 '{agendaTitle}' (Karar Taslağı #{meeting.DecisionNumber}) açıldı ve Kat Maliklerinin belge/görüş & ön onay ekranına yansıtıldı."
                : $"🎉 Karar #{meeting.DecisionNumber} kaydedildi. A4/A5 Noter Defteri baskı şablonunu alabilirsiniz.") + notifyNote;
            if (string.Equals(returnTo, "Oylamalar", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(Oylamalar), new { buildingId });
            }
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        private async Task<string?> SaveMeetingAttachmentFileAsync(int buildingId, int recordId, Microsoft.AspNetCore.Http.IFormFile? file)
        {
            if (file == null || file.Length == 0 || file.Length > 15 * 1024 * 1024) return null;
            var ext = System.IO.Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? "";
            var allowedExts = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".webp", ".doc", ".docx", ".xls", ".xlsx" };
            if (!allowedExts.Contains(ext)) return null;

            var env = HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.Hosting.IWebHostEnvironment)) as Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
            var webRoot = env?.WebRootPath ?? System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
            var folder = System.IO.Path.Combine(webRoot, "uploads", "buildings", buildingId.ToString(), "meeting-docs");
            System.IO.Directory.CreateDirectory(folder);

            var uniqueName = $"m{recordId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}".Substring(0, 32) + ext;
            var fullPath = System.IO.Path.Combine(folder, uniqueName);
            using (var stream = new System.IO.FileStream(fullPath, System.IO.FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return $"/uploads/buildings/{buildingId}/meeting-docs/{uniqueName}";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDecisionBookMetadata(
            int buildingId,
            string? notaryName,
            string? journalNo,
            DateTime? openDate,
            DateTime? lastClosingDate,
            int? totalPages)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            building.DecisionBookNotaryName = string.IsNullOrWhiteSpace(notaryName) ? null : notaryName.Trim();
            building.DecisionBookJournalNo = string.IsNullOrWhiteSpace(journalNo) ? null : journalNo.Trim();
            building.DecisionBookOpenDate = openDate;
            building.DecisionBookLastClosingDate = lastClosingDate;
            building.DecisionBookTotalPages = totalPages;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"📒 '{building.Name}' fiziksel Noter Karar Defteri künyesi güncellendi.";
            return RedirectToAction(nameof(Oylamalar), new { buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMeetingAttachment(
            int buildingId,
            int meetingId,
            string? commentText,
            string? documentTitle,
            Microsoft.AspNetCore.Http.IFormFile? attachmentFile,
            string? returnTo = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var meeting = await _context.SystemMeetings.FirstOrDefaultAsync(m => m.Id == meetingId && m.ContextType == "Building" && m.ContextId == buildingId);
            if (meeting == null) return NotFound();

            var fileUrl = await SaveMeetingAttachmentFileAsync(buildingId, meetingId, attachmentFile);
            if (string.IsNullOrWhiteSpace(commentText) && string.IsNullOrWhiteSpace(fileUrl))
            {
                TempData["ErrorMessage"] = "Lütfen bir görüş notu yazın veya belge (PDF/görsel) yükleyin.";
                return string.Equals(returnTo, "Toplantilar", StringComparison.OrdinalIgnoreCase)
                    ? RedirectToAction(nameof(Toplantilar), new { buildingId })
                    : RedirectToAction(nameof(Oylamalar), new { buildingId });
            }

            _context.BuildingDecisionAttachments.Add(new BuildingDecisionAttachment
            {
                SystemMeetingId = meeting.Id,
                AuthorUserId = GetUserId(),
                AuthorName = User.Identity?.Name ?? "Site Yöneticisi",
                AuthorRoleLabel = "Yönetici",
                CommentText = string.IsNullOrWhiteSpace(commentText) ? null : commentText.Trim(),
                DocumentTitle = !string.IsNullOrWhiteSpace(documentTitle) ? documentTitle.Trim() : attachmentFile?.FileName,
                FileUrl = fileUrl,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Belge / yönetim notu toplantı dosyasına eklendi ve davetli katılımcı ekranına yansıtıldı.";
            return string.Equals(returnTo, "Toplantilar", StringComparison.OrdinalIgnoreCase)
                ? RedirectToAction(nameof(Toplantilar), new { buildingId })
                : RedirectToAction(nameof(Oylamalar), new { buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignUnitProxyVote(
            int buildingId,
            int meetingId,
            int buildingUnitId,
            string? proxyPersonName = null,
            string? participationType = "Vekil",
            string? voteChoice = null,
            string? dissentNote = null,
            Microsoft.AspNetCore.Http.IFormFile? proxyDocumentFile = null,
            string? returnTo = null,
            [FromServices] FacilityTaskDispatchService? taskDispatch = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var meeting = await _context.SystemMeetings.FirstOrDefaultAsync(m => m.Id == meetingId && m.ContextType == "Building" && m.ContextId == buildingId);
            var unit = await _context.BuildingUnits
                .Include(u => u.Building)
                .FirstOrDefaultAsync(u => u.Id == buildingUnitId);
            if (meeting == null || unit == null) return NotFound();

            bool unitAllowed = unit.BuildingId == buildingId
                || (meeting.IsSiteWide && meeting.HousingComplexId.HasValue && unit.Building?.HousingComplexId == meeting.HousingComplexId.Value);
            if (!unitAllowed) return Forbid();

            var vote = await _context.BuildingDecisionVotes
                .FirstOrDefaultAsync(v => v.SystemMeetingId == meetingId && v.BuildingUnitId == buildingUnitId && !v.IsDeleted);

            string? proxyDocUrl = await SaveMeetingAttachmentFileAsync(buildingId, meetingId, proxyDocumentFile);
            string partType = string.IsNullOrWhiteSpace(participationType) ? "Vekil" : participationType.Trim();

            if (vote == null)
            {
                vote = new BuildingDecisionVote
                {
                    SystemMeetingId = meetingId,
                    BuildingUnitId = buildingUnitId,
                    VoterUserId = unit.OwnerUserId,
                    VoterName = !string.IsNullOrWhiteSpace(unit.OwnerName) ? unit.OwnerName : $"Daire {unit.DoorNumber} Maliki",
                    ParticipationType = partType,
                    ProxyPersonName = string.IsNullOrWhiteSpace(proxyPersonName) ? null : proxyPersonName.Trim(),
                    ProxyDocumentUrl = proxyDocUrl,
                    VoteChoice = !string.IsNullOrWhiteSpace(voteChoice) ? voteChoice.Trim() : "Pending",
                    DissentNote = string.IsNullOrWhiteSpace(dissentNote) ? null : dissentNote.Trim(),
                    UnitLandShareSnapshot = unit.LandShare,
                    ApprovalTraceCode = !string.IsNullOrWhiteSpace(voteChoice) && voteChoice != "Pending"
                        ? $"VEK-{Guid.NewGuid().ToString("N").Substring(0, 5).ToUpperInvariant()}"
                        : null,
                    VotedAt = !string.IsNullOrWhiteSpace(voteChoice) && voteChoice != "Pending" ? DateTime.UtcNow : null,
                    CreatedAt = DateTime.UtcNow
                };
                _context.BuildingDecisionVotes.Add(vote);
            }
            else
            {
                vote.ParticipationType = partType;
                if (!string.IsNullOrWhiteSpace(proxyPersonName)) vote.ProxyPersonName = proxyPersonName.Trim();
                if (!string.IsNullOrWhiteSpace(proxyDocUrl)) vote.ProxyDocumentUrl = proxyDocUrl;
                if (dissentNote != null) vote.DissentNote = string.IsNullOrWhiteSpace(dissentNote) ? null : dissentNote.Trim();
                if (!string.IsNullOrWhiteSpace(voteChoice))
                {
                    vote.VoteChoice = voteChoice.Trim();
                    if (vote.VoteChoice != "Pending")
                    {
                        vote.VotedAt = DateTime.UtcNow;
                        vote.ApprovalTraceCode ??= $"VEK-{Guid.NewGuid().ToString("N").Substring(0, 5).ToUpperInvariant()}";
                    }
                }
                vote.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            if (meeting.WorkflowStage == "Decision")
            {
                var execRes = await _governanceService.TryExecuteDecisionOnQuorumAsync(meetingId, GetUserId() ?? "system", taskDispatch);
                if (execRes.Executed && !string.IsNullOrWhiteSpace(execRes.Message))
                {
                    TempData["SuccessMessage"] = $"Daire {unit.DoorNumber} vekalet/oy kaydı alındı ve yasal çoğunluk sağlandığı için karar yürürlüğe girdi: {execRes.Message}";
                    return string.Equals(returnTo, "Toplantilar", StringComparison.OrdinalIgnoreCase)
                        ? RedirectToAction(nameof(Toplantilar), new { buildingId })
                        : RedirectToAction(nameof(Oylamalar), new { buildingId });
                }
            }

            TempData["SuccessMessage"] = $"Daire {unit.DoorNumber} için temsil/vekalet bilgisi güncellendi ({partType}).";
            return string.Equals(returnTo, "Toplantilar", StringComparison.OrdinalIgnoreCase)
                ? RedirectToAction(nameof(Toplantilar), new { buildingId })
                : RedirectToAction(nameof(Oylamalar), new { buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitBoardMemberVote(
            int buildingId,
            int meetingId,
            int buildingManagerId,
            string voteChoice,
            string? dissentNote = null,
            [FromServices] FacilityTaskDispatchService? taskDispatch = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();
            int rootId = building.ParentBuildingId ?? building.Id;

            var meeting = await _context.SystemMeetings.FirstOrDefaultAsync(m => m.Id == meetingId && m.ContextType == "Building" && m.ContextId == buildingId);
            var member = await _context.BuildingManagers
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == buildingManagerId && m.BuildingId == rootId && m.IsActive);
            if (meeting == null || member == null) return NotFound();

            if (!member.CanVoteBoardDecisions())
            {
                TempData["ErrorMessage"] = "Yalnızca görev süresi dolmamış Seviye 1-3 Asil Yönetim Kurulu üyeleri kurul karar oylamasına katılabilir (Yedek ve Denetçi oy kullanamaz).";
                return RedirectToAction(nameof(Oylamalar), new { buildingId });
            }

            var cleanChoice = string.Equals(voteChoice, "Ret", StringComparison.OrdinalIgnoreCase) ? "Ret" : "Kabul";
            var existingVote = await _context.BoardDecisionVotes
                .FirstOrDefaultAsync(v => v.SystemMeetingId == meetingId && v.BuildingManagerId == buildingManagerId && !v.IsDeleted);

            string voterDisplay = !string.IsNullOrWhiteSpace(member.User?.FirstName)
                ? $"{member.User.FirstName} {member.User.LastName}".Trim()
                : (member.User?.DisplayName ?? member.User?.Email ?? "Kurul Üyesi");
            string roleTitle = BuildingManager.GetRoleLabel(member.Role);

            if (existingVote == null)
            {
                _context.BoardDecisionVotes.Add(new BoardDecisionVote
                {
                    SystemMeetingId = meetingId,
                    BuildingManagerId = member.Id,
                    VoterUserId = member.UserId,
                    VoterName = voterDisplay,
                    RoleTitle = roleTitle,
                    HierarchyLevel = BuildingManager.GetHierarchyLevel(member.Role),
                    VoteChoice = cleanChoice,
                    DissentNote = string.IsNullOrWhiteSpace(dissentNote) ? null : dissentNote.Trim(),
                    ApprovalTraceCode = $"KRL-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant()}",
                    VotedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existingVote.VoteChoice = cleanChoice;
                existingVote.DissentNote = string.IsNullOrWhiteSpace(dissentNote) ? null : dissentNote.Trim();
                existingVote.VotedAt = DateTime.UtcNow;
                existingVote.ApprovalTraceCode ??= $"KRL-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant()}";
                existingVote.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var activeBoard = await _context.BuildingManagers
                .AsNoTracking()
                .Where(m => m.BuildingId == rootId && m.IsActive)
                .ToListAsync();
            var allBoardVotes = await _context.BoardDecisionVotes
                .AsNoTracking()
                .Where(v => v.SystemMeetingId == meetingId && !v.IsDeleted)
                .ToListAsync();
            var qCheck = GMK360.Web.Helpers.KmkQuorumHelper.ComputeBoardMajority(activeBoard, allBoardVotes);
            if (qCheck.IsRejectedByMath)
            {
                TempData["ErrorMessage"] = $"{voterDisplay} ({roleTitle}) oyu kaydedildi. {qCheck.StatusText} ({qCheck.ApprovedUnits} Evet / {qCheck.RejectedUnits} Ret).";
            }
            else if (qCheck.IsQuorumMet)
            {
                TempData["SuccessMessage"] = $"{voterDisplay} ({roleTitle}) oyu kaydedildi. Salt çoğunluk sağlandı ({qCheck.ApprovedUnits}/{qCheck.RequiredUnitCount}) — Karar yazdırılabilir.";
            }
            else
            {
                TempData["SuccessMessage"] = $"{voterDisplay} ({roleTitle}) için kurul oyu kaydedildi ({cleanChoice}). Durum: {qCheck.StatusText}.";
            }
            return RedirectToAction(nameof(Oylamalar), new { buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExecuteApprovedDecision(
            int meetingId,
            int buildingId,
            [FromServices] FacilityTaskDispatchService taskDispatch)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var res = await _governanceService.TryExecuteDecisionOnQuorumAsync(meetingId, GetUserId() ?? "system", taskDispatch, forceOverrideAutoFlag: true);
            if (res.Executed)
            {
                TempData["SuccessMessage"] = $"⚡ Karar yürürlüğe alındı: {res.Message}";
            }
            else
            {
                TempData["ErrorMessage"] = !string.IsNullOrWhiteSpace(res.Message)
                    ? res.Message
                    : "Karar henüz yasal KMK çoğunluğunu (sayı/arsa payı) sağlamadığı için uygulanamaz.";
            }
            return RedirectToAction(nameof(Oylamalar), new { buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConcludeMeetingWithDecision(int meetingId, int buildingId, string decisionText, string? returnTo = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var meeting = await _context.SystemMeetings
                .Include(m => m.Votes)
                .Include(m => m.BoardVotes)
                .FirstOrDefaultAsync(m => m.Id == meetingId && m.ContextType == "Building" && m.ContextId == buildingId);
            if (meeting == null) return NotFound();

            if (string.Equals(meeting.WorkflowStage, "Decision", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(meeting.QuorumType, "BoardMajority", StringComparison.OrdinalIgnoreCase))
                {
                    int boardRootId = building.ParentBuildingId ?? building.Id;
                    var roster = await _context.BuildingManagers
                        .AsNoTracking()
                        .Where(m => m.BuildingId == boardRootId && m.IsActive)
                        .ToListAsync();
                    var boardQuorum = GMK360.Web.Helpers.KmkQuorumHelper.ComputeBoardMajority(roster, meeting.BoardVotes);
                    if (boardQuorum.TotalUnits > 0 && !boardQuorum.IsQuorumMet)
                    {
                        TempData["ErrorMessage"] = $"Kurul Onay Kilidi: Yeterli Asil Üye (Seviye 1-3) onayı sağlanmadı (Onay Bekliyor: {boardQuorum.ApprovedUnits}/{boardQuorum.RequiredUnitCount}). Karar kesinleştirilemez.";
                        return string.Equals(returnTo, "Oylamalar", StringComparison.OrdinalIgnoreCase)
                            ? RedirectToAction(nameof(Oylamalar), new { buildingId })
                            : RedirectToAction(nameof(Detail), new { id = buildingId });
                    }
                }
                else
                {
                    var scopeUnits = (meeting.IsSiteWide && meeting.HousingComplexId.HasValue)
                        ? await _context.BuildingUnits
                            .Where(u => u.Building.HousingComplexId == meeting.HousingComplexId.Value && !(u.Category ?? "").StartsWith("OrtakAlan"))
                            .ToListAsync()
                        : await _context.BuildingUnits
                            .Where(u => u.BuildingId == buildingId && !(u.Category ?? "").StartsWith("OrtakAlan"))
                            .ToListAsync();

                    var quorum = GMK360.Web.Helpers.KmkQuorumHelper.Compute(scopeUnits, meeting.Votes, meeting.QuorumType);
                    if (scopeUnits.Any() && !quorum.IsQuorumMet)
                    {
                        TempData["ErrorMessage"] = $"Onay Kilidi: Yeterli çoğunluk / PIN onayı sağlanmadan karar kesinleştirilemez (Onay Bekliyor: {quorum.ApprovedUnits}/{quorum.RequiredUnitCount} Malik).";
                        return string.Equals(returnTo, "Oylamalar", StringComparison.OrdinalIgnoreCase)
                            ? RedirectToAction(nameof(Oylamalar), new { buildingId })
                            : RedirectToAction(nameof(Detail), new { id = buildingId });
                    }
                }
            }

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

        // 3.2 Noter Karar Defteri Baskı Şablonu (KMK Md. 32 A4 / A5 Islak İmza Tutanağı + QR)
        [HttpGet]
        public async Task<IActionResult> PrintDecisionSheet(int meetingId, string? format = "a4")
        {
            var meeting = await _context.SystemMeetings
                .Include(m => m.Decisions)
                .Include(m => m.Votes)
                .Include(m => m.BoardVotes)
                .Include(m => m.Attachments)
                .FirstOrDefaultAsync(m => m.Id == meetingId && m.ContextType == "Building");

            if (meeting == null) return NotFound("Toplantı kaydı bulunamadı.");

            var building = await _context.Buildings
                .Include(b => b.HousingComplex)
                .Include(b => b.Units)
                .Include(b => b.City)
                .Include(b => b.District)
                .FirstOrDefaultAsync(b => b.Id == meeting.ContextId);

            if (!CanAccessBuilding(building, GetUserId()))
                return Forbid();

            if (string.IsNullOrWhiteSpace(meeting.VerificationToken))
            {
                meeting.VerificationToken = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant();
                await _context.SaveChangesAsync();
            }

            List<BuildingUnit> rawUnits;
            if (meeting.IsSiteWide && building?.HousingComplexId != null)
            {
                rawUnits = await _context.BuildingUnits
                    .Include(u => u.Building)
                    .Where(u => u.Building.HousingComplexId == building.HousingComplexId && !(u.Category ?? "").StartsWith("OrtakAlan"))
                    .ToListAsync();
            }
            else
            {
                rawUnits = (building?.Units ?? Enumerable.Empty<BuildingUnit>())
                    .Where(u => !(u.Category ?? "").StartsWith("OrtakAlan", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var eligibleUnits = rawUnits
                .OrderBy(u => u.Building?.BlockName ?? u.Building?.Name)
                .ThenBy(u => u.FloorLevel)
                .ThenBy(u => u.DoorNumber)
                .ToList();

            int rootIdForBoard = building != null ? (building.ParentBuildingId ?? building.Id) : meeting.ContextId;
            var boardRoster = await _context.BuildingManagers
                .AsNoTracking()
                .Include(m => m.User)
                .Where(m => m.BuildingId == rootIdForBoard && m.IsActive)
                .OrderBy(m => m.Role)
                .ToListAsync();

            ViewBag.Building = building;
            ViewBag.BoardRoster = boardRoster;
            ViewBag.EligibleUnits = eligibleUnits;
            ViewBag.Format = string.Equals(format, "a5", StringComparison.OrdinalIgnoreCase) ? "a5" : "a4";
            ViewBag.QuorumSummary = string.Equals(meeting.QuorumType, "BoardMajority", StringComparison.OrdinalIgnoreCase)
                ? GMK360.Web.Helpers.KmkQuorumHelper.ComputeBoardMajority(boardRoster, meeting.BoardVotes)
                : GMK360.Web.Helpers.KmkQuorumHelper.Compute(eligibleUnits, meeting.Votes, meeting.QuorumType);
            ViewBag.VerifyUrl = Url.Action("VerifyDecision", "BuildingManager", new { meetingId = meeting.Id, code = meeting.VerificationToken }, Request.Scheme);
            return View(meeting);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> VerifyDecision(int meetingId, string? code)
        {
            var meeting = await _context.SystemMeetings
                .AsNoTracking()
                .Include(m => m.Decisions)
                .Include(m => m.Votes)
                .Include(m => m.Attachments)
                .FirstOrDefaultAsync(m => m.Id == meetingId && m.ContextType == "Building" && !m.IsDeleted);

            if (meeting == null || string.IsNullOrWhiteSpace(code) || !string.Equals(meeting.VerificationToken, code.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return NotFound("Karar doğrulama kaydı bulunamadı veya doğrulama kodu geçersiz.");
            }

            var building = await _context.Buildings
                .AsNoTracking()
                .Include(b => b.Units)
                .FirstOrDefaultAsync(b => b.Id == meeting.ContextId);

            ViewBag.Building = building;
            ViewBag.QuorumSummary = GMK360.Web.Helpers.KmkQuorumHelper.Compute(building?.Units, meeting.Votes, meeting.QuorumType);
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
        public async Task<IActionResult> UploadMeetingScan(int meetingId, Microsoft.AspNetCore.Http.IFormFile? scanFile, string? returnTo = null)
        {
            var meeting = await _context.SystemMeetings.FirstOrDefaultAsync(m => m.Id == meetingId && m.ContextType == "Building");
            if (meeting == null) return NotFound();

            var building = await GetManagedBuildingAsync(meeting.ContextId);
            if (building == null) return Forbid();

            if (scanFile == null || scanFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen yüklenecek PDF veya görsel dosyasını seçiniz.";
                return string.Equals(returnTo, "Oylamalar", StringComparison.OrdinalIgnoreCase)
                    ? RedirectToAction(nameof(Oylamalar), new { buildingId = building.Id })
                    : RedirectToAction(nameof(Detail), new { id = building.Id });
            }

            var ext = System.IO.Path.GetExtension(scanFile.FileName)?.ToLowerInvariant();
            var allowedExts = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };
            if (string.IsNullOrEmpty(ext) || !allowedExts.Contains(ext))
            {
                TempData["ErrorMessage"] = "Yalnızca PDF veya görsel (JPG, PNG, WEBP) dosyaları yüklenebilir.";
                return string.Equals(returnTo, "Oylamalar", StringComparison.OrdinalIgnoreCase)
                    ? RedirectToAction(nameof(Oylamalar), new { buildingId = building.Id })
                    : RedirectToAction(nameof(Detail), new { id = building.Id });
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
            return string.Equals(returnTo, "Oylamalar", StringComparison.OrdinalIgnoreCase)
                ? RedirectToAction(nameof(Oylamalar), new { buildingId = building.Id })
                : RedirectToAction(nameof(Detail), new { id = building.Id });
        }

        private async Task<bool> TrySendActivationEmailAsync(
            GMK360.Core.Entities.Identity.ApplicationUser? targetUser,
            string roleTitle,
            string buildingName)
        {
            var res = await TrySendActivationEmailDetailedAsync(targetUser, roleTitle, buildingName);
            return res.Success;
        }

        private async Task<(bool Success, string? ErrorMessage, string? ActivateUrl)> TrySendActivationEmailDetailedAsync(
            GMK360.Core.Entities.Identity.ApplicationUser? targetUser,
            string roleTitle,
            string buildingName)
        {
            if (targetUser == null
                || string.IsNullOrWhiteSpace(targetUser.Email)
                || targetUser.Email.EndsWith("@gmk360.local", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Geçerli bir kişisel e-posta adresi girilmemiş.", null);
            }

            if (string.IsNullOrWhiteSpace(targetUser.ActivationToken))
            {
                return (false, "Kullanıcı hesabını daha önce doğrulamış görünüyor (mevcut şifresiyle giriş yapabilir).", null);
            }

            var activateUrl = $"{Request.Scheme}://{Request.Host}/Account/ActivateResident?token={Uri.EscapeDataString(targetUser.ActivationToken)}";
            var portalInstallUrl = $"{Request.Scheme}://{Request.Host}/FacilityPortal?installPwa=1";
            var emailSvc = HttpContext.RequestServices.GetService<IEmailService>();
            if (emailSvc == null)
            {
                return (false, "Sistem e-posta servisi (SMTP) yapılandırılmamış.", activateUrl);
            }

            var fullName = $"{targetUser.FirstName} {targetUser.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(fullName)) fullName = targetUser.Email;

            var subject = $"GMK360 | {buildingName} ({roleTitle}) Davet, Şifre & Mobil Uygulama Kurulumu";
            var body = $@"
<div style=""font-family:Arial,sans-serif;max-width:580px;margin:0 auto;padding:22px;border:1px solid #e2e8f0;border-radius:14px;background:#ffffff;"">
  <h2 style=""color:#15803d;margin-top:0;"">GMK360 Bina &amp; Site Portalı Daveti</h2>
  <p>Merhaba <strong>{System.Net.WebUtility.HtmlEncode(fullName)}</strong>,</p>
  <p><strong>{System.Net.WebUtility.HtmlEncode(buildingName)}</strong> yönetiminde <strong>{System.Net.WebUtility.HtmlEncode(roleTitle)}</strong> olarak hesabınız oluşturuldu.</p>
  <div style=""background:#f8fafc;border:1px solid #e2e8f0;border-radius:10px;padding:14px;margin:16px 0;"">
    <strong style=""color:#0f172a;"">1. Adım: Şifre ve Kişisel Onay PIN'inizi Belirleyin</strong>
    <p style=""margin:8px 0 14px;font-size:14px;color:#334155;"">Aşağıdaki butona tıklayarak ad-soyadınızı doğrulayın, giriş şifrenizi ve 4–6 haneli hızlı giriş PIN kodunuzu belirleyin:</p>
    <a href=""{activateUrl}"" style=""background:#15803d;color:#fff;text-decoration:none;padding:12px 22px;border-radius:999px;font-weight:bold;display:inline-block;"">Profilimi Doğrula &amp; Şifre/PIN Belirle</a>
  </div>
  <div style=""background:#fffbeb;border:1px solid #fde68a;border-radius:10px;padding:14px;margin:16px 0;font-size:13px;color:#451a03;"">
    <strong>2. Adım: Telefonunuza Özel Uygulama (PWA) Olarak İndirin</strong>
    <p style=""margin:8px 0 12px;"">Şifre ve PIN kurulumundan sonra aşağıdaki butona dokunup mobil uygulamayı ana ekranınıza kurabilirsiniz <em>(Gmail içinden açılırsa sağ üst ⋮ menüsünden <strong>Chrome/Safari'de Aç</strong> seçin)</em>:</p>
    <p style=""margin:0 0 12px;"">
      <a href=""{portalInstallUrl}"" style=""background:#f59e0b;color:#111827;text-decoration:none;padding:10px 18px;border-radius:999px;font-weight:bold;display:inline-block;"">📲 Telefon Uygulamasını İndir / Kur</a>
    </p>
    <ul style=""margin:4px 0 0;padding-left:18px;line-height:1.6;"">
      <li><strong>Android (Chrome):</strong> Portal üstündeki <em>Uygulamayı İndir</em> butonuna basın veya ⋮ menü &rarr; <em>Uygulamayı yükle</em> seçin.</li>
      <li><strong>iPhone (Safari):</strong> Alttaki <strong>Paylaş</strong> simgesine dokunup <strong>""Ana Ekrana Ekle""</strong> seçin.</li>
    </ul>
  </div>
  <p style=""font-size:12px;color:#64748b;"">Buton çalışmazsa aktivasyon bağlantısını tarayıcınıza yapıştırın:<br/><a href=""{activateUrl}"">{activateUrl}</a></p>
</div>";

            try
            {
                var res = await emailSvc.SendEmailWithResultAsync(targetUser.Email, subject, body);
                return (res.Success, res.Success ? null : (!string.IsNullOrWhiteSpace(res.Message) ? res.Message : "SMTP e-posta gönderimi başarısız."), activateUrl);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, activateUrl);
            }
        }

        private async Task<(bool OwnerSent, bool TenantSent, string? FallbackUrl, string? WarningDetail)> SendUnitActivationInvitesAsync(
            Building building,
            BuildingUnit unit)
        {
            bool ownerSent = false;
            bool tenantSent = false;
            string? fallbackUrl = null;
            string? warningDetail = null;

            if (!string.IsNullOrWhiteSpace(unit.OwnerUserId) && !string.IsNullOrWhiteSpace(unit.OwnerEmail)
                && !unit.OwnerEmail.EndsWith("@gmk360.local", StringComparison.OrdinalIgnoreCase))
            {
                var ow = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == unit.OwnerUserId);
                var res = await TrySendActivationEmailDetailedAsync(ow, $"Daire {unit.DoorNumber} Kat Maliki", building.Name);
                ownerSent = res.Success;
                if (!res.Success && !string.IsNullOrWhiteSpace(res.ActivateUrl))
                {
                    fallbackUrl ??= res.ActivateUrl;
                    warningDetail ??= res.ErrorMessage;
                }
            }

            if (!unit.IsEmpty && !string.IsNullOrWhiteSpace(unit.TenantUserId) && !string.IsNullOrWhiteSpace(unit.TenantEmail)
                && !unit.TenantEmail.EndsWith("@gmk360.local", StringComparison.OrdinalIgnoreCase))
            {
                var tn = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == unit.TenantUserId);
                var res = await TrySendActivationEmailDetailedAsync(tn, $"Daire {unit.DoorNumber} Kiracısı", building.Name);
                tenantSent = res.Success;
                if (!res.Success && !string.IsNullOrWhiteSpace(res.ActivateUrl))
                {
                    fallbackUrl ??= res.ActivateUrl;
                    warningDetail ??= res.ErrorMessage;
                }
            }

            return (ownerSent, tenantSent, fallbackUrl, warningDetail);
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
            if (targetUser == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı kaydı bulunamadı.";
                return RedirectToAction(nameof(Sakinler), new { buildingId = selectedBuildingFilter });
            }

            if (targetUser.IsShadowAccount && string.IsNullOrWhiteSpace(targetUser.ActivationToken))
            {
                targetUser.ActivationToken = Guid.NewGuid().ToString("N");
                targetUser.ActivationTokenExpiresAt = DateTime.UtcNow.AddDays(14);
                await _context.SaveChangesAsync();
            }

            if (string.IsNullOrWhiteSpace(targetUser.ActivationToken))
            {
                TempData["WarningMessage"] = $"{targetUser.Email} hesabını zaten doğrulamış; mevcut şifresiyle /FacilityPortal üzerinden giriş yapabilir.";
                return RedirectToAction(nameof(Sakinler), new { buildingId = selectedBuildingFilter });
            }

            string roleTitle = string.Equals(roleType, "Tenant", StringComparison.OrdinalIgnoreCase)
                ? $"Daire {unit.DoorNumber} Kiracısı"
                : $"Daire {unit.DoorNumber} Kat Maliki";

            var res = await TrySendActivationEmailDetailedAsync(targetUser, roleTitle, building.Name);
            if (res.Success)
            {
                TempData["SuccessMessage"] = $"✉️ {targetUser.Email} adresine şifre, PIN ve PWA kurulum daveti gönderildi.";
            }
            else
            {
                TempData["WarningMessage"] = $"E-posta gönderilemedi ({res.ErrorMessage ?? "SMTP ayarını kontrol edin"}). İsterseniz aşağıdaki davet bağlantısını kopyalayıp doğrudan iletebilirsiniz:";
                if (!string.IsNullOrWhiteSpace(res.ActivateUrl))
                {
                    TempData["InviteFallbackLink"] = res.ActivateUrl;
                }
            }

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

            var inviteRes = await SendUnitActivationInvitesAsync(building, unit);
            var mailNote = (inviteRes.OwnerSent || inviteRes.TenantSent)
                ? " E-posta adresi girilenlere şifre/PIN ve PWA kurulum daveti gönderildi."
                : "";
            if (!inviteRes.OwnerSent && !inviteRes.TenantSent && !string.IsNullOrWhiteSpace(inviteRes.FallbackUrl))
            {
                TempData["WarningMessage"] = $"Daire kaydedildi ancak davet e-postası gönderilemedi ({inviteRes.WarningDetail ?? "SMTP ayarını kontrol edin"}). Bağlantıyı kopyalayıp iletebilirsiniz:";
                TempData["InviteFallbackLink"] = inviteRes.FallbackUrl;
            }

            TempData["SuccessMessage"] = $"'{unitNumber}' bağımsız bölümü sakin/iletişim bilgileriyle eklendi.{mailNote}";
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
            string? linkedSourceLabel = null,
            int? linkedDecisionMeetingId = null)
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

            SystemMeeting? linkedDec = linkedDecisionMeetingId.HasValue
                ? await _context.SystemMeetings.AsNoTracking().FirstOrDefaultAsync(m => m.Id == linkedDecisionMeetingId.Value && m.ContextType == "Building" && !m.IsDeleted)
                : null;
            if (linkedDec != null)
            {
                string decCitation = $"Defter Sayfa #{linkedDec.BookPageNumber ?? linkedDec.Id} · Karar #{linkedDec.DecisionNumber ?? linkedDec.Id.ToString("D2")}";
                resolvedSourceLabel = string.IsNullOrWhiteSpace(resolvedSourceLabel)
                    ? $"{decCitation} — {linkedDec.AgendaTitle}"
                    : $"{resolvedSourceLabel} | {decCitation}";
                if (string.IsNullOrWhiteSpace(linkedSourceType) || linkedSourceType == "Genel")
                {
                    linkedSourceType = "KararDefteri";
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

            if (result.Success && result.CreatedExpenseId.HasValue && linkedDec != null)
            {
                var exp = await _context.BuildingExpenses.FirstOrDefaultAsync(e => e.Id == result.CreatedExpenseId.Value);
                if (exp != null)
                {
                    exp.LinkedDecisionMeetingId = linkedDec.Id;
                    await _context.SaveChangesAsync();
                }
            }

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

            var rootIdsForManagers = buildings
                .Where(b => allowedIds.Contains(b.Id))
                .Select(b => b.ParentBuildingId ?? b.Id)
                .Distinct()
                .ToList();
            var governanceMembers = rootIdsForManagers.Any()
                ? await _context.BuildingManagers
                    .AsNoTracking()
                    .Include(m => m.User)
                    .Where(m => rootIdsForManagers.Contains(m.BuildingId) && m.IsActive)
                    .OrderBy(m => m.Role)
                    .ThenBy(m => m.Id)
                    .ToListAsync()
                : new System.Collections.Generic.List<BuildingManager>();

            ViewBag.Buildings = buildings;
            ViewBag.SelectedBuildingId = buildingId;
            ViewBag.HasAuthorityDoc = checkBuildingId > 0 && await HasDecisionAuthorityAsync(checkBuildingId);
            ViewBag.ResidentProfiles = residentProfiles;
            ViewBag.GovernanceMembers = governanceMembers;
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

            var inviteRes = await SendUnitActivationInvitesAsync(building, unit);
            if (!inviteRes.OwnerSent && !inviteRes.TenantSent && !string.IsNullOrWhiteSpace(inviteRes.FallbackUrl))
            {
                TempData["WarningMessage"] = $"Kayıt güncellendi ancak e-posta daveti gönderilemedi ({inviteRes.WarningDetail ?? "SMTP ayarını kontrol edin"}). Bağlantıyı kopyalayıp iletebilirsiniz:";
                TempData["InviteFallbackLink"] = inviteRes.FallbackUrl;
            }

            var mailNote = (inviteRes.OwnerSent || inviteRes.TenantSent) ? " E-posta adresi olanlara şifre/PIN ve PWA kurulum daveti gönderildi." : "";
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

            var allowedComplexIds = buildings
                .Where(b => allowedIds.Contains(b.Id) && b.HousingComplexId.HasValue)
                .Select(b => b.HousingComplexId!.Value)
                .Distinct()
                .ToList();

            var latestDuesDecision = allowedIds.Any()
                ? await _context.SystemMeetings
                    .AsNoTracking()
                    .Where(m => m.ContextType == "Building" && !m.IsDeleted && m.DecisionCategory == "DuesUpdate" && m.IsExecuted
                        && (allowedIds.Contains(m.ContextId)
                            || (m.HousingComplexId.HasValue && m.IsSiteWide && allowedComplexIds.Contains(m.HousingComplexId.Value))))
                    .OrderByDescending(m => m.ExecutedAt ?? m.MeetingDate)
                    .FirstOrDefaultAsync()
                : null;

            var decisionsForExpense = allowedIds.Any()
                ? await _context.SystemMeetings
                    .AsNoTracking()
                    .Where(m => m.ContextType == "Building" && !m.IsDeleted && m.WorkflowStage == "Decision"
                        && (allowedIds.Contains(m.ContextId)
                            || (m.HousingComplexId.HasValue && m.IsSiteWide && allowedComplexIds.Contains(m.HousingComplexId.Value))))
                    .OrderByDescending(m => m.BookPageNumber ?? 0)
                    .ThenByDescending(m => m.MeetingDate)
                    .Take(30)
                    .ToListAsync()
                : new System.Collections.Generic.List<SystemMeeting>();

            ViewBag.Buildings = buildings;
            ViewBag.SelectedBuildingId = buildingId;
            ViewBag.LivingUnits = livingUnits;
            ViewBag.FixedExpenses = fixedExpenses;
            ViewBag.BuildingStaffs = buildingStaffs;
            ViewBag.LatestDuesDecision = latestDuesDecision;
            ViewBag.DecisionsForExpense = decisionsForExpense;
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
                .Include(s => s.Documents)
                .Include(s => s.User)
                .Include(s => s.HousingComplex)
                .Include(s => s.Building).ThenInclude(b => b.HousingComplex)
                .Where(s => s.IsActive && !s.IsDeleted
                    && (scopeIds.Contains(s.BuildingId)
                        || (s.IsSiteWide && s.HousingComplexId.HasValue && building.HousingComplexId == s.HousingComplexId)))
                .OrderBy(s => s.FullName)
                .ToListAsync();
            var bStaffIds = buildingStaffs.Select(s => s.Id).ToList();

            ViewBag.BuildingStaffs = buildingStaffs;
            var formerBuildingStaffs = await _context.BuildingStaffs
                .Include(s => s.Documents)
                .Include(s => s.User)
                .Include(s => s.HousingComplex)
                .Include(s => s.Building).ThenInclude(b => b.HousingComplex)
                .Where(s => !s.IsActive && !s.IsDeleted
                    && (scopeIds.Contains(s.BuildingId)
                        || (s.IsSiteWide && s.HousingComplexId.HasValue && building.HousingComplexId == s.HousingComplexId)))
                .OrderByDescending(s => s.LeaveDate ?? s.UpdatedAt ?? s.CreatedAt)
                .ToListAsync();
            ViewBag.FormerBuildingStaffs = formerBuildingStaffs;
            var allPayrollStaffIds = bStaffIds.Concat(formerBuildingStaffs.Select(fs => fs.Id)).Distinct().ToList();
            ViewBag.MonthlyPayrolls = allPayrollStaffIds.Any()
                ? await _context.BuildingStaffMonthlyPayrolls
                    .Include(p => p.Staff)
                    .Where(p => allPayrollStaffIds.Contains(p.BuildingStaffId) && !p.IsDeleted)
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
            var accComplexIds = scopeBuildings
                .Where(b => b.HousingComplexId.HasValue)
                .Select(b => b.HousingComplexId!.Value)
                .Distinct()
                .ToList();
            ViewBag.DecisionsForExpense = await _context.SystemMeetings
                .AsNoTracking()
                .Where(m => m.ContextType == "Building" && !m.IsDeleted && m.WorkflowStage == "Decision"
                    && (scopeIds.Contains(m.ContextId)
                        || (m.HousingComplexId.HasValue && m.IsSiteWide && accComplexIds.Contains(m.HousingComplexId.Value))))
                .OrderByDescending(m => m.BookPageNumber ?? 0)
                .ThenByDescending(m => m.MeetingDate)
                .Take(30)
                .ToListAsync();

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
            string? email,
            string? address,
            string roleType,
            string? jobDescription,
            decimal netSalary,
            decimal sgkEmployerShare,
            decimal incomeAndStampTax,
            decimal fringeBenefits,
            decimal monthlySeveranceFund,
            DateTime hireDate,
            bool sendActivationEmail = false,
            bool isSiteWide = false)
        {
            var building = await _context.Buildings.Include(b => b.HousingComplex).FirstOrDefaultAsync(b => b.Id == buildingId);
            if (building == null || !CanAccessBuilding(building, GetUserId())) return Forbid();

            var staff = new BuildingStaff
            {
                BuildingId = buildingId,
                IsSiteWide = isSiteWide && building.HousingComplexId.HasValue,
                HousingComplexId = building.HousingComplexId,
                FullName = fullName?.Trim() ?? "Personel",
                TcIdentityNo = tcIdentityNo?.Trim(),
                SgkNumber = sgkNumber?.Trim(),
                PhoneNumber = phoneNumber?.Trim(),
                Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
                Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim(),
                QuickPinHash = null, // Personel kendi gizli PIN'ini ilk girişte / profilinden belirler
                RoleType = string.IsNullOrWhiteSpace(roleType) ? "Kapıcı (Vergi İstisnalı)" : roleType.Trim(),
                JobDescription = string.IsNullOrWhiteSpace(jobDescription) ? null : jobDescription.Trim(),
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

            var dispatchSvc = HttpContext.RequestServices.GetService(typeof(FacilityTaskDispatchService)) as FacilityTaskDispatchService;
            string? inviteExtraMsg = null;
            if (dispatchSvc != null)
            {
                var prov = await dispatchSvc.ProvisionStaffAccountAsync(staff, GetUserId(), resetTempPassword: true);
                if (prov.User != null)
                {
                    bool mailed = await PrepareStaffInviteNotificationAsync(building, staff, prov.User, prov.TempPassword, sendActivationEmail);
                    inviteExtraMsg = mailed
                        ? $" Aktivasyon bağlantısı ve geçici şifre '{staff.Email}' adresine gönderildi."
                        : " İlk giriş bağlantısı ve geçici şifre kart üzerinde / aşağıda hazırlandı.";
                }
                else if (!string.IsNullOrWhiteSpace(prov.ErrorMessage))
                {
                    TempData["WarningMessage"] = $"Personel eklendi ancak kullanıcı hesabı uyarısı: {prov.ErrorMessage}";
                }
            }

            TempData["Success"] = $"'{staff.FullName}' kadroya eklendi. Aylık işletme maliyeti: {staff.TotalMonthlyOperatingCost:N2} ₺, Kıdem fonu: {staff.MonthlySeveranceFund:N2} ₺.{inviteExtraMsg}";
            return RedirectToAction(nameof(Accounting), new { buildingId, tab = "personel" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBuildingStaff(
            int buildingId,
            int buildingStaffId,
            string fullName,
            string roleType,
            string? jobDescription,
            string? tcIdentityNo,
            string? sgkNumber,
            string? phoneNumber,
            string? email,
            string? address,
            DateTime hireDate,
            decimal netSalary,
            decimal sgkEmployerShare,
            decimal incomeAndStampTax,
            decimal fringeBenefits,
            decimal monthlySeveranceFund,
            bool isSiteWide = false,
            int? addressCityId = null,
            int? addressDistrictId = null,
            int? addressNeighborhoodId = null,
            int? addressStreetId = null,
            string? addressStreetName = null,
            string? addressBuildingNo = null,
            string? addressApartmentNo = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var staff = await _context.BuildingStaffs
                .Include(s => s.User)
                .Include(s => s.Building)
                .FirstOrDefaultAsync(s => s.Id == buildingStaffId && !s.IsDeleted);
            if (staff == null) return NotFound();
            var staffBld = await GetManagedBuildingAsync(staff.BuildingId);
            if (staffBld == null) return Forbid();

            staff.FullName = string.IsNullOrWhiteSpace(fullName) ? staff.FullName : fullName.Trim();
            staff.RoleType = string.IsNullOrWhiteSpace(roleType) ? staff.RoleType : roleType.Trim();
            staff.JobDescription = string.IsNullOrWhiteSpace(jobDescription) ? null : jobDescription.Trim();
            staff.TcIdentityNo = string.IsNullOrWhiteSpace(tcIdentityNo) ? null : tcIdentityNo.Trim();
            staff.SgkNumber = string.IsNullOrWhiteSpace(sgkNumber) ? null : sgkNumber.Trim();
            staff.PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? staff.PhoneNumber : phoneNumber.Trim();
            staff.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
            staff.IsSiteWide = isSiteWide && staffBld.HousingComplexId.HasValue;
            staff.HousingComplexId = staffBld.HousingComplexId;

            if (addressCityId.HasValue && addressCityId.Value > 0)
            {
                var resolved = await GMK360.Web.Helpers.AddressCaptureHelper.ResolveAndEnrichAsync(_context, new GMK360.Web.Helpers.StructuredAddressInput
                {
                    CityId = addressCityId,
                    DistrictId = addressDistrictId,
                    NeighborhoodId = addressNeighborhoodId,
                    StreetId = addressStreetId,
                    StreetName = addressStreetName,
                    BuildingNo = addressBuildingNo,
                    ApartmentNo = addressApartmentNo
                });
                if (resolved.IsValid)
                {
                    GMK360.Web.Helpers.AddressCaptureHelper.ApplyToStaff(staff, resolved);
                    if (staff.User != null) GMK360.Web.Helpers.AddressCaptureHelper.ApplyToUser(staff.User, resolved);
                }
            }
            else if (address != null)
            {
                staff.Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
            }

            if (hireDate != default) staff.HireDate = hireDate;
            staff.NetSalary = Math.Max(0m, netSalary);
            staff.SgkEmployerShare = Math.Max(0m, sgkEmployerShare);
            staff.IncomeAndStampTax = Math.Max(0m, incomeAndStampTax);
            staff.FringeBenefits = Math.Max(0m, fringeBenefits);
            staff.MonthlySeveranceFund = Math.Max(0m, monthlySeveranceFund);
            staff.UpdatedAt = DateTime.UtcNow;

            if (staff.User != null)
            {
                staff.User.DisplayName = staff.FullName;
                if (!string.IsNullOrWhiteSpace(staff.PhoneNumber)) staff.User.PhoneNumber = staff.PhoneNumber;
                if (!string.IsNullOrWhiteSpace(staff.Address)) staff.User.Address = staff.Address;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"'{staff.FullName}' kadro, görev ve maaş/SGK bilgileri güncellendi.";
            return RedirectToAction(nameof(Accounting), new { buildingId, tab = "personel" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateBuildingStaff(
            int buildingId,
            int buildingStaffId,
            string actionMode,
            DateTime? leaveDate,
            string? terminationReason)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var staff = await _context.BuildingStaffs.FirstOrDefaultAsync(s => s.Id == buildingStaffId && !s.IsDeleted);
            if (staff == null) return NotFound();
            if (await GetManagedBuildingAsync(staff.BuildingId) == null) return Forbid();

            if (string.Equals(actionMode, "delete_test", StringComparison.OrdinalIgnoreCase))
            {
                bool hasPaidPayroll = await _context.BuildingStaffMonthlyPayrolls
                    .AnyAsync(p => p.BuildingStaffId == staff.Id && !p.IsDeleted && (p.IsPaid || p.IsSalaryPaid || p.IsSgkPaid));
                if (hasPaidPayroll)
                {
                    TempData["ErrorMessage"] = $"'{staff.FullName}' için ödenmiş bordro/kıdem kaydı bulunduğu için tamamen silinemez. Muhasebe bütünlüğü için 'Resmi İşten Çıkış Ver' seçeneğini kullanın.";
                    return RedirectToAction(nameof(Accounting), new { buildingId, tab = "personel" });
                }

                staff.IsActive = false;
                staff.IsDeleted = true;
                staff.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"'{staff.FullName}' kadrodan ve aktif listeden kaldırıldı.";
            }
            else
            {
                staff.IsActive = false;
                staff.LeaveDate = leaveDate ?? DateTime.Today;
                staff.TerminationReason = string.IsNullOrWhiteSpace(terminationReason) ? "İşten Çıkış" : terminationReason.Trim();
                staff.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"'{staff.FullName}' için {staff.LeaveDate:dd.MM.yyyy} tarihli işten çıkış işlendi. Özlük dosyası ve geçmiş bordroları yasal arşivde korundu.";
            }

            return RedirectToAction(nameof(Accounting), new { buildingId, tab = "personel" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendStaffActivationInvite(
            int buildingId,
            int buildingStaffId,
            string? email,
            string? phoneNumber,
            bool sendEmail = false)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var staff = await _context.BuildingStaffs.FirstOrDefaultAsync(s => s.Id == buildingStaffId && !s.IsDeleted);
            if (staff == null) return NotFound();
            var staffBld = await GetManagedBuildingAsync(staff.BuildingId);
            if (staffBld == null) return Forbid();

            staff.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
            if (!string.IsNullOrWhiteSpace(phoneNumber)) staff.PhoneNumber = phoneNumber.Trim();
            staff.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var dispatchSvc = HttpContext.RequestServices.GetService(typeof(FacilityTaskDispatchService)) as FacilityTaskDispatchService;
            if (dispatchSvc == null)
            {
                TempData["ErrorMessage"] = "Hesap servisi başlatılamadı.";
                return RedirectToAction(nameof(Accounting), new { buildingId, tab = "personel" });
            }

            var prov = await dispatchSvc.ProvisionStaffAccountAsync(staff, GetUserId(), resetTempPassword: true);
            if (prov.User == null)
            {
                TempData["ErrorMessage"] = prov.ErrorMessage ?? "Davet bağlantısı üretilemedi.";
                return RedirectToAction(nameof(Accounting), new { buildingId, tab = "personel" });
            }

            bool mailed = await PrepareStaffInviteNotificationAsync(staffBld, staff, prov.User, prov.TempPassword, sendEmail);
            TempData["Success"] = mailed
                ? $"'{staff.FullName}' için yeni aktivasyon linki ve geçici şifre '{staff.Email}' adresine gönderildi."
                : $"'{staff.FullName}' için yeni ilk giriş daveti ve geçici şifre aşağıda oluşturuldu.";
            return RedirectToAction(nameof(Accounting), new { buildingId, tab = "personel" });
        }

        private async Task<bool> PrepareStaffInviteNotificationAsync(
            Building building,
            BuildingStaff staff,
            GMK360.Core.Entities.Identity.ApplicationUser user,
            string? tempPassword,
            bool sendEmail)
        {
            var activateUrl = Url.Action("ActivateResident", "Account", new { token = user.ActivationToken }, Request.Scheme) ?? "";
            var loginUrl = Url.Action("Login", "Account", null, Request.Scheme) ?? "";

            TempData["StaffActivationLink"] = activateUrl;
            TempData["StaffTempPassword"] = tempPassword ?? "";
            TempData["StaffInviteName"] = staff.FullName;
            TempData["StaffInviteEmail"] = !string.IsNullOrWhiteSpace(staff.Email) ? staff.Email : (user.Email ?? "");
            TempData["StaffInvitePhone"] = staff.PhoneNumber ?? user.PhoneNumber ?? "";

            if (!sendEmail || string.IsNullOrWhiteSpace(staff.Email) || staff.Email.EndsWith("@gmk360.local", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var emailSvc = HttpContext.RequestServices.GetService(typeof(IEmailService)) as IEmailService;
            if (emailSvc == null) return false;

            var encodedName = System.Net.WebUtility.HtmlEncode(staff.FullName);
            var encodedBuilding = System.Net.WebUtility.HtmlEncode(building.Name);
            var encodedRole = System.Net.WebUtility.HtmlEncode(staff.RoleType);
            var tempPwHtml = !string.IsNullOrWhiteSpace(tempPassword)
                ? $@"<p><strong>Geçici İlk Giriş Şifreniz:</strong> <code style=""background:#f1f5f9;padding:4px 8px;border-radius:6px;font-size:15px;"">{System.Net.WebUtility.HtmlEncode(tempPassword)}</code></p>"
                : "";

            var html = $@"
<div style=""font-family:Arial,sans-serif;max-width:560px;margin:0 auto;padding:20px;border:1px solid #e2e8f0;border-radius:12px;"">
  <h2 style=""color:#15803d;margin-top:0;"">GMK360 Personel İlk Giriş &amp; Özlük Kurulumu</h2>
  <p>Merhaba <strong>{encodedName}</strong>,</p>
  <p><strong>{encodedBuilding}</strong> yönetimi sizi <strong>{encodedRole}</strong> olarak GMK360 sistemine ekledi.</p>
  <p>Güvenliğiniz gereği giriş şifrenizi, yalnızca sizin bileceğiniz 4-6 haneli saha PIN kodunuzu ve ikametgah adresinizi ilk girişte kendiniz belirlersiniz.</p>
  {tempPwHtml}
  <p style=""margin:22px 0;"">
    <a href=""{activateUrl}"" style=""background:#15803d;color:#fff;text-decoration:none;padding:12px 24px;border-radius:999px;font-weight:bold;display:inline-block;"">Hesabımı Kur (Şifre, PIN &amp; Adres)</a>
  </p>
  <p style=""font-size:12px;color:#64748b;"">Dilerseniz <a href=""{loginUrl}"">{loginUrl}</a> üzerinden e-posta veya telefonunuz ve geçici şifrenizle giriş yaparak da kurulum ekranına geçebilirsiniz.</p>
</div>";

            var res = await emailSvc.SendEmailWithResultAsync(staff.Email, $"{building.Name} - GMK360 Personel İlk Giriş Daveti", html);
            TempData["StaffEmailSent"] = res.Success ? "true" : "false";
            return res.Success;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadBuildingStaffDocument(
            int buildingId,
            int buildingStaffId,
            string documentType,
            string? title,
            Microsoft.AspNetCore.Http.IFormFile? documentFile,
            string? physicalArchiveLocation,
            DateTime? issueDate,
            DateTime? expiryDate,
            string? notes)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var staff = await _context.BuildingStaffs.FirstOrDefaultAsync(s => s.Id == buildingStaffId && !s.IsDeleted);
            if (staff == null) return NotFound();
            if (await GetManagedBuildingAsync(staff.BuildingId) == null) return Forbid();

            string? savedUrl = null;
            if (documentFile != null && documentFile.Length > 0)
            {
                if (documentFile.Length > 15 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "Özlük belgesi boyutu en fazla 15 MB olabilir.";
                    return RedirectToAction(nameof(Accounting), new { buildingId, tab = "personel" });
                }

                var ext = System.IO.Path.GetExtension(documentFile.FileName)?.ToLowerInvariant() ?? "";
                var allowedExts = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".webp", ".doc", ".docx" };
                if (!allowedExts.Contains(ext))
                {
                    TempData["ErrorMessage"] = "Sadece PDF, JPG, PNG, WEBP veya Word (.doc/.docx) dosyaları yüklenebilir.";
                    return RedirectToAction(nameof(Accounting), new { buildingId, tab = "personel" });
                }

                var env = HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.Hosting.IWebHostEnvironment)) as Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
                var webRoot = env?.WebRootPath ?? System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
                var folder = System.IO.Path.Combine(webRoot, "uploads", "buildings", staff.BuildingId.ToString(), "staff-docs", staff.Id.ToString());
                System.IO.Directory.CreateDirectory(folder);

                var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}".Substring(0, 28) + ext;
                var fullPath = System.IO.Path.Combine(folder, fileName);
                using (var stream = new System.IO.FileStream(fullPath, System.IO.FileMode.Create))
                {
                    await documentFile.CopyToAsync(stream);
                }
                savedUrl = $"/uploads/buildings/{staff.BuildingId}/staff-docs/{staff.Id}/{fileName}";
            }

            if (string.IsNullOrWhiteSpace(savedUrl) && string.IsNullOrWhiteSpace(physicalArchiveLocation))
            {
                TempData["ErrorMessage"] = "Lütfen belge dosyası yükleyin veya fiziksel klasör / arşiv yerini belirtin.";
                return RedirectToAction(nameof(Accounting), new { buildingId, tab = "personel" });
            }

            var cleanType = string.IsNullOrWhiteSpace(documentType) ? "Diger" : documentType.Trim();
            var doc = new BuildingStaffDocument
            {
                BuildingStaffId = staff.Id,
                DocumentType = cleanType,
                Title = string.IsNullOrWhiteSpace(title) ? BuildingStaffDocument.GetLabel(cleanType) : title.Trim(),
                FileUrl = savedUrl,
                PhysicalArchiveLocation = string.IsNullOrWhiteSpace(physicalArchiveLocation) ? null : physicalArchiveLocation.Trim(),
                IssueDate = issueDate,
                ExpiryDate = expiryDate,
                IsVerified = true,
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
                UploadedByUserId = GetUserId(),
                CreatedAt = DateTime.UtcNow
            };

            _context.BuildingStaffDocuments.Add(doc);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"📁 '{staff.FullName}' özlük dosyasına '{doc.Title}' belgesi eklendi.";
            return RedirectToAction(nameof(Accounting), new { buildingId, tab = "personel" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBuildingStaffDocument(int buildingId, int documentId)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var doc = await _context.BuildingStaffDocuments
                .Include(d => d.Staff)
                .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);
            if (doc == null) return NotFound();
            if (await GetManagedBuildingAsync(doc.Staff.BuildingId) == null) return Forbid();

            doc.IsDeleted = true;
            doc.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{doc.Title}' özlük dosyasından kaldırıldı.";
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
            var complexIds = buildings.Where(b => b.HousingComplexId.HasValue).Select(b => b.HousingComplexId!.Value).Distinct().ToList();
            if (buildingId.HasValue)
            {
                var b = await GetManagedBuildingAsync(buildingId.Value);
                if (b == null) return Forbid();
                allowedIds = b.HousingComplexId.HasValue
                    ? buildings.Where(x => x.HousingComplexId == b.HousingComplexId).Select(x => x.Id).ToList()
                    : new System.Collections.Generic.List<int> { b.Id };
                complexIds = b.HousingComplexId.HasValue
                    ? new System.Collections.Generic.List<int> { b.HousingComplexId.Value }
                    : new System.Collections.Generic.List<int>();
            }

            var meetings = (allowedIds.Any() || complexIds.Any())
                ? await _context.SystemMeetings
                    .Include(m => m.Decisions)
                    .Include(m => m.Votes)
                    .Include(m => m.BoardVotes)
                    .Include(m => m.Attachments)
                    .Where(m => m.ContextType == "Building" && !m.IsDeleted
                        && m.WorkflowStage == "Decision"
                        && (allowedIds.Contains(m.ContextId)
                            || (m.IsSiteWide && m.HousingComplexId.HasValue && complexIds.Contains(m.HousingComplexId.Value))))
                    .OrderByDescending(m => m.MeetingDate)
                    .ToListAsync()
                : new System.Collections.Generic.List<SystemMeeting>();

            var allUnitsLoaded = allowedIds.Any()
                ? await _context.BuildingUnits
                    .AsNoTracking()
                    .Include(u => u.Building)
                    .Where(u => allowedIds.Contains(u.BuildingId) && !(u.Category ?? "").StartsWith("OrtakAlan"))
                    .OrderBy(u => u.Building.BlockName ?? u.Building.Name).ThenBy(u => u.FloorLevel).ThenBy(u => u.DoorNumber)
                    .ToListAsync()
                : new System.Collections.Generic.List<BuildingUnit>();

            var buildingUnitsMap = allUnitsLoaded
                .GroupBy(u => u.BuildingId)
                .ToDictionary(g => g.Key, g => g.ToList());
            var complexUnitsMap = allUnitsLoaded
                .Where(u => u.Building?.HousingComplexId != null)
                .GroupBy(u => u.Building!.HousingComplexId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());
            ViewBag.UnitsByBuilding = buildingUnitsMap;
            ViewBag.UnitsByComplex = complexUnitsMap;
            ViewBag.SelectedBuildingObj = buildingId.HasValue
                ? buildings.FirstOrDefault(b => b.Id == buildingId.Value)
                : buildings.FirstOrDefault();

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

            var openComplaints = allowedIds.Any()
                ? await _context.SupportTickets
                    .AsNoTracking()
                    .Where(t => t.ContextType == "Building" && t.ContextId.HasValue && allowedIds.Contains(t.ContextId.Value)
                        && t.Category != "ManagerTodo" && t.Status != "Closed")
                    .OrderByDescending(t => t.Id)
                    .Take(30)
                    .ToListAsync()
                : new System.Collections.Generic.List<SupportTicket>();

            var selectedBld = buildingId.HasValue
                ? buildings.FirstOrDefault(b => b.Id == buildingId.Value)
                : buildings.FirstOrDefault();
            int rosterRootId = selectedBld != null ? (selectedBld.ParentBuildingId ?? selectedBld.Id) : 0;
            var boardManagers = rosterRootId > 0
                ? await _context.BuildingManagers
                    .AsNoTracking()
                    .Include(m => m.User)
                    .Where(m => m.BuildingId == rosterRootId && m.IsActive)
                    .OrderBy(m => m.Role)
                    .ToListAsync()
                : new System.Collections.Generic.List<BuildingManager>();

            string FormatMgrName(GMK360.Core.Entities.Identity.ApplicationUser? usr) =>
                usr == null ? "" : (!string.IsNullOrWhiteSpace(usr.FirstName) ? $"{usr.FirstName} {usr.LastName}".Trim() : (usr.DisplayName ?? usr.Email ?? ""));

            string defaultChairName = boardManagers
                .Where(m => m.Role == BuildingManagerRole.HeadManager)
                .Select(m => FormatMgrName(m.User))
                .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? "";
            if (string.IsNullOrWhiteSpace(defaultChairName))
            {
                var curUid = GetUserId();
                if (!string.IsNullOrEmpty(curUid))
                {
                    var curU = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == curUid);
                    defaultChairName = FormatMgrName(curU);
                }
            }
            var azaNamesList = boardManagers
                .Where(m => m.Role == BuildingManagerRole.ViceChair || m.Role == BuildingManagerRole.BoardMember || m.Role == BuildingManagerRole.ReserveMember || m.Role == BuildingManagerRole.Auditor)
                .Select(m =>
                {
                    var nm = FormatMgrName(m.User);
                    if (string.IsNullOrWhiteSpace(nm)) return "";
                    return m.Role switch
                    {
                        BuildingManagerRole.ViceChair => $"{nm} (Bşk. Yrd.)",
                        BuildingManagerRole.ReserveMember => $"{nm} (Yedek)",
                        BuildingManagerRole.Auditor => $"{nm} (Denetçi)",
                        _ => nm
                    };
                })
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .ToList();

            var decisionScans = meetingIds.Any()
                ? await _context.DocumentArchives
                    .AsNoTracking()
                    .Where(d => !d.IsDeleted && d.Category == "KararDefteri" && d.SourceRecordId.HasValue && meetingIds.Contains(d.SourceRecordId.Value))
                    .OrderByDescending(d => d.Id)
                    .ToListAsync()
                : new System.Collections.Generic.List<DocumentArchive>();

            var boardPlan = GMK360.Web.Helpers.KmkBoardRecommendationHelper.BuildPlan(allUnitsLoaded.Count, selectedBld, boardManagers);

            ViewBag.Buildings = buildings;
            ViewBag.BuildingMap = buildings.ToDictionary(b => b.Id, b => b.Name);
            ViewBag.SelectedBuildingId = buildingId;
            ViewBag.SelectedBuildingObj = selectedBld;
            ViewBag.BoardRoster = boardManagers;
            ViewBag.BoardPlan = boardPlan;
            ViewBag.DefaultChairpersonName = defaultChairName;
            ViewBag.DefaultBoardMembersSummary = string.Join(", ", azaNamesList);
            ViewBag.ScansByMeeting = decisionScans.GroupBy(d => d.SourceRecordId!.Value).ToDictionary(g => g.Key, g => g.ToList());
            ViewBag.Approvals = approvals;
            ViewBag.QuotesByMeeting = meetingQuotes.GroupBy(q => q.SourceReferenceId).ToDictionary(g => g.Key, g => g.ToList());
            ViewBag.JobsByMeeting = jobsByMeeting;
            ViewBag.StaffsByMeeting = staffsByMeeting;
            ViewBag.AllSiteUnits = allUnitsLoaded;
            ViewBag.OpenComplaints = openComplaints;
            ViewBag.NextBookPage = (meetings.Where(m => m.BookPageNumber.HasValue).Select(m => m.BookPageNumber!.Value).DefaultIfEmpty(0).Max()) + 1;
            return View(meetings);
        }

        // 11b. Toplantı & Ön Görüşme Masası (Müzakere & Dijital Evrak Çantası)
        [HttpGet]
        public async Task<IActionResult> Toplantilar(int? buildingId, string tab = "open")
        {
            var buildings = await GetUserManagedRootBuildingsAsync();
            var allowedIds = buildings.Select(b => b.Id).ToList();
            var complexIds = buildings.Where(b => b.HousingComplexId.HasValue).Select(b => b.HousingComplexId!.Value).Distinct().ToList();
            if (buildingId.HasValue)
            {
                var b = await GetManagedBuildingAsync(buildingId.Value);
                if (b == null) return Forbid();
                allowedIds = b.HousingComplexId.HasValue
                    ? buildings.Where(x => x.HousingComplexId == b.HousingComplexId).Select(x => x.Id).ToList()
                    : new System.Collections.Generic.List<int> { b.Id };
                complexIds = b.HousingComplexId.HasValue
                    ? new System.Collections.Generic.List<int> { b.HousingComplexId.Value }
                    : new System.Collections.Generic.List<int>();
            }

            var allDiscussions = (allowedIds.Any() || complexIds.Any())
                ? await _context.SystemMeetings
                    .Include(m => m.Decisions)
                    .Include(m => m.Votes)
                    .Include(m => m.BoardVotes)
                    .Include(m => m.Attachments)
                    .Where(m => m.ContextType == "Building" && !m.IsDeleted
                        && (m.WorkflowStage == "Discussion" || m.WorkflowStage == "ArchivedNegative")
                        && (allowedIds.Contains(m.ContextId)
                            || (m.IsSiteWide && m.HousingComplexId.HasValue && complexIds.Contains(m.HousingComplexId.Value))))
                    .OrderByDescending(m => m.MeetingDate)
                    .ToListAsync()
                : new System.Collections.Generic.List<SystemMeeting>();

            bool isArchive = string.Equals(tab, "archive", StringComparison.OrdinalIgnoreCase);
            var openList = allDiscussions.Where(m => m.WorkflowStage == "Discussion").ToList();
            var archiveList = allDiscussions.Where(m => m.WorkflowStage == "ArchivedNegative").ToList();

            var allUnitsLoaded = allowedIds.Any()
                ? await _context.BuildingUnits
                    .AsNoTracking()
                    .Include(u => u.Building)
                    .Where(u => allowedIds.Contains(u.BuildingId) && !(u.Category ?? "").StartsWith("OrtakAlan"))
                    .OrderBy(u => u.Building.BlockName ?? u.Building.Name).ThenBy(u => u.FloorLevel).ThenBy(u => u.DoorNumber)
                    .ToListAsync()
                : new System.Collections.Generic.List<BuildingUnit>();

            var openComplaints = allowedIds.Any()
                ? await _context.SupportTickets
                    .AsNoTracking()
                    .Where(t => t.ContextType == "Building" && t.ContextId.HasValue && allowedIds.Contains(t.ContextId.Value)
                        && t.Category != "ManagerTodo" && t.Status != "Closed")
                    .OrderByDescending(t => t.Id)
                    .Take(30)
                    .ToListAsync()
                : new System.Collections.Generic.List<SupportTicket>();

            ViewBag.Buildings = buildings;
            ViewBag.BuildingMap = buildings.ToDictionary(b => b.Id, b => b.Name);
            ViewBag.SelectedBuildingId = buildingId;
            ViewBag.SelectedBuildingObj = buildingId.HasValue
                ? buildings.FirstOrDefault(b => b.Id == buildingId.Value)
                : buildings.FirstOrDefault();
            ViewBag.SelectedTab = isArchive ? "archive" : "open";
            ViewBag.OpenCount = openList.Count;
            ViewBag.ArchiveCount = archiveList.Count;
            ViewBag.UnitsByBuilding = allUnitsLoaded.GroupBy(u => u.BuildingId).ToDictionary(g => g.Key, g => g.ToList());
            ViewBag.UnitsByComplex = allUnitsLoaded.Where(u => u.Building?.HousingComplexId != null).GroupBy(u => u.Building!.HousingComplexId!.Value).ToDictionary(g => g.Key, g => g.ToList());
            ViewBag.AllSiteUnits = allUnitsLoaded;
            ViewBag.OpenComplaints = openComplaints;

            return View(isArchive ? archiveList : openList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDiscussionMeeting(
            int buildingId,
            string agendaTitle,
            DateTime meetingDate,
            string? meetingTime = "20:00",
            string? meetingLocation = null,
            string? invitationScope = "OwnersAndProxyTenants",
            string? decisionText = null,
            bool isSiteWide = true,
            Microsoft.AspNetCore.Http.IFormFile? attachmentFile = null,
            string? attachmentTitle = null)
        {
            var userId = GetUserId();
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            DateTime effectiveDate = meetingDate;
            if (!string.IsNullOrWhiteSpace(meetingTime) && TimeSpan.TryParse(meetingTime, out var parsedTime))
            {
                effectiveDate = meetingDate.Date.Add(parsedTime);
            }

            int yearCount = await _context.SystemMeetings.CountAsync(m =>
                m.ContextType == "Building" && m.ContextId == buildingId && m.WorkflowStage == "Discussion" && m.MeetingDate.Year == effectiveDate.Year && !m.IsDeleted);
            string tplNo = $"TPL-{effectiveDate.Year}/{(yearCount + 1):D2}";
            string cleanScope = string.IsNullOrWhiteSpace(invitationScope) ? "OwnersAndProxyTenants" : invitationScope.Trim();

            var meeting = new SystemMeeting
            {
                ContextType = "Building",
                ContextId = buildingId,
                IsSiteWide = isSiteWide && building.HousingComplexId.HasValue,
                HousingComplexId = building.HousingComplexId,
                AgendaTitle = agendaTitle.Trim(),
                DecisionNumber = tplNo,
                MeetingLocation = string.IsNullOrWhiteSpace(meetingLocation) ? "Site Sosyal Alan / Online Katılım" : meetingLocation.Trim(),
                QuorumType = "Standard",
                DecisionCategory = "General",
                WorkflowStage = "Discussion",
                InvitationScope = cleanScope,
                AutoExecuteOnQuorum = true,
                VerificationToken = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant(),
                MeetingDate = effectiveDate,
                IsConcluded = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.SystemMeetings.Add(meeting);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(decisionText))
            {
                _context.SystemMeetingDecisions.Add(new SystemMeetingDecision
                {
                    SystemMeetingId = meeting.Id,
                    DecisionText = decisionText.Trim(),
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            if (attachmentFile != null && attachmentFile.Length > 0)
            {
                var savedUrl = await SaveMeetingAttachmentFileAsync(buildingId, meeting.Id, attachmentFile);
                if (!string.IsNullOrEmpty(savedUrl))
                {
                    _context.BuildingDecisionAttachments.Add(new BuildingDecisionAttachment
                    {
                        SystemMeetingId = meeting.Id,
                        AuthorUserId = userId,
                        AuthorName = User.Identity?.Name ?? "Site Yöneticisi",
                        AuthorRoleLabel = "Yönetici",
                        DocumentTitle = string.IsNullOrWhiteSpace(attachmentTitle) ? attachmentFile.FileName : attachmentTitle.Trim(),
                        FileUrl = savedUrl,
                        CreatedAt = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                }
            }

            var inviteCounts = await SendMeetingInvitationNotificationsAsync(building, meeting, sendEmails: true);
            string notifyNote = inviteCounts.NotifiedCount > 0
                ? $" ({inviteCounts.NotifiedCount} kişiye bildirim, {inviteCounts.EmailedCount} kişiye davet e-postası gönderildi.)"
                : "";

            TempData["SuccessMessage"] = $"🤝 '{meeting.AgendaTitle}' ({tplNo}) toplantı masası açıldı ve davetli malik/vekil ekranlarına yansıtıldı.{notifyNote}";
            return RedirectToAction(nameof(Toplantilar), new { buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteMeetingToDecision(
            int meetingId,
            int buildingId,
            string? decisionCategory = "General",
            string? quorumType = "Standard",
            string? decisionText = null,
            string? proposedDues = null,
            int? paymentDay = null,
            string? targetName = null,
            string? targetEmail = null,
            string? targetPhone = null,
            string? targetRole = null,
            string? additionalCandidates = null,
            int? targetUnitId = null,
            string? exemptionMode = null,
            string? proposedMultiplier = null,
            string? proposedAdjustment = null,
            string? duesNote = null,
            int? relatedTicketId = null,
            string? estimatedBudget = null,
            bool autoExecuteOnQuorum = true,
            int? bookPageNumber = null,
            DateTime? sourceDocDate = null,
            string? sourceDocNumber = null,
            string? sourceDocSender = null,
            string? chairpersonName = null,
            string? boardMembersSummary = null,
            string? meetingEndTime = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var meeting = await _context.SystemMeetings
                .Include(m => m.Decisions)
                .FirstOrDefaultAsync(m => m.Id == meetingId && m.ContextType == "Building" && m.ContextId == buildingId && !m.IsDeleted);
            if (meeting == null) return NotFound();

            string cleanCategory = string.IsNullOrWhiteSpace(decisionCategory) ? "General" : decisionCategory.Trim();
            string cleanQuorum = string.IsNullOrWhiteSpace(quorumType) ? "Standard" : quorumType.Trim();
            if (string.Equals(cleanCategory, "BoardDecision", StringComparison.OrdinalIgnoreCase))
            {
                cleanQuorum = "BoardMajority";
                meeting.InvitationScope = "BoardOnly";
            }
            else if (string.Equals(cleanCategory, "ManagementPlan", StringComparison.OrdinalIgnoreCase))
            {
                cleanQuorum = "ManagementPlan";
            }
            else if (string.Equals(cleanCategory, "SecondAssembly", StringComparison.OrdinalIgnoreCase))
            {
                cleanQuorum = "SecondAssembly";
            }
            else if (string.Equals(cleanCategory, "UnitExemption", StringComparison.OrdinalIgnoreCase) && string.Equals(cleanQuorum, "Standard", StringComparison.OrdinalIgnoreCase))
            {
                cleanQuorum = "Unanimous";
            }
            else if (string.Equals(cleanCategory, "AppointManager", StringComparison.OrdinalIgnoreCase)
                || string.Equals(cleanCategory, "AppointBoardMember", StringComparison.OrdinalIgnoreCase)
                || string.Equals(cleanCategory, "CapitalProject", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(cleanQuorum, "Standard", StringComparison.OrdinalIgnoreCase))
                {
                    cleanQuorum = "Innovation";
                }
            }

            decimal? parsedDues = ParseFlexibleDecimal(proposedDues);
            decimal? parsedBudget = ParseFlexibleDecimal(estimatedBudget);
            decimal? parsedMult = ParseFlexibleDecimal(proposedMultiplier);
            decimal? parsedAdj = ParseFlexibleDecimal(proposedAdjustment);
            var proposalPayload = new DecisionProposalPayload
            {
                ProposedDues = parsedDues.HasValue && parsedDues.Value >= 0m ? Math.Round(parsedDues.Value, 2) : null,
                PaymentDay = paymentDay.HasValue ? Math.Clamp(paymentDay.Value, 1, 31) : null,
                TargetName = string.IsNullOrWhiteSpace(targetName) ? null : targetName.Trim(),
                TargetEmail = string.IsNullOrWhiteSpace(targetEmail) ? null : targetEmail.Trim(),
                TargetPhone = string.IsNullOrWhiteSpace(targetPhone) ? null : targetPhone.Trim(),
                TargetRole = string.IsNullOrWhiteSpace(targetRole) ? null : targetRole.Trim(),
                AdditionalCandidates = string.IsNullOrWhiteSpace(additionalCandidates) ? null : additionalCandidates.Trim(),
                TargetUnitId = targetUnitId,
                ExemptionMode = string.IsNullOrWhiteSpace(exemptionMode) ? null : exemptionMode.Trim(),
                ProposedMultiplier = parsedMult.HasValue && parsedMult.Value > 0m ? Math.Round(parsedMult.Value, 2) : null,
                ProposedAdjustment = parsedAdj.HasValue ? Math.Round(parsedAdj.Value, 2) : null,
                DuesNote = string.IsNullOrWhiteSpace(duesNote) ? null : duesNote.Trim(),
                RelatedTicketId = relatedTicketId,
                EstimatedBudget = parsedBudget.HasValue && parsedBudget.Value > 0m ? Math.Round(parsedBudget.Value, 2) : null
            };
            bool hasStructuredPayload = proposalPayload.ProposedDues.HasValue
                || !string.IsNullOrWhiteSpace(proposalPayload.TargetName)
                || !string.IsNullOrWhiteSpace(proposalPayload.TargetEmail)
                || !string.IsNullOrWhiteSpace(proposalPayload.TargetPhone)
                || proposalPayload.TargetUnitId.HasValue
                || proposalPayload.RelatedTicketId.HasValue
                || proposalPayload.EstimatedBudget.HasValue;

            int decisionSeq = await _context.SystemMeetings.CountAsync(m =>
                m.ContextType == "Building" && m.ContextId == buildingId && m.WorkflowStage == "Decision" && m.MeetingDate.Year == meeting.MeetingDate.Year && !m.IsDeleted) + 1;
            int nextPageNo = bookPageNumber
                ?? ((await _context.SystemMeetings
                    .Where(m => m.ContextType == "Building" && m.ContextId == buildingId && !m.IsDeleted && m.WorkflowStage == "Decision" && m.BookPageNumber.HasValue)
                    .MaxAsync(m => (int?)m.BookPageNumber) ?? 0) + 1);
            int rootIdProm = building.ParentBuildingId ?? building.Id;
            var promMgrs = await _context.BuildingManagers
                .AsNoTracking()
                .Include(m => m.User)
                .Where(m => m.BuildingId == rootIdProm && m.IsActive)
                .OrderBy(m => m.Role)
                .ToListAsync();
            string? defChairProm = promMgrs
                .Where(m => m.Role == BuildingManagerRole.HeadManager)
                .Select(m => !string.IsNullOrWhiteSpace(m.User?.FirstName) ? $"{m.User.FirstName} {m.User.LastName}".Trim() : (m.User?.DisplayName ?? m.User?.Email))
                .FirstOrDefault();
            string? defAzasProm = string.Join(", ", promMgrs
                .Where(m => m.Role == BuildingManagerRole.BoardMember || m.Role == BuildingManagerRole.Auditor)
                .Select(m => !string.IsNullOrWhiteSpace(m.User?.FirstName) ? $"{m.User.FirstName} {m.User.LastName}".Trim() : (m.User?.DisplayName ?? m.User?.Email))
                .Where(n => !string.IsNullOrWhiteSpace(n)));

            meeting.SourceDocNumber ??= meeting.DecisionNumber;
            meeting.SourceDocDate ??= meeting.MeetingDate;
            meeting.SourceDocSender ??= "Ön Görüşme & Müzakere Masası";
            meeting.WorkflowStage = "Decision";
            meeting.DecisionNumber = $"{meeting.MeetingDate.Year}/{decisionSeq:D2}";
            meeting.BookPageNumber = nextPageNo;
            meeting.ChairpersonName = !string.IsNullOrWhiteSpace(chairpersonName) ? chairpersonName.Trim() : (meeting.ChairpersonName ?? defChairProm);
            meeting.BoardMembersSummary = !string.IsNullOrWhiteSpace(boardMembersSummary) ? boardMembersSummary.Trim() : (meeting.BoardMembersSummary ?? (string.IsNullOrWhiteSpace(defAzasProm) ? null : defAzasProm));
            if (!string.IsNullOrWhiteSpace(meetingEndTime)) meeting.MeetingEndTime = meetingEndTime.Trim();
            meeting.DecisionCategory = cleanCategory;
            meeting.QuorumType = cleanQuorum;
            meeting.AutoExecuteOnQuorum = autoExecuteOnQuorum;
            meeting.ProposedPayloadJson = hasStructuredPayload ? System.Text.Json.JsonSerializer.Serialize(proposalPayload) : null;
            meeting.IsConcluded = false;
            meeting.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(decisionText))
            {
                _context.SystemMeetingDecisions.Add(new SystemMeetingDecision
                {
                    SystemMeetingId = meeting.Id,
                    DecisionText = decisionText.Trim(),
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            var inviteCounts = await SendMeetingInvitationNotificationsAsync(building, meeting, sendEmails: true);
            string notifyNote = inviteCounts.NotifiedCount > 0
                ? $" ({inviteCounts.NotifiedCount} malik/vekile oylama bildirimi, {inviteCounts.EmailedCount} kişiye e-posta gönderildi.)"
                : "";
            TempData["SuccessMessage"] = $"✅ Görüşme olumlu sonuçlandığı için resmi Karar Defteri & Oylama masasına aktarıldı (Karar #{meeting.DecisionNumber}). Tüm belge ve yorumlar korundu.{notifyNote}";
            return RedirectToAction(nameof(Oylamalar), new { buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseMeetingAsNegative(int meetingId, int buildingId, string? closingNote = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var meeting = await _context.SystemMeetings.FirstOrDefaultAsync(m =>
                m.Id == meetingId && m.ContextType == "Building" && m.ContextId == buildingId && !m.IsDeleted);
            if (meeting == null) return NotFound();

            meeting.WorkflowStage = "ArchivedNegative";
            meeting.IsConcluded = true;
            meeting.ClosingNote = string.IsNullOrWhiteSpace(closingNote)
                ? "Ön görüşme olumsuz sonuçlandı — Karar Defterine işlenmeden arşive kaldırıldı."
                : closingNote.Trim();
            meeting.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"🗄️ '{meeting.AgendaTitle}' konusu Karar Defterine alınmadan olumsuz sonuçla arşive kaldırıldı.";
            return RedirectToAction(nameof(Toplantilar), new { buildingId, tab = "archive" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SeedDemoResidentsForSite(int buildingId, bool includeAllSiteBlocks = true, int? selectedBuildingFilter = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            List<int> targetBuildingIds;
            if (includeAllSiteBlocks && building.HousingComplexId.HasValue)
            {
                targetBuildingIds = await _context.Buildings
                    .AsNoTracking()
                    .Where(b => b.HousingComplexId == building.HousingComplexId)
                    .Select(b => b.Id)
                    .ToListAsync();
            }
            else
            {
                targetBuildingIds = new System.Collections.Generic.List<int> { building.Id };
            }

            var emptyUnits = await _context.BuildingUnits
                .Where(u => targetBuildingIds.Contains(u.BuildingId)
                    && !(u.Category ?? "").StartsWith("OrtakAlan")
                    && string.IsNullOrWhiteSpace(u.OwnerName))
                .OrderBy(u => u.BuildingId)
                .ThenBy(u => u.FloorLevel)
                .ThenBy(u => u.DoorNumber)
                .ToListAsync();

            if (!emptyUnits.Any())
            {
                TempData["SuccessMessage"] = "Tüm bağımsız bölümlerde zaten malik bilgisi girili durumda.";
                return RedirectToAction(nameof(Sakinler), new { buildingId });
            }

            string[] ownerPool = new[]
            {
                "Ahmet Yılmaz", "Mehmet Demir", "Ayşe Kaya", "Fatma Şahin", "Mustafa Çelik",
                "Emine Yıldız", "Ali Koç", "Hatice Arslan", "Hüseyin Doğan", "Zehra Öztürk",
                "İbrahim Aydın", "Merve Özkan", "Ömer Kılıç", "Elif Aslan", "Yusuf Çetin",
                "Büşra Erdoğan", "İsmail Aksoy", "Esra Kurt", "Ramazan Özdemir", "Gamze Polat",
                "Serkan Şimşek", "Sevgi Yavuz", "Murat Tekin", "Gülşen Kaplan", "Kadir Korkmaz",
                "Derya Güneş", "Burak Acar", "Seda Taş", "Orhan Tunç", "Nermin Bulut"
            };
            string[] tenantPool = new[]
            {
                "Caner Vural", "Deniz Aktaş", "Eren Karataş", "Selin Yalçın", "Tolga Işık",
                "Gizem Yüksel", "Furkan Güler", "Pınar Sarı", "Volkan Keskin", "Melis Duman",
                "Barış Toprak", "Damla Köse", "Sinan Ünal", "Ceren Kılıç", "Onur Uçar",
                "Tuğçe Sönmez", "Hakan Mutlu", "Ece Ergün", "Berkay Altun", "İrem Özer"
            };

            int ownerOccupiedCount = 0;
            int tenantOccupiedCount = 0;

            for (int i = 0; i < emptyUnits.Count; i++)
            {
                var u = emptyUnits[i];
                bool isTenantOccupied;

                if (i < 5)
                {
                    // İlk 5 boş daire tek malik ("Hasan Karahan"): 1'i kendi oturuyor, 4'ü kiracılı
                    u.OwnerName = "Hasan Karahan";
                    u.OwnerPhone = "05329010001";
                    u.OwnerEmail = "hasan.karahan@gmk360.local";
                    isTenantOccupied = i > 0;
                }
                else if (i < 7)
                {
                    // İkinci çoklu malik ("Zeynep Aydın"): 2 daire (1'i oturan + 1'i kiracılı)
                    u.OwnerName = "Zeynep Aydın";
                    u.OwnerPhone = "05329020002";
                    u.OwnerEmail = "zeynep.aydin@gmk360.local";
                    isTenantOccupied = (i == 6);
                }
                else
                {
                    var name = ownerPool[(i - 7) % ownerPool.Length];
                    int suffix = ((i - 7) / ownerPool.Length);
                    u.OwnerName = suffix > 0 ? $"{name} ({suffix + 1})" : name;
                    u.OwnerPhone = $"0532{(1100000 + u.Id * 7):D7}";
                    u.OwnerEmail = $"malik{u.Id}@gmk360.local";
                    // Kalanlarda yaklaşık %60 malik kendi oturuyor (i % 5 >= 3 ise kiracılı)
                    isTenantOccupied = (i % 5) >= 3;
                }

                u.IsEmpty = false;
                if (isTenantOccupied)
                {
                    var tName = tenantPool[i % tenantPool.Length];
                    u.TenantName = $"{tName}";
                    u.TenantPhone = $"0542{(2200000 + u.Id * 9):D7}";
                    u.TenantEmail = $"kiraci{u.Id}@gmk360.local";
                    tenantOccupiedCount++;
                }
                else
                {
                    u.TenantName = null;
                    u.TenantPhone = null;
                    u.TenantEmail = null;
                    ownerOccupiedCount++;
                }

                await SyncResidentUsersAndPeriodsAsync(u);
            }

            await _context.SaveChangesAsync();
            int ownPct = (int)Math.Round(ownerOccupiedCount * 100.0 / emptyUnits.Count);
            TempData["SuccessMessage"] = $"⚡ {emptyUnits.Count} boş bölüm örnek verilerle dolduruldu: {ownerOccupiedCount} kendi oturan malik (~%{ownPct}), {tenantOccupiedCount} kiracılı (Hasan Karahan için 5 daire: 1 oturan + 4 kiracılı tanımlandı).";
            return selectedBuildingFilter.HasValue
                ? RedirectToAction(nameof(Sakinler), new { buildingId = selectedBuildingFilter.Value })
                : RedirectToAction(nameof(Sakinler));
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
            string? email = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var meeting = await _context.SystemMeetings
                .FirstOrDefaultAsync(m => m.Id == meetingId && m.ContextType == "Building" && m.ContextId == buildingId);
            if (meeting == null) return NotFound();

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phoneNumber))
            {
                TempData["ErrorMessage"] = "Yetkili hesabı açmak için ad soyad ve telefon numarası zorunludur.";
                return RedirectToAction(nameof(Oylamalar), new { buildingId });
            }

            var cleanRole = string.IsNullOrWhiteSpace(roleTitle) ? "Karar Yetkilisi / Usta" : roleTitle.Trim();
            var staff = new BuildingStaff
            {
                BuildingId = buildingId,
                FullName = fullName.Trim(),
                PhoneNumber = phoneNumber.Trim(),
                Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
                QuickPinHash = null,
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

            var dispatchSvc = HttpContext.RequestServices.GetService(typeof(FacilityTaskDispatchService)) as FacilityTaskDispatchService;
            if (dispatchSvc != null)
            {
                var prov = await dispatchSvc.ProvisionStaffAccountAsync(staff, GetUserId(), resetTempPassword: true);
                if (prov.User != null)
                {
                    await PrepareStaffInviteNotificationAsync(building, staff, prov.User, prov.TempPassword, sendEmail: true);
                }
            }

            TempData["SuccessMessage"] = $"🔑 '{staff.FullName}' için Karar #{meeting.Id} kapsamında geçici şifre / ilk giriş daveti oluşturuldu. Kendi şifresini ve PIN kodunu girişte belirleyecektir.";
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

        // 13. Yasal Prosedürler & Yönetici El Kitabı (Merkezden Güncellenen Salt Okunur Kitap Modülü)
        [HttpGet]
        public async Task<IActionResult> MevzuatRehberi(string? slug = null, string? q = null)
        {
            await KmkGuideSeederService.EnsureKmkGuidesSeededAsync(_context);

            var allChapters = await _context.Articles
                .AsNoTracking()
                .Where(a => a.IsPublished)
                .OrderBy(a => a.IsMembersOnly)
                .ThenBy(a => a.PublishedAt ?? a.CreatedAt)
                .ToListAsync();

            var filteredChapters = allChapters;
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                filteredChapters = allChapters
                    .Where(a =>
                        a.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        (a.SeoTags != null && a.SeoTags.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                        a.Category.Contains(term, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var activeChapter = !string.IsNullOrWhiteSpace(slug)
                ? allChapters.FirstOrDefault(a => string.Equals(a.Slug, slug, StringComparison.OrdinalIgnoreCase))
                : filteredChapters.FirstOrDefault() ?? allChapters.FirstOrDefault();

            string? prevSlug = null;
            string? nextSlug = null;
            if (activeChapter != null && allChapters.Count > 1)
            {
                int idx = allChapters.FindIndex(a => a.Id == activeChapter.Id);
                if (idx > 0) prevSlug = allChapters[idx - 1].Slug;
                if (idx >= 0 && idx < allChapters.Count - 1) nextSlug = allChapters[idx + 1].Slug;
            }

            ViewBag.AllChapters = filteredChapters;
            ViewBag.TotalChapterCount = allChapters.Count;
            ViewBag.SearchQuery = q ?? "";
            ViewBag.PrevSlug = prevSlug;
            ViewBag.NextSlug = nextSlug;
            ViewBag.PrevTitle = prevSlug != null ? allChapters.FirstOrDefault(x => x.Slug == prevSlug)?.Title : null;
            ViewBag.NextTitle = nextSlug != null ? allChapters.FirstOrDefault(x => x.Slug == nextSlug)?.Title : null;
            return View(activeChapter);
        }

        private async Task<(int NotifiedCount, int EmailedCount)> SendMeetingInvitationNotificationsAsync(
            Building building,
            SystemMeeting meeting,
            bool sendEmails = true)
        {
            var scopedBuildingIds = new System.Collections.Generic.List<int> { building.Id };
            if (meeting.IsSiteWide && building.HousingComplexId.HasValue)
            {
                scopedBuildingIds = await _context.Buildings
                    .AsNoTracking()
                    .Where(b => b.HousingComplexId == building.HousingComplexId.Value)
                    .Select(b => b.Id)
                    .ToListAsync();
            }

            var scopedUnits = await _context.BuildingUnits
                .AsNoTracking()
                .Where(u => scopedBuildingIds.Contains(u.BuildingId) && !(u.Category ?? "").StartsWith("OrtakAlan"))
                .ToListAsync();

            var recipientUserIds = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var u in scopedUnits)
            {
                if (!string.IsNullOrWhiteSpace(u.OwnerUserId)) recipientUserIds.Add(u.OwnerUserId);
            }

            int rootBuildingId = building.ParentBuildingId ?? building.Id;
            var boardUserIds = await _context.BuildingManagers
                .AsNoTracking()
                .Where(m => m.BuildingId == rootBuildingId && m.IsActive && !string.IsNullOrWhiteSpace(m.UserId))
                .Select(m => m.UserId)
                .ToListAsync();
            foreach (var bid in boardUserIds) recipientUserIds.Add(bid);

            if (string.Equals(meeting.InvitationScope, "AllResidents", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var u in scopedUnits.Where(x => !x.IsEmpty && !string.IsNullOrWhiteSpace(x.TenantUserId)))
                {
                    recipientUserIds.Add(u.TenantUserId!);
                }
            }
            else if (string.Equals(meeting.InvitationScope, "OwnersAndProxyTenants", StringComparison.OrdinalIgnoreCase))
            {
                var proxyUnitIds = await _context.BuildingDecisionVotes
                    .AsNoTracking()
                    .Where(v => v.SystemMeetingId == meeting.Id && v.ParticipationType == "Vekil" && !v.IsDeleted)
                    .Select(v => v.BuildingUnitId)
                    .ToListAsync();
                foreach (var u in scopedUnits.Where(x => proxyUnitIds.Contains(x.Id) && !string.IsNullOrWhiteSpace(x.TenantUserId)))
                {
                    recipientUserIds.Add(u.TenantUserId!);
                }
            }

            if (!recipientUserIds.Any()) return (0, 0);

            var users = await _context.Users
                .AsNoTracking()
                .Where(u => recipientUserIds.Contains(u.Id))
                .ToListAsync();

            bool isDiscussion = string.Equals(meeting.WorkflowStage, "Discussion", StringComparison.OrdinalIgnoreCase);
            string stageLabel = isDiscussion ? "Toplantı Çağrısı" : "Resmi Karar / Oylama Çağrısı";
            string targetUrl = $"/FacilityPortal/Index?buildingId={building.Id}#meetingCard_{meeting.Id}";
            string scopeDesc = meeting.IsSiteWide && building.HousingComplex != null
                ? $"{building.HousingComplex.Name} (Site Geneli)"
                : $"{building.Name} ({building.BlockName ?? "Bina"})";

            int notified = 0;
            foreach (var usr in users)
            {
                _context.UserNotifications.Add(new UserNotification
                {
                    ApplicationUserId = usr.Id,
                    Title = $"📅 {stageLabel}: {meeting.AgendaTitle}",
                    Message = $"{scopeDesc} • {meeting.MeetingDate:dd.MM.yyyy HH:mm} • {meeting.MeetingLocation ?? "Ortak Alan / Online"}",
                    IsRead = false,
                    LinkUrl = targetUrl,
                    CreatedAt = DateTime.UtcNow
                });
                notified++;
            }
            await _context.SaveChangesAsync();

            int emailed = 0;
            if (sendEmails)
            {
                var emailSvc = HttpContext.RequestServices.GetService<IEmailService>();
                if (emailSvc != null)
                {
                    string fullUrl = $"{Request.Scheme}://{Request.Host}{targetUrl}";
                    string subject = $"GMK360 | {scopeDesc} — {stageLabel}: {meeting.AgendaTitle}";

                    foreach (var usr in users.Where(u => !string.IsNullOrWhiteSpace(u.Email) && !u.Email.EndsWith("@gmk360.local", StringComparison.OrdinalIgnoreCase)))
                    {
                        string displayName = !string.IsNullOrWhiteSpace(usr.FirstName) ? $"{usr.FirstName} {usr.LastName}".Trim() : usr.Email!;
                        string body = $@"
<div style=""font-family:Arial,sans-serif;max-width:580px;margin:0 auto;padding:20px;border:1px solid #e2e8f0;border-radius:14px;background:#ffffff;"">
  <span style=""display:inline-block;background:#eff6ff;color:#1d4ed8;font-size:12px;font-weight:bold;padding:4px 10px;border-radius:999px;margin-bottom:8px;"">{System.Net.WebUtility.HtmlEncode(stageLabel)} • {System.Net.WebUtility.HtmlEncode(scopeDesc)}</span>
  <h2 style=""color:#0f172a;margin:4px 0 12px;"">{System.Net.WebUtility.HtmlEncode(meeting.AgendaTitle)}</h2>
  <p>Sayın <strong>{System.Net.WebUtility.HtmlEncode(displayName)}</strong>,</p>
  <p>Tarafınızın katılımına/görüşüne açık yeni bir {(isDiscussion ? "site/blok toplantı gündemi" : "resmi karar defteri ön oylaması")} başlatılmıştır:</p>
  <table style=""width:100%;border-collapse:collapse;background:#f8fafc;border-radius:8px;margin:14px 0;font-size:14px;"">
    <tr><td style=""padding:8px 12px;color:#64748b;"">Tarih &amp; Saat:</td><td style=""padding:8px 12px;font-weight:bold;"">{meeting.MeetingDate:dd.MM.yyyy HH:mm}</td></tr>
    <tr><td style=""padding:8px 12px;color:#64748b;"">Yer / Bağlantı:</td><td style=""padding:8px 12px;"">{System.Net.WebUtility.HtmlEncode(meeting.MeetingLocation ?? "Site Sosyal Alan / Online")}</td></tr>
    <tr><td style=""padding:8px 12px;color:#64748b;"">Evrak / Sıra No:</td><td style=""padding:8px 12px;"">{System.Net.WebUtility.HtmlEncode(meeting.DecisionNumber ?? $"#{meeting.Id}")}</td></tr>
  </table>
  <p style=""margin:20px 0;"">
    <a href=""{fullUrl}"" style=""background:#1d4ed8;color:#fff;text-decoration:none;padding:12px 22px;border-radius:999px;font-weight:bold;display:inline-block;"">Toplantı Masasına Git &amp; Görüş / Onay Ver</a>
  </p>
  <p style=""font-size:12px;color:#64748b;"">Giriş yaptıktan sonra mobil uygulama (/FacilityPortal) üzerinden belgeleri inceleyebilir, görüş yazabilir veya ön onayınızı bildirebilirsiniz.</p>
</div>";
                        try
                        {
                            var res = await emailSvc.SendEmailWithResultAsync(usr.Email!, subject, body);
                            if (res.Success) emailed++;
                        }
                        catch { }
                    }
                }
            }

            return (notified, emailed);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMeetingCallReminders(int meetingId, int buildingId)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            var meeting = await _context.SystemMeetings
                .FirstOrDefaultAsync(m => m.Id == meetingId && m.ContextType == "Building" && m.ContextId == buildingId && !m.IsDeleted);
            if (meeting == null) return NotFound();

            var res = await SendMeetingInvitationNotificationsAsync(building, meeting, sendEmails: true);
            TempData[res.NotifiedCount > 0 ? "SuccessMessage" : "WarningMessage"] = res.NotifiedCount > 0
                ? $"📣 '{meeting.AgendaTitle}' için {res.NotifiedCount} yetkili üyeye portal bildirimi, {res.EmailedCount} kişiye e-posta toplantı çağrısı gönderildi."
                : "Kapsam içinde kayıtlı kullanıcı profili bulunamadı (Malik/Kiracılara e-posta adresi tanımladığınızdan emin olun).";

            return string.Equals(meeting.WorkflowStage, "Discussion", StringComparison.OrdinalIgnoreCase)
                ? RedirectToAction(nameof(Toplantilar), new { buildingId })
                : RedirectToAction(nameof(Oylamalar), new { buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveEmergencySafetyInfo(
            int buildingId,
            string? emergencyAssemblyPoint,
            string? fireEquipmentLocations,
            string? utilityShutoffLocations,
            string? emergencyContactsNote,
            Microsoft.AspNetCore.Http.IFormFile? emergencyMapFile = null)
        {
            var building = await GetManagedBuildingAsync(buildingId);
            if (building == null) return Forbid();

            building.EmergencyAssemblyPoint = string.IsNullOrWhiteSpace(emergencyAssemblyPoint) ? null : emergencyAssemblyPoint.Trim();
            building.FireEquipmentLocations = string.IsNullOrWhiteSpace(fireEquipmentLocations) ? null : fireEquipmentLocations.Trim();
            building.UtilityShutoffLocations = string.IsNullOrWhiteSpace(utilityShutoffLocations) ? null : utilityShutoffLocations.Trim();
            building.EmergencyContactsNote = string.IsNullOrWhiteSpace(emergencyContactsNote) ? null : emergencyContactsNote.Trim();

            if (emergencyMapFile != null && emergencyMapFile.Length > 0 && emergencyMapFile.Length <= 15 * 1024 * 1024)
            {
                var ext = System.IO.Path.GetExtension(emergencyMapFile.FileName)?.ToLowerInvariant() ?? "";
                var allowed = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };
                if (allowed.Contains(ext))
                {
                    var env = HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.Hosting.IWebHostEnvironment)) as Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
                    var webRoot = env?.WebRootPath ?? System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
                    var folder = System.IO.Path.Combine(webRoot, "uploads", "buildings", buildingId.ToString(), "safety");
                    System.IO.Directory.CreateDirectory(folder);
                    var fileName = $"safety_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
                    var fullPath = System.IO.Path.Combine(folder, fileName);
                    using (var stream = new System.IO.FileStream(fullPath, System.IO.FileMode.Create))
                    {
                        await emergencyMapFile.CopyToAsync(stream);
                    }
                    building.EmergencyMapUrl = $"/uploads/buildings/{buildingId}/safety/{fileName}";
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "🆘 Acil durum toplanma yeri, yangın dolabı/hortumu ve ana vana bilgileri kaydedildi; tüm sakinlerin telefon portalında anında görünür.";
            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }
    }
}
