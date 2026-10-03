import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# I will find the block starting with:
# var childQuote = (ViewBag.Quotes as IEnumerable<GMK360.Core.Entities.B2B.B2BQuoteRequest>)?.FirstOrDefault(q => q.SourceReferenceId == childDoc.Id);
# and end at the closing } of that block.
target_child_quotes = r'var childQuote = \(ViewBag\.Quotes as IEnumerable<GMK360\.Core\.Entities\.B2B\.B2BQuoteRequest>\)\?\.FirstOrDefault\(q => q\.SourceReferenceId == childDoc\.Id\);.*?@if \(childQuote != null.*?</tr>\s*\}\s*'
content = re.sub(target_child_quotes, '', content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)