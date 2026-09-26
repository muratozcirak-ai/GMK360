import re

files = [
    'GMK360.Web/Controllers/ConstructionProjectController.cs',
    'GMK360.Web/Views/ConstructionProject/Details.cshtml',
    'GMK360.Web/BackgroundServices/DocumentExpiryWorker.cs'
]

for filepath in files:
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
            
        content = content.replace('LegalDocumentStatus.Pending', '"Bekliyor"')
        content = content.replace('LegalDocumentStatus.InProgress', '"İşlemde"')
        content = content.replace('LegalDocumentStatus.Completed', '"Tamamlandı"')
        
        with open(filepath, 'w', encoding='utf-8-sig') as f:
            f.write(content)
    except:
        pass
        
