import re
with open(r'GMK360.Web\Views\Modules\Mulk.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# I will create a new section for this feature right after the "Taksitli Mahsuplaşma" section and before the "GMSİ" section.

new_section = '''    <!-- YENİ EKLENEN: Dijital Demirbaş ve Teslim Tutanağı -->
    <div class="row align-items-center mb-4 pb-4 border-bottom">
        <div class="col-lg-6 mb-3 mb-lg-0 pe-lg-5">
            <h6 class="text-success fw-bold text-uppercase mb-2">Sıfır İhtilaf, Tam Güven</h6>
            <h2 class="fw-bold text-navy mb-4">SMS Onaylı Dijital Demirbaş ve Teslim Tutanağı</h2>
            <p class="text-muted lead fs-6 mb-4">
                Evi kiracıya teslim ederken yaşanan "Kombi zaten kırıktı", "Duvarlar boyasızdı" tartışmalarını tarihe gömün.
            </p>
            <p class="text-muted small">
                Anahtarı teslim etmeden önce evdeki tüm cihazları (marka/model belirterek) ve evin fiziksel durumunu sisteme işleyin. Gerekirse fotoğraf ekleyin. Kiracınız bu durumu cep telefonuna gelen bir SMS (OTP) şifresi ile yasal olarak onaylar. Sözleşme bitiminde ve tahliye sırasında depozito kesintileri veya hasar tartışmaları %100 dijital delillerle, sorunsuz bir şekilde çözülür.
            </p>
        </div>
        <div class="col-lg-6">
            <div class="bg-light rounded-5 p-4 shadow-sm border">
                <h5 class="fw-bold text-navy mb-3"><i class="bi bi-list-check text-success me-2"></i> Mülk Teslim Tutanağı</h5>
                
                <div class="card border-0 shadow-sm text-start mb-2">
                    <div class="card-body p-3 d-flex justify-content-between align-items-center">
                        <div>
                            <div class="fw-bold text-dark small"><i class="bi bi-snow text-info me-2"></i>Klima (Arçelik 18.000 BTU)</div>
                            <div class="text-muted" style="font-size: 0.7rem;">Durum: Sorunsuz çalışıyor, kumandası teslim edildi.</div>
                        </div>
                        <span class="badge bg-success" style="font-size: 0.7rem;">Kiracı Onayladı</span>
                    </div>
                </div>

                <div class="card border-0 shadow-sm text-start mb-2">
                    <div class="card-body p-3 d-flex justify-content-between align-items-center">
                        <div>
                            <div class="fw-bold text-dark small"><i class="bi bi-paint-bucket text-danger me-2"></i>Duvar Boyası & Zemin</div>
                            <div class="text-muted" style="font-size: 0.7rem;">Durum: Yeni boyalı (Jotun Kırık Beyaz), parkelerde çizik yok.</div>
                        </div>
                        <span class="badge bg-success" style="font-size: 0.7rem;">Kiracı Onayladı</span>
                    </div>
                </div>
                
                <div class="text-center mt-3">
                    <div class="badge bg-dark bg-opacity-10 text-navy px-3 py-2 rounded-pill small">
                        <i class="bi bi-shield-lock-fill text-success me-1"></i> SMS OTP ile Yasal Güvence Altında
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- 3. Dijital Hafıza & GMSİ Asistanı -->'''

content = content.replace('<!-- 3. Dijital Hafıza & GMSİ Asistanı -->', new_section)

with open(r'GMK360.Web\Views\Modules\Mulk.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
