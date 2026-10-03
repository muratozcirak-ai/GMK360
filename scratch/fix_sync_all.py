import codecs
import re

path = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('_context.ConstructionBudgets', '_context.ConstructionBudgetItems')
content = content.replace('b.ProjectId == projectId', 'b.ConstructionProjectId == projectId')
content = content.replace('ProjectId = projectId,', '')
content = content.replace('IsQuoteRequired = false', '')

# Remove extra commas from the end of the object initialization
content = re.sub(r',\s*}', '\n                    }', content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)