import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

new_hero = '''<!-- CENTERED HERO WITH EMPTY MIDDLE BANNER -->
<div class="py-5" style="background: #091325 url('/images/hero-center.jpg') no-repeat center center; background-size: cover; position: relative; min-height: 480px; display: flex; align-items: center;">
    <div class="container position-relative z-1 text-center py-4">
        <h1 class="display-4 fw-bolder mb-3 text-white" style="letter-spacing: -1px; text-shadow: 0 4px 15px rgba(0,0,0,0.5);">
            Sektörün Değişen Yüzü: <br/> 
            <span class="text-orange">Dijital Ekosistem</span>
        </h1>
        <p class="lead text-white-50 mb-5 mx-auto" style="max-width: 650px; text-shadow: 0 2px 10px rgba(0,0,0,0.5);">
            Aradığınız evi bulun, güvenilir usta çağırın veya tüm inşaat/şantiye süreçlerinizi tek tuşla yönetin.
        </p>
        
        <div class="bg-white p-2 rounded-pill shadow-lg d-inline-flex align-items-center mb-3">
            <select class="form-select border-0 fw-bold text-dark bg-transparent w-auto ms-3 shadow-none">
                <option>Emlak Alım/Satım</option>
                <option>Tadilat & Usta</option>
                <option>Günlük Kiralık</option>
            </select>
            <div class="vr text-muted my-2 mx-2"></div>
            <input type="text" class="form-control border-0 bg-transparent shadow-none" style="min-width:300px;" placeholder="İl, ilçe, proje veya hizmet arayın...">
            <button class="btn btn-orange rounded-pill px-5 fw-bold ms-2">Hemen Bul</button>
        </div>
        <p class="small text-white-50 mt-2"><i class="bi bi-robot text-info me-1"></i> Yapay Zeka Asistanımız sizin için eşleştirsin.</p>
    </div>
</div>
'''

content = re.sub(r'<!-- MODERN SPLIT HERO SECTION.*?(?=<!-- DETAYLI MOD)', new_hero + '\n', content, flags=re.DOTALL)

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
