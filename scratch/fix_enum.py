with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

content = content.replace('GMK360.Core.Entities.Construction.ConstructionProjectStatus', 'GMK360.Core.Entities.Construction.ProjectStatus')

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
