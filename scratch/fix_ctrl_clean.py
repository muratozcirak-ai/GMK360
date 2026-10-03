import codecs
import re

path = 'GMK360.Web/Controllers/PhaseZeroController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'\[HttpPost\]\s*public async Task<IActionResult> AddFirmQuote.*?return RedirectToAction\("Index", new \{ projectId = doc\?\.ConstructionProjectId \}\);\s*\}'
content = re.sub(target, '', content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)