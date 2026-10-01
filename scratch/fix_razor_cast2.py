import codecs

path = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('@if (ViewBag.Stakeholders != null && ((IEnumerable<dynamic>)ViewBag.Stakeholders).Any())', '@{ var stakeholdersList = ViewBag.Stakeholders as List<GMK360.Core.Entities.Construction.ProjectStakeholder>; }\n                              @if (stakeholdersList != null && stakeholdersList.Any())')
content = content.replace('foreach (var st in (IEnumerable<dynamic>)ViewBag.Stakeholders)', 'foreach (var st in stakeholdersList)')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print("Replaced!")