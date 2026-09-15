import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\InventoryController.cs'

with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('p.Status == 1', 'p.Status == GMK360.Core.Entities.Construction.ProjectStatus.Projelendirme_Teklif')
content = content.replace('p.Status == 2', 'p.Status == GMK360.Core.Entities.Construction.ProjectStatus.Aktif_Santiye')
content = content.replace('p.Status == 3', 'p.Status == GMK360.Core.Entities.Construction.ProjectStatus.Tamamlandi_Teslim')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
