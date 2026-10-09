import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Replace the 3rd card's link
content = re.sub(r'(<!-- 3\. Mülk & Kiracı Takibi -->.*?href=")(".*?class="text-decoration-none fw-bold text-success mt-auto d-inline-block stretched-link">Sistemi Keşfet)', r'\1/Modules/Mulk" target="_blank\2', content, flags=re.DOTALL)

# Add the 9th VIP module at the end of the 8-card grid
vip_module = '''    </div> <!-- End of 8-card row -->

    <!-- 9. VIP Varlık & Portföy Yönetimi (8+1 Premium Layer) -->
    <div class="row mt-5">
        <div class="col-12">
            <div class="card border-0 shadow-lg rounded-4 overflow-hidden bg-navy text-white hover-lift">
                <div class="row g-0">
                    <div class="col-lg-8 p-4 p-lg-5 d-flex flex-column justify-content-center">
                        <div class="d-flex align-items-center mb-3">
                            <div class="bg-warning bg-opacity-25 rounded-3 d-flex align-items-center justify-content-center me-3 flex-shrink-0" style="width: 50px; height: 50px;">
                                <i class="bi bi-gem fs-4 text-warning"></i>
                            </div>
                            <h4 class="fw-bold text-white mb-0">VIP Varlık & Portföy Yönetimi</h4>
                        </div>
                        <p class="text-white-50 mb-4" style="line-height: 1.6;">
                            <strong>Mali Müşavirler, Varlık Fonları (Family Office) ve Profesyonel Mülk Danışmanları için Özel Çözüm.</strong><br>
                            Farklı müşterilerinize ait onlarca dağınık mülkün kira tahsilatını, GMSİ vergi süreçlerini ve bakım operasyonlarını tek bir merkezden, elit bir kokpitle kusursuzca yönetin.
                        </p>
                        <div>
                            <a href="#" class="btn btn-outline-warning fw-bold px-4 rounded-pill stretched-link">VIP Arayüzü Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                        </div>
                    </div>
                    <div class="col-lg-4 bg-dark bg-opacity-50 d-flex align-items-center justify-content-center p-4">
                        <div class="text-center">
                            <i class="bi bi-briefcase text-warning opacity-75 mb-3" style="font-size: 5rem;"></i>
                            <h6 class="text-white-50 tracking-wide text-uppercase mb-0">Kurumsal Müşavir Kokpiti</h6>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div> <!-- End of container-fluid -->
'''

# The original has "    </div>\n</div>\n\n<!-- ORTAK VİZYON & SİSTEM FELSEFESİ -->"
# We need to insert the VIP module right before the closing of the container-fluid holding the 8 cards.
content = content.replace('    </div>\n</div>\n\n<!-- ORTAK VİZYON', vip_module + '\n\n<!-- ORTAK VİZYON')

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
