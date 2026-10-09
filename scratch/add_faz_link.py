import re

with open(r'GMK360.Web\Views\Shared\_ConstructionLayout.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Find the insertion point for 'Aktif Fazlar'
# It should be in ŞANTİYE & PROJELER, under Şirket Garajı & Araçlar
garaj_item = '<a href="/CompanyVehicles/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-primary"></i> Şirket Garajı & Araçlar</a>'
faz_item = '<a href="/ConstructionPhases/Active" class="list-group-item list-group-item-action border-0 ps-5 fw-bold"><i class="bi bi-arrow-right-short text-primary"></i> Aktif Fazlar (İş Akışı)</a>'

content = content.replace(garaj_item, garaj_item + '\n                                    ' + faz_item)

with open(r'GMK360.Web\Views\Shared\_ConstructionLayout.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
