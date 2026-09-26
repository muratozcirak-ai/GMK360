import re

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

# Eski Bina target
eski_target = '<h6 class="fw-bold mb-0 text-muted">Arazi ve Dış Alanlar (Mevcut)</h6>'
eski_replace = '<h6 class="fw-bold mb-0 text-muted">Arazi ve Dış Alanlar (Mevcut)</h6>\n                                <a asp-action="Amenities" asp-route-projectId="@Model.Id" asp-route-isExisting="true" class="btn btn-sm btn-outline-danger rounded-pill"><i class="bi bi-pencil me-1"></i> Yönet</a>'

html = re.sub(r'<h6 class="fw-bold mb-0 text-muted">Arazi ve Dış Alanlar \(Mevcut\)</h6>', eski_replace, html)

# Yeni Bina button needs asp-route-isExisting="false"
yeni_target = '<a asp-action="Amenities" asp-route-projectId="@Model.Id" class="btn btn-sm btn-outline-primary rounded-pill"><i class="bi bi-pencil me-1"></i> Yönet</a>'
yeni_replace = '<a asp-action="Amenities" asp-route-projectId="@Model.Id" asp-route-isExisting="false" class="btn btn-sm btn-outline-primary rounded-pill"><i class="bi bi-pencil me-1"></i> Yönet</a>'

html = html.replace(yeni_target, yeni_replace)

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)

print("SUCCESS")
