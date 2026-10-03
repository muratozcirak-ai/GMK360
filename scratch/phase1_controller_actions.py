import codecs
import re

path = 'GMK360.Web/Controllers/PhaseOneController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'// Sadece Fizibilite Aşaması İçin Listeyi Gönderiyoruz'
replacement = '''var itemIds = items.Select(i => i.Id).ToList();
            var quoteRequests = await _context.B2BQuoteRequests
                .Include(q => q.Invites).ThenInclude(i => i.NetworkContact)
                .Where(q => q.SourceModule == "PhaseOneBudget" && itemIds.Contains(q.SourceReferenceId))
                .ToListAsync();
            ViewBag.Quotes = quoteRequests;

            // Sadece Fizibilite Aşaması İçin Listeyi Gönderiyoruz'''

content = re.sub(target, replacement, content)

target2 = r'\[HttpPost\("PhaseOne/DeleteItem/\{id\}"\)\]'
replacement2 = '''[HttpPost("PhaseOne/RequestQuote/{id}")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> RequestQuote(int id)
        {
            var item = await _context.ConstructionBudgetItems
                .Include(i => i.ConstructionProject)
                .FirstOrDefaultAsync(i => i.Id == id);
            
            if (item == null) return NotFound();

            bool exists = await _context.B2BQuoteRequests.AnyAsync(q => q.SourceModule == "PhaseOneBudget" && q.SourceReferenceId == id);
            if (!exists)
            {
                var quoteRequest = new GMK360.Core.Entities.B2B.B2BQuoteRequest
                {
                    RequesterAgencyId = item.ConstructionProject.AgencyId,
                    RequesterUserId = "ProjeYöneticisi",
                    SourceModule = "PhaseOneBudget",
                    SourceReferenceId = item.Id,
                    Title = item.ConstructionProject.Name + " - " + item.ItemName + " (İmalat Teklifi)",
                    Description = "Bu imalat/kalem (" + item.ItemName + ") için taşeron fiyat araştırması ve onayını bekliyorum.",
                    Deadline = System.DateTime.Now.AddDays(7),
                    Status = "Draft"
                };
                _context.B2BQuoteRequests.Add(quoteRequest);
                item.QuoteStatus = BudgetQuoteStatus.EstimatedOrQuoted; // Moved from waiting to estimating
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", new { projectId = item.ConstructionProjectId });
        }

        [HttpPost("PhaseOne/UpdatePrice")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdatePrice(int id, decimal plannedUnitPrice, string description)
        {
            var item = await _context.ConstructionBudgetItems.FindAsync(id);
            if (item == null) return NotFound();

            item.PlannedUnitPrice = plannedUnitPrice;
            item.Description = description;
            item.QuoteStatus = BudgetQuoteStatus.EstimatedOrQuoted;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { projectId = item.ConstructionProjectId });
        }

        [HttpPost("PhaseOne/AcceptQuote")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AcceptQuote(int inviteId, int budgetItemId)
        {
            var item = await _context.ConstructionBudgetItems.FindAsync(budgetItemId);
            if (item == null) return NotFound();

            var invite = await _context.B2BQuoteInvites.FindAsync(inviteId);
            if (invite == null) return NotFound();

            invite.IsFeasibilitySelected = true;
            item.PlannedUnitPrice = invite.OfferedPrice ?? 0;
            item.QuoteStatus = BudgetQuoteStatus.EstimatedOrQuoted;
            
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { projectId = item.ConstructionProjectId });
        }

        [HttpPost("PhaseOne/DeleteItem/{id}")]'''

content = re.sub(target2, replacement2, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)