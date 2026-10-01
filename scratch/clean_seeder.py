import io
import re

filepath = r'GMK360.Data\Seeds\DemoSeeder.cs'
try:
    with io.open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # The seeder has references to ProjectPhases and PhaseTasks.
    # We will just remove lines containing 'ProjectPhase' or 'PhaseTask' completely.
    lines = content.split('\n')
    new_lines = [l for l in lines if 'ProjectPhase' not in l and 'PhaseTask' not in l]

    with io.open(filepath, 'w', encoding='utf-8') as f:
        f.write('\n'.join(new_lines))
    print("Cleaned DemoSeeder.cs")
except Exception as e:
    print(e)
