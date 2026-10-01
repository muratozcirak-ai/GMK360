import io

filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Make form multipart
content = content.replace('<form id="manageDocForm" method="post" action="/PhaseZero/UpdateDoc">',
                          '<form id="manageDocForm" method="post" action="/PhaseZero/UpdateDoc" enctype="multipart/form-data">')

# Add File Upload field to Modal (before the footer)
old_modal_contact = """<div class="col-md-12">
                                <label class="form-label fw-semibold text-danger"><i class="bi bi-incognito"></i> Kurum İçi İlgili (Tanıdık / Gizli Kontak)</label>
                                <input type="text" class="form-control" name="institutionContact" id="modalInstitutionContact" placeholder="Örn: Ahmet Bey - Vezne, Veya İhaleyi Veren Şirket Md.">
                                <small class="text-danger">Bu alan vitrinde görünmez! Sadece yöneticiler ve atanan kişi görebilir.</small>
                            </div>"""
new_modal_contact = """<div class="col-md-12">
                                <label class="form-label fw-semibold text-danger"><i class="bi bi-incognito"></i> Kurum İçi İlgili (Tanıdık / Gizli Kontak)</label>
                                <input type="text" class="form-control" name="institutionContact" id="modalInstitutionContact" placeholder="Örn: Ahmet Bey - Vezne, Veya İhaleyi Veren Şirket Md.">
                                <small class="text-danger">Bu alan vitrinde görünmez! Sadece yöneticiler ve atanan kişi görebilir.</small>
                            </div>
                            
                            <div class="col-md-12 mt-4 border-top pt-3">
                                <label class="form-label fw-bold text-dark"><i class="bi bi-cloud-arrow-up text-primary"></i> Fiziksel Evrak / Dekont Yükle</label>
                                <input type="file" class="form-control" name="uploadedFile" id="modalUploadedFile" accept=".pdf,.jpg,.jpeg,.png">
                                <small class="text-muted">Evrakın taranmış halini veya faturasını buradan yükleyebilirsiniz.</small>
                                <div id="modalExistingFileArea" class="mt-2 d-none">
                                    <span class="badge bg-success"><i class="bi bi-check-circle"></i> Yüklü Evrak Var:</span> 
                                    <a href="#" id="modalExistingFileLink" target="_blank" class="fw-bold text-primary ms-1">Görüntüle / İndir</a>
                                </div>
                            </div>"""
content = content.replace(old_modal_contact, new_modal_contact)

# Update Modal JS
old_js = """document.getElementById('modalAddCost').value = data.additionalCost == 0 ? '' : data.additionalCost;
                    
                    var modal = new bootstrap.Modal(document.getElementById('manageDocModal'));"""
new_js = """document.getElementById('modalAddCost').value = data.additionalCost == 0 ? '' : data.additionalCost;
                    
                    if (data.filePath) {
                        document.getElementById('modalExistingFileArea').classList.remove('d-none');
                        document.getElementById('modalExistingFileLink').href = data.filePath;
                    } else {
                        document.getElementById('modalExistingFileArea').classList.add('d-none');
                        document.getElementById('modalExistingFileLink').href = '#';
                    }
                    
                    var modal = new bootstrap.Modal(document.getElementById('manageDocModal'));"""
content = content.replace(old_js, new_js)

# Main Doc Status Red logic
old_td_status = """<td class="text-center">
                                                @if (doc.Status == "Bekliyor")"""
new_td_status = """<td class="text-center">
                                                @if (isDependent && !isReadyToApply) { <span class="badge bg-danger rounded-pill px-3 shadow-sm"><i class="bi bi-lock-fill"></i> Kilitli (Önkoşul)</span> }
                                                else if (doc.Status == "Bekliyor")"""
content = content.replace(old_td_status, new_td_status)

# Main Doc Title Red Logic + Paperclip
old_td_title = """<td class="fw-bold ps-4">
                                                @if(isDependent) { <i class="bi bi-chevron-expand text-primary me-2 fs-5 align-middle"></i> } else { <span style="margin-left: 28px;"></span> }
                                                @doc.DocumentName"""
new_td_title = """<td class="fw-bold ps-4 @(isDependent && !isReadyToApply ? "text-danger" : "text-dark")">
                                                @if(isDependent) { <i class="bi bi-chevron-expand text-primary me-2 fs-5 align-middle"></i> } else { <span style="margin-left: 28px;"></span> }
                                                @doc.DocumentName
                                                @if(!string.IsNullOrEmpty(doc.FilePath)) { <a href="@doc.FilePath" target="_blank" class="ms-1" title="Evrak Yüklendi"><i class="bi bi-paperclip text-primary fs-5"></i></a> }"""
content = content.replace(old_td_title, new_td_title)

# Child Doc Paperclip
old_child_title = """<span class="text-secondary fw-semibold">@childDoc.DocumentName</span>"""
new_child_title = """<span class="text-secondary fw-semibold">@childDoc.DocumentName</span>
                                                            @if(!string.IsNullOrEmpty(childDoc.FilePath)) { <a href="@childDoc.FilePath" target="_blank" class="ms-1" title="Evrak Yüklendi"><i class="bi bi-paperclip text-primary fs-5"></i></a> }"""
content = content.replace(old_child_title, new_child_title)


with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated Index.cshtml successfully.")
