import codecs
import re
import glob

correct_map = {
    'PhaseOne': 'YikimVeZeminHazirligi',
    'PhaseTwo': 'TemelVeAltYapi',
    'PhaseThree': 'KabaInsaatKarkas',
    'PhaseFour': 'CatiVeDisCephe',
    'PhaseFive': 'InceIslerIcMekan',
    'PhaseSix': 'ElektrikVeZayifAkim',
    'PhaseSeven': 'MekanikTesisatVeMakine',
    'PhaseEight': 'PeyzajVeTeslim'
}

wrong_map = {
    'PhaseOne': 'YikimVeZeminHazirligi',
    'PhaseTwo': 'KabaInsaat',
    'PhaseThree': 'CatıVeYalitim',
    'PhaseFour': 'InceInsaatVeTesisat',
    'PhaseFive': 'DisCepheVePencere',
    'PhaseSix': 'ZeminVeIcMekan',
    'PhaseSeven': 'PeyzajVeCevre',
    'PhaseEight': 'TestVeTeslim'
}

ctrls = glob.glob('GMK360.Web/Controllers/Phase*Controller.cs')
for path in ctrls:
    if 'PhaseZero' in path: continue
    
    phase_name = ""
    for k in correct_map.keys():
        if k in path: phase_name = k

    if not phase_name: continue
    
    wrong_enum = wrong_map[phase_name]
    correct_enum = correct_map[phase_name]

    with codecs.open(path, 'r', 'utf-8-sig') as f:
        content = f.read()

    if wrong_enum != correct_enum:
        content = content.replace(f'BudgetPhaseCategory.{wrong_enum}', f'BudgetPhaseCategory.{correct_enum}')
        with codecs.open(path, 'w', 'utf-8-sig') as f:
            f.write(content)
        print(f"Fixed {path}")