import codecs
import re

path = 'GMK360.Core/Entities/Construction/ConstructionProjectExpense.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('public int ProjectId { get; set; }', 'public int? ProjectId { get; set; }\n        public string? PhotoPath { get; set; }')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Entity updated.')