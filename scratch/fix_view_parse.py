import sys

filepath = 'GMK360.Web/Views/ModuleDocumentRule/Index.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

replacement = """var prereqIds = item.PrerequisiteTemplateIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Where(x => int.TryParse(x, out _)).Select(int.Parse).ToList();"""

content = content.replace("""var prereqIds = item.PrerequisiteTemplateIds.Split(',').Select(int.Parse).ToList();""", replacement)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
