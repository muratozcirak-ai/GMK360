import re

files = [
    'GMK360.Web/Controllers/ConstructionProjectController.cs',
    'GMK360.Web/Views/ConstructionProject/Details.cshtml'
]

for filepath in files:
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
            
        content = content.replace('GMK360.Core.Entities.Construction."Bekliyor"', '"Bekliyor"')
        content = content.replace('GMK360.Core.Entities.Construction."İşlemde"', '"İşlemde"')
        content = content.replace('GMK360.Core.Entities.Construction."Tamamlandı"', '"Tamamlandı"')
        content = content.replace('GMK360.Core.Entities.Construction.LegalDocumentStatus.Issue', '"Sorunlu"')
        
        with open(filepath, 'w', encoding='utf-8-sig') as f:
            f.write(content)
    except Exception as e:
        print(e)
