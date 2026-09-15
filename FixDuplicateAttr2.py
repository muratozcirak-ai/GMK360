import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('[HttpPost]\r\n        [ValidateAntiForgeryToken]\r\n                [HttpPost]\r\n        [ValidateAntiForgeryToken]', '[HttpPost]\r\n        [ValidateAntiForgeryToken]')
content = content.replace('[HttpPost]\n        [ValidateAntiForgeryToken]\n                [HttpPost]\n        [ValidateAntiForgeryToken]', '[HttpPost]\n        [ValidateAntiForgeryToken]')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
