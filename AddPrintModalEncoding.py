import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\SubcontractorContract\Details.cshtml'
try:
    with codecs.open(filepath, 'r', 'utf-8-sig') as f:
        content = f.read()
except UnicodeDecodeError:
    with codecs.open(filepath, 'r', 'cp1254') as f:
        content = f.read()

btn_html = '''        <div class="d-flex gap-2">
            <button class="btn btn-warning shadow-sm" data-bs-toggle="modal" data-bs-target="#printModal">
                <i class="bi bi-printer me-2"></i> Şablondan Yazdır
            </button>
            <a asp-action="ProjectContracts" asp-route-id="@Model.ProjectId" class="btn btn-light border shadow-sm">
                <i class="bi bi-arrow-left me-2"></i> Sözleşmelere Dön
            </a>
        </div>'''

content = re.sub(
    r'<a asp-action="ProjectContracts" asp-route-id="@Model\.ProjectId" class="btn btn-light border">.*?</a>',
    btn_html,
    content,
    flags=re.DOTALL
)

modal_html = '''
<!-- YAZDIRMA MODALI -->
<div class="modal fade" id="printModal" tabindex="-1">
    <div class="modal-dialog">
        <form action="/SubcontractorContract/PrintContract" method="get" target="_blank">
            <input type="hidden" name="contractId" value="@Model.Id" />
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-warning border-0">
                    <h5 class="modal-title fw-bold text-dark"><i class="bi bi-printer me-2"></i> Sözleşme Yazdır</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body p-4">
                    <p class="text-muted mb-4">Aşağıdan yazdırmak istediğiniz sözleşme şablonunu seçin. Sistem <strong>Firma Adı</strong>, <strong>Tutar</strong> ve <strong>Proje Adı</strong> gibi bilgileri otomatik dolduracaktır.</p>
                    
                    <div class="mb-3">
                        <label class="form-label fw-bold">Şablon Seçiniz</label>
                        <select name="templateId" class="form-select" required>
                            <option value="">-- Şablon Seçin --</option>
                            @if(ViewBag.Templates != null)
                            {
                                foreach(var t in ViewBag.Templates)
                                {
                                    <option value="@t.Id">@t.TemplateName (@t.Category)</option>
                                }
                            }
                        </select>
                    </div>
                </div>
                <div class="modal-footer border-0 bg-light">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">İptal</button>
                    <button type="submit" class="btn btn-warning fw-bold"><i class="bi bi-file-earmark-pdf me-1"></i> Önizle & Yazdır</button>
                </div>
            </div>
        </form>
    </div>
</div>
'''

if 'id="printModal"' not in content:
    content = content + "\n" + modal_html

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
