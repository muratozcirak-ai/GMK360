import codecs
import re
import os

path = 'GMK360.Web/Controllers/PhaseOneController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Remove the force seed methods
content = re.sub(r'\[HttpGet\("PhaseOne/ForceSeedPhaseTwo"\)\].*?return Content\("Phase 2 Seeded C#"\);\s*\}', '', content, flags=re.DOTALL)
content = re.sub(r'\[HttpGet\("PhaseOne/ForceSeed"\)\].*?return Content\("Seeded C#"\);\s*\}', '', content, flags=re.DOTALL)

# Replace names
content = content.replace('PhaseOneController', 'PhaseTwoController')
content = content.replace('PhaseOne', 'PhaseTwo')
content = content.replace('BudgetPhaseCategory.YikimVeZeminHazirligi', 'BudgetPhaseCategory.TemelVeAltyapi')
content = content.replace('Phase Category 2', 'Phase Category 3')

# Replace Sync logic
content = content.replace('t.PhaseCategory == (BudgetPhaseCategory)2', 't.PhaseCategory == (BudgetPhaseCategory)3')
content = content.replace('PhaseCategory == (BudgetPhaseCategory)2', 'PhaseCategory == (BudgetPhaseCategory)3')
content = content.replace('phaseCategory: 2', 'phaseCategory: 3')

with codecs.open('GMK360.Web/Controllers/PhaseTwoController.cs', 'w', 'utf-8-sig') as f:
    f.write(content)