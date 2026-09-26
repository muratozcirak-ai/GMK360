import sys

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

target = '''<div class="mb-3">
                        <label class="form-label fw-bold">Evrak / Kriter Adı</label>'''

insertion = '''<div class="mb-3">
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

html = html.replace(target, insertion)

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)

print("SUCCESS")
