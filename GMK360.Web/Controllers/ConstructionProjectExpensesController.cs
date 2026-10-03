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

        public async Task<IActionResult> Index(int? projectId, int? expenseType, DateTime? startDate, DateTime? endDate)
        {
            ViewBag.SelectedType = expenseType;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.Projects = new SelectList(await _context.ConstructionProjects.Where(p => !p.IsDeleted).ToListAsync(), "Id", "Name", projectId);
            
            var query = _context.Set<ConstructionProjectExpense>()
                .Include(e => e.Project)
                .Where(e => !e.IsDeleted);

            if (projectId.HasValue)
            {
                query = query.Where(e => e.ProjectId == projectId.Value);
            }
            if (expenseType.HasValue)
            {
                query = query.Where(e => (int)e.ExpenseType == expenseType.Value);
            }
            if (startDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate <= endDate.Value);
            }
            
            var expenses = await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();
            return View(expenses);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int? ProjectId, int ExpenseType, string Title, string Description, decimal Amount, string DocumentNo, DateTime ExpenseDate, bool IsPaid, Microsoft.AspNetCore.Http.IFormFile photo)
        {
            string photoPath = null;
            if (photo != null && photo.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(photo.FileName);
                var filePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
                using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }
                photoPath = "/uploads/" + fileName;
            }
            var expense = new ConstructionProjectExpense
            {
                AgencyId = 1,
                ProjectId = ProjectId,
                PhotoPath = photoPath,
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