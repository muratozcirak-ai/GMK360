import codecs

path = 'GMK360.Web/Views/Shared/_ConstructionLayout.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

new_links = '''
                    <div class="list-group-item bg-light fw-bold mt-2 text-uppercase" style="font-size: 0.8rem; color: #6c757d;">
                        LOJİSTİK & MÜŞTERİ YÖNETİMİ
                    </div>
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

target = 'KURUMSAL'
idx = content.find(target)
if idx != -1:
    # go back to the start of the div
    div_start = content.rfind('<div', 0, idx)
    content = content[:div_start] + new_links + '\n                    ' + content[div_start:]

    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.write(content)
    print('Links successfully added this time.')
else:
    print('Target not found')
