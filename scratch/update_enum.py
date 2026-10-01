import io
import re

filepath = r'GMK360.Core\Entities\Construction\ConstructionBudgetItem.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

new_enum = """
    public enum BudgetPhaseCategory
    {
        ResmiEvraklarVeProsedurler = 1,
        YikimVeZeminHazirligi = 2,
        TemelVeAltYapi = 3,
        KabaInsaatKarkas = 4,
        CatiVeDisCephe = 5,
        InceIslerIcMekan = 6,
        ElektrikVeZayifAkim = 7,
        MekanikTesisatVeMakine = 8,
        PeyzajVeTeslim = 9
    }
"""

content = re.sub(r'public enum BudgetPhaseCategory\s*\{[^}]+\}', new_enum.strip(), content)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
