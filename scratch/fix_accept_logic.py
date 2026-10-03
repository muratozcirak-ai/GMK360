import codecs
import re

path = 'GMK360.Web/Controllers/PhaseZeroController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'invite\.Status = GMK360\.Core\.Entities\.B2B\.QuoteInviteStatus\.Accepted;\s*document\.EstimatedCost = invite\.OfferedPrice;'
replacement = '''invite.IsFeasibilitySelected = true;
            document.EstimatedCost = invite.OfferedPrice;'''
content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)