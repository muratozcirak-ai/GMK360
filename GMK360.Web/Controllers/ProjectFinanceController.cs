using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Construction;
using System.Collections.Generic;

namespace GMK360.Web.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "InsaatFirmasi,Admin,Corporate")]
    public class ProjectFinanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectFinanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("ProjectFinance/Index/{projectId}")]
        public async Task<IActionResult> Index(int projectId)
        {
            var project = await _context.ConstructionProjects
                .Include(p => p.Blocks)
                .Include(p => p.BudgetItems)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return NotFound();

            ViewData["ProjectId"] = project.Id;
            ViewData["ProjectName"] = project.Name;
            
            ViewBag.StartDate = project.StartDate.ToString("dd.MM.yyyy");
            ViewBag.EndDate = project.EndDate?.ToString("dd.MM.yyyy") ?? "Belirtilmedi";
            
            string duration = "-";
            if (project.EndDate.HasValue) 
            {
                var months = ((project.EndDate.Value.Year - project.StartDate.Year) * 12) 
                             + project.EndDate.Value.Month - project.StartDate.Month;
                duration = months > 0 ? $"{months} Ay" : "Tamamlanmak Üzere";
            }
            ViewBag.Duration = duration;
            ViewBag.Address = !string.IsNullOrEmpty(project.Address) ? project.Address : "Adres girilmedi";

            int givenFlats = 0;
            int givenShops = 0;
            int newTotalFlats = 0;
            int newTotalShops = 0;

            if (project.Blocks != null && project.Blocks.Any())
            {
                givenFlats = project.Blocks.Where(b => b.IsExistingBuilding).Sum(b => b.TotalApartments);
                givenShops = project.Blocks.Where(b => b.IsExistingBuilding).Sum(b => b.TotalShops);

                newTotalFlats = project.Blocks.Where(b => !b.IsExistingBuilding).Sum(b => b.TotalApartments);
                newTotalShops = project.Blocks.Where(b => !b.IsExistingBuilding).Sum(b => b.TotalShops);
            }

            int leftFlats = Math.Max(0, newTotalFlats - givenFlats);
            int leftShops = Math.Max(0, newTotalShops - givenShops);

            // Yeni DB Alanları üzerinden dinamik hesaplama
            decimal estimatedValue = (leftFlats * project.AverageFlatSalePrice) + project.ExpectedTotalShopRevenue;

            ViewBag.GivenFlats = givenFlats;
            ViewBag.GivenShops = givenShops;
            ViewBag.LeftFlats = leftFlats;
            ViewBag.LeftShops = leftShops;
            
            ViewBag.AvgFlatPrice = project.AverageFlatSalePrice;
            ViewBag.TotalShopRevenue = project.ExpectedTotalShopRevenue;
            ViewBag.EstimatedValue = estimatedValue.ToString("N2") + " ₺";

            // Bütçe Aşama Verileri
            var budgetItems = project.BudgetItems?.ToList() ?? new List<ConstructionBudgetItem>();
            
            // Faz 0 Evraklarını Sanal Bütçe Kalemi Olarak Ekle
            var phase0Docs = await _context.ProjectLegalDocuments
                .Where(d => d.ConstructionProjectId == projectId)
                .ToListAsync();
            
            var phase0BudgetItems = phase0Docs.Select(d => new GMK360.Core.Entities.Construction.ConstructionBudgetItem
            {
                ItemName = d.DocumentName,
                PlannedUnitPrice = (d.EstimatedCost ?? 0) + (d.DocumentFee ?? 0) + (d.AdditionalCost ?? 0),
                Quantity = 1,
                ActualTotalCost = d.ActualCost ?? 0,
                PhaseCategory = GMK360.Core.Entities.Construction.BudgetPhaseCategory.ResmiEvraklarVeProsedurler
            }).ToList();
            
            budgetItems.AddRange(phase0BudgetItems);
            var phaseData = new Dictionary<BudgetPhaseCategory, dynamic>();
            
            foreach (BudgetPhaseCategory phase in Enum.GetValues(typeof(BudgetPhaseCategory)))
            {
                var itemsInPhase = budgetItems.Where(b => b.PhaseCategory == phase).ToList();
                decimal planned = itemsInPhase.Sum(x => x.PlannedTotalCost);
                decimal actual = itemsInPhase.Sum(x => x.ActualTotalCost);
                decimal diff = actual - planned;
                
                phaseData.Add(phase, new {
                    Planned = planned,
                    Actual = actual,
                    Diff = diff,
                    Items = itemsInPhase
                });
            }

            ViewBag.PhaseData = phaseData;
            ViewBag.TotalPlanned = budgetItems.Sum(x => x.PlannedTotalCost);
            ViewBag.TotalActual = budgetItems.Sum(x => x.ActualTotalCost);
            ViewBag.TotalDiff = ViewBag.TotalActual - ViewBag.TotalPlanned;

            return View();
        }

        // --- BUTTONSUZ KAYIT ICIN API ENDPOINT ---
        public class UpdateExpectationDto
        {
            public int ProjectId { get; set; }
            public decimal AvgFlatPrice { get; set; }
            public decimal TotalShopRevenue { get; set; }
        }

        [IgnoreAntiforgeryToken]
        [HttpPost("ProjectFinance/UpdateExpectations")]
        public async Task<IActionResult> UpdateExpectations([FromBody] UpdateExpectationDto dto)
        {
            var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == dto.ProjectId);
            if (project == null) return NotFound();

            project.AverageFlatSalePrice = dto.AvgFlatPrice;
            project.ExpectedTotalShopRevenue = dto.TotalShopRevenue;
            
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
