import re

filepath = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()
content = content.replace('\ufeff', '')
with open(filepath, 'w', encoding='utf-8-sig') as f:
    f.write(content)
    
ctrl_path = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with open(ctrl_path, 'r', encoding='utf-8') as f:
    ctrl_content = f.read()
ctrl_content = ctrl_content.replace('\ufeff', '')
ctrl_content = ctrl_content.replace('LegalDocumentStatus.Issue', '"Sorunlu"')
with open(ctrl_path, 'w', encoding='utf-8-sig') as f:
    f.write(ctrl_content)
