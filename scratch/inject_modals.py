import io
import re

filepath = r'GMK360.Web\Views\DailyTimesheet\DailyCheckin.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Ekleme yapacağımız yer: Kaydet butonunun sağına iki yeni buton.
# Arama: <button class="btn btn-primary fw-bold ms-2 shadow-sm rounded-pill px-4" onclick="savePuantaj()">
replacement_buttons = """
            <button class="btn btn-danger fw-bold ms-2 shadow-sm rounded-pill px-3" data-bs-toggle="modal" data-bs-target="#cashRequestModal" title="Şantiye Acil Nakit/Kasa Talebi">
                <i class="bi bi-cash-stack"></i> Nakit
            </button>
            <button class="btn btn-warning text-dark fw-bold ms-2 shadow-sm rounded-pill px-3" data-bs-toggle="modal" data-bs-target="#dailyLogModal" title="Şantiye Günlük Jurnali">
                <i class="bi bi-journal-text"></i> Jurnal
            </button>
            <input type="date" class="form-control d-inline-block w-auto ms-2" id="dateSelector" value="@ViewBag.TargetDate" onchange="changeDate(this.value)">
            <button class="btn btn-primary fw-bold ms-2 shadow-sm rounded-pill px-4" onclick="savePuantaj()">
                <i class="bi bi-save me-1"></i> Kaydet
            </button>
"""

content = re.sub(
    r'<input type="date".*?onclick="savePuantaj.*?Kaydet\s*</button>', 
    replacement_buttons, 
    content, 
    flags=re.DOTALL
)

# Modal HTML'lerini dosyanın sonuna (Scripts'den önce) ekleyelim.
modals = """
<!-- KASA TALEBİ MODAL -->
<div class="modal fade" id="cashRequestModal" tabindex="-1">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content border-danger border-2 rounded-4 shadow-lg">
            <div class="modal-header bg-danger bg-opacity-10 border-0">
                <h5 class="modal-title fw-bold text-danger"><i class="bi bi-cash-coin me-2"></i> Şantiye Acil Kasa/Avans Talebi</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form action="/SiteOperations/RequestCash" method="post">
                <div class="modal-body">
                    <input type="hidden" name="projectId" value="@ViewBag.ProjectId" />
                    <div class="alert alert-warning small">
                        <i class="bi bi-info-circle-fill me-1"></i> Bu talep anında merkez onaya düşecektir. Unutulma riski yoktur.
                    </div>
                    <div class="mb-3">
                        <label class="form-label fw-bold">Talep Edilen Tutar (TL)</label>
                        <input type="number" name="amount" class="form-control form-control-lg text-danger fw-bold" required placeholder="Örn: 15000" min="1" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label fw-bold">Gerekçe / Nerelere Harcanacak?</label>
                        <textarea name="reason" class="form-control" required rows="3" placeholder="Örn: Acil çivi, taşeron yemeği, hırdavat..."></textarea>
                    </div>
                </div>
                <div class="modal-footer border-0">
                    <button type="button" class="btn btn-secondary rounded-pill" data-bs-dismiss="modal">İptal</button>
                    <button type="submit" class="btn btn-danger fw-bold rounded-pill px-4">Talebi Gönder</button>
                </div>
            </form>
        </div>
    </div>
</div>

<!-- ŞANTİYE JURNALİ MODAL -->
<div class="modal fade" id="dailyLogModal" tabindex="-1">
    <div class="modal-dialog modal-lg modal-dialog-centered">
        <div class="modal-content border-warning border-2 rounded-4 shadow-lg">
            <div class="modal-header bg-warning bg-opacity-10 border-0">
                <h5 class="modal-title fw-bold text-dark"><i class="bi bi-journal-check text-warning me-2"></i> Şantiye Seyir Defteri (Jurnal)</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form action="/SiteOperations/SaveLog" method="post">
                <div class="modal-body bg-light">
                    <input type="hidden" name="projectId" value="@ViewBag.ProjectId" />
                    <p class="text-muted small mb-4">Şantiye şefi olarak günün özetini, yaşanan gecikmeleri ve ustaların hatalarını buraya not edin. Bu rapor doğrudan patrona ulaşır ve sorumluluğunuzu korur.</p>
                    
                    <div class="row g-3">
                        <div class="col-md-12">
                            <label class="form-label fw-bold text-primary">1. Bugün Ne Yapıldı? (Genel İlerleme)</label>
                            <textarea name="progress" class="form-control border-primary" rows="2" placeholder="Örn: 3. Kat kalıpları %80 çakıldı..."></textarea>
                        </div>
                        <div class="col-md-12">
                            <label class="form-label fw-bold text-danger">2. Engeller, Sorunlar ve Gecikmeler</label>
                            <textarea name="obstacles" class="form-control border-danger bg-white" rows="3" placeholder="Örn: Ahmet Usta malzemeyi geç söylediği için iş 3 saat durdu."></textarea>
                            <small class="text-danger">Usta manipülasyonlarına karşı kendinizi koruyun.</small>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label fw-bold text-info">3. Acil Malzeme İhtiyacı</label>
                            <textarea name="needs" class="form-control border-info" rows="2" placeholder="Örn: Yarın sabah 10 torba çimento şart."></textarea>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label fw-bold">4. Hava Durumu</label>
                            <input type="text" name="weather" class="form-control" placeholder="Örn: Yağmurlu, iş yavaşladı." />
                        </div>
                    </div>
                </div>
                <div class="modal-footer border-0">
                    <button type="submit" class="btn btn-warning text-dark fw-bold rounded-pill px-5 w-100">Jurnali Kaydet ve Merkeze İlet</button>
                </div>
            </form>
        </div>
    </div>
</div>
"""

content = content.replace("<!-- Hızlı Eleman Ekle Modal -->", modals + "\n<!-- Hızlı Eleman Ekle Modal -->")

# En üste TempData Success Message alert'ini de ekleyelim ki jurnal ve avans kaydında mesaj çıksın.
alert_box = """
    @if (TempData["SuccessMessage"] != null)
    {
        <div class="alert alert-success alert-dismissible fade show rounded-4 shadow-sm mb-4" role="alert">
            <i class="bi bi-check-circle-fill me-2"></i> @TempData["SuccessMessage"]
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    }
"""
content = content.replace('<div class="row mb-4 align-items-center">', alert_box + '\n    <div class="row mb-4 align-items-center">')

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Injected Modals into DailyCheckin")
