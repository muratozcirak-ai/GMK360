import re

with open('GMK360.Web/Views/AdminLegalDocument/Index.cshtml', 'r', encoding='utf-8') as f:
    text = f.read()

# Add header
text = text.replace('<th>Hedef Modül</th>', '<th>Aşama (Faz)</th>\n                                  <th>Hedef Modül</th>')

# Add row column
row_pattern = r'<td>\s*<span class="badge bg-secondary">'
row_replacement = '<td><span class="badge bg-info text-dark">@(item.Stage ?? "-")</span></td>\n                                  ' + row_pattern

text = re.sub(row_pattern, row_replacement, text)

# Add to form
form_pattern = r'<label class="form-label fw-bold">Hedef Modül</label>'
form_replacement = r'''<div class="mb-3">
                        <label class="form-label fw-bold">Aşama (Stage)</label>
                        <select name="Stage" class="form-select">
                            <option value="">(Yok - Diğer)</option>
                            <option value="1. Yıkım Öncesi ve Yıkım Aşaması Evrakları">1. Yıkım Öncesi ve Yıkım Aşaması Evrakları</option>
                            <option value="2. Yapım (İnşaat) Aşaması Evrakları">2. Yapım (İnşaat) Aşaması Evrakları</option>
                            <option value="3. Satış ve Teslim Aşaması Evrakları">3. Satış ve Teslim Aşaması Evrakları</option>
                        </select>
                    </div>
                    <div class="mb-3">
                        <label class="form-label fw-bold">Hedef Modül</label>'''
                        
text = text.replace('<label class="form-label fw-bold">Hangi Modül İçin Geçerli?</label>', '<label class="form-label fw-bold">Hangi Modül İçin Geçerli?</label>').replace(
    '<div class="mb-3">\n                        <label class="form-label fw-bold">Hangi Modül İçin Geçerli?</label>',
    '''<div class="mb-3">
                        <label class="form-label fw-bold">Aşama (Faz)</label>
                        <select name="Stage" class="form-select">
                            <option value="">(Yok - Genel)</option>
                            <option value="1. Yıkım Öncesi ve Yıkım Aşaması Evrakları">1. Yıkım Öncesi ve Yıkım Aşaması Evrakları</option>
                            <option value="2. Yapım (İnşaat) Aşaması Evrakları">2. Yapım (İnşaat) Aşaması Evrakları</option>
                            <option value="3. Satış ve Teslim Aşaması Evrakları">3. Satış ve Teslim Aşaması Evrakları</option>
                        </select>
                    </div>
                    <div class="mb-3">
                        <label class="form-label fw-bold">Hangi Modül İçin Geçerli?</label>'''
)

with open('GMK360.Web/Views/AdminLegalDocument/Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(text)
