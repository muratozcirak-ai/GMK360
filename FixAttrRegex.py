import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace any occurrence of multiple HttpPost/ValidateAntiForgeryToken sequentially
pattern = re.compile(r'\[HttpPost\]\s*\[ValidateAntiForgeryToken\]\s*\[HttpPost\]\s*\[ValidateAntiForgeryToken\]', re.MULTILINE)
content = pattern.sub('[HttpPost]\r\n        [ValidateAntiForgeryToken]', content)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
