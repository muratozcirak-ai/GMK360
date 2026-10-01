import codecs

path = 'GMK360.Web/Views/Shared/_ConstructionLayout.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Add Company Garage and Project Expenses and CRM links under LOGISTICS & CRM or similar
new_links = '''
                    <div class="list-group-item bg-light text-muted fw-bold border-0 mt-3" style="font-size: 0.75rem;">LOJİSTİK & MÜŞTERİ YÖNETİMİ</div>
                    <a href="/CompanyGarage" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Controller"]?.ToString() == "CompanyGarage" ? "active" : "")">
                        <i class="bi bi-truck me-2 text-warning"></i> Şirket Garajı & Araçlar
                    </a>
                    <a href="/ConstructionProjectExpenses" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Controller"]?.ToString() == "ConstructionProjectExpenses" ? "active" : "")">
                        <i class="bi bi-receipt me-2 text-danger"></i> Proje Genel Giderleri
                    </a>
                    <a href="/ProjectCrm" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Controller"]?.ToString() == "ProjectCrm" ? "active" : "")">
                        <i class="bi bi-people me-2 text-info"></i> Potansiyel Alıcı & CRM
                    </a>
'''

# Find a good place to insert. E.g. right before "KURUMSAL & DİĞER"
if 'LOJİSTİK & MÜŞTERİ YÖNETİMİ' not in content:
    target = '<div class="list-group-item bg-light text-muted fw-bold border-0 mt-3" style="font-size: 0.75rem;">KURUMSAL & DİĞER</div>'
    content = content.replace(target, new_links + '\n                    ' + target)
    
    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.write(content)
    print('Added missing links to layout.')
else:
    print('Links already exist.')