import re

with open('scratch/migration.sql', 'r', encoding='utf-16') as f:
    text = f.read()
    
matches = re.findall(r'CREATE TABLE \[SystemDocumentDependencies\].*?;', text, re.DOTALL)
if matches:
    print(matches[-1])
else:
    print("Not found")
