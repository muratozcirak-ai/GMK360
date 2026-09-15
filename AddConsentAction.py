import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ProfileController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

new_method = '''
        [HttpPost]
        [Authorize]
        [IgnoreAntiforgeryToken] // Layout'tan ajax ile gelirken token uyuşmazlığı olmaması için
        public async Task<IActionResult> AcceptMapConsent()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.HasMapConsent = true;
                user.MapConsentDate = System.DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                return Ok();
            }
            return Unauthorized();
        }
'''

# Find the end of the class. I'll just insert it before the last closing brace.
content = content.replace('    }\r\n}\r\n', new_method + '\r\n    }\r\n}\r\n').replace('    }\n}\n', new_method + '\n    }\n}\n')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
