import codecs
import re

path = 'GMK360.Core/Entities/Construction/ProjectLegalDocument.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

if 'public BudgetPhaseCategory? LinkedPhaseCategory' not in content:
    content = re.sub(
        r'(public decimal\? AdditionalCost \{ get; set; \}.*?\n)',
        r'\1\n        // Hedef Faz Bağlantısı (Akıllı Köprü)\n        public BudgetPhaseCategory? LinkedPhaseCategory { get; set; }\n',
        content
    )
    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.write(content)
    print("Added to model")