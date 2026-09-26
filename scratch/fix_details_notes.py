import sys
filepath = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('doc.Notes', 'doc.IssueNotes')
content = content.replace('childDoc.Notes', 'childDoc.IssueNotes')

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
