import io
import re
filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

match = re.search(r'<tr data-bs-toggle=.*?</tr>', content, re.DOTALL)
if match:
    with io.open("scratch/tr_out.txt", "w", encoding="utf-8") as out:
        out.write(match.group(0))
