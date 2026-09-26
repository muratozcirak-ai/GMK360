import re

filepath = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'

try:
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
        
    content = content.replace('using GMK360.Core.Entities.Construction;', '')
    content = content.replace('LegalDocumentStatus.Pending', '"Bekliyor"')
    content = content.replace('LegalDocumentStatus.InProgress', '"İşlemde"')
    content = content.replace('LegalDocumentStatus.Completed', '"Tamamlandı"')
    
    with open(filepath, 'w', encoding='utf-8-sig') as f:
        f.write(content)
except Exception as e:
    print(e)
