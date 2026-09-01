using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Data;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Construction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class ProjectMaterialController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<GMK360.Core.Entities.Identity.ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProjectMaterialController(ApplicationDbContext context, 
            UserManager<GMK360.Core.Entities.Identity.ApplicationUser> userManager,
            IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }

        private async Task<int?> GetUserAgencyIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            var agencyConsultant = await _context.AgencyConsultants
                .FirstOrDefaultAsync(a => a.UserId == user.Id);
            
            return agencyConsultant?.AgencyId;
        }

        // GET: ProjectMaterial/Index/5 (projectId)
        public async Task<IActionResult> Index(int projectId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var project = await _context.ConstructionProjects
                .Include(p => p.MaterialCatalogs)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return NotFound();

            if (!User.IsInRole("Admin") && project.AgencyId != agencyId)
            {
                return Unauthorized();
            }

            ViewBag.ProjectName = project.Name;
            ViewBag.ProjectId = project.Id;

            return View(project.MaterialCatalogs.OrderBy(m => m.Category).ToList());
        }

        // GET: ProjectMaterial/Create/5 (projectId)
        public async Task<IActionResult> Create(int projectId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            
            if (project == null || (!User.IsInRole("Admin") && project.AgencyId != agencyId))
            {
                return NotFound();
            }

            ViewBag.ProjectId = projectId;
            return View();
        }

        // POST: ProjectMaterial/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int constructionProjectId, string category, string materialName, string description, decimal priceDifference, IFormFile imageFile)
        {
            var agencyId = await GetUserAgencyIdAsync();
            var project = await _context.ConstructionProjects.FindAsync(constructionProjectId);
            
            if (project == null || (!User.IsInRole("Admin") && project.AgencyId != agencyId))
            {
                return Unauthorized();
            }

            string uploadedImageUrl = "";

            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "materials");
                Directory.CreateDirectory(uploadsFolder);
                
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                
                uploadedImageUrl = "/uploads/materials/" + uniqueFileName;
            }

            var material = new ProjectMaterialCatalog
            {
                ConstructionProjectId = constructionProjectId,
                Category = category,
                MaterialName = materialName,
                Description = description ?? "",
                PriceDifference = priceDifference,
                ImageUrl = uploadedImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProjectMaterialCatalogs.Add(material);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Malzeme seçeneği başarıyla eklendi.";
            return RedirectToAction(nameof(Index), new { projectId = constructionProjectId });
        }

        // POST: ProjectMaterial/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var material = await _context.ProjectMaterialCatalogs
                .Include(m => m.ConstructionProject)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (material == null) return NotFound();

            var agencyId = await GetUserAgencyIdAsync();
            if (!User.IsInRole("Admin") && material.ConstructionProject.AgencyId != agencyId)
            {
                return Unauthorized();
            }

            int projectId = material.ConstructionProjectId;

            _context.ProjectMaterialCatalogs.Remove(material);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Malzeme silindi.";
            return RedirectToAction(nameof(Index), new { projectId = projectId });
        }
    }
}
