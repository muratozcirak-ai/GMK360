import codecs
import re

path = 'GMK360.Web/Controllers/PhaseOneController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'\[HttpPost\("PhaseOne/DeleteItem/\{id\}"\)\]\s*\[HttpPost\("PhaseOne/SyncFromPool/\{projectId\}"\)\]'
replacement = '[HttpPost("PhaseOne/SyncFromPool/{projectId}")]'
content = re.sub(target, replacement, content)

target2 = r'\[HttpPost\("PhaseOne/DeleteItem/\{id\}"\)\]\s*\{'
replacement2 = '[HttpPost("PhaseOne/DeleteItem/{id}")]\n        public async Task<IActionResult> DeleteItem(int id)\n        {'
content = re.sub(target2, replacement2, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)