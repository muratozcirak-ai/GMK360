import codecs
import re

with codecs.open('GMK360.Web/Controllers/PhaseTwoController.cs', 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('PhaseTwoController', 'PhaseOneController')
content = content.replace('PhaseTwo', 'PhaseOne')
content = content.replace('BudgetPhaseCategory.TemelVeAltYapi', 'BudgetPhaseCategory.YikimVeZeminHazirligi')
content = content.replace('PhaseCategory == (BudgetPhaseCategory)3', 'PhaseCategory == (BudgetPhaseCategory)2')

with codecs.open('GMK360.Web/Controllers/PhaseOneController.cs', 'w', 'utf-8-sig') as f:
    f.write(content)