import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

pattern = re.compile(r'<div class="col-lg-6">\s*<div class="card border-0 shadow-sm rounded-4 h-100 bg-white">\s*<div class="card-header bg-transparent border-bottom-0 pt-4 pb-2 px-4 d-flex justify-content-between align-items-center">\s*<h6 class="fw-bold text-dark mb-0"><i class="bi bi-activity text-primary me-2"></i>.*?</div>\s*</div>\s*</div>\s*</div>\s*</div>', re.DOTALL)

# wait, it's safer to just split by the exact HTML we injected earlier, since I have the exact string I wrote!
# I wrote this earlier:
exact_html = '''    <div class="col-lg-6">
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
    </div>'''

new_html = '''    <!-- PERSONEL TRENDİ -->
    <div class="col-lg-6">
        <div class="card border-0 shadow-sm rounded-4 h-100 bg-white">
            <div class="card-header bg-transparent border-bottom-0 pt-4 pb-2 px-4 d-flex justify-content-between align-items-center">
                <h6 class="fw-bold text-dark mb-0"><i class="bi bi-people-fill text-primary me-2"></i> Sahadaki Personel (Son 6 Gün)</h6>
                <a href="/DailyTimesheets/ProjectTimesheet/@Model.Id" class="text-decoration-none small text-muted hover-primary" title="Puantaja Git"><i class="bi bi-arrow-right"></i> Tümünü Gör</a>
            </div>
            <div class="card-body px-4 pt-1 pb-4 d-flex flex-column justify-content-end">
                
                <!-- CSS Bar Chart -->
                <div class="d-flex align-items-end justify-content-between h-100 mt-2 px-1 px-md-3" style="min-height: 120px;">
                    <!-- Gün 1 -->
                    <div class="d-flex flex-column align-items-center hover-lift" title="Pazartesi: 10 Personel">
                        <div class="text-muted small fw-bold mb-1">10</div>
                        <div class="bg-light border border-secondary border-opacity-25 rounded-top transition" style="width: 28px; height: 40px;"></div>
                        <div class="small text-muted mt-2" style="font-size: 0.7rem;">Pzt</div>
                    </div>
                    <!-- Gün 2 -->
                    <div class="d-flex flex-column align-items-center hover-lift" title="Salı: 10 Personel">
                        <div class="text-muted small fw-bold mb-1">10</div>
                        <div class="bg-light border border-secondary border-opacity-25 rounded-top transition" style="width: 28px; height: 40px;"></div>
                        <div class="small text-muted mt-2" style="font-size: 0.7rem;">Sal</div>
                    </div>
                    <!-- Gün 3 -->
                    <div class="d-flex flex-column align-items-center hover-lift" title="Çarşamba: 10 Personel">
                        <div class="text-muted small fw-bold mb-1">10</div>
                        <div class="bg-light border border-secondary border-opacity-25 rounded-top transition" style="width: 28px; height: 40px;"></div>
                        <div class="small text-muted mt-2" style="font-size: 0.7rem;">Çar</div>
                    </div>
                    <!-- Gün 4 (Spike) -->
                    <div class="d-flex flex-column align-items-center hover-lift" title="Perşembe: 14 Personel">
                        <div class="text-primary fw-bold mb-1">14</div>
                        <div class="bg-primary bg-opacity-75 rounded-top shadow-sm transition" style="width: 28px; height: 75px;"></div>
                        <div class="small fw-bold text-dark mt-2" style="font-size: 0.7rem;">Per</div>
                    </div>
                    <!-- Gün 5 -->
                    <div class="d-flex flex-column align-items-center hover-lift" title="Cuma: 12 Personel">
                        <div class="text-secondary fw-bold mb-1">12</div>
                        <div class="bg-secondary bg-opacity-50 rounded-top transition" style="width: 28px; height: 60px;"></div>
                        <div class="small text-muted mt-2" style="font-size: 0.7rem;">Cum</div>
                    </div>
                    <!-- Gün 6 (Bugün) -->
                    <div class="d-flex flex-column align-items-center hover-lift" title="Bugün: 28 Personel">
                        <div class="text-success fw-bold mb-1 fs-6">28</div>
                        <div class="bg-success rounded-top shadow-sm transition" style="width: 28px; height: 100px;"></div>
                        <div class="small fw-bold text-success mt-2" style="font-size: 0.75rem;">Bgn</div>
                    </div>
                </div>

            </div>
        </div>
    </div>'''

# Instead of exact replace (which might fail on encoding), I will use regex on the div structure.
# We look for <div class="col-lg-6"> followed by <i class="bi bi-activity text-primary me-2"></i>
# and ending at <!-- FİNANSAL RÖNTGEN -->
pattern2 = re.compile(r'<div class="col-lg-6">\s*<div class="card border-0 shadow-sm rounded-4 h-100 bg-white">\s*<div class="card-header.*?bi-activity.*?</div>.*?</div>\s*</div>\s*</div>\s*<!--', re.DOTALL)

match = pattern2.search(content)
if match:
    # preserve the <!-- 
    replaced = pattern2.sub(new_html + '\n    <!--', content, count=1)
    with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
        f.write(replaced)
    print("Replaced!")
else:
    print("Not found!")

