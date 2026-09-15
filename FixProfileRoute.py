import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ProfileController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('[HttpPost]\r\n        [Authorize]\r\n        [IgnoreAntiforgeryToken]', '[HttpPost("Profile/AcceptMapConsent")]\r\n        [Authorize]\r\n        [IgnoreAntiforgeryToken]')
content = content.replace('[HttpPost]\n        [Authorize]\n        [IgnoreAntiforgeryToken]', '[HttpPost("Profile/AcceptMapConsent")]\n        [Authorize]\n        [IgnoreAntiforgeryToken]')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
