import re
with open(r'GMK360.Web\Views\Modules\Emlak.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

old_broker_section = '''<!-- 1. MERKEZİ YÖNETİM & BROKER VİZYONU (User's specific request) -->
    <div class="row align-items-center mb-4 pb-4 border-bottom">
        <div class="col-lg-6 mb-3 mb-lg-0 pe-lg-5">
            <h6 class="text-danger fw-bold text-uppercase mb-2">Broker & Ofis Patronlarına Özel</h6>
            <h2 class="fw-bold text-navy mb-4">Sınırsız Danışman Ekleme ve Tek Merkezden Yönetim (Acente Modeli)</h2>
            <p class="text-muted lead fs-6 mb-4">
                Büyük bir emlak ofisisiniz (Acente/Broker) ve altınızda onlarca emlak danışmanı mı çalışıyor? GMK360'a ofisiniz için bir kere üye olmanız yeterli.
            </p>
            <p class="text-muted small">
                Altınızdaki danışmanlar için ekstra sistem lisans ücretleri ödemezsiniz. Her danışman kendi alt hesabıyla (agent) sisteme girer, sadece kendi müşterisini ve portföyünü yönetir. Siz (Ofis Sahibi/Broker) ise merkezdeki "Patron Kokpiti"nden herkesin performansını, yetki belgelerini ve kapattığı satışları tek ekrandan kuş bakışı denetlersiniz. Tam kurumsal acente yönetimi.
            </p>
        </div>
        <div class="col-lg-6">
            <div class="bg-dark rounded-5 p-4 shadow-sm border text-center text-white">
                <h5 class="fw-bold text-white mb-4 border-bottom border-secondary pb-3"><i class="bi bi-diagram-3-fill text-danger me-2"></i> Ofis Yönetim Kokpiti</h5>
                
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <span class="text-white-50">Aktif Danışman (Agent)</span>
                    <span class="badge bg-danger rounded-pill px-3">12 Personel</span>
                </div>
                
                <div class="bg-white bg-opacity-10 rounded p-3 text-start mb-2">
                    <div class="d-flex justify-content-between align-items-center mb-1">
                        <span class="fw-bold small">Danışman: Hakan Yılmaz</span>
                        <span class="badge bg-success small">Online</span>
                    </div>
                    <div class="text-white-50" style="font-size:0.7rem;">Yayındaki Portföy: 8 | Bu Ayki Satış: 2</div>
                </div>
                
                <div class="bg-white bg-opacity-10 rounded p-3 text-start">
                    <div class="d-flex justify-content-between align-items-center mb-1">
                        <span class="fw-bold small">Danışman: Elif Kaya</span>
                        <span class="badge bg-success small">Online</span>
                    </div>
                    <div class="text-white-50" style="font-size:0.7rem;">Yayındaki Portföy: 15 | Bu Ayki Satış: 5</div>
                </div>
            </div>
        </div>
    </div>'''

new_broker_section = '''<!-- 1. ŞUBE & MERKEZİ YÖNETİM (Franchise/Broker Vizyonu) -->
    <div class="row align-items-center mb-4 pb-4 border-bottom">
        <div class="col-lg-6 mb-3 mb-lg-0 pe-lg-5">
            <h6 class="text-danger fw-bold text-uppercase mb-2">Franchise & Broker Vizyonu</h6>
            <h2 class="fw-bold text-navy mb-4">Tek Merkezden Tüm Şubeleri ve Danışmanları Yönetin</h2>
            <p class="text-muted lead fs-6 mb-4">
                İstanbul'da, Ankara'da veya tüm Türkiye'de şubeleri olan dev bir emlak markası mısınız? GMK360'ın hiyerarşik acente modeli tam size göre.
            </p>
            <p class="text-muted small">
                Sisteme "Genel Merkez" olarak bir kez kayıt olun. Altınıza sınırsız sayıda <strong>Şube (Franchise)</strong> tanımlayın, şubeler de kendi altlarına sınırsız sayıda <strong>Danışman (Agent)</strong> eklesin. Her danışman sadece kendi mülkünü, her şube müdürü sadece kendi ofisini görür. Siz ise (Genel Merkez/Broker) holding kokpitinden tüm Türkiye'deki şubelerin anlık ciro ve satış performanslarını kuş bakışı denetlersiniz.
            </p>
        </div>
        <div class="col-lg-6">
            <div class="bg-dark rounded-5 p-4 shadow-sm border text-center text-white">
                <h5 class="fw-bold text-white mb-4 border-bottom border-secondary pb-3"><i class="bi bi-diagram-3-fill text-danger me-2"></i> Genel Merkez Şube Kokpiti</h5>
                
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <span class="text-white-50">Toplam Şube / Danışman</span>
                    <span class="badge bg-danger rounded-pill px-3">5 Şube / 42 Danışman</span>
                </div>
                
                <div class="bg-white bg-opacity-10 rounded p-3 text-start mb-2 border-start border-3 border-danger">
                    <div class="d-flex justify-content-between align-items-center mb-1">
                        <span class="fw-bold small">Ataşehir Merkez Şubesi</span>
                        <span class="badge bg-success small">12 Danışman Aktif</span>
                    </div>
                    <div class="text-white-50" style="font-size:0.7rem;">Aylık Satış Hacmi: 45 Milyon ₺ | Yayındaki İlan: 124</div>
                </div>
                
                <div class="bg-white bg-opacity-10 rounded p-3 text-start border-start border-3 border-info">
                    <div class="d-flex justify-content-between align-items-center mb-1">
                        <span class="fw-bold small">Ankara Çankaya Şubesi</span>
                        <span class="badge bg-success small">8 Danışman Aktif</span>
                    </div>
                    <div class="text-white-50" style="font-size:0.7rem;">Aylık Satış Hacmi: 28 Milyon ₺ | Yayındaki İlan: 85</div>
                </div>
            </div>
        </div>
    </div>'''

content = content.replace(old_broker_section, new_broker_section)

with open(r'GMK360.Web\Views\Modules\Emlak.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
