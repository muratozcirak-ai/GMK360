import io
import re

filepath = r'GMK360.Web\Views\DailyTimesheets\Index.cshtml'
with io.open(filepath, 'r', encoding='latin1') as f:
    content = f.read()

content = re.sub(r't\.ProjectPhaseId', '0', content)
content = re.sub(r't\.ProjectPhase', 'null', content)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
