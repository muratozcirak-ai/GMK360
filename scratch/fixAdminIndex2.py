with open('GMK360.Web/Views/AdminLegalDocument/Index.cshtml', 'r', encoding='utf-8') as f:
    text = f.read()

text = text.replace('<th>Hedef Modül</th>', '<th>Aşama (Faz)</th>\n                              <th>Hedef Modül</th>')

text = text.replace('<td>\n                                      <span class="badge bg-secondary">', 
'<td><span class="badge bg-info text-dark">@(item.Stage ?? "-")</span></td>\n                                  <td>\n                                      <span class="badge bg-secondary">')

text = text.replace('<div class="mb-3">\n                        <label class="form-label fw-bold">Hangi Modül İçin Geçerli?</label>',
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
