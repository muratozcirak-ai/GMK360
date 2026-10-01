import io
import re

filepath = r'GMK360.Core\Entities\Construction\DailyTimesheet.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_lines = [line for line in lines if 'ProjectPhaseId' not in line and 'ProjectPhase' not in line and 'PhaseTaskId' not in line and 'PhaseTask' not in line]

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.writelines(new_lines)
