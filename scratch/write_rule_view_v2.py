code = """@model List<GMK360.Core.Entities.ModuleDocumentRule>
@{
    ViewData["Title"] = "Modül Evrak Kuralları";
    Layout = "_AdminLayout";
    var allTemplates = ViewBag.Templates as List<GMK360.Core.Entities.SystemLegalDocumentTemplate>;
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
                <div class="card-body p-4">
                    @if (!Model.Any())
                    {
                        <div class="text-center text-muted py-5">
                            <i class="ph ph-tree-structure display-1 text-light mb-3"></i>
                            <h4>Sistemde henüz bir kural eşleştirmesi bulunmuyor.</h4>
                        </div>
                    }
                    else
                    {
                        var modules = Model.GroupBy(m => m.TargetModule).ToList();
                        
                        <div class="accordion" id="rulesAccordion">
                            @foreach (var mod in modules)
                            {
                                var modId = mod.Key.Replace(" ", "");
                                <div class="accordion-item border-0 mb-3 bg-light rounded-4">
                                    <h2 class="accordion-header">
                                        <button class="accordion-button fw-bold fs-5 rounded-4" type="button" data-bs-toggle="collapse" data-bs-target="#collapse_@modId" aria-expanded="true">
                                            <i class="ph ph-buildings me-2 text-primary"></i> Modül: @mod.Key
                                        </button>
                                    </h2>
                                    <div id="collapse_@modId" class="accordion-collapse collapse show">
                                        <div class="accordion-body bg-white rounded-bottom-4">
                                            
                                            @{
                                                var stages = mod.GroupBy(m => m.Stage).ToList();
                                            }

                                            @foreach (var stg in stages)
                                            {
                                                <div class="mb-4">
                                                    <h6 class="fw-bold text-secondary border-bottom pb-2 mb-3">
                                                        <i class="ph ph-git-merge me-1"></i> Aşama (Faz): @stg.Key
                                                    </h6>
                                                    
                                                    <div class="ps-4 border-start border-2 border-primary">
                                                        @foreach (var item in stg.OrderBy(x => x.DisplayOrder))
                                                        {
                                                            <div class="card border-0 shadow-sm mb-2 rounded-3">
                                                                <div class="card-body py-2 d-flex justify-content-between align-items-center">
                                                                    <div>
                                                                        <span class="badge bg-secondary me-2">#@item.DisplayOrder</span>
                                                                        <span class="fw-bold">
                                                                            <i class="ph ph-file-text me-1 text-primary"></i> 
                                                                            @(item.SystemLegalDocumentTemplate?.Name ?? "Bilinmiyor")
                                                                        </span>
                                                                        
                                                                        @if (!string.IsNullOrEmpty(item.PrerequisiteTemplateIds))
                                                                        {
                                                                            var prereqIds = item.PrerequisiteTemplateIds.Split(',').Select(int.Parse).ToList();
                                                                            var prereqNames = allTemplates?.Where(t => prereqIds.Contains(t.Id)).Select(t => t.Name).ToList();
                                                                            if(prereqNames != null && prereqNames.Any())
                                                                            {
                                                                                <div class="mt-1 ms-5 text-warning fw-bold" style="font-size: 0.85rem;">
                                                                                    <i class="ph ph-lock-key me-1"></i> Ön Koşullar: 
                                                                                    <span class="text-dark">@string.Join(", ", prereqNames)</span>
                                                                                </div>
                                                                            }
                                                                        }
                                                                    </div>
                                                                    <div>
                                                                        <form asp-action="Delete" method="post" class="d-inline" onsubmit="return confirm(\'Bu kuralı silmek istediğinize emin misiniz?\');">
                                                                            <input type="hidden" name="id" value="@item.Id" />
                                                                            <button type="submit" class="btn btn-sm btn-outline-danger rounded-circle"><i class="ph ph-trash"></i></button>
                                                                        </form>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        }
                                                    </div>
                                                </div>
                                            }

                                        </div>
                                    </div>
                                </div>
                            }
                        </div>
                    }
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
                            <select name="SystemLegalDocumentTemplateId" class="form-select border-primary" asp-items="ViewBag.TemplatesSelect" required>
                                <option value="">-- Havuzdan Evrak Seç --</option>
                            </select>
                        </div>
                        <div class="col-md-12 mt-3">
                            <div class="form-check form-switch mb-2">
                                <input class="form-check-input" type="checkbox" id="hasPrerequisiteToggle">
                                <label class="form-check-label fw-bold" for="hasPrerequisiteToggle">Bu evrakın ön koşulu (bağımlılığı) var mı?</label>
                            </div>
                            <div id="prerequisiteDiv" style="display:none;" class="p-3 bg-light rounded-3 border">
                                <label class="form-label fw-bold text-warning"><i class="ph ph-lock me-1"></i>Hangi evrak(lar) alınmadan kilit açılmasın?</label>
                                <!-- Çoklu seçim için Select2 sınıfı ekledik ve multiple yaptık -->
                                <select name="PrerequisiteTemplateIdsList" class="form-select select2-multiple" multiple="multiple" asp-items="ViewBag.TemplatesSelect" style="width: 100%;">
                                </select>
                                <small class="text-muted mt-1 d-block">Not: Birden fazla ön koşul evrakı seçebilirsiniz. Seçilen evrakların hepsi alınmadan ana evrak kilitli kalacaktır.</small>
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
        $(document).ready(function() {
            // Modal içindeki select2'nin düzgün çalışması için dropdownParent ayarı yapıyoruz
            $('.select2-multiple').select2({
                placeholder: "Bağımlı evrakları seçin...",
                allowClear: true,
                dropdownParent: $('#addRuleModal')
            });

            $('#hasPrerequisiteToggle').on('change', function() {
                var div = $('#prerequisiteDiv');
                if(this.checked) {
                    div.slideDown();
                } else {
                    div.slideUp();
                    $('.select2-multiple').val(null).trigger('change');
                }
            });
        });
    </script>
}
"""
with open('GMK360.Web/Views/ModuleDocumentRule/Index.cshtml', 'w', encoding='utf-8') as f:
    f.write(code)
