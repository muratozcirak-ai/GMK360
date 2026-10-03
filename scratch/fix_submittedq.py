import codecs

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    lines = f.readlines()

for i in range(len(lines)):
    if 'var submittedQ = relatedQuote?.Invites.Count(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted' in lines[i]:
        lines[i] = lines[i].replace('i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted', 'i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted || i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Accepted')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.writelines(lines)