import re

filepath = r'GMK360.Web\Views\Admin\B2bList.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

jquery_script = '<script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>\n    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css"'
content = content.replace('<link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css"', jquery_script)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
