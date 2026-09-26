import re

ctrl_file = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with open(ctrl_file, 'r', encoding='utf-8') as f:
    ctrl_content = f.read()

ctrl_content = re.sub(r'GMK360\.Core\.Entities\.Construction\.LegalDocumentStatus\.[a-zA-Z]+', '"Sorunlu"', ctrl_content)

with open(ctrl_file, 'w', encoding='utf-8-sig') as f:
    f.write(ctrl_content)

view_file = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'
with open(view_file, 'r', encoding='utf-8') as f:
    view_content = f.read()

view_content = view_content.replace('selected="@(doc.Status == "Bekliyor")"', 'selected="@(doc.Status?.ToString() == "Bekliyor")"')
view_content = view_content.replace('selected="@(doc.Status == "İşlemde")"', 'selected="@(doc.Status?.ToString() == "İşlemde")"')
view_content = view_content.replace('selected="@(doc.Status == "Tamamlandı")"', 'selected="@(doc.Status?.ToString() == "Tamamlandı")"')
view_content = view_content.replace('selected="@(doc.Status == "Sorunlu")"', 'selected="@(doc.Status?.ToString() == "Sorunlu")"')

view_content = re.sub(r'@\(doc\.Status == "([^"]+)"\)', r'@(doc.Status?.ToString() == "\1")', view_content)

# Fix razor syntax issue at top of file
view_content = view_content.replace('@model ', '@model ')

with open(view_file, 'w', encoding='utf-8-sig') as f:
    f.write(view_content)
