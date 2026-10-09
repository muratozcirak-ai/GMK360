import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Find the start of the Vitals block
start_tag = '<!-- PROJE VİTALLERİ (HAYATİ VERİLER) -->'
start_idx = content.find(start_tag)

# Find the start of the Konum card
end_tag = 'Konum Verileri ve Çevre Özellikleri'
end_idx = content.find(end_tag)

# Backtrack to the start of that card div
if end_idx != -1:
    end_idx = content.rfind('<div class="card mb-4 shadow-sm border-0 rounded-4">', 0, end_idx)
    if end_idx == -1:
        end_idx = content.rfind('<div class="card', 0, content.find(end_tag))

if start_idx != -1 and end_idx != -1:
    new_compact_ui = '''<!-- PROJE VİTALLERİ (HAYATİ VERİLER) -->
<div class="row g-3 mb-3 mt-1">
    <!-- İlerleme -->
    <div class="col-md-4">
        <div class="card border-0 shadow-sm rounded-3 h-100 p-2 d-flex flex-row align-items-center bg-white hover-lift">
            <div class="bg-success bg-opacity-10 text-success rounded-circle d-flex align-items-center justify-content-center me-2" style="width: 40px; height: 40px;"><i class="bi bi-pie-chart-fill fs-5"></i></div>
            <div class="lh-sm">
                <p class="text-muted mb-0" style="font-size: 0.7rem; font-weight: 700;">PROJE İLERLEME</p>
                <h5 class="fw-bolder mb-0 text-dark">%45 <span class="fw-normal text-muted" style="font-size: 0.75rem;">Tamamlandı</span></h5>
            </div>
        </div>
    </div>
    <!-- Bütçe -->
    <div class="col-md-4">
        <div class="card border-0 shadow-sm rounded-3 h-100 p-2 d-flex flex-row align-items-center bg-white hover-lift">
            <div class="bg-primary bg-opacity-10 text-primary rounded-circle d-flex align-items-center justify-content-center me-2" style="width: 40px; height: 40px;"><i class="bi bi-wallet-fill fs-5"></i></div>
            <div class="lh-sm">
                <p class="text-muted mb-0" style="font-size: 0.7rem; font-weight: 700;">BÜTÇE / HARCANAN</p>
                <h5 class="fw-bolder mb-0 text-dark">4.5M <span class="fw-normal text-muted" style="font-size: 0.75rem;">/ 10M ₺</span></h5>
            </div>
        </div>
    </div>
    <!-- Personel -->
    <div class="col-md-4">
        <div class="card border-0 shadow-sm rounded-3 h-100 p-2 d-flex flex-row align-items-center bg-white hover-lift">
            <div class="bg-warning bg-opacity-10 text-warning rounded-circle d-flex align-items-center justify-content-center me-2" style="width: 40px; height: 40px;"><i class="bi bi-people-fill fs-5"></i></div>
            <div class="lh-sm">
                <p class="text-muted mb-0" style="font-size: 0.7rem; font-weight: 700;">BUGÜN SAHADA</p>
                <h5 class="fw-bolder mb-0 text-dark">28 <span class="fw-normal text-muted" style="font-size: 0.75rem;">Personel</span></h5>
            </div>
        </div>
    </div>
</div>

<!-- AŞAMA HARİTASI (METRO HATTI) -->
<div class="card border-0 shadow-sm rounded-3 mb-3 bg-white overflow-hidden">
    <div class="card-header bg-transparent border-bottom-0 py-2 px-3 d-flex justify-content-between align-items-center">
        <h6 class="fw-bold text-dark mb-0" style="font-size: 0.9rem;"><i class="bi bi-map-fill text-danger me-2"></i> Aşama Haritası</h6>
        <div>
            <form action="/ConstructionProject/SyncAllPhases" method="post" class="d-inline-block m-0 p-0 me-2">
                <input type="hidden" name="projectId" value="@Model.Id" />
                <button type="submit" class="btn btn-sm btn-outline-primary py-0 px-2 rounded-pill fw-bold" style="font-size: 0.75rem;">
                    <i class="bi bi-cloud-download me-1"></i> Havuzdan Çek
                </button>
            </form>
            <!-- Faz 0 Butonu: Kırmızı arkaplan, beyaz metin -->
            <a href="/PhaseZero/Index/@Model.Id" class="btn btn-sm btn-danger text-white py-0 px-2 rounded-pill fw-bold border-0 shadow-sm" style="font-size: 0.75rem;"><i class="bi bi-file-earmark-lock-fill me-1"></i> Faz 0: Resmî Evrak</a>
        </div>
    </div>
    <div class="card-body px-3 pt-1 pb-3">
        
        <div class="d-flex justify-content-between position-relative text-center px-1">
            <!-- Arka Plan Çizgisi -->
            <div class="position-absolute top-50 start-0 w-100 border-top border-2 border-light" style="z-index: 1; transform: translateY(-50%);"></div>
            
            <!-- Duraklar (Daha küçük) -->
            <a href="/PhaseOne/Index/@Model.Id" class="position-relative z-2 text-decoration-none hover-lift d-block" style="width: 10%;">
                <div class="rounded-circle bg-success text-white mx-auto d-flex align-items-center justify-content-center shadow-sm mb-1" style="width: 28px; height: 28px;"><i class="bi bi-check-lg" style="font-size: 0.8rem;"></i></div>
                <span class="fw-bold text-success d-block lh-1" style="font-size: 0.65rem;">1. Hafriyat</span>
            </a>
            
            <a href="/PhaseTwo/Index/@Model.Id" class="position-relative z-2 text-decoration-none hover-lift d-block" style="width: 10%;">
                <div class="rounded-circle bg-success text-white mx-auto d-flex align-items-center justify-content-center shadow-sm mb-1" style="width: 28px; height: 28px;"><i class="bi bi-check-lg" style="font-size: 0.8rem;"></i></div>
                <span class="fw-bold text-success d-block lh-1" style="font-size: 0.65rem;">2. Temel</span>
            </a>
            
            <a href="/PhaseThree/Index/@Model.Id" class="position-relative z-2 text-decoration-none hover-lift d-block" style="width: 14%;">
                <!-- Aktif Durak: Daha büyük ama eskisi kadar kaba değil -->
                <div class="rounded-circle bg-primary text-white mx-auto d-flex align-items-center justify-content-center shadow mb-1 border border-2 border-white" style="width: 40px; height: 40px; margin-top:-5px; animation: pulse 2s infinite;"><i class="bi bi-hammer" style="font-size: 1rem;"></i></div>
                <span class="fw-bolder text-primary d-block lh-1 mt-1" style="font-size: 0.7rem;">3. Kaba İnşaat</span>
            </a>
            
            <a href="/PhaseFour/Index/@Model.Id" class="position-relative z-2 text-decoration-none hover-lift d-block" style="opacity: 0.7; width: 10%;">
                <div class="rounded-circle bg-light border border-secondary text-secondary mx-auto d-flex align-items-center justify-content-center mb-1" style="width: 28px; height: 28px;"><i class="bi bi-bricks" style="font-size: 0.75rem;"></i></div>
                <span class="fw-bold text-secondary d-block lh-1" style="font-size: 0.65rem;">4. Duvar/Çatı</span>
            </a>
            
            <a href="/PhaseFive/Index/@Model.Id" class="position-relative z-2 text-decoration-none hover-lift d-block" style="opacity: 0.7; width: 10%;">
                <div class="rounded-circle bg-light border border-secondary text-secondary mx-auto d-flex align-items-center justify-content-center mb-1" style="width: 28px; height: 28px;"><i class="bi bi-lightning" style="font-size: 0.75rem;"></i></div>
                <span class="fw-bold text-secondary d-block lh-1" style="font-size: 0.65rem;">5. Tesisat</span>
            </a>
            
            <a href="/PhaseSix/Index/@Model.Id" class="position-relative z-2 text-decoration-none hover-lift d-block" style="opacity: 0.7; width: 10%;">
                <div class="rounded-circle bg-light border border-secondary text-secondary mx-auto d-flex align-items-center justify-content-center mb-1" style="width: 28px; height: 28px;"><i class="bi bi-paint-bucket" style="font-size: 0.75rem;"></i></div>
                <span class="fw-bold text-secondary d-block lh-1" style="font-size: 0.65rem;">6. İnce İşler</span>
            </a>
            
            <a href="/PhaseSeven/Index/@Model.Id" class="position-relative z-2 text-decoration-none hover-lift d-block" style="opacity: 0.7; width: 10%;">
                <div class="rounded-circle bg-light border border-secondary text-secondary mx-auto d-flex align-items-center justify-content-center mb-1" style="width: 28px; height: 28px;"><i class="bi bi-tools" style="font-size: 0.75rem;"></i></div>
                <span class="fw-bold text-secondary d-block lh-1" style="font-size: 0.65rem;">7. Montaj</span>
            </a>
            
            <a href="/PhaseEight/Index/@Model.Id" class="position-relative z-2 text-decoration-none hover-lift d-block" style="opacity: 0.7; width: 10%;">
                <div class="rounded-circle bg-light border border-secondary text-secondary mx-auto d-flex align-items-center justify-content-center mb-1" style="width: 28px; height: 28px;"><i class="bi bi-tree" style="font-size: 0.75rem;"></i></div>
                <span class="fw-bold text-secondary d-block lh-1" style="font-size: 0.65rem;">8. Teslim</span>
            </a>
        </div>
        
    </div>
</div>
'''
    
    content = content[:start_idx] + new_compact_ui + content[end_idx:]

    with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
        f.write(content)
