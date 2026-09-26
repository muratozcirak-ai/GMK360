import sys

filepath = 'GMK360.Web/Controllers/ConstructionProjectController.cs'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

target1 = """_context.ProjectLegalDocuments.Add(new ProjectLegalDocument
                        {
                            ConstructionProjectId = id.Value,
                            SystemTemplateId = rule.SystemLegalDocumentTemplateId,
                            DocumentName = rule.SystemLegalDocumentTemplate?.Name ?? "Bilinmeyen Evrak",
                            Stage = rule.Stage,
                            DisplayOrder = rule.DisplayOrder,
                            Status = "Bekliyor"
                        });"""

replacement1 = """var newMain = new ProjectLegalDocument
                        {
                            ConstructionProjectId = id.Value,
                            SystemTemplateId = rule.SystemLegalDocumentTemplateId,
                            DocumentName = rule.SystemLegalDocumentTemplate?.Name ?? "Bilinmeyen Evrak",
                            Stage = rule.Stage,
                            DisplayOrder = rule.DisplayOrder,
                            Status = "Bekliyor"
                        };
                        _context.ProjectLegalDocuments.Add(newMain);
                        existingDocMap[rule.SystemLegalDocumentTemplateId] = newMain;"""

target2 = """_context.ProjectLegalDocuments.Add(new ProjectLegalDocument
                                {
                                    ConstructionProjectId = id.Value,
                                    SystemTemplateId = pr.PrerequisiteTemplateId,
                                    DocumentName = pr.PrerequisiteTemplate?.Name ?? "Bilinmeyen Ön Koşul Evrakı",
                                    Stage = rule.Stage,
                                    DisplayOrder = rule.DisplayOrder - 1,
                                    Status = "Bekliyor"
                                });"""

replacement2 = """var newPrereq = new ProjectLegalDocument
                                {
                                    ConstructionProjectId = id.Value,
                                    SystemTemplateId = pr.PrerequisiteTemplateId,
                                    DocumentName = pr.PrerequisiteTemplate?.Name ?? "Bilinmeyen Ön Koşul Evrakı",
                                    Stage = rule.Stage,
                                    DisplayOrder = rule.DisplayOrder - 1,
                                    Status = "Bekliyor"
                                };
                                _context.ProjectLegalDocuments.Add(newPrereq);
                                existingDocMap[pr.PrerequisiteTemplateId] = newPrereq;"""

if "existingDocMap[rule.SystemLegalDocumentTemplateId] = newMain;" not in content:
    content = content.replace(target1, replacement1)
    content = content.replace(target2, replacement2)
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
        print("Bug fixed.")
else:
    print("Already fixed.")
