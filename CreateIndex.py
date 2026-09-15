import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Index.cshtml'

content = '''@model IEnumerable<GMK360.Core.Entities.Construction.ConstructionProject>
@using GMK360.Core.Entities.Construction
@{
    ViewData["Title"] = "Şantiyelerimiz (Projeler)";
    Layout = "_ConstructionLayout";

    var aktifler = Model.Where(x => x.Status == ProjectStatus.Aktif_Santiye).ToList();
    var teklifler = Model.Where(x => x.Status == ProjectStatus.Projelendirme_Teklif).ToList();
    var satistakiler = Model.Where(x => x.Status == ProjectStatus.Satista_Topraktan).ToList();
    var tamamlananlar = Model.Where(x => x.Status == ProjectStatus.Tamamlandi_Teslim).ToList();
}

<div class="container-fluid mt-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h2 class="fw-bold mb-0 text-dark"><i class="bi bi-building text-primary me-2"></i> Şantiyelerimiz</h2>
            <p class="text-muted mb-0">Firmanızın geçmiş, güncel ve gelecek tüm projelerini buradan yönetin.</p>
        </div>
        <a href="/ConstructionProject/Create" class="btn btn-primary rounded-pill px-4 shadow-sm">
            <i class="bi bi-plus-lg me-1"></i> Yeni Proje (Şantiye) Aç
        </a>
    </div>

    <!-- Filtre Sekmeleri -->
    <ul class="nav nav-pills mb-4 gap-2" id="projectTabs" role="tablist">
        <li class="nav-item" role="presentation">
            <button class="nav-link active rounded-pill px-4" id="aktif-tab" data-bs-toggle="pill" data-bs-target="#aktif" type="button" role="tab">
                <i class="bi bi-cone-striped me-1"></i> Aktif Şantiyeler <span class="badge bg-white text-primary ms-1">@aktifler.Count</span>
            </button>
        </li>
        <li class="nav-item" role="presentation">
            <button class="nav-link rounded-pill px-4 bg-light text-dark" id="teklif-tab" data-bs-toggle="pill" data-bs-target="#teklif" type="button" role="tab">
                <i class="bi bi-pencil-square me-1"></i> Teklif Aşamasındakiler <span class="badge bg-secondary ms-1">@teklifler.Count</span>
            </button>
        </li>
        <li class="nav-item" role="presentation">
            <button class="nav-link rounded-pill px-4 bg-light text-dark" id="satis-tab" data-bs-toggle="pill" data-bs-target="#satis" type="button" role="tab">
                <i class="bi bi-cash-coin me-1"></i> Satışta Olanlar <span class="badge bg-secondary ms-1">@satistakiler.Count</span>
            </button>
        </li>
        <li class="nav-item" role="presentation">
            <button class="nav-link rounded-pill px-4 bg-light text-dark" id="tamamlanan-tab" data-bs-toggle="pill" data-bs-target="#tamamlanan" type="button" role="tab">
                <i class="bi bi-trophy me-1"></i> Tamamlananlar <span class="badge bg-secondary ms-1">@tamamlananlar.Count</span>
            </button>
        </li>
    </ul>

    <div class="tab-content" id="projectTabsContent">
        <!-- Aktif Şantiyeler -->
        <div class="tab-pane fade show active" id="aktif" role="tabpanel">
            <div class="row g-4">
                @foreach(var p in aktifler)
                {
                    <div class="col-xl-3 col-lg-4 col-md-6">
                        @await Html.PartialAsync("_ProjectCardPartial", p)
                    </div>
                }
                @if(!aktifler.Any()){ <div class="col-12"><div class="alert alert-light text-center py-5 border text-muted"><i class="bi bi-info-circle fs-1 d-block mb-3"></i>Şu an devam eden aktif şantiyeniz bulunmuyor.</div></div> }
            </div>
        </div>

        <!-- Teklif / Projelendirme -->
        <div class="tab-pane fade" id="teklif" role="tabpanel">
            <div class="row g-4">
                @foreach(var p in teklifler)
                {
                    <div class="col-xl-3 col-lg-4 col-md-6">
                        @await Html.PartialAsync("_ProjectCardPartial", p)
                    </div>
                }
                @if(!teklifler.Any()){ <div class="col-12"><div class="alert alert-light text-center py-5 border text-muted">Teklif aşamasında proje bulunmuyor.</div></div> }
            </div>
        </div>

        <!-- Satışta Olanlar -->
        <div class="tab-pane fade" id="satis" role="tabpanel">
            <div class="row g-4">
                @foreach(var p in satistakiler)
                {
                    <div class="col-xl-3 col-lg-4 col-md-6">
                        @await Html.PartialAsync("_ProjectCardPartial", p)
                    </div>
                }
                @if(!satistakiler.Any()){ <div class="col-12"><div class="alert alert-light text-center py-5 border text-muted">Satışta olan projeniz bulunmuyor.</div></div> }
            </div>
        </div>

        <!-- Tamamlananlar -->
        <div class="tab-pane fade" id="tamamlanan" role="tabpanel">
            <div class="row g-4">
                @foreach(var p in tamamlananlar)
                {
                    <div class="col-xl-3 col-lg-4 col-md-6">
                        @await Html.PartialAsync("_ProjectCardPartial", p)
                    </div>
                }
                @if(!tamamlananlar.Any()){ <div class="col-12"><div class="alert alert-light text-center py-5 border text-muted">Henüz tamamlanmış projeniz bulunmuyor.</div></div> }
            </div>
        </div>
    </div>
</div>

<style>
    /* Tab pill active durumunu ezmek için */
    .nav-pills .nav-link.active {
        background-color: #0d6efd !important;
        color: white !important;
    }
    .nav-pills .nav-link.active .badge {
        background-color: white !important;
        color: #0d6efd !important;
    }
</style>
'''

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
