import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Extract just the @foreach and @if related to quotes to see the nesting
lines = content.split('\n')
for i, line in enumerate(lines):
    if '@foreach' in line or 'relatedQuote' in line or 'childQuote' in line or '<tr>' in line or '</tr>' in line or 'Alınan Fiyat' in line:
        if not 'bi-arrow-return-right' in line:
            print(f"{i}: {line.strip()[:100]}")