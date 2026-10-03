import codecs
import re

path = 'GMK360.Web/Controllers/PhaseZeroController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

method = '''
        [HttpPost]
        public async Task<IActionResult> AddFirmQuote(int quoteRequestId, string companyName, decimal? offeredPrice, string? offerNotes)
        {
            var quoteRequest = await _context.B2BQuoteRequests.FindAsync(quoteRequestId);
            if (quoteRequest == null) return NotFound();

            var contact = new GMK360.Core.Entities.B2B.B2BNetworkContact 
            { 
                CompanyName = companyName,
                Status = "Aktif"
            };
            _context.B2BNetworkContacts.Add(contact);
            await _context.SaveChangesAsync();

            var invite = new GMK360.Core.Entities.B2B.B2BQuoteInvite
            {
                QuoteRequestId = quoteRequestId,
                NetworkContactId = contact.Id,
                OfferedPrice = offeredPrice,
                OfferNotes = offerNotes,
                Status = (offeredPrice.HasValue || !string.IsNullOrEmpty(offerNotes)) ? GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted : GMK360.Core.Entities.B2B.QuoteInviteStatus.Pending,
                RespondedAt = System.DateTime.Now
            };

            _context.B2BQuoteInvites.Add(invite);
            await _context.SaveChangesAsync();

            var doc = await _context.ProjectLegalDocuments.FindAsync(quoteRequest.SourceReferenceId);
            return RedirectToAction("Index", new { projectId = doc?.ConstructionProjectId });
        }
'''

content = content.replace('    }\r\n}', method + '    }\r\n}')
content = content.replace('    }\n}', method + '    }\n}')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)