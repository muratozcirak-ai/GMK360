import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs'
with codecs.open(filepath, 'r', 'utf-8', errors='ignore') as f:
    content = f.read()

# 1. Latitude/Longitude in Create GET mapping
content = content.replace('model.Latitude = draft.Latitude;', 'model.Latitude = draft.Latitude?.ToString(System.Globalization.CultureInfo.InvariantCulture);')
content = content.replace('model.Longitude = draft.Longitude;', 'model.Longitude = draft.Longitude?.ToString(System.Globalization.CultureInfo.InvariantCulture);')

# 2. StatusId in ConstructionProject creation
content = content.replace('StatusId = GMK360.Core.Entities.Construction.ProjectConstants.StatusTeklif', 'Status = GMK360.Core.Entities.Construction.ProjectStatus.Projelendirme_Teklif')
content = content.replace('project.StatusId = GMK360.Core.Entities.Construction.ProjectConstants.StatusTeklif;', 'project.Status = GMK360.Core.Entities.Construction.ProjectStatus.Projelendirme_Teklif;')
content = content.replace('existingProject.StatusId = project.StatusId;', 'existingProject.Status = project.Status;')

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(content)
