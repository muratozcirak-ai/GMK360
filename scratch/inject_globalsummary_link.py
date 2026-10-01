import io
import re

filepath = r'GMK360.Web\Views\DigitalHome\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

card_html = """
        <!-- PATRON PUANTAJ ÖZET KARTI -->
        <div class="col-xl-3 col-sm-6 mb-xl-0 mb-4 mt-4">
            <a href="/DailyTimesheet/GlobalSummary" class="text-decoration-none">
                <div class="card h-100 shadow-sm border-0 border-start border-warning border-4 hover-shadow">
                    <div class="card-body p-3">
                        <div class="row">
                            <div class="col-8">
                                <div class="numbers">
                                    <p class="text-sm mb-0 text-capitalize font-weight-bold text-dark">Patron Paneli</p>
                                    <h6 class="font-weight-bolder text-warning mb-0">Tüm Şantiyeler Puantajı</h6>
                                    <span class="text-xs text-muted">Yemek ve Personel Özeti</span>
                                </div>
                            </div>
                            <div class="col-4 text-end">
                                <div class="icon icon-shape bg-gradient-warning shadow text-center border-radius-md">
                                    <i class="bi bi-bar-chart-steps text-lg opacity-10" aria-hidden="true"></i>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </a>
        </div>
"""

# Just append it near the "Aktif Şantiyeler" section or at the end of the top row.
# We can search for the first row and append it.
content = re.sub(r'(<div class="col-xl-3 col-sm-6 mb-xl-0 mb-4">.*?</div>\s*</div>\s*</div>\s*</a>\s*</div>)', r'\1\n' + card_html, content, count=1, flags=re.DOTALL)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Injected GlobalSummary link to DigitalHome")
