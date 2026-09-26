import re

filepath = r'GMK360.Web\Views\Admin\B2bList.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('<div class="modal-dialog modal-dialog-centered">', '<div class="modal-dialog modal-lg modal-dialog-centered">')

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
