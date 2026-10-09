import codecs
import re

path = 'GMK360.Web/Views/ProjectFinance/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

banner_html = '''
    <!-- Toplam Maliyet Özeti (Fizibilite vs Gerçekleşen) -->
    <div class="row g-3 mb-4 mt-2">
        <div class="col-md-4">
            <div class="card border-0 shadow-sm rounded-4 bg-primary text-white h-100">
                <div class="card-body p-3 text-center">
                    <h6 class="fw-bold mb-1"><i class="bi bi-calculator me-1"></i> Toplam Planlanan Bütçe (Fizibilite)</h6>
                    <h4 class="fw-bold mb-0">@(((decimal)ViewBag.TotalPlanned).ToString("N2")) ₺</h4>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card border-0 shadow-sm rounded-4 bg-success text-white h-100">
                <div class="card-body p-3 text-center">
                    <h6 class="fw-bold mb-1"><i class="bi bi-cash-coin me-1"></i> Toplam Gerçekleşen Harcama</h6>
                    <h4 class="fw-bold mb-0">@(((decimal)ViewBag.TotalActual).ToString("N2")) ₺</h4>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card border-0 shadow-sm rounded-4 @((decimal)ViewBag.TotalDiff > 0 ? "bg-danger" : "bg-info") text-white h-100">
                <div class="card-body p-3 text-center">
                    <h6 class="fw-bold mb-1"><i class="bi bi-activity me-1"></i> Bütçe Sapması (Fark)</h6>
                    <h4 class="fw-bold mb-0">@((Math.Abs((decimal)ViewBag.TotalDiff)).ToString("N2")) ₺ @((decimal)ViewBag.TotalDiff > 0 ? "(Aşıldı)" : "(Tasarruf)")</h4>
                </div>
            </div>
        </div>
    </div>
'''

if 'Toplam Planlanan Bütçe' not in content:
    content = content.replace(
        '<!-- Alt Kısım: Detaylı Akordeon Yapısı -->',
        banner_html + '\n    <!-- Alt Kısım: Detaylı Akordeon Yapısı -->'
    )

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print("Updated ProjectFinance Index")