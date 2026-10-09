import re

with open('GMK360.Web/Views/Shared/_ConstructionLayout.cshtml', 'r', encoding='utf-8') as f:
    content = f.read()

old_links = '''<a href="/DocumentArchives/Index" class="list-group-item list-group-item-action border-0 ps-5 fw-bold text-dark"><i class="bi bi-arrow-right-short text-secondary"></i> Evrak Belge Arşivi</a>
                                    <a href="/AgencyWeb/Index" class="list-group-item list-group-item-action border-0 ps-5 fw-bold text-dark"><i class="bi bi-arrow-right-short text-secondary"></i> Kurumsal Web Sitesi Yönetimi</a>'''

new_links = '''<a href="/DocumentArchive/Index?context=construction" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-secondary"></i> Evrak & Belge Arşivi</a>
                                    <a href="/Dashboard/SiteYonetimi" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-secondary"></i> Kurumsal Web Sitem (CMS)</a>'''

if old_links in content:
    content = content.replace(old_links, new_links)
    with open('GMK360.Web/Views/Shared/_ConstructionLayout.cshtml', 'w', encoding='utf-8') as f:
        f.write(content)
    print("Fixed layout links!")
else:
    print("Not found!")
