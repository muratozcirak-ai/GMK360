import codecs
import re

def insert_property(path, prop):
    with codecs.open(path, 'r', 'utf-8-sig') as f:
        content = f.read()
    
    if 'public string? ItemCode' not in content and 'public string ItemCode' not in content:
        # Insert after ItemName
        content = re.sub(r'(public string ItemName { get; set; }.*?)\n', r'\1\n        public string? ItemCode { get; set; } // Poz Kodu / İmalat Kodu\n', content)
        with codecs.open(path, 'w', 'utf-8-sig') as f:
            f.write(content)

insert_property('GMK360.Core/Entities/SystemPhaseTemplate.cs', 'public string? ItemCode { get; set; }')
insert_property('GMK360.Core/Entities/Construction/ConstructionBudgetItem.cs', 'public string? ItemCode { get; set; }')