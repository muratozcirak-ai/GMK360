import codecs
import re

path = 'GMK360.Web/Controllers/PhaseZeroController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# We need to find the exact block and replace it correctly.
# The messed up block is:
'''
        [HttpPost("PhaseZero/RequestQuote/{documentId}")]
        [IgnoreAntiforgeryToken]
        
        [HttpPost]
        public async Task<IActionResult> AcceptQuote(int documentId, int inviteId)
'''
# Actually, I'll just use regex to extract AcceptQuote and move it before the RequestQuote attributes.

target_bad = r'(\[HttpPost\("PhaseZero/RequestQuote/\{documentId\}"\)\]\s*\[IgnoreAntiforgeryToken\])\s*(\[HttpPost\]\s*public async Task<IActionResult> AcceptQuote\(int documentId, int inviteId\)\s*\{.*?\s*return RedirectToAction\(nameof\(Index\), new \{ projectId = document\.ConstructionProjectId \} \);\s*\})\s*public async Task<IActionResult> RequestQuote\('

replacement = r'\2\n\n        \1\n        public async Task<IActionResult> RequestQuote('

content = re.sub(target_bad, replacement, content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)