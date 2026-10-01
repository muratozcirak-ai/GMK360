import codecs
path = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

import re
match = re.search(r'public async Task<IActionResult> Details.*?return View\(project\);', content, re.DOTALL)
if match:
    lines = match.group(0).split('\n')
    for i, line in enumerate(lines):
        if 'ViewBag' in line:
            print(f'Line {i}: {line.strip()}')