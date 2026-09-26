import re

filepath = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the query in Details action
pattern = r'var globalTemplates = await _context\.SystemLegalDocumentTemplates\.Where\(t => t\.TargetModule == "Construction"\)\.ToListAsync\(\);'
replacement = '''var globalRules = await _context.ModuleDocumentRules
                .Include(r => r.SystemLegalDocumentTemplate)
                .Where(r => r.TargetModule == "Construction")
                .ToListAsync();'''

content = re.sub(pattern, replacement, content)

# Also in the loop:
# foreach(var template in globalTemplates) -> foreach(var rule in globalRules)
# template.Id -> rule.SystemLegalDocumentTemplateId
# template.Name -> rule.SystemLegalDocumentTemplate.Name
# template.Stage -> rule.Stage
# template.DisplayOrder -> rule.DisplayOrder

loop_pattern = r'foreach\s*\(\s*var template in globalTemplates\s*\)\s*\{[\s\S]*?\}'
loop_replacement = '''foreach(var rule in globalRules)
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
            }'''

content = re.sub(loop_pattern, loop_replacement, content)

with open(filepath, 'w', encoding='utf-8-sig') as f:
    f.write(content)
