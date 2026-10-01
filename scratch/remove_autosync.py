import io
import re

filepath = r'GMK360.Web\Controllers\ConstructionProjectController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Pattern to remove lines 172-205 (the Faz 0 Evraklari kontrol et ve otomatik ekle logic)
pattern = r'(?s)// Faz 0 Evraklari kontrol et ve otomatik ekle.*?if\(addedNew\) await _context\.SaveChangesAsync\(\);\s*'
content = re.sub(pattern, '', content)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Removed old auto-sync logic.")
