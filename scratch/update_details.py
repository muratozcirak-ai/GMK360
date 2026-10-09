import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Replace the 9-box phase grid with Vitals + Metro Line
start_tag = '<!-- PROJE FAZLARI VE ANA YÖNETİM (KİBAR LİSTE) -->'
end_tag = '<!-- KONUM VE ÇEVRE ÖZELLİKLERİ -->'

start_idx = content.find(start_tag)
end_idx = content.find(end_tag)

if start_idx != -1 and end_idx != -1:
    new_ui = '''<!-- PROJE VİTALLERİ (HAYATİ VERİLER) -->
<div class="row g-4 mb-4 mt-2">
    <!-- İlerleme -->
    <div class="col-md-4">
        <div class="card border-0 shadow-sm rounded-4 h-100 p-4 d-flex flex-row align-items-center bg-white hover-lift">
            <div class="bg-success bg-opacity-10 text-success rounded-circle d-flex align-items-center justify-content-center me-3" style="width: 60px; height: 60px;"><i class="bi bi-pie-chart-fill fs-2"></i></div>
            <div>
                <p class="text-muted mb-1 small fw-bold text-uppercase">Proje İlerleme</p>
                <h3 class="fw-bolder mb-0 text-dark">%45 <span class="fs-6 fw-normal text-muted">Tamamlandı</span></h3>
            </div>
        </div>
    </div>
    <!-- Bütçe -->
    <div class="col-md-4">
        <div class="card border-0 shadow-sm rounded-4 h-100 p-4 d-flex flex-row align-items-center bg-white hover-lift">
            <div class="bg-primary bg-opacity-10 text-primary rounded-circle d-flex align-items-center justify-content-center me-3" style="width: 60px; height: 60px;"><i class="bi bi-wallet-fill fs-2"></i></div>
            <div>
                <p class="text-muted mb-1 small fw-bold text-uppercase">Bütçe / Harcanan</p>
                <h3 class="fw-bolder mb-0 text-dark">4.5M <span class="fs-6 fw-normal text-muted">/ 10M ₺</span></h3>
            </div>
        </div>
    </div>
    <!-- Personel -->
    <div class="col-md-4">
        <div class="card border-0 shadow-sm rounded-4 h-100 p-4 d-flex flex-row align-items-center bg-white hover-lift">
            <div class="bg-warning bg-opacity-10 text-warning rounded-circle d-flex align-items-center justify-content-center me-3" style="width: 60px; height: 60px;"><i class="bi bi-people-fill fs-2"></i></div>
            <div>
                <p class="text-muted mb-1 small fw-bold text-uppercase">Bugün Sahada</p>
                <h3 class="fw-bolder mb-0 text-dark">28 <span class="fs-6 fw-normal text-muted">Personel</span></h3>
            </div>
        </div>
    </div>
</div>

<!-- AŞAMA HARİTASI (METRO HATTI) -->
<div class="card border-0 shadow-sm rounded-4 mb-4 bg-white overflow-hidden">
    <div class="card-header bg-transparent border-bottom-0 pt-4 pb-2 px-4 d-flex justify-content-between align-items-center">
        <h5 class="fw-bold text-dark mb-0"><i class="bi bi-map-fill text-danger me-2"></i> Aşama Haritası (Metro Hattı)</h5>
        <div>
            <form action="/ConstructionProject/SyncAllPhases" method="post" class="d-inline-block m-0 p-0 me-2">
                <input type="hidden" name="projectId" value="@Model.Id" />
                <button type="submit" class="btn btn-sm btn-outline-primary rounded-pill fw-bold">
                    <i class="bi bi-cloud-download me-1"></i> Havuzdan Çek
                </button>
            </form>
            <a href="/PhaseZero/Index/@Model.Id" class="btn btn-sm btn-danger bg-opacity-10 text-danger rounded-pill fw-bold border-0 shadow-none"><i class="bi bi-file-earmark-lock-fill me-1"></i> Faz 0: Resmî Evrak</a>
        </div>
    </div>
    <div class="card-body p-4 pb-5">
        
        <div class="d-flex justify-content-between position-relative text-center px-2">
            <!-- Arka Plan Çizgisi -->
            <div class="position-absolute top-50 start-0 w-100 border-top border-4 border-light" style="z-index: 1; transform: translateY(-50%);"></div>
            
            <!-- Duraklar -->
            <a href="/PhaseOne/Index/@Model.Id" class="position-relative z-2 text-decoration-none hover-lift d-block" style="width: 10%;">
                <div class="rounded-circle bg-success text-white mx-auto d-flex align-items-center justify-content-center shadow-sm mb-2" style="width: 45px; height: 45px;"><i class="bi bi-check-lg fs-5"></i></div>
                <small class="fw-bold text-success d-block">1. Hafriyat</small>
            </a>
            
            <a href="/PhaseTwo/Index/@Model.Id" class="position-relative z-2 text-decoration-none hover-lift d-block" style="width: 10%;">
                <div class="rounded-circle bg-success text-white mx-auto d-flex align-items-center justify-content-center shadow-sm mb-2" style="width: 45px; height: 45px;"><i class="bi bi-check-lg fs-5"></i></div>
                <small class="fw-bold text-success d-block">2. Temel</small>
            </a>
            
            <a href="/PhaseThree/Index/@Model.Id" class="position-relative z-2 text-decoration-none hover-lift d-block" style="width: 12%;">
                <!-- Aktif Durak: Daha büyük ve parlıyor -->
                <div class="rounded-circle bg-primary text-white mx-auto d-flex align-items-center justify-content-center shadow-lg mb-2 border border-3 border-white" style="width: 60px; height: 60px; margin-top:-8px; animation: pulse 2s infinite;"><i class="bi bi-hammer fs-4"></i></div>
                <small class="fw-bolder text-primary d-block mt-1">3. Kaba İnşaat</small>
            </a>
            
            <a href="/PhaseFour/Index/@Model.Id" class="position-relative z-2 text-decoration-none hover-lift d-block" style="opacity: 0.6; width: 10%;">
                <div class="rounded-circle bg-light border border-2 border-secondary text-secondary mx-auto d-flex align-items-center justify-content-center mb-2" style="width: 45px; height: 45px;"><i class="bi bi-bricks fs-5"></i></div>
                <small class="fw-bold text-secondary d-block">4. Duvar/Çatı</small>
            </a>
            
            <a href="/PhaseFive/Index/@Model.Id" class="position-relative z-2 text-decoration-none hover-lift d-block" style="opacity: 0.6; width: 10%;">
                <div class="rounded-circle bg-light border border-2 border-secondary text-secondary mx-auto d-flex align-items-center justify-content-center mb-2" style="width: 45px; height: 45px;"><i class="bi bi-lightning fs-5"></i></div>
                <small class="fw-bold text-secondary d-block">5. Tesisat</small>
            </a>
            
            <a href="/PhaseSix/Index/@Model.Id" class="position-relative z-2 text-decoration-none hover-lift d-block" style="opacity: 0.6; width: 10%;">
                <div class="rounded-circle bg-light border border-2 border-secondary text-secondary mx-auto d-flex align-items-center justify-content-center mb-2" style="width: 45px; height: 45px;"><i class="bi bi-paint-bucket fs-5"></i></div>
                <small class="fw-bold text-secondary d-block">6. İnce İşler</small>
            </a>
            
            <a href="/PhaseSeven/Index/@Model.Id" class="position-relative z-2 text-decoration-none hover-lift d-block" style="opacity: 0.6; width: 10%;">
                <div class="rounded-circle bg-light border border-2 border-secondary text-secondary mx-auto d-flex align-items-center justify-content-center mb-2" style="width: 45px; height: 45px;"><i class="bi bi-tools fs-5"></i></div>
                <small class="fw-bold text-secondary d-block">7. Montaj</small>
            </a>
            
            <a href="/PhaseEight/Index/@Model.Id" class="position-relative z-2 text-decoration-none hover-lift d-block" style="opacity: 0.6; width: 10%;">
                <div class="rounded-circle bg-light border border-2 border-secondary text-secondary mx-auto d-flex align-items-center justify-content-center mb-2" style="width: 45px; height: 45px;"><i class="bi bi-tree fs-5"></i></div>
                <small class="fw-bold text-secondary d-block">8. Teslim</small>
            </a>
        </div>
        
    </div>
</div>

'''
    content = content[:start_idx] + new_ui + content[end_idx:]
    
    with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
        f.write(content)

