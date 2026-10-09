import re
with open(r'GMK360.Web\Views\Shared\_Layout.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

new_footer = '''    <footer class="bg-navy text-white mt-auto py-5 border-top border-secondary border-opacity-25">
        <div class="container-fluid px-4 px-lg-5">
            <div class="row gy-5">
                
                <!-- 1. Marka & Sosyal Medya -->
                <div class="col-lg-3 col-md-6">
                    <span class="fs-4 fw-bold text-white d-flex align-items-center mb-3">
                        <i class="bi bi-infinity text-orange me-2 fs-3"></i> GMK<span class="text-orange">360</span>
                    </span>
                    <p class="text-white-50 small mb-4 lh-lg" style="max-width: 300px;">
                        Tüm yaşam ve iş alanlarınızı tek merkezden yönetmenizi sağlayan, yapay zeka destekli yeni nesil dijital ekosistem.
                    </p>
                    <div class="d-flex gap-3">
                        <a href="#" class="text-white-50 text-decoration-none hover-orange transition-300"><i class="bi bi-linkedin fs-5"></i></a>
                        <a href="#" class="text-white-50 text-decoration-none hover-orange transition-300"><i class="bi bi-instagram fs-5"></i></a>
                        <a href="#" class="text-white-50 text-decoration-none hover-orange transition-300"><i class="bi bi-twitter-x fs-5"></i></a>
                        <a href="#" class="text-white-50 text-decoration-none hover-orange transition-300"><i class="bi bi-youtube fs-5"></i></a>
                    </div>
                </div>

                <!-- 2. Çözümler / Modüller -->
                <div class="col-lg-3 col-md-6">
                    <h6 class="text-uppercase fw-bold text-white mb-4 tracking-wide">Çözümlerimiz</h6>
                    <ul class="list-unstyled d-flex flex-column gap-3 small">
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">İnşaat & Şantiye ERP</a></li>
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Bina, Site & AVM Yönetimi</a></li>
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Mülk & Kiracı Takibi</a></li>
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Tedarikçi Pazar Yeri</a></li>
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Usta & Tadilat Ağı</a></li>
                    </ul>
                </div>

                <!-- 3. Kurumsal & Destek -->
                <div class="col-lg-3 col-md-6">
                    <h6 class="text-uppercase fw-bold text-white mb-4 tracking-wide">Kurumsal & Destek</h6>
                    <ul class="list-unstyled d-flex flex-column gap-3 small">
                        <li><a href="/Home/About" class="text-white-50 text-decoration-none hover-white transition-300">Hakkımızda</a></li>
                        <li><a href="/Home/Contact" class="text-white-50 text-decoration-none hover-white transition-300">İletişim & Şubeler</a></li>
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Sıkça Sorulan Sorular</a></li>
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Gizlilik Politikası</a></li>
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Kullanım Koşulları</a></li>
                    </ul>
                </div>

                <!-- 4. Yapay Zeka Destek Merkezi -->
                <div class="col-lg-3 col-md-6">
                    <h6 class="text-uppercase fw-bold text-orange mb-4 tracking-wide"><i class="bi bi-robot me-2"></i> Akıllı Destek Merkezi</h6>
                    <div class="bg-white bg-opacity-10 p-4 rounded-4 border border-secondary border-opacity-25">
                        <p class="text-white-50 small mb-3 lh-sm">Sistem kullanımı, mevzuat veya modüller hakkında sorularınız mı var? Yapay zekamız 7/24 size yardımcı olmaya hazır.</p>
                        <button class="btn btn-sm btn-orange rounded-pill w-100 fw-bold shadow-sm d-flex align-items-center justify-content-center">
                            <i class="bi bi-chat-dots-fill me-2"></i> Asistanla Konuş
                        </button>
                    </div>
                    <div class="mt-4 text-md-end text-start">
                        <a href="/Admin/Index" class="text-white-50 text-decoration-none small hover-white transition-300"><i class="bi bi-shield-lock me-1"></i> Sistem Yöneticisi</a>
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
        .hover-orange:hover { color: #f97316 !important; }
        .transition-300 { transition: all 0.3s ease; }
    </style>'''

content = re.sub(r'<footer class="bg-navy text-white mt-auto py-5">.*?</footer>', new_footer, content, flags=re.DOTALL)

with open(r'GMK360.Web\Views\Shared\_Layout.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
