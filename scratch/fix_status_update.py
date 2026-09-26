import re

ctrl_path = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with open(ctrl_path, 'r', encoding='utf-8') as f:
    ctrl_content = f.read()

pattern = r'doc\.Status = \(GMK360\.Core\.Entities\.Construction\.LegalDocumentStatus\)statusId;'
replacement = '''switch(statusId) {
                case 0: doc.Status = "Bekliyor"; break;
                case 1: doc.Status = "İşlemde"; break;
                case 2: doc.Status = "Sorunlu"; break;
                case 3: doc.Status = "Tamamlandı"; break;
                default: doc.Status = "Bekliyor"; break;
            }'''

ctrl_content = re.sub(pattern, replacement, ctrl_content)

with open(ctrl_path, 'w', encoding='utf-8-sig') as f:
    f.write(ctrl_content)
