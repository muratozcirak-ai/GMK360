import re
with open(r'GMK360.Web\Views\Shared\_Layout.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

new_footer = '''    <footer class="bg-navy text-white mt-auto py-5 border-top border-secondary border-opacity-25">
        <div class="container-fluid px-4 px-lg-5">
            <div class="row gy-5">
                
                <!-- 1. Kurumsal Kimlik & Güven -->
                <div class="col-lg-3 col-md-6">
                    <span class="fs-4 fw-bold text-white d-flex align-items-center mb-3">
                        <i class="bi bi-infinity text-orange me-2 fs-3"></i> GMK<span class="text-orange">360</span>
                    </span>
                    <p class="text-white-50 small mb-4 lh-sm" style="max-width: 300px;">
                        Yaşam ve İş Alanınızın İhtiyaç Duyduğu Her Şey Tek Platformda.
                    </p>
                    
                    <ul class="list-unstyled text-white-50 small mb-4">
                        <li class="mb-2"><i class="bi bi-geo-alt me-2 text-orange"></i> Ataşehir, İstanbul / Türkiye</li>
                        <li class="mb-2"><i class="bi bi-telephone me-2 text-orange"></i> 0850 123 45 67</li>
                        <li class="mb-2"><i class="bi bi-envelope me-2 text-orange"></i> info@gmk360.com</li>
                    </ul>

                    <div class="d-flex gap-3">
                        <a href="#" class="text-white-50 text-decoration-none hover-orange transition-300"><i class="bi bi-linkedin fs-5"></i></a>
                        <a href="#" class="text-white-50 text-decoration-none hover-orange transition-300"><i class="bi bi-instagram fs-5"></i></a>
                        <a href="#" class="text-white-50 text-decoration-none hover-orange transition-300"><i class="bi bi-youtube fs-5"></i></a>
                    </div>
                </div>

                <!-- 2. Profesyonel Çözümler -->
                <div class="col-lg-3 col-md-6">
                    <h6 class="text-uppercase fw-bold text-white mb-4 tracking-wide">ÇÖZÜMLER</h6>
                    <ul class="list-unstyled d-flex flex-column gap-3 small">
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Şantiye & İnşaat ERP</a></li>
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Bina & Site Yönetimi</a></li>
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Tedarikçi Pazar Yeri</a></li>
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Emlak & Acente Ağı</a></li>
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Usta & Hizmet Ekosistemi</a></li>
                    </ul>
                </div>

                <!-- 3. Kurumsal & Yasal -->
                <div class="col-lg-3 col-md-6">
                    <h6 class="text-uppercase fw-bold text-white mb-4 tracking-wide">KURUMSAL</h6>
                    <ul class="list-unstyled d-flex flex-column gap-3 small">
                        <li><a href="/Home/About" class="text-white-50 text-decoration-none hover-white transition-300">Hakkımızda</a></li>
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">KVKK Aydınlatma Metni</a></li>
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Kullanım Koşulları</a></li>
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Gizlilik Politikası</a></li>
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Sıkça Sorulan Sorular (SSS)</a></li>
                    </ul>
                </div>

                <!-- 4. Girişler & Uygulama & Yapay Zeka -->
                <div class="col-lg-3 col-md-6">
                    <h6 class="text-uppercase fw-bold text-white mb-4 tracking-wide">GİRİŞ & DESTEK</h6>
                    
                    <div class="d-flex flex-column gap-2 mb-4">
                        <a href="/Auth/Login" class="btn btn-outline-secondary btn-sm text-start text-white border-secondary hover-bg-navy"><i class="bi bi-person me-2"></i> Müşteri / Kullanıcı Girişi</a>
                        <a href="/Admin/Index" class="btn btn-outline-orange btn-sm text-start text-white border-orange"><i class="bi bi-shield-lock me-2"></i> Pro / Yönetici Girişi</a>
                        <button class="btn btn-orange btn-sm text-start fw-bold shadow-sm mt-2"><i class="bi bi-robot me-2"></i> Akıllı Asistan'a Sor</button>
                    </div>

                    <h6 class="small text-white-50 mb-3 mt-4">Mobil Uygulamamızı İndirin</h6>
                    <div class="d-flex gap-2">
                        <a href="#" class="d-inline-block bg-dark border border-secondary rounded px-3 py-1 text-white text-decoration-none d-flex align-items-center hover-lift">
                            <i class="bi bi-google-play fs-4 me-2"></i>
                            <div class="text-start lh-1">
                                <span style="font-size: 0.6rem;" class="d-block text-white-50">GET IT ON</span>
                                <span style="font-size: 0.8rem;" class="fw-bold">Google Play</span>
                            </div>
                        </a>
                        <a href="#" class="d-inline-block bg-dark border border-secondary rounded px-3 py-1 text-white text-decoration-none d-flex align-items-center hover-lift">
                            <i class="bi bi-apple fs-4 me-2"></i>
                            <div class="text-start lh-1">
                                <span style="font-size: 0.6rem;" class="d-block text-white-50">Download on the</span>
                                <span style="font-size: 0.8rem;" class="fw-bold">App Store</span>
                            </div>
                        </a>
                    </div>
                </div>

            </div>
            
            <hr class="border-secondary my-4 border-opacity-50">
            
            <div class="d-flex flex-column flex-md-row justify-content-between align-items-center text-white-50 small">
                <div class="mb-2 mb-md-0">
                    &copy; @DateTime.Now.Year GMK360 Süper Uygulama. Tüm hakları saklıdır.
                </div>
                <div>
                    Made with <i class="bi bi-heart-fill text-danger mx-1"></i> by GMK Tech
                </div>
            </div>
        </div>
    </footer>
    
    <style>
        .hover-white:hover { color: #fff !important; }
        .hover-orange:hover { color: #f97316 !important; border-color: #f97316 !important; }
        .hover-bg-navy:hover { background-color: rgba(255,255,255,0.1) !important; }
        .transition-300 { transition: all 0.3s ease; }
        .border-orange { border-color: #f97316 !important; }
        .btn-outline-orange { border-color: #f97316; color: #f97316; }
        .btn-outline-orange:hover { background-color: #f97316; color: #fff; }
    </style>'''

content = re.sub(r'<footer class="bg-navy text-white mt-auto py-5 border-top border-secondary border-opacity-25">.*?</footer>', new_footer, content, flags=re.DOTALL)

with open(r'GMK360.Web\Views\Shared\_Layout.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
