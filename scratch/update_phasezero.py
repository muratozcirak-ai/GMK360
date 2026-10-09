import codecs
import re

path_ctrl = 'GMK360.Web/Controllers/PhaseZeroController.cs'
with codecs.open(path_ctrl, 'r', 'utf-8-sig') as f:
    content_ctrl = f.read()

content_ctrl = content_ctrl.replace(
    'originalLocation = doc.OriginalLocation,',
    'originalLocation = doc.OriginalLocation,\n                linkedPhaseCategory = doc.LinkedPhaseCategory,'
)

with codecs.open(path_ctrl, 'w', 'utf-8-sig') as f:
    f.write(content_ctrl)

path_view = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path_view, 'r', 'utf-8-sig') as f:
    content_view = f.read()

# Add to Javascript openManageModal
content_view = content_view.replace(
    "document.getElementById('modalAddCost').value = data.additionalCost == 0 ? '' : data.additionalCost;",
    "document.getElementById('modalAddCost').value = data.additionalCost == 0 ? '' : data.additionalCost;\n                    document.getElementById('modalLinkedPhase').value = data.linkedPhaseCategory !== null ? data.linkedPhaseCategory : '';"
)

# Add HTML Dropdown to the manageModal inside modal-body
dropdown_html = '''
                <div class="col-md-12">
                    <label class="form-label fw-bold text-dark"><i class="bi bi-link-45deg"></i> İlgili İmalat Fazı (Ön Koşul Bağlantısı)</label>
                    <select class="form-select border-primary" name="linkedPhaseCategory" id="modalLinkedPhase">
                        <option value="">-- Bağlantı Yok (Sadece Faz 0) --</option>
                        <option value="0">Faz 1: Yıkım ve Zemin Hazırlığı</option>
                        <option value="1">Faz 2: Temel ve Alt Yapı</option>
                        <option value="2">Faz 3: Kaba İnşaat (Karkas)</option>
                        <option value="3">Faz 4: Çatı ve Dış Cephe</option>
                        <option value="4">Faz 5: İnce İşler (İç Mekan)</option>
                        <option value="5">Faz 6: Elektrik ve Zayıf Akım</option>
                        <option value="6">Faz 7: Mekanik Tesisat ve Makine</option>
                        <option value="7">Faz 8: Peyzaj ve Teslim</option>
                    </select>
                    <small class="text-muted mt-1"><i class="bi bi-info-circle"></i> Seçtiğiniz imalat fazında bu evrak sarı renkli uyarı olarak gösterilecektir.</small>
                </div>
'''

content_view = re.sub(
    r'(<div class="row g-3">.*?)(<div class="col-md-6">)',
    r'\1' + dropdown_html + r'\n                        \2',
    content_view, count=1, flags=re.DOTALL
)

with codecs.open(path_view, 'w', 'utf-8-sig') as f:
    f.write(content_view)
    
print("Updated PhaseZero UI and Controller")