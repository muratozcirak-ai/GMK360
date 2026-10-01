import io
import re

filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Inject the "Şablondan Güncellemeleri Çek" button in the page header.
# Search for: <h4 class="fw-bold mb-0 text-dark"><i class="bi bi-folder-check text-primary me-2"></i> Resmi Evrak ve Ruhsatlar</h4>
header_pattern = r'(<h4 class="fw-bold mb-0 text-dark"><i class="bi bi-folder-check text-primary me-2"></i> Resmi Evrak ve Ruhsatlar</h4>)'
button_injection = """\\1
                        <button class="btn btn-sm btn-outline-primary fw-bold ms-3 rounded-pill shadow-sm" onclick="checkTemplateUpdates(@Model.Id)">
                            <i class="bi bi-arrow-repeat"></i> Şablondan Güncelle
                        </button>"""
content = re.sub(header_pattern, button_injection, content)

# 2. Inject the Sync Modal and JavaScript
modal_injection = """
    <!-- SYNC / DIFF MODAL -->
    <div class="modal fade" id="syncModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content border-0 shadow-lg rounded-4">
                <div class="modal-header bg-light border-bottom-0 rounded-top-4">
                    <h5 class="modal-title fw-bold text-dark"><i class="bi bi-arrow-repeat text-primary me-2"></i> Merkez Şablon Güncellemeleri</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Kapat"></button>
                </div>
                <div class="modal-body p-4">
                    <div id="syncLoading" class="text-center py-5">
                        <div class="spinner-border text-primary mb-3" role="status"></div>
                        <h6 class="text-muted fw-bold">Şablon karşılaştırılıyor, lütfen bekleyin...</h6>
                    </div>
                    
                    <div id="syncContent" style="display:none;">
                        <div class="alert alert-info small d-flex align-items-center rounded-3 mb-4">
                            <i class="bi bi-info-circle-fill fs-4 me-3 text-info"></i>
                            <div>
                                Merkez şablon (Global Havuz) ile bu proje arasındaki farklar aşağıdadır. İçi dolu veya tamamlanmış evraklarınız (veri kaybı olmaması için) silinmez, yalnızca pasife veya "Muaf" durumuna alınır.
                            </div>
                        </div>

                        <!-- Yeni Eklenenler -->
                        <div class="mb-4">
                            <h6 class="fw-bold text-success border-bottom pb-2"><i class="bi bi-plus-circle-fill me-2"></i> Yeni Eklenen Evraklar</h6>
                            <ul class="list-group list-group-flush" id="syncNewList"></ul>
                        </div>

                        <!-- İsim/Faz Değişiklikleri -->
                        <div class="mb-4">
                            <h6 class="fw-bold text-warning border-bottom pb-2"><i class="bi bi-pencil-fill me-2"></i> İsim / Faz Değişiklikleri</h6>
                            <ul class="list-group list-group-flush" id="syncChangedList"></ul>
                        </div>

                        <!-- Kalkan/İptal Olanlar -->
                        <div class="mb-4">
                            <h6 class="fw-bold text-danger border-bottom pb-2"><i class="bi bi-trash-fill me-2"></i> İptal Olan (Muaf) Evraklar</h6>
                            <ul class="list-group list-group-flush" id="syncRemovedList"></ul>
                        </div>
                    </div>
                </div>
                <div class="modal-footer border-top-0 bg-light rounded-bottom-4">
                    <button type="button" class="btn btn-outline-secondary fw-bold rounded-pill px-4" data-bs-dismiss="modal">İptal</button>
                    <button type="button" class="btn btn-primary fw-bold rounded-pill px-4 shadow-sm" id="btnApplySync" onclick="applyTemplateUpdates(@Model.Id)" style="display:none;">
                        <i class="bi bi-check2-circle me-1"></i> Değişiklikleri Uygula
                    </button>
                </div>
            </div>
        </div>
    </div>
"""

