import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Replace the exact block
old_card2 = '''<!-- 2. Bina, Site & AVM Yönetimi -->
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
                    <a href="#" class="text-decoration-none fw-bold text-info mt-auto d-inline-block stretched-link" style="font-size: 0.8rem;">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>'''

new_card2 = '''<!-- 2. Bina, Site & AVM Yönetimi -->
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
                    <a href="/Modules/Bina" target="_blank" class="text-decoration-none fw-bold text-info mt-auto d-inline-block stretched-link" style="font-size: 0.8rem;">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>'''

content = content.replace(old_card2, new_card2)

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
