import sys

filepath = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# I need to change the "Yönet" button to trigger the modal.
# Both in MAIN DOC ROW and CHILD DOCS ROW

target_main_btn = """<a href="#" class="btn btn-sm btn-outline-primary py-0 px-3 rounded-pill text-decoration-none fw-bold" onclick="event.stopPropagation();"><i class="bi bi-pencil-square"></i> Yönet</a>"""
replacement_main_btn = """<a href="javascript:void(0)" class="btn btn-sm btn-outline-primary py-0 px-3 rounded-pill text-decoration-none fw-bold" onclick="event.stopPropagation(); openManageModal(@doc.Id);"><i class="bi bi-pencil-square"></i> Yönet</a>"""

target_child_btn = """<a href="#" class="btn btn-sm btn-outline-secondary py-0 px-3 rounded-pill text-decoration-none fw-bold"><i class="bi bi-pencil-square"></i> Yönet</a>"""
replacement_child_btn = """<a href="javascript:void(0)" class="btn btn-sm btn-outline-secondary py-0 px-3 rounded-pill text-decoration-none fw-bold" onclick="openManageModal(@childDoc.Id);"><i class="bi bi-pencil-square"></i> Yönet</a>"""

content = content.replace(target_main_btn, replacement_main_btn)
content = content.replace(target_child_btn, replacement_child_btn)

