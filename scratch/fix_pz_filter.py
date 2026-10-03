import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace the condition in the @if and @foreach
target1 = r'i => i\.Status == GMK360\.Core\.Entities\.B2B\.QuoteInviteStatus\.Submitted'
replacement1 = r'i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted || (i.OfferNotes != null && i.OfferNotes.Contains("KEŞİF"))'
content = content.replace(target1, replacement1)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)