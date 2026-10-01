import codecs

path = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace the dynamic casting with strong typing
target = '''@if (ViewBag.Stakeholders != null && ((IEnumerable<dynamic>)ViewBag.Stakeholders).Any())
                              {
                                  foreach (var st in (IEnumerable<dynamic>)ViewBag.Stakeholders)'''
                                  
replacement = '''@{ var stakeholdersList = ViewBag.Stakeholders as List<GMK360.Core.Entities.Construction.ProjectStakeholder>; }
                              @if (stakeholdersList != null && stakeholdersList.Any())
                              {
                                  foreach (var st in stakeholdersList)'''
                                  
if target in content:
    content = content.replace(target, replacement)
    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.write(content)
    print("Fixed Razor casting successfully!")
else:
    print("Target not found in Razor view.")