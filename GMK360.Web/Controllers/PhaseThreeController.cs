using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Construction;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GMK360.Web.Controllers
{
    public class PhaseThreeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PhaseThreeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("PhaseThree/Index/{projectId}")]
        public async Task<IActionResult> Index(int projectId)
        {
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project == null) return NotFound();

            ViewData["ProjectId"] = project.Id;
            ViewData["ProjectName"] = project.Name;

            var items = await _context.ConstructionBudgetItems
                .Where(b => b.ConstructionProjectId == projectId && b.PhaseCategory == BudgetPhaseCategory.KabaInsaatKarkas)
                .ToListAsync();

            var templates = await _context.SystemPhaseTemplates.Where(t => t.PhaseCategory == BudgetPhaseCategory.KabaInsaatKarkas).ToListAsync();
            ViewData["HasTemplates"] = templates.Any();

            var itemIds = items.Select(i => i.Id).ToList();
            var quoteRequests = await _context.B2BQuoteRequests
                .Include(q => q.Invites).ThenInclude(i => i.NetworkContact)
                .Where(q => q.SourceModule == "PhaseThreeBudget" && itemIds.Contains(q.SourceReferenceId))
                .ToListAsync();
            ViewBag.Quotes = quoteRequests;

            // Sadece Fizibilite Aşaması İçin Listeyi Gönderiyoruz
            return View(items);
        }

        [HttpPost("PhaseThree/AddItem")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AddItem(int projectId, string subCategory, string itemName, decimal plannedUnitPrice)
        {
            var item = new ConstructionBudgetItem
            {
                ConstructionProjectId = projectId,
                PhaseCategory = BudgetPhaseCategory.KabaInsaatKarkas,
                SubCategory = subCategory,
                ItemName = itemName,
                PlannedUnitPrice = plannedUnitPrice,
                Quantity = 1,
                Unit = "Adet",
                SourceType = BudgetItemSourceType.Manual,
                QuoteStatus = BudgetQuoteStatus.WaitingForPrice
            };
            _context.ConstructionBudgetItems.Add(item);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { projectId = projectId });
        }

        [HttpPost("PhaseThree/SyncFromPool/{projectId}")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SyncFromPool(int projectId)
        {
            var existingItems = await _context.ConstructionBudgetItems
                .Where(b => b.ConstructionProjectId == projectId && b.PhaseCategory == BudgetPhaseCategory.KabaInsaatKarkas)
                .Select(b => b.ItemName)
                .ToListAsync();

            var templates = await _context.SystemPhaseTemplates
                .Where(t => t.PhaseCategory == BudgetPhaseCategory.KabaInsaatKarkas && !existingItems.Contains(t.ItemName))
                .ToListAsync();

            foreach(var t in templates)
            {
                var item = new ConstructionBudgetItem
                {
                    ConstructionProjectId = projectId,
                    PhaseCategory = BudgetPhaseCategory.KabaInsaatKarkas,
                    SubCategory = t.SubCategory,
                    ItemName = t.ItemName,
                    Quantity = 1,
                    Unit = "Adet",
                    SourceType = BudgetItemSourceType.Manual,
                    QuoteStatus = t.IsQuoteRequired ? BudgetQuoteStatus.WaitingForPrice : BudgetQuoteStatus.EstimatedOrQuoted
                };
                _context.ConstructionBudgetItems.Add(item);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { projectId = projectId });
        }

        [HttpPost("PhaseThree/UpdatePrice")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdatePrice(int id, decimal totalCost, decimal quantity, string unit, string description)
        {
            var item = await _context.ConstructionBudgetItems.FindAsync(id);
            if (item == null) return NotFound();

            item.Quantity = quantity > 0 ? quantity : 1;
            item.Unit = string.IsNullOrEmpty(unit) ? "Götürü" : unit;
            item.PlannedUnitPrice = totalCost / item.Quantity; // Calculate unit price from total
            
            item.Description = description;
            item.QuoteStatus = BudgetQuoteStatus.EstimatedOrQuoted;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { projectId = item.ConstructionProjectId });
        }

        [HttpPost("PhaseThree/AcceptQuote")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AcceptQuote(int inviteId, int budgetItemId)
        {
            var item = await _context.ConstructionBudgetItems.FindAsync(budgetItemId);
            if (item == null) return NotFound();

            var invite = await _context.B2BQuoteInvites.FindAsync(inviteId);
            if (invite == null) return NotFound();

            invite.IsFeasibilitySelected = true;
            item.PlannedUnitPrice = invite.OfferedPrice ?? 0;
            item.QuoteStatus = BudgetQuoteStatus.EstimatedOrQuoted;
            
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { projectId = item.ConstructionProjectId });
        }

        [HttpPost("PhaseThree/DeleteItem/{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.ConstructionBudgetItems.FindAsync(id);
            if (item != null)
            {
                _context.ConstructionBudgetItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", new { projectId = item?.ConstructionProjectId });
        }
    }
}