import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# 1. Remove the long VIP banner at the bottom
banner_pattern = r'<!-- 9\. VIP Varlık & Portföy Yönetimi.*?</div>\s*</div>\s*</div>'
content = re.sub(banner_pattern, '', content, flags=re.DOTALL)

# 2. Replace the 8th card (Yapay Zeka) with VIP Varlık
old_card8 = '''<!-- 8. Yapay Zeka & Evrak -->
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
                    <a href="#" class="text-decoration-none fw-bold text-dark mt-auto d-inline-block stretched-link" style="font-size: 0.8rem;">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>'''

new_card8 = '''<!-- 8. VIP Varlık & Portföy Yönetimi -->
        <div class="col-lg-3 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 h-100 hover-lift bg-white">
                <div class="card-body p-3 d-flex flex-column">
                    <div class="d-flex align-items-center mb-2">
                        <div class="bg-dark bg-opacity-10 rounded-3 d-flex align-items-center justify-content-center me-3 flex-shrink-0" style="width: 40px; height: 40px;">
                            <i class="bi bi-gem fs-5 text-dark"></i>
                        </div>
                        <h6 class="fw-bold text-navy mb-0 lh-sm">VIP Varlık & Portföy</h6>
                    </div>
                    <p class="text-muted mb-3" style="font-size: 0.8rem; line-height: 1.4;">Mali müşavirler, family office ve varlık yöneticileri için holding seviyesinde mülk ve finans kokpiti.</p>
                    <a href="/Modules/Vip" target="_blank" class="text-decoration-none fw-bold text-dark mt-auto d-inline-block stretched-link" style="font-size: 0.8rem;">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>'''

content = content.replace(old_card8, new_card8)

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
