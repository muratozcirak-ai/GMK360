import codecs
import re

path = 'GMK360.Core/Entities/Construction/ConstructionBudgetItem.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'public BudgetQuoteStatus QuoteStatus \{ get; set; \} = BudgetQuoteStatus.WaitingForPrice;'
replacement = '''public BudgetQuoteStatus QuoteStatus { get; set; } = BudgetQuoteStatus.WaitingForPrice;
        public ProcurementStrategy ProcurementStrategy { get; set; } = ProcurementStrategy.NotSelected;'''

content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)