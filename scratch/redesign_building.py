import codecs

path = 'GMK360.Web/Views/BuildingManager/Detail.cshtml'

content = '''@model GMK360.Core.Entities.Building
@{
    ViewData["Title"] = Model.Name + " Yönetim Paneli";
    Layout = "~/Views/Shared/_FacilityLayout.cshtml";

    var totalUnpaid = Model.Units?.SelectMany(u => u.Debts).Where(d => !d.IsPaid).Sum(d => d.Amount) ?? 0;
    var totalPaid = Model.Units?.SelectMany(u => u.Debts).Where(d => d.IsPaid).Sum(d => d.Amount) ?? 0;
}

<div class="container-fluid py-4 px-md-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h3 class="fw-bold text-dark mb-1"><i class="bi bi-building-fill-gear text-primary me-2"></i> @Model.Name Yönetim Paneli</h3>
            <p class="text-muted mb-0">Bina ID: #BLD-@Model.Id | @Model.City, @Model.District</p>
        </div>
        <div>
            <button class="btn btn-warning shadow-sm rounded-pill fw-bold text-dark px-4 me-2" data-bs-toggle="modal" data-bs-target="#newMeetingModal">
                <i class="bi bi-people-fill me-1"></i> Yeni Toplantı / Karar
            </button>
            <button class="btn btn-primary shadow-sm rounded-pill px-4" data-bs-toggle="modal" data-bs-target="#expenseModal">
                <i class="bi bi-plus-circle me-1"></i> Gider/Aidat İşle
            </button>
        </div>
    </div>

    <!-- 4 KARTLI ÖZET -->
    <div class="row g-3 mb-4">
        <div class="col-md-3">
            <div class="card bg-white shadow-sm border-0 rounded-4 h-100">
                <div class="card-body">
                    <h6 class="text-muted text-uppercase fw-semibold mb-2">Kasada Kalan</h6>
                    <h3 class="mb-0 text-success fw-bold">14.500,00 ₺</h3>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card bg-white shadow-sm border-0 rounded-4 h-100">
                <div class="card-body">
                    <h6 class="text-muted text-uppercase fw-semibold mb-2">Toplanan (Bu Ay)</h6>
                    <h3 class="mb-0 text-primary fw-bold">@totalPaid.ToString("N2") ₺</h3>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card bg-white shadow-sm border-0 rounded-4 h-100">
                <div class="card-body">
                    <h6 class="text-muted text-uppercase fw-semibold mb-2">Bekleyen Alacak (Geciken)</h6>
                    <h3 class="mb-0 text-danger fw-bold">@totalUnpaid.ToString("N2") ₺</h3>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card bg-white shadow-sm border-0 rounded-4 h-100">
                <div class="card-body">
                    <h6 class="text-muted text-uppercase fw-semibold mb-2">Aktif İş Emirleri / Bakım</h6>
                    <h3 class="mb-0 text-dark fw-bold">2 Adet</h3>
                </div>
            </div>
        </div>
    </div>

    <!-- SEKMELER (TABS) -->
    <ul class="nav nav-pills mb-4 bg-white p-2 rounded-pill shadow-sm" id="pills-tab" role="tablist">
        <li class="nav-item" role="presentation">
            <button class="nav-link active rounded-pill px-4 fw-bold" id="pills-karar-tab" data-bs-toggle="pill" data-bs-target="#pills-karar" type="button" role="tab">Akıllı Karar Defteri & Toplantı</button>
        </li>
        <li class="nav-item" role="presentation">
            <button class="nav-link rounded-pill px-4 fw-bold" id="pills-finans-tab" data-bs-toggle="pill" data-bs-target="#pills-finans" type="button" role="tab">Fatura & Aidat Takibi</button>
        </li>
        <li class="nav-item" role="presentation">
            <button class="nav-link rounded-pill px-4 fw-bold" id="pills-sakinler-tab" data-bs-toggle="pill" data-bs-target="#pills-sakinler" type="button" role="tab">Daire Sakinleri & Kiracılar</button>
        </li>
        <li class="nav-item" role="presentation">
            <button class="nav-link rounded-pill px-4 fw-bold" id="pills-usta-tab" data-bs-toggle="pill" data-bs-target="#pills-usta" type="button" role="tab">Usta Bul & İş Takibi</button>
        </li>
    </ul>

    <!-- SEKME İÇERİKLERİ -->
    <div class="tab-content" id="pills-tabContent">
        
        <!-- 1. AKILLI KARAR DEFTERİ -->
        <div class="tab-pane fade show active" id="pills-karar" role="tabpanel">
            <div class="row">
                <div class="col-md-8">
                    <!-- Çoklu PIN Onaylı Çatı Onarımı Toplantısı -->
                    <div class="card border-warning shadow-sm rounded-4 mb-4">
                        <div class="card-header bg-warning bg-opacity-10 border-bottom border-warning p-3">
                            <div class="d-flex justify-content-between align-items-center">
                                <h5 class="fw-bold text-dark mb-0"><i class="bi bi-hammer text-warning me-2"></i>Olağanüstü Gündem: Çatı İzolasyon ve Akıntı Tamiri</h5>
                                <span class="badge bg-danger rounded-pill px-3">Katılım Oranı: 18/20</span>
                            </div>
                        </div>
                        <div class="card-body p-4">
                            <p class="text-muted fw-bold">Hedef Kitle: SADECE MÜLK SAHİPLERİ (Kiracıları İlgilendirmez)</p>
                            <p class="text-dark">Kış aylarında yaşanan çatı akıntıları nedeniyle, izolasyon yenilenmesi gerekmektedir. Esnaf havuzundan alınan en uygun teklif <strong>40.000 TL</strong>'dir. Daire başı düşen pay <strong>2.000 TL</strong> olacaktır. Bu onarım "Demirbaş Gideri" olduğu için sadece ev sahiplerine fatura edilecek ve yıl sonu kira gelir beyannamesinden düşülebilecektir.</p>
                            
                            <div class="bg-light p-3 rounded-3 mt-3 border">
                                <h6 class="fw-bold mb-3">Çoklu PIN Onay Durumu (Karar Defteri)</h6>
                                <div class="progress mb-2" style="height: 25px;">
                                    <div class="progress-bar bg-success fw-bold" role="progressbar" style="width: 75%;" aria-valuenow="75" aria-valuemin="0" aria-valuemax="100">ONAY (15 Kişi)</div>
                                    <div class="progress-bar bg-danger fw-bold" role="progressbar" style="width: 15%;" aria-valuenow="15" aria-valuemin="0" aria-valuemax="100">RET (3 Kişi)</div>
                                </div>
                                <small class="text-muted">Salt çoğunluk sağlanmıştır. Karar oy çokluğuyla kabul edilmiştir.</small>
                            </div>
                            
                            <div class="mt-4 d-flex justify-content-end">
                                <button class="btn btn-outline-success fw-bold me-2"><i class="bi bi-file-earmark-pdf me-1"></i> Karar Defteri PDF İndir</button>
                                <button class="btn btn-primary fw-bold"><i class="bi bi-tools me-1"></i> İşi Ustaya Ver (İş Emri)</button>
                            </div>
                        </div>
                    </div>

                    <!-- Kiracıları İlgilendiren Sıradan Anket -->
                    <div class="card border-0 shadow-sm rounded-4">
                        <div class="card-body p-4">
                            <h6 class="fw-bold text-dark"><i class="bi bi-question-circle-fill text-info me-2"></i>Anket: Açık Otopark Çizgilerinin Yenilenmesi</h6>
                            <p class="text-muted small">Hedef Kitle: TÜM SAKİNLER (Kiracılar ve Oturan Ev Sahipleri)</p>
                            <div class="d-flex align-items-center mt-3">
                                <button class="btn btn-outline-primary btn-sm px-4 rounded-pill me-2">Evet, Yapılsın</button>
                                <button class="btn btn-outline-secondary btn-sm px-4 rounded-pill">Gerek Yok</button>
                                <span class="ms-auto text-muted small"><i class="bi bi-clock me-1"></i> 2 gün kaldı</span>
                            </div>
                        </div>
                    </div>
                </div>
                
                <div class="col-md-4">
                    <div class="card border-0 shadow-sm rounded-4 mb-4 bg-light">
                        <div class="card-body">
                            <h6 class="fw-bold mb-3"><i class="bi bi-bell-fill text-warning me-2"></i>Son Bildirimler</h6>
                            <ul class="list-group list-group-flush bg-transparent">
                                <li class="list-group-item bg-transparent px-0 border-bottom">
                                    <small class="text-muted d-block">10 dakika önce</small>
                                    <span class="fw-semibold">Daire 14 (Kiracı) aidat ödemesini yaptı.</span>
                                </li>
                                <li class="list-group-item bg-transparent px-0 border-bottom">
                                    <small class="text-muted d-block">2 saat önce</small>
                                    <span class="fw-semibold">Çatı tamiri için 3 yeni usta teklifi geldi.</span>
                                </li>
                                <li class="list-group-item bg-transparent px-0 border-0">
                                    <small class="text-muted d-block">Dün</small>
                                    <span class="fw-semibold">Asansör bakım faturası sisteme yüklendi.</span>
                                </li>
                            </ul>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- 2. FİNANS VE AİDAT -->
        <div class="tab-pane fade" id="pills-finans" role="tabpanel">
            <div class="card border-0 shadow-sm rounded-4">
                <div class="card-header bg-white border-bottom p-4 d-flex justify-content-between align-items-center">
                    <h5 class="fw-bold mb-0">Borç ve Fatura Tablosu</h5>
                    <button class="btn btn-sm btn-outline-primary fw-bold"><i class="bi bi-receipt me-1"></i> Yeni Fatura Gir</button>
                </div>
                <div class="card-body p-0">
                    <div class="table-responsive">
                        <table class="table table-hover align-middle mb-0">
                            <thead class="table-light">
                                <tr>
                                    <th class="ps-4">Tarih</th>
                                    <th>Daire / Sakin</th>
                                    <th>Açıklama (Tür)</th>
                                    <th>Tutar</th>
                                    <th>Durum</th>
                                    <th class="pe-4 text-end">İşlem</th>
                                </tr>
                            </thead>
                            <tbody>
                                @if (Model.Units != null) {
                                    foreach (var unit in Model.Units)
                                    {
                                        foreach(var debt in unit.Debts)
                                        {
                                            <tr>
                                                <td class="ps-4">@debt.BuildingExpense.Date.ToString("dd.MM.yyyy")</td>
                                                <td><span class="fw-bold">Daire @unit.UnitNumber</span><br><small class="text-muted">@(string.IsNullOrEmpty(unit.TenantName) ? unit.OwnerName : unit.TenantName)</small></td>
                                                <td>
                                                    @debt.BuildingExpense.Description
                                                    @if(debt.BuildingExpense.Description.Contains("Aidat")) { <span class="badge bg-light text-dark border ms-2">Sakin</span> }
                                                    else { <span class="badge bg-danger bg-opacity-10 text-danger border border-danger ms-2">Mülk Sahibi</span> }
                                                </td>
                                                <td class="fw-bold">@debt.Amount.ToString("N2") ₺</td>
                                                <td>
                                                    @if(debt.IsPaid){ <span class="badge bg-success rounded-pill">Ödendi</span> }
                                                    else { <span class="badge bg-warning text-dark rounded-pill">Bekliyor</span> }
                                                </td>
                                                <td class="text-end pe-4">
                                                    @if(!debt.IsPaid) {
                                                        <button class="btn btn-sm btn-outline-success"><i class="bi bi-check-lg"></i> Tahsil Et</button>
                                                    }
                                                </td>
                                            </tr>
                                        }
                                    }
                                }
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>

        <!-- 3. SAKİNLER -->
        <div class="tab-pane fade" id="pills-sakinler" role="tabpanel">
            <div class="row">
                @if (Model.Units != null) {
                    foreach (var unit in Model.Units)
                    {
                        <div class="col-md-4 mb-4">
                            <div class="card border-0 shadow-sm rounded-4 h-100">
                                <div class="card-body position-relative">
                                    <div class="position-absolute top-0 end-0 mt-3 me-3">
                                        @if(unit.IsEmpty) { <span class="badge bg-secondary">BOŞ DAİRE</span> }
                                        else if(!string.IsNullOrEmpty(unit.TenantName)) { <span class="badge bg-info text-dark">KİRACI VAR</span> }
                                        else { <span class="badge bg-primary">EV SAHİBİ OTURUYOR</span> }
                                    </div>
                                    <h4 class="fw-bold text-dark mb-3"><i class="bi bi-door-closed text-muted me-2"></i>Daire @unit.UnitNumber</h4>
                                    
                                    <div class="mb-2">
                                        <small class="text-muted d-block text-uppercase fw-bold" style="font-size:0.7rem;">Mülk Sahibi</small>
                                        <span class="fw-semibold">@unit.OwnerName</span> <br>
                                        <small><i class="bi bi-telephone text-muted me-1"></i>@unit.OwnerPhone</small>
                                    </div>

                                    @if(!string.IsNullOrEmpty(unit.TenantName)) {
                                        <div class="mt-3 pt-3 border-top">
                                            <small class="text-muted d-block text-uppercase fw-bold" style="font-size:0.7rem;">Kiracı</small>
                                            <span class="fw-semibold">@unit.TenantName</span> <br>
                                            <small><i class="bi bi-telephone text-muted me-1"></i>@unit.TenantPhone</small>
                                        </div>
                                    }
                                </div>
                            </div>
                        </div>
                    }
                }
            </div>
        </div>

        <!-- 4. USTA & İŞ TAKİBİ -->
        <div class="tab-pane fade" id="pills-usta" role="tabpanel">
            <div class="row align-items-center mb-4">
                <div class="col-md-8">
                    <h5 class="fw-bold mb-1">GMK360 Esnaf / Usta Ağı</h5>
                    <p class="text-muted">Binanızın bakım onarımları için bölgenizdeki puanlanmış ustalardan teklif alın.</p>
                </div>
                <div class="col-md-4 text-end">
                    <button class="btn btn-dark rounded-pill fw-bold px-4"><i class="bi bi-search me-1"></i> Usta / Tedarikçi Çağır</button>
                </div>
            </div>

            <div class="card border-0 shadow-sm rounded-4">
                <div class="card-body p-4 text-center py-5">
                    <i class="bi bi-tools text-muted opacity-25" style="font-size: 5rem;"></i>
                    <h5 class="fw-bold mt-3">Aktif İş Emri Yok</h5>
                    <p class="text-muted w-50 mx-auto">Çatı aktarımı, asansör bakımı veya temizlik hizmeti gibi ihtiyaçlarınız için sistem üzerinden talebinizi açın, esnaflar size fiyat teklifi versin.</p>
                </div>
            </div>
        </div>
    </div>
</div>
'''

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print("BuildingManager Detail page completely redesigned!")