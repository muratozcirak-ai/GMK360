import codecs

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

lines = content.split('\n')
for i, line in enumerate(lines):
    if 'bool isOnlyPrereq' in line or 'if (renderedDocIds.Contains' in line:
        print(f"Line {i}: {line}")