import codecs
import re

phases = [
    ('PhaseOneController.cs', 'YikimVeZeminHazirligi'),
    ('PhaseTwoController.cs', 'TemelVeAltYapi'),
    ('PhaseThreeController.cs', 'KabaInsaatKarkas'),
    ('PhaseFourController.cs', 'CatiVeDisCephe'),
    ('PhaseFiveController.cs', 'InceIslerIcMekan'),
    ('PhaseSixController.cs', 'ElektrikVeZayifAkim'),
    ('PhaseSevenController.cs', 'MekanikTesisatVeMakine'),
    ('PhaseEightController.cs', 'PeyzajVeTeslim'),
]

for ctrl_file, cat_enum in phases:
    path = f'GMK360.Web/Controllers/{ctrl_file}'
    with codecs.open(path, 'r', 'utf-8-sig') as f:
        content = f.read()

    injection = f'''
            var linkedDocs = await _context.ProjectLegalDocuments
                .Where(d => d.ConstructionProjectId == projectId && d.LinkedPhaseCategory == BudgetPhaseCategory.{cat_enum})
                .ToListAsync();
            ViewBag.LinkedDocs = linkedDocs;
'''
    if 'ViewBag.LinkedDocs' not in content:
        content = re.sub(
            r'(var model = await _context\.ConstructionBudgetItems.*?\.ToListAsync\(\);\n)',
            r'\1' + injection,
            content, count=1, flags=re.DOTALL
        )
        with codecs.open(path, 'w', 'utf-8-sig') as f:
            f.write(content)
        print(f"Updated {ctrl_file}")