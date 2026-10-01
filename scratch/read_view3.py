import io
filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

import re
match = re.search(r'<tbody>(.*?)</tbody>', content, re.DOTALL)
if match:
    print(match.group(1)[:2000])
