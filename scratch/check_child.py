import codecs

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Show the exact child row rendering logic so I can replace it
lines = content.split('\n')
for i, line in enumerate(lines):
    if 'foreach (var pr in prereqs)' in line:
        print(f"--- Line {i} ---")
        for j in range(i, min(i+50, len(lines))):
            print(lines[j])
        break
