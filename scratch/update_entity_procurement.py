import codecs
import re

path = 'GMK360.Core/Entities/Construction/ConstructionBudgetItem.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'public BudgetQuoteStatus QuoteStatus \{ get; set; \} = BudgetQuoteStatus.NotRequired;'
replacement = '''public BudgetQuoteStatus QuoteStatus { get; set; } = BudgetQuoteStatus.NotRequired;
        public ProcurementStrategy ProcurementStrategy { get; set; } = ProcurementStrategy.NotSelected;'''

content = re.sub(target, replacement, content)

target_enum = r'namespace GMK360.Core.Entities.Construction\s*\{'
replacement_enum = '''namespace GMK360.Core.Entities.Construction
{
    public enum ProcurementStrategy
    {
        NotSelected = 0,
        Purchase = 1,          // Satın Alma (B2B Teklif)
        Rent = 2,              // Aylık Kiralama
        InternalTransfer = 3,  // Kendi Depomuzdan / Eski Şantiyeden
        Borrow = 4             // Kardeş Firmadan Ödünç
    }'''
content = re.sub(target_enum, replacement_enum, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)