import io

filepath = r'GMK360.Web\Views\PublicRealEstate\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# I will replace the hero section to include the Marketplace Hooks
target_hero = """<p class="lead mb-4 opacity-75">Binlerce satılık ve kiralık emlak ilanı tek tık uzağınızda.</p>"""
replacement_hero = """<p class="lead mb-4 opacity-75">Emlak, Tadilat, Hizmet ve Malzeme Tedariği Tek Çatıda.</p>
        
        <!-- PAZARYERİ KANCALARI (MARKETPLACE HOOKS) -->
        <div class="row justify-content-center mb-4 g-3">
            <div class="col-auto">
                <a href="#emlak" class="btn btn-outline-light rounded-pill px-4 active fw-bold">🏠 Ev Arıyorum</a>
            </div>
            <div class="col-auto">
                <a href="/Usta/TalepAc" class="btn btn-outline-warning rounded-pill px-4 fw-bold">🛠️ Usta/Firma Arıyorum</a>
            </div>
            <div class="col-auto">
                <a href="/Usta/Basvuru" class="btn btn-warning rounded-pill px-4 fw-bold text-dark shadow">Hizmet/Malzeme Sağlayıcıyım</a>
            </div>
        </div>"""

content = content.replace(target_hero, replacement_hero)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Updated PublicRealEstate/Index.cshtml with Marketplace Hooks")
