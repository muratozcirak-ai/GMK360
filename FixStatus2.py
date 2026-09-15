filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs'
with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

content = content.replace('StatusId = GMK360.Core.Entities.Construction.ProjectConstants.StatusTeklif', 'Status = GMK360.Core.Entities.Construction.ProjectStatus.Projelendirme_Teklif')
content = content.replace('existingProject.StatusId = project.StatusId;', 'existingProject.Status = project.Status;')

with open(filepath, 'w', encoding='utf-8-sig') as f:
    f.write(content)
