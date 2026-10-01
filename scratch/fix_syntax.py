import io
import re

filepath = r'GMK360.Web\Controllers\CustomerPortalController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

pattern = r'// Insaattan son fotograflar.*?ViewBag\.RecentPhotos = recentPhotos;'
content = re.sub(pattern, 'ViewBag.RecentPhotos = new List<object>();', content, flags=re.DOTALL)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
