import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

new_hero = '''<!-- MODERN SPLIT HERO SECTION -->
<div class="py-5" style="background-color: #030a16; position: relative; overflow: hidden; min-height: 500px; display: flex; align-items: center;">
    <div class="container position-relative z-1">
        <div class="row align-items-center">
            <!-- Sol Taraf: Metin ve Arama -->
            <div class="col-lg-6 text-start py-5">
                <h1 class="display-4 fw-bolder mb-4 text-white" style="letter-spacing: -1px;">
                    Sektörün Değişen Yüzü: <br/> 
                    <span class="text-orange">Dijital Ekosistem</span>
                </h1>
                <p class="lead text-white-50 mb-5" style="max-width: 90%;">
                    Aradığınız evi bulun, güvenilir usta çağırın veya tüm inşaat/şantiye süreçlerinizi tek tuşla yönetin. 
                </p>
                
                <!-- Kompakt Arama Kutusu -->
                <div class="bg-white p-2 rounded-pill shadow-lg d-flex align-items-center mb-3">
                    <select class="form-select border-0 fw-bold text-dark bg-transparent w-auto ms-3 shadow-none">
                        <option>Emlak Alım/Satım</option>
                        <option>Tadilat & Usta</option>
                        <option>Günlük Kiralık</option>
                    </select>
                    <div class="vr text-muted my-2"></div>
                    <input type="text" class="form-control border-0 bg-transparent shadow-none ms-2" placeholder="Neye ihtiyacınız var?">
                    <button class="btn btn-orange rounded-pill px-4 fw-bold">Hemen Bul</button>
                </div>
                <p class="small text-white-50 ms-3"><i class="bi bi-robot text-info me-1"></i> Yapay Zeka Asistanımız sizin için eşleştirsin.</p>
            </div>
            
            <!-- Sağ Taraf: İzometrik Görsel -->
            <div class="col-lg-6 d-none d-lg-block position-relative">
                <img src="/images/isometric-hero.jpg" class="w-100" style="mix-blend-mode: lighten; filter: contrast(1.1) brightness(1.1); transform: scale(1.15);" alt="Dijital Ekosistem">
            </div>
        </div>
    </div>
</div>
'''

content = re.sub(r'<!-- MASTER HERO.*?</div>\s*</div>\s*</div>\s*</div>', new_hero, content, flags=re.DOTALL)
# fallback if regex fails
if 'MODERN SPLIT' not in content:
    content = re.sub(r'<!-- MASTER HERO.*?</div>\s*</div>', new_hero, content, flags=re.DOTALL)

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
