import re
with open(r'GMK360.Web\Views\Modules\Tedarik.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

old_escrow = '''    <!-- 3. B2B ve Escrow Güvenli Ödeme -->
    <div class="row align-items-center mb-4 pb-4 border-bottom">
        <div class="col-lg-6 mb-3 mb-lg-0 pe-lg-5">
            <h6 class="text-success fw-bold text-uppercase mb-2">Finansal Güvenlik (Escrow)</h6>
            <h2 class="fw-bold text-navy mb-4">Milyonluk Siparişlerde %100 Finansal Güven</h2>
            <p class="text-muted lead fs-6 mb-4">
                İnşaat sektörünün en büyük sorunu olan "Malı gönderdim, çeki yazıldı veya ödeme gelmedi" derdine son veriyoruz.
            </p>
            <ul class="list-unstyled text-muted small mb-0">
                <li class="mb-3 d-flex"><i class="bi bi-shield-lock-fill text-success me-3 fs-5 mt-1"></i> <div><strong>Güvenli Ödeme Havuzu (Escrow):</strong> Alıcı parayı sisteme kilitler. Malzemeler şantiyeye sorunsuz teslim edildiğinde (veya e-irsaliye onaylandığında) tutar anında tedarikçinin hesabına geçer. Açık hesap riskini sıfırlayın.</div></li>
                <li class="mb-0 d-flex"><i class="bi bi-tags-fill text-success me-3 fs-5 mt-1"></i> <div><strong>Çift Katmanlı Fiyatlama (B2B/B2C):</strong> Mülk sahibine perakende fiyattan (B2C) satarken; sisteme kayıtlı "Pro" yetkili şantiyelere toptan iskontolu (B2B) fiyatlar tanımlayın. Sektörel ticari dengeleri bozmayın.</div></li>
            </ul>
        </div>
        <div class="col-lg-6 text-center">
            <div class="bg-light rounded-5 p-5 border">
                <i class="bi bi-bank text-success mb-3" style="font-size: 4rem;"></i>
                <h5 class="fw-bold text-navy">Ödeme Havuzda Bekliyor</h5>
                <h3 class="fw-bolder text-dark mb-1">₺ 1.250.000</h3>
                <p class="text-muted small mb-4">Sipariş: 100 Ton İnşaat Demiri</p>
                <button class="btn btn-success rounded-pill fw-bold btn-sm"><i class="bi bi-truck me-2"></i> Teslimat Onaylandığında Hesabınıza Geçer</button>
            </div>
        </div>
    </div>'''

new_b2b = '''    <!-- 3. Çift Katmanlı Fiyatlama ve Dijital İhale -->
    <div class="row align-items-center mb-4 pb-4 border-bottom">
        <div class="col-lg-6 mb-3 mb-lg-0 pe-lg-5">
            <h6 class="text-success fw-bold text-uppercase mb-2">Toptan & Perakende Yönetimi</h6>
            <h2 class="fw-bold text-navy mb-4">Çift Katmanlı Fiyatlama ve <br>Dijital İhale (RFP) Tahtası</h2>
            <p class="text-muted lead fs-6 mb-4">
                Sektörel ticari dengelerinizi bozmayın. Alıcı türüne göre fiyat politikalarınızı dinamik olarak yönetin.
            </p>
            <ul class="list-unstyled text-muted small mb-0">
                <li class="mb-3 d-flex"><i class="bi bi-tags-fill text-success me-3 fs-5 mt-1"></i> <div><strong>Toptana Ayrı, Perakendeye Ayrı:</strong> Mülk veya pansiyon sahiplerine perakende (B2C) fiyattan satarken; sisteme kayıtlı müteahhitlere ve büyük şantiyelere otomatik olarak "Pro İskontolu" (B2B) toptan fiyatlarınızı gösterin.</div></li>
                <li class="mb-0 d-flex"><i class="bi bi-megaphone-fill text-success me-3 fs-5 mt-1"></i> <div><strong>Dev Projelerin Tedarikçisi Olun:</strong> Büyük inşaat firmaları 500 metrekare seramik alacağı zaman standart sepet kullanmaz. Sistemin "Teklif İste (RFP)" tahtasına düşen dev hacimli taleplere özel teklifinizi verin, ihaleyi siz kazanın.</div></li>
            </ul>
        </div>
        <div class="col-lg-6">
            <div class="bg-light rounded-5 p-4 border text-center shadow-sm">
                <h5 class="fw-bold text-navy mb-4 border-bottom pb-3"><i class="bi bi-percent text-success me-2"></i> Dinamik İskonto & Teklif Paneli</h5>
                
                <div class="card border-0 shadow-sm text-start mb-3 border-start border-4 border-info">
                    <div class="card-body p-3 d-flex justify-content-between align-items-center">
                        <div>
                            <div class="fw-bold text-dark small">Bireysel Müşteri (Ev Sahibi)</div>
                            <div class="text-muted" style="font-size: 0.75rem;">Yüzey Temizleyici (5 Kg)</div>
                        </div>
                        <span class="badge bg-navy text-white px-3 py-2">₺ 140,00</span>
                    </div>
                </div>

                <div class="card border border-success shadow-sm text-start bg-success bg-opacity-10">
                    <div class="card-body p-3 d-flex justify-content-between align-items-center">
                        <div>
                            <div class="fw-bold text-success small"><i class="bi bi-building-check me-1"></i> Pro Şantiye / AVM (B2B Müşteri)</div>
                            <div class="text-muted" style="font-size: 0.75rem;">Yüzey Temizleyici (5 Kg) - %25 İskonto</div>
                        </div>
                        <span class="badge bg-success px-3 py-2">₺ 105,00</span>
                    </div>
                </div>
                
                <div class="text-center mt-3">
                    <div class="badge bg-dark bg-opacity-10 text-navy px-3 py-2 rounded-pill small">
                        Sistem alıcının yetki sınıfını otomatik tanır ve doğru fiyatı gösterir.
                    </div>
                </div>
            </div>
        </div>
    </div>'''

content = content.replace(old_escrow, new_b2b)
content = content.replace('Milyonluk siparişlerde %100 tahsilat garantisiyle siz verin', 'Dev projelerin malzemesini dijital ihale sistemiyle siz verin')

with open(r'GMK360.Web\Views\Modules\Tedarik.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
