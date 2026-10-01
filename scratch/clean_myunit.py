import io
import re

filepath = r'GMK360.Web\Views\CustomerPortal\MyUnitProgress.cshtml'
with io.open(filepath, 'r', encoding='latin1') as f:
    content = f.read()

content = re.sub(r'@using GMK360\.Core\.Entities\.Construction', '', content)
content = re.sub(r'var allPhases = Model\.Project\.Phases.*?;', 'var allPhases = new List<object>();', content, flags=re.DOTALL)
content = re.sub(r'var totalPhases = allPhases\.Count;', 'var totalPhases = 0;', content)
content = re.sub(r'var completedPhases = allPhases\.Count\(p => p\.Status == 2\);', 'var completedPhases = 0;', content)
content = re.sub(r'var completedPercentage = totalPhases > 0 \? \(completedPhases \* 100\) / totalPhases : 0;', 'var completedPercentage = 0;', content)
content = re.sub(r'@foreach\s*\(var phase in allPhases.*?\)\s*\{.*?(?:<div.*?</div>\s*)+\}', '', content, flags=re.DOTALL)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
