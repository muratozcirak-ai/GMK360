import re

filepath = r'GMK360.Web\Views\ConstructionProject\Create.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

pattern1 = r'document\.addEventListener\("DOMContentLoaded", function\(\) \{ handleStatusChange\(\); \}\);'
content = re.sub(pattern1, '', content)

pattern2 = r'function handleStatusChange\(\) \{.*?\}\s*\}'
content = re.sub(pattern2, '', content, flags=re.DOTALL)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Create.cshtml scripts updated.")
