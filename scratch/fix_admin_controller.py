import codecs
import re

path = 'GMK360.Web/Controllers/SystemPhaseTemplateController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'public async Task<IActionResult> Add\(int phaseCategory, string subCategory, string itemName, bool isQuoteRequired\)'
replacement = r'public async Task<IActionResult> Add(int phaseCategory, string subCategory, string itemName)'
content = re.sub(target, replacement, content)

target2 = r'IsQuoteRequired = isQuoteRequired'
replacement2 = r'IsQuoteRequired = false'
content = re.sub(target2, replacement2, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)