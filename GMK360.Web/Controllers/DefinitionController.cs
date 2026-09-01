using System.Linq;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class DefinitionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DefinitionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.DefinitionCategories
                                         .Include(c => c.Values)
                                         .ToListAsync();
            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(string name, string prefix)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var systemCode = GenerateSystemCode(prefix, name);
                _context.DefinitionCategories.Add(new DefinitionCategory { Name = name, SystemCode = systemCode });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Values(int categoryId)
        {
            ViewBag.Category = await _context.DefinitionCategories.FindAsync(categoryId);
            var values = await _context.DefinitionValues
                                     .Where(v => v.CategoryId == categoryId)
                                     .OrderBy(v => v.Order)
                                     .ToListAsync();
            return View(values);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(int id, string name)
        {
            var category = await _context.DefinitionCategories.FindAsync(id);
            if (category != null && !string.IsNullOrWhiteSpace(name))
            {
                category.Name = name;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddValue(int categoryId, string name, int order, bool hasCount, string subOptions)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var category = await _context.DefinitionCategories.FindAsync(categoryId);
                string prefix = category?.SystemCode ?? "VAL";
                var systemCode = GenerateSystemCode(prefix, name);

                _context.DefinitionValues.Add(new DefinitionValue 
                { 
                    CategoryId = categoryId, 
                    Name = name, 
                    SystemCode = systemCode,
                    Order = order,
                    HasCount = hasCount,
                    SubOptions = subOptions ?? string.Empty
                });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> EditValue(int id, string name, int order, bool hasCount, string subOptions)
        {
            var value = await _context.DefinitionValues.FindAsync(id);
            if (value != null && !string.IsNullOrWhiteSpace(name))
            {
                value.Name = name;
                value.Order = order;
                value.HasCount = hasCount;
                value.SubOptions = subOptions ?? string.Empty;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.DefinitionCategories
                                         .Include(c => c.Values)
                                         .FirstOrDefaultAsync(c => c.Id == id);
            if (category != null)
            {
                if (category.Values != null && category.Values.Any())
                {
                    _context.DefinitionValues.RemoveRange(category.Values);
                }
                _context.DefinitionCategories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteValue(int id, int categoryId)
        {
            var value = await _context.DefinitionValues.FindAsync(id);
            if (value != null)
            {
                _context.DefinitionValues.Remove(value);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private string GenerateSystemCode(string prefix, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return Guid.NewGuid().ToString().Substring(0, 8);
            
            var chars = new System.Collections.Generic.Dictionary<char, char>
            {
                {'ç', 'c'}, {'ğ', 'g'}, {'ı', 'i'}, {'ö', 'o'}, {'ş', 's'}, {'ü', 'u'},
                {'Ç', 'C'}, {'Ğ', 'G'}, {'İ', 'I'}, {'Ö', 'O'}, {'Ş', 'S'}, {'Ü', 'U'}
            };
            
            string code = name;
            foreach (var c in chars)
            {
                code = code.Replace(c.Key, c.Value);
            }
            
            code = System.Text.RegularExpressions.Regex.Replace(code.ToUpperInvariant(), @"[^A-Z0-9]+", "_");
            code = code.Trim('_');
            
            if (string.IsNullOrWhiteSpace(prefix))
                return code;
                
            return prefix.EndsWith("_") ? $"{prefix}{code}" : $"{prefix}_{code}";
        }
        // --- FINANSAL GIDER TIPLERI (LIABILITY TYPES) ---
        public async Task<IActionResult> LiabilityTypes()
        {
            var types = await _context.LiabilityTypes.ToListAsync();
            return View(types);
        }

        [HttpPost]
        public async Task<IActionResult> AddLiabilityType(string name, string category)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _context.LiabilityTypes.Add(new LiabilityType 
                { 
                    Name = name, 
                    Category = category ?? "Genel Gider",
                    IsSystemType = false // Admin'in eklediği sonradan eklenmiş sayılır (silinebilir)
                });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(LiabilityTypes));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLiabilityType(int id)
        {
            var type = await _context.LiabilityTypes.FindAsync(id);
            if (type != null && !type.IsSystemType) // Sistem tipleri silinemesin
            {
                _context.LiabilityTypes.Remove(type);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(LiabilityTypes));
        }
    }
}
