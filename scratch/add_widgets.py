import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

target = '<!-- ÖZET ALANLARI (COLLAPSIBLE) -->'

new_blocks = '''
<!-- PATRON KONTROL PANELİ (GÜNLÜK & FİNANS) -->
<div class="row g-4 mb-4">
    <!-- ŞANTİYE OLAY GÜNLÜĞÜ -->
    <div class="col-lg-6">
        <div class="card border-0 shadow-sm rounded-4 h-100 bg-white">
            <div class="card-header bg-transparent border-bottom-0 pt-4 pb-2 px-4 d-flex justify-content-between align-items-center">
                <h6 class="fw-bold text-dark mb-0"><i class="bi bi-activity text-primary me-2"></i> Şantiye Günlüğü (Son 24s)</h6>
                <a href="#" class="text-decoration-none small text-muted hover-primary"><i class="bi bi-arrow-right"></i> Tümü</a>
            </div>
            <div class="card-body px-4 pt-2 pb-4">
                <div class="position-relative ms-2">
                    <div class="position-absolute h-100 border-start border-2 border-light" style="left: 6px; top: 10px; z-index: 1;"></div>
                    
                    <div class="d-flex position-relative mb-3 z-2">
                        <div class="bg-success rounded-circle mt-1 shadow-sm d-flex justify-content-center align-items-center" style="width: 14px; height: 14px; margin-left: 0;"></div>
                        <div class="ms-3">
                            <div class="fw-bold text-dark" style="font-size: 0.85rem;">Ahmet Usta (Taşeron) sahaya giriş yaptı.</div>
                            <div class="text-muted" style="font-size: 0.75rem;"><i class="bi bi-clock me-1"></i> Bugün 08:15</div>
                        </div>
                    </div>
                    
                    <div class="d-flex position-relative mb-3 z-2">
                        <div class="bg-primary rounded-circle mt-1 shadow-sm d-flex justify-content-center align-items-center" style="width: 14px; height: 14px; margin-left: 0;"></div>
                        <div class="ms-3">
                            <div class="fw-bold text-dark" style="font-size: 0.85rem;">C25 Beton (100 Mikser) dökümü tamamlandı.</div>
                            <div class="text-muted" style="font-size: 0.75rem;"><i class="bi bi-clock me-1"></i> Dün 16:30 &bull; A Blok Temel</div>
                        </div>
                    </div>

                    <div class="d-flex position-relative z-2">
                        <div class="bg-warning rounded-circle mt-1 shadow-sm d-flex justify-content-center align-items-center" style="width: 14px; height: 14px; margin-left: 0;"></div>
                        <div class="ms-3">
                            <div class="fw-bold text-dark" style="font-size: 0.85rem;">Demir siparişi yola çıktı (Merkez Depo).</div>
                            <div class="text-muted" style="font-size: 0.75rem;"><i class="bi bi-clock me-1"></i> Dün 11:00 &bull; 14'lük Nervürlü</div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    
    <!-- FİNANSAL RÖNTGEN -->
    <div class="col-lg-6">
        <div class="card border-0 shadow-sm rounded-4 h-100 bg-white">
            <div class="card-header bg-transparent border-bottom-0 pt-4 pb-2 px-4 d-flex justify-content-between align-items-center">
                <h6 class="fw-bold text-dark mb-0"><i class="bi bi-pie-chart-fill text-success me-2"></i> Finansal Röntgen</h6>
                <a href="/ProjectFinance/Index/@Model.Id" class="text-decoration-none small text-muted hover-success"><i class="bi bi-arrow-right"></i> Detay</a>
            </div>
            <div class="card-body px-4 pt-2 pb-4">
                <div class="d-flex justify-content-between align-items-end mb-2">
                    <div>
                        <div class="text-muted small fw-bold">Toplam Gider</div>
                        <h4 class="fw-bold text-dark mb-0">4.5M <span class="fs-6 text-muted">₺</span></h4>
                    </div>
                    <div class="text-end">
                        <div class="text-muted small fw-bold">Bütçe Durumu</div>
                        <span class="badge bg-success bg-opacity-10 text-success border border-success"><i class="bi bi-graph-up-arrow me-1"></i> Bütçe İçinde</span>
                    </div>
                </div>
                
                <!-- Yatay Progress Çubuğu -->
                <div class="progress rounded-pill mb-3 shadow-sm" style="height: 20px;">
                    <div class="progress-bar bg-danger" role="progressbar" style="width: 50%" aria-valuenow="50" aria-valuemin="0" aria-valuemax="100" title="Malzeme & Demir: %50"></div>
                    <div class="progress-bar bg-primary" role="progressbar" style="width: 30%" aria-valuenow="30" aria-valuemin="0" aria-valuemax="100" title="Taşeron Hakedişleri: %30"></div>
                    <div class="progress-bar bg-warning text-dark" role="progressbar" style="width: 20%" aria-valuenow="20" aria-valuemin="0" aria-valuemax="100" title="Personel & SSK: %20"></div>
                </div>
                
                <!-- Lejant (Legend) -->
                <div class="row g-2 mt-2">
                    <div class="col-4">
                        <div class="d-flex align-items-center">
                            <span class="d-inline-block bg-danger rounded-circle me-2" style="width: 10px; height: 10px;"></span>
                            <div class="lh-1">
                                <div class="small fw-bold text-dark">Malzeme</div>
                                <div class="text-muted" style="font-size: 0.7rem;">%50 (2.25M)</div>
                            </div>
                        </div>
                    </div>
                    <div class="col-4">
                        <div class="d-flex align-items-center">
                            <span class="d-inline-block bg-primary rounded-circle me-2" style="width: 10px; height: 10px;"></span>
                            <div class="lh-1">
                                <div class="small fw-bold text-dark">Taşeron</div>
                                <div class="text-muted" style="font-size: 0.7rem;">%30 (1.35M)</div>
                            </div>
                        </div>
                    </div>
                    <div class="col-4">
                        <div class="d-flex align-items-center">
                            <span class="d-inline-block bg-warning rounded-circle me-2" style="width: 10px; height: 10px;"></span>
                            <div class="lh-1">
                                <div class="small fw-bold text-dark">Personel</div>
                                <div class="text-muted" style="font-size: 0.7rem;">%20 (900K)</div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

'''

# handle encoding differences (Ö vs O)
target_alt = '<!-- ZET ALANLARI (COLLAPSIBLE) -->'

idx = content.find('<!--')
while idx != -1:
    if 'ZET ALANLARI' in content[idx:idx+50] and 'COLLAPSIBLE' in content[idx:idx+50]:
        break
    idx = content.find('<!--', idx+1)

if idx != -1:
    content = content[:idx] + new_blocks + content[idx:]
    with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
        f.write(content)
    print("Widgets added successfully.")
else:
    print("Could not find the insertion point!")

