import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

modal_html = '''
<!-- TEVHİT (BİNA BİRLEŞTİRME) MODALI -->
<div class="modal fade" id="mergeBlocksModal" tabindex="-1" aria-labelledby="mergeBlocksModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content border-0 shadow-lg rounded-4">
            <div class="modal-header bg-dark text-white border-bottom-0 rounded-top-4">
                <h5 class="modal-title fw-bold" id="mergeBlocksModalLabel"><i class="bi bi-intersect me-2"></i> Tevhit (Blok Birleştirme)</h5>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body p-4">
                <div class="alert alert-info border-0 rounded-3 small">
                    <i class="bi bi-info-circle-fill me-2"></i> 
                    Seçtiğiniz bloklar tek bir <b>Hedef Bina</b> altında birleştirilecektir. Eski binalar yıkım kayıtlarında bağımsız tutulsa da, Faz (İnşaat Aşamaları) tek bir takvim üzerinden yürüyecektir.
                </div>
                
                <div class="mb-3">
                    <label class="form-label fw-bold">Birleştirilecek Bloklar (Tevhit Edilecekler)</label>
                    <div class="form-check mb-2">
                        <input class="form-check-input" type="checkbox" value="" id="blockA" checked>
                        <label class="form-check-label" for="blockA">
                            Beyaz Konak Apartmanı - A Blok
                        </label>
                    </div>
                    <div class="form-check mb-2">
                        <input class="form-check-input" type="checkbox" value="" id="blockB" checked>
                        <label class="form-check-label" for="blockB">
                            Beyaz Konak Apartmanı - B Blok
                        </label>
                    </div>
                </div>

                <div class="mb-3">
                    <label class="form-label fw-bold">Yeni Hedef Bina Adı</label>
                    <input type="text" class="form-control" value="Beyaz Konak (Tevhit Edilmiş Yeni Proje)" />
                </div>
            </div>
            <div class="modal-footer bg-light border-top-0 rounded-bottom-4">
                <button type="button" class="btn btn-secondary rounded-pill px-4" data-bs-dismiss="modal">İptal</button>
                <button type="button" class="btn btn-dark rounded-pill px-4 fw-bold"><i class="bi bi-intersect me-2"></i> Binaları Birleştir</button>
            </div>
        </div>
    </div>
</div>
'''

idx_scripts = content.find('@section Scripts')
if idx_scripts != -1:
    content = content[:idx_scripts] + modal_html + '\n' + content[idx_scripts:]
else:
    content += '\n' + modal_html

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("Modal added!")
