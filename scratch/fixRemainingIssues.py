import re
import sys

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

# 1. Fix old building image
html = html.replace('Model.CurrentStateImageUrl', 'ViewBag.CurrentStateImageUrl')

# 2. Add Ynet to New Building if missing
new_target = r'<h6 class="fw-bold mb-0 text-muted">Arazi ve D.. Alanlar \(Hedef\)</h6>'
new_replace = '<h6 class="fw-bold mb-0 text-muted">Arazi ve Dış Alanlar (Hedef)</h6>\n                                <a asp-action="Amenities" asp-route-projectId="@Model.Id" asp-route-isExisting="false" class="btn btn-sm btn-outline-primary rounded-pill"><i class="bi bi-pencil me-1"></i> Yönet</a>'
if 'asp-route-isExisting="false"' not in html:
    html = re.sub(new_target, new_replace, html)

# 3. Fix Mojibake in 'Ak Konular'
html = html.replace('AÃ§Ä±k Konular / Ã–n HazÄ±rlÄ±k Talepleri', 'Açık Konular / Ön Hazırlık Talepleri')
html = html.replace('HenÃ¼z tartÄ±ÅŸmaya aÃ§Ä±lmÄ±ÅŸ bir konu veya Ã¶n talep bulunmuyor.', 'Henüz tartışmaya açılmış bir konu veya ön talep bulunmuyor.')

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)
print("SUCCESS")
