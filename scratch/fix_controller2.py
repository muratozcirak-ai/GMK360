import sys
filepath = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    lines = f.readlines()

# Let's inspect where addedNew is
start = -1
end = -1
for i, line in enumerate(lines):
    if "bool addedNew = false;" in line:
        start = i
    if "if(addedNew) await _context.SaveChangesAsync();" in line:
        end = i
        break

if start != -1 and end != -1:
    # replace everything between start and end
    new_lines = lines[:start+1]
    new_lines.append("""
            foreach(var rule in globalRules)
            {
                if (!existingDocs.Contains(rule.SystemLegalDocumentTemplateId))
                {
                    _context.ProjectLegalDocuments.Add(new ProjectLegalDocument
                    {
                        ConstructionProjectId = id,
                        SystemTemplateId = rule.SystemLegalDocumentTemplateId,
                        DocumentName = rule.SystemLegalDocumentTemplate?.Name ?? "Bilinmeyen Evrak",
                        Stage = rule.Stage,
                        DisplayOrder = rule.DisplayOrder,
                        Status = "Bekliyor"
                    });
                    addedNew = true;
                }
            }
""")
    new_lines.extend(lines[end:])
    with open(filepath, 'w', encoding='utf-8-sig') as f:
        f.writelines(new_lines)
