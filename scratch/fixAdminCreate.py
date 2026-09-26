with open('GMK360.Web/Controllers/AdminLegalDocumentController.cs', 'r', encoding='utf-8') as f:
    text = f.read()

text = text.replace('public async Task<IActionResult> Create(string Name, string TargetModule, string IssuedBy, bool IsMandatory, string LegalReference)', 
'public async Task<IActionResult> Create(string Name, string TargetModule, string IssuedBy, bool IsMandatory, string LegalReference, string Stage)')

text = text.replace('TargetModule = TargetModule,', 
'TargetModule = TargetModule,\n                Stage = Stage,')

with open('GMK360.Web/Controllers/AdminLegalDocumentController.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)
