import codecs

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

lines = content.split('\n')
i = 288
block = ''
for j in range(i, min(i+40, len(lines))):
    block += lines[j] + '\n'

with codecs.open('scratch/child_block.txt', 'w', 'utf-8-sig') as f:
    f.write(block)