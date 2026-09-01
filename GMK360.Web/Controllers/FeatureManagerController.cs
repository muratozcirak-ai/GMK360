using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using Microsoft.AspNetCore.Authorization;

namespace GMK360.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class FeatureManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeatureManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Grupların Listesi (İç Özellikler, Dış Özellikler vb.)
        public async Task<IActionResult> Index()
        {
            // Sadece ParentCategoryId'si NULL olan ve SystemCode'u GROUP_ ile başlayanlar Gruptur.
            var groups = await _context.DefinitionCategories
                .Where(c => c.ParentCategoryId == null && c.SystemCode.StartsWith("GROUP_"))
                .ToListAsync();
                
            return View(groups);
        }

        [HttpPost]
        public async Task<IActionResult> AddGroup(string name, string systemCode)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var code = string.IsNullOrWhiteSpace(systemCode) ? $"GROUP_{name.ToUpper().Replace(" ", "_")}" : systemCode;
                _context.DefinitionCategories.Add(new DefinitionCategory { Name = name, SystemCode = code });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 2. Bir Gruba ait Soruların Listesi (Asansör, Banyo vb.)
        public async Task<IActionResult> Questions(int groupId)
        {
            var group = await _context.DefinitionCategories.FindAsync(groupId);
            if (group == null) return NotFound();

            ViewBag.Group = group;
            
            var questions = await _context.DefinitionCategories
                .Where(c => c.ParentCategoryId == groupId)
                .ToListAsync();
                
            return View(questions);
        }

        [HttpPost]
        public async Task<IActionResult> AddQuestion(int groupId, string name, string systemCode, bool isMultiSelect, bool isMediaTag)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var code = string.IsNullOrWhiteSpace(systemCode) ? $"QUESTION_{name.ToUpper().Replace(" ", "_")}" : systemCode;
                _context.DefinitionCategories.Add(new DefinitionCategory 
                { 
                    Name = name, 
                    SystemCode = code,
                    ParentCategoryId = groupId,
                    IsMultiSelect = isMultiSelect,
                    IsMediaTag = isMediaTag
                });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Questions), new { groupId });
        }

        // 3. Bir Soruya ait Cevapların Listesi (Yok, 1 Adet vb.)
        public async Task<IActionResult> Answers(int questionId)
        {
            var question = await _context.DefinitionCategories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == questionId);
                
            if (question == null) return NotFound();

            ViewBag.Question = question;
            
            var answers = await _context.DefinitionValues
                .Where(v => v.CategoryId == questionId)
                .OrderBy(v => v.Order)
                .ToListAsync();
                
            return View(answers);
        }

        [HttpPost]
        public async Task<IActionResult> AddAnswer(int questionId, string name, int order, bool requiresTextInput)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _context.DefinitionValues.Add(new DefinitionValue 
                { 
                    CategoryId = questionId, 
                    Name = name, 
                    Order = order,
                    RequiresTextInput = requiresTextInput,
                    SystemCode = "" // Artık zorunlu değil
                });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Answers), new { questionId });
        }
        
        [HttpPost]
        public async Task<IActionResult> DeleteAnswer(int id, int questionId)
        {
            var val = await _context.DefinitionValues.FindAsync(id);
            if (val != null)
            {
                _context.DefinitionValues.Remove(val);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Answers), new { questionId });
        }
    }
}
