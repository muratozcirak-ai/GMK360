import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

new_hero = '''<!-- CENTERED HERO TRAFFIC ROUTER -->
<div class="py-5" style="background: #091325 url('/images/hero-center.jpg') no-repeat center center; background-size: cover; position: relative; min-height: 500px; display: flex; align-items: center;">
    <div class="container position-relative z-1 text-center py-4">
        <h1 class="display-4 fw-bolder mb-3 text-white" style="letter-spacing: -1px; text-shadow: 0 4px 15px rgba(0,0,0,0.5);">
            Sektörün Değişen Yüzü: <br/> 
            <span class="text-orange">Dijital Ekosistem</span>
        </h1>
        <p class="lead text-white-50 mb-5 mx-auto" style="max-width: 650px; text-shadow: 0 2px 10px rgba(0,0,0,0.5);">
            Aradığınız evi bulun, güvenilir usta çağırın veya tüm inşaat/şantiye süreçlerinizi tek tuşla yönetin.
        </p>
        
        <!-- TRAFFIC ROUTER BUTTONS -->
        <div class="row g-3 justify-content-center mx-auto" style="max-width: 900px;">
           <!-- 1. Emlak -->
           <div class="col-md-6">
               <a href="/PublicRealEstate" class="text-decoration-none text-start bg-white bg-opacity-10 rounded-4 p-3 d-flex align-items-center hover-lift border border-white border-opacity-10 h-100 backdrop-blur" style="backdrop-filter: blur(10px);">
                   <div class="bg-orange rounded-circle d-flex justify-content-center align-items-center me-3 flex-shrink-0" style="width: 50px; height: 50px;">
                       <i class="bi bi-house-door text-white fs-4"></i>
                   </div>
                   <div>
                       <small class="text-white-50 d-block mb-1">Ev mi arıyorsunuz? Mülk mü satacaksınız?</small>
                       <h6 class="text-white fw-bold mb-0">Emlak Portalını Keşfet <i class="bi bi-arrow-right ms-1 text-orange"></i></h6>
                   </div>
               </a>
           </div>
           <!-- 2. Usta -->
           <div class="col-md-6">
               <a href="/ServiceProvider" class="text-decoration-none text-start bg-white bg-opacity-10 rounded-4 p-3 d-flex align-items-center hover-lift border border-white border-opacity-10 h-100 backdrop-blur" style="backdrop-filter: blur(10px);">
                   <div class="bg-primary rounded-circle d-flex justify-content-center align-items-center me-3 flex-shrink-0" style="width: 50px; height: 50px;">
                       <i class="bi bi-tools text-white fs-4"></i>
                   </div>
                   <div>
                       <small class="text-white-50 d-block mb-1">Tadilat, tamirat veya güvenilir bir usta mı lazım?</small>
                       <h6 class="text-white fw-bold mb-0">Usta Ağına Bağlan <i class="bi bi-arrow-right ms-1 text-primary"></i></h6>
                   </div>
               </a>
           </div>
           <!-- 3. Günlük Kiralık -->
           <div class="col-md-6">
               <a href="/Home/GunlukKiralama" class="text-decoration-none text-start bg-white bg-opacity-10 rounded-4 p-3 d-flex align-items-center hover-lift border border-white border-opacity-10 h-100 backdrop-blur" style="backdrop-filter: blur(10px);">
                   <div class="bg-info rounded-circle d-flex justify-content-center align-items-center me-3 flex-shrink-0" style="width: 50px; height: 50px;">
                       <i class="bi bi-calendar-check text-white fs-4"></i>
                   </div>
                   <div>
                       <small class="text-white-50 d-block mb-1">Otellerden sıkıldınız mı? Ev konforunu yaşayın.</small>
                       <h6 class="text-white fw-bold mb-0">Günlük Kiralık Dünyası <i class="bi bi-arrow-right ms-1 text-info"></i></h6>
                   </div>
               </a>
           </div>
           <!-- 4. Pazar Yeri -->
           <div class="col-md-6">
               <a href="/B2BPurchasing/Index" class="text-decoration-none text-start bg-white bg-opacity-10 rounded-4 p-3 d-flex align-items-center hover-lift border border-white border-opacity-10 h-100 backdrop-blur" style="backdrop-filter: blur(10px);">
                   <div class="bg-success rounded-circle d-flex justify-content-center align-items-center me-3 flex-shrink-0" style="width: 50px; height: 50px;">
                       <i class="bi bi-shop text-white fs-4"></i>
                   </div>
                   <div>
                       <small class="text-white-50 d-block mb-1">İnşaat malzemesi, mobilya veya hırdavat arayın.</small>
                       <h6 class="text-white fw-bold mb-0">Tedarikçi Pazar Yeri <i class="bi bi-arrow-right ms-1 text-success"></i></h6>
                   </div>
               </a>
           </div>
        </div>
    </div>
</div>
'''

content = re.sub(r'<!-- MODERN SPLIT HERO SECTION.*?(?=<!-- DETAYLI MOD)', new_hero + '\n', content, flags=re.DOTALL)
# Fallback if I renamed it
if 'CENTERED HERO WITH EMPTY MIDDLE BANNER' in content:
    content = re.sub(r'<!-- CENTERED HERO WITH EMPTY MIDDLE BANNER.*?(?=<!-- DETAYLI MOD)', new_hero + '\n', content, flags=re.DOTALL)

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
