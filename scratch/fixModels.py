import re

def strip_bom(filepath):
    with open(filepath, 'rb') as f:
        data = f.read()
    while data.startswith(b'\xef\xbb\xbf'):
        data = data[3:]
    with open(filepath, 'wb') as f:
        f.write(b'\xef\xbb\xbf' + data)

strip_bom('GMK360.Web/Views/ConstructionProject/Details.cshtml')

# Fix DocumentExpiryWorker.cs
with open('GMK360.Web/BackgroundServices/DocumentExpiryWorker.cs', 'r', encoding='utf-8') as f:
    text = f.read()
text = text.replace('ExpiryDate', 'CompletedDate') # As a fallback
text = text.replace('LegalDocumentStatus.Approved', 'LegalDocumentStatus.Completed')
with open('GMK360.Web/BackgroundServices/DocumentExpiryWorker.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)

# Check ConstructionProjectController.cs for missed replacements
with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'r', encoding='utf-8') as f:
    text = f.read()
text = text.replace('AppliedTo', 'InstitutionContact')
text = text.replace('InstitutionPhone', 'InstitutionContact') # just to remove the error
text = text.replace('ExpiryDate', 'CompletedDate')
text = text.replace('LegalDocumentStatus.NotApplied', 'LegalDocumentStatus.Pending')
text = text.replace('LegalDocumentStatus.Applied', 'LegalDocumentStatus.InProgress')
text = text.replace('LegalDocumentStatus.Approved', 'LegalDocumentStatus.Completed')
text = text.replace('.Notes', '.IssueNotes')

with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)

print('Fixed')
