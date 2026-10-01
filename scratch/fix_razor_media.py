import io
import re

filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('@media', '@@media')

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Fixed razor @@media syntax.")
