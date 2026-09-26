code = """@model List<GMK360.Core.Entities.ModuleDocumentRule>
@{
    ViewData["Title"] = "Modül Evrak Kuralları";
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
                <i class="ph ph-tree-structure text-primary me-2"></i>Şablon & Aşama Kuralları
            </h3>
            <p class="text-muted mb-0">Hangi aşamada hangi evrak istenecek ve kilit (ön koşul) mantığı ne olacak, buradan belirleyin.</p>
        </div>
        <div>
            <a href="/AdminLegalDocument/Index" class="btn btn-outline-secondary rounded-pill px-4 fw-bold shadow-sm me-2">
                <i class="ph ph-arrow-left me-1"></i> Havuza Dön
            </a>
            <button class="btn btn-primary rounded-pill px-4 fw-bold shadow-sm" data-bs-toggle="modal" data-bs-target="#addRuleModal">
                <i class="ph ph-plus me-1"></i> Yeni Kural Ekle
            </button>
        </div>
    </div>

    <div class="row">
        <div class="col-12">
            <div class="card shadow-sm border-0 rounded-4">
                <div class="card-body p-0 table-responsive">
                    <table class="table table-hover align-middle mb-0">
                        <thead class="bg-light">
                            <tr>
                                <th class="ps-4">Sıra</th>
                                <th>Modül</th>
                                <th>Aşama (Faz)</th>
                                <th>İstenen Evrak (Havuzdan)</th>
                                <th>Ön Koşul (Bağımlılık)</th>
                                <th class="text-end pe-4">İşlemler</th>
                            </tr>
                        </thead>
                        <tbody>
                        @if (!Model.Any())
                        {
                            <tr>
                                <td colspan="6" class="text-center text-muted py-4">Sistemde henüz bir kural eşleştirmesi bulunmuyor.</td>
                            </tr>
                        }
                        else
                        {
                            @foreach (var item in Model)
                            {
                                <tr>
                                    <td class="ps-4 fw-bold text-muted">@item.DisplayOrder</td>
                                    <td><span class="badge bg-primary">@item.TargetModule</span></td>
                                    <td class="fw-bold">@item.Stage</td>
                                    <td>
                                        <i class="ph ph-file-text me-1 text-secondary"></i> 
                                        @(item.SystemLegalDocumentTemplate?.Name ?? "Bilinmiyor")
                                    </td>
                                    <td>
                                        @if(item.PrerequisiteTemplateId.HasValue)
                                        {
                                            <span class="badge bg-warning text-dark border border-warning" style="background-color: #fff3cd !important;">
                                                <i class="ph ph-lock me-1"></i> @(item.PrerequisiteTemplate?.Name ?? "Bilinmiyor")
                                            </span>
                                        }
                                        else
                                        {
                                            <span class="text-muted"><i class="ph ph-check-circle me-1"></i>Ön Koşul Yok</span>
                                        }
                                    </td>
                                    <td class="text-end pe-4">
                                        <form asp-action="Delete" method="post" class="d-inline" onsubmit="return confirm('Bu kuralı silmek istediğinize emin misiniz? (Önceden açılmış şantiyeleri etkilemez)');">
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

<!-- Kural Ekleme Modalı -->
<div class="modal fade" id="addRuleModal" tabindex="-1">
    <div class="modal-dialog modal-lg">
        <div class="modal-content rounded-4 border-0 shadow">
            <form asp-action="Create" method="post">
                <div class="modal-header border-bottom-0 bg-light rounded-top-4">
                    <h5 class="modal-title fw-bold text-primary"><i class="ph ph-link me-2"></i>Yeni Evrak Kuralı Ekle</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body p-4">
                    <div class="row g-3">
                        <div class="col-md-4">
                            <label class="form-label fw-bold">Modül</label>
                            <select name="TargetModule" class="form-select" required>
                                <option value="Construction">İnşaat (Construction)</option>
                                <option value="RealEstate">Emlak / Satış</option>
                            </select>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label fw-bold">Aşama / Faz</label>
                            <input type="text" name="Stage" class="form-control" placeholder="Örn: 1. Yıkım Öncesi" required>
                        </div>
                        <div class="col-md-2">
                            <label class="form-label fw-bold">Sıra No</label>
                            <input type="number" name="DisplayOrder" class="form-control" value="1" required>
                        </div>
                        <div class="col-md-12 mt-4">
                            <label class="form-label fw-bold text-primary">Hangi Evrak İstenecek? (Havuzdan Seç)</label>
                            <select name="SystemLegalDocumentTemplateId" class="form-select border-primary" asp-items="ViewBag.Templates" required>
                                <option value="">-- Havuzdan Evrak Seç --</option>
                            </select>
                        </div>
                        <div class="col-md-12 mt-3">
                            <div class="form-check form-switch mb-2">
                                <input class="form-check-input" type="checkbox" id="hasPrerequisiteToggle">
                                <label class="form-check-label fw-bold" for="hasPrerequisiteToggle">Bu evrakın ön koşulu (bağımlılığı) var mı?</label>
                            </div>
                            <div id="prerequisiteDiv" style="display:none;" class="p-3 bg-light rounded-3 border">
                                <label class="form-label fw-bold text-warning"><i class="ph ph-lock me-1"></i>Hangi evrak alınmadan kilit açılmasın?</label>
                                <select name="PrerequisiteTemplateId" class="form-select" asp-items="ViewBag.Templates">
                                    <option value="">-- Ön Koşul Evrakını Seç --</option>
                                </select>
                                <small class="text-muted mt-1 d-block">Not: Seçilen ön koşul evrakı şantiyede 'Alındı' yapılmadan, ana evrak kilitli kalacaktır.</small>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="modal-footer border-top-0 rounded-bottom-4">
                    <button type="button" class="btn btn-secondary rounded-pill px-4" data-bs-dismiss="modal">İptal</button>
                    <button type="submit" class="btn btn-primary rounded-pill px-4 fw-bold">Kuralı Kaydet</button>
                </div>
            </form>
        </div>
    </div>
</div>

@section Scripts {
    <script>
        document.getElementById('hasPrerequisiteToggle').addEventListener('change', function() {
            var div = document.getElementById('prerequisiteDiv');
            if(this.checked) {
                div.style.display = 'block';
            } else {
                div.style.display = 'none';
                document.querySelector('select[name="PrerequisiteTemplateId"]').value = '';
            }
        });
    </script>
}
"""
import os
os.makedirs('GMK360.Web/Views/ModuleDocumentRule', exist_ok=True)
with open('GMK360.Web/Views/ModuleDocumentRule/Index.cshtml', 'w', encoding='utf-8') as f:
    f.write(code)
