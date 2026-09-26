with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

# Add isExisting=true to the Mevcut button
target = '<a asp-action="Amenities" asp-route-projectId="@Model.Id" class="btn btn-sm btn-outline-primary rounded-pill"><i class="bi bi-pencil me-1"></i> Yönet</a>'
replace = '<a asp-action="Amenities" asp-route-projectId="@Model.Id" asp-route-isExisting="true" class="btn btn-sm btn-outline-danger rounded-pill"><i class="bi bi-pencil me-1"></i> Yönet</a>'

html = html.replace(target, replace)

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)
