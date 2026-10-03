import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# I injected: @{ var childQuote = ...; }
# I need to replace it with: var childQuote = ...;
target = r'@\{\s*var childQuote = \(ViewBag\.Quotes as IEnumerable<GMK360\.Core\.Entities\.B2B\.B2BQuoteRequest>\)\?\.FirstOrDefault\(q => q\.SourceReferenceId == childDoc\.Id\);\s*\}'
replacement = r'var childQuote = (ViewBag.Quotes as IEnumerable<GMK360.Core.Entities.B2B.B2BQuoteRequest>)?.FirstOrDefault(q => q.SourceReferenceId == childDoc.Id);'

content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)