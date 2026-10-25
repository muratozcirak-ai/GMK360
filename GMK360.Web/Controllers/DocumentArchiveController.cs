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

        public async Task<IActionResult> Index(string context = "construction", int? projectId = null, string category = null, int? year = null, string cabinet = null)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            var agency = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == agencyId);
            var agencyName = agency?.CompanyName ?? "Genel Merkez";
            ViewBag.AgencyName = agencyName;

            bool isCorporate = (cabinet == "corporate" || projectId == 0);
            ViewBag.IsCorporateCabinet = isCorporate;
            ViewBag.Cabinet = cabinet;

            var query = _context.DocumentArchives
                .Include(d => d.Project)
                .Where(d => d.AgencyId == agencyId && d.IsDeleted == false);

            if (isCorporate)
            {
                query = query.Where(d => d.ProjectId == null);
                ViewBag.SelectedProjectName = $"{agencyName} Merkez Arşivi";
                ViewBag.ProjectId = 0;
            }
            else if (projectId.HasValue && projectId.Value > 0)
            {
                query = query.Where(d => d.ProjectId == projectId.Value);
                var proj = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == projectId.Value);
                ViewBag.SelectedProjectName = proj?.Name;
                ViewBag.ProjectId = projectId.Value;
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
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentYear = year ?? DateTime.Now.Year;

            // Kurumsal ve Proje Dolabı Kategorileri
            var corporateCategories = new[] { 
                "Şirket Mülk & Yerleşke (Tapu / Kira)", 
                "Hukuk & Şirket Sözleşmeleri", 
                "Resmi & Ticari Evraklar", 
                "Şirket Araçları & Garaj", 
                "Merkez Personel & İK", 
                "Kurumsal Prosedürler & Yönetmelikler", 
                "Finans & Banka Evrakları", 
                "Genel Kurumsal Arşiv" 
            };

            var projectCategories = new[] { 
                "Sözleşmeler", 
                "Kentsel Dönüşüm Prosedürleri", 
                "Çizimler", 
                "Bina Görselleri", 
                "Resmi Evraklar", 
                "Bağımsız Bölüm (Daire) Evrakları",
                "Personel & Özlük Dosyaları",
                "Tutanaklar", 
                "Faturalar/Fişler", 
                "Fazlar", 
                "Diğer" 
            };

            var baseCategories = isCorporate ? corporateCategories : projectCategories;

            var existingCategories = await _context.DocumentArchives
                .Where(d => d.AgencyId == agencyId && !d.IsDeleted && (isCorporate ? d.ProjectId == null : (projectId.HasValue && projectId.Value > 0 ? d.ProjectId == projectId.Value : true)))
                .Select(d => d.Category)
                .Distinct()
                .ToListAsync();

            var allCategories = baseCategories.Union(existingCategories.Where(c => !string.IsNullOrEmpty(c))).Distinct().ToList();
            ViewBag.Categories = allCategories.ToArray();

            // Toplam Kurumsal Evrak Sayısı
            ViewBag.CorporateDocCount = await _context.DocumentArchives.CountAsync(d => d.AgencyId == agencyId && !d.IsDeleted && d.ProjectId == null);

            if (context == "construction")
            {
                ViewBag.Projects = await _context.ConstructionProjects.Where(p => p.AgencyId == agencyId).ToListAsync();
            }

            return View(documents);
        }

        [HttpPost]
        public async Task<IActionResult> UploadManual(IFormFile file, string category, string customCategory, int? year, int? projectId, string notes)
        {
            var agencyId = await GetCurrentAgencyIdAsync();

            var finalCategory = !string.IsNullOrWhiteSpace(customCategory) ? customCategory.Trim() : (string.IsNullOrEmpty(category) ? "Diğer" : category);
            int? actualProjectId = (projectId.HasValue && projectId.Value > 0) ? projectId.Value : null;

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
                    Category = finalCategory,
                    Year = year ?? DateTime.Now.Year,
                    Title = string.IsNullOrEmpty(notes) ? file.FileName : notes,
                    SourceModule = "Manual",
                    ProjectId = actualProjectId,
                    UploadDate = DateTime.Now
                };

                _context.DocumentArchives.Add(archive);
                await _context.SaveChangesAsync();
            }

            if (!actualProjectId.HasValue)
            {
                return RedirectToAction("Index", new { cabinet = "corporate", category = finalCategory, year = year });
            }
            else
            {
                return RedirectToAction("Index", new { projectId = actualProjectId, category = finalCategory, year = year });
            }
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