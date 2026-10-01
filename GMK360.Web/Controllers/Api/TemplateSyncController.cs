using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Construction;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemplateSyncController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TemplateSyncController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("CheckUpdates/{projectId}")]
        public async Task<IActionResult> CheckUpdates(int projectId)
        {
            var projectDocs = await _context.ProjectLegalDocuments
                .Where(d => d.ConstructionProjectId == projectId && !d.IsCustom)
                .ToListAsync();

            var masterRules = await _context.ModuleDocumentRules
                .Include(r => r.SystemLegalDocumentTemplate)
                .Where(r => r.TargetModule == "PhaseZeroDocument")
                .ToListAsync();

            var diff = new
            {
                NewDocs = masterRules.Where(r => r.IsActive && !projectDocs.Any(d => d.SystemTemplateId == r.SystemLegalDocumentTemplateId))
                    .Select(r => new { RuleId = r.Id, Name = r.SystemLegalDocumentTemplate.Name, Stage = r.Stage }),
                
                ChangedDocs = masterRules.Where(r => r.IsActive && projectDocs.Any(d => d.SystemTemplateId == r.SystemLegalDocumentTemplateId && (d.DocumentName != r.SystemLegalDocumentTemplate.Name || d.Stage != r.Stage)))
                    .Select(r => new { RuleId = r.Id, OldName = projectDocs.First(d => d.SystemTemplateId == r.SystemLegalDocumentTemplateId).DocumentName, NewName = r.SystemLegalDocumentTemplate.Name }),
                
                RemovedDocs = projectDocs.Where(d => d.SystemTemplateId != null && !masterRules.Any(r => r.SystemLegalDocumentTemplateId == d.SystemTemplateId && r.IsActive))
                    .Select(d => new { DocId = d.Id, Name = d.DocumentName, Status = d.Status, HasFile = !string.IsNullOrEmpty(d.FilePath) })
            };

            return Ok(new { success = true, diff });
        }

        [HttpPost("ApplyUpdates")]
        public async Task<IActionResult> ApplyUpdates([FromForm] int projectId)
        {
            var projectDocs = await _context.ProjectLegalDocuments
                .Where(d => d.ConstructionProjectId == projectId && !d.IsCustom)
                .ToListAsync();

            var masterRules = await _context.ModuleDocumentRules
                .Include(r => r.SystemLegalDocumentTemplate)
                .Where(r => r.TargetModule == "PhaseZeroDocument")
                .ToListAsync();

            // 1. Add New Docs
            var newRules = masterRules.Where(r => r.IsActive && !projectDocs.Any(d => d.SystemTemplateId == r.SystemLegalDocumentTemplateId));
            foreach (var rule in newRules)
            {
                _context.ProjectLegalDocuments.Add(new ProjectLegalDocument
                {
                    ConstructionProjectId = projectId,
                    SystemTemplateId = rule.SystemLegalDocumentTemplateId,
                    DocumentName = rule.SystemLegalDocumentTemplate.Name,
                    Stage = rule.Stage,
                    Status = "Bekliyor",
                    DisplayOrder = rule.DisplayOrder,
                    IsActive = true
                });
            }

            // 2. Update Changed Docs
            var changedRules = masterRules.Where(r => r.IsActive && projectDocs.Any(d => d.SystemTemplateId == r.SystemLegalDocumentTemplateId && (d.DocumentName != r.SystemLegalDocumentTemplate.Name || d.Stage != r.Stage)));
            foreach (var rule in changedRules)
            {
                var pDoc = projectDocs.First(d => d.SystemTemplateId == rule.SystemLegalDocumentTemplateId);
                pDoc.DocumentName = rule.SystemLegalDocumentTemplate.Name;
                pDoc.Stage = rule.Stage;
                pDoc.DisplayOrder = rule.DisplayOrder;
            }

            // 3. Handle Removed/Inactive Docs
            var removedDocs = projectDocs.Where(d => d.SystemTemplateId != null && !masterRules.Any(r => r.SystemLegalDocumentTemplateId == d.SystemTemplateId && r.IsActive));
            foreach (var rDoc in removedDocs)
            {
                // Güvenlik Kontrolü: Evrak doluysa (Tamamlandı veya Dosya varsa) KORU.
                if (rDoc.Status == "Tamamlandı" || rDoc.Status == "Alındı" || !string.IsNullOrEmpty(rDoc.FilePath) || rDoc.DocumentFee > 0)
                {
                    // Arşivde kalsın, dokunma.
                    rDoc.IsActive = false;
                }
                else
                {
                    // Boşta bekliyorsa, durumu "Muaf/İstenmiyor" yap
                    rDoc.Status = "Muaf/İstenmiyor";
                    rDoc.IsActive = false; // Soft delete
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
