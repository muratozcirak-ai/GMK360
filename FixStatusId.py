import codecs

files_to_fix = [
    r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\AdminController.cs',
    r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs',
    r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\DailyTimesheetsController.cs',
    r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\DashboardController.cs',
    r'C:\Users\murat\source\repos\GMK360\GMK360.Data\Seeds\DemoSeeder.cs'
]

for filepath in files_to_fix:
    content = None
    try:
        with codecs.open(filepath, 'r', 'utf-8-sig') as f:
            content = f.read()
    except:
        with codecs.open(filepath, 'r', 'cp1254') as f:
            content = f.read()

    # Replace StatusId with Status
    content = content.replace('p.StatusId ==', 'p.Status ==')
    content = content.replace('p.StatusId=', 'p.Status=')
    content = content.replace('StatusId =', 'Status =')
    
    # Replace the enum constants
    content = content.replace('GMK360.Core.Entities.Construction.ProjectConstants.StatusTeklif', 'GMK360.Core.Entities.Construction.ProjectStatus.Projelendirme_Teklif')
    content = content.replace('GMK360.Core.Entities.Construction.ProjectConstants.StatusAktif', 'GMK360.Core.Entities.Construction.ProjectStatus.Aktif_Santiye')
    content = content.replace('GMK360.Core.Entities.Construction.ProjectConstants.StatusTamamlandi', 'GMK360.Core.Entities.Construction.ProjectStatus.Tamamlandi_Teslim')
    
    content = content.replace('ProjectConstants.StatusTeklif', 'ProjectStatus.Projelendirme_Teklif')
    content = content.replace('ProjectConstants.StatusAktif', 'ProjectStatus.Aktif_Santiye')
    content = content.replace('ProjectConstants.StatusTamamlandi', 'ProjectStatus.Tamamlandi_Teslim')

    with codecs.open(filepath, 'w', 'utf-8-sig') as f:
        f.write(content)
