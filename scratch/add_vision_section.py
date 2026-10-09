import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

vision_section = '''
    </div>
</div>

<!-- ORTAK VİZYON & SİSTEM FELSEFESİ -->
<div class="container-fluid bg-light py-5 my-5">
    <div class="container">
        <div class="row g-5 align-items-center">
            
            <!-- 1. Kendi Kendini Büyüten Ağ -->
            <div class="col-lg-6">
                <div class="pe-lg-4">
                    <div class="bg-orange bg-opacity-10 rounded-circle d-inline-flex align-items-center justify-content-center mb-4" style="width: 70px; height: 70px;">
                        <i class="bi bi-share fs-2 text-orange"></i>
                    </div>
                    <h2 class="fw-bold text-navy mb-4">Stok Kodu Yok, Karmaşa Yok:<br><span class="text-orange">Ortak Açık Hesap Ağı</span></h2>
                    <p class="lead text-muted mb-4 fs-6">İster müteahhit, ister tedarikçi, ister site yöneticisi olun. Birlikte çalıştığınız firmaları, ustaları ve müşterileri tek tıkla kendi ağınıza davet edin.</p>
                    <p class="text-muted mb-4 small">
                        Karmaşık muhasebe formlarıyla uğraşmayın. Sadece <strong>serbest metinler</strong> ("100 torba çimento, şu şantiyeye gitti") kullanarak açık hesaplarınızı ve hakedişlerinizi şeffafça yönetin. Ticaret yaptığınız kişiyi ağa çektikçe, platform kendi kendini büyüten dev bir ekosisteme dönüşür.
                    </p>
                    <ul class="list-unstyled text-muted small">
                        <li class="mb-2"><i class="bi bi-check-circle-fill text-orange me-2"></i> Bakkal defteri sadeliğinde, dev ERP gücünde</li>
                        <li class="mb-2"><i class="bi bi-check-circle-fill text-orange me-2"></i> Alt taşeron ve usta davet sistemi ile şeffaf hakediş</li>
                        <li class="mb-2"><i class="bi bi-check-circle-fill text-orange me-2"></i> Serbest metinle hızlı ve yormayan veri girişi</li>
                    </ul>
                </div>
            </div>

            <!-- 2. Yanmaz, Kaybolmaz Kasa -->
            <div class="col-lg-6">
                <div class="bg-white p-5 rounded-5 shadow-sm border border-secondary border-opacity-10">
                    <div class="bg-primary bg-opacity-10 rounded-circle d-inline-flex align-items-center justify-content-center mb-4" style="width: 70px; height: 70px;">
                        <i class="bi bi-safe fs-2 text-primary"></i>
                    </div>
                    <h3 class="fw-bold text-navy mb-3">Kara Defteriniz Artık Cebinizde: <br>Yanmaz, Kaybolmaz, 7/24 Sizinle</h3>
                    <p class="text-muted mb-4 small">
                        "Defter kaybolursa yandık, dükkanda yangın çıkarsa veya masaüstü bilgisayar çökerse bütün alacaklar bitti" derdi sona erdi. Kasanız dükkanda, aklınız yolda kalmasın.
                    </p>
                    <div class="d-flex align-items-start mb-3">
                        <div class="bg-light rounded p-2 me-3"><i class="bi bi-phone text-navy fs-5"></i></div>
                        <div>
                            <h6 class="fw-bold text-navy mb-1">Mekandan Bağımsız Mobilite</h6>
                            <p class="text-muted small mb-0">Şantiyede, yolda, evde... İnternetin olduğu her an, dünyanın her yerinden cep telefonunuzla ticaretinizi yönetin.</p>
                        </div>
                    </div>
                    <div class="d-flex align-items-start">
                        <div class="bg-light rounded p-2 me-3"><i class="bi bi-shield-check text-navy fs-5"></i></div>
                        <div>
                            <h6 class="fw-bold text-navy mb-1">Banka Düzeyinde Veri Güvenliği</h6>
                            <p class="text-muted small mb-0">Telefonunuz kırılsa bile veriniz kaybolmaz. Verileriniz yanmaya, çalınmaya ve silinmeye karşı %100 bulut koruması altındadır.</p>
                        </div>
                    </div>
                </div>
            </div>

        </div>
    </div>
</div>
'''

content = re.sub(r'    </div>\s*</div>\s*<style>', vision_section + '\n<style>', content, flags=re.DOTALL)

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
