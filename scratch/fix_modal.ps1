@model IEnumerable<GMK360.Core.Entities.B2b.B2bCompany>
@{
    string listType = ViewBag.ListType as string ?? "firma";
    ViewData["Title"] = "GMK360 - " + ViewBag.PageTitle;
    Layout = "~/Views/Shared/_AdminLayout.cshtml";
    IEnumerable<GMK360.Core.Entities.DefinitionValue> categories = ViewBag.Categories as IEnumerable<GMK360.Core.Entities.DefinitionValue>;
    IEnumerable<GMK360.Core.Entities.City> cities = ViewBag.Cities as IEnumerable<GMK360.Core.Entities.City>;
    
    // UI Renk ve İkon Kararları
    string btnColor = listType == "usta" ? "btn-warning text-dark" : (listType == "tedarikci" ? "btn-success" : "btn-primary");
    string iconClass = listType == "usta" ? "ph-helmet text-warning" : (listType == "tedarikci" ? "ph-truck text-success" : "ph-buildings text-primary");
    string badgeColor = listType == "usta" ? "bg-warning text-dark" : (listType == "tedarikci" ? "bg-success" : "bg-primary");
}

<div class="container-fluid mt-4">
    <!-- Header -->
    <div class="row mb-4">
        <div class="col-12 d-flex justify-content-between align-items-center">
            <div>
                <nav aria-label="breadcrumb">
                    <ol class="breadcrumb mb-1">
                        <li class="breadcrumb-item"><a href="@Url.Action("B2bMarketplace", "Admin")" class="text-decoration-none">B2B Pazar Yeri</a></li>
                        <li class="breadcrumb-item active" aria-current="page">@ViewBag.PageTitle</li>
                    </ol>
                </nav>
                <h2 class="fw-bold text-dark mb-0"><i class="ph-fill @iconClass me-2"></i> @ViewBag.PageTitle</h2>
            </div>
            <div>
                <button class="btn @btnColor rounded-pill px-4 fw-bold shadow-sm" data-bs-toggle="modal" data-bs-target="#addModal">
                    <i class="ph ph-plus-circle me-1"></i> Yeni @(listType == "usta" ? "Usta" : (listType == "tedarikci" ? "Tedarikçi" : "Firma")) Ekle
                </button>
            </div>
        </div>
    </div>

    <!-- Filters -->
    <div class="card border-0 shadow-sm mb-4" style="border-radius: 15px;">
        <div class="card-body p-3">
            <form method="get" class="row g-3 align-items-end">
                <input type="hidden" name="type" value="@listType" />
                <div class="col-md-3">
                    <label class="form-label text-muted small fw-bold mb-1">Arama</label>
                    <div class="input-group">
                        <span class="input-group-text bg-white border-end-0 text-muted"><i class="ph ph-magnifying-glass"></i></span>
                        <input type="text" name="q" class="form-control border-start-0 ps-0" placeholder="İsim, vergi no, telefon..." value="@Context.Request.Query["q"]">
                    </div>
                </div>
                <div class="col-md-3">
                    <label class="form-label text-muted small fw-bold mb-1">Sektör / Ustalık</label>
                    <select class="form-select border-0 shadow-sm select2-search" style="width: 100%;" data-placeholder="Tüm Sektörler">
                        <option value="">Tüm Sektörler</option>
                        @if(categories != null)
                        {
                            foreach(var cat in categories)
                            {
                                <option value="@cat.Id">@cat.Name</option>
                            }
                        }
                    </select>
                </div>
                <div class="col-md-3">
                    <label class="form-label text-muted small fw-bold mb-1">İl</label>
                    <select class="form-select border-0 shadow-sm select2-search" style="width: 100%;" data-placeholder="Tüm İller">
                        <option value="">Tüm İller</option>
                        @if(cities != null)
                        {
                            foreach(var city in cities)
                            {
                                <option value="@city.Id">@city.Name</option>
                            }
                        }
                    </select>
                </div>
                <div class="col-md-3">
                    <button type="submit" class="btn btn-dark rounded-pill w-100 fw-bold">Filtrele</button>
                </div>
            
        </div>
    </div>

    <!-- Grid -->
    <div class="card border-0 shadow-sm" style="border-radius: 15px;">
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-hover align-middle mb-0">
                    <thead class="bg-light">
                        <tr>
                            <th class="px-4 py-3 border-0 rounded-top-start">@(listType == "usta" ? "Usta Adı Soyadı" : "Firma Ünvanı")</th>
                            <th class="px-4 py-3 border-0">Kategoriler / Alanlar</th>
                            @if(listType != "usta")
                            {
                                <th class="px-4 py-3 border-0">Vergi Dairesi & No</th>
                            }
                            <th class="px-4 py-3 border-0">Kayıt Eden (Referans)</th>
                            <th class="px-4 py-3 border-0">Puan</th>
                            <th class="px-4 py-3 border-0 text-end rounded-top-end">İşlemler</th>
                        </tr>
                    </thead>
                    <tbody>
                        @if(!Model.Any()) {
                            <tr><td colspan="6" class="text-center py-5 text-muted">Bu kategoride henüz kayıt bulunamadı.</td></tr>
                        }
                        @foreach(var item in Model)
                        {
                            <tr>
                                <td class="px-4 py-3">
                                    <div class="fw-bold text-dark">@item.Name</div>
                                    <div class="small text-muted">@item.CompanyType.ToString()</div>
                                </td>
                                <td class="px-4 py-3">
                                    @foreach(var cc in item.CompanyCategories)
                                    {
                                        <span class="badge @badgeColor bg-opacity-10 text-dark border border-opacity-25 px-2 py-1 me-1 mb-1">@cc.DefinitionValue?.Name</span>
                                    }
                                </td>
                                @if(listType != "usta")
                                {
                                    <td class="px-4 py-3">
                                        <div class="text-dark">@(string.IsNullOrEmpty(item.TaxOffice) ? "-" : item.TaxOffice)</div>
                                        <div class="small text-muted fw-bold">@(string.IsNullOrEmpty(item.TaxNumber) ? "-" : item.TaxNumber)</div>
                                    </td>
                                }
                                <td class="px-4 py-3">
                                    @if(item.AddedByAgency != null)
                                    {
                                        <span class="badge bg-secondary bg-opacity-10 text-secondary border border-secondary border-opacity-25 px-2 py-1">
                                            <i class="ph-fill ph-handshake me-1"></i> @item.AddedByAgency.CompanyName
                                        </span>
                                    }
                                    else
                                    {
                                        <span class="text-muted">-</span>
                                    }
                                </td>
                                <td class="px-4 py-3">
                                    @for(int i=0; i<5; i++)
                                    {
                                        if(i < item.Rating) { <i class="ph-fill ph-star text-warning"></i> }
                                        else { <i class="ph ph-star text-muted"></i> }
                                    }
                                </td>
                                <td class="px-4 py-3 text-end">
                                    <button class="btn btn-sm btn-light border shadow-sm rounded-circle"><i class="ph ph-caret-right"></i></button>
                                </td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
            
        </div>
    </div>
</div>

<!-- Modal: Yeni Ekle (Mock - UI için hazırlanacak) -->
<div class="modal fade" id="addModal" tabindex="-1">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content" style="border-radius: 15px;">
            <div class="modal-header border-0 pb-0">
                <h5 class="modal-title fw-bold">Yeni @(listType == "usta" ? "Usta" : (listType == "tedarikci" ? "Tedarikçi" : "Firma")) Ekle</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form asp-action="AddUsta" method="post">
            </div>
            <div class="modal-body">
                <p class="text-muted small">Bu ekran **Faz 2**'de dinamik Sihirbaz (Wizard) ile değiştirilecektir. Arayüzün (UI) Masterpage ile nasıl ayrıştığını göstermek için Mock olarak bırakılmıştır.</p>
                
                @if(listType == "usta")
                {
                    <div class="mb-3">
                        <label class="form-label fw-bold small">Usta Adı Soyadı (veya Taşeron Ekip Adı)</label>
                        <input type="text" class="form-control" placeholder="Örn: Ahmet Yılmaz" name="name" required />
                    </div>
                    <div class="row">
                        <div class="col-6 mb-3">
                            <label class="form-label fw-bold small">TC Kimlik No</label>
                            <input type="text" class="form-control" placeholder="İsteğe Bağlı" name="tcKimlik" />
                        </div>
                        <div class="col-6 mb-3">
                            <label class="form-label fw-bold small">Telefon Numarası</label>
                            <input type="text" class="form-control" placeholder="05XX XXX XX XX" name="phone" required />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-4 mb-3">
                            <label class="form-label fw-bold small">Hizmet İli</label>
                            <select class="form-select border-0 shadow-sm select2-search" id="modalCity" name="cityId" required style="width: 100%;">
                                <option value="">İl Seçin</option>
                                @if(cities != null)
                                {
                                    foreach(var c in cities)
                                    {
                                        <option value="@c.Id">@c.Name</option>
                                    }
                                }
                            </select>
                        </div>
                        <div class="col-4 mb-3">
                            <label class="form-label fw-bold small">İlçe</label>
                            <select class="form-select border-0 shadow-sm select2-search" id="modalDistrict" name="districtId" style="width: 100%;">
                                <option value="">İlçe Seçin</option>
                            </select>
                        </div>
                        <div class="col-4 mb-3">
                            <label class="form-label fw-bold small">Mahalle</label>
                            <select class="form-select border-0 shadow-sm select2-search" id="modalNeighborhood" name="neighborhoodId" style="width: 100%;">
                                <option value="">Mahalle Seçin</option>
                            </select>
                        </div>
                    </div>
                }
                else
                {
                    <div class="mb-3">
                        <label class="form-label fw-bold small">Firma Ünvanı</label>
                        <input type="text" class="form-control" placeholder="Örn: XYZ Mimarlık A.Ş." />
                    </div>
                    <div class="row">
                        <div class="col-6 mb-3">
                            <label class="form-label fw-bold small">Vergi Dairesi</label>
                            <input type="text" class="form-control" />
                        </div>
                        <div class="col-6 mb-3">
                            <label class="form-label fw-bold small">Vergi No</label>
                            <input type="text" class="form-control" />
                        </div>
                    </div>
                }

                <div class="mb-3">
                    <label class="form-label fw-bold small">@(listType == "usta" ? "Ustalık Alanları" : "Sektörleri")</label>
                    <select class="form-select border-0 shadow-sm select2-search" multiple="multiple" style="width: 100%;" name="categoryIds" required>
                        @if(categories != null)
                        {
                            foreach(var cat in categories)
                            {
                                <option value="@cat.Id">@cat.Name</option>
                            }
                        }
                    </select>
                </div>
            </div>
            </div>
            <div class="modal-footer border-0 pt-0">
                <button type="button" class="btn btn-secondary rounded-pill px-4" data-bs-dismiss="modal">İptal</button>
                <button type="submit" class="btn @btnColor rounded-pill px-4 fw-bold">Kaydet</button>
            </div>
            
        </div>
    </div>
</div>

@section Scripts {
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
    <script>
        $(document).ready(function() {
            $('.select2-search').select2({
                theme: "classic",
                width: 'resolve'
            });
        });
    </script>
}

