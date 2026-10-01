import io
filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    lines = f.readlines()
print(lines[145].strip())
print(lines[215].strip())
