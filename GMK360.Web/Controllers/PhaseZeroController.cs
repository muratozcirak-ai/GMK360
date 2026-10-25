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
        private readonly GMK360.Web.Services.B2BSupplierSyncService _supplierSyncService;

        public PhaseZeroController(ApplicationDbContext context, GMK360.Web.Services.B2BSupplierSyncService supplierSyncService)
        {
            _context = context;
            _supplierSyncService = supplierSyncService;
        }

        [HttpGet("PhaseZero/Index/{projectId}")]
        public async Task<IActionResult> Index(int projectId)
        {
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project == null) return NotFound();
            await _supplierSyncService.AutoSyncExistingNetworkContactsAsync(project.AgencyId, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "ProjeYöneticisi");

            ViewData["ProjectId"] = projectId;
            ViewData["ProjectName"] = project.Name;

            var globalRules = await _context.ModuleDocumentRules
                .Include(r => r.Prerequisites)
                .ThenInclude(p => p.PrerequisiteTemplate)
                .Where(r => r.TargetModule == "Construction")
                .ToListAsync();

            ViewBag.GlobalRules = globalRules;

            var docs = await _context.ProjectLegalDocuments
                .Include(d => d.SystemTemplate)
                .Where(d => d.ConstructionProjectId == projectId)
                .OrderBy(d => d.DisplayOrder)
                .ToListAsync();

            // Eksik önkoşul evrakları varsa otomatik oluştur
            var existingTmplIds = docs.Where(d => d.SystemTemplateId != null).Select(d => d.SystemTemplateId.Value).ToHashSet();
            bool anyNewPz = false;
            foreach (var rule in globalRules)
            {
                if (rule.Prerequisites != null)
                {
                    int subOrder = 1;
                    foreach (var pre in rule.Prerequisites)
                    {
                        if (pre.PrerequisiteTemplate != null && !existingTmplIds.Contains(pre.PrerequisiteTemplateId))
                        {
                            var childDoc = new ProjectLegalDocument
                            {
                                ConstructionProjectId = projectId,
                                SystemTemplateId = pre.PrerequisiteTemplateId,
                                DocumentName = pre.PrerequisiteTemplate.Name,
                                Stage = rule.Stage,
                                DisplayOrder = rule.DisplayOrder * 10 + subOrder++,
                                Status = "Bekliyor",
                                EstimatedCost = 0,
                                ActualCost = 0,
                                DocumentFee = 0,
                                AdditionalCost = 0
                            };
                            _context.ProjectLegalDocuments.Add(childDoc);
                            docs.Add(childDoc);
                            existingTmplIds.Add(pre.PrerequisiteTemplateId);
                            anyNewPz = true;
                        }
                    }
                }
            }
            if (anyNewPz)
            {
                await _context.SaveChangesAsync();
            }

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
                .Include(r => r.Prerequisites)
                    .ThenInclude(p => p.PrerequisiteTemplate)
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
                    existingTemplateIds.Add(rule.SystemLegalDocumentTemplateId);
                    addedCount++;
                }

                if (rule.Prerequisites != null)
                {
                    int subOrder = 1;
                    foreach (var pre in rule.Prerequisites)
                    {
                        if (pre.PrerequisiteTemplate != null && !existingTemplateIds.Contains(pre.PrerequisiteTemplateId))
                        {
                            var childDoc = new ProjectLegalDocument
                            {
                                ConstructionProjectId = projectId,
                                SystemTemplateId = pre.PrerequisiteTemplateId,
                                DocumentName = pre.PrerequisiteTemplate.Name,
                                Stage = rule.Stage,
                                DisplayOrder = rule.DisplayOrder * 10 + subOrder++,
                                Status = "Bekliyor",
                                EstimatedCost = 0,
                                ActualCost = 0,
                                DocumentFee = 0,
                                AdditionalCost = 0
                            };
                            _context.ProjectLegalDocuments.Add(childDoc);
                            existingTemplateIds.Add(pre.PrerequisiteTemplateId);
                            addedCount++;
                        }
                    }
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
                linkedPhaseCategory = doc.LinkedPhaseCategory,
                filePath = doc.FilePath
            });
        }

        [HttpPost("PhaseZero/UpdateDoc")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateDoc(int id, string status, string assignedUserId, string institutionContact, decimal? documentFee, decimal? additionalCost, string originalLocation, GMK360.Core.Entities.Construction.BudgetPhaseCategory? linkedPhaseCategory, Microsoft.AspNetCore.Http.IFormFile uploadedFile)
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
            doc.LinkedPhaseCategory = linkedPhaseCategory;

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

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Evrak başarıyla güncellendi." });
            }
            string? returnUrl = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", new { projectId = doc.ConstructionProjectId });
        }
        
        [HttpPost]
        public async Task<IActionResult> AcceptQuote(int documentId, int inviteId, bool isPaidUpfront = false, DateTime? dueDate = null)
        {
            var document = await _context.ProjectLegalDocuments
                .Include(d => d.ConstructionProject)
                .FirstOrDefaultAsync(d => d.Id == documentId);
            if (document == null) return NotFound();

            var invite = await _context.B2BQuoteInvites
                .Include(i => i.NetworkContact)
                .FirstOrDefaultAsync(i => i.Id == inviteId);
            if (invite == null) return NotFound();

            invite.IsFeasibilitySelected = true;
            invite.Status = GMK360.Core.Entities.B2B.QuoteInviteStatus.Accepted;
            document.EstimatedCost = invite.OfferedPrice;
            document.ActualCost = invite.OfferedPrice;
            document.Status = "İşlemde";

            var agencyId = document.ConstructionProject?.AgencyId ?? 1;
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "ProjeYöneticisi";

            if (invite.NetworkContact != null && invite.OfferedPrice.HasValue && invite.OfferedPrice.Value > 0)
            {
                var syncResult = await _supplierSyncService.EnsureSupplierSynchronizedAsync(
                    agencyId,
                    currentUserId,
                    invite.NetworkContact.CompanyName,
                    invite.NetworkContact.ContactPerson,
                    invite.NetworkContact.PhoneNumber,
                    invite.NetworkContact.Email,
                    document.DocumentName);

                await _supplierSyncService.OpenOrUpdateCurrentAccountAsync(
                    agencyId,
                    syncResult.PhonebookContact.Id,
                    document.ConstructionProjectId,
                    invite.OfferedPrice.Value,
                    $"{document.DocumentName} - Teklif Kabulü",
                    $"FAZ0-DOC-{document.Id}",
                    currentUserId,
                    isPaidUpfront,
                    dueDate);

                // Şantiye Kara Defterine Teslimat / İmalat Takibi Görevi Ekle
                var targetDeliveryDate = dueDate ?? DateTime.UtcNow.AddDays(7);
                var paymentTermsText = isPaidUpfront ? "Peşin Ödendi" : $"Vadeli (Vade: {targetDeliveryDate:dd.MM.yyyy})";
                await _supplierSyncService.CreateDeliveryAgendaRecordAsync(
                    agencyId,
                    document.ConstructionProjectId,
                    syncResult.PhonebookContact.Id,
                    invite.NetworkContact.CompanyName,
                    document.DocumentName,
                    invite.OfferedPrice.Value,
                    paymentTermsText,
                    targetDeliveryDate);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = isPaidUpfront 
                ? $"Teklif kabul edildi! {invite.OfferedPrice:N2} ₺ tutar peşin olarak ödendi, şantiye kara defterine teslimat görevi işlendi."
                : $"Teklif kabul edildi! {invite.OfferedPrice:N2} ₺ tutar vadeli olarak cari hesaba ve şantiye kara defterine teslimat görevi olarak işlendi.";
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

        

        [HttpPost("PhaseZero/ConfirmInspectionAppointment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmInspectionAppointment(
            int inviteId, 
            int documentId, 
            DateTime? confirmedDate, 
            DateTime? appointmentDate, 
            string? appointmentTime, 
            string? managerNotes)
        {
            var doc = await _context.ProjectLegalDocuments
                .Include(d => d.ConstructionProject)
                .FirstOrDefaultAsync(d => d.Id == documentId);
            if (doc == null) return NotFound();

            var agencyId = doc.ConstructionProject?.AgencyId ?? 1;
            var invite = await _context.B2BQuoteInvites
                .Include(i => i.NetworkContact)
                .Include(i => i.QuoteRequest)
                .FirstOrDefaultAsync(i => i.Id == inviteId);
            if (invite == null) return NotFound();

            // Tarih ve saat birleştirme
            DateTime finalDate = confirmedDate ?? DateTime.Today.AddDays(1).AddHours(10);
            if (appointmentDate.HasValue)
            {
                finalDate = appointmentDate.Value.Date;
                if (!string.IsNullOrWhiteSpace(appointmentTime) && TimeSpan.TryParse(appointmentTime, out var ts))
                {
                    finalDate = finalDate.Add(ts);
                }
                else
                {
                    finalDate = finalDate.AddHours(10);
                }
            }

            var notes = string.IsNullOrWhiteSpace(managerNotes) ? "Şantiye şefi usta ile teyit etti." : managerNotes.Trim();

            // Şantiye Kara Defteri'ne (AgendaRecord) KESİN KAYIT aç
            await _supplierSyncService.CreateDiscoveryAgendaRecordAsync(
                inviteId, 
                finalDate, 
                invite.NetworkContact?.ContactPerson, 
                notes);

            invite.OfferNotes = $"[RANDEVU KESİNLEŞTİ] Tarih: {finalDate:dd.MM.yyyy HH:mm} | Not: {notes}";
            doc.Status = "Keşif Randevusu Alındı";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Saha keşif randevusu ({finalDate:dd.MM.yyyy HH:mm}) teyit edildi ve Şantiye Kara Defteri'ne (Ajandaya) işlendi.";
            return RedirectToAction("Index", new { projectId = doc.ConstructionProjectId });
        }
    }
}