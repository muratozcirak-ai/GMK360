using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Construction;
using System.Linq;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers
{
    public class PhaseZeroController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PhaseZeroController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("PhaseZero/Index/{projectId}")]
        public async Task<IActionResult> Index(int projectId)
        {
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project == null) return NotFound();

            ViewData["ProjectId"] = projectId;
            ViewData["ProjectName"] = project.Name;

            var docs = await _context.ProjectLegalDocuments
                .Include(d => d.SystemTemplate)
                .Where(d => d.ConstructionProjectId == projectId)
                .OrderBy(d => d.DisplayOrder)
                .ToListAsync();

            ViewBag.GlobalRules = await _context.ModuleDocumentRules
                .Include(r => r.Prerequisites)
                .ThenInclude(p => p.PrerequisiteTemplate)
                .Where(r => r.TargetModule == "Construction")
                .ToListAsync();

            // Fetch quote requests for these documents to show status
            var docIds = docs.Select(d => d.Id).ToList();
            var quoteRequests = await _context.B2BQuoteRequests
                .Include(q => q.Invites).ThenInclude(i => i.NetworkContact)
                .Where(q => q.SourceModule == "PhaseZeroDocument" && docIds.Contains(q.SourceReferenceId))
                .ToListAsync();
            
            ViewBag.Quotes = quoteRequests;


            return View(docs);
        }

        [HttpPost("PhaseZero/SyncDocs/{projectId}")]
        public async Task<IActionResult> SyncDocs(int projectId)
        {
            var rules = await _context.ModuleDocumentRules
                .Include(r => r.SystemLegalDocumentTemplate)
                .Where(r => r.TargetModule == "Construction")
                .OrderBy(r => r.DisplayOrder)
                .ToListAsync();

            var existingTemplateIds = await _context.ProjectLegalDocuments
                .Where(d => d.ConstructionProjectId == projectId && d.SystemTemplateId != null)
                .Select(d => d.SystemTemplateId.Value)
                .ToListAsync();

            int addedCount = 0;
            foreach (var rule in rules)
            {
                if (!existingTemplateIds.Contains(rule.SystemLegalDocumentTemplateId))
                {
                    var doc = new ProjectLegalDocument
                    {
                        ConstructionProjectId = projectId,
                        SystemTemplateId = rule.SystemLegalDocumentTemplateId,
                        DocumentName = rule.SystemLegalDocumentTemplate.Name,
                        Stage = rule.Stage,
                        InstitutionContact = null,
                        Status = "Bekliyor",
                        DisplayOrder = rule.DisplayOrder,
                        EstimatedCost = 0,
                        ActualCost = 0
                    };
                    _context.ProjectLegalDocuments.Add(doc);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", new { projectId = projectId });
        }

        [HttpGet("PhaseZero/GetDoc/{id}")]
        public async Task<IActionResult> GetDoc(int id)
        {
            var doc = await _context.ProjectLegalDocuments.FindAsync(id);
            if (doc == null) return NotFound();
            
            return Json(new {
                id = doc.Id,
                documentName = doc.DocumentName,
                status = doc.Status,
                assignedUserId = doc.AssignedUserId,
                institutionContact = doc.InstitutionContact,
                documentFee = doc.DocumentFee ?? 0,
                additionalCost = doc.AdditionalCost ?? 0,
                originalLocation = doc.OriginalLocation,
                filePath = doc.FilePath
            });
        }

        [HttpPost("PhaseZero/UpdateDoc")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateDoc(int id, string status, string assignedUserId, string institutionContact, decimal? documentFee, decimal? additionalCost, string originalLocation, Microsoft.AspNetCore.Http.IFormFile uploadedFile)
        {
            var doc = await _context.ProjectLegalDocuments.FindAsync(id);
            if (doc == null) return NotFound();

            // GÜVENLİK NOTU: Burada ileride User.Identity.Role == "Admin" || doc.AssignedUserId == User.Identity.Name kontrolü eklenecek.
            
            doc.Status = status;
            doc.AssignedUserId = assignedUserId;
            doc.InstitutionContact = institutionContact;
            doc.DocumentFee = documentFee;
            doc.AdditionalCost = additionalCost;
            doc.OriginalLocation = originalLocation;

            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                var uploadsFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", "faz0");
                if (!System.IO.Directory.Exists(uploadsFolder))
                    System.IO.Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = System.Guid.NewGuid().ToString() + "_" + uploadedFile.FileName;
                var filePath = System.IO.Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
                doc.FilePath = "/uploads/faz0/" + uniqueFileName;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { projectId = doc.ConstructionProjectId });
        }
        
        [HttpPost]
        public async Task<IActionResult> AcceptQuote(int documentId, int inviteId)
        {
            var document = await _context.ProjectLegalDocuments.FindAsync(documentId);
            if (document == null) return NotFound();

            var invite = await _context.B2BQuoteInvites.FindAsync(inviteId);
            if (invite == null) return NotFound();

            invite.IsFeasibilitySelected = true;
            document.EstimatedCost = invite.OfferedPrice;
            document.Status = "İşlemde";
            
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Teklif başarıyla seçildi ve fizibilite güncellendi.";
            return RedirectToAction(nameof(Index), new { projectId = document.ConstructionProjectId });
        }

        [HttpPost("PhaseZero/RequestQuote/{documentId}")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> RequestQuote(int documentId)
        {
            var doc = await _context.ProjectLegalDocuments
                .Include(d => d.ConstructionProject)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (doc == null) return NotFound();

            // Sadece daha önce aynı evrak için talep açılmamışsa aç
            bool exists = await _context.B2BQuoteRequests.AnyAsync(q => q.SourceModule == "PhaseZeroDocument" && q.SourceReferenceId == documentId);
            
            if (!exists)
            {
                var quoteRequest = new GMK360.Core.Entities.B2B.B2BQuoteRequest
                {
                    RequesterAgencyId = doc.ConstructionProject.AgencyId,
                    RequesterUserId = "ProjeYöneticisi",
                    SourceModule = "PhaseZeroDocument",
                    SourceReferenceId = doc.Id,
                    Title = doc.ConstructionProject.Name + " - " + doc.DocumentName + " (Fiyat Araştırması)",
                    Description = "Bu proje (" + doc.ConstructionProject.Name + ") için bu evrak/işlem (" + doc.DocumentName + ") lazım. Fiyat araştırması ve onayını bekliyorum.",
                    Deadline = System.DateTime.Now.AddDays(7),
                    Status = "Draft"
                };

                _context.B2BQuoteRequests.Add(quoteRequest);
                
                doc.Status = "Fiyat Araştırılıyor"; // Satınalma sürecine girdi
                doc.AssignedUserId = "Satınalma Departmanı"; // Otomatik satınalmaya atandı
                
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", new { projectId = doc.ConstructionProjectId });
    
        }

        
    }
}
