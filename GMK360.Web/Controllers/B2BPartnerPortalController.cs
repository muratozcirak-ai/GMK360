using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.B2B;
using GMK360.Web.Services;

namespace GMK360.Web.Controllers
{
    public class B2BPartnerPortalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly B2BSupplierSyncService _syncService;

        public B2BPartnerPortalController(ApplicationDbContext context, B2BSupplierSyncService syncService)
        {
            _context = context;
            _syncService = syncService;
        }

        [HttpGet("B2BPartnerPortal/Invite/{token}")]
        public async Task<IActionResult> Invite(string token)
        {
            var invite = await _context.B2BQuoteInvites
                .Include(i => i.NetworkContact)
                .Include(i => i.InviteItems)
                .Include(i => i.QuoteRequest)
                    .ThenInclude(qr => qr.RequesterAgency)
                .Include(i => i.QuoteRequest)
                    .ThenInclude(qr => qr.Items)
                        .ThenInclude(it => it.MaterialCatalog)
                .FirstOrDefaultAsync(i => i.AccessToken == token);

            if (invite == null) return NotFound("Geçersiz veya süresi dolmuş davet linki.");

            dynamic projectInfo = null;
            if (invite.QuoteRequest.SourceModule == "PhaseZeroDocument")
            {
                var doc = await _context.ProjectLegalDocuments
                    .Include(d => d.ConstructionProject)
                    .FirstOrDefaultAsync(d => d.Id == invite.QuoteRequest.SourceReferenceId);

                if (doc != null && doc.ConstructionProject != null)
                {
                    projectInfo = new {
                        Name = doc.ConstructionProject.Name,
                        Address = doc.ConstructionProject.Address,
                        Lat = doc.ConstructionProject.Latitude,
                        Lng = doc.ConstructionProject.Longitude,
                        BuiltYear = "1970", 
                        Area = doc.ConstructionProject.TotalLandArea > 0 ? doc.ConstructionProject.TotalLandArea.ToString() : "1250",
                        Type = doc.ConstructionProject.ProjectType ?? "2 Blok Bitişik Nizam",
                        Units = "43 Daire, 2 Dükkan" 
                    };
                }
            }
            else if (invite.QuoteRequest.SourceModule == "ConstructionProject")
            {
                var project = await _context.ConstructionProjects
                    .FirstOrDefaultAsync(p => p.Id == invite.QuoteRequest.SourceReferenceId);

                if (project != null)
                {
                    projectInfo = new {
                        Name = project.Name,
                        Address = project.Address,
                        Lat = project.Latitude,
                        Lng = project.Longitude,
                        BuiltYear = "2024",
                        Area = project.TotalLandArea > 0 ? project.TotalLandArea.ToString() : "1500",
                        Type = project.ProjectType ?? "Konut / Ticari",
                        Units = "Şantiye Projesi"
                    };
                }
            }

            ViewBag.ProjectInfo = projectInfo;
            if (invite.NetworkContact != null && !string.IsNullOrEmpty(invite.NetworkContact.CompanyName))
            {
                ViewBag.B2bCompany = await _context.B2bCompanies.FirstOrDefaultAsync(c => c.Name == invite.NetworkContact.CompanyName);
            }
            return View(invite);
        }

