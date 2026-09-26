import sys

filepath = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

old_query = """            var globalRules = await _context.ModuleDocumentRules
                .Include(r => r.SystemLegalDocumentTemplate)
                .Where(r => r.TargetModule == "Construction")
                .ToListAsync();"""

new_query = """            var globalRules = await _context.ModuleDocumentRules
                .Include(r => r.SystemLegalDocumentTemplate)
                .Include(r => r.Prerequisites)
                    .ThenInclude(p => p.PrerequisiteTemplate)
                .Where(r => r.TargetModule == "Construction")
                .ToListAsync();"""

content = content.replace(old_query, new_query)


old_loop = """            foreach(var rule in globalRules)
            {
                if (!existingDocs.Contains((int?)rule.SystemLegalDocumentTemplateId))
                {
                    _context.ProjectLegalDocuments.Add(new ProjectLegalDocument
                    {
                        ConstructionProjectId = id.Value,
                        SystemTemplateId = rule.SystemLegalDocumentTemplateId,
                        DocumentName = rule.SystemLegalDocumentTemplate?.Name ?? "Bilinmeyen Evrak",
                        Stage = rule.Stage,
                        DisplayOrder = rule.DisplayOrder,
                        Status = "Bekliyor"
                    });
                    addedNew = true;
                }
            }"""

new_loop = """            foreach(var rule in globalRules)
            {
                // Ana evrakı ekle
                if (!existingDocs.Contains((int?)rule.SystemLegalDocumentTemplateId))
                {
                    _context.ProjectLegalDocuments.Add(new ProjectLegalDocument
                    {
                        ConstructionProjectId = id.Value,
                        SystemTemplateId = rule.SystemLegalDocumentTemplateId,
                        DocumentName = rule.SystemLegalDocumentTemplate?.Name ?? "Bilinmeyen Evrak",
                        Stage = rule.Stage,
                        DisplayOrder = rule.DisplayOrder,
                        Status = "Bekliyor"
                    });
                    existingDocs.Add((int?)rule.SystemLegalDocumentTemplateId);
                    addedNew = true;
                }

                // Ön koşullarını da ayrı ayrı görev olarak ekle (şantiyeye çek)
                if (rule.Prerequisites != null)
                {
                    foreach (var pr in rule.Prerequisites)
                    {
                        if (!existingDocs.Contains((int?)pr.PrerequisiteTemplateId))
                        {
                            _context.ProjectLegalDocuments.Add(new ProjectLegalDocument
                            {
                                ConstructionProjectId = id.Value,
                                SystemTemplateId = pr.PrerequisiteTemplateId,
                                DocumentName = pr.PrerequisiteTemplate?.Name ?? "Bilinmeyen Ön Koşul Evrakı",
                                Stage = rule.Stage, // Aynı aşamada görünmesi için
                                DisplayOrder = rule.DisplayOrder - 1, // Ana evraktan önce görünsün diye
                                Status = "Bekliyor"
                            });
                            existingDocs.Add((int?)pr.PrerequisiteTemplateId);
                            addedNew = true;
                        }
                    }
                }
            }"""

content = content.replace(old_loop, new_loop)

with open(filepath, 'w', encoding='utf-8-sig') as f:
    f.write(content)