js_injection = """
        function checkTemplateUpdates(projectId) {
            var modal = new bootstrap.Modal(document.getElementById('syncModal'));
            modal.show();
            
            document.getElementById('syncLoading').style.display = 'block';
            document.getElementById('syncContent').style.display = 'none';
            document.getElementById('btnApplySync').style.display = 'none';

            fetch('/api/TemplateSync/CheckUpdates/' + projectId)
                .then(res => res.json())
                .then(data => {
                    document.getElementById('syncLoading').style.display = 'none';
                    document.getElementById('syncContent').style.display = 'block';
                    
                    if(data.success && data.diff) {
                        // Yeni Evraklar
                        let newList = document.getElementById('syncNewList');
                        newList.innerHTML = '';
                        if(data.diff.newDocs.length === 0) {
                            newList.innerHTML = '<li class="list-group-item text-muted small border-0 px-0">Yeni eklenen evrak yok.</li>';
                        } else {
                            data.diff.newDocs.forEach(d => {
                                newList.innerHTML += `<li class="list-group-item border-0 px-0"><i class="bi bi-file-earmark-plus text-success me-2"></i> <strong>${d.name}</strong> <span class="badge bg-light text-secondary ms-2">${d.stage}</span></li>`;
                            });
                        }

                        // Değişen Evraklar
                        let changedList = document.getElementById('syncChangedList');
                        changedList.innerHTML = '';
                        if(data.diff.changedDocs.length === 0) {
                            changedList.innerHTML = '<li class="list-group-item text-muted small border-0 px-0">Değişiklik yok.</li>';
                        } else {
                            data.diff.changedDocs.forEach(d => {
                                changedList.innerHTML += `<li class="list-group-item border-0 px-0"><i class="bi bi-arrow-right-short text-warning me-2"></i> <s>${d.oldName}</s> <i class="bi bi-arrow-right mx-1"></i> <strong>${d.newName}</strong></li>`;
                            });
                        }

                        // Kalkan Evraklar
                        let removedList = document.getElementById('syncRemovedList');
                        removedList.innerHTML = '';
                        if(data.diff.removedDocs.length === 0) {
                            removedList.innerHTML = '<li class="list-group-item text-muted small border-0 px-0">İptal edilen evrak yok.</li>';
                        } else {
                            data.diff.removedDocs.forEach(d => {
                                let badge = d.hasFile || d.status === 'Alındı' ? '<span class="badge bg-secondary ms-2">Dolu - Arşivlenecek</span>' : '<span class="badge bg-danger ms-2">Boş - Muaf Sayılacak</span>';
                                removedList.innerHTML += `<li class="list-group-item border-0 px-0"><i class="bi bi-x-circle text-danger me-2"></i> ${d.name} ${badge}</li>`;
                            });
                        }

                        // Eğer fark varsa Uygula butonunu göster
                        if(data.diff.newDocs.length > 0 || data.diff.changedDocs.length > 0 || data.diff.removedDocs.length > 0) {
                            document.getElementById('btnApplySync').style.display = 'inline-block';
                        }
                    }
                })
                .catch(err => {
                    document.getElementById('syncLoading').innerHTML = '<h6 class="text-danger fw-bold">Bağlantı hatası oluştu.</h6>';
                });
        }

        function applyTemplateUpdates(projectId) {
            let formData = new FormData();
            formData.append('projectId', projectId);
            
            fetch('/api/TemplateSync/ApplyUpdates', {
                method: 'POST',
                body: formData
            })
            .then(res => res.json())
            .then(data => {
                if(data.success) {
                    alert("Şablon eşitlemesi başarıyla tamamlandı!");
                    location.reload();
                } else {
                    alert("Bir hata oluştu.");
                }
            });
        }
"""

content = content.replace("<!-- SYNC / DIFF MODAL YERLEŞİMİ (BURAYA ENJEKTE EDİLECEK) -->", "") # Just in case
content = content.replace("</body>", modal_injection + "\n</body>")
content = content.replace("function openManageModal(docId) {", js_injection + "\n        function openManageModal(docId) {")

# To filter out IsActive = false from the main view list (so removed/exempt docs don't clutter visually, or they show up as "Muaf"):
# We already render docs. Let's make sure Muaf docs have a distinct look.
# Currently they have Status = "Muaf/İstenmiyor" which we can style.
status_style_injection = """
                                                                @{
                                                                    var statusBadgeClass2 = childDoc.Status switch {
                                                                        "Tamamlandı" or "Alındı" => "bg-success text-white",
                                                                        "Sorunlu" => "bg-danger text-white",
                                                                        "Muaf/İstenmiyor" => "bg-secondary bg-opacity-25 text-secondary border border-secondary",
                                                                        "İşlemde" => "bg-info text-dark",
                                                                        _ => "bg-warning bg-opacity-10 text-warning border border-warning"
                                                                    };
                                                                }
"""
content = re.sub(r'var statusBadgeClass2 = childDoc\.Status switch \{.*?\};', status_style_injection.strip(), content, flags=re.DOTALL)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Injected Diff/Sync Modal")
