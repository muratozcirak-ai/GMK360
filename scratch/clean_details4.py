import io
import re

filepath = r'GMK360.Web\Views\ConstructionProject\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_lines = []
skip = False
for line in lines:
    if 'id="collapseFaz0"' in line or 'drafts' in line or 'recentPhotos' in line or 'SentAt' in line:
        continue
    new_lines.append(line)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.writelines(new_lines)
