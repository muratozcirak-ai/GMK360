code = '''@model List<GMK360.Core.Entities.SystemLegalDocumentTemplate>
@{
    ViewData["Title"] = "Global Evrak Havuzu";
    Layout = "_AdminLayout";
}

<div class="container-fluid py-4">
    @if(TempData["ErrorMessage"] != null)
    {
        <div class="alert alert-danger alert-dismissible fade show rounded-4" role="alert">
            <i class="ph ph-warning-circle me-2"></i> @TempData["ErrorMessage"]
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    }
    @if(TempData["SuccessMessage"] != null)
    {
        <div class="alert alert-success alert-dismissible fade show rounded-4" role="alert">
            <i class="ph ph-check-circle me-2"></i> @TempData["SuccessMessage"]
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    }

    <div class="d-flex align-items-center justify-content-between mb-4">
        <div>
            <h3 class="mb-0 text-primary fw-bold">
                <i class="ph ph-files text-primary me-2"></i>Global Evrak Havuzu (Kütüphane)
            </h3>
            <p class="text-muted mb-0">Tüm modüllerde kullanılabilecek saf evrak şablonları.</p>
        </div>
        <button class="btn btn-primary rounded-pill px-4 fw-bold shadow-sm" data-bs-toggle="modal" data-bs-target="#addModal">
            <i class="ph ph-plus me-1"></i> Yeni Evrak Ekle
        </button>
    </div>

    <div class="row">
        <div class="col-12">
            <div class="card shadow-sm border-0 rounded-4">
                <div class="card-body p-0 table-responsive">
                    <table class="table table-hover align-middle mb-0">
                        <thead class="bg-light">
                            <tr>
                                <th class="ps-4">ID</th>
                                <th>Evrak Adı</th>
                                <th>Kimden Alınır / Kaynak</th>
                                <th>Kayıt Tarihi</th>
                                <th class="text-end pe-4">İşlemler</th>
                            </tr>
                        </thead>
                        <tbody>
                        @if (!Model.Any())
                        {
                            <tr>
                                <td colspan="5" class="text-center text-muted py-4">Sistemde henüz evrak bulunmuyor.</td>
                            </tr>
                        }
                        else
                        {
                            @foreach (var item in Model)
                            {
                                <tr>
                                    <td class="ps-4">
                                        <span class="badge bg-secondary">#@item.Id</span>
                                    </td>
                                    <td class="fw-bold">
                                        @item.Name
                                    </td>
                                    <td>@(item.IssuedBy ?? "-")</td>
                                    <td>@item.CreatedAt.ToString("dd.MM.yyyy HH:mm")</td>
                                    <td class="text-end pe-4">
                                        <button type="button" class="btn btn-sm btn-outline-primary rounded-circle me-1 edit-btn"
                                                data-id="@item.Id"
                                                data-name="@item.Name"
                                                data-issuedby="@item.IssuedBy">
                                            <i class="ph ph-pencil"></i>
                                        </button>
                                        <form asp-action="Delete" method="post" class="d-inline" onsubmit="return confirm(\'Silmek istediğinize emin misiniz?\');">
                                            <input type="hidden" name="id" value="@item.Id" />
                                            <button type="submit" class="btn btn-sm btn-outline-danger rounded-circle"><i class="ph ph-trash"></i></button>
                                        </form>
                                    </td>
                                </tr>
                            }
                        }
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</div>

<div class="modal fade" id="addModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content rounded-4 border-0 shadow">
            <form asp-action="Create" method="post">
                <div class="modal-header border-bottom-0">
                    <h5 class="modal-title fw-bold text-primary">Yeni Evrak Ekle (Havuza)</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label fw-bold">Evrak Adı</label>
                        <input type="text" name="Name" class="form-control" required placeholder="Örn: Yapı Ruhsatı">
                    </div>
                    <div class="mb-3">
                        <label class="form-label fw-bold">Kimden Alınır / Kaynak Kurum</label>
                        <input type="text" name="IssuedBy" class="form-control" placeholder="Örn: Belediye">
                    </div>
                </div>
                <div class="modal-footer bg-light border-top-0 rounded-bottom-4">
                    <button type="button" class="btn btn-secondary rounded-pill px-4" data-bs-dismiss="modal">İptal</button>
                    <button type="submit" class="btn btn-primary rounded-pill px-4 fw-bold">Kaydet</button>
                </div>
            </form>
        </div>
    </div>
</div>

<div class="modal fade" id="editModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content rounded-4 border-0 shadow">
            <form asp-action="Edit" method="post">
                <input type="hidden" name="Id" id="edit-id" />
                <div class="modal-header border-bottom-0">
                    <h5 class="modal-title fw-bold text-primary">Evrak Düzenle</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label fw-bold">Evrak Adı</label>
                        <input type="text" name="Name" id="edit-name" class="form-control" required>
                    </div>
                    <div class="mb-3">
                        <label class="form-label fw-bold">Kimden Alınır / Kaynak Kurum</label>
                        <input type="text" name="IssuedBy" id="edit-issuedby" class="form-control">
                    </div>
                </div>
                <div class="modal-footer bg-light border-top-0 rounded-bottom-4">
                    <button type="button" class="btn btn-secondary rounded-pill px-4" data-bs-dismiss="modal">İptal</button>
                    <button type="submit" class="btn btn-primary rounded-pill px-4 fw-bold">Güncelle</button>
                </div>
            </form>
        </div>
    </div>
</div>

@section Scripts {
    <script>
        .ready(function() {
            .edit-btn.on("click", function() {
                var btn = ;
                #edit-id.val(btn.data("id"));
                #edit-name.val(btn.data("name"));
                #edit-issuedby.val(btn.data("issuedby"));
                #editModal.modal("show");
            });
        });
    </script>
}'''
with open('GMK360.Web/Views/AdminLegalDocument/Index.cshtml', 'w', encoding='utf-8') as f:
    f.write(code)
