import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

new_modules = '''<!-- DETAYLI MODÜLLER (B2B & SAAS) SECTION -->
<div class="container py-5 mt-5">
    <div class="text-center mb-5">
        <h6 class="text-orange fw-bold text-uppercase tracking-wide">Yönetim Panellerimiz</h6>
        <h2 class="fw-bold text-navy display-6">Profesyoneller İçin Ayrıcalıklı Modüller</h2>
        <p class="text-muted lead mx-auto mt-3" style="max-width: 800px;">Her sektörün ihtiyacı farklıdır. Sizi gereksiz menülerle yormuyoruz; sadece size özel tasarlanmış, hayatınızı kolaylaştıran çalışma alanınıza giriş yapın.</p>
    </div>

    <!-- 8 KARTLIK SİMETRİK GRID (4x2) -->
    <div class="row g-4">
        
        <!-- 1. İnşaat, Hafriyat & Yıkım -->
        <div class="col-lg-3 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 h-100 hover-lift bg-white">
                <div class="card-body p-4">
                    <div class="bg-primary bg-opacity-10 rounded-circle d-flex align-items-center justify-content-center mb-4" style="width: 50px; height: 50px;">
                        <i class="bi bi-cone-striped fs-4 text-primary"></i>
                    </div>
                    <h5 class="fw-bold text-navy mb-3">İnşaat, Hafriyat & Yıkım</h5>
                    <p class="text-muted small mb-4">Şantiye fazları, taşeron hakedişleri, kaba/ince iş ve resmi birim fiyatları ile maliyet analizi yapın.</p>
                    <a href="#" class="text-decoration-none fw-bold small text-primary mt-auto d-inline-block">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>

        <!-- 2. Bina, Site & AVM Yönetimi -->
        <div class="col-lg-3 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 h-100 hover-lift bg-white">
                <div class="card-body p-4">
                    <div class="bg-info bg-opacity-10 rounded-circle d-flex align-items-center justify-content-center mb-4" style="width: 50px; height: 50px;">
                        <i class="bi bi-building fs-4 text-info"></i>
                    </div>
                    <h5 class="fw-bold text-navy mb-3">Bina, Site & AVM</h5>
                    <p class="text-muted small mb-4">İş hanı, plaza ve sitelerinizin aidat, havuz/peyzaj giderleri ve personel vardiyalarını tek ekrandan yönetin.</p>
                    <a href="#" class="text-decoration-none fw-bold small text-info mt-auto d-inline-block">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>

        <!-- 3. Mülk & Kiracı Takibi -->
        <div class="col-lg-3 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 h-100 hover-lift bg-white">
                <div class="card-body p-4">
                    <div class="bg-success bg-opacity-10 rounded-circle d-flex align-items-center justify-content-center mb-4" style="width: 50px; height: 50px;">
                        <i class="bi bi-house-check fs-4 text-success"></i>
                    </div>
                    <h5 class="fw-bold text-navy mb-3">Mülk & Kiracı Takibi</h5>
                    <p class="text-muted small mb-4">Ev sahipleri için tapu, DASK, kira tahsilatı, otomatik TÜFE artışı ve dijital kontrat arşivi.</p>
                    <a href="#" class="text-decoration-none fw-bold small text-success mt-auto d-inline-block">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>

        <!-- 4. Emlak Ofisleri & Acenteler -->
        <div class="col-lg-3 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 h-100 hover-lift bg-white">
                <div class="card-body p-4">
                    <div class="bg-orange bg-opacity-10 rounded-circle d-flex align-items-center justify-content-center mb-4" style="width: 50px; height: 50px;">
                        <i class="bi bi-signpost-split fs-4 text-orange"></i>
                    </div>
                    <h5 class="fw-bold text-navy mb-3">Emlak Ofisleri</h5>
                    <p class="text-muted small mb-4">Danışmanların portföy, akıllı ilan yayınlama, müşteri eşleştirme ve satış/kiralama süreçleri.</p>
                    <a href="#" class="text-decoration-none fw-bold small text-orange mt-auto d-inline-block">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>

        <!-- 5. Günlük & Kısa Dönem -->
        <div class="col-lg-3 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 h-100 hover-lift bg-white">
                <div class="card-body p-4">
                    <div class="bg-secondary bg-opacity-10 rounded-circle d-flex align-items-center justify-content-center mb-4" style="width: 50px; height: 50px;">
                        <i class="bi bi-calendar-range fs-4 text-secondary"></i>
                    </div>
                    <h5 class="fw-bold text-navy mb-3">Kısa Dönem Kiralık</h5>
                    <p class="text-muted small mb-4">Check-in / Check-out takvimi, temizlik yönlendirme ve dinamik fiyatlama ile gelirinizi katlayın.</p>
                    <a href="#" class="text-decoration-none fw-bold small text-secondary mt-auto d-inline-block">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>

        <!-- 6. Tedarikçi Pazar Yeri -->
        <div class="col-lg-3 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 h-100 hover-lift bg-white">
                <div class="card-body p-4">
                    <div class="bg-warning bg-opacity-10 rounded-circle d-flex align-items-center justify-content-center mb-4" style="width: 50px; height: 50px;">
                        <i class="bi bi-shop-window fs-4 text-warning"></i>
                    </div>
                    <h5 class="fw-bold text-navy mb-3">Tedarikçi Pazar Yeri</h5>
                    <p class="text-muted small mb-4">İnşaat malzemesi, mobilya ve hırdavat satıcıları için B2B / B2C ürün sergileme ve sipariş yönetimi.</p>
                    <a href="#" class="text-decoration-none fw-bold small text-warning mt-auto d-inline-block">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>

        <!-- 7. Usta & Tadilat Yönetimi -->
        <div class="col-lg-3 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 h-100 hover-lift bg-white">
                <div class="card-body p-4">
                    <div class="bg-danger bg-opacity-10 rounded-circle d-flex align-items-center justify-content-center mb-4" style="width: 50px; height: 50px;">
                        <i class="bi bi-hammer fs-4 text-danger"></i>
                    </div>
                    <h5 class="fw-bold text-navy mb-3">Usta & Tadilat </h5>
                    <p class="text-muted small mb-4">Hizmet verenler, mimarlar ve tamirat ekipleri için keşif, randevu ve iş teslimi takibi.</p>
                    <a href="#" class="text-decoration-none fw-bold small text-danger mt-auto d-inline-block">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>

        <!-- 8. Yapay Zeka & Evrak -->
        <div class="col-lg-3 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 h-100 hover-lift bg-white">
                <div class="card-body p-4">
                    <div class="bg-dark bg-opacity-10 rounded-circle d-flex align-items-center justify-content-center mb-4" style="width: 50px; height: 50px;">
                        <i class="bi bi-robot fs-4 text-dark"></i>
                    </div>
                    <h5 class="fw-bold text-navy mb-3">Yapay Zeka & Evrak</h5>
                    <p class="text-muted small mb-4">Sözleşme analizi, beyanname hatırlatıcı ve akıllı asistan ile yasal/operasyonel evraklarınız güvende.</p>
                    <a href="#" class="text-decoration-none fw-bold small text-dark mt-auto d-inline-block">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>

    </div>
</div>
'''

content = re.sub(r'<!-- DETAYLI MODÜLLER.*?</div>\s*</div>\s*</div>\s*</div>\s*</div>', new_modules, content, flags=re.DOTALL)

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
