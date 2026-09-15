import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\DocumentArchiveController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

new_content = '''using GMK360.Core.Entities;
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

        private int GetCurrentAgencyId()
        {
            var claim = User.FindFirst("AgencyId");
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        public async Task<IActionResult> Index(string context = "construction", int? projectId = null, string category = null, int? year = null)
        {
            var agencyId = GetCurrentAgencyId();
            
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
            ViewBag.Categories = new[] { "Sözleşmeler", "Tutanaklar", "Projeler/Çizimler", "Ruhsatlar/İzinler", "Faturalar/Fişler", "Diğer" };
            
            if (context == "construction")
            {
                ViewBag.Projects = await _context.ConstructionProjects.Where(p => p.AgencyId == agencyId).ToListAsync();
            }

            return View(documents);
        }

        [HttpPost]
        public async Task<IActionResult> UploadManual(IFormFile file, string category, int? year, int? projectId, string notes)
        {
            var agencyId = GetCurrentAgencyId();

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
                    FileUrl = $"/uploads/archive/{agencyId}/{uniqueFileName}",
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
            var agencyId = GetCurrentAgencyId();
            var doc = await _context.DocumentArchives.FirstOrDefaultAsync(d => d.Id == id && d.AgencyId == agencyId);
            
            if (doc != null)
            {
                doc.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
            
            return Ok();
        }
    }
}'''

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(new_content)
