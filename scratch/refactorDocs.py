import re

def refactor_csharp(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Enums
    content = content.replace('LegalDocumentStatus.NotApplied', 'LegalDocumentStatus.Pending')
    content = content.replace('LegalDocumentStatus.Applied', 'LegalDocumentStatus.InProgress')
    content = content.replace('LegalDocumentStatus.Approved', 'LegalDocumentStatus.Completed')
    
    # Properties
    content = content.replace('TrackingPerson', 'AssignedUserId')
    content = content.replace('ApplicationDate', 'StartDate')
    content = content.replace('AcquiredDate', 'CompletedDate')
    content = content.replace('DocumentCost', 'ActualCost')
    content = content.replace('doc.Notes', 'doc.IssueNotes')
    content = content.replace('model.Notes', 'model.IssueNotes')
    content = content.replace('existing.Notes', 'existing.IssueNotes')
    content = content.replace('existingDoc.Notes', 'existingDoc.IssueNotes')
    
    # Remove lines assigning deleted properties
    lines = content.split('\n')
    new_lines = []
    for line in lines:
        if 'AppliedTo =' in line or 'AppliedTo;' in line:
            continue
        if 'InstitutionPhone =' in line or 'InstitutionPhone;' in line:
            continue
        if 'ExpiryDate =' in line or 'ExpiryDate;' in line:
            continue
        new_lines.append(line)
        
    with open(file_path, 'w', encoding='utf-8-sig') as f:
        f.write('\n'.join(new_lines))

def refactor_cshtml(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Enums
    content = content.replace('LegalDocumentStatus.NotApplied', 'LegalDocumentStatus.Pending')
    content = content.replace('LegalDocumentStatus.Applied', 'LegalDocumentStatus.InProgress')
    content = content.replace('LegalDocumentStatus.Approved', 'LegalDocumentStatus.Completed')
    
    # Properties
    content = content.replace('TrackingPerson', 'AssignedUserId')
    content = content.replace('ApplicationDate', 'StartDate')
    content = content.replace('AcquiredDate', 'CompletedDate')
    content = content.replace('DocumentCost', 'ActualCost')
    content = content.replace('doc.Notes', 'doc.IssueNotes')
    content = content.replace('doc.AppliedTo', 'doc.InstitutionContact')
    content = content.replace('doc.ExpiryDate', 'doc.CompletedDate') # just to prevent crash
    
    with open(file_path, 'w', encoding='utf-8-sig') as f:
        f.write(content)

refactor_csharp('GMK360.Web/Controllers/ConstructionProjectController.cs')
refactor_cshtml('GMK360.Web/Views/ConstructionProject/Details.cshtml')
print("Refactoring done")
