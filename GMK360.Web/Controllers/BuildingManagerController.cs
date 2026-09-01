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

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // 1. Yönettiğim Binalar
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var buildings = await _context.Buildings
                .Where(b => b.ManagerUserId == userId)
                .Include(b => b.Units)
                .ToListAsync();

            if (buildings.Count == 1)
            {
                var building = buildings.First();
                if (building.OnboardingStep < 3)
                {
                    return RedirectToAction(nameof(Onboarding), new { id = building.Id });
                }
                return RedirectToAction(nameof(Detail), new { id = building.Id });
            }

            return View(buildings);
        }

        // 2. Yeni Bina Ekle (Artık SPA Onboarding'e yönlendirebiliriz)
        [HttpPost]
        public async Task<IActionResult> CreateBuilding(string name, string address)
        {
            var building = new Building
            {
                Name = name,
                Address = address,
                ManagerUserId = GetUserId(),
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
            Building building = null;

            if (id.HasValue)
            {
                building = await _context.Buildings
                    .Include(b => b.Units)
                    .FirstOrDefaultAsync(b => b.Id == id && b.ManagerUserId == userId);
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
            var userId = GetUserId();
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == buildingId && b.ManagerUserId == userId);
            
            if (building == null) return Json(new { success = false, message = "Bina bulunamadı." });

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
            var userId = GetUserId();
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == buildingId && b.ManagerUserId == userId);
            
            if (building == null) return Json(new { success = false, message = "Bina bulunamadı." });

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
                .FirstOrDefaultAsync(b => b.Id == id && b.ManagerUserId == userId);

            if (building == null) return NotFound();

            return View(building);
        }

        // 4. Daire / Bölüm Ekle
        [HttpPost]
        public async Task<IActionResult> AddUnit(int buildingId, string unitNumber, string ownerName, string ownerPhone, string tenantName, string tenantPhone, bool isEmpty)
        {
            var unit = new BuildingUnit
            {
                BuildingId = buildingId,
                UnitNumber = unitNumber,
                OwnerName = ownerName,
                OwnerPhone = ownerPhone,
                TenantName = tenantName,
                TenantPhone = tenantPhone,
                IsEmpty = isEmpty
            };

            _context.BuildingUnits.Add(unit);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        // 5. Gider Ekle ve Dağıt
        [HttpPost]
        public async Task<IActionResult> AddExpense(int buildingId, string description, decimal totalAmount, string payerType)
        {
            // Gideri oluştur
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

            // Binadaki tüm dairelere eşit paylaştır (Boş dairelerin durumu PayerType'a göre yönetilebilir ama şimdilik standart)
            var units = await _context.BuildingUnits.Where(u => u.BuildingId == buildingId).ToListAsync();
            
            if (units.Any())
            {
                decimal amountPerUnit = totalAmount / units.Count;

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

            return RedirectToAction(nameof(Detail), new { id = buildingId });
        }

        // 6. Ödeme Alındı İşaretle
        [HttpPost]
        public async Task<IActionResult> MarkDebtAsPaid(int debtId, string paidBy)
        {
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
            var query = _context.BuildingUnits.Include(u => u.Building).Where(u => u.Building.ManagerUserId == userId);
            if (buildingId.HasValue) query = query.Where(u => u.BuildingId == buildingId.Value);
            
            var units = await query.ToListAsync();
            ViewBag.Buildings = await _context.Buildings.Where(b => b.ManagerUserId == userId).ToListAsync();
            ViewBag.SelectedBuildingId = buildingId;
            return View(units);
        }

        // 8. Aidat ve Borç Takibi
        public async Task<IActionResult> Aidatlar(int? buildingId)
        {
            var userId = GetUserId();
            var query = _context.UnitDebts
                .Include(d => d.BuildingUnit).ThenInclude(u => u.Building)
                .Include(d => d.BuildingExpense)
                .Where(d => d.BuildingUnit.Building.ManagerUserId == userId);
                
            if (buildingId.HasValue) query = query.Where(d => d.BuildingUnit.BuildingId == buildingId.Value);
            
            var debts = await query.OrderByDescending(d => d.Id).ToListAsync();
            ViewBag.Buildings = await _context.Buildings.Where(b => b.ManagerUserId == userId).ToListAsync();
            ViewBag.SelectedBuildingId = buildingId;
            return View(debts);
        }

        // 9. Giderler
        public async Task<IActionResult> Giderler(int? buildingId)
        {
            var userId = GetUserId();
            var query = _context.BuildingExpenses.Include(e => e.Building).Where(e => e.Building.ManagerUserId == userId);
            
            if (buildingId.HasValue) query = query.Where(e => e.BuildingId == buildingId.Value);
            
            var expenses = await query.OrderByDescending(e => e.Date).ToListAsync();
            ViewBag.Buildings = await _context.Buildings.Where(b => b.ManagerUserId == userId).ToListAsync();
            ViewBag.SelectedBuildingId = buildingId;
            return View(expenses);
        }

        // 7. İç Talepler (Ticket) Yönetimi
        public async Task<IActionResult> TicketManagement(int buildingId)
        {
            var userId = GetUserId();
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == buildingId && b.ManagerUserId == userId);
            if (building == null) return Unauthorized();

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

            if (ticket == null || ticket.AssignedToUserId != userId)
                return Unauthorized();

            ticket.Status = status;
            ticket.UpdatedAt = DateTime.Now;
            
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(TicketManagement), new { buildingId = ticket.ContextId });
        }
    }
}
