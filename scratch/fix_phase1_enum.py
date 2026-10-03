import codecs
import re

path = 'GMK360.Web/Controllers/PhaseOneController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'SourceType = BudgetItemSourceType\.SystemTemplate,'
replacement = 'SourceType = BudgetItemSourceType.Manual,'
content = re.sub(target, replacement, content)

target2 = r'QuoteStatus = t\.IsQuoteRequired \? BudgetQuoteStatus\.WaitingForPrice : BudgetQuoteStatus\.NoQuoteRequired'
replacement2 = 'QuoteStatus = t.IsQuoteRequired ? BudgetQuoteStatus.WaitingForPrice : BudgetQuoteStatus.EstimatedOrQuoted'
content = re.sub(target2, replacement2, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)