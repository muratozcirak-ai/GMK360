import codecs
import re

path = 'GMK360.Web/Controllers/PhaseZeroController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'InstitutionContact = null,\s*Status = "Bekliyor",\s*DisplayOrder = rule\.DisplayOrder\s*\}'
replacement = '''InstitutionContact = null,
                        Status = "Bekliyor",
                        DisplayOrder = rule.DisplayOrder,
                        EstimatedCost = _context.ProjectLegalDocuments
                            .Where(x => x.SystemTemplateId == rule.SystemLegalDocumentTemplateId && x.EstimatedCost > 0)
                            .OrderByDescending(x => x.CreatedAt)
                            .Select(x => x.EstimatedCost)
                            .FirstOrDefault()
                    }'''

content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)