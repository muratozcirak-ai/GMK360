import io
import re

filepath = r'GMK360.Web\Controllers\CustomerPortalController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('.ThenInclude(ph => ph.PhaseTasks)', '')

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
