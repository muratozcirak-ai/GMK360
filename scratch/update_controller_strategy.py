import codecs
import re

path = 'GMK360.Web/Controllers/PhaseOneController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'\[HttpPost\("PhaseOne/RequestQuote/\{id\}"\)\]'
replacement = '''[HttpPost("PhaseOne/SelectStrategy")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SelectStrategy(int id, int strategy)
        {
            var item = await _context.ConstructionBudgetItems.FindAsync(id);
            if (item == null) return NotFound();

            item.ProcurementStrategy = (ProcurementStrategy)strategy;
            
            // If they select Internal Transfer or Borrow, mark it as Estimated/Done
            if (item.ProcurementStrategy == ProcurementStrategy.InternalTransfer || item.ProcurementStrategy == ProcurementStrategy.Borrow)
            {
                item.QuoteStatus = BudgetQuoteStatus.EstimatedOrQuoted;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { projectId = item.ConstructionProjectId });
        }

        [HttpPost("PhaseOne/RequestQuote/{id}")]'''
content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)