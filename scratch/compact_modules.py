import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

new_modules = '''<!-- DETAYLI MODÜLLER (B2B & SAAS) SECTION -->
<div class="container-fluid px-4 px-lg-5 py-4 mt-2">
    <div class="text-center mb-4">
        <h6 class="text-orange fw-bold text-uppercase tracking-wide mb-1" style="font-size: 0.85rem;">Yönetim Panellerimiz</h6>
        <h2 class="fw-bold text-navy mb-2">Profesyoneller İçin Ayrıcalıklı Modüller</h2>
    </div>

    <!-- 8 KARTLIK SİMETRİK GRID (4x2) - COMPACT -->
    <div class="row g-3">
        
        <!-- 1. İnşaat, Hafriyat & Yıkım -->
        <div class="col-lg-3 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 h-100 hover-lift bg-white">
                <div class="card-body p-3 d-flex flex-column">
                    <div class="d-flex align-items-center mb-2">
                        <div class="bg-primary bg-opacity-10 rounded-3 d-flex align-items-center justify-content-center me-3 flex-shrink-0" style="width: 40px; height: 40px;">
                            <i class="bi bi-cone-striped fs-5 text-primary"></i>
                        </div>
                        <h6 class="fw-bold text-navy mb-0 lh-sm">İnşaat, Hafriyat & Yıkım</h6>
                    </div>
                    <p class="text-muted mb-3" style="font-size: 0.8rem; line-height: 1.4;">Şantiye fazları, taşeron hakedişleri, kaba/ince iş ve resmi birim fiyatları ile maliyet analizi yapın.</p>
                    <a href="#" class="text-decoration-none fw-bold text-primary mt-auto d-inline-block" style="font-size: 0.8rem;">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>

        <!-- 2. Bina, Site & AVM Yönetimi -->
        <div class="col-lg-3 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 h-100 hover-lift bg-white">
                <div class="card-body p-3 d-flex flex-column">
                    <div class="d-flex align-items-center mb-2">
                        <div class="bg-info bg-opacity-10 rounded-3 d-flex align-items-center justify-content-center me-3 flex-shrink-0" style="width: 40px; height: 40px;">
                            <i class="bi bi-building fs-5 text-info"></i>
                        </div>
                        <h6 class="fw-bold text-navy mb-0 lh-sm">Bina, Site & AVM</h6>
                    </div>
                    <p class="text-muted mb-3" style="font-size: 0.8rem; line-height: 1.4;">İş hanı, plaza ve sitelerinizin aidat, havuz/peyzaj giderleri ve personel vardiyalarını tek ekrandan yönetin.</p>
                    <a href="#" class="text-decoration-none fw-bold text-info mt-auto d-inline-block" style="font-size: 0.8rem;">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>

        <!-- 3. Mülk & Kiracı Takibi -->
        <div class="col-lg-3 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 h-100 hover-lift bg-white">
                <div class="card-body p-3 d-flex flex-column">
                    <div class="d-flex align-items-center mb-2">
                        <div class="bg-success bg-opacity-10 rounded-3 d-flex align-items-center justify-content-center me-3 flex-shrink-0" style="width: 40px; height: 40px;">
                            <i class="bi bi-house-check fs-5 text-success"></i>
                        </div>
                        <h6 class="fw-bold text-navy mb-0 lh-sm">Mülk & Kiracı Takibi</h6>
                    </div>
                    <p class="text-muted mb-3" style="font-size: 0.8rem; line-height: 1.4;">Ev sahipleri için tapu, DASK, kira tahsilatı, otomatik TÜFE artışı ve dijital kontrat arşivi.</p>
                    <a href="#" class="text-decoration-none fw-bold text-success mt-auto d-inline-block" style="font-size: 0.8rem;">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>

        <!-- 4. Emlak Ofisleri & Danışmanlar -->
        <div class="col-lg-3 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 h-100 hover-lift bg-white">
                <div class="card-body p-3 d-flex flex-column">
                    <div class="d-flex align-items-center mb-2">
                        <div class="bg-orange bg-opacity-10 rounded-3 d-flex align-items-center justify-content-center me-3 flex-shrink-0" style="width: 40px; height: 40px;">
                            <i class="bi bi-signpost-split fs-5 text-orange"></i>
                        </div>
                        <h6 class="fw-bold text-navy mb-0 lh-sm">Acenteler & Danışmanlar</h6>
                    </div>
                    <p class="text-muted mb-3" style="font-size: 0.8rem; line-height: 1.4;">Danışmanların portföy, akıllı ilan yayınlama, müşteri eşleştirme ve satış/kiralama süreçleri.</p>
                    <a href="#" class="text-decoration-none fw-bold text-orange mt-auto d-inline-block" style="font-size: 0.8rem;">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>

        <!-- 5. Günlük & Kısa Dönem -->
        <div class="col-lg-3 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 h-100 hover-lift bg-white">
                <div class="card-body p-3 d-flex flex-column">
                    <div class="d-flex align-items-center mb-2">
                        <div class="bg-secondary bg-opacity-10 rounded-3 d-flex align-items-center justify-content-center me-3 flex-shrink-0" style="width: 40px; height: 40px;">
                            <i class="bi bi-calendar-range fs-5 text-secondary"></i>
                        </div>
                        <h6 class="fw-bold text-navy mb-0 lh-sm">Kısa Dönem Kiralık</h6>
                    </div>
                    <p class="text-muted mb-3" style="font-size: 0.8rem; line-height: 1.4;">Check-in / Check-out takvimi, temizlik yönlendirme ve dinamik fiyatlama ile gelirinizi katlayın.</p>
                    <a href="#" class="text-decoration-none fw-bold text-secondary mt-auto d-inline-block" style="font-size: 0.8rem;">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>

        <!-- 6. Tedarikçi Pazar Yeri -->
        <div class="col-lg-3 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 h-100 hover-lift bg-white">
                <div class="card-body p-3 d-flex flex-column">
                    <div class="d-flex align-items-center mb-2">
                        <div class="bg-warning bg-opacity-10 rounded-3 d-flex align-items-center justify-content-center me-3 flex-shrink-0" style="width: 40px; height: 40px;">
                            <i class="bi bi-shop-window fs-5 text-warning"></i>
                        </div>
                        <h6 class="fw-bold text-navy mb-0 lh-sm">Tedarikçi Pazar Yeri</h6>
                    </div>
                    <p class="text-muted mb-3" style="font-size: 0.8rem; line-height: 1.4;">İnşaat malzemesi, mobilya ve hırdavat satıcıları için B2B / B2C ürün sergileme ve sipariş yönetimi.</p>
                    <a href="#" class="text-decoration-none fw-bold text-warning mt-auto d-inline-block" style="font-size: 0.8rem;">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>

        <!-- 7. Usta & Tadilat Yönetimi -->
        <div class="col-lg-3 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 h-100 hover-lift bg-white">
                <div class="card-body p-3 d-flex flex-column">
                    <div class="d-flex align-items-center mb-2">
                        <div class="bg-danger bg-opacity-10 rounded-3 d-flex align-items-center justify-content-center me-3 flex-shrink-0" style="width: 40px; height: 40px;">
                            <i class="bi bi-hammer fs-5 text-danger"></i>
                        </div>
                        <h6 class="fw-bold text-navy mb-0 lh-sm">Usta & Tadilat</h6>
                    </div>
                    <p class="text-muted mb-3" style="font-size: 0.8rem; line-height: 1.4;">Hizmet verenler, mimarlar ve tamirat ekipleri için keşif, randevu ve iş teslimi takibi.</p>
                    <a href="#" class="text-decoration-none fw-bold text-danger mt-auto d-inline-block" style="font-size: 0.8rem;">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>

        <!-- 8. Yapay Zeka & Evrak -->
        <div class="col-lg-3 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 h-100 hover-lift bg-white">
                <div class="card-body p-3 d-flex flex-column">
                    <div class="d-flex align-items-center mb-2">
                        <div class="bg-dark bg-opacity-10 rounded-3 d-flex align-items-center justify-content-center me-3 flex-shrink-0" style="width: 40px; height: 40px;">
                            <i class="bi bi-robot fs-5 text-dark"></i>
                        </div>
                        <h6 class="fw-bold text-navy mb-0 lh-sm">Yapay Zeka & Evrak</h6>
                    </div>
                    <p class="text-muted mb-3" style="font-size: 0.8rem; line-height: 1.4;">Sözleşme analizi, yapay zeka ile mülk değerlendirme, beyanname hatırlatıcı ve akıllı asistan.</p>
                    <a href="#" class="text-decoration-none fw-bold text-dark mt-auto d-inline-block" style="font-size: 0.8rem;">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>

    </div>
</div>
'''

content = re.sub(r'<!-- DETAYLI MODÜLLER.*?</style>', new_modules, content, flags=re.DOTALL)

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