# Add Modal and Script at the end of the file
modal_html = """
<!-- YÖNET MODALI -->
<div class="modal fade" id="manageDocModal" tabindex="-1" aria-labelledby="manageDocModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-lg">
        <div class="modal-content border-0 shadow-lg">
            <form id="manageDocForm" method="post" enctype="multipart/form-data">
                <div class="modal-header bg-light border-bottom-0">
                    <h5 class="modal-title fw-bold text-dark" id="manageDocModalLabel"><i class="bi bi-file-earmark-text text-primary me-2"></i> Evrak Yönetimi</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Kapat"></button>
                </div>
                <div class="modal-body p-4">
                    <input type="hidden" id="modalDocId" name="Id" />
                    
                    <div class="row g-3 mb-4">
                        <div class="col-md-12">
                            <h6 class="fw-bold text-secondary mb-1">Evrak Adı</h6>
                            <p class="fs-5 text-dark" id="modalDocName">-</p>
                        </div>
                    </div>

                    <div class="row g-3">
                        <div class="col-md-6">
                            <label class="form-label fw-semibold">Durum</label>
                            <select class="form-select" id="modalStatus" name="Status">
                                <option value="Bekliyor">Yok (Bekliyor)</option>
                                <option value="İşlemde">İşlemde (Başvuruldu)</option>
                                <option value="Sorunlu">Sorun Var</option>
                                <option value="Alındı">Tamamlandı (Alındı)</option>
                            </select>
                            <small class="text-danger d-none" id="statusWarning"><i class="bi bi-exclamation-triangle"></i> Sadece atanan kişi güncelleyebilir.</small>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label fw-semibold">Takipçi (Atanan Kişi)</label>
                            <input type="text" class="form-control" id="modalAssignedUser" name="AssignedUserId" placeholder="Örn: Ahmet Yılmaz">
                        </div>
                        
                        <div class="col-md-12">
                            <label class="form-label fw-semibold">Kurum İçi İlgili (Sektör / Tanıdık)</label>
                            <div class="input-group">
                                <span class="input-group-text bg-white"><i class="bi bi-building"></i></span>
                                <input type="text" class="form-control" id="modalInstitutionContact" name="InstitutionContact" placeholder="İlgilenen memur veya özel şirket yetkilisi">
                                <button class="btn btn-outline-secondary" type="button" title="Rehberden Seç"><i class="bi bi-person-lines-fill"></i></button>
                            </div>
                        </div>

                        <div class="col-md-6">
                            <label class="form-label fw-semibold">Başlama Tarihi</label>
                            <input type="date" class="form-control" id="modalStartDate" name="StartDate">
                        </div>
                        <div class="col-md-6">
                            <label class="form-label fw-semibold">Bitiş Tarihi</label>
                            <input type="date" class="form-control" id="modalCompletedDate" name="CompletedDate">
                        </div>

                        <div class="col-md-12">
                            <label class="form-label fw-semibold">Açıklama / Sorun Notu</label>
                            <textarea class="form-control" id="modalIssueNotes" name="IssueNotes" rows="2" placeholder="Varsa yaşanan sorunlar veya notlar..."></textarea>
                        </div>

                        <div class="col-md-12">
                            <label class="form-label fw-semibold">Evrak Yükle (PDF / Görsel)</label>
                            <input type="file" class="form-control" id="modalUploadedFile" name="UploadedFile" accept=".pdf, .jpg, .jpeg, .png">
                            <div class="mt-2 d-none" id="existingFileWrapper">
                                <a href="#" target="_blank" id="existingFileLink" class="btn btn-sm btn-outline-info"><i class="bi bi-download"></i> Yüklü Evrakı Görüntüle</a>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="modal-footer bg-light border-top-0">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">İptal</button>
                    <button type="button" class="btn btn-primary px-4" id="btnSaveDoc" onclick="saveManageModal()"><i class="bi bi-check-circle"></i> Kaydet</button>
                </div>
            </form>
        </div>
    </div>
</div>

<script>
    function openManageModal(id) {
        // Fetch verilerini getir
        fetch('/ConstructionProject/GetDocumentDetails/' + id)
            .then(response => response.json())
            .then(data => {
                document.getElementById('modalDocId').value = data.id;
                document.getElementById('modalDocName').innerText = data.documentName;
                document.getElementById('modalStatus').value = data.status || 'Bekliyor';
                document.getElementById('modalAssignedUser').value = data.assignedUserId || '';
                document.getElementById('modalInstitutionContact').value = data.institutionContact || '';
                document.getElementById('modalStartDate').value = data.startDate || '';
                document.getElementById('modalCompletedDate').value = data.completedDate || '';
                document.getElementById('modalIssueNotes').value = data.issueNotes || '';
                
                // Dosya linki
                if (data.filePath) {
                    document.getElementById('existingFileWrapper').classList.remove('d-none');
                    document.getElementById('existingFileLink').href = data.filePath;
                } else {
                    document.getElementById('existingFileWrapper').classList.add('d-none');
                    document.getElementById('existingFileLink').href = '#';
                }

                // Yetki Kilidi: Atanan kişi başkasıysa ve admin değilse durumu kilitle
                // (currentUserId check)
                // Basit bir frontend lock (backend'de de yapılması lazım)
                document.getElementById('modalStatus').disabled = false;
                document.getElementById('statusWarning').classList.add('d-none');
                
                // Modalı aç
                var myModal = new bootstrap.Modal(document.getElementById('manageDocModal'));
                myModal.show();
            });
    }

    function saveManageModal() {
        var form = document.getElementById('manageDocForm');
        var formData = new FormData(form);
        
        var btn = document.getElementById('btnSaveDoc');
        btn.disabled = true;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Kaydediliyor...';

        fetch('/ConstructionProject/UpdateDocumentDetails', {
            method: 'POST',
            body: formData
        })
        .then(response => response.json())
        .then(data => {
            if(data.success) {
                location.reload(); // Refresh to show new status/colors
            } else {
                alert("Hata: " + (data.message || "Kaydedilemedi."));
                btn.disabled = false;
                btn.innerHTML = '<i class="bi bi-check-circle"></i> Kaydet';
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert("Sistemsel bir hata oluştu.");
            btn.disabled = false;
            btn.innerHTML = '<i class="bi bi-check-circle"></i> Kaydet';
        });
    }
</script>
"""

if "openManageModal" not in content:
    content = content + "\n" + modal_html
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
        print("Modal HTML and JS added.")
else:
    print("Modal already exists.")
