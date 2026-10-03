import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace ANY instance of checking for Submitted status with our new logic:
# i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted
# But wait, there might be spaces.
content = re.sub(r'i\s*=>\s*i\.Status\s*==\s*GMK360\.Core\.Entities\.B2B\.QuoteInviteStatus\.Submitted', 'i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted || (i.OfferNotes != null && i.OfferNotes.Contains("KEŞİF"))', content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)