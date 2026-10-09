import codecs

path = 'GMK360.Web/Views/Home/Index.cshtml'

content = '''@{
    ViewData["Title"] = "GMK360 - Tüm Yaşam Ekosisteminiz";
    Layout = "~/Views/Shared/_Layout.cshtml";
}

<!-- MASTER HERO & B2C PORTAL GİRİŞLERİ -->
<div class="bg-navy text-white text-center py-5" style="background: linear-gradient(135deg, #0f172a 0%, #1e3a8a 100%); position: relative; overflow: hidden; padding-top: 8rem !important; padding-bottom: 6rem !important;">
    <div style="position:absolute; top:-50%; left:-10%; width: 500px; height: 500px; background: rgba(249,115,22,0.1); border-radius: 50%; filter: blur(50px);"></div>
    <div style="position:absolute; bottom:-30%; right:-5%; width: 400px; height: 400px; background: rgba(59,130,246,0.2); border-radius: 50%; filter: blur(60px);"></div>

    <div class="container position-relative z-1">
        <h1 class="display-3 fw-bolder mb-4" style="letter-spacing: -2px;">
            Hayatınızı Kolaylaştıran <br/> 
            <span class="text-orange">Dijital Ekosistem</span>
        </h1>
        <p class="lead text-white-50 mb-5 mx-auto" style="max-width: 700px;">
            Aradığınız evi bulun, güvenilir usta çağırın veya mülkünüzü tek tuşla yönetin. İhtiyacınız olan her şey tek platformda.
        </p>
        
        <!-- B2C PORTAL TABS (Emlak, Usta, Günlük, Pazar Yeri) -->
        <div class="row justify-content-center">
            <div class="col-lg-10">
                <ul class="nav nav-pills nav-fill bg-white bg-opacity-10 p-2 rounded-top" id="portalTabs" role="tablist" style="border-bottom: 3px solid #f97316;">
                    <li class="nav-item" role="presentation">
                        <button class="nav-link active text-white fw-bold py-3 fs-5" data-bs-toggle="pill" data-bs-target="#emlak" type="button"><i class="ph-fill ph-house me-2"></i>Emlak & Arsa</button>
                    </li>
                    <li class="nav-item" role="presentation">
                        <button class="nav-link text-white fw-bold py-3 fs-5" data-bs-toggle="pill" data-bs-target="#usta" type="button"><i class="ph-fill ph-wrench me-2"></i>Usta & Esnaf</button>
                    </li>
                    <li class="nav-item" role="presentation">
                        <button class="nav-link text-white fw-bold py-3 fs-5" data-bs-toggle="pill" data-bs-target="#gunluk" type="button"><i class="ph-fill ph-calendar-check me-2"></i>Günlük Kiralık</button>
                    </li>
                    <li class="nav-item" role="presentation">
                        <button class="nav-link text-white fw-bold py-3 fs-5" data-bs-toggle="pill" data-bs-target="#pazar" type="button"><i class="ph-fill ph-storefront me-2"></i>Pazar Yeri (Malzeme)</button>
                    </li>
                </ul>
                
                <div class="tab-content bg-white p-4 rounded-bottom shadow-lg" id="portalTabsContent">
                    <!-- Emlak Arama -->
                    <div class="tab-pane fade show active" id="emlak" role="tabpanel">
                        <div class="d-flex align-items-center">
                            <select class="form-select border-0 fw-bold text-dark fs-5 shadow-none w-auto" style="background-color: #f8fafc;">
                                <option>Satılık</option>
                                <option>Kiralık</option>
                            </select>
                            <div class="vr mx-3 text-muted"></div>
                            <input type="text" class="form-control border-0 shadow-none fs-5" placeholder="İl, ilçe, mahalle veya proje adı yazın...">
                            <a href="/Dashboard/Corporate" class="btn btn-orange btn-lg px-5 fw-bold rounded-pill"><i class="bi bi-search me-2"></i> İlanları Gör</a>
                        </div>
                    </div>
                    <!-- Usta Arama -->
                    <div class="tab-pane fade" id="usta" role="tabpanel">
                        <div class="d-flex align-items-center">
                            <select class="form-select border-0 fw-bold text-dark fs-5 shadow-none w-auto" style="background-color: #f8fafc;">
                                <option>Tesisatçı</option>
                                <option>Elektrikçi</option>
                                <option>Boyacı</option>
                                <option>Temizlik Şirketi</option>
                            </select>
                            <div class="vr mx-3 text-muted"></div>
                            <input type="text" class="form-control border-0 shadow-none fs-5" placeholder="Bulunduğunuz ilçeyi yazın (Örn: Kadıköy)...">
                            <a href="/Dashboard/ServiceProvider" class="btn btn-primary btn-lg px-5 fw-bold rounded-pill"><i class="bi bi-search me-2"></i> Usta Bul</a>
                        </div>
                    </div>
                    <!-- Günlük Kiralık Arama -->
                    <div class="tab-pane fade" id="gunluk" role="tabpanel">
                        <div class="d-flex align-items-center">
                            <input type="text" class="form-control border-0 shadow-none fs-5 bg-light me-3" placeholder="Nereye Gideceksiniz?">
                            <input type="date" class="form-control border-0 shadow-none fs-5 bg-light me-3">
                            <a href="/Dashboard/Corporate" class="btn btn-info text-white btn-lg px-5 fw-bold rounded-pill w-50"><i class="bi bi-calendar3 me-2"></i> Müsaitlik Ara</a>
                        </div>
                    </div>
                    <!-- Pazar Yeri -->
                    <div class="tab-pane fade" id="pazar" role="tabpanel">
                        <div class="d-flex align-items-center">
                            <input type="text" class="form-control border-0 shadow-none fs-5 bg-light" placeholder="İnşaat malzemesi, mobilya veya yapı market ürünü arayın...">
                            <a href="/B2BPurchasing/Index" class="btn btn-success btn-lg px-5 fw-bold rounded-pill ms-3 w-50"><i class="bi bi-shop me-2"></i> Ürün Ara</a>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- DETAYLI MODÜLLER (B2B & SAAS) SECTİON -->
<div class="container py-5 mt-5">
    <div class="text-center mb-5">
        <h6 class="text-orange fw-bold text-uppercase tracking-wide">Yönetim Panellerimiz</h6>
        <h2 class="fw-bold text-navy display-6">Profesyoneller İçin Ayrıcalıklı Modüller</h2>
        <p class="text-muted lead mx-auto mt-3" style="max-width: 800px;">Her sektörün ihtiyacı farklıdır. Sizi gereksiz menülerle yormuyoruz; sadece size özel tasarlanmış, hayatınızı kolaylaştıran çalışma alanınıza giriş yapın.</p>
    </div>

    <div class="row g-4">
        <!-- Bina Yönetimi -->
        <div class="col-md-4">
            <div class="card h-100 border-0 shadow-sm rounded-4 p-4 hover-lift">
                <div class="d-inline-flex align-items-center justify-content-center bg-primary bg-opacity-10 text-primary rounded-circle mb-4" style="width: 70px; height: 70px;">
                    <i class="ph-fill ph-building-apartment" style="font-size: 32px;"></i>
                </div>
                <h4 class="fw-bold text-navy mb-2">Bina Yönetimi</h4>
                <p class="text-muted mb-4">Tek bir apartmanın aidat, asansör bakımı, karar defteri ve gelir-gider süreçlerini dijitalleştirin.</p>
                <a href="/BuildingManager/Detail/1" class="btn btn-outline-primary fw-bold mt-auto rounded-pill">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
            </div>
        </div>

        <!-- Site Yönetimi -->
        <div class="col-md-4">
            <div class="card h-100 border-0 shadow-sm rounded-4 p-4 hover-lift">
                <div class="d-inline-flex align-items-center justify-content-center bg-info bg-opacity-10 text-info rounded-circle mb-4" style="width: 70px; height: 70px;">
                    <i class="ph-fill ph-buildings" style="font-size: 32px;"></i>
                </div>
                <h4 class="fw-bold text-navy mb-2">Toplu Konut & Site</h4>
                <p class="text-muted mb-4">Çoklu bloklar, havuz/peyzaj giderleri, güvenlik personeli ve toplu anket/oylama ile devasa siteleri kolayca yönetin.</p>
                <a href="/BuildingManager/Detail/1" class="btn btn-outline-info fw-bold mt-auto rounded-pill">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
            </div>
        </div>

        <!-- Bireysel Mülk Takibi -->
        <div class="col-md-4">
            <div class="card h-100 border-0 shadow-sm rounded-4 p-4 hover-lift">
                <div class="d-inline-flex align-items-center justify-content-center bg-success bg-opacity-10 text-success rounded-circle mb-4" style="width: 70px; height: 70px;">
                    <i class="ph-fill ph-key" style="font-size: 32px;"></i>
                </div>
                <h4 class="fw-bold text-navy mb-2">Bireysel Mülk Takibi</h4>
                <p class="text-muted mb-4">Tapularınız, DASK yenileme tarihleri, emlak vergisi taksitleri ve değer artış analizleriniz tek ekranda.</p>
                <a href="/Dashboard/PropertyOwner" class="btn btn-outline-success fw-bold mt-auto rounded-pill">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
            </div>
        </div>

        <!-- Kiracı Takibi -->
        <div class="col-md-4">
            <div class="card h-100 border-0 shadow-sm rounded-4 p-4 hover-lift">
                <div class="d-inline-flex align-items-center justify-content-center bg-warning bg-opacity-10 text-warning rounded-circle mb-4" style="width: 70px; height: 70px;">
                    <i class="ph-fill ph-wallet" style="font-size: 32px;"></i>
                </div>
                <h4 class="fw-bold text-navy mb-2">Kiracı & Kontrat Yönetimi</h4>
                <p class="text-muted mb-4">Geciken kiralar için otomatik SMS, TÜFE oranlı zam hesaplama ve GMSİ kira gelir beyannamesi asistanı.</p>
                <a href="/Dashboard/PropertyOwner" class="btn btn-outline-warning text-dark fw-bold mt-auto rounded-pill">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
            </div>
        </div>

        <!-- İnşaat & Müteahhit -->
        <div class="col-md-4">
            <div class="card h-100 border-0 shadow-sm rounded-4 p-4 hover-lift">
                <div class="d-inline-flex align-items-center justify-content-center bg-danger bg-opacity-10 text-danger rounded-circle mb-4" style="width: 70px; height: 70px;">
                    <i class="ph-fill ph-crane" style="font-size: 32px;"></i>
                </div>
                <h4 class="fw-bold text-navy mb-2">İnşaat & ERP Modülü</h4>
                <p class="text-muted mb-4">Şantiye 8 faz yönetimi, taşeron hakedişleri, taşeron yemek/puantaj kesintileri ve yapay zeka destekli maliyet analizi.</p>
                <a href="/ConstructionProject/Index" class="btn btn-outline-danger fw-bold mt-auto rounded-pill">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
            </div>
        </div>

        <!-- Günlük Kiralık -->
        <div class="col-md-4">
            <div class="card h-100 border-0 shadow-sm rounded-4 p-4 hover-lift">
                <div class="d-inline-flex align-items-center justify-content-center bg-secondary bg-opacity-10 text-secondary rounded-circle mb-4" style="width: 70px; height: 70px;">
                    <i class="ph-fill ph-calendar-plus" style="font-size: 32px;"></i>
                </div>
                <h4 class="fw-bold text-navy mb-2">Günlük Kiralık (Airbnb)</h4>
                <p class="text-muted mb-4">Check-in / Check-out takvimi, temizlik ekibi yönlendirme ve dinamik fiyatlama ile kısa dönem kiralamalarınızı uçurun.</p>
                <a href="/Dashboard/Corporate" class="btn btn-outline-secondary fw-bold mt-auto rounded-pill">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
            </div>
        </div>
    </div>
</div>

<style>
    .hover-lift { transition: transform 0.3s ease, box-shadow 0.3s ease; border: 1px solid transparent; }
    .hover-lift:hover { transform: translateY(-10px); box-shadow: 0 1rem 3rem rgba(0,0,0,.15)!important; border-color: #f97316; }
    .nav-pills .nav-link { color: #fff; transition: 0.3s; }
    .nav-pills .nav-link.active, .nav-pills .show>.nav-link { background-color: #f97316; color: #fff !important; }
</style>
'''

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print("Updated Home Index based on user's new vision!")