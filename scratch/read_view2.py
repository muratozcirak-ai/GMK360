import io
filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    lines = f.readlines()

for i, line in enumerate(lines):
    if "<td>" in line and "doc.SystemTemplate.Name" in line:
        print("".join(lines[i-3:i+30]))
        break
