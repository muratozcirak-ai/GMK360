import sys
filepath = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

import re

# We need to find the block starting at `var existingDocs = await _context.ProjectLegalDocuments...`
# and ending at `if(addedNew) await _context.SaveChangesAsync();`

start_marker = "var existingDocs = await _context.ProjectLegalDocuments.Where(d => d.ConstructionProjectId == id).Select(d => d.SystemTemplateId).ToListAsync();"
end_marker = "if(addedNew) await _context.SaveChangesAsync();"

if start_marker in content and end_marker in content:
    start_idx = content.find(start_marker)
    end_idx = content.find(end_marker) + len(end_marker)
    
    old_block = content[start_idx:end_idx]
    
    new_block = """// Dinamik Senkronizasyon (Faz 0)
            var projectDocs = await _context.ProjectLegalDocuments.Where(d => d.ConstructionProjectId == id).ToListAsync();
            var existingDocMap = projectDocs.Where(d => d.SystemTemplateId.HasValue).ToDictionary(d => d.SystemTemplateId.Value, d => d);
            
            var globalRules = await _context.ModuleDocumentRules
                .Include(r => r.SystemLegalDocumentTemplate)
                .Include(r => r.Prerequisites)
                    .ThenInclude(p => p.PrerequisiteTemplate)
                .Where(r => r.TargetModule == "Construction")
                .ToListAsync();

            bool changed = false;
            var validTemplateIds = new HashSet<int>();

            foreach(var rule in globalRules)
            {
                if (rule.SystemLegalDocumentTemplateId > 0)
                {
                    validTemplateIds.Add(rule.SystemLegalDocumentTemplateId);
                    
                    if (existingDocMap.TryGetValue(rule.SystemLegalDocumentTemplateId, out var existingMain))
                    {
                        // Guncelle
                        if (existingMain.Stage != rule.Stage || existingMain.DisplayOrder != rule.DisplayOrder)
                        {
                            existingMain.Stage = rule.Stage;
                            existingMain.DisplayOrder = rule.DisplayOrder;
                            _context.ProjectLegalDocuments.Update(existingMain);
                            changed = true;
                        }
                    }
                    else
                    {
                        // Ekle
                        _context.ProjectLegalDocuments.Add(new ProjectLegalDocument
                        {
                            ConstructionProjectId = id.Value,
                            SystemTemplateId = rule.SystemLegalDocumentTemplateId,
                            DocumentName = rule.SystemLegalDocumentTemplate?.Name ?? "Bilinmeyen Evrak",
                            Stage = rule.Stage,
                            DisplayOrder = rule.DisplayOrder,
                            Status = "Bekliyor"
                        });
                        changed = true;
                    }
                }

                if (rule.Prerequisites != null)
                {
                    foreach (var pr in rule.Prerequisites)
                    {
                        if (pr.PrerequisiteTemplateId > 0)
                        {
                            validTemplateIds.Add(pr.PrerequisiteTemplateId);
                            
                            if (existingDocMap.TryGetValue(pr.PrerequisiteTemplateId, out var existingPr))
                            {
                                // Guncelle
                                if (existingPr.Stage != rule.Stage || existingPr.DisplayOrder != (rule.DisplayOrder - 1))
                                {
                                    existingPr.Stage = rule.Stage;
                                    existingPr.DisplayOrder = rule.DisplayOrder - 1;
                                    _context.ProjectLegalDocuments.Update(existingPr);
                                    changed = true;
                                }
                            }
                            else
                            {
                                // Ekle
                                _context.ProjectLegalDocuments.Add(new ProjectLegalDocument
                                {
                                    ConstructionProjectId = id.Value,
                                    SystemTemplateId = pr.PrerequisiteTemplateId,
                                    DocumentName = pr.PrerequisiteTemplate?.Name ?? "Bilinmeyen Ön Koşul Evrakı",
                                    Stage = rule.Stage,
                                    DisplayOrder = rule.DisplayOrder - 1,
                                    Status = "Bekliyor"
                                });
                                changed = true;
                            }
                        }
                    }
                }
            }

            // Temizlik: Eger bir evrak artik Global Kurallarda yoksa ve SystemTemplateId'si varsa, projeden de sil
            var obsoleteDocs = projectDocs.Where(d => d.SystemTemplateId.HasValue && !validTemplateIds.Contains(d.SystemTemplateId.Value)).ToList();
            if (obsoleteDocs.Any())
            {
                _context.ProjectLegalDocuments.RemoveRange(obsoleteDocs);
                changed = true;
            }

            if(changed) await _context.SaveChangesAsync();"""
            
    content = content.replace(old_block, new_block)
    
    with open(filepath, 'w', encoding='utf-8-sig') as f:
        f.write(content)
else:
    print("Could not find the markers!")
