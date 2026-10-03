import codecs

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Let's find all occurrences of 'Asım Hafriyat' or something? No, that's data.
# Let's find childQuote variable.
lines = content.split('\n')
for i, line in enumerate(lines):
    if 'childQuote' in line:
        print(f"Line {i}: {line.strip()[:150]}")