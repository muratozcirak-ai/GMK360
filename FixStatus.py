import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs'
with codecs.open(filepath, 'r', 'utf-8') as f:
    content = f.read()

import re

# Find: project = new GMK360.Core.Entities.Construction.ConstructionProject { AgencyId = agencyId.Value, Status = GMK360.Core.Entities.Construction.ProjectStatus.Projelendirme_Teklif, CreatedAt = DateTime.UtcNow };
old_code = r"project = new GMK360.Core.Entities.Construction.ConstructionProject { AgencyId = agencyId.Value, Status = GMK360.Core.Entities.Construction.ProjectStatus.Projelendirme_Teklif, CreatedAt = DateTime.UtcNow };"
new_code = r"project = new GMK360.Core.Entities.Construction.ConstructionProject { AgencyId = agencyId.Value, Status = (GMK360.Core.Entities.Construction.ProjectStatus)model.StatusId, CreatedAt = DateTime.UtcNow };"

if old_code in content:
    content = content.replace(old_code, new_code)
    print("Replaced new project initialization.")

# Find if it updates status for existing drafts in SaveStep1
# We should add: project.Status = (GMK360.Core.Entities.Construction.ProjectStatus)model.StatusId;
# after project.Name = model.Name;

insert_target = r"project.Name = model.Name;"
insert_code = r"project.Status = (GMK360.Core.Entities.Construction.ProjectStatus)model.StatusId;" + "\n                " + insert_target

if insert_target in content:
    # only replace the first occurrence (which is inside SaveStep1)
    content = content.replace(insert_target, insert_code, 1)
    print("Injected status update.")

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(content)
