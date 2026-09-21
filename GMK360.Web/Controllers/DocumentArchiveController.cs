using GMK360.Core.Entities;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class DocumentArchiveController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocumentArchiveController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<int> GetCurrentAgencyIdAsync()
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return 0;
            
            return await _context.AgencyConsultants
                .Where(a => a.UserId == currentUserId)
                .Select(a => a.AgencyId)
                .FirstOrDefaultAsync();
        }

        public async Task<IActionResult> Index(string context = "construction", int? projectId = null, string category = null, int? year = null)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            
            var query = _context.DocumentArchives
                .Include(d => d.Project)
                .Where(d => d.AgencyId == agencyId && d.IsDeleted == false);

            if (projectId.HasValue)
            {
                query = query.Where(d => d.ProjectId == projectId.Value);
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(d => d.Category == category);
            }

            if (year.HasValue)
            {
                query = query.Where(d => d.Year == year.Value);
            }

            var documents = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();

            ViewBag.Context = context;
            ViewBag.ProjectId = projectId;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentYear = year ?? DateTime.Now.Year;

            // Kategorileri listelemek için
            var existingCategories = await _context.DocumentArchives
                .Where(d => d.AgencyId == agencyId && !d.IsDeleted)
                .Select(d => d.Category)
                .Distinct()
                .ToListAsync();
            
            var defaultCategories = new[] { "Sözleşmeler", "Çizimler", "Bina Görselleri", "Resmi Evraklar", "Tutanaklar", "Faturalar/Fişler", "Fazlar", "Diğer" };
            var allCategories = defaultCategories.Union(existingCategories.Where(c => !string.IsNullOrEmpty(c))).Distinct().ToList();
            
            ViewBag.Categories = allCategories.ToArray();
            
            if (context == "construction")
            {
                ViewBag.Projects = await _context.ConstructionProjects.Where(p => p.AgencyId == agencyId).ToListAsync();
            }

            return View(documents);
        }

        [HttpPost]
        public async Task<IActionResult> UploadManual(IFormFile file, string category, int? year, int? projectId, string notes)
        {
            var agencyId = await GetCurrentAgencyIdAsync();

            if (file != null && file.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "archive", agencyId.ToString());
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var archive = new DocumentArchive
                {
                    AgencyId = agencyId,
                    FileName = file.FileName,
                    DocumentUrl = $"/uploads/archive/{agencyId}/{uniqueFileName}",
                    FileExtension = Path.GetExtension(file.FileName).ToLower(),
                    Category = string.IsNullOrEmpty(category) ? "Diğer" : category,
                    Year = year ?? DateTime.Now.Year,
                    Title = string.IsNullOrEmpty(notes) ? file.FileName : notes,
                    SourceModule = "Manual",
                    ProjectId = projectId,
                    UploadDate = DateTime.Now
                };

                _context.DocumentArchives.Add(archive);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", new { projectId = projectId, category = category, year = year });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            var doc = await _context.DocumentArchives.FirstOrDefaultAsync(d => d.Id == id && d.AgencyId == agencyId);
            
            if (doc != null)
            {
                doc.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
            
            return Ok();
        }
    }
}