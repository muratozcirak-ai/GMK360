import io
import re

filepath = r'GMK360.Web\Views\DigitalHome\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

card_html = """
        <!-- BİREYSEL MÜLK & KİRACI TAKİBİ -->
        <div class="col-xl-3 col-sm-6 mb-xl-0 mb-4 mt-4">
            <a href="/Landlord/Dashboard" class="text-decoration-none">
                <div class="card h-100 shadow-sm border-0 border-start border-primary border-4 hover-shadow">
                    <div class="card-body p-3">
                        <div class="row">
                            <div class="col-8">
                                <div class="numbers">
                                    <p class="text-sm mb-0 text-capitalize font-weight-bold text-dark">Bireysel Mülk Takibi</p>
                                    <h6 class="font-weight-bolder text-primary mb-0">Ev & Kiracı Yönetimi</h6>
                                    <span class="text-xs text-muted">Kira Sözleşmeleri ve Tahsilat</span>
                                </div>
                            </div>
                            <div class="col-4 text-end">
                                <div class="icon icon-shape bg-gradient-primary shadow text-center border-radius-md">
                                    <i class="bi bi-houses-fill text-lg opacity-10" aria-hidden="true"></i>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </a>
        </div>
"""

# Append it near the previously injected GlobalSummary link
content = re.sub(r'(<!-- PATRON PUANTAJ ÖZET KARTI -->.*?</div>\s*</a>\s*</div>)', r'\1\n' + card_html, content, count=1, flags=re.DOTALL)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Injected Landlord Dashboard link to DigitalHome")
