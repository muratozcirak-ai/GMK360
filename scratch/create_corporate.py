import codecs

path = 'GMK360.Web/Views/Dashboard/Corporate.cshtml'
content = '''@{
    ViewData["Title"] = "Emlak ve İlan Yönetimi";
    Layout = "~/Views/Shared/_DashboardLayout.cshtml";
}

<div class="container-fluid py-4 px-md-4">
    <!-- Üst Başlık -->
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h2 class="fw-bold" style="background: -webkit-linear-gradient(45deg, #0f2027, #203a43, #2c5364); -webkit-background-clip: text; -webkit-text-fill-color: transparent;">
                <i class="bi bi-buildings-fill me-2"></i> Kurumsal Emlak & İlan Masası
            </h2>
            <p class="text-muted mb-0">Portföyünüzü yönetin, satılık/kiralık ilanlarınızı eşleştirin ve günlük kiralık takviminizi izleyin.</p>
        </div>
        <div>
            <button class="btn btn-warning shadow-sm rounded-pill px-4 fw-bold me-2"><i class="bi bi-calendar-check-fill me-1"></i> Günlük Kiralık Rezervasyon</button>
            <a href="/Property/Create" class="btn btn-dark rounded-pill shadow-sm px-4 fw-bold">
                <i class="bi bi-plus-lg me-1"></i> Yeni Portföy Ekle
            </a>
        </div>
    </div>

    <!-- 4 KARTLI ÖZET -->
    <div class="row g-3 mb-4">
        <div class="col-md-3">
            <div class="card bg-white shadow-sm border-0 rounded-4 h-100 border-start border-4 border-primary">
                <div class="card-body p-4">
                    <h6 class="text-muted text-uppercase fw-semibold mb-2">Aktif Portföy</h6>
                    <h3 class="mb-0 text-dark fw-bold">14 Adet</h3>
                    <small class="text-success"><i class="bi bi-arrow-up-short"></i> Geçen haftadan +2</small>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card bg-white shadow-sm border-0 rounded-4 h-100 border-start border-4 border-info">
                <div class="card-body p-4">
                    <h6 class="text-muted text-uppercase fw-semibold mb-2">Günlük Kiralık (Boş)</h6>
                    <h3 class="mb-0 text-dark fw-bold">3 Daire</h3>
                    <small class="text-info">Bugün giriş beklenen: 1</small>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card bg-white shadow-sm border-0 rounded-4 h-100 border-start border-4 border-success">
                <div class="card-body p-4">
                    <h6 class="text-muted text-uppercase fw-semibold mb-2">Eşleşen Müşteri Talebi</h6>
                    <h3 class="mb-0 text-dark fw-bold">8 Kişi</h3>
                    <small class="text-muted">Akıllı eşleşme motoru buldu</small>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card bg-white shadow-sm border-0 rounded-4 h-100 border-start border-4 border-warning">
                <div class="card-body p-4">
                    <h6 class="text-muted text-uppercase fw-semibold mb-2">Bu Ayki Kiralama / Satış</h6>
                    <h3 class="mb-0 text-dark fw-bold">3 İşlem</h3>
                    <small class="text-warning fw-bold">55.000 TL Komisyon Hedefi</small>
                </div>
            </div>
        </div>
    </div>

    <!-- SEKMELER (TABS) -->
    <ul class="nav nav-pills mb-4 bg-white p-2 rounded-pill shadow-sm d-inline-flex" id="emlak-tab" role="tablist">
        <li class="nav-item" role="presentation">
            <button class="nav-link active rounded-pill px-4 fw-bold" data-bs-toggle="pill" data-bs-target="#pills-portfoy" type="button" role="tab">Portföy (Satılık/Kiralık)</button>
        </li>
        <li class="nav-item" role="presentation">
            <button class="nav-link rounded-pill px-4 fw-bold" data-bs-toggle="pill" data-bs-target="#pills-gunluk" type="button" role="tab">Günlük Kiralık (AirBnb)</button>
        </li>
        <li class="nav-item" role="presentation">
            <button class="nav-link rounded-pill px-4 fw-bold" data-bs-toggle="pill" data-bs-target="#pills-musteri" type="button" role="tab">Alıcı CRM & Eşleştirme</button>
        </li>
    </ul>

    <!-- SEKME İÇERİKLERİ -->
    <div class="tab-content" id="emlak-tabContent">
        
        <!-- 1. PORTFÖY -->
        <div class="tab-pane fade show active" id="pills-portfoy" role="tabpanel">
            <div class="row">
                <div class="col-md-4 mb-4">
                    <div class="card border-0 shadow-sm rounded-4 h-100 overflow-hidden hover-lift">
                        <img src="https://images.unsplash.com/photo-1512917774080-9991f1c4c750?auto=format&fit=crop&q=80&w=400&h=200" class="card-img-top" style="height: 200px; object-fit: cover;" alt="...">
                        <div class="card-body p-4 position-relative">
                            <span class="badge bg-danger position-absolute top-0 end-0 mt-3 me-3 fs-6">Satılık</span>
                            <h5 class="fw-bold text-dark">Beyaz Konak Apt. Kat:4</h5>
                            <p class="text-muted small mb-2"><i class="bi bi-geo-alt-fill text-danger me-1"></i> Kadıköy, İstanbul</p>
                            <p class="fw-bold fs-4 text-primary mb-3">12.500.000 ₺</p>
                            <div class="d-flex justify-content-between text-muted small border-top pt-3">
                                <span><i class="bi bi-door-open"></i> 3+1</span>
                                <span><i class="bi bi-aspect-ratio"></i> 145 m²</span>
                                <span><i class="bi bi-building"></i> Sıfır Bina</span>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-md-4 mb-4">
                    <div class="card border-0 shadow-sm rounded-4 h-100 overflow-hidden hover-lift">
                        <img src="https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&q=80&w=400&h=200" class="card-img-top" style="height: 200px; object-fit: cover;" alt="...">
                        <div class="card-body p-4 position-relative">
                            <span class="badge bg-success position-absolute top-0 end-0 mt-3 me-3 fs-6">Kiralık</span>
                            <h5 class="fw-bold text-dark">Lüks Rezidans Manzaralı</h5>
                            <p class="text-muted small mb-2"><i class="bi bi-geo-alt-fill text-danger me-1"></i> Ataşehir, İstanbul</p>
                            <p class="fw-bold fs-4 text-primary mb-3">35.000 ₺ <span class="fs-6 text-muted fw-normal">/ay</span></p>
                            <div class="d-flex justify-content-between text-muted small border-top pt-3">
                                <span><i class="bi bi-door-open"></i> 2+1</span>
                                <span><i class="bi bi-aspect-ratio"></i> 90 m²</span>
                                <span><i class="bi bi-cup-hot"></i> Eşyalı</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- 2. GÜNLÜK KİRALIK (AIRBNB MANTIKLI) -->
        <div class="tab-pane fade" id="pills-gunluk" role="tabpanel">
            <div class="card border-0 shadow-sm rounded-4">
                <div class="card-body p-4">
                    <h5 class="fw-bold text-dark mb-4"><i class="bi bi-calendar3-range text-warning me-2"></i>Günlük Kiralık & Rezervasyon Takvimi</h5>
                    
                    <!-- Takvim Mockup -->
                    <div class="table-responsive">
                        <table class="table table-bordered text-center align-middle">
                            <thead class="table-light">
                                <tr>
                                    <th class="text-start">Daire / Tesis</th>
                                    <th>Pzt (12 Eki)</th>
                                    <th>Sal (13 Eki)</th>
                                    <th>Çar (14 Eki)</th>
                                    <th>Per (15 Eki)</th>
                                    <th>Cum (16 Eki)</th>
                                    <th>Cmt (17 Eki)</th>
                                    <th>Paz (18 Eki)</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td class="text-start fw-bold">Marina Suit 1A</td>
                                    <td colspan="3" class="bg-primary text-white rounded position-relative">
                                        <small class="fw-bold">Ahmet K. (3 Gece)</small>
                                        <br><small class="opacity-75">Ödendi</small>
                                    </td>
                                    <td class="bg-warning text-dark"><small class="fw-bold">Temizlik</small></td>
                                    <td colspan="3" class="bg-success text-white">
                                        <small class="fw-bold">Yabancı Turist (Booking)</small>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="text-start fw-bold">Moda Çatı Katı</td>
                                    <td></td>
                                    <td></td>
                                    <td colspan="2" class="bg-primary text-white">
                                        <small class="fw-bold">Ayşe Y.</small>
                                    </td>
                                    <td class="bg-warning text-dark"><small class="fw-bold">Temizlik</small></td>
                                    <td></td>
                                    <td></td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
        
        <!-- 3. MÜŞTERİ (ALICI) CRM EŞLEŞTİRME -->
        <div class="tab-pane fade" id="pills-musteri" role="tabpanel">
            <div class="card border-0 shadow-sm rounded-4">
                <div class="card-body p-4">
                    <h5 class="fw-bold text-dark mb-4"><i class="bi bi-robot text-info me-2"></i>Yapay Zeka Müşteri Eşleştirme</h5>
                    <div class="alert alert-info border-info shadow-sm">
                        <i class="bi bi-magic me-2"></i>Sistem, portföyünüze yeni eklenen <strong>Beyaz Konak Apt. (Satılık 3+1)</strong> için CRM havuzunuzdaki geçmiş alıcı taleplerini taradı ve <strong>3 potansiyel eşleşme</strong> buldu!
                    </div>
                    
                    <div class="list-group">
                        <a href="#" class="list-group-item list-group-item-action d-flex justify-content-between align-items-center p-3">
                            <div>
                                <h6 class="fw-bold mb-1">Mehmet Bey <span class="badge bg-success ms-2">%95 Eşleşme</span></h6>
                                <small class="text-muted">Arama Kriteri: Kadıköy, 3+1, Bütçe: 10M - 13M TL</small>
                            </div>
                            <button class="btn btn-sm btn-outline-primary"><i class="bi bi-telephone me-1"></i> Hemen Ara</button>
                        </a>
                        <a href="#" class="list-group-item list-group-item-action d-flex justify-content-between align-items-center p-3">
                            <div>
                                <h6 class="fw-bold mb-1">Elif Hanım <span class="badge bg-warning text-dark ms-2">%80 Eşleşme</span></h6>
                                <small class="text-muted">Arama Kriteri: Anadolu Yakası, Yeni Bina, Bütçe: Sınırsız</small>
                            </div>
                            <button class="btn btn-sm btn-outline-primary"><i class="bi bi-telephone me-1"></i> Hemen Ara</button>
                        </a>
                    </div>
                </div>
            </div>
        </div>

    </div>
</div>
'''

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print("Corporate Emlak Dashboard created!")