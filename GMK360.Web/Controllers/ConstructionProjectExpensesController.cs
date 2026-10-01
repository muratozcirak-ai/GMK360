using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Construction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class ConstructionProjectExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConstructionProjectExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? projectId)
        {
            ViewBag.Projects = new SelectList(await _context.ConstructionProjects.Where(p => !p.IsDeleted).ToListAsync(), "Id", "Name", projectId);
            
            var query = _context.Set<ConstructionProjectExpense>()
                .Include(e => e.Project)
                .Where(e => !e.IsDeleted);

            if (projectId.HasValue)
            {
                query = query.Where(e => e.ProjectId == projectId.Value);
            }
            
            var expenses = await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();
            return View(expenses);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int ProjectId, int ExpenseType, string Title, string Description, decimal Amount, string DocumentNo, DateTime ExpenseDate, bool IsPaid)
        {
            var expense = new ConstructionProjectExpense
            {
                AgencyId = 1,
                ProjectId = ProjectId,
                ExpenseType = (ConstructionExpenseType)ExpenseType,
                Title = Title,
                Description = Description,
                Amount = Amount,
                DocumentNo = DocumentNo,
                ExpenseDate = ExpenseDate,
                IsPaid = IsPaid
            };
            
            _context.Add(expense);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Gider (Fatura/Fiş) başarıyla kaydedildi.";
            return RedirectToAction(nameof(Index), new { projectId = ProjectId });
        }
    }
}