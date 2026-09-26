import sys

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

# 1. Remove from Mevcut
mevcut_target = '<h6 class="fw-bold mb-0 text-muted">Arazi ve Dış Alanlar (Mevcut)</h6>\n                                <a asp-action="Amenities" asp-route-projectId="@Model.Id" class="btn btn-sm btn-outline-primary rounded-pill"><i class="bi bi-pencil me-1"></i> Yönet</a>'
mevcut_replace = '<h6 class="fw-bold mb-0 text-muted">Arazi ve Dış Alanlar (Mevcut)</h6>'

if mevcut_target not in html:
    print("MEVCUT NOT FOUND EXACTLY, TRYING REGEX")
    import re
    html = re.sub(r'<h6 class="fw-bold mb-0 text-muted">Arazi ve Dış Alanlar \(Mevcut\)</h6>\s*<a asp-action="Amenities"[^>]*>.*?</a>', r'<h6 class="fw-bold mb-0 text-muted">Arazi ve Dış Alanlar (Mevcut)</h6>', html)
else:
    html = html.replace(mevcut_target, mevcut_replace)

# 2. Add to Hedef
hedef_target = '<h6 class="fw-bold mb-0 text-muted">Arazi ve Dış Alanlar (Hedef)</h6>'
hedef_replace = '<h6 class="fw-bold mb-0 text-muted">Arazi ve Dış Alanlar (Hedef)</h6>\n                                <a asp-action="Amenities" asp-route-projectId="@Model.Id" class="btn btn-sm btn-outline-primary rounded-pill"><i class="bi bi-pencil me-1"></i> Yönet</a>'

if hedef_target not in html:
    print("HEDEF NOT FOUND EXACTLY, TRYING REGEX")
    import re
    html = re.sub(r'<h6 class="fw-bold mb-0 text-muted">Arazi ve Dış Alanlar \(Hedef\)</h6>', hedef_replace, html)
else:
    html = html.replace(hedef_target, hedef_replace)

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)

print("SUCCESS")
