import sys

with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'r', encoding='utf-8') as f:
    code = f.read()

code = code.replace(
    'public async Task<IActionResult> AddCustomLegalDocument(int projectId, string documentName, string appliedTo, string institutionPhone, string trackingPerson)',
    'public async Task<IActionResult> AddCustomLegalDocument(int projectId, string customName, string appliedTo, string institutionPhone, string trackingPerson, string stage)'
)

code = code.replace(
    'DocumentName = documentName,',
    'DocumentName = customName,\n                Stage = stage,'
)

with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'w', encoding='utf-8') as f:
    f.write(code)

print("SUCCESS")