        [HttpPost("B2BPartnerPortal/SubmitQuote")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuote(
            int inviteId, 
            decimal offeredPrice, 
            string? paymentTerm, 
            string? offerNotes,
            bool isVatIncluded,
            Dictionary<int, decimal>? itemUnitPrices)
        {
            var invite = await _context.B2BQuoteInvites
                .Include(i => i.InviteItems)
                .FirstOrDefaultAsync(i => i.Id == inviteId);
            if (invite == null) return NotFound();

            invite.IsVatIncludedGlobally = isVatIncluded;
            var vatTag = isVatIncluded ? "[KDV Dahil] " : "[+KDV Hariç] ";

            // Ödeme şartı ve notları yapısal olarak birleştir
            var structuredNotes = vatTag;
            if (!string.IsNullOrWhiteSpace(paymentTerm))
            {
                structuredNotes += $"[Ödeme: {paymentTerm.Trim()}] ";
            }
            if (!string.IsNullOrWhiteSpace(offerNotes))
            {
                structuredNotes += offerNotes.Trim();
            }

            invite.OfferedPrice = offeredPrice;
            invite.OfferNotes = structuredNotes ?? "";
            invite.Status = QuoteInviteStatus.Submitted;
            invite.RespondedAt = DateTime.UtcNow;

            // Çoklu kalem birim fiyatları varsa kaydet
            if (itemUnitPrices != null && itemUnitPrices.Any())
            {
                foreach (var kvp in itemUnitPrices)
                {
                    int quoteItemId = kvp.Key;
                    decimal unitPrice = kvp.Value;

                    var existingItem = invite.InviteItems?.FirstOrDefault(ii => ii.B2BQuoteItemId == quoteItemId);
                    if (existingItem != null)
                    {
                        existingItem.OfferedUnitPrice = unitPrice;
                        existingItem.IsVatIncluded = isVatIncluded;
                    }
                    else
                    {
                        var newItem = new B2BQuoteInviteItem
                        {
                            B2BQuoteInviteId = invite.Id,
                            B2BQuoteItemId = quoteItemId,
                            OfferedUnitPrice = unitPrice,
                            IsVatIncluded = isVatIncluded,
                            VatRate = 20
                        };
                        _context.Set<B2BQuoteInviteItem>().Add(newItem);
                    }
                }
            }

            var quoteReq = await _context.B2BQuoteRequests.FirstOrDefaultAsync(q => q.Id == invite.QuoteRequestId);
            if (quoteReq != null && quoteReq.SourceModule == "PhaseZeroDocument")
            {
                var doc = await _context.ProjectLegalDocuments.FirstOrDefaultAsync(d => d.Id == quoteReq.SourceReferenceId);
                if (doc != null) doc.Status = "Teklif Geldi";
            }

            var invContact = await _context.B2BNetworkContacts.FirstOrDefaultAsync(nc => nc.Id == invite.NetworkContactId);
            if (invContact != null)
            {
                var b2bComp = await _context.B2bCompanies.FirstOrDefaultAsync(c => c.Name == invContact.CompanyName);
                if (b2bComp != null && (b2bComp.InvitationStatus == GMK360.Core.Entities.B2b.B2bInvitationStatus.Shadow || b2bComp.InvitationStatus == GMK360.Core.Entities.B2b.B2bInvitationStatus.Invited))
                {
                    b2bComp.InvitationStatus = GMK360.Core.Entities.B2b.B2bInvitationStatus.Active;
                }
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Teklifiniz ve ödeme koşullarınız başarıyla iletildi. Yetkili değerlendirdikten sonra onay durumunda cari hesabınız ve şantiye takviminiz oluşturulacaktır.";
            return RedirectToAction("Invite", new { token = invite.AccessToken });
        }

        [HttpPost("B2BPartnerPortal/RequestInspection")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestInspection(
            int inviteId, 
            DateTime? inspectionDate,
            DateTime? inspectionDateOnly, 
            string? inspectionTimeOnly,
            string? requestedByPerson, 
            string? notes)
        {
            var invite = await _context.B2BQuoteInvites.FirstOrDefaultAsync(i => i.Id == inviteId);
            if (invite == null) return NotFound();

            var finalDate = inspectionDateOnly ?? inspectionDate;
            string dateProposal;
            if (finalDate.HasValue)
            {
                var timePart = string.IsNullOrWhiteSpace(inspectionTimeOnly) ? "" : " (Saat: " + inspectionTimeOnly.Trim() + ")";
                dateProposal = $"{finalDate.Value:dd.MM.yyyy}{timePart}";
            }
            else
            {
                dateProposal = "Tarih Şantiyeye Bırakıldı (Arayıp Kararlaştıralım)";
            }

            var ustaName = string.IsNullOrWhiteSpace(requestedByPerson) ? "Yetkili / Usta" : requestedByPerson.Trim();
            var noteText = string.IsNullOrWhiteSpace(notes) ? "Saha incelemesi talep edildi." : notes.Trim();

            // Kara deftere doğrudan YAZMIYORUZ! Şantiye masasına talep/öneri olarak iletiyoruz.
            invite.OfferNotes = $"[KEŞİF TALEBİ] Usta: {ustaName} | Öneri: {dateProposal} | Not: {noteText}";
            invite.RespondedAt = DateTime.UtcNow;

            var quoteReq = await _context.B2BQuoteRequests.FirstOrDefaultAsync(q => q.Id == invite.QuoteRequestId);
            if (quoteReq != null && quoteReq.SourceModule == "PhaseZeroDocument")
            {
                var doc = await _context.ProjectLegalDocuments.FirstOrDefaultAsync(d => d.Id == quoteReq.SourceReferenceId);
                if (doc != null) doc.Status = "Keşif İstendi";
            }

            var invContact2 = await _context.B2BNetworkContacts.FirstOrDefaultAsync(nc => nc.Id == invite.NetworkContactId);
            if (invContact2 != null)
            {
                var b2bComp2 = await _context.B2bCompanies.FirstOrDefaultAsync(c => c.Name == invContact2.CompanyName);
                if (b2bComp2 != null && (b2bComp2.InvitationStatus == GMK360.Core.Entities.B2b.B2bInvitationStatus.Shadow || b2bComp2.InvitationStatus == GMK360.Core.Entities.B2b.B2bInvitationStatus.Invited))
                {
                    b2bComp2.InvitationStatus = GMK360.Core.Entities.B2b.B2bInvitationStatus.Active;
                }
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "🎉 Saha keşif ve inceleme talebiniz şantiyeye başarıyla iletildi! Şantiye yetkilisi randevuyu teyit etmek için sizinle iletişime geçecektir.";
            return RedirectToAction("Invite", new { token = invite.AccessToken });
        }

        [HttpPost("B2BPartnerPortal/ActivateAccount")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateAccount(int inviteId)
        {
            var invite = await _context.B2BQuoteInvites
                .Include(i => i.NetworkContact)
                .Include(i => i.QuoteRequest).ThenInclude(qr => qr.RequesterAgency)
                .FirstOrDefaultAsync(i => i.Id == inviteId);
            if (invite == null) return NotFound();

            if (invite.NetworkContact != null)
            {
                var b2bComp = await _context.B2bCompanies.FirstOrDefaultAsync(c => c.Name == invite.NetworkContact.CompanyName);
                if (b2bComp != null)
                {
                    b2bComp.InvitationStatus = GMK360.Core.Entities.B2b.B2bInvitationStatus.Active;
                    b2bComp.IsVerified = true;
                    await _context.SaveChangesAsync();
                }
            }

            var refAgency = invite.QuoteRequest?.RequesterAgency?.CompanyName ?? "Ana Firma";
            TempData["SuccessMessage"] = $"🎉 Kurumsal hesabınız aktifleştirildi! Artık GMK360 ekosisteminde aktif üyesiniz ve sizi davet eden {refAgency} firmasına Kazan-Kazan referans puanı tanımlandı.";
            return RedirectToAction("Invite", new { token = invite.AccessToken });
        }
    }
}
