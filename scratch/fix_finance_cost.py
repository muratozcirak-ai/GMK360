import codecs
import re

path = 'GMK360.Web/Controllers/ProjectFinanceController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'PlannedUnitPrice = d\.EstimatedCost \?\? 0,'
replacement = 'PlannedUnitPrice = (d.EstimatedCost ?? 0) + (d.DocumentFee ?? 0) + (d.AdditionalCost ?? 0),'
content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)