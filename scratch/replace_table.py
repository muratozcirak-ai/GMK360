import re

filepath = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'

try:
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Find the block containing the table for Phase0Docs
    # The table is inside <div class="table-responsive">...</div> under the "FAZ 0" accordion.
    
    table_pattern = re.compile(r'<table class="table table-hover align-middle mb-0 border-top">[\s\S]*?</table>', re.MULTILINE)
    
    new_html = '''
    <div class="accordion" id="accordionPhase0">
        @if(ViewBag.GroupedPhase0Docs != null)
        {
            var groups = (IEnumerable<IGrouping<string, GMK360.Core.Entities.Construction.ProjectLegalDocument>>)ViewBag.GroupedPhase0Docs;
            int gIndex = 0;
            foreach (var group in groups)
            {
                gIndex++;
                string headerId = "headingPhase0_" + gIndex;
                string collapseId = "collapsePhase0_" + gIndex;
                
                <div class="accordion-item mb-3 border-0 shadow-sm rounded-4 overflow-hidden">
                    <h2 class="accordion-header" id="@headerId">
                        <button class="accordion-button @(gIndex == 1 ? "" : "collapsed") bg-light fw-bold text-dark" type="button" data-bs-toggle="collapse" data-bs-target="#@collapseId" aria-expanded="@(gIndex == 1 ? "true" : "false")" aria-controls="@collapseId">
                            <i class="ph ph-folder text-warning me-2 fs-5"></i> @group.Key
                            <span class="badge bg-primary rounded-pill ms-auto">@group.Count() Evrak</span>
                        </button>
                    </h2>
                    <div id="@collapseId" class="accordion-collapse collapse @(gIndex == 1 ? "show" : "")" aria-labelledby="@headerId" data-bs-parent="#accordionPhase0">
                        <div class="accordion-body p-0">
                            <table class="table table-hover align-middle mb-0">
                                <thead class="table-light">
                                    <tr>
                                        <th class="ps-4">Sıra</th>
                                        <th>Evrak Adı</th>
                                        <th>Kimden Alınır / Makam</th>
                                        <th>Kurum İçi İlgili</th>
                                        <th>Sorumlu Takipçi</th>
                                        <th>Durum</th>
                                        <th>Açıklama / Not</th>
                                        <th class="text-end pe-3">İşlem</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    @foreach(var doc in group)
                                    {
                                        <tr>
                                            <td class="ps-4">
                                                <span class="badge bg-secondary">@doc.DisplayOrder</span>
                                            </td>
                                            <td class="fw-bold">
                                                @if(doc.IsLocked)
                                                {
                                                    <i class="bi bi-lock-fill text-danger me-2" title="@doc.MissingPrerequisitesMessage"></i>
                                                }
                                                else
                                                {
                                                    <i class="bi bi-unlock-fill text-success me-2" title="Kilit Açık"></i>
                                                }
                                                @(doc.SystemTemplate?.Name ?? doc.DocumentName)
                                            </td>
                                            <td><span class="badge bg-light text-dark border"><i class="bi bi-building"></i> @(doc.InstitutionContact ?? "Belirtilmedi")</span></td>
                                            <td><span class="text-muted fst-italic">Bilinmiyor</span></td>
                                            <td>
                                                @if(!string.IsNullOrEmpty(doc.AssignedUserId)) {
                                                    <span><i class="bi bi-person-check text-primary"></i> @doc.AssignedUserId</span>
                                                } else {
                                                    <span class="text-danger small"><i class="bi bi-exclamation-circle"></i> Atanmadı</span>
                                                }
                                            </td>
                                            <td>
                                                @if(doc.Status == "Tamamlandı") {
                                                    <span class="badge bg-success rounded-pill px-3"><i class="bi bi-check2-all"></i> Onaylandı</span>
                                                } else if (doc.Status == "İşlemde") {
                                                    <span class="badge bg-warning text-dark rounded-pill px-3"><i class="bi bi-hourglass-split"></i> İşlemde</span>
                                                } else if (doc.Status == "Sorunlu") {
                                                    <span class="badge bg-danger rounded-pill px-3"><i class="bi bi-x-circle"></i> Sorun Çıktı</span>
                                                } else {
                                                    <span class="badge bg-danger rounded-pill px-3"><i class="bi bi-x"></i> Yok</span>
                                                }
                                            </td>
                                            <td>
                                                @if(!string.IsNullOrEmpty(doc.IssueNotes)) { <small class="text-muted">@doc.IssueNotes</small> } else { <span class="text-muted">-</span> }
                                            </td>
                                            <td class="text-end pe-3">
                                                @if(!string.IsNullOrEmpty(doc.FilePath))
                                                {
                                                    <a href="@doc.FilePath" target="_blank" class="btn btn-sm btn-outline-primary rounded-pill me-1" title="Evrakı Görüntüle">
                                                        <i class="bi bi-file-earmark-pdf"></i>
                                                    </a>
                                                }
                                                <button class="btn btn-sm btn-outline-primary rounded-pill" data-bs-toggle="modal" data-bs-target="#editDocModal_@doc.Id" @(doc.IsLocked ? "disabled" : "") title="@(doc.IsLocked ? doc.MissingPrerequisitesMessage : "")">
                                                    <i class="bi bi-pencil-square"></i> Yönet
                                                </button>
                                            </td>
                                        </tr>
                                        <!-- Güncelle Modal -->
                                        <div class="modal fade" id="editDocModal_@doc.Id" tabindex="-1">
                                            <div class="modal-dialog">
                                                <div class="modal-content border-0 shadow">
                                                    <form asp-action="UpdateLegalDocumentStatus" asp-controller="ConstructionProject" method="post" enctype="multipart/form-data">
                                                        <div class="modal-header bg-light border-0">
                                                            <h5 class="modal-title text-dark fw-bold"><i class="bi bi-pencil-square text-primary me-2"></i>Evrak / Aşama Yönetimi</h5>
                                                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                                                        </div>
                                                        <div class="modal-body p-4">
                                                            <input type="hidden" name="documentId" value="@doc.Id" />
                                                            <div class="alert alert-info border-0 rounded-3 mb-4">
                                                                <strong class="d-block mb-1 fs-5">@(doc.SystemTemplate?.Name ?? doc.DocumentName)</strong>
                                                                <small>@(doc.SystemTemplate?.TargetModule ?? "Projeye özel eklenmiş evrak.")</small>
                                                            </div>
                                                            <div class="mb-3">
                                                                <label class="form-label fw-bold">Nereye Başvuruldu / Kimden Alınır?</label>
                                                                <input type="text" class="form-control" name="institutionContact" value="@doc.InstitutionContact">
                                                            </div>
                                                            <div class="mb-3">
                                                                <label class="form-label fw-bold">Evrak / Başvuru Durumu</label>
                                                                <select class="form-select" name="statusId">
                                                                    <option value="0" selected="@(doc.Status?.ToString() == "Bekliyor")">Alınmadı / Yok</option>
                                                                    <option value="1" selected="@(doc.Status?.ToString() == "İşlemde")">Başvuruldu / Bekliyor</option>
                                                                    <option value="2" selected="@(doc.Status?.ToString() == "Sorunlu")">Sorun Çıktı / İptal (Açıklama Giriniz)</option>
                                                                    <option value="3" selected="@(doc.Status?.ToString() == "Tamamlandı")">Onaylandı / Var</option>
                                                                </select>
                                                            </div>
                                                            <div class="mb-3">
                                                                <label class="form-label fw-bold">Tahmini Harç / Maliyet (₺)</label>
                                                                <input type="number" step="0.01" class="form-control" name="estimatedCost" value="@doc.EstimatedCost">
                                                            </div>
                                                            <div class="mb-3">
                                                                <label class="form-label fw-bold">Kesinleşen Maliyet (₺)</label>
                                                                <input type="number" step="0.01" class="form-control" name="actualCost" value="@doc.ActualCost">
                                                            </div>
                                                            <div class="mb-3">
                                                                <label class="form-label fw-bold">Açıklama / Süreç Notları</label>
                                                                <textarea class="form-control" name="notes" rows="3" placeholder="Örn: Belediye reddetti çünkü projede eksiklik var...">@doc.IssueNotes</textarea>
                                                            </div>
                                                            <hr/>
                                                            <div class="row bg-light rounded p-3 mb-3 border">
                                                                <h6 class="fw-bold text-success mb-3"><i class="bi bi-check-circle"></i> Tarihler</h6>
                                                                <div class="col-md-6 mb-3">
                                                                    <label class="form-label fw-bold text-info"><i class="bi bi-calendar-plus"></i> Başvuru Tarihi</label>
                                                                    <input type="date" class="form-control" name="applicationDate" value="@(doc.StartDate?.ToString("yyyy-MM-dd"))">
                                                                </div>
                                                                <div class="col-md-6 mb-3">
                                                                    <label class="form-label fw-bold text-success"><i class="bi bi-calendar-check"></i> Alındı/Bitiş Tarihi</label>
                                                                    <input type="date" class="form-control" name="acquiredDate" value="@(doc.CompletedDate?.ToString("yyyy-MM-dd"))">
                                                                </div>
                                                                <div class="col-12 mt-2">
                                                                    <label class="form-label fw-bold text-primary"><i class="bi bi-file-earmark-pdf"></i> Evrak/Makbuz Yükle (PDF/Görsel)</label>
                                                                    <input type="file" class="form-control" name="documentFile" accept=".pdf,image/*">
                                                                    @if(!string.IsNullOrEmpty(doc.FilePath))
                                                                    {
                                                                        <div class="mt-2 small">
                                                                            <a href="@doc.FilePath" target="_blank" class="text-primary fw-bold"><i class="bi bi-paperclip"></i> Mevcut Dosyayı Görüntüle</a>
                                                                        </div>
                                                                    }
                                                                </div>
                                                            </div>
                                                        </div>
                                                        <div class="modal-footer bg-light">
                                                            <button type="button" class="btn btn-secondary rounded-pill" data-bs-dismiss="modal">İptal</button>
                                                            <button type="submit" class="btn btn-primary rounded-pill fw-bold">Değişiklikleri Kaydet</button>
                                                        </div>
                                                    </form>
                                                </div>
                                            </div>
                                        </div>
                                    }
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            }
        }
    </div>
    '''
    content = table_pattern.sub(new_html, content)
    
    with open(filepath, 'w', encoding='utf-8-sig') as f:
        f.write(content)
except Exception as e:
    print(e)
