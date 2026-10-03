import codecs
import re

path = 'GMK360.Core/Entities/Construction/ConstructionBudgetItem.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'public string ItemName \{ get; set; \}'
replacement = 'public string? SubCategory { get; set; }\n        public string ItemName { get; set; }'

content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)