import re

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

# Eski Bina target
html = re.sub(
    r'<h6 class="fw-bold mb-0 text-muted">Arazi ve D. Alanlar \(Mevcut\)</h6>',
    r'<h6 class="fw-bold mb-0 text-muted">Arazi ve Dış Alanlar (Mevcut)</h6>\n                                <a asp-action="Amenities" asp-route-projectId="@Model.Id" asp-route-isExisting="true" class="btn btn-sm btn-outline-danger rounded-pill"><i class="bi bi-pencil me-1"></i> Yönet</a>',
    html
)

# Yeni Bina target
html = re.sub(
    r'<h6 class="fw-bold mb-0 text-muted">Arazi ve D. Alanlar \(Hedef\)</h6>',
    r'<h6 class="fw-bold mb-0 text-muted">Arazi ve Dış Alanlar (Hedef)</h6>\n                                <a asp-action="Amenities" asp-route-projectId="@Model.Id" asp-route-isExisting="false" class="btn btn-sm btn-outline-primary rounded-pill"><i class="bi bi-pencil me-1"></i> Yönet</a>',
    html
)

# Custom Doc modal stage
target_modal = r'<div class="mb-3">\s*<label class="form-label fw-bold">Evrak / Kriter Ad.</label>'
insertion_modal = r'''<div class="mb-3">
                        <label class="form-label fw-bold text-danger">Aşama Kategorisi</label>
                        <select class="form-select border-danger" name="stage" required>
                            <option value="1. Yıkım Öncesi ve Yıkım Aşaması Evrakları">1. Yıkım Öncesi ve Yıkım Aşaması Evrakları</option>
                            <option value="2. Yapım (İnşaat) Aşaması Evrakları">2. Yapım (İnşaat) Aşaması Evrakları</option>
                            <option value="3. Satış ve Teslim Aşaması Evrakları">3. Satış ve Teslim Aşaması Evrakları</option>
                            <option value="Diğer Evraklar ve Kriterler">Diğer Evraklar ve Kriterler</option>
                        </select>
                    </div>
                    <div class="mb-3">
                        <label class="form-label fw-bold">Evrak / Kriter Adı</label>'''

if 'name="stage"' not in html:
    html = re.sub(target_modal, insertion_modal, html, count=1)

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)
