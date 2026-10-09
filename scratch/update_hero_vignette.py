import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

new_hero = '''<!-- CENTERED HERO TRAFFIC ROUTER (UNIFIED NETWORK) -->
<div class="py-4" style="background: url('/images/hero-center.jpg') no-repeat center center; background-size: cover; position: relative; min-height: 400px; display: flex; align-items: center; background-color: #0f172a;">
    
    <!-- Koyu Lacivert Dairesel Karartma (Radial Gradient Vignette) -->
    <div style="position: absolute; top: 0; left: 0; right: 0; bottom: 0; background: radial-gradient(circle at center, rgba(15, 23, 42, 0.95) 0%, rgba(15, 23, 42, 0.7) 40%, rgba(15, 23, 42, 0.1) 100%);"></div>

    <div class="container position-relative z-1 text-center py-3">
        <h1 class="display-5 fw-bolder mb-5 text-white" style="letter-spacing: -1px; text-shadow: 0 4px 15px rgba(0,0,0,0.8);">
            Sektörün Değişen Yüzü: <br/> 
            <span class="text-orange">Dijital Ekosistem</span>
        </h1>
        
        <!-- TRAFFIC ROUTER BUTTONS (SHRUNK) -->
        <div class="row g-3 justify-content-center mx-auto" style="max-width: 800px;">
           <!-- 1. Emlak -->
           <div class="col-md-6">
               <a href="/PublicRealEstate" class="text-decoration-none text-start bg-navy bg-opacity-75 rounded-4 p-2 d-flex align-items-center hover-lift border border-secondary border-opacity-25 h-100 backdrop-blur" style="backdrop-filter: blur(8px);">
                   <div class="bg-orange rounded-circle d-flex justify-content-center align-items-center mx-3 flex-shrink-0" style="width: 35px; height: 35px;">
                       <i class="bi bi-house-door text-white fs-6"></i>
                   </div>
                   <div>
                       <small class="text-white-50 d-block" style="font-size: 0.75rem;">Ev mi arıyorsunuz? Mülk mü satacaksınız?</small>
                       <h6 class="text-white fw-bold mb-0" style="font-size: 0.9rem;">Emlak Portalını Keşfet <i class="bi bi-arrow-right ms-1 text-orange"></i></h6>
                   </div>
               </a>
           </div>
           <!-- 2. Usta -->
           <div class="col-md-6">
               <a href="/ServiceProvider" class="text-decoration-none text-start bg-navy bg-opacity-75 rounded-4 p-2 d-flex align-items-center hover-lift border border-secondary border-opacity-25 h-100 backdrop-blur" style="backdrop-filter: blur(8px);">
                   <div class="bg-primary rounded-circle d-flex justify-content-center align-items-center mx-3 flex-shrink-0" style="width: 35px; height: 35px;">
                       <i class="bi bi-tools text-white fs-6"></i>
                   </div>
                   <div>
                       <small class="text-white-50 d-block" style="font-size: 0.75rem;">Tadilat, tamirat veya güvenilir usta mı lazım?</small>
                       <h6 class="text-white fw-bold mb-0" style="font-size: 0.9rem;">Usta Ağına Bağlan <i class="bi bi-arrow-right ms-1 text-primary"></i></h6>
                   </div>
               </a>
           </div>
           <!-- 3. Günlük Kiralık -->
           <div class="col-md-6">
               <a href="/Home/GunlukKiralama" class="text-decoration-none text-start bg-navy bg-opacity-75 rounded-4 p-2 d-flex align-items-center hover-lift border border-secondary border-opacity-25 h-100 backdrop-blur" style="backdrop-filter: blur(8px);">
                   <div class="bg-info rounded-circle d-flex justify-content-center align-items-center mx-3 flex-shrink-0" style="width: 35px; height: 35px;">
                       <i class="bi bi-calendar-check text-white fs-6"></i>
                   </div>
                   <div>
                       <small class="text-white-50 d-block" style="font-size: 0.75rem;">Otellerden sıkıldınız mı? Ev konforunu yaşayın.</small>
                       <h6 class="text-white fw-bold mb-0" style="font-size: 0.9rem;">Günlük Kiralık Dünyası <i class="bi bi-arrow-right ms-1 text-info"></i></h6>
                   </div>
               </a>
           </div>
           <!-- 4. Pazar Yeri -->
           <div class="col-md-6">
               <a href="/B2BPurchasing/Index" class="text-decoration-none text-start bg-navy bg-opacity-75 rounded-4 p-2 d-flex align-items-center hover-lift border border-secondary border-opacity-25 h-100 backdrop-blur" style="backdrop-filter: blur(8px);">
                   <div class="bg-success rounded-circle d-flex justify-content-center align-items-center mx-3 flex-shrink-0" style="width: 35px; height: 35px;">
                       <i class="bi bi-shop text-white fs-6"></i>
                   </div>
                   <div>
                       <small class="text-white-50 d-block" style="font-size: 0.75rem;">İnşaat malzemesi, mobilya veya hırdavat arayın.</small>
                       <h6 class="text-white fw-bold mb-0" style="font-size: 0.9rem;">Tedarikçi Pazar Yeri <i class="bi bi-arrow-right ms-1 text-success"></i></h6>
                   </div>
               </a>
           </div>
        </div>
    </div>
</div>
'''

content = re.sub(r'<!-- CENTERED HERO TRAFFIC ROUTER.*?</div>\s*</div>\s*</div>\s*</div>\s*</div>', new_hero, content, flags=re.DOTALL)

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
