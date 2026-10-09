import codecs
import re

# 1. Update AgencyPhonebook.cs
path_pb = 'GMK360.Core/Entities/Construction/AgencyPhonebook.cs'
with codecs.open(path_pb, 'r', 'utf-8-sig') as f:
    content_pb = f.read()

if 'public bool DeductMealCost' not in content_pb:
    content_pb = re.sub(
        r'(public string\? Iban \{ get; set; \}\n)',
        r'\1\n        // --- SÖZLEŞME VE ŞANTİYE GİDERLERİ ---\n        public bool DeductMealCost { get; set; } = false; // Günlük iaşe/yemek bedeli hakedişten düşülecek mi?\n',
        content_pb
    )
    with codecs.open(path_pb, 'w', 'utf-8-sig') as f:
        f.write(content_pb)

# 2. Update ConstructionBudgetItem.cs
path_bi = 'GMK360.Core/Entities/Construction/ConstructionBudgetItem.cs'
with codecs.open(path_bi, 'r', 'utf-8-sig') as f:
    content_bi = f.read()

if 'public decimal? EstimatedMaterialCost' not in content_bi:
    content_bi = re.sub(
        r'(public decimal PlannedUnitPrice \{ get; set; \} = 0;.*?\n)',
        r'\1        public decimal? EstimatedMaterialCost { get; set; } // Malzeme Tahmini (Döviz bazlı hesaplamalar için)\n        public decimal? EstimatedLaborCost { get; set; } // İşçilik Tahmini (Yerel enflasyon için)\n',
        content_bi
    )
    with codecs.open(path_bi, 'w', 'utf-8-sig') as f:
        f.write(content_bi)

print("Entities updated safely.")