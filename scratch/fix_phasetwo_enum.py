import codecs
import re

path = 'GMK360.Web/Controllers/PhaseTwoController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('BudgetPhaseCategory.TemelVeAltyapi', 'BudgetPhaseCategory.TemelVeAltYapi')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)