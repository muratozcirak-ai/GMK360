filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs'
with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

content = content.replace('project.StatusId = model.StatusId;', 'project.Status = (GMK360.Core.Entities.Construction.ProjectStatus)model.StatusId;')
content = content.replace('project.StatusId = GMK360.Core.Entities.Construction.ProjectConstants.StatusTeklif;', 'project.Status = GMK360.Core.Entities.Construction.ProjectStatus.Projelendirme_Teklif;')
content = content.replace('p.StatusId == 1', 'p.Status == GMK360.Core.Entities.Construction.ProjectStatus.Aktif_Santiye')
content = content.replace('p.StatusId == GMK360.Core.Entities.Construction.ProjectConstants.StatusAktif', 'p.Status == GMK360.Core.Entities.Construction.ProjectStatus.Aktif_Santiye')
content = content.replace('p.StatusId == 2', 'p.Status == GMK360.Core.Entities.Construction.ProjectStatus.Tamamlandi_Teslim')
content = content.replace('p.StatusId == GMK360.Core.Entities.Construction.ProjectConstants.StatusTamamlandi', 'p.Status == GMK360.Core.Entities.Construction.ProjectStatus.Tamamlandi_Teslim')
content = content.replace('project.StatusId = 1;', 'project.Status = GMK360.Core.Entities.Construction.ProjectStatus.Aktif_Santiye;')
content = content.replace('p.StatusId == 0', 'p.Status == GMK360.Core.Entities.Construction.ProjectStatus.Projelendirme_Teklif')
content = content.replace('project.StatusId = 0;', 'project.Status = GMK360.Core.Entities.Construction.ProjectStatus.Projelendirme_Teklif;')
content = content.replace('project.StatusId == 0', 'project.Status == GMK360.Core.Entities.Construction.ProjectStatus.Projelendirme_Teklif')

# Also duplicate validate token
content = content.replace('[HttpPost]\n        [ValidateAntiForgeryToken]\n        [ValidateAntiForgeryToken]', '[HttpPost]\n        [ValidateAntiForgeryToken]')

with open(filepath, 'w', encoding='utf-8-sig') as f:
    f.write(content)
