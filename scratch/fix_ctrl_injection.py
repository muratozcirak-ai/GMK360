import codecs
import re
import glob

ctrls = glob.glob('GMK360.Web/Controllers/Phase*Controller.cs')
for path in ctrls:
    if 'PhaseZero' in path: continue
    
    with codecs.open(path, 'r', 'utf-8-sig') as f:
        content = f.read()

    # Revert the accidental injection everywhere
    content = content.replace(
        'item.EstimatedMaterialCost = estimatedMaterialCost;\n            item.EstimatedLaborCost = estimatedLaborCost;\n            item.QuoteStatus = BudgetQuoteStatus.EstimatedOrQuoted;',
        'item.QuoteStatus = BudgetQuoteStatus.EstimatedOrQuoted;'
    )

    # Now carefully inject ONLY inside UpdatePrice
    # find: item.Description = description;
    # next line: item.QuoteStatus = ...
    content = re.sub(
        r'(item\.Description = description;\s+)(item\.QuoteStatus = BudgetQuoteStatus\.EstimatedOrQuoted;)',
        r'\1item.EstimatedMaterialCost = estimatedMaterialCost;\n            item.EstimatedLaborCost = estimatedLaborCost;\n            \2',
        content
    )

    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.write(content)
        
    print(f"Fixed {path}")
