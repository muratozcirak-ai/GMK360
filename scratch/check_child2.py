import codecs

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Show the second loop
occurrences = []
lines = content.split('\n')
for i, line in enumerate(lines):
    if 'foreach (var pr in prereqs)' in line:
        occurrences.append(i)

if len(occurrences) > 1:
    i = occurrences[1]
    # Replace the child row rendering block
    print(f"Second loop at {i}")